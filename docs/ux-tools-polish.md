# 工具 / 转换页 UX 打磨清单（task-43）

范围：`src\DocumentTools.App\Features\Modules\Tools\`、`Features\Modules\Convert\`、`Ui\Controls\`。
每条给「控件/页面 → 改法 → 落地或否决理由」；末尾给前后快照与移交项。

## 0. 结论摘要

| 项 | 状态 | 落点 |
| --- | --- | --- |
| A 路径类输入只读 + 选择/拖放 | 已落地（Tools 全部 4 处） | `ToolViewKit.PathBox`（`Ui\Controls\ToolViewKit.cs:341`） |
| B 宽度与排版（数字框 / 双列 MinWidth） | 已落地；`<70px` 的 5 条 Medium 属 Core Schema，已移交 | `Ui\Controls\ToolViewKit.cs:367`、`NameVerticalFeatureModule.cs:142-143` |
| C.1 空列表提示 + 空态按钮 | 已落地 4 个页面 | `Ui\Controls\BorderedFileList.cs:104/509` |
| C.2 双击行打开 | 已落地 | `BorderedFileList.cs:101/552` |
| C.3 行级成功/失败/进行中着色 + 失败原因 ToolTip | 已落地 | `BorderedFileList.cs:627/188/181` |
| C.4 长任务可取消 | 已落地（Convert、DataMatrix） | `ConvertFeatureModule.cs:392`、`DataMatrixFeatureModule.cs:252` |
| C.5 失败清单复制到剪贴板 | 已落地（Convert） | `ConvertFeatureModule.cs:328` |
| C.6 记住上次目录 | **否决并移交**（需 Core 加字段 + 对话框加起始目录参数） | 见 §3.6 |
| 追加：键盘工具卡 / UAC 卡 / 延迟框 | 审查通过（他人页面，未改） | 见 §4 |

快照前后（全量 47 页）：

| 时点 | fatal | High | Medium | Low |
| --- | --- | --- | --- | --- |
| 改前 | 0 | 14 | 39 | 76 |
| 本次收尾实测（全量 47 页） | 0 | 2 | 28 | 6 |

剩下的 2 条 High 是 `exam.circle` 的「路径框可编辑」，归 `circle-fix-dev`；28 条 Medium 里属于本任务页面的只有 `code.barcode` 1 条（`width: 60` 的 Schema 根因，见 §2 移交），其余分布在 `pdf.watermark.add`(8)、`html.pdf`(6)、`image.pdf`(4)、`exam.report`(3)、`excel.pdf`(3)、`word.split`/`pdf.split`/`ppt.split`(各 1) —— 都由各自页面 owner 收口，本任务不重复验。

> 中间过程数字：先落到 `14 / 32 / 59`（工具被他人改动期间），再修 `word.replace`/`excel.replace` 空态（两页各 `M2→M0`，实测 `--only` 结果），最后由各 owner 修掉 `word.epub`/`pdf.watermark.add`/`html.pdf`/`exam.*` 的路径框与空白项，收敛到 `2 / 28 / 6`。

## 1. A：路径类输入一律只读 + 「选择…」/ 拖放写入

| 页面 | 控件 | 改前 | 改后 |
| --- | --- | --- | --- |
| `code.barcode` | `_logoPath`（LOGO 图片路径） | 可编辑 `TextBox`，`MinWidth=200`，可手打路径 | `ToolViewKit.PathBox("LOGO 图片路径", …)`：只读、可聚焦复制、可直接拖图片进来 |
| `code.barcode` | `_readPath`（要识别的条码图片路径） | 同上 | 同上 |
| `twain.debug` | `_path`（扫描图片保存目录） | 可编辑 `TextBox` | `ToolViewKit.PathBox(…, folder: true)`，拖入文件夹即写入 |
| `other.extra` | `_watchPath`（监视目录） | 可编辑 `TextBox` | 同上 |

改动落点：`BarcodeFeatureModule.cs:48-49`、`MiscToolModules.cs`（Twain `_path`、Extra `_watchPath`）。
`PathBox` 不自己发明语义，直接复用 ui-shell 的 `Ui\PathInputs.cs`（`ForFile`/`ForFolder` 已封 `IsReadOnly`、`IsTabStop`、`MinWidth = UiTokens.PathBoxMinWidth`、拖放），只额外补三件事：`AutomationProperties.Name/HelpText`、`TextChanged` 同步的 ToolTip（空 → 选择提示；失效 → `MissingFileNotice`/`MissingFolderNotice` 同句式；有效 → 全路径）。

配套：`BarcodeFeatureModule.cs:286` 识别前图片不存在时，状态行从「请先选择图片」改成 `PathInputs.MissingFileNotice(path)` 的可执行文案（红态）。

否决：**不**把路径框做成「可编辑但带校验」——同一政策两套写法会让用户以为是普通输入框，且和 Exam 侧已统一过的只读框不一致。

## 2. B：宽度与排版

| 位置 | 改法 | 理由 |
| --- | --- | --- |
| 数字输入（金额、数量、页码） | 统一 `ToolViewKit.NumberBox`：`Width ∈ [70,110]`（`UiTokens.NumberBoxMinWidth/MaxWidth`）、右对齐 | 窄框右对齐才能按位对齐读数；宽度不写死，避免高 DPI 下被切 |
| `other.extra` 金额框 | `MinWidth=70 / Width=110`、右对齐、占位「如 1234.56」 | 原为普通左对齐宽框，数字对齐看起来像文本 |
| `other.extra` 延迟框 | 宽 90、右对齐，单位写进标签「延迟(ms)」 | 单位进标签，字段内只留数字，方便直接改 |
| `name.vertical` 双列 | 两列 `MinWidth = UiTokens.PathBoxMinWidth`（`NameVerticalFeatureModule.cs:142-143`） | 窗口变窄时原等分列会把源框压成一条缝 |
| `ToolViewKit.Field` 标签列 | 固定 `labelWidth` + `GridLength.Auto`，内容默认 `Stretch`，`stretchContent:false` 时左对齐 | 同一页标签左边界对齐；尾部按钮靠右不抢宽度 |

未落地（移交）：`code.barcode` 1 条、`word.split` 1 条、`excel.pdf` 3 条 Medium「可编辑输入框宽度 60px < 70px」。根因不在页面：宽度由 Core `FeatureOptionSchemas` 的 `Text(..., width: 60)` 决定，`Ui\Options\OptionPanelBuilder.cs:228` 原样照抄。**修法应在这两处**（不在本任务写入范围）：要么把 60 提到 ≥70，要么在 `OptionPanelBuilder` 里统一 `Math.Max(width, UiTokens.NumberBoxMinWidth)`。

## 3. C：六条交互增强

### 3.1 空列表提示 + 空态「选择文件」
- `BorderedFileList` 新增 `EmptyHint` / `EmptyActionText` / `EmptyAction`（`BorderedFileList.cs:104`），空态覆盖层 `BuildEmptyOverlay()`（`:509`）。
- 文案（都含「还没有」，与审计口径一致）：`convert` 「还没有文件：点下面的「选择文件」，或把文件 / 文件夹拖到这里。」；`word.replace`/`excel.replace` 「还没有文件：点下面的「选择文件」，或把 Word / Excel 文档拖到这里。」；`code.datamatrix` 「还没有文件：点「选择文件」，或把 Word / PDF 拖到这里。」
- 空态里的「选择文件」按钮是底部主操作的副本，显式 `IsTabStop = false`：否则 Tab 序列会先落到它、再跳回底部，形成「顺序倒退」。

### 3.2 双击行打开
`BorderedFileList.cs:101` 新增 `ItemActivated`；`:552` `OnListMouseDoubleClick` 的兜底规则：有自定义动作则调用，否则打开源文件，源文件不存在则打开其所在目录。理由：列表最常见诉求就是「看一眼这个文件」，但删/改必须仍走右键菜单，避免误触破坏性操作。

### 3.3 行级状态着色 + 失败原因
`FileRowState { None, Pending, Running, Success, Failed }`（`BorderedFileList.cs:627`）、`MarkState(path, state, reason)`（`:188`）、`FailedItems`（`:181`）。
`ApplyListChrome()` 里用四个 `DataTrigger` 绑 `FileListItem.State`：Failed → `ToolViewKit.ErrorTextBrush`、Success → `SuccessTextBrush`、Running/Pending → `BusyTextBrush`，同时把 `ToolTip` 绑到 `Reason`（异常原文/失败原因），不塞进状态行。
配色修正：原先直接用 `Brushes.Tomato`（2.82:1）与 `Brushes.ForestGreen`（4.21:1）低于 4.5:1 对比度，改成 `ToolViewKit.ToneBrush`（`ToolViewKit.cs:179`）背后的 `ErrorTextBrush #C62828` / `SuccessTextBrush #1B5E20` / `BusyTextBrush #1565C0`，色相不变、过 4.5:1。

### 3.4 长任务可取消
- `Convert`：`CancellationTokenSource` + 状态栏右侧「取消」按钮，`RunPerFileAsync` / `RunBatchAsync` / `RunMergeAsync` 三处把令牌传给 `ExecuteAsync`；取消后文案「已取消：成功 X 个，失败 Y 个，剩余 Z 个未处理。」，合并没有完成时补「输出目录里可能留有不完整的文件。」，且取消时不再弹「是否打开输出目录」。
- `DataMatrix`：原 `RunDecode` 是同步阻塞循环（UI 冻死、点不到取消），改成 `async Task RunDecodeAsync`，逐文件 `await Task.Run(() => _decoder.ReadDocument(path), token)`，状态行给「正在解码第 i 个 / 共 N 个：文件名（逐页渲染，文件较大时较慢）」，「取消」按钮仅在运行期可见；取消后保留已解析的树。

### 3.5 失败清单复制到剪贴板
`ConvertFeatureModule.cs:328` `CopyFailures()`：把 `FailedItems` 拼成 `名称 \t 路径 \t 原因` 拷到剪贴板（可直接贴进 Excel/工单），成功态给条数，剪贴板被占用时给「复制失败：剪贴板正被其他程序占用，请稍后重试。」并把异常原文落按钮 ToolTip。按钮只在有失败项时出现。

### 3.6 「记住上次目录 / 常用参数」——否决并移交
理由（核实过）：`src\DocumentTools.Core\Settings\AppSettings.cs` 是固定字段模型（`ListViewShowGridLines`、`ListViewLineHeight`、`StartFeatureId` 等），没有「上次目录」；`IDialogService.OpenFiles/SaveFile/PickFolder` 也没有起始目录参数。要落地必须同时改 Core 与 `Infrastructure\Dialogs\`，两处都超本任务范围。
当前折中：`BorderedFileList.LastOutputDirectory` 只服务「打开转换后目录 / 打开转换后文件」，不冒充「记住上次目录」。

## 4. 追加三项（本轮新增能力，审查结论）

| 页面/能力 | 现状 | UX 审查结论 |
| --- | --- | --- |
| `system.tools`「键盘工具」卡（`SystemFeatureModules.cs:925-961`） | 「锁定键盘」用 `ControlAppearance.Caution`、「解锁键盘」Secondary；锁定时状态行变红并写明「按键已被拦截，请用鼠标点「解锁键盘」」；卡片下有 `Caption` 交代 Ctrl+Alt+Del 例外、离开本页/退出程序自动解锁 | 符合危险操作三要素：动作说明、可见后果、可逆入口。「锁定」用 Caution 而非 Primary 是对的——本页没有唯一主操作。人工冒烟已写进 `docs\qa-clickthrough.md` §10（键盘锁必须真机，且测完确认已解锁） |
| `about`「管理员批准模式（UAC）」卡（`AboutFeatureModule.cs:77-80`） | `InfoRow("当前状态", _uacStatus)` 只读状态行；切换按钮 `ControlAppearance.Caution`；切换前 `Confirm`，确认文案含「修改需要管理员身份，且重启电脑后生效」；禁用态用 Firebrick 提示「已禁用（不弹 UAC 提示）」 | 只读/危险操作提示达标：状态可读、后果写清、需重启写明、失败走 `Error("无法读取管理员批准模式：…")`。与「键盘工具」同一套政策（Caution + 说明 + 确认框） |
| `other.extra` 延迟框 | 宽 90、右对齐、单位进标签「延迟(ms)」；默认 `NewFileWatchFilter.DefaultDelayMilliseconds = 500`，范围 100–6553600（解析在 `ParseDelay`） | 宽度在 70–110 区间内，右对齐达标。另补事件列表空态提示「还没有监视事件：点「开始监视」后，新建的 jpg/tif/bmp 会出现在上面的列表里。」，消掉该页唯一一条「缺空态提示」 |

## 5. 验收

| 门槛 | 结果 |
| --- | --- |
| `dotnet build DocumentTools.sln --no-incremental` | `已成功生成。 0 个警告 0 个错误`（重写 `MiscToolModules.cs` 后复跑确认） |
| `dotnet test tests\DocumentTools.Tests` | `失败: 0，通过: 505，已跳过: 3，总计: 508` |
| `dotnet run --project tools\FeatureSmoke` | `功能总数 47，创建成功 47，布局通过 47` + `启动落点检查 95 项，失败 0` |
| `dotnet run --project tools\UiSnapshot`（全量） | `SUMMARY pages=47 rendered=47 fatal=0 high=2 medium=28 low=6 oversized=0`；`SUITE\|hidpi=16\|interact=4\|interactOk=4\|interactHang=0`；`docs\qa-screens\_index.csv` 保持 48 行 |
| 单页快照 | `--only twain.debug,other.misc,other.extra` → `pages=3 … high=0 medium=0 low=1`（唯一 Low 是 `other.misc` 的「无输入控件」，该页只有释放按钮，属预期）；`--only word.replace,excel.replace` → `pages=2 … high=0 medium=0 low=0` |
| 乱码自检 | 占位符三连问号 0 命中、GBK 二次编码样例串 0 命中、`MiscToolModules.cs` 的 `U+FFFD` 计数 0 |

> 过程中 `tools\UiSnapshot` 曾被他人改动导致编不过（`AuditSelfTest.cs(435,52): CS1061 PropertyMetadata.Inherits`），本文数字取自它恢复后的一次完整运行。

## 6. 残余风险 / 移交

1. **`<70px` 输入框 Medium**：本任务页面只剩 `code.barcode` 1 条（另有 `word.split`/`pdf.split`/`ppt.split`/`excel.pdf` 同类 4 条在转换页）。根因在 Core `FeatureOptionSchemas` 的 `width: 60` 与 `Ui\Options\OptionPanelBuilder.cs:228` 原样照抄，不在本任务写入范围 → 移交 ui-shell / Core（两处任一改掉即可整批消失）。
2. **2 条 High「路径框可编辑」**：`exam.circle`，归 `circle-fix-dev`；改成 `PathInputs` 只读框后 `ui.policy.pathbox.coverage` 才能整体翻绿。
3. **C.6 记住上次目录**：需 `AppSettings` 加字段 + `IDialogService` 加起始目录参数 → 移交。
4. **DataMatrix 取消延迟**：`Task.Run` 只能在外层取消，`ReadDocument` 内部逐页渲染不可中断，因此「取消」在单文件解码期间要等这一页跑完才生效（状态行已写明「文件较大时较慢」）。
5. **空态覆盖层与列表同格叠加**：改用 `_host` 网格 + `ZIndex` 后，空态按钮不进 Tab 序列；若以后有人给 `BorderedFileList` 加第二个覆盖层，需要重新排 `ZIndex`。

## 7. 事故记录（编码）

本轮我一度用 `Set-Content -Encoding utf8` 改写 `MiscToolModules.cs`（含中文），PowerShell 5.1 以 GBK 读入导致「合法 UTF-8 + 内容乱码」，全解决方案编译红约半小时。已按 `write` 整文件重写修复，`U+FFFD` 归零。
**规则固化：含中文的 `.cs` 只用 `write`/`edit`，禁止 `Set-Content` / `Out-File` / `>` / `[IO.File]::WriteAllText`。** 同类事故本项目已发生两次（另一次是 `OfficeConvertService.cs`）。
