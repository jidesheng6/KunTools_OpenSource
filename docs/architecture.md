# Architecture

## Goals

1. **WPF-UI (lepoco) shell** with PerMonitorV2 DPI awareness and FluentWindow chrome.
2. **Decoupled layers** so UI frameworks can be swapped without rewriting business logic.
3. **GitHub-ready layout** (`src/` / `tests/` / `docs/` / `assets/` / `libs/`) with clear licensing for commercial Aspose dependencies.

## Layering

| Project | Role |
|---------|------|
| `src/DocumentTools.App` | WPF-UI shell, feature views, dialogs, DI composition |
| `src/DocumentTools.Core` | Feature catalog, convert/barcode/text services, Aspose adapters |
| `tests/DocumentTools.Tests` | Unit tests for Core |
| `assets/` | Icons, optional official `.lic` files (never commit secrets) |
| `docs/` | Design notes（权威口径） |
| `libs/Aspose.Hook/` | Private Hook binary (gitignored) + README |
| `DocumentTools/` *(legacy)* | Original net48 WinForms monolith from `dev` — reference while features migrate |
| `legacy/` | Historical packages / migration backups |

---

## 设计模式硬约束（必须遵守，禁止瞎写）

> 本项目不是「堆控件脚本」。任何新功能、加深业务、修交互，都必须落入下列模式。  
> 权威交互见 [feature-ux-spec.md](feature-ux-spec.md)；完成度见 [feature-gap.md](feature-gap.md)。

### 允许的模式与落点

| 模式 | 落点 | 职责 |
|------|------|------|
| **Feature Module（插件）** | `IFeatureModule` / `FeatureModuleBase` | 自描述 `FeatureDescriptor`，`CreateView` 组装视图；壳只枚举模块 |
| **Factory** | `IFeatureViewFactory` | 按 FeatureId 创建视图；**禁止**在 `MainWindow` 里 `switch` 出具体页 |
| **Catalog** | `IFeatureCatalog` / `BuiltInFeatures` | 菜单、搜索、隐藏功能的唯一数据源 |
| **Strategy** | `IDocumentOperation` | **凡「选文件 → 开始处理 → 产出文件」** 一律走 Operation；按 FeatureId 选策略 |
| **Service / Facade（领域服务）** | `I*Service` in Core | Aspose / 系统 API 只活在这里；可单测；**无对话框、无 WPF 类型** |
| **DI Composition Root** | `ServiceRegistration` / `AddDocumentToolsCore` | 注册模块与 Operation；业务代码不 `new` 具体服务实现 |
| **Options 字典契约** | `DocumentOperationRequest.Options` | 视图只收集 **字符串键值**（键名以 feature-ux-spec §6 为准）；Operation 映射到强类型 Options DTO，再调 Service |
| **UI Facade** | `IDialogService` / `INavigationService` | 隔离 OpenFile/Folder/MessageBox；视图不直接依赖 Win32 对话框细节也可接受经 Facade |

### 分层依赖方向（单向）

```
MainWindow / FeatureView  →  IDialogService / IDocumentOperation / I*Service(接口)
                                      ↓
                            Core Operation（策略编排）
                                      ↓
                            Core Service（Aspose / IO）
```

- `DocumentTools.App` **禁止** `using Aspose.*`（许可引导除外：`AsposeBootstrap`）。
- `DocumentTools.Core` **禁止** 引用 WPF / WinForms / `System.Windows.*`。
- 视图 **禁止** 直接调用 Aspose；工具页也只能依赖 `IBarcodeService` / `IDataMatrixService` 等接口。

### 控件 ≠ 业务（关键纠偏）

| 错误（曾出现） | 正确 |
|----------------|------|
| 在 `GenericConvertView` 里用巨大 `switch(featureId)` 创建 CheckBox/Combo，同时定义业务有哪些参数 | 参数契约在 Core：`FeatureOptionSchemas`（纯数据：Key/Label/Kind/Default） |
| View 里硬编码 OpenFile 过滤器字符串与 Operation 参数键 | 过滤器也放 `FeatureOptionSchemas.GetFileFilter`；键名以 feature-ux-spec 为准 |
| Operation / Service 里出现 `CheckBox`、`MessageBox`、WPF 类型 | 永远禁止；对话框只经 `IDialogService` |
| Combo 把「显示文案」当 Options 值且与 Core 解析缠死 | `OptionChoice.Value` 进 Options；`Label` 仅显示 |

渲染层：`OptionPanelBuilder`（App）只认 `OptionFieldKind`，**不知道**具体功能业务。  
编排层：`IDocumentOperation` 只认 Options 字典 / 强类型 DTO，**不知道**控件。

文件类参数（封面图、水印图、Chrome 路径）用 `OptionFieldKind.FilePath` + `OptionField.FileFilter`：
Core 只说「这是个文件路径，对话框用这个过滤器」，App 负责渲染 `浏览…` 按钮并调 `IDialogService.OpenFiles`。
可选附件的语义统一走 `PathGuard.OptionalExistingFile`（空/不存在 → `null` 回退默认行为），
必填输入才用 `RequireExistingFile`。

**两类页面的参数契约边界**（2026-11-05 定，避免把「表单 + 列表」页判成不合规）：

| 页面类型 | 参数面板写法 | 约束 |
|----------|--------------|------|
| 转换页 / 工具页（单次输入 → 单次产出） | 必须 `OptionPanelBuilder` + `FeatureOptionSchemas`，视图零参数语义 | 新参数只能加 Schema 键；面板不得硬编码 Label/默认值 |
| 考评页（多列表、多步、跨表读写） | 允许视图直接建控件（`ExamViewKit` / DataGrid / 列表） | 每个参数必须在 Core 有对应强类型 DTO 属性；UI 只做输入、校验与状态文案，**不得复制业务语义**（如「弃权该记几分」）；DTO 缺字段就先加 DTO，不许在视图里拼字典键 |

判据一句话：**UI 可以决定「怎么摆」，不能决定「是什么」。**


### 功能类型 → 正确写法

| 功能类型 | 正确结构 | 错误写法（禁止） |
|----------|----------|------------------|
| 转换 / 合并 / 拆分 / 水印 / 压缩 / HTML / 图片管线 / 文档替换 | `BuiltInFeatures` + `IDocumentOperation` + Service + `GenericConvertView` 或专用 View 只填 Options | View 里写 Aspose；或 Operation 里弹对话框 |
| 无文件列表工具（二维码、身份证、姓名竖排） | Module + View + `I*Service`；分区对齐原 GroupBox | 套 `BorderedFileList`「选文件壳」 |
| 系统设置 | Module + View + `IAppSettings` | 改 `App.config` 硬编码；迁 FormWidth/锚点 |
| 考评 / Twain / 系统工具深度 API | 先补 Core Service，再接 View；路径来自 `IAppSettings` | View 里复制 legacy `Util` 大段粘贴且不拆接口 |

**考评页的例外与下限**：考评 7 页（`exam.*`）是「表单 + 列表」工作流，不是「选文件 → 点处理」的转换流程，因此允许 View 直接建控件、直接把表单值装进 Core 的强类型 DTO（如 `ExamInsertRequest`、`ExamReportParaRequest`），不要求走 `OptionPanelBuilder`。但下限有两条，缺一即视为瞎写：

1. 每个参数必须在 Core 有**强类型 DTO 属性**并真被读写；View 只做取值与显示，**不得复制业务语义**（例如不得在 View 里自己算「无效/弃权两行怎么加」「表号怎么换位」，这些必须在 `ExamInsertService` / `ExamIndexService`）。
2. 面板上的取值必须与落库字段一一对上。历史事故：`ExamInsertView` 曾把面板的「选项分值」丢掉、`DoInsert` 写死 `"0"`，于是面板上的控件对结果毫无影响——**存在但无效的控件比没有这个控件更坏**。

### 文件与模块组织

1. **新功能**放在 `Features/Modules/<Area>/` 独立文件（或该 Area 的小集合）；**不要**继续往 `SimpleToolModules.cs` 无限制堆类（该文件仅为迁移过渡）。
2. **新业务逻辑**放在 `Core/Services/...` 与 `Core/Operations/...`；纯解析/计划（如页码范围）放 `Core/Common` 或 Service 旁的静态 Planner，便于单测。
3. **对照 legacy**：先读 `BodyForm` Tab + `Util.*` / `UI.ConvertStart`，把参数键写入 `feature-ux-spec`，再写代码；禁止凭「常见软件默认」发明交互。
4. **加深已有功能**：优先改 Operation 映射 + Service 实现；只有键缺失时才动 View 控件。

### PR / 自检清单（未满足即视为瞎写）

- [ ] 能指出用了哪种模式（Module / Factory / Strategy / Service / Options）
- [ ] App 层无新增 `Aspose.` 引用
- [ ] 批量文件功能走 `IDocumentOperation`，且 Options 键与规范一致
- [ ] 布局符合 feature-ux-spec（列表上、操作下、有分区则分区）
- [ ] 完成度已更新 `feature-gap.md`
- [ ] 每个 `FeatureOptionSchemas` 键都有 Schema 文件之外的消费点（`FeatureOptionSchemaReachabilityTests` 会逐个键检查，孤儿键即失败）
- [ ] Core 可单测的纯逻辑有测试或至少 Planner 可测
- [ ] **视图里没有业务知识（路径布局 / 分隔符 / 命名规则 / SQL / 编码）**：路径拼接与布局、分隔符/正则、文件名与编码规则、SQL 与表名列名、文档格式判断都不许出现在 `Features\Modules\**` 或 `Ui\**` 里 —— 它们是 Operation / Service / `Core\Common` 的职责，视图只收集键值并按返回值渲染。判例与逐处清单位于 `docs\decoupling-audit.md`（Exam 模块逐条判例 + 未修项理由），新增代码按该清单的同款口径自查。
- [ ] `dotnet run --project tools\FeatureSmoke` 通过（真 DI + 真工厂逐页创建并 Measure/Arrange，当前 47/47）

---

## 冒烟验证（视图改动的强制检查）

`tools/FeatureSmoke` 是独立的 net8.0-windows 控制台工程（不进 sln，按需 `dotnet run`）：
它加载真实的 `App.xaml` 资源字典、用 `AppHost.CreateServiceProvider()` 建真容器、
对 `IFeatureCatalog` 里每个 FeatureId 调 `IFeatureViewFactory.Create`，再对该视图走一次
`Measure`/`Arrange`/`UpdateLayout`，最后实例化 `MainWindow`。任一页面构造函数或模板期异常
都会让退出码非 0。

用途：把「导航点开就炸」（缺 DI 注册、别名冲突、资源键写错、控件构造期读空集合）挡在人工点测之前。
注意它只验证**能建能排**，不验证按钮业务 —— 业务正确性仍靠 Core 单测与实机点测。

UX/可用性机器的常态化检查：`tools/UiSnapshot --self-test`（规则自证）+ 全量快照的七条 UX 规则
（只读路径框 / 输入宽度 / 标签对齐 / 空态提示 / Tab 顺序 / 壳层按钮高度 / 文字对比度），
命中清单与判定口径见 [`docs/qa-report-ux.md`](qa-report-ux.md)。

### 测试集为什么串行（`xunit.runner.json`）

`tests/DocumentTools.Tests/xunit.runner.json` 里 `"parallelizeTestCollections": false`。这套测试大量依赖
**进程级**共享状态：ACE/Jet OLE DB 提供者、Aspose 的字体缓存与许可状态、`ConversionLog` 的日志目录环境变量。
并行跑不同测试类时会互相踩——实测过同一命令两次运行、红的是不同用例（`ExamReportExtrasAccessTests` /
`ExamIndexServiceAccessTests` 的 ACE 用例），而每个用例单独跑必过；归因日志用例则是
`Assert.Single() 收到 3 条`（并行时别的类把日志写进了当时生效的目录）。

串行化后 `dotnet test` 约 60 s（并行时约 24 s）。**这个交换是有意的**：发布闸门的第一职责是
「同样的输入给同样的结论」，只要还允许并行，这些红就无法与真缺陷区分。改并行设置之前请先解决
「测试要不要碰进程级状态」，而不是把 `parallelizeTestCollections` 改回 `true` 再靠重跑碰运气。
（改环境变量的用例另外归进 `ConversionLogCollection`，`DisableParallelization = true`，即使将来恢复并行也串行。）

---

## Patterns（English summary）

| Pattern | Where |
|---------|--------|
| **Feature Module** | `IFeatureModule` |
| **Factory** | `IFeatureViewFactory` |
| **Strategy** | `IDocumentOperation` |
| **DI Composition Root** | `ServiceRegistration` |
| **Facade** | `IDialogService` / `INavigationService` |
| **Catalog** | `IFeatureCatalog` |

## Startup order

`Program.Main` must keep this order:

1. 单实例互斥体 `Local\DocumentTools.SingleInstance`（已是第二个实例 → 前置旧窗口后返回 0）
2. `AsposeBootstrap.Start()` → EnableLog + `ShowHookDetails` + `StartHook` + console/file logs (+ optional `.lic`)
3. WPF `App` + DI → `MainWindow` (FluentWindow)

`Aspose.Hook.dll` (net8.0) lives under `libs/Aspose.Hook/` (gitignored binary) or via `-p:AsposeHookPath=...`.

## 启动与异常兜底

`Program.Main` 是唯一进程入口，负责单实例与全局兜底；`App.OnStartup` 只负责建容器和主窗口。
测试与快照工具（`tools\FeatureSmoke`、`tools\UiSnapshot`、QaFunction）会加载 App 程序集但**不经过 `Program.Main`**，
所以互斥体与全局处理器都写在 `Main` 里，工具不会被误拦。

| 路径 | 触发 | 行为 |
|------|------|------|
| 正常启动 | 首个实例 | `AsposeBootstrap.Start()` → 建容器 → `MainWindow.Show()`；「关于」页可一键打开日志目录 |
| 第二个实例 | 已有实例持有 `Local\DocumentTools.SingleInstance` | 不建容器；还原并前置已有窗口（`ShowWindow(SW_RESTORE)` + `SetForegroundWindow`）后退出，返回码 0 |
| 启动失败 | `Program.Main` 或 `App.OnStartup` 抛异常 | 写 `logs\crash-<yyyyMMdd-HHmmss>.log` 并追加 `logs\startup-crash.log`（旧路径不变），弹窗给出日志路径，退出码 -1 |
| 运行期未捕获 | `DispatcherUnhandledException` / `AppDomain.UnhandledException` / `TaskScheduler.UnobservedTaskException` | 各写一个 crash 日志；界面线程异常标记 `Handled` 后经 `IDialogService` 提示日志路径；进程级异常提示后退出；后台任务异常写日志且本次会话最多提示一次 |

日志内容统一包含：时间戳与阶段、程序集名与版本、`RuntimeInformation` 运行时 / 操作系统 / 架构、进程号与程序目录、异常全串。
`CrashLog.Notifier` 由 `App.OnStartup` 注入，容器未就绪时自动退化为 `MessageBox`，保证任何阶段出错都能把日志路径交到用户手里。

限制：程序集缺失或加载期失败（例如删掉 `DocumentTools.Core.dll`）发生在 `Program.Main` 执行之前或 JIT 阶段，
托管处理器还没装好，因此**不会**产生 `logs\` 文件，进程可能静默退出；这类故障只能靠 Windows 事件日志或控制台排查。

## 壳层：折叠分组不许用 `Expander`

导航分组、设置页的「启动修正」提示条都用 `src\DocumentTools.App\Shell\CollapsibleSection.cs`，**不要换回 `System.Windows.Controls.Expander`**。

原因是 WPF 默认模板的实测行为：`Expander` 的内容行占用剩余空间（`*`），而 `ScrollViewer → StackPanel` 按无限高测量子元素，于是展开后内容行被压成 0 高。

| 布局 | 实测 |
|------|------|
| `Border` → `Expander`（直接） | `MeasuredHeight = 41.8`，正常 |
| `Border` → `ScrollViewer` → `StackPanel` → `Expander` | `MeasuredHeight = 19.8`、内容高度 `0.0`，只剩分组标题 |

后果特别隐蔽：页面截图全绿，因为坏的是**壳**；侧边栏能看见「WORD」标题却点不到下面的功能，用户会以为导航是空的。
`CollapsibleSection` 把头部与本体放进同一个 `StackPanel`，折叠只切 `Body.Visibility`，任何宿主里测量结果都一致。
`tools\UiSnapshot` 的壳层检查（`ShellAudit.cs`）对每个分组断言「展开后可见按钮数不减、每个导航按钮 `ActualHeight ≥ 20`」，并对旧 `Expander` 布局自证会报红，成为这条结论的回归锁。

以后要做新的可折叠分组，照这个来：

1. 用 `new CollapsibleSection()`，不要从 `Expander` 继承或包装它；
2. 标题走 `Title`（同时写自动化名称）、右侧计数走 `Trailing`、分类图标走 `Glyph`，不要在头部自己拼 `TextBlock`；
3. 内容**加到 `Body.Children`**，`Body` 就是普通 `StackPanel`，排版规则与页面里一致；
4. 折叠只看 `IsExpanded`，需要联动就订阅 `ExpandedChanged`，**不要自己在外面切 `Visibility`**（两处状态会让收起态和 chevron 脱节）；
5. 宿主仍然可以是 `ScrollViewer → StackPanel`，这正是它比 `Expander` 强的地方；
6. 新增或改动导航分组后，跑一次 `dotnet run --project tools\UiSnapshot`，确认 `SHELL|…problems=0`。

### 怎么验证交互样式（三条硬规矩）

1. **取像素值，不取「看起来对」**。尺寸用 `ActualHeight` / `TransformToAncestor` 的坐标，颜色用 `((SolidColorBrush)c.Background).Color`，把数字贴进汇报；「按钮高一点、图标挪了挪」这种描述无法回归，也没法在评审里判对错。
2. **别信 `IsMouseOver`**。它是只读属性、只由真实输入驱动：离屏自检合成 `MouseEnter` / `PreviewMouseLeftButtonDown` 时它恒为 `false`。任何 `<判断 IsMouseOver 再改样式>` 的写法在这里都测不到，等于把反馈做成不可验证的东西——`CollapsibleSection.SetHeaderFill` 就去掉了这道判断，改为「事件到了就换画刷」。
3. **hover 必须比 pressed 淡**。WPF-UI 的 `ControlFillColorTertiaryBrush` 是 alpha `#4D`、`ControlFillColorSecondaryBrush` 是 `#80`，按名字直觉容易配反，会出现「按下比悬停更淡」。定稿口径：悬停 `#4DF9F9F9`、按下 `#80F9F9F9`、移出清回透明。

### 键盘顺序：逻辑顺序必须等于视觉顺序

Tab 默认按**逻辑树顺序**走。`FeaturePageBase.BuildBottomActionLayout` 早期用 `DockPanel` + `LastChildFill`，逼着内容最后添加，于是逻辑顺序只能是「操作条 → 内容」，键盘用户第一下 Tab 就跳到页面底部的按钮上（`UiSnapshot` 规则 5「Tab 顺序倒退」就是这么报出来的，全仓曾一次报出 53 条）。

现在的做法：**用 `Grid` 按「内容 → 选项 → 操作条」的顺序添加**，视觉顺序与逻辑顺序一致，一行 `TabIndex` 都不需要。

同时记住反例：**`KeyboardNavigation.TabIndex` 设在容器上没有用**——它只在同一父级的孩子之间排序，不会传给容器内部的按钮；想靠「给底部条设 100、给内容设 0」来纠正顺序是无效的（全仓 `TabIndex=` 命中数为 0，不是没写过，是写了不生效被删掉）。需要控制顺序时改添加顺序，不要改 `TabIndex`。



## DPI checklist (WPF-UI)

1. Main window inherits `ui:FluentWindow`.
2. `app.manifest` declares `PerMonitorV2` + `dpiAware true/PM`.
3. Prefer WPF-UI controls for chrome (TitleBar, Button, TextBox).
4. File lists use bordered `ListView`/`GridView` to match legacy WinForms ListView with grid lines.

## Adding a feature

1. Read **[feature-ux-spec.md](feature-ux-spec.md)** for layout archetype, interaction contract, and option keys.
2. Read **本文件「设计模式硬约束」**；确认功能类型对应的正确结构。
3. Add `FeatureDescriptor` to `BuiltInFeatures` (stable id).
4. Implement Core service / `IDocumentOperation` if there is business logic.
5. Add `Features/Modules/<Area>/` module + view in App（操作区在 **底部**，分区保留原 GroupBox 标题）.
6. Register with `AddFeatureModule<T>()` (or batch `CreateAll()`).
7. Update **[feature-gap.md](feature-gap.md)**.

Do **not** edit `MainWindow` for each new page.
