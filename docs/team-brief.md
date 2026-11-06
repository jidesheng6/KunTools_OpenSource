# 团队通用简报（DocumentTools WPF 迁移）

本文件是所有 teammate 的共同前置阅读。分派给你的具体任务在任务描述里，本文件只讲不许违反的规则与固定命令。

## 1. 仓库与工作树

- 仓库根：`D:\sharp_project\source\repos\DocumentTools`，分支 `feat/wpfui-migration`。
- **工作树是脏的**：大量迁移改动未提交（`git status` 一半是 `??`）。绝对禁止 `git reset --hard`、`git checkout --`、`git clean`、`git stash`、删除任何你没创建的文件。不要提交。
- 只改你的任务白名单里列出的文件/目录。需要动白名单外的文件，先 `send_message` 给 `lead` 说明原因并等回复。
- 权威文档（按优先级）：`docs/architecture.md`（分层+设计模式硬约束）> `docs/feature-ux-spec.md`（交互/布局/参数）> `docs/feature-gap.md`（完成度现状）。

## 2. 不可违反的硬约束

1. **App 层禁止 `using Aspose.*`**（唯一例外 `src\DocumentTools.App\Infrastructure\Licensing\AsposeBootstrap.cs`）。
2. **Core 层禁止 WPF/WinForms/`System.Windows.*`**；对话框只能经 `IDialogService`。
3. **视图禁止直接调 Aspose / 直接写业务参数**：参数契约在 Core `FeatureOptionSchemas`（`OptionFieldKind`：`Check` / `Text` / `Choice` / `FilePath`），App `OptionPanelBuilder` 只按 Kind 渲染。
4. **App 内不要再写 `using Button = …` / `using TextBox = …`**：`src\DocumentTools.App\GlobalUsings.cs` 已有全局别名，重复声明会 `CS1537`。`Rectangle` 用 `using Rectangle = System.Windows.Shapes.Rectangle;`。
5. **布局规范**：列表/内容在上，选项与操作区在下（`FeaturePageBase.BuildBottomActionLayout`）；没有文件列表的页不要套「选文件壳」。
6. **文本编码（最高优先级）**：含中文的 `.cs` 用 **UTF-8**；任何 `Content = "???"` / `base("???"` / `PlaceholderText = "???"` 形式的中文乱码都视为失败。
   - **禁止**用 PowerShell 的 `Get-Content | Set-Content`、`>`、`Out-File`、`Set-Content` 写 `.cs`（默认 ANSI/GBK 会把整文件中文变成 `锛?` 这类不可逆乱码）。
   - 写文件只用 `write`/`edit` 工具；必须改文件时优先 `edit` 局部替换。
   - 已发生一次事故：`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs` 被 GBK 重写，10 个编译错误、5 处中文字符永久丢失（`。`→`?`）。改完任何文件立即跑下面第 5 条自检。
   - **读中文文件也会骗人**：本宿主里 `Get-Content`（含 `Select-String` 输出）默认按 ANSI/GBK 解码，读 UTF-8 文件会显示成 `杞崲鍩?` 这类乱码——**那是显示问题，文件本身是好的**。禁止据此“修复”。要判断真实编码，用 `read` 工具读，或 `Get-Content -Encoding utf8`；要确认是否真损坏，跑下面第 5 条 GBK 自检（0 命中即健康）。
7. **可选附件语义**：可选文件（封面/水印图/LOGO）走 `PathGuard.OptionalExistingFile`（空或不存在 → `null` 回退），必填输入才用 `PathGuard.RequireExistingFile`。
8. 新模块放 `Features/Modules/<Area>/`，不要继续往 `SimpleToolModules.cs` 堆；纯逻辑放 `Core/Common` 或 Service 旁，便于单测。

## 3. 固定命令（每次交付前全部跑一遍）

```powershell
# 构建（要求 0 警告 0 错误）
dotnet build DocumentTools.sln --no-incremental -v q --nologo

# 单元测试（要求全绿）
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo

# 视图冒烟：真 DI + 真视图工厂逐页创建并 Measure/Arrange（要求 47/47 + 启动落点 95 项 0 失败）
dotnet run --project tools\FeatureSmoke -v q --nologo
```

**验证用的应用实例必须关掉**（踩过两次坑）：`DocumentTools.exe` 会锁住 `src\DocumentTools.App\bin\Debug\net8.0-windows\DocumentTools.Core.dll`，导致其他人和你自己的下一次构建报 `MSB3027 / MSB3021`。

```powershell
# 启动验证后立刻关闭；留在后台就会锁 bin
Start-Process src\DocumentTools.App\bin\Debug\net8.0-windows\DocumentTools.exe
Start-Sleep -Seconds 8
Get-Process -Name DocumentTools | Select-Object Id, MainWindowTitle
Stop-Process -Name DocumentTools -Force
```

```powershell

# 中文乱码自检（要求 0 命中）
Get-ChildItem src,tests,tools -Recurse -Filter *.cs |
  Where-Object FullName -notmatch '\\(bin|obj)\\' |
  Select-String -Pattern 'Content = "\?{3,}|base\("\?{3,}|PlaceholderText = "\?{3,}'

# GBK→UTF8 二次编码自检（要求 0 命中；上面的 ASCII 占位检查抓不到这类损坏）
Get-ChildItem src,tests,tools -Recurse -Filter *.cs |
  Where-Object FullName -notmatch '\\(bin|obj)\\' |
  Select-String -Pattern '锛|銆|鐨|鏂|鎴|鍥剧|鏂囨。|闇€'
```

环境：`dotnet --version` = 10.0.300；目标框架 `net8.0-windows`；ACE (Microsoft Access Database Engine 12.0) 本机可用（`Microsoft.ACE.OLEDB.12.0`），但仓库内**没有任何 `.mdb` 夹具**，也没有 Vfsoft 注册表数据路径。

## 4. 能力选择

写代码或跑命令前，先看宿主提供的技能目录（`<available_skills>`）与本会话的 MCP/CLI 工具；与本任务匹配的**必须**先加载或调用：

- 造/读 Office 文档夹具 → `office`、`office-docx`、`office-xlsx`、`office-pptx`。
- 导出 PDF 或渲染文档预览 → `office-export-pdf`、`office-render`。
- 本仓库的 Cordis 插件/界面相关任务 → `cordis-*`。

目录里没有匹配项时，直接用 shell / 脚本完成，并在汇报里说明缺了什么能力。

## 5. 汇报格式（`send_message` 给 `lead`）

1. **结论**：一句话说清做成了什么 / 没做成什么。
2. **改动清单**：文件路径 + 关键行号（不要贴大段代码）。
3. **验证**：第 3 节全部命令的关键输出行（警告数、通过数、47/47、两条乱码自检命中数）。
4. **残余风险 / 需要 Lead 决策的点**：只写真实存在、你验证过的。
5. 不要重复叙述你已经想过但没落地的方案；不要为了显得完整而虚构未运行的测试。
