# 收口账本（PM：task-15 初版 → task-22 同步 → 收口定稿 → task-46 dev 对齐同步 → dev 对齐收口）

**当前阶段：dev 对齐收口，待用户验收。**

**收口摘要（dev 对齐收口定稿，2026-11-06 01:02）**：功能面闭合边界不变——47 个 FeatureId 全部能建能排（FeatureSmoke 47/47/47 + 启动落点 95 项失败 0），26 个转换页的 Schema 键与 Operation 消费链逐条对得上、面板不再有「有控件无消费」的假参数（最后一个 `voteGenDate` 已接线，`ExamVoteView.cs:73-79`；`tests\DocumentTools.Tests\FeatureOptionSchemaReachabilityTests.cs` 守门：`:41` 断言解析到的 Schema 键 `>= 100`、`:55` 断言孤儿键为 0），P1-1..P1-7 与 P2-1..P2-10 在册项或已关闭或不适用。**基准已从 legacy 升级为 `dev`（tip `ce4cc88`）并对齐收口**：审计报告均已出稿，本账本 §1 已按它们回写过一轮，后续冲突仍以报告为准。同批新增工作也已定稿：单文件发布与移除非托管依赖（`docs\packaging.md`）、壳层与工具页 UX 加固（`docs\ux-shell-polish.md`、`docs\ux-tools-polish.md`）、视图与业务解耦判例（`docs\decoupling-audit.md`），UI/UX 的自动化判定口径见 `docs\qa-report-ux.md`。**授权口径**：本构建不带 Aspose 授权，评估限制与产物表现只认 `docs\feature-gap.md` §9 的 blockquote（`docs\feature-gap.md:195`），本账本不重述、也不再给「放授权文件」类引导。全链闸门最新定稿：`& .\tools\gate.ps1 -MaxSkips 3` → **总判定 PASS / 退出码 0 / 142 s**（2026-11-06 01:00:08–01:02:30，日志 `%TEMP%\dt-gate-20261106-010008.log`）。仍需外部条件才能证明的项（含 OCR 不迁的前置条件）不记成缺陷，逐条列在 §4，报「未验证」而非「已通过」。

**dev 对齐四条线（已收口，判断面见对应审计报告，本账本只记结论）**

- **task-38 转换域**（`docs\dev-parity-convert.md`）：`pdf.compliance` 由 5 项补齐为 dev 的 **27 项**（Schema `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:356` 段、注释 `:359`、选项起 `:364`，默认第 0 项 `PDF_A_1A`）；`webp.image` 补齐 dev 的 **5 档同序**（同上 `:522-536`，选项 `:530-534` JPG/JPEG/PNG/BMP/GIF），并锁回归 `DevParityConvertTests`（`docs\dev-parity-convert.md:167-168`）。
- **task-39 工具/系统域**（`docs\dev-parity-tools.md`）：本轮真修 4 项——网卡静态 IP 格式校验（新 `src\DocumentTools.Core\Services\System\NetworkAddressValidator.cs:8`，入口 `:11`）、路径监视延迟与二次确认（新 `NewFileWatchFilter.cs:9`）、键盘锁定（新 `KeyboardLockService.cs:10`）、UAC 开关（`WindowsSystemToolsService.cs:93`）；**判定不迁 3 项**（彩蛋音效、系统通知 Toast、测试文件脚手架，其中前两项 Lead 已确认放弃）、**待迁 2 项**（图片目录批量分类、OCR），判断面见 `docs\dev-parity-tools.md:142-163`。
- **task-40 考评域**（`docs\dev-parity-exam.md`）：已修 6 条——MSS3 页码改 0 基下拉（`ExamMss3View.cs:45-48`）、项目列字段长度恢复（`ResetItemColumnLengths`）、数据包单位顺序（`ExamPackageService.cs:77`/`:225`）、单张图片收集（`CollectExternalImages`）、自动旋转（`ExamCircleOrientationService.cs:26`/`:39`）、圆圈替换图插值改回 dev 的 `Linear`（`ExamCircleFixService.cs:92-95`）；圆圈等价性实测 `maxDiff=0`（`docs\dev-parity-exam.md:80`）；仍未迁 3 项见 `:192-198`。
- **task-42/43/44 UX 三线**（`docs\ux-shell-polish.md`、`docs\ux-tools-polish.md`、`docs\qa-report-ux.md`）：全量 47 页 `high=0 medium=0 low=6`（6 条全是「页面本来没有输入控件」观察项，`docs\qa-report-ux.md:12`）；Tab 顺序问题 42 页 53–70 条 → **53 → 0**（`FeaturePageBase` 由 `DockPanel` 改 `Grid`，`docs\qa-report-ux.md:110`/`:119`），且不依赖 `TabIndex`；壳层导航行高 28.5 → 34（`docs\ux-shell-polish.md:36`），路径框统一走新建 `Ui\PathInputs.cs`（`IsReadOnly=true`，`:51`）。至于「有意偏离 legacy」的全部条目，唯一权威清单在 `docs\feature-gap.md` §9，本账本只引用不重复。

- 取数时刻：2026-11-05 19:37 – **2026-11-06 01:02**。门槛窗口本轮 = 21:15 单点复测 + **01:00:08 全链闸门定稿（142 s）**；上一轮 22:02:18（160 s）与更早 19:52–20:05 的数字保留在 §0 尾注，便于对比「同期涨了多少」。
- 方法：每条状态都由我直接读源码 / 截图得出，**不引用他人汇报**；读不到运行行为的写「未验证」。**行号是取数时的行号，结论按行为判定，不按行号判定**（团队仍在并发提交，19:58 就撞到过一次 App 层红）。
- 上游：`docs\backlog-next.md`（§3.2 P1-1..P1-7、§3.3 P2-1..P2-9、§4「并不缺」）。
- 边界：本次只改这份文档与 `docs\backlog-next.md`，未动任何源码 / 测试 / 工具。

## 0. 门槛实测（2026-11-06 01:00:08 全链闸门定稿）

### 0.1 基准分支 = `dev`

「对齐 legacy WinForms」不再是唯一基准：`dev` 分支（tip `ce4cc88`，2026-11-04）是并行的**事实基准**——它是同一套 WinForms 界面上做过服务层重构的版本（`DocumentTools\Services\`，含 `AudioService`、`RegistryService`、`ImageProcessingService`、`UIAdapter`、`DocumentConversionService` 等），用户要求新版与它在**业务行为与 UI 行为**上一毛一样，并且用 Costura.Fody 单文件发布（`DocumentTools\DocumentTools.csproj` 里 `PackageReference Costura.Fody`、`EmbeddedResource` 打进 `costura-win-x64\TWAINDSM.dll`/`WebView2Loader.dll`/`SQLite.Interop.dll`/`OpenCvSharpExtern.dll` 等、`FodyWeavers.xml` 在 `DocumentTools\` 根）。本仓库当前分支的 HEAD 与 `dev` 是同一个提交（`ce4cc88`），差异全在工作树里未提交。

对齐结论不写在本账本，由各域审计报告承担（**均已出稿，本账本只引用结论与行号**）：转换域 → `docs\dev-parity-convert.md`（226 行）；工具/系统域与 OCR → `docs\dev-parity-tools.md`（174 行）；考评域 → `docs\dev-parity-exam.md`（209 行）；发布形态 → `docs\packaging.md`（198 行）；壳层 UX → `docs\ux-shell-polish.md`（134 行）；工具/转换页 UX → `docs\ux-tools-polish.md`（117 行）；UI/UX 自动化审查 → `docs\qa-report-ux.md`（144 行）；解耦判例 → `docs\decoupling-audit.md`。索引与权威优先级见 `docs\INDEX.md`。

### 0.2 发布形态（数字全部取自 `docs\packaging.md` 的实测，本账本不重述理由）

- **产物**：自包含单文件 `DocumentTools.exe` **160,590,199 B**（`docs\packaging.md:33`）；`artifacts\publish\win-x64` 共 **14 个发布文件** + 1 个运行期日志（`:29`）；首次发布合计 **168,635,741 B**（不含 `logs\`，含日志为 168,641,558 B，`:46`）。
- **冷启动**到窗口出现 **1.7–2.6 s**（`docs\packaging.md:64`、`:169` 两处一致；测量条件「先删 `%TEMP%\.net\DocumentTools`」），区间上界来自首次解压原生库、不写第二个来源不明的数字。
- **必须保住 `EnableCompressionInSingleFile=true`**：开启时 exe 由 334.6 MB 压到 160.6 MB（`:62`）；关掉后产出的 334.6 MB exe **连续 3 次卡死在 `AsposeBootstrap.Start` 的 `StartHook` 之前**（进程常驻、无窗口、日志停在 "Loaded Aspose assemblies (before-StartHook)"，`:173-175`）。
- **必须 `ExcludeFromSingleFile`**：`TWAINDSM.dll`（206,240 B，NTwain 只按 exe 目录/System32 探测，`:34`）、`assets\embedded\PersonCode.zip`（2,288,286 B，运行期按相对路径读取，`:35`）、`assets\embedded\**`（`:68`）——打进单文件则运行期读不到。
- 单文件下 TWAIN 身份已改 `TWIdentity.Create`（见 §4 第 3 条），否则 `Assembly.Location` 恒为空会崩。

| 门槛 | 命令 | 实测结果 |
| --- | --- | --- |
| G0 全链闸门 | `& .\tools\gate.ps1 -MaxSkips 3` | **总判定 PASS，退出码 0，142 s**（2026-11-06 01:00:08–01:02:30，日志 `%TEMP%\dt-gate-20261106-010008.log`）；六个门全过，下面 G1/G2/G3/G9/G10 的定稿数字均取自这一跑。上一轮 22:02:18 为 160 s，缩短来自测试集并行不确定修复后的稳定耗时 |
| G1 构建 | `dotnet build DocumentTools.sln -v q --nologo` | **0 个警告 / 0 个错误**（闸门内 `--no-incremental` 4.2 s；20:50 单跑亦 0/0，20:46–20:49 曾连续撞到他人并发红：`ExamInsertView.cs` CS0103 `WeightOr`、`ExamVoteView.cs` CS0103 `ParseDouble`、`SingleDocumentNamingTests.cs` CS1674，均已自愈） |
| G2 单测 | `dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo` | **定稿（01:00 闸门内）：通过 505 / 失败 0 / 跳过 3（上限 3）/ 总计 508，53.8 s**。3 个跳过 = 三条 Word opt-in 白名单用例（需 `DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1`），`gate.ps1` 会逐条打印 `跳过用例：{name}`（`tools\gate.ps1:218`），日志文件只留摘要行；**上限是白名单不是免检额度**——没装 Word 的机器传 `-MaxSkips 0` 也不该红（`tools\gate.ps1:28-33`），判定实现见 `:205`（`$skippedCount -le $MaxSkips`）。**耗时 26 s → 53.8 s 是并行不确定修复的固定成本，不算退化**（见下方「本轮修掉的两条真缺陷」①）。22:02 闸门内为 363/366（69.4 s），增量是本轮新增的 dev 对齐与 UX 回归用例；20:57/21:03 记到的「失败 1（`Task13OptionTests.PptPdf_ExportNotesIsPositionChoice`）」是旧二进制下的过时断言（该测试现已按 3 档断言，`tests\DocumentTools.Tests\Task13OptionTests.cs:219-233`，与 `FeatureOptionSchemas.cs:60-65` 一致） |
| G3 冒烟 | `dotnet run --project tools\FeatureSmoke` | 两行：**功能总数 47，创建成功 47，布局通过 47** + **启动落点检查 95 项，失败 0**（闸门内 6.2 s，exit 0；后者是 `NavigationFallback` 判定：可见功能按配置打开、被隐藏/未注册的启动功能回退首页并有说明） |
| G9 快照巡检 | `dotnet run --project tools\UiSnapshot -- --out %TEMP%\…`（不写仓库） | 闸门内 29.8 s：`SUMMARY pages=47 rendered=47 fatal=0 high=0 medium=0 low=6 oversized=0`，exit 0；`SUITE|hidpi=16|interact=4|interactOk=4|interactHang=0`（16 张高 DPI = 8 页 × 120/144 DPI，H0/M0；4 个交互场景全 ok：word.pdf / exam.vote / code.barcode / exam.circle）。日志文件只留摘要，个案明细在 `docs\qa-screens\_index.csv` / `_hidpi.csv` / `_interact.csv`（**`_index.csv` 只有全量运行有权改写**）。low=6 全是「页面本来没有输入控件」观察项（home/about/word.merge/pdf.merge/code.datamatrix/other.misc，见 `docs\qa-report-ux.md:12`）；早前一次 `medium=1`（system.settings 空文本按钮）单页复跑 `medium=0`、全量复跑亦 `medium=0`，判为并发构建瞬态 |
| G10 工具可运行 | `QaFunction`（全量）/ `QaCompat --all` / `ExamFixture --output <temp>` / `QaCompat --help` | 闸门内：**QaFunction 必测 16/16、扩展 42/42、追加 4/4 全 PASS，FAIL 行 0、SKIP 行 0**（43.1 s）；**QaCompat `PASS 14 / FAIL 0 / 无法验证 2`**（4.8 s，两条无法验证是 `ace.real-missing`、`twain.no-source`）；`ExamFixture` 生成 `Exam_Base.mdb`(360448 B) 并列出各表行数、exit 0；`QaCompat --help` 打印用法与退出码说明、exit 0。`QaFunction` 写 `artifacts\qa-function\result.json|result.md`（该目录被 `.gitignore:64` 忽略），**退出码 = 失败项数** |
| G4 占位乱码 | 扫描 `Content = ` / `base(` / `PlaceholderText = ` 后紧跟连续三个及以上问号的字面量（正则见 `docs\backlog-next.md` §0） | **0 命中** |
| G5 GBK 乱码 | `锛\|銆\|鐨\|鏂\|鎴\|鍥剧\|鏂囨。\|闇€` | **0 命中** |
| G6 App 直调 Aspose | App 层 `using Aspose` | **0 命中** |
| G7 Core 含 WPF/对话框 | Core 层 `System.Windows` / `MessageBox` | **0 命中** |
| G8 壳层直弹 MessageBox | `MainWindow.xaml.cs` 内 `MessageBox.Show` | `MainWindow.xaml.cs` **0 命中**；扫描 `Shell` + `Features` 只允许 1 条 `src\DocumentTools.App\Shell\CrashLog.cs:94`（`:93` 注释声明「全库唯一允许的 MessageBox」，容器未就绪时先走 `IDialogService` → `:86-90`）；Facade 三处 `IDialogService.cs:55`、`:58`、`:61` 属实现体，不计入 |

**本轮修掉的两条真缺陷（都是验收阶段才暴露，记进账本备查）：**

1. **测试集并行不确定（同一命令两次红的是不同用例）**：`ExamIndexServiceAccessTests` / `ExamReportExtrasAccessTests` 里的 ACE 用例会互相踩（进程级 OLEDB/临时库状态共享），表现为「这次红 A、下次红 B」，单看某一次跑很容易误判成实现回归。修法双层：① `tests\DocumentTools.Tests\xunit.runner.json` 置 `"parallelizeTestCollections": false`（整集串行）+ `"longRunningTestSeconds": 120`；② 对改进程级环境变量的用例再收一层集合 —— `tests\DocumentTools.Tests\ConversionLogCollection.cs:9` `[CollectionDefinition(Name, DisableParallelization = true)]`，`WordLocalOfficeTests.cs:13` 与 `WordLocalOfficeDiagnosticsTests.cs:13` 挂 `[Collection(ConversionLogCollection.Name)]`。**代价：test 从 26 s 涨到 53.8 s（01:00 闸门内），记为固定成本，不算性能退化。**
2. **`tools\gate.ps1` 无 BOM 时在 PowerShell 5.1 下不可运行**：文件当时是无 BOM 的 UTF-8，PS 5.1 的 `ParseFile` 会按 ANSI 解中文注释，直接报 38 个语法错误——也就是说脚本头部写的 `powershell -File` 调用方式**当时是跑不起来的**。现已改为带 BOM（实测首字节 `EF BB BF`），`& .\tools\gate.ps1` 可正常执行（22:02 那一跑即证）。

**更早一轮（19:46–19:49）的两条红灯根因，归档如下，不必再查：**

1. `ExamPackageServiceAccessTests.Merge_TargetAutoIncrementColumn_IsNotCopiedFromSource`（`找不到输出表 'Exam_PeopleDes'`）—— 夹具前缀误删。现 `tools\ExamFixture\ExamFixtureBuilder.cs:343` 有注释「名字更长的 Exam_PeopleDes 一起删掉。」、`:344` 的 `RemoveAll` 已改为精确匹配；`:414` 仍正常向三张绑定表各插两行。191 例全绿即证。
2. `ExamReportServiceAccessTests.ReplaceContent_ContentWithoutRaid_AppendsRaid`（期望 `Item=甲项&Raid=9`，实际 `Item=甲项&Raid=1`）—— 已按实现侧口径对齐（`ExamReportService.cs:157` 注释：改 Kind/Raid 只作用选中项目；legacy 同义，`DocumentTools\Util.cs:854` 的 `WHERE ReportID=… AND Content='{TempContent}'`）。

## 1. P1 / P2 逐条对账

状态口径：**已关闭** / **部分完成** / **不适用**（legacy 无此语义）。所有「已关闭」都附我复核过的锚点。

| 编号 | backlog 要求（摘要） | 状态 | 复核证据（文件:行号） | 结论 |
| --- | --- | --- | --- | --- |
| **P1-1** | 「保存到源目录」逐文件写各自源目录、不弹框 | **已关闭（按 Lead 定案）** | `ConvertFeatureModule.cs:183-188`：勾选 → `RunPerFileAsync(files, outputDirectory: null, writeToSource: true)`，`:185` 注释「逐文件调用 → 每个文件都能写回自己的目录（规范 §3.2），不需要统一目录，也不弹框」；`:283` `OutputDirectory = writeToSource ? fileDirectory! : outputDirectory!`；`:275` 状态带「（保存到本文件所在目录：{fileDirectory}）」。需要统一目录的只剩多对一：`image.pdf` 未勾「每张图一个 PDF」→ `:168-181` `RunBatchAsync`，跨目录时 `:232` 提示「合并结果只能保存到一个目录，但所选文件来自 {N} 个不同目录；请选择输出目录。」；合并页走 `:160` `RunMergeAsync`（SaveFile） | 验收口径已成真：`A\a.docx`+`B\b.docx` → 生成 `A\a.pdf`、`B\b.pdf`，不弹框；`image.pdf` 合并模式仍按需选统一目录 |
| **P1-2** | 逐文件「正在处理第 N 个文件」，结束首段「处理完毕」 | **部分完成** | 转换页达标：`:275-276`「正在处理第 {i+1} 个文件 / 共 {total} 个：{fileName}」、`:302-303`「处理完毕，成功 {n} 个文件{writtenBack}。」/「处理完毕，成功 {n} 个{writtenBack}，失败 {failed} 个。{reasons}」；逐文件执行 `:248-321`（每轮单文件请求）。替换页未达标：`ReplaceFeatureModule.cs:149` 仍「正在处理 {_list.Paths.Count} 个文件…」，`:152-157` 整批一次 `ExecuteAsync` | 26 个转换功能共用页已关闭；`word.replace`/`excel.replace` 残留（两页 legacy 也是整批跑，故按「不阻断」处理，见 §4 第 5 条） |
| **P1-3** | `code.barcode` 补「识别条码」 | **已关闭** | `src\DocumentTools.Core\Services\Code\IBarcodeService.cs:25` `IReadOnlyList<string> ReadTexts(string imageFile);`；实现 `BarcodeService.cs:59`、`:73`（`new BarCodeReader(stream, DecodeType.AllSupportedTypes.Exclude(DecodeType.MicrE13B))`）；UI `BarcodeFeatureModule.cs:90` 卡「条码识别」、`:93` 「识别条码」、`:94` 「复制识别内容」，文案 `:297`「还没有识别结果：请先「识别条码」。」`:302`「已复制识别内容到剪贴板。」 | 测试按评估版口径断言：QR 断前缀（评估版会在解码结果尾部追加 Aspose 水印句），Code39 断精确值；纯白图返回空列表不抛 |
| **P1-4** | 查找/替换拆成两张卡（原型 D） | **已关闭** | `ReplaceFeatureModule.cs:91` 注释「原型 D：查找 / 替换是两个分区」、`:92-96` `Card("查找内容", Hint(配对说明), pairBar, _findHost)`、`:97` `Card("替换内容", _replaceHost)` | 与 spec 原型 D 一致；`excel.replace` 的「EXCEL 清空行数据」仍作为 Schema 分区存在（`FeatureOptionSchemas.cs:579-582`） |
| **P1-5** | 壳层两处直接 MessageBox → `IDialogService` | **已关闭** | `MainWindow.xaml.cs:21` `private readonly IDialogService _dialogs;`、`:258` `_dialogs.Info($"当前版本没有「{featureId}」这个功能入口。", "提示")`、`:336` `_dialogs.Error($"打开功能失败：{featureId}\n\n{ex.Message}")`；文件内 `MessageBox.Show` = 0 | G8 达标，未注册入口文案保留 |
| **P1-6** | 拖放按功能后缀过滤、被拒不进列表 | **已关闭** | `BorderedFileList.cs` 的 `FileFilter` 属性 + `OnDrop` 用 `MatchesFileFilter` 分流并统计拒绝数；三处接线 `ConvertFeatureModule.cs:91-95`（`:91` 设 `FileFilter`、`:92-95` 提示「已忽略 {N} 个不支持的文件。」）、`ReplaceFeatureModule.cs:84`、`DataMatrixFeatureModule.cs:58`（该页用本地 `DropFilter` 常量 `:40`） | `word.pdf` 拖 `.txt` 不入列表且提示；`image.compress` 拖 `.png` 正常入列表 |
| **P1-7** | `refreshPageNumber` 只有 word 生效 | **已关闭（B 方案）** | `FeatureOptionSchemas.cs:317-318` 注释、`:319-322` `"pdf.split" => SplitSections(..., includeRefreshPageNumber: false)`、`:323` `"word.split"` 走默认 true、`:324-327` `"ppt.split"` false；`SplitSections(..., bool includeRefreshPageNumber = true)` 定义在 `:625`，勾选项按 `:642-645` 条件追加 | pdf/ppt 面板不再出现该假参数；缺一条「pdf.split 面板无此键」的 Schema 断言 |
| **P2-1** | 条码补 rotate / sizeMode / 宽 / 高 | **已关闭（产物断言已补）** | `BarcodeRenderOptions.cs:25` `Rotate`、`:28` `SizeMode`、`:31` `BarcodeWidth`、`:34` `BarcodeHeight`；渲染 `BarcodeService.cs:56` → `ApplyAppearance`（先缩放后旋转、90/270 交换宽高、角度归一）；Schema `FeatureOptionSchemas.cs:417-420`（`sizeMode`/`barcodeWidth`/`barcodeHeight`/`rotate`，选项表 `:40` `BarcodeSizeModes`）；**产物级断言**：`tools\QaFunction\Program.Extended.cs:99` `RunExt("code.barcode.appearance", CheckBarcodeAppearance)` → `:1069` 注释「P2-1：`sizeMode=fixed` 的像素尺寸、`rotate` 的宽高互换，以及『确实旋转了』的方向证据」、`:1126` `Require(r90W == plainH && r90H == plainW, …)` | 功能齐；像素尺寸与 90° 宽高互换由 QaFunction 实测（定稿一跑：PASS） |
| **P2-2** | 身份证结果补第 4 行「校验码」 | **已关闭** | `IdCardFeatureModule.cs:44` 出生日期、`:45` 性别、`:46` 顺序码、`:47` 校验码 | 4 行齐全；复制结果同步含 4 项 |
| **P2-3** | 关于页补「捐赠」「软件说明」 | **已关闭（超出原要求）** | `AboutFeatureModule.cs:67-72` `Card("软件说明", …)`、`:77-81` `Card("捐赠", …)` + `:81` `MakeButton("查看捐赠说明", ControlAppearance.Secondary, ShowDonate)`、`:267` 注释（优先用 `assets\donate\*.png\|jpg`）、`:327` `"捐赠说明"` | 「开发信息 / 引用库 / 下载框架 / 更新链接」仍未迁，原条目已声明「其余可选」 |
| **P2-4** | twain 分区对齐 legacy 4 个 | **已关闭** | `MiscToolModules.cs:102-103`「TWAIN 设置」、`:107-108`「扫描配置」、`:113-114`「数据信息」（设备/会话状态/保存目录/本次张数/耗时）、`:122`「扫描图片预览」 | 与 legacy `TWAIN设置/扫描配置/数据信息/扫描图片预览`（Designer `10484/10238/10094/10180`）一一对应 |
| **P2-5** | DataMatrix 结果树套「解析信息」卡 | **已关闭** | `DataMatrixFeatureModule.cs:72` 注释、`:73` `Card("解析信息", _treeFrame)` | 与 legacy `groupBox14="解析信息"`（Designer `8421`）一致 |
| **P2-6** | name.vertical 拼音/双字对齐/替换/计数/导出 | **已关闭** | Core：`NameVerticalService.cs:10` `NamePinyinMode`、`:39` `NameVerticalOptions`、`:69` `PinyinMode` 默认 `InitialsUpper`、`:73` `NameVerticalResult(Lines, DuplicateLineIndices)`、`:82` `ToVerticalText()`、`:120` `ToVertical(...)`、`:135` `Process(...)`；Schema `FeatureOptionSchemas.cs:525-541`（文本清洗 `:527-530` / 文字处理 `:531-536` / 拼音与重名 `:537-540`，含 `doubleAlign` `:532`、`replaceEnabled` `:533`、`pinyinEnabled` `:538`、`pinyinMode` `:539`、`markDuplicate` `:540`）；**UI 已接线**：`NameVerticalFeatureModule.cs:109` 源文本卡、`:113-114` 重名检查卡、`:124` 竖排结果卡、`:154` 「导出 TXT」、`:170` `_service.Process(...)`、`:219-225` 消费 doubleAlign/replace/pinyin、`:188-190` 计数与去重行数、`:347/354/362/366` 导出文案 | Lead 交办时以为「UI 还在 task-16」，实际已落地：面板按键走 Schema 三张分区，双字对齐/替换/拼音/计数/导出全部可见可点 |
| **P2-7** | image.compress 补 noCompress / useSourceExtension / bmpPixelBits | **已关闭（产物断言已补）** | Schema `FeatureOptionSchemas.cs:466-493`：格式表 `:470-481`（PNG/JPG/JPEG/BMP/**TIF/TIFF/GIF**）、`:484` `Check("useSourceExtension", "沿用源文件后缀名")`、`:485` `Check("noCompress", "不执行压缩操作")`、`:493` `Choice("bmpPixelBits", "色彩位数", BmpPixelBits, 3)`（选项表 `:180`）；消费端 `ImageConvertOperation.cs:71`（`bmpPixelBits`）、`:74`（`useSourceExtension` 默认 false）、`:75`（`noCompress`）；TIF/GIF 走自写编码器 `src\DocumentTools.Core\Services\Image\TiffEncoder.cs` / `GifEncoder.cs`（`ImageConversionService.cs:231` `TiffEncoder.Encode(...)`、`:233` `GifEncoder.Encode(...)`）；**产物级断言**：`tools\QaFunction\Program.Extended.cs:100-102` 三个 `RunExt`（`image.compress.useSourceExtension` / `.noCompress` / `.bmpPixelBits`），实现体 `:1250`（同名换后缀）、`:1277`（`noCompress=true` 只改名不重编码）、`:1309`（直读 BMP 头 `biBitCount`） | 键位、消费链、产物三层齐；TIF 体积口径见 §4 第 6 条 |
| **P2-8** | ppt.pdf 备注位置升级为 Choice | **已关闭（口径收窄为 3 档，产物断言已补）** | `FeatureOptionSchemas.cs:60-65` `PptNotePositions` = `None`/`BottomFull`/`BottomTruncated`（`:58-59` 注释：早先的「右侧占位」档会让导出静默退回 `None`、备注全丢，已按真实能力收窄），`:265` `Choice("exportNotes", "备注导出位置", PptNotePositions, 0)`；映射仍兼容旧配置：`OfficeConvertOperation.cs:123` `NotesPosition = MapNotesPosition(options)`、`:217-237` 接受 `BottomFull`/`BottomTruncated`/`RightFull`（其余落 `None`）并把旧布尔 `true→BottomFull`、`false→None`；`OfficeConvertService.cs:184-191` 用 `Enum.TryParse` 消费；**产物级断言**：`tools\QaFunction\Program.Extended.cs:103` `RunExt("ppt.pdf.exportNotes", CheckPptExportNotes)` + `tools\QaFunction\FileProbe.cs:263` 注释「第一页的 MediaBox 尺寸（pt）。用于证明 exportNotes=RightFull 真的加宽了页面」 | 面板三选一真实生效、产物侧有页面尺寸证据；`RightFull` 只在映射层兼容、面板不再提供。**单测已同步为 3 档**（`Task13OptionTests.cs:219-233`），先前「需测试侧改断言」的记述已作废 |
| **P2-9** | 替换页补「忽略脚注」勾选 | **已关闭（word）/ 不适用（excel）** | `FeatureOptionSchemas.cs:556` `Check("ignoreFootnotes", "忽略脚注")` 位于 `word.replace`（`:550`）的「匹配选项」；`excel.replace`（`:573`）无此勾选；消费端 `ReplaceDocumentOperation.cs:39` | Excel 无脚注语义，不给 excel 面板加该键是正确取舍 |
| **P2-10** | 假参数：`voteGenDate` 只有 Schema 定义 | **已关闭（接线完成）** | `FeatureOptionSchemas.cs:592` `Text("voteGenDate", "生成日期(留空=当天)", width: 150)`；**接线证据**：`ExamVoteView.cs:73` `_genDate` 定义、`:76` `Text = SchemaDefault("voteGenDate", string.Empty)`、`:116` 参与布局、`:190` `GenerationDate = ExamViewKit.Value(_genDate)`；请求字段 `IVoteReportService.cs:35`；守门测试 `tests\DocumentTools.Tests\FeatureOptionSchemaReachabilityTests.cs`（`:41` 键数 `>= 100`、`:55` 孤儿键 0） | Lead 裁决**接线而非删键**：`voteFontSize`/`voteRowHeight` 两个兄弟键在 `ExamVoteView` 有消费点（`:64,:70`），只删 `voteGenDate` 会让「同一张卡三个参数只有两个生效」变成「只剩两个」，用户手工指定生成日期的诉求会消失；`VoteReportService.ResolveGenerationDate` 早已支持。**验收口径**：填了日期→产物日期随之变；留空→当天日期 |

## 1.1 本轮新关闭（原不在 P1 / P2 清单，属考评与文档偏差）

| 项 | 证据（文件:行号） | 结论 |
| --- | --- | --- |
| 考评改号 / 物理顺序重建 | `ExamIndexService.cs:8` 请求（`OriginIndex` `:13`、`NewIndex` `:16`、`RebuildPhysicalOrder` `:23` 默认 true）、`:27` 结果（`Swapped` `:32`、`PhysicalOrderRebuilt` `:37`、`UpdatedTables` `:40`）、`:31` 注释「目标序号已被占用时 legacy 走三步换位（Util.cs:3863-3887）」、`:93` `ChangeExamIndex`（事务 `:139`→`:190`）、`:284` 物理重建（事务 → `:348` Commit，`:353` 文案「物理顺序已按 ExamID 升序重建，{n} 条记录逐列校验一致」）；测试 `ExamIndexServiceAccessTests.cs`；UI `ExamInsertView.cs:153-154` 卡「重排表号」、`:164` `Secondary("重排表号", ChangeExamIndex)`、`:373` 注释 | 已关闭。三步换位 + 事务内 DELETE/INSERT 重写 + 逐列校验都在；读不到「真实考评库上跑一遍」（§4 第 5 条） |
| 考评批量模板 | `ExamBatchInsertTemplates.cs:13` 类、`:22` 剪贴板分隔符、`:26-27` 注释「序号即 legacy `CustomSelectors_1`…`CustomSelectors_11` 后缀」、`:28-41` **11 组**内置选项（好/较好/一般/差 … A/B/C/D、a/b/c/d）、`:47` `ExpandItems`（`:53` X 占位符校验）、`:77` `ExpandSelectors`、`:100` `FromPreset`、`:121` `FromNames`、`:157` `ParseClipboardNames`、`:172` `FromClipboard`；测试 `ExamBatchInsertTemplatesTests.cs`；UI `ExamInsertView.cs:131-132` 卡「批量项目」、`:144-145` 卡「批量选项」 | 已关闭。11 组与 legacy 序号一一对应，中文序号与剪贴板解析都在 |
| 报表结构扩展 | `ExamReportService.cs:28` `ChangeDataFormat`、`:30` `NewDataFormat`、`:37` `OldRaid`、`:75` `GetGridSize`、`:77` `ListExamRaids`、`:89` `KindRegex`、`:173-183` `OldRaid` 定点替换 / 无段追加、`:190-192` `Kind=` 替换、`:223`/`:255` 两个实现；测试 `ExamReportServiceAccessTests.cs` + `ExamReportExtrasAccessTests.cs`；UI `ExamReportView.cs:106-119` 卡「报表结构」（`:107` 标题、`:108` 读取结构与身份、`:119` 测评身份）、`:121-135` 卡「内容替换」（`:131` 原 Raid、`:132` 新 Raid、`:134` 新数据格式）、`:27-33` 三个输入/两个勾选 | 已关闭。身份下拉回填（`:67-72`）与空值拦截（`:269`）都在 |
| `twain.debug` 文档偏差 | `ITwainScanService.cs:14` 接口、`:22-26` 重写后的注释（`:23` 返回落盘路径、`:24` 无扫描仪/无 DSM 抛 `NotSupportedException` 且消息取 `UnavailableReason`、`:25` 有数据源但没扫到返回空集合）、`:27` `Scan(...)` 签名 | 已关闭（文档侧）。之前账本写「返回空集合」而未区分抛异常分支，现按接口注释对齐；实机扫描仍未验证（§4 第 4 条） |
| 姓名纵排 Core（P2-6） | 见 §1 P2-6 行 | 已关闭，且 UI 已于本轮回填（不再等 task-16） |
| TIF / GIF 输出已落地（自写编码器） | `FeatureOptionSchemas.cs:470-481`（格式表含 TIF/TIFF/GIF）、`ImageConversionService.cs:231`（`TiffEncoder.Encode`）、`:233`（`GifEncoder.Encode`）；实现体 `src\DocumentTools.Core\Services\Image\TiffEncoder.cs`、`GifEncoder.cs`；体积与时延实测、决策理由见 `docs\feature-gap.md:164`（TIF）与 `:167`（GIF） | 已关闭。SkiaSharp 3.119 无 TIFF 编码器且 `Encode(Gif)` 返回 null，故自写；**体积口径见 §4 第 6 条** |
| `word.pdf` 删掉 `removeTempDocx` 键 | `src` 全层 grep `removeTempDocx` **0 命中**；当前键表 `FeatureOptionSchemas.cs:211-224`（输出与后续只有 `saveSource` `:214` + `openPdfOptions` `:215`） | 删键成立：`OfficeConvertService.WordToPdf`（`:20`）与 `WordToPdfViaLocalOffice`（`:110`）都不产生 temp docx 目录，保留该勾选等于假参数 |
| `useLocalOffice` 真接 Word COM | `OfficeConvertService.cs:30 WordToPdf` → `:41` 在 `options.UseLocalOffice` 时转 `:118 WordToPdfViaLocalOffice(...)`；Schema `FeatureOptionSchemas.cs:219` | 已关闭。该键曾是「面板有、实现无」的假参数，现为真实分支（无 Word 时抛中文提示） |
| `word.replace` 只认 `regexSub` | `ReplaceDocumentOperation.cs:47` `UseRegexSubstitutions = IsTrue(request, "regexSub")`，`:45` 注释「legacy 也只由 `UseRegexSub` 驱动（`DocumentTools\Util.cs:5380` 参数、`:5410` `UseSubstitutions = UseRegexSub`；源码注释里的 `DocumentTools\Util.cs:5461` 是旧行号（现指向 Excel 保存行），实为 Excel 保存行）」 | 已关闭。此前兼容别名会让「面板未勾也走正则」的行为与 legacy 不一致 |
| `excel.replace` 删行区间不再多删 | `DocumentReplaceService.cs:118` `int totalRows = options.ExcelEndRow - options.ExcelStartRow + 1;` → `:129` `cells.DeleteRows(start - 1, totalRows)` | 已关闭。旧写法把行数当区间端点传，末行多删一行 |
| `word.replace` 替换后字体样式（legacy `UseNewStyle`） | Schema `FeatureOptionSchemas.cs:566-570`「替换后字体样式」：`replaceUseNewStyle`/`replaceFontName`/`replaceFontSize`/`replaceBold`（位于 `word.replace` `:550-571`）；legacy 侧该开关原为死开关（`docs\feature-gap.md:187` 行记录「`Util.cs:5373` 形参 `UseNewStyle` 未被使用」） | 已关闭。迁移时按「真实可用」重做：键在面板可勾、消费端到达替换逻辑；**与 legacy 的差异（legacy 为死开关）已记入 `docs\feature-gap.md` §9，属有意改进** |
| `exam.package` 合并守卫与失败消息 | `ExamPackageService.cs:346-347` 注释「底层 ACE 的报错只讲现象（"操作必须使用一个可更新的查询。"）…这里补成可照做的中文消息」+ `:348-351` `$"合并失败：写入目标数据包 {targetRoot} 的 {BaseDatabaseName} 时出错（事务已回滚，目标库保持合并前状态，源数据包未改动）…"`（`:344` 先 `target.Rollback()`）；UI 守卫 `ExamPackageView.cs:158`「请先选择源数据包与目标数据包目录。」、`:178`/`:283`「目标目录下没有 Exam_Base.mdb。」、`:187` 空态「目标包里没有单位记录。」 | 已关闭。ACE 原始报错不再直接抛给用户，失败消息指明写哪一端、是否已回滚 |
| `exam.single` 内置测评人码图（001–999） | `ExamSingleView.cs:30` 勾选「使用内置测评人码图（001–999）」、`:37` `ToolTip`「内置码图起始序号，001–999；份数留空表示取到 999 为止。」、`:69` 区块名「内置码图」、`:77` `Hint`（含「legacy 默认就是内置码图」与文件名规则）；`tools\QaFunction\Program.Extended.cs:126` `RunExt("exam.single", CheckExamSingle)` | 已关闭。内置码图分支与外部图片分支都在，legacy 默认路径（不提供图片源）可走通 |

## 2. §4「经查证不缺」抽样复核（5 条，本轮重读）

| 抽样项 | 结论 | 本轮依据 |
| --- | --- | --- |
| 47 个功能全部注册、均有图标 | **仍成立** | `BuiltInFeatures.cs` 的 `All` 仍 47 项；`FeatureIcons.cs` 全 Id → `SymbolRegular`，未知 Id 走前缀推断；FeatureSmoke 19:59 打印 47/47/47 |
| 侧栏分组折叠 / 搜索强制展开 / 激活自动展开 | **仍成立** | `MainWindow.xaml.cs:103-155`（Expander 分组）、`:106`（搜索时强制展开）、`:185-217`（激活时展开并高亮） |
| 网格线 / 行高可配 | **仍成立** | `ConvertFeatureModule.cs:79-81`（`ShowGridLines`/`RowHeight`/`MinHeight`）、`BorderedFileList.cs:87-92`（`RowHeight` 夹取 18–64）、`:341`（行高 +8 的计算） |
| 合并顺序=列表顺序 + SaveFile + `outputFile` | **仍成立** | `ConvertFeatureModule.cs:160` 走 `RunMergeAsync`；`MergeDocumentOperation.cs:30-35` `outputFile` 优先、否则 `merged.pdf`/`merged.docx`，`:44` `Ok("合并完成。")` |
| 移除水印 / EXCEL→WORD / EPUB 两页只有 `saveSource` | **仍成立** | 四个 FeatureId 在 `FeatureOptionSchemas.cs` 仍只给 `Section("输出", Check("saveSource", …))`（走 `:595-598` 的 `_` 默认分支或 `:545-548` 显式同构分支）；legacy 移除水印页确无参数区（Designer `4828-4904`） |

## 3. 47 页 UI 一致性抽查

**源码不变量**：App 层 `SetBody(` 实调用 **21 处**（一个视图类服务多个 FeatureId，如 `GenericConvertView` 覆盖 26 个功能）。其中 **19 处**走 `BuildBottomActionLayout` / `BuildToolLayout`（操作条 Dock 到底部），2 处例外：`HomeFeatureModule.cs:90`（卡片网格 `ScrollViewer`，点击即导航）、`SystemFeatureModules.cs:471`（`system.tools` 三张卡各自内嵌按钮，legacy TabPage_26 同为分组卡片式）。两处都不构成「操作区不在底部」的违反。

**截图目视**（只描述看到的）：`word.pdf.png`（列表在上、两分区「输出与后续」「转换参数」、底栏「选择文件 … 开始处理」）、`pdf.watermark.add.png`（四分区「输出/水印内容/对齐与位置/平铺设置」，可见 字体颜色 #FF0000、水平/垂直对齐=居中、图片宽高 100、自定义位置 X/Y、平铺间距 25/25）、`code.datamatrix.png`（真列表 + 空态文案 + 底栏两钮）、`exam.circle.png`（单卡 + 预览 + 底部操作条，无假列表）、`system.network.png`（列表在上 + 「IP 设置」+ 底栏 5 钮）、`other.misc.png`（无假列表，四个「释放…」按钮 + 说明卡 + 底栏两钮）、`exam.package.png`（目录卡 + 单位表 + 底栏 4 钮）。

| 类别 | 结果 | 证据 |
| --- | --- | --- |
| 布局违反（操作区不在底部） | **0 条** | 19/21 视图底部操作条；两个例外见上 |
| 假空列表 / 假按钮 | **0 条** | `BorderedFileList` 仅 3 处实例化（`ConvertFeatureModule.cs`、`ReplaceFeatureModule.cs`、`DataMatrixFeatureModule.cs`），三页 legacy 本就是多文件列表页；空态是真控件 + 文案；所有按钮经 `ToolViewKit.Primary/Secondary(string, Action)` 或 `ExamViewKit` 绑定真实 handler |
| 文案含未实现字样 | **0 条构成问题** | 全仓 `未实现\|敬请期待\|暂不支持\|待实现\|TODO` 仅 `DocumentInfoService.cs:23`「暂不支持获取该文件信息」（类型兜底，非功能缺失）；`BorderedFileList` 的「当前页未接入文件信息查询。」在三个列表页均已接线后不可达 |

`_issues.csv` 的 Low 项只认截图能看见的部分：`system.network` **确认存在**（「VMware Network A…」「本地连接* 9」「Loopback Pseudo-I」及描述列被裁且无省略号）；`exam.package` 的 2px 溢出（98.0 vs 96.0）**目视未见截断**，按「未见」记录。

## 4. 残余风险（当前有效，共 9 条）

1. **`exam.*` 未在真实 VFSoft 库验证**：全部考评证据来自 `ExamFixture` 生成的合成库。本机 ACE 12.0 可用（01:00 闸门 508 例里只有 3 条 Word opt-in 被跳过，考评 Access 用例真实执行），但真实库的历史表结构差异（自增列/附加索引/长字段）未覆盖；`ExamPackageService.Reset`、`ReorderUnits` 同样只有合成库证据。无 ACE 的机器上 `ExamFixture` 直接 `return 2`（`tools\ExamFixture\Program.cs:99` 判定、`:102` 返回）、`[AceFact]` 用例随之跳过——他机「全绿」不代表考评写库路径被执行过。
2. **`system.*` 写操作与 UAC 需隔离机人工点测**：暂停更新天数 / 快捷方式小箭头 / 粘滞键 / 键盘锁定 / UAC 开关 / 静态 IP / DHCP 都会改真实系统；源码层有管理员校验（`WindowsSystemToolsService.cs:93` `IsUacEnabled`）与 WMI→netsh 回退，但没有在一次性隔离机上验证过「改完能回滚」。
3. **实体扫描仪未联调**：`twain.debug` 无设备可用，只能验证「无设备时的提示文案」。单文件发布下 TWAIN 身份已从 `TWIdentity.CreateFromAssembly` 改为 `TWIdentity.Create`（`TwainScanService.cs:215` 构造、`:219` 调用、`:238` 使用；`docs\packaging.md:182` 记录），修掉了 `Assembly.Location` 在单文件下恒为空导致的崩溃；真机扫描路径仍未走过。
4. **Aspose 评估模式限制**：本构建不带授权，口径只认 `docs\feature-gap.md` §9 的 blockquote（`docs\feature-gap.md:195`）。影响面：集合 >4 元素受限、转换产物带评估水印句、条码解码结果尾部也被追加水印文本（故 QR 用例只断前缀）。任何「产物字节级一致」类断言都不可复现。
5. **OCR 不迁**：成本未被接受（`docs\dev-parity-tools.md:160-163`：4–6 人日、随包体积 60–120 MB）；task-48 只钉住前置条件（模型与体积先定），不排期。功能面上 OCR 属「有意不迁」，不是待办。
6. **路径框只读规则的覆盖判定曾两处假阴性，现为硬断言**：规则①最初 `ContainsBrowseButton` 向上递归致假阳性泛滥、且用 `Inlines.Count == 0` 取不到 `TextBlock.Text` 致假阴性空转（`docs\qa-report-ux.md:32-33`），修后规则与自证（`--self-test` 20/20，`docs\qa-report-ux.md:26`）一起加固；但该规则判的是**渲染后视觉树**，不是源码级绑定，布局重构时仍需复查。其覆盖已进 `QaFunction` 扩展项 `ui.policy.pathbox.coverage`（`tools\QaFunction\Program.UiPolicy.cs:111`；同文件 `:110` 是 `ui.policy.pathbox`，`:77` 另列 `ui.policy.navrow`、`ui.policy.missing-path`）——**该 id 是 `tools\QaFunction` 的扩展项，对应 `qa-report-ux.md` 规则①；两套编号不同名，勿互搜**。
7. **TIF/GIF 自研编码器有体积税**：`TiffEncoder` 是 TIFF 6.0 单页 24bpp RGB + 条带 LZW（备选 PackBits/无压缩），`GifEncoder` 是 GIF89a 单帧 8bpp + 中位切分。A4 300dpi 实测（`docs\feature-gap.md:164`）：白页 LZW 0.17 MB（0.7%）、表格线+文字 0.38 MB（1.5%）、**彩色噪声渐变 25.55 MB（102.6%，比不压缩还大）**，编码 0.44/0.50/1.25 s。→ 扫描件/二值文档选 TIF 收益最大，彩色照片应选 JPEG；这是取舍不是缺陷，但「体积必须变小」类断言不成立。
8. **红绿必须同一时刻连跑（并发窗口）**：一条命令六门 `& .\tools\gate.ps1 -MaxSkips 3`（`tools\gate.ps1:34` 上限、`:28-33` 说明上限是白名单而非免检额度、`:218` 逐条打印 `跳过用例：{name}`、`:131` 检测 `DocumentTools`/`testhost` 占用、`:480` 打印 `总判定：ABORT（未进入任何门）`）；`-MaxSkips` 收紧到 0 在无 Word 机器上也不该红。**并发期单跑会瞬态变红**：本轮 G1 撞过 `ExamInsertView.cs` CS0103 `WeightOr`、`ExamVoteView.cs` CS0103 `ParseDouble`，UiSnapshot 曾报 `medium=1`，均在他人提交落盘后自愈。报红前先 `Select-String` 读断言源码，并确认 `dotnet test` 真重编过（`MSB3026` 文件被 testhost 锁定 = 刚才读的是旧 dll）。
9. **GUI 人工点测未做**：47 页「能建能排」有 FeatureSmoke、视觉树有 UiSnapshot、端到端有 QaFunction，但**没有真人逐页点测**（拖放、右键菜单、取消路径、粘贴路径、Tab 走查都没有人工回归）；点测脚本 `docs\qa-clickthrough.md` 已给出 17 段主线 / 全量 47 页的顺序与耗时估计，尚未执行。

## 5. 交接说明（tools）

| 工具 | 跑法 | 退出码 | 是否写仓库 |
| --- | --- | --- | --- |
| `tools\gate.ps1` | `& .\tools\gate.ps1 -MaxSkips 3`（PowerShell 5.1；日志目录可加 `-LogDirectory <dir>`） | 0 = 六门全过；非 0 = 某门失败或 ABORT（检测到 `DocumentTools`/`testhost` 占用时拒绝开跑，`:131` 检测、`:480` 打印 `总判定：ABORT（未进入任何门）`，属功能性正确）。**跳过 3 = 三条 Word opt-in 白名单**（脚本逐条打印 `跳过用例：{name}`，`tools\gate.ps1:218`）；无 Word 机器传 `-MaxSkips 0` 也不该红（`:28-33`）。定稿一跑 **PASS / 退出码 0 / 142 s**（2026-11-06 01:00:08–01:02:30） | 只写 `%TEMP%\dt-gate-<时间戳>.log`（摘要行；被它调起的 `QaFunction`/`UiSnapshot` 仍会各自写 `artifacts\`、`docs\qa-screens\`） |
| `tools\FeatureSmoke` | `dotnet run --project tools\FeatureSmoke -v q --nologo` | 0 = 全部建得出且排得下；1 = 有创建/布局/启动落点失败（`tools\FeatureSmoke\Program.cs:111`）。输出两行：「功能总数 47，创建成功 47，布局通过 47」（`:105`）与「启动落点检查 95 项，失败 0」（`:106`，判定是 `NavigationFallback`，注释 `:76`） | **不写文件**（真 DI 容器 + 真资源字典，只读） |
| `tools\UiSnapshot` | `dotnet run --project tools\UiSnapshot [--out <目录>] [--only <featureId>] [--self-test]` | 0 = 无致命错误（`tools\UiSnapshot\Program.cs:223` `return problems == 0 ? 0 : 1`），1 = 有；`--self-test` 走 `AuditSelfTest.Run()`（`:69` 参数、`:97` 调用） | **写仓库**：默认 `docs\qa-screens\`（`:40`），png + `_index.csv`/`_issues.csv`（`:182`/`:183`）+ 高 DPI 明细 `_hidpi.csv`（`:262`） |
| `tools\QaFunction` | `dotnet run --project tools\QaFunction` | **退出码 = 失败项数**（`tools\QaFunction\Program.cs:1056` `return requiredFail + extendedFail + bonusFail`，0 = 全通过） | **写仓库**：`artifacts\qa-function\`（`:60`），含 `result.json`（`:1086`）/`result.md`（`:1101`）与转换中间产物 |
| `tools\ExamFixture` | `dotnet run --project tools\ExamFixture -- [--output <目录>] [--no-seed] [--no-boolitem] [--probe-autoincrement] [--selfreport] [--verify <db>]` | 0 = 成功（`tools\ExamFixture\Program.cs:90`/`:93`）；1 = 用法错/生成失败（`:139`/`:173`）；**2 = ACE OLEDB 不可用**（`:99` 判定、`:102` 返回，退出码语义见 `:8` 注释） | **默认写仓库外**（`%TEMP%`）；显式 `--output` 才落仓库，且目录必须新建或为空 |
| `tools\ExportEmbeddedAssets` | `dotnet run --project tools\ExportEmbeddedAssets -- --resx <Resources.resx> [--out assets/embedded]` | 1 = 用法错（缺 `--resx`，`tools\ExportEmbeddedAssets\Program.cs:24-25`）；0 = 导出成功 | **写仓库**：默认 `assets\embedded\`（`:8` 默认值、`:30` 建目录） |
| **UX 审查报告**（任务产出，不是工具） | `docs\qa-report-ux.md`（144 行，已出稿）：把「输入控件政策」（`docs\feature-ux-spec.md` §4.9）逐页机械核对，判定材料来自 `tools\UiSnapshot` 的 `_issues.csv`/`_interact.csv` 与截图。跑法沿用 `UiSnapshot`（本行不新增命令） | 沿用 `UiSnapshot` 的 0/1 语义；报告本身没有退出码 | **写仓库**：报告落 `docs\qa-report-ux.md`，引用的截图仍在 `docs\qa-screens\`（**`_index.csv`/`_issues.csv` 只有全量运行有权改写**，子集运行只写 `partial-*.csv`） |

建议顺序：`ExamFixture`（造夹具）→ `QaFunction`（端到端）→ `UiSnapshot`（快照 + 高 DPI）→ `FeatureSmoke`（冒烟）→ `dotnet test`。`FeatureSmoke`/`UiSnapshot` 都会实例化真容器，可与单测并行，但**不要与 `dotnet build` 同时跑**（抢 `bin` 锁）。
