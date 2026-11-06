# DocumentTools

Windows 桌面端文档转换与办公辅助工具。**47 个功能**统一挂在左侧导航，界面为 **WPF + WPF-UI Fluent**，支持 **PerMonitorV2 DPI 自适应**（100% / 125% / 150% / 200% 均可读）。

## 1. 技术栈

| 层 | 实现 |
|---|---|
| 运行时 | .NET 8（`net8.0-windows`，WPF） |
| 界面 | [WPF-UI](https://github.com/lepoco/wpfui) 4.3.0（Fluent 主题、`FluentWindow`、`SymbolIcon` 图标） |
| 文档引擎 | Aspose.Words / PDF / Cells / Slides.NET / BarCode / HTML（版本由 `libs\Aspose.Hook\Aspose.Hook.dll` 注入） |
| 图片编解码 | SkiaSharp 3.119.0（JPG / PNG / BMP / WEBP / ICO，另含自写的 TIFF、GIF、BMP 写入器） |
| HTML 渲染 | PuppeteerSharp 20.2.4（`html.pdf`，需本机 Chrome 或 Edge） |
| 扫描仪 | NTwain 3.7.5（`twain.debug`，需 TWAIN 驱动 / DSM） |
| 汉字拼音 | NPinyin.Core 3.0.0（`name.vertical`） |
| 数据库 | `System.Data.OleDb` 访问 Access（`exam.*`） |

各组件的**实际版本**只有一处数据源：`src\DocumentTools.App\Features\Modules\About\AboutFeatureModule.cs` 的引用库卡（`Libraries` 表，界面「关于软件 → 引用库」直接显示同一张表），升包时同步该表。

## 2. 运行前提

- **.NET 8 Desktop Runtime**（开发机为 SDK）。
- **Access 相关功能（`exam.*`）**：需要 Microsoft Access Database Engine（ACE）**12.0**，且**位数与进程一致** —— 本程序按 `AnyCPU` 构建，在 64 位 Windows 上是 64 位进程，因此需要 **64 位 ACE**。缺驱动时 `src\DocumentTools.Core\Data\AccessDb.cs` 会报「未检测到可用的 Microsoft Access Database Engine (ACE) OLE DB 提供者（当前进程 64 位…）」。
- **扫描功能（`twain.debug`）**：需要扫描仪与其 TWAIN 驱动；无设备时页面显示「未检测到设备。」而不是崩溃。
- **本机 Office 转换**：`word.pdf` 的「使用本机Office转换」(`useLocalOffice`) 需要本机安装 **Microsoft Word**；未装时抛「未检测到本机 Microsoft Word（Word.Application 未注册）…请安装 Word 或取消该勾选」。该选项走晚绑定 COM，不需要引用 Office Interop 程序集。
- **发布形态**：正式产物按单文件方式分发，非托管依赖随主程序一起内嵌释放，用户不需要单独准备 DLL —— 与 `dev` 分支用 Costura.Fody 内嵌 `TWAINDSM.dll`、`WebView2Loader.dll` 的做法等价；新版对应方案见 task-41 产出的 `docs\packaging.md`。
- **TWAIN 依赖**：扫描走 TWAIN 数据源管理器 `TWAINDSM.dll`（32/64 位各一份，随发布形态内嵌）加上扫描仪自己的驱动；上面第 24 条的「未检测到设备。」就是缺其中任一环时的表现。
- **Aspose 授权**：本构建不带 Aspose 授权，按评估模式运行；评估限制与产物表现见 `docs\feature-gap.md` 第 9 节。`libs\Aspose.Hook\Aspose.Hook.dll`（net8.0 版）仍需就位，否则构建直接报错（见该目录 README）；程序启动第一件事是 `AsposeBootstrap.Start()`：开 Hook、写日志到控制台与 `logs/aspose-hook.log`。

## 3. 功能清单（47 项，顺序与左栏一致）

分类来自 `src\DocumentTools.Core\Features\BuiltInFeatures.cs`（`IFeatureCatalog` 的唯一数据源），与界面左栏分组一致。

| 分类 | 数量 | 功能 |
|---|---|---|
| 首页 | 1 | 首页（入口卡片） |
| WORD | 7 | WORD 转 PDF、合并 WORD、拆分 WORD、WORD 转 EXCEL、WORD/PDF/EXCEL 转图片、WORD 转 EPUB、WORD 内容批量替换 |
| PDF | 10 | PDF 转 WORD、PDF 转 EXCEL、PDF 转 PPT、合并 PDF、拆分 PDF、添加 PDF 水印、移除 PDF 水印、PDF 版本转换、压缩 PDF、HTML 转 PDF |
| EXCEL | 3 | EXCEL 转 PDF、EXCEL 转 WORD、EXCEL 内容批量替换 |
| PPT | 2 | PPT 转 PDF、拆分 PPT |
| 图片 | 4 | 图片转 PDF、压缩图片、WEBP 转图片、图片转 ICO |
| 电子书 | 2 | EPUB 转 WORD、EPUB 转 PDF |
| 工具 | 7 | 简易二维码、特殊二维码校验、身份证校验、姓名横转竖、Twain 扫描仪调试、其他功能、补充功能 |
| 考评 | 7 | MSS3 模板修改、单人表生成、考评表信息插入、考评报表修改、选举结果报告单样式生成、考评系统数据包管理、考评圆圈修复工具 |
| 系统 | 3 | 系统设置、系统工具、网卡设置 |
| 关于 | 1 | 关于软件 |

共性交互：批量功能都是「加文件进列表 → 设参数 → 点『开始处理』才问输出目录」；勾「保存到源目录」时**按每个文件自己的目录写回、不弹框**；处理中禁用主按钮并逐文件报「正在处理第 N 个文件 / 共 M 个」；合并类走「另存为」单文件而不是选目录。

## 4. 设置存在哪里

- 位置：`%LOCALAPPDATA%\DocumentTools\settings.json`（`src\DocumentTools.Core\Settings\SettingsStore.cs`，写入走「临时文件 + 覆盖重命名」，断电不会留半截配置）。
- 首次运行**没有**该文件时，从遗留 `App.config` 播种初始值（`src\DocumentTools.Core\Settings\AppSettings.cs` 的 `FromLegacyConfig`）；找不到配置文件就用内置默认值，`StartFeatureId` 固定回落到首页。
- 读盘逐字段修正坏值（行高夹到 18–64、非法布尔回默认、非法字符的路径清空），修正明细进 `IAppSettings.LastLoadWarnings`；**目录型设置缺失不阻断保存**，只在设置页报告 Ok / NotSet / Missing / Invalid。
- 导入 / 导出：**全有或全无** —— 只要有一个字段被拒，整次导入不生效并逐字段报告原因（`src\DocumentTools.Core\Settings\AppSettingsService.cs`）。
- 故意不迁的两项：`FormWidth` / `FormHeight`（WPF 布局自适应）与 `WidgetAnchorMode`（锚点在 WPF 无对应概念）。

## 5. 构建与验证

```powershell
dotnet restore DocumentTools.sln
dotnet build DocumentTools.sln
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj
dotnet run --project tools\FeatureSmoke
dotnet run --project tools\UiSnapshot
dotnet run --project src\DocumentTools.App
```

**一条命令跑完全部六道门**（推荐，交接/发版前用这个，不要手工拼命令）：

```powershell
& .\tools\gate.ps1 -MaxSkips 3
```

顺序固定 build → test → FeatureSmoke → UiSnapshot → QaFunction(--bonus) → QaCompat，任一门失败立即停并返回非 0；开始前会检测 `DocumentTools.exe` / `testhost` 是否占着 `bin`，占用时 **ABORT 拒绝开跑**（这是功能性正确，不是故障——把占用进程关掉再跑）。全链约 160 s（`dotnet test` 整集串行后约 60–70 s）。跳过上限：**有 Word 的机器上是 3**（ACE 用例 + opt-in 本机 Word 用例），无 Word 的机器可收到 2。日志尾部写 `%TEMP%\dt-gate-<时间戳>.log`。在 Windows PowerShell 5.1 里用 `& .\tools\gate.ps1`（脚本带 BOM、按 5.1 语法写）。

- `dotnet build` 期望 **0 警告 0 错误**（若报 `MSB3026/MSB3027` 复制失败，通常是上一次的 `DocumentTools.exe` / `testhost` 还占着 `bin`，关掉再跑）。
- `dotnet test` 期望全绿；`exam.*` 的 Access 用例在缺 ACE 的机器上会**自动跳过**并写明原因，另有 `WordLocalOfficeTests` 需要 `DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1` 才跑（会拉起本机 Word）。
- `FeatureSmoke`：用真实 DI 容器 + 真实视图工厂逐个创建 47 个页面并跑布局，输出「功能总数 47，创建成功 47，布局通过 47」与「启动落点检查 95 项，失败 0」（可见功能按配置打开、被隐藏或未注册的启动功能回退首页并有说明）。
- `UiSnapshot`：1200×800 逐页渲染 PNG，并做机器可判定检查（超出视口 / 被裁切 / 空文本按钮 / 键盘可达），默认产物在 `docs\qa-screens\`，支持 `--out` / `--only` / `--pages` / `--dpi` / `--self-test`；退出码 0 表示无致命与高/中优先级问题。

三个按需使用的工具：

- `tools\QaFunction`：端到端跑真 operation，喂 `testdata\` 夹具并逐产物结构化校验（签名 / OOXML 条目 / 页面对象），产物写 `artifacts\qa-function`，**退出码 = 失败项数**。
- `tools\QaCompat`：环境与兼容性探针（ACE / HTML / TWAIN / 许可 / 只读目录 / 非法路径），`--all` 或 `--only ace,html,twain,license,readonly,path`，报告写 `docs\qa-report-compat.md`，退出码 0 = 全部 PASS 或无法验证，1 = 存在 FAIL。
- `tools\ExamFixture`：生成考评 Access 夹具库（`--output` / `--no-seed` / `--no-boolitem` / `--probe-autoincrement` / `--selfreport`），退出码 0 成功、**2 表示本机缺 ACE/Jet**、1 其它失败。

`artifacts/` 已被 `.gitignore` 忽略，随时可删。

## 6. 评估模式与错误提示

本构建不带 Aspose 授权，所以按评估模式运行：Aspose 会限制单次处理的集合元素数（典型原文 `At most 4 elements (for any collection) can be viewed in evaluation mode.`）并在产物上留评估水印；完整的限制清单与「哪些断言因此不可复现」写在 `docs\feature-gap.md` 第 9 节。程序不把英文原文甩给用户：`src\DocumentTools.Core\Services\Office\OperationErrors.cs` 把这类异常翻译成中文可操作提示，输出目录不可写、浏览器缺失等也各有提示，原始异常保留在 `InnerException` 里便于排查。

## 7. 已知限制

- **TIF / GIF 输出走自研编码器**：SkiaSharp 3.119 没有 TIFF/GIF 编码器（`Encode` 返回 null、`SKCodec` 也解不了 TIFF），所以 TIFF 6.0 与 GIF89a 由仓库自己写（`TiffEncoder.cs` / `GifEncoder.cs`，BMP 同理）。TIFF 默认 LZW（可选 PackBits / 不压缩），**没有 flate 压缩**：A4 300dpi 扫描页实测能压到原图的 0.7%–1.5%，但彩色噪声类内容反而会涨到 102.6% —— **彩色照片请选 JPEG**。完整对照见 `docs\feature-gap.md` 第 9 节。
- **`exam.*` 的 Access 路径未在真实 VFSoft 数据上验证**：现有验证基于 `tools\ExamFixture` 生成的合成库（表结构与列类型照 legacy，但不含真实业务数据与历史脏数据）。
- **需要管理员权限的系统功能（`system.tools` / `system.network`）未在隔离机验证**：代码对需要提升的项做了可用性判断与提示，但真实域环境 / 受限账户下的行为未实测。
- 纸质与扫描相关工作流（TWAIN）只在开发机做过联调，未覆盖多品牌扫描仪。

## 8. 仓库结构

```text
├── src/
│   ├── DocumentTools.App/      # WPF-UI 壳、功能页、对话框、DI、AsposeBootstrap
│   └── DocumentTools.Core/     # 无 UI 领域层：功能目录、参数契约、operation、service
├── tests/DocumentTools.Tests/  # xUnit（Core 逻辑 + 页面契约）
├── tools/                      # FeatureSmoke / UiSnapshot / QaFunction / QaCompat / QaPerf / ExamFixture / ExportEmbeddedAssets
├── testdata/                   # 端到端夹具（进仓库）
├── docs/                       # 架构、交互规范、完成度、QA 报告
├── assets/                     # 内嵌资源、图标、授权文件（勿提交密钥）
├── libs/Aspose.Hook/           # 私有 Hook 二进制（gitignore）+ 放置说明
├── DocumentTools/              # 遗留 net48 WinForms 单体（迁移参考，不参与构建目标）
├── Directory.Build.props
└── DocumentTools.sln
```

## 9. 依赖与许可

| 依赖 | 许可 | 说明 |
|---|---|---|
| WPF-UI | MIT | UI 框架 |
| SkiaSharp / NTwain / PuppeteerSharp / NPinyin.Core | MIT 等开源许可 | 见各自包声明 |
| Aspose.* | 商业（本构建不携带授权） | 文档引擎，评估模式限制见 `docs\feature-gap.md` 第 9 节；版本与 Hook 对齐，勿随意升级 |
| 本仓库代码 | MIT | 见 [LICENSE](LICENSE) |

## 10. 贡献

见 [CONTRIBUTING.md](CONTRIBUTING.md)。架构与分层硬约束见 [docs/architecture.md](docs/architecture.md)，交互与参数规范见 [docs/feature-ux-spec.md](docs/feature-ux-spec.md)。
