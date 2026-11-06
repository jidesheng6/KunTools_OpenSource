# Contributing

感谢参与。本仓库是 **.NET 8 + WPF-UI** 的 Windows 桌面应用，业务层与界面层严格分层。下面的约束不是风格偏好——违反任何一条都会让功能「看起来能点、实际不生效」，因此 PR 会以此为准。

## 1. 环境

- .NET 8 SDK（开发）/.NET 8 Desktop Runtime（运行）
- Windows 10 1809 或更高（PerMonitorV2 DPI）
- Aspose 授权：本构建不带 Aspose 授权，按评估模式运行（单次处理每集合上限 4 个元素、产物带评估水印），限制清单见 `docs\feature-gap.md` 第 9 节；发布形态与 TWAIN 非托管依赖见 task-41 产出的 `docs\packaging.md`
- 可选：Access 相关功能需要 ACE 12.0 且位数与进程一致；扫描功能需要 TWAIN 驱动（发布形态下 `TWAINDSM.dll` 随主程序内嵌）；「使用本机Office转换」需要本机安装 Word
- `libs\Aspose.Hook\Aspose.Hook.dll`（net8.0 版）必须就位，否则构建直接报错

```powershell
dotnet restore DocumentTools.sln
dotnet build DocumentTools.sln
dotnet run --project src\DocumentTools.App
```

## 2. 分层与硬约束

1. **App 层禁止 `using Aspose.*`**，唯一例外是 `src\DocumentTools.App\Infrastructure\Licensing\AsposeBootstrap.cs`（Hook 初始化）。转换逻辑一律落在 Core。
2. **Core 禁止 WPF / WinForms**：不出现 `System.Windows.*`、`MessageBox`、控件类型；需要弹窗时由 App 侧通过 `IDialogService` 实现，Core 只返回结果与文案。
3. **视图禁止直调 Aspose、禁止直写业务参数**：参数契约集中在 Core 的 `FeatureOptionSchemas`（键 / 标签 / 类型 / 默认值 / 候选项），App 的 `OptionPanelBuilder` 只按字段类型渲染，视图只负责收集键值并交给 `IDocumentOperation`。
4. **App 内不要再声明 `Button` / `TextBox` 别名**：`src\DocumentTools.App\GlobalUsings.cs` 已全局别名到 WPF-UI 控件，重复声明会 `CS1537`。需要 `System.Windows.Shapes.Rectangle` 时用 `using Rectangle = System.Windows.Shapes.Rectangle;`。
5. **含中文的 `.cs` 必须是 UTF-8**：文件里出现「`Content =` / `base(` / `PlaceholderText =` 后面紧跟连续三个及以上问号」的字面量，一律视为失败（中文被写成问号是真实发生过的回归，见 `docs\backlog-next.md` §0 的 G4 自检命令）。用支持 UTF-8 的编辑器/写文件方式，不要用 `Get-Content | Set-Content`、`>`、`Out-File`、`Set-Content` 直接重写源码文件。
6. **可选附件走 `PathGuard.OptionalExistingFile`**：留空或路径不存在时返回 `null` 并回退到默认行为；只有必须存在的输入才用 `RequireExistingFile`。不要自己写 `File.Exists` 判断后抛裸异常。
7. **新功能放 `src\DocumentTools.App\Features\Modules\<Area>\`**，业务实现放 `src\DocumentTools.Core\Services\…` 与 `src\DocumentTools.Core\Operations\…`；纯解析/计划类逻辑放 `Core\Common` 或对应 service 旁的静态 planner。
8. **功能 id 稳定**：`word.pdf`、`exam.insert` 这类 id 是导航、设置里的隐藏项、收藏与文档的公共键，不允许改名；新增功能要进 `BuiltInFeatures.All`（分类同时进 `FeatureCategory`）。

## 3. 禁止假参数

每个进 `FeatureOptionSchemas` 的键都必须有**真实消费点**：Core 的 operation / service 读到它并改变输出，或视图确有对应行为。

- 自检方法：搜索该键名，确认除「定义 + 渲染」之外存在消费代码；若只有定义，请删键或补实现。
- 反面案例：`word.pdf` 的「使用本机Office转换」(`useLocalOffice`) 曾写成 `_ = options.UseLocalOffice;` —— 勾与不勾结果完全一致；现在真接 Word COM，并配了可选开启的回归测试（`DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1`）。
- 反面案例：`pdf.split` / `ppt.split` 曾提供「拆分后更新页码」，但只有 Word 分支消费它；现在这两个分支不显示该勾选，而不是让它静默无效。
- 当 legacy 行为本身是缺陷（例如排序开关被注释掉、结束行当删除行数传），**不要为了「对齐」而迁一个空转开关**：要么实现正确语义，要么不提供，并在 `docs\feature-gap.md` 第 9 节写明理由。

## 4. 改动前后必须跑的四条命令

```powershell
dotnet build DocumentTools.sln
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj
dotnet run --project tools\FeatureSmoke
dotnet run --project tools\UiSnapshot
```

- 构建期望 **0 警告 0 错误**；出现 `MSB3026/MSB3027` 复制失败，多半是上一次的程序或 `testhost` 还占着 `bin`，关掉重跑即可。
- 测试里 Access 用例在缺 ACE 的机器上会自动跳过并写明原因，不要为了让它们「通过」而放宽断言。
- `FeatureSmoke` 期望「功能总数 47，创建成功 47，布局通过 47」+「启动落点检查 95 项，失败 0」；页面布局改动后这项是必看项。
- `UiSnapshot` 退出码 0 表示无致命与高/中优先级问题；它会把截图写到 `docs\qa-screens\`（批量重渲染用 `--out` 指到临时目录，避免污染仓库）。

只想跑一条命令时，用发布闸门（六门串跑、任一门失败即停）：

```powershell
& .\tools\gate.ps1 -MaxSkips 3
```

它按 build → test → FeatureSmoke → UiSnapshot → QaFunction(--bonus) → QaCompat 顺序执行，开始前检测 `DocumentTools.exe` / `testhost` 占用并 **ABORT**（占用时拒绝开跑是设计行为，关掉占用进程再跑）；全链约 160 s（`dotnet test` 整集串行约 60–70 s）；跳过上限：有 Word 的机器上是 3（ACE 用例 + opt-in 用例），无 Word 可收到 2。日志尾部写 `%TEMP%\dt-gate-<时间戳>.log`。

涉及具体功能实现时，再按需跑：

```powershell
dotnet run --project tools\QaFunction      # 端到端产物校验，退出码 = 失败项数
dotnet run --project tools\QaCompat -- --all   # 环境/兼容性探针，报告写 docs\qa-report-compat.md
dotnet run --project tools\ExamFixture -- --output $env:TEMP\examfx   # 生成考评 Access 夹具
```

## 5. 文档优先级

改行为时先看文档，冲突按此顺序取信：

1. `docs\architecture.md` —— 分层与设计模式硬约束（最高）
2. `docs\feature-ux-spec.md` —— 交互、布局、参数键的权威规范
3. `docs\feature-gap.md` —— 完成度现状与「有意偏离 legacy」的理由表
4. `docs\backlog-next.md` / `docs\closeout-ledger.md` —— 当前缺口与验收口径

「与 `dev` 分支一致」这类要求的判定材料不在这四份里，而在各自的审计报告：转换域见 `docs\dev-parity-convert.md`、工具/系统域与 OCR 见 `docs\dev-parity-tools.md`、考评域见 `docs\dev-parity-exam.md`、解耦判例见 `docs\decoupling-audit.md`、发布形态见 `docs\packaging.md`（均由对应任务产出，出稿前先按任务卡口径执行，不要引用未落盘的内容）。

行为变化请同步更新对应文档；**有意偏离遗留实现**的，写进 `docs\feature-gap.md` 第 9 节并给出理由，不要只改代码。

## 6. Pull request

- 一个 PR 尽量只做一件事；功能改动请说明用了哪种既有模式（Feature Module / Operation / Service），以及新增了哪些参数键与它们的消费点。
- UI 改动附上 DPI 100% 与 150% 的截图（`tools\UiSnapshot --dpi 120` 也可）。
- 不要提交 `bin/`、`obj/`、`artifacts/`、授权密钥、个人绝对路径 HintPath，也不要提交 `assets\embedded\` 下的专有二进制。
- 提交信息用一句话说清「改了什么 + 为什么」，中文即可。
