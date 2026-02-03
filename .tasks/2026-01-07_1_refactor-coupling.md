# 背景
文件名：2026-01-07_1_refactor-coupling.md
创建于：2026-01-07_13:32:00
创建者：AI Assistant
主分支：main
任务分支：dev
Yolo模式：Ask

# 任务描述
重构 DocumentTools 项目，解决高耦合性问题，提高代码可维护性。项目目前存在以下主要问题：
1. BodyForm.MainForm 静态依赖导致全局耦合
2. UI.cs 中172处直接访问 BodyForm.MainForm
3. BodyForm.cs 文件过大（9922行），违反单一职责原则
4. Util.cs 文件过大（6105行），包含太多不相关的工具方法
5. 缺少依赖注入机制
6. 缺少服务层抽象

# 项目概览
DocumentTools 是一个基于 WinForms 的文档处理工具，使用 Aspose 库进行文档转换和处理。项目使用 .NET Framework 4.8，C# 8.0。

⚠️ 警告：永远不要修改此部分 ⚠️
核心RIPER-5协议规则：
- 必须在每个响应开头声明模式 [MODE: MODE_NAME]
- RESEARCH模式：只允许阅读、分析、提问，禁止建议和实施
- INNOVATE模式：只允许讨论方案，禁止具体规划和实施
- PLAN模式：只允许详细规划，禁止实施
- EXECUTE模式：只允许实施已批准的计划，禁止偏离
- REVIEW模式：验证实施与计划的一致性
⚠️ 警告：永远不要修改此部分 ⚠️

# 分析

## 发现的耦合性问题

### 1. 静态依赖问题
- **位置**：`BodyForm.cs:34` 定义了 `public static BodyForm MainForm;`
- **问题**：全局单例模式，导致整个项目都依赖这个静态实例
- **影响范围**：UI.cs (172处引用), Util.cs (12处引用)

### 2. UI.cs 紧耦合
- **问题**：所有方法都直接访问 `BodyForm.MainForm` 的控件和属性
- **示例**：
  - `AddWordFilesAsync()` 访问 `BodyForm.MainForm.WordFileListView`
  - `ConvertStart()` 方法中大量访问 `BodyForm.MainForm` 的各种控件
- **影响**：无法进行单元测试，UI 逻辑与业务逻辑混合

### 3. BodyForm.cs 职责过重
- **文件大小**：9922行
- **问题**：一个类承担了太多职责
  - UI 控件管理
  - 业务逻辑处理
  - 事件处理
  - 配置管理
  - 文件操作
- **影响**：难以维护和扩展

### 4. Util.cs 职责混乱
- **文件大小**：6105行
- **问题**：包含各种不相关的工具方法
  - 文档转换方法
  - 文件操作方法
  - 图像处理方法
  - 数据库操作方法
  - 系统操作方法
- **影响**：难以定位和维护特定功能

### 5. 缺少依赖注入
- **问题**：所有依赖都是硬编码的
- **影响**：无法进行依赖替换和测试

### 6. 缺少服务层抽象
- **问题**：虽然有 TextProcessingService，但大部分业务逻辑仍在 UI 和 Util 中
- **影响**：业务逻辑与 UI 耦合，无法复用

## 代码结构分析

### 当前架构
```
DocumentTools/
├── BodyForm.cs (9922行) - 主窗体，包含所有UI和业务逻辑
├── UI.cs (902行) - UI辅助方法，直接访问BodyForm.MainForm
├── Util.cs (6105行) - 工具类，包含各种不相关的方法
├── Services/
│   ├── TextProcessingService.cs - 文本处理服务（已存在）
│   └── TextProcessingOptions.cs - 文本处理选项（已存在）
└── DataBaseFactory.cs - 数据库工厂（已存在，设计较好）
```

### 依赖关系
- UI.cs → BodyForm.MainForm (172处)
- Util.cs → BodyForm.MainForm (12处)
- BodyForm → Util (大量调用)
- BodyForm → 各种 Aspose 库（直接依赖）

# 提议的解决方案

## 方案概述
采用分层架构和依赖注入，逐步重构代码，降低耦合度。

## 重构策略

### 阶段1：引入依赖注入容器
- 使用 Microsoft.Extensions.DependencyInjection（项目已引用）
- 创建服务注册和配置类
- 将 BodyForm.MainForm 改为通过依赖注入获取

### 阶段2：创建服务层接口
- 为文档转换功能创建接口（IDocumentConversionService）
- 为文件操作创建接口（IFileOperationService）
- 为UI操作创建接口（IUIHelperService）
- 将 Util.cs 中的方法按功能分类到不同的服务类

### 阶段3：重构 UI.cs
- 将 UI.cs 中的方法改为接受参数而不是直接访问 BodyForm.MainForm
- 创建 IFormAccessor 接口来封装对窗体的访问
- 通过依赖注入传递依赖

### 阶段4：拆分 BodyForm.cs
- 按功能模块拆分 BodyForm
  - BodyForm.cs - 主窗体（只负责UI）
  - DocumentConversionViewModel.cs - 文档转换视图模型
  - FileOperationViewModel.cs - 文件操作视图模型
  - SettingsViewModel.cs - 设置视图模型
- 使用 MVVM 模式（简化版）

### 阶段5：重构 Util.cs
- 按功能分类创建服务类：
  - DocumentConversionService - 文档转换
  - FileOperationService - 文件操作
  - ImageProcessingService - 图像处理
  - SystemOperationService - 系统操作
  - DatabaseService - 数据库操作（基于 DataBaseFactory）

# 当前执行步骤："2. 制定详细重构计划"

# 详细重构计划

## 阶段1：创建依赖注入基础设施

### 1.1 创建服务注册类
**文件路径**：`DocumentTools/Services/ServiceConfiguration.cs`
**功能**：配置依赖注入容器，注册所有服务
**类名**：`ServiceConfiguration`
**方法签名**：
```csharp
public static class ServiceConfiguration
{
    public static IServiceProvider ConfigureServices()
    public static void RegisterServices(IServiceCollection services)
}
```

### 1.2 创建窗体访问接口
**文件路径**：`DocumentTools/Services/IFormAccessor.cs`
**功能**：封装对BodyForm控件的访问，解耦UI.cs和BodyForm
**接口定义**：
```csharp
public interface IFormAccessor
{
    // ListView访问
    ListView GetWordFileListView();
    ListView GetPdfFileListView();
    ListView GetExcelFileListView();
    // ... 其他ListView访问方法
    
    // 控件值访问
    string GetPDFComplianceValue();
    string GetWord2PDFOptimizValue();
    bool GetWord2PdfNeedUpdateFields();
    // ... 其他控件值访问方法
    
    // 设置方法
    void SetStatusLabelText(string text);
    void SetButtonEnabled(Button button, bool enabled);
}
```

### 1.3 创建窗体访问实现类
**文件路径**：`DocumentTools/Services/FormAccessor.cs`
**功能**：实现IFormAccessor接口，封装对BodyForm的访问
**类名**：`FormAccessor`
**构造函数**：`public FormAccessor(BodyForm form)`

### 1.4 修改BodyForm构造函数
**文件路径**：`DocumentTools/BodyForm.cs`
**修改位置**：第474-484行
**更改内容**：
- 移除 `public static BodyForm MainForm;` 静态字段（第34行）
- 移除构造函数中的 `MainForm = this;`（第479行）
- 添加私有字段 `private readonly IFormAccessor _formAccessor;`
- 修改构造函数接受IServiceProvider参数

### 1.5 修改Program.cs
**文件路径**：`DocumentTools/Program.cs`
**修改内容**：
- 在Main方法中创建ServiceProvider
- 通过依赖注入创建BodyForm实例

## 阶段2：创建服务层接口和实现

### 2.1 创建文档转换服务接口
**文件路径**：`DocumentTools/Services/IDocumentConversionService.cs`
**接口定义**：
```csharp
public interface IDocumentConversionService
{
    Task ConvertWord2PdfAsync(ConversionOptions options);
    Task ConvertPdf2WordAsync(ConversionOptions options);
    Task ConvertExcel2PdfAsync(ConversionOptions options);
    // ... 其他转换方法
}
```

### 2.2 创建转换选项类
**文件路径**：`DocumentTools/Services/ConversionOptions.cs`
**功能**：封装转换参数，替代直接访问BodyForm控件

### 2.3 创建文档转换服务实现
**文件路径**：`DocumentTools/Services/DocumentConversionService.cs`
**功能**：将Util.cs中的转换方法迁移到此服务类
**依赖**：IFormAccessor（用于获取转换参数）

### 2.4 创建文件操作服务接口
**文件路径**：`DocumentTools/Services/IFileOperationService.cs`
**功能**：封装文件选择、文件操作等功能

### 2.5 创建文件操作服务实现
**文件路径**：`DocumentTools/Services/FileOperationService.cs`
**功能**：将Util.cs中的文件操作方法迁移到此服务类

### 2.6 创建UI辅助服务接口
**文件路径**：`DocumentTools/Services/IUIHelperService.cs`
**功能**：封装UI操作，替代UI.cs中的静态方法

### 2.7 创建UI辅助服务实现
**文件路径**：`DocumentTools/Services/UIHelperService.cs`
**功能**：将UI.cs中的方法迁移到此服务类，通过IFormAccessor访问控件

## 阶段3：重构UI.cs

### 3.1 重构UI.cs方法签名
**文件路径**：`DocumentTools/UI.cs`
**策略**：将静态方法改为实例方法，通过构造函数注入IUIHelperService
**更改示例**：
- `public static void AddWordFilesAsync()` → 移除，功能迁移到IUIHelperService
- `public static void ConvertStart(...)` → 移除，功能迁移到IDocumentConversionService

### 3.2 创建UI操作适配器
**文件路径**：`DocumentTools/Services/UIAdapter.cs`
**功能**：提供向后兼容的静态方法，内部调用服务层
**目的**：逐步迁移，不破坏现有代码

## 阶段4：重构Util.cs（按功能拆分）

### 4.1 分析Util.cs方法分类
**策略**：将Util.cs中的方法按功能分类：
- 文档转换方法 → DocumentConversionService
- 文件操作方法 → FileOperationService  
- 图像处理方法 → ImageProcessingService
- 系统操作方法 → SystemOperationService
- 数据库操作方法 → DatabaseService（基于DataBaseFactory）

### 4.2 创建图像处理服务
**文件路径**：`DocumentTools/Services/IImageProcessingService.cs` 和 `ImageProcessingService.cs`
**功能**：封装图像处理相关方法

### 4.3 创建系统操作服务
**文件路径**：`DocumentTools/Services/ISystemOperationService.cs` 和 `SystemOperationService.cs`
**功能**：封装系统操作相关方法（注册表、文件系统等）

### 4.4 创建数据库服务
**文件路径**：`DocumentTools/Services/IDatabaseService.cs` 和 `DatabaseService.cs`
**功能**：基于DataBaseFactory封装数据库操作

## 阶段5：更新依赖关系

### 5.1 更新BodyForm
**修改内容**：
- 通过构造函数注入所需服务
- 移除对Util静态方法的直接调用
- 使用服务层方法替代

### 5.2 更新所有调用点
**策略**：逐步替换所有对UI.cs和Util.cs静态方法的调用

## 错误处理策略
- 所有服务方法使用try-catch包装
- 使用ILogger接口记录错误（如果项目中有日志框架）
- 抛出自定义异常类型

## 测试策略
- 为每个服务接口创建单元测试
- 使用Mock对象测试服务层
- 保持向后兼容，确保现有功能不受影响

# 任务进度
[2026-01-07_13:32:00]
- 已修改：创建任务文件
- 更改：分析项目结构，识别耦合性问题
- 原因：为重构工作建立基础
- 阻碍因素：无
- 状态：未确认

[2026-01-07_13:45:00]
- 已修改：制定详细重构计划
- 更改：创建5个阶段的重构计划，包含具体文件路径、类名、方法签名
- 原因：为执行阶段提供详细规范
- 阻碍因素：无
- 状态：未确认

[2026-01-07_14:10:00]
- 已修改：实现阶段1的基础设施
- 更改：新增DI配置(ServiceConfiguration)、窗体访问接口与实现(IFormAccessor/FormAccessor)；Program入口使用DI创建BodyForm；BodyForm支持DI构造并标记静态MainForm为废弃
- 原因：为移除静态全局依赖、引入依赖注入打基础
- 阻碍因素：仍需逐步迁移UI/Util对MainForm的直接访问
- 状态：未确认

[2026-01-07_14:20:00]
- 已修改：创建服务层骨架（阶段2框架）
- 更改：新增 ConversionOptions、IDocumentConversionService/DocumentConversionService、IFileOperationService/FileOperationService、IUIHelperService/UIHelperService、UIAdapter；在 ServiceConfiguration 中注册服务；更新 csproj 引入新文件
- 原因：搭建服务层接口与占位实现，为迁移 Util/UI 逻辑做准备
- 阻碍因素：服务方法尚未实现，需后续迁移逻辑
- 状态：未确认

# 实施清单

## 阶段1：创建依赖注入基础设施

1. 创建 `DocumentTools/Services/ServiceConfiguration.cs` 文件
   - 定义 `ServiceConfiguration` 静态类
   - 实现 `ConfigureServices()` 方法，返回 `IServiceProvider`
   - 实现 `RegisterServices(IServiceCollection services)` 方法
   - 注册所有服务接口和实现类

2. 创建 `DocumentTools/Services/IFormAccessor.cs` 文件
   - 定义 `IFormAccessor` 接口
   - 添加所有ListView访问方法（GetWordFileListView, GetPdfFileListView等）
   - 添加所有控件值访问方法（GetPDFComplianceValue, GetWord2PDFOptimizValue等）
   - 添加设置方法（SetStatusLabelText, SetButtonEnabled等）

3. 创建 `DocumentTools/Services/FormAccessor.cs` 文件
   - 实现 `IFormAccessor` 接口
   - 添加私有字段 `private readonly BodyForm _form;`
   - 实现构造函数 `public FormAccessor(BodyForm form)`
   - 实现所有接口方法，通过 `_form` 访问控件

4. 修改 `DocumentTools/BodyForm.cs`
   - 删除第34行的 `public static BodyForm MainForm;` 静态字段声明
   - 在类中添加私有字段 `private readonly IFormAccessor _formAccessor;`
   - 修改构造函数，添加 `IServiceProvider serviceProvider` 参数
   - 在构造函数中删除 `MainForm = this;` 语句（第479行）
   - 在构造函数中通过 `serviceProvider.GetService<IFormAccessor>()` 获取FormAccessor实例

5. 修改 `DocumentTools/Program.cs`
   - 在 `Main()` 方法开始处调用 `ServiceConfiguration.ConfigureServices()` 创建ServiceProvider
   - 修改 `Application.Run(new BodyForm())` 为通过ServiceProvider创建BodyForm实例

## 阶段2：创建服务层接口和实现

6. 创建 `DocumentTools/Services/ConversionOptions.cs` 文件
   - 定义 `ConversionOptions` 类，包含所有转换所需的参数属性
   - 为每种转换类型创建对应的选项类（Word2PdfOptions, Pdf2WordOptions等）

7. 创建 `DocumentTools/Services/IDocumentConversionService.cs` 文件
   - 定义 `IDocumentConversionService` 接口
   - 添加所有文档转换方法的异步版本签名

8. 创建 `DocumentTools/Services/DocumentConversionService.cs` 文件
   - 实现 `IDocumentConversionService` 接口
   - 添加私有字段 `private readonly IFormAccessor _formAccessor;`
   - 在构造函数中注入 `IFormAccessor`
   - 将 `Util.cs` 中的转换方法迁移到此服务类
   - 修改方法签名，使用 `ConversionOptions` 参数替代直接访问BodyForm控件

9. 创建 `DocumentTools/Services/IFileOperationService.cs` 文件
   - 定义 `IFileOperationService` 接口
   - 添加文件选择、文件操作等方法签名

10. 创建 `DocumentTools/Services/FileOperationService.cs` 文件
    - 实现 `IFileOperationService` 接口
    - 将 `Util.cs` 中的文件操作方法迁移到此服务类

11. 创建 `DocumentTools/Services/IUIHelperService.cs` 文件
    - 定义 `IUIHelperService` 接口
    - 添加所有UI操作方法签名（AddWordFilesAsync, ConvertStart等）

12. 创建 `DocumentTools/Services/UIHelperService.cs` 文件
    - 实现 `IUIHelperService` 接口
    - 添加私有字段 `private readonly IFormAccessor _formAccessor;` 和 `private readonly IDocumentConversionService _conversionService;`
    - 在构造函数中注入依赖
    - 将 `UI.cs` 中的方法迁移到此服务类
    - 修改方法实现，通过 `_formAccessor` 访问控件，通过 `_conversionService` 执行转换

## 阶段3：重构UI.cs

13. 创建 `DocumentTools/Services/UIAdapter.cs` 文件
    - 定义 `UIAdapter` 静态类
    - 添加私有静态字段 `private static IUIHelperService _uiHelperService;`
    - 添加 `Initialize(IUIHelperService uiHelperService)` 方法
    - 添加向后兼容的静态方法，内部调用 `_uiHelperService`

14. 修改 `DocumentTools/UI.cs`
    - 将所有静态方法标记为 `[Obsolete]`，并添加注释指向新的服务类
    - 或者保留方法但内部调用 `UIAdapter` 的对应方法

15. 更新 `DocumentTools/Program.cs`
    - 在创建ServiceProvider后，调用 `UIAdapter.Initialize()` 初始化适配器

## 阶段4：重构Util.cs（按功能拆分）

16. 分析 `DocumentTools/Util.cs` 文件
    - 识别所有方法，按功能分类
    - 列出需要迁移到各个服务类的方法清单

17. 创建 `DocumentTools/Services/IImageProcessingService.cs` 和 `ImageProcessingService.cs`
    - 定义接口和实现类
    - 将Util.cs中的图像处理方法迁移到此服务类
    - 在ServiceConfiguration中注册服务

18. 创建 `DocumentTools/Services/ISystemOperationService.cs` 和 `SystemOperationService.cs`
    - 定义接口和实现类
    - 将Util.cs中的系统操作方法迁移到此服务类
    - 在ServiceConfiguration中注册服务

19. 创建 `DocumentTools/Services/IDatabaseService.cs` 和 `DatabaseService.cs`
    - 定义接口和实现类
    - 基于DataBaseFactory封装数据库操作
    - 将Util.cs中的数据库操作方法迁移到此服务类
    - 在ServiceConfiguration中注册服务

20. 更新 `DocumentTools/Services/ServiceConfiguration.cs`
    - 注册所有新创建的服务接口和实现类

## 阶段5：更新依赖关系

21. 更新 `DocumentTools/BodyForm.cs`
    - 在构造函数中注入 `IUIHelperService`、`IDocumentConversionService` 等所需服务
    - 查找所有对 `Util.` 静态方法的调用，替换为对应的服务方法调用
    - 查找所有对 `UI.` 静态方法的调用，替换为对应的服务方法调用

22. 更新所有调用点
    - 搜索整个项目中对 `Util.` 的调用
    - 搜索整个项目中对 `UI.` 的调用
    - 逐步替换为服务层调用或通过依赖注入获取服务实例

23. 编译和测试
    - 编译项目，修复所有编译错误
    - 运行应用程序，测试基本功能
    - 验证所有转换功能是否正常工作

24. 清理代码
    - 删除 `Util.cs` 中已迁移的方法（保留未迁移的工具方法）
    - 删除 `UI.cs` 中已迁移的方法（或保留为Obsolete）
    - 更新代码注释和文档

# 最终审查
[待完成]
