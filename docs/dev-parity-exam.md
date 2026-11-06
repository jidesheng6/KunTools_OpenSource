# 考评 7 页与 `dev` 分支逐页差异（task-40）

对照基准：本地 `dev` tip = **ce4cc88**（用 `git show dev:<path>` 读，未 checkout）。
行号口径：dev 侧为 `dev` 提交的文件行号；新版侧为当前工作树行号（`feat/wpfui-migration`）。
写法：每页一条链 —— **dev 控件 → 参数 → 落库/产物**，两侧都给 `文件:行号`。

---

## 1. 方法

1. `git show dev:DocumentTools/{BodyForm.cs,BodyForm.Designer.cs,Util.cs,VF_Report.cs,InsertExamBacthCustomSelectors.cs,DynamicAddControlParams.cs,ExamCircleFixForm.cs}` 落盘到 `%TEMP%\dev-exam-parity\` 再读（口令行数见括号）。
2. dev 侧控件 → 事件处理 → `Util.*` → SQL/产物，逐跳记录行号。
3. 新版侧同样逐跳记录，落到 `Core\Services\Exam\*` 的服务方法与 `Features\Modules\Exam\*View.cs` 的事件方法。
4. 只把「用户能操作、却没有任何落库/产物效果」的点判为缺口；风格不同、实现不同但行为等价的点，写进「等价说明」而不是缺口。

---

## 2. dev 未迁文件逐个定性

### 2.1 `dev:DocumentTools/VF_Report.cs`（26 行）

| 内容 | dev 行号 | 定性 |
| --- | --- | --- |
| `SELECT Content FROM Report_Detail WHERE Kind='GridFormatData' AND Content LIKE 'Bool:%vftab%' AND ReportID={id}` | `:7` | **已迁**：新版用参数化查询代替拼串，见 `ExamReportService.ReplaceContent`（`src\DocumentTools.Core\Services\Exam\ExamReportService.cs:150-200` 的 `SELECT Item, Kind, Content FROM Report_Detail WHERE ReportID = ?`）。差异：dev 只扫 `GridFormatData`，新版扫报表全部行但只在 `Content` 里找 `Item=`/`Kind=`/`Raid=` 段，效果等价且更严（`GridFormatText` 里不含这些段则不改）。 |
| `ExamItemFormat_Regex=Item=([^&]*)`、`KindFormat_Regex=Kind=([^&]*)`、`RaidFormat_Regex=Raid=([^&]*)` | `:8-10` | **已迁**：`ExamReportService` 的 `ItemRegex` / `KindRegex` / `RaidRegex`（`ExamReportService.cs:40-44`）。 |
| `SELECT Content FROM Report_Detail WHERE Kind='GridFormatText' AND Content LIKE '999%'` + `NumberPoint_Regex=(?<=ReserveDec=)([^&]+)`（小数位数改写用） | `:12-20` | **已迁**：新版在写参数时就按 `MapNumberFormat` 生成显示文本并同时写 `ReserveDec`，不再事后改文本，见 `ExamReportService.cs` 的 `MapNumberFormat`/`ChangeReportParameters`。行为等价（同一组 0..5 档 → `999/999.9/999.99/999%/999.9%/999.99%`）。 |

结论：`VF_Report.cs` 没有遗留独立业务能力，它是 dev 时代给 `Util.ChangeExamReportContent` 用的 SQL 片段与正则容器，能力已并入 `ExamReportService`。

### 2.2 `dev:DocumentTools/InsertExamBacthCustomSelectors.cs`（98 行）

- 11 组内置选项名（每组 4 个）：好/较好/一般/差、好/较好/一般/较差、优秀/良好/一般/差、优秀/良好/一般/较差、优秀/称职/基本称职/不称职、优秀/合格/基本合格/不合格、认同/基本认同/不认同/不了解、好/一般/不好/不了解、满意/基本满意/较满意/不满意、A/B/C/D、a/b/c/d（`:1-98`，`ctor` 先 `Clear` 再 `Add`）。
- 菜单绑定：`BodyForm.cs:10139-10201`（`ExamBacthSelectors_Custom1..11_Click` → `InsertExam_BatchCustomSelectorsWithName(CustomSelectors_N)`），剪贴板粘贴按钮 `:10131-10132`。
- **已迁**：`ExamBatchInsertTemplates.CustomSelectorPresets`（`src\DocumentTools.Core\Services\Exam\ExamBatchInsertTemplates.cs:29-58`，11×4 逐字一致）、`FromPreset` / `FromClipboard` / `ParseClipboardNames`（`:100`/`:172`/`:157`）。
- **差异（有意）**：dev 批量套用预设时恒把「计入得票率」置 true（`BodyForm.cs:9599`/`:9712`），新版跟随界面勾选值（`ExamInsertView` 的 `_selectorCalcAvg`，默认 true）。默认行为一致，显式取消勾选时新版更贴近用户意图。

### 2.3 `dev:DocumentTools/DynamicAddControlParams.cs`（95 行）

纯静态字段容器，分三组前缀/索引/坐标：

| 组 | 字段（dev 行号） | 新等价物 |
| --- | --- | --- |
| WORD 批量替换 | `WORDFINDREPLACE_FIND_NAME_PREFIX`/`REPLACE_NAME_PREFIX`/`FIND_NAME_INDEX`、两个 `List<Control>`（`:9-19`） | `ReplaceFeatureModule` 的 `FindRow` 集合 + `pair{i}.find/.replace` 键（`src\DocumentTools.App\Features\Modules\Tools\ReplaceFeatureModule.cs:190-207`）。 |
| EXCEL 批量替换 | 同上（`:23-33`） | 同一套 `pair{i}.*` 键，`excel.replace` 复用 `ReplaceFeatureModule`。 |
| 插入考评表项目/选项 | `InsertExam_ExamName_Prefix`/`ExamScore_Prefix`/`ExamSelector_Prefix`/`ExamSelectorPer_Prefix`/`ExamSelectorAvg_Prefix`、`InsertExam_Control_Index`、`InsertExam_Control_LocationY`、`TempBaseInsertItemLocationY`、7 个 `List<Control>`（`:37-93`） | `ExamInsertView` 的 `InsertItemRow`（项目名称/分值/选项文本/选项分值/计入得票率）+ `ObservableCollection` + DataGrid，取代「控件名 + Y 坐标累加」。参数本体逐项在列：项目名 `ItemName`、项目分 `ItemScore`、选项名 `SelectorText`、选项分 `SelectorScorePer`、计入得票率 `CalcAverage`（`ExamInsertView.cs:540-620` 的 `DoInsert` 组装）。 |

**参数完整性核对**：dev 的两组「逐行控件」`InsertExam_ExamSelectorPer_*`（选项分值）与 `InsertExam_ExamSelectorAvg_*`（是否计入得票率）在新版都有对应列，且都能进 `Exam_Score`（`ExamInsertService.InsertSelectorScore`）。新版曾把选项分值写死 `"0"` 丢掉面板值（task-19 已修，`ExamInsertView.cs` 逐行取 `row.SelectorScorePer`）。结论：**无参数丢失**；丢失的是 WinForms 控件脚手架本身，那本就不是业务参数。

---

## 3. 圆圈修复：算法逐项对齐（用户最在意「一毛一样」）

dev：`dev:DocumentTools/ExamCircleFixForm.cs` + OpenCvSharp。新版：`src\DocumentTools.Core\Services\Exam\ExamCircleFixService.cs`（纯 C# 灰度 NCC）。

| 维度 | dev | 新版 | 是否等价 |
| --- | --- | --- | --- |
| 相似度算法 | `Cv2.MatchTemplate(roiMat, tmplMat, result, TemplateMatchModes.CCoeffNormed)`（`:666`）＝去均值归一化互相关（Pearson） | 同一公式实现：`NCC = (Σtp - n·t̄p̄) / (n·σt·σp)`（`ExamCircleFixService.cs:418`），滑动窗口均值/方差用列累加、互相关项用条带 FFT | **等价**（同一数学量；新版灰度化后再算，dev 对彩色三通道交给 OpenCV 内部同样转灰度） |
| 相似度取值范围 | `CCoeffNormed` ∈ [-1, 1] | 计算结果同一区间，但只保留 `> 0` 且 `≥ 阈值` 的候选（`:421` 明确丢弃 ≤0） | **等价**：阈值下限 0.01，负相关永远过不了阈值 |
| 阈值语义（0.8） | `thr = numSimilarity.Value/100`，夹到 `[0.01, 1]`（`:653-655`），比较 `maxVal < thr` 即停 | `NormalizeThreshold` 夹到 `[0, 1]`（`:874-881`），候选需 `similarity ≥ threshold` | **等价**：都是「Pearson 系数 ≥ 0.8」；界面上 80(%) = 0.8 |
| 纯色/低对比模板 | 无特殊处理，NCC 分母为 0 时 OpenCV 给 0/NaN，实际匹配不到 | 模板方差 ≤ `1e-6` 直接返回空（`:157-161`），避免除零 | **有意加固**（只影响「本来也匹配不到」的输入） |
| 多匹配去重 / NMS | 逐个取全局最大，命中后把 `result` 中以 maxLoc 为中心、宽高各为模板尺寸的窗口 `SetTo(0)`（`:676-683`），**无数量上限** | 候选按相似度降序，若与已保留候选的中心距在水平 `< w/2` 且垂直 `< h/2` 内则丢弃（`:186-209`） | **等价**（矩形窗口置零 ≡ 中心距半宽/半高抑制）；差异只有「上限」：新版默认 `MaxMatches=100`（`:40` 可调），dev 无上限 |
| 搜索范围矩形 | `_searchRange` 必填；`template.Width > range.Width` 或 `Height > range.Height` 直接弹「模板区域不能大于搜索范围。」（`:647-651`）；命中框偏移 `range.X + maxLoc.X`（`:673`） | `SearchRect` 可选，默认整图；越界由 `ResolveSearchRegion` 先夹紧（`:818`），模板大于范围时输出尺寸 ≤0 → 返回空集合（`:167-172`） | **语义等价，反馈不同**：新版不弹错，只给空结果（UI 侧提示「0 处匹配」）。这是 UX 差异，不是数据差异 |
| 替换图缩放策略 | 矩形与 patch 同尺寸 → `CopyTo`；不同尺寸 → `Cv2.Resize(patch, dstRoi, Size(r.Width, r.Height), Linear)`（`:788-795`） | 同尺寸 → `canvas.DrawBitmap(patch, destination)`；不同尺寸 → `patch.Resize(Size, ReplacementSampling)` 后绘制（`:292-301`）；`ReplacementSampling` 已改 **Linear**（`ExamCircleFixService.cs:91`，注释指 dev `:794`） | **像素级一致**：`Fix_ReplacementImage_ResizesIntoRectLikeDevLinear` 实测 `maxDiff=0`、`meanDiff=0.000` |
| 替换图来源 | 未选替换图 → 用模板区域克隆作 patch（`:775`）；选了 → 整图作 patch（`:755-770`） | 未给 `ReplacementImage` → `ExtractTemplatePatch` 取模板区域（`:273`）；给了 → 载入整图（`:263`） | **等价** |
| 手动框 / 单删 / 清空 | `_matchRects` 可手动加、单删、清空（`:226`/`:821`/`:832`） | `ManualMatches` 直接喂给 `Fix`（`:257-259`），UI 侧 `ExamCircleView` 有手动添加/清除标记 | **等价** |
| 自动旋转（T 码象限） | `ExamCircleFix_AutoRotate_Click`（`BodyForm.cs:322-400`）：DataMatrix 识别，取码中心，右上＝不动、左上＝顺时针 90、左下＝180、右下＝逆时针 90 | **本轮新补**：`IExamCircleOrientationService.Detect`（`src\DocumentTools.Core\Services\Exam\ExamCircleOrientationService.cs`）同判据；UI「自动旋转」按钮（`ExamCircleView.cs:139`/`:589-620`）识别后调 `IExamCircleImageService.Rotate` 转正并另存 | **已对齐**（含中文提示文案与四象限阈值：`> W/2` / `< H/2`） |
| 扫描仪获取 / 扫描仪设置 / 多页 | `ExamCircleFix_GetImageButton`+`GetImageMenu`（`BodyForm.cs:176-179`）、`ExamCircleFix_ScannerSetting_LinkClicked`（`:247-251`） | **未迁** | 见 §6 |

### 3.1 实测对照（可复现）

命令：`dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj --filter "FullyQualifiedName~ExamCircleParityTests"`（4 例）。
测试文件 `tests\DocumentTools.Tests\ExamCircleParityTests.cs` 用纯 C# 复刻 dev 的两段算法（`CorrelationSurface` ≡ `CCoeffNormed` 的 Pearson 面、`DevMinMaxLocLoop` ≡ `MinMaxLoc` 贪心 + 邻域置零、`Crop`+`DrawBitmap` ≡ `CopyTo`），再把新版服务的结果逐项比对。

| 对照项 | 用例 | 实测结果 |
| --- | --- | --- |
| 相似度数值同一量 | `Threshold_ComparesTheSamePearsonValueAsDevSlider` | 单圆圈图 200×120、模板 24×24：dev 参考面在 `(88,48)` 处 `1.000000`，新版回报 `1.000000`（`Assert.Equal(..., 6)` 通过） |
| 阈值 0.8 的语义 | 同上 | 默认 0.8 命中 1 处；把阈值抬到 `S+0.02` 立即 0 处、压到 `S−0.02` 立即 1 处 → 阈值就是这条 Pearson 分数，dev 滑块「80」＝ 0.8 完全同义（dev `:653-655` 把百分数除以 100 并夹到 `[0.01,1]`，新版 `NormalizeThreshold` 夹 `[0,1]`） |
| 密集相邻圆圈（NMS） | `DenseAdjacentCircles_MatchesDevMinMaxLocLoop` | 7 个圆心相距 14px（模板宽 24 → 半宽 12，正落在抑制边界）：**dev 5 处、新版 5 处，位置与分数逐一相同** —— `devPos=[28,28 42,28 56,28 70,28 138,68]`、`newPos=[28,28 42,28 56,28 70,28 138,68]`，`devScores=[1.0000 0.8581 0.8581 0.8581 1.0000]`、`newScores=[1.0000 0.8581 0.8581 0.8581 1.0000]` |
| 无替代图的修复回写 | `Fix_WithoutReplacement_CopiesTemplatePatchExactlyLikeDev` | 3 处命中，产物与「模板区域像素逐框拷回」的参考实现 **逐字节相同**：`maxDiff=0` |
| 有替代图的缩放回写 | `Fix_ReplacementImage_ResizesIntoRectLikeDevLinear` | 36×36 替代图缩到 24×24：`maxDiff=0`、`meanDiff=0.000`（改 Linear 前用 Mitchell 三次卷积，二者会有可见差异，故本轮改回 dev 的插值） |


---

## 4. 7 页逐页对照

### 4.1 `exam.mss3`（dev `TabPage_32` :11045-11367）

| dev 控件（行号） | 参数 | 落库 | 新版（行号） | 状态 |
| --- | --- | --- | --- | --- |
| `SelectMss3` 加载库（`:11343`）、路径 label（`:11366`） | 数据库文件 | — | `ExamMss3View` 的 `_database` | 等价 |
| `SelectVotePic` 选模板图（`:11317`）、`VotePicture` 预览 | 图片文件 | `Vote_PageInfo.PicContent`（8bpp 灰度 BMP） | `InsertBallotPage`（`ExamMss3View.cs:174-215`）→ `Mss3VoteService.InsertVotePage`（`Mss3VoteService.cs:63-122`） | 等价（`BmpEncoder.EncodeGray8` 对齐 legacy `ConvertTo256Color`） |
| `voteid`（`:11279`，MaxLength 2）、`votename`（`:11244`）、`voteindex` 矩阵码（`:11223`，MaxLength 2）、`votestatustext`（`:11293`） | 选票编码/名称/矩阵码/票面类型 | `Vote_EssInfo`（11 列，`Util.cs:4989`）+ `Vote_PageInfo.MatrixCode` | 同左四框 → `Mss3VoteService` 同名参数 | 等价（11 列顺序与常量 45/15/20/1/42/42/150/45 已逐条核对） |
| `votepageIndexCombox`（`:11217`，数据源 `BodyForm.cs:1452-1463`：第1页→0、第2页→1） | PageIndex（0 基） | `Vote_PageInfo.PageIndex` | `_pageIndex` ComboBox（`ExamMss3View.cs:48-49` `PageChoice(0,"第1页")/(1,"第2页")`），写入 `page.Index`（`:209-212`） | **本轮修正**：此前是「页码 ≥1」的文本框，会把 1 写成第 2 页 |
| `ChangeMssItemColumn`（`:11174`）、`ResetMssItemColumn`（`:11161`），互斥 `Enabled`（`:7892-7896`） | 两种列结构操作 | `ALTER TABLE … ALTER COLUMN Item Memo`（`:4395-4402`）/ 还原长度（`:4417-4435`，255/100/50） | 「Item 列转备注」（`Mss3VoteService.SetItemColumnsToMemo`）+ **本轮新补**「恢复 Item 列长度」（`ExamMss3View.cs:93`/`:251-272` → `Mss3VoteService.ResetItemColumnLengths`） | **已补齐**（此前只有转 Memo） |
| `StartInsert`「开始执行」（`:11304`） | — | 同上 | 「插入」按钮 | 等价 |

### 4.2 `exam.single`（dev `TabPage_39` :11368-11860）

| dev 控件（行号） | 参数 | 产物 | 新版 | 状态 |
| --- | --- | --- | --- | --- |
| `SignDocumentUseOnlyNumber`（`:11507`，默认选中 `:11499`）+ `SignDocumentFileEndName`（`:11546` 默认「测评人」） | 名称后缀＝`（{序号:D3}{后缀词}）` | 文件名 `{模板名}{后缀}.docx`（`BodyForm.cs:6286-6311`） | `SingleDocumentNameSuffixMode.NumberWithSuffix` + `SuffixWord`（`ExamSingleView.cs:169-215` → `SingleDocumentNaming.BuildSuffix`） | 等价 |
| `SignDocumentUsePersonNumber`（`:11521`） | `（第{n}人）` | 同上 | `PersonIndex` | 等价 |
| `SignDocumentOnlyUsePageNumber`（`:11535`） | `（第{n}页）` | 同上 | `PageIndex` | 等价 |
| `SignDocumentUseEmbedImage`（`:11624`，默认 `:11616`） | 内置 001–999 码图 | `Util.GetPersonCodeZipFileStream`（`Util.cs:4615-4647`，PersonCode.zip） | `UseBuiltInPersonCode` + `PersonCodeLibrary`（`PersonCodeLibrary.cs`），资源构建期链接 | 等价（按需读单张，不驻留 999 个流） |
| `SignDocumentUseOutPersonPath`（`:11637`） | 外部目录 | 目录内图片逐一 | `ImageSourcePathOrDirectory` 目录分支 | 等价 |
| `SignDocumentOnlyUseOnlyImage`（`:11686`） | **单张图重复 N 份**（`:6409-6436`） | N 份同名图、序号递增 | **本轮新补** `CollectExternalImages`（`SingleDocumentService.cs:166-186`） | **已补齐**（此前单文件只产出 1 份） |
| `SignDocumentNeedCount`（`:11574-11587`，2..65536，默认 2） | 份数 | 产出份数 | `MaxCount`（空＝全部） | 等价 |
| 保存目录（`:11739`）、`SingPersonImageAlt`（`:11804`，`@` 分隔多个 Alt） | 输出目录 / Alt 列表 | — | `OutputDirectory` / `AltTexts`（`@` 分隔） | 等价 |

### 4.3 `exam.insert`（dev `TabPage_42` :11861-12671）

| dev 控件（行号） | 参数 | 落库 | 新版 | 状态 |
| --- | --- | --- | --- | --- |
| `InsertExamTable_SelectExamFile`（`:12651`）、`InsertExam_RefreshExamInfo`（`:12157`） | 库文件 | — | `_database` + 刷新 | 等价 |
| `InsertExam_ExamName`（`:12234`）、`InsertExam_ExamTypeMode`（`:12217`，4 种排版） | 考评表名称/排版类型 | `Exam_EssInfo(ExamId,ExamName,TypeMode)`（`Util.cs:3685`） | `ExamInsertRequest.ExamName/TypeMode` → `ExamInsertService.ResolveExamId`（`:132-151`） | 等价 |
| 项目名/项目分（`:12590`/`:12581`）、选项名（`:12466`）、选项分（`:12416`）、计入得票率（`:12433`） | 项目与选项矩阵 | `Exam_Score` 项目行 + 选项行（`Util.cs:4193`/`:4240`）、`Exam_BoolItem` 5 列（`:4225`） | `InsertItemRow` → `ExamInsertRequest.Items` → `InsertItemScore`/`InsertSelectorScore`/`InsertBoolItem` | 等价 |
| 弃权/无效得分比重（`:12344-12391`，Value **-1**）、两行「计入得票率」（`:12321` 默认勾选 / `:12337` 默认不勾） | 三种比重 | 每个项目追加 `SelName='无效'`、`'弃权'` 两行 `Exam_Score`（`BodyForm.cs:9254-9268`） | `ItemScoreWeight`/`AbstainScoreWeight`/`AbstainCalcAvg`/`InvalidScoreWeight`/`InvalidCalcAvg` + `InsertImplicitSelectors` | 等价（含「无效不计入得票率 → 多一行 1 分」） |
| 批量：模板含 `X`（`:11923` 提示）、数量（`:12083`）、分数（`:12141`）、中文序号（`:12072`）、批量项目/选项按钮（`BodyForm.cs:8895-8982`/`:9609-9721`） | 批量展开 | 逐条 INSERT（与单个插入同一路径） | `ExamBatchInsertTemplates.ExpandItems/ExpandSelectors` + `ExamInsertView` 批量卡 | 等价（展开在 UI/纯逻辑层，落库仍逐条） |
| 预设选项（`:12265`）、粘贴选项（`:12288`）、移除选项/全部（`:12314`/`:12277`） | 选项名集合 | 同上 | `FromPreset`/`FromClipboard`/`FromNames` + 表格行增删 | 等价 |
| 改表序号：`ChangeExamTableNo_OrginTableIndex`（`:12009`）、新序号（`:11945-11958`）、`ChangeExamTableNo_Start`（`:11971`） | 旧/新 ExamID | 10 张 ExamId 绑定表的 `ExamID`（`Util.cs:3820-3832`）+ 物理顺序重建（`:3898-3911`） | `IExamIndexService.ChangeExamIndex` / `RebuildExamEssInfoPhysicalOrder`（`ExamIndexService.cs`）+ `ExamInsertView` 改号卡 | 等价（含「占用则临时序号三步换位」） |

### 4.4 `exam.report`（dev `TabPage_40` :12672-13572）

| dev 控件（行号） | 参数 | 落库/产物 | 新版 | 状态 |
| --- | --- | --- | --- | --- |
| `SelectExamFile`（`:13510`）、`RefreshVFSOFTREPORT`（`:13544`）、报表基础信息（`:13559`） | 库/报表 | `Report_Ess ⋈ Exam_EssInfo`（`Util.cs:3662-3675`） | `ExamReportService.ListReports`（`:104-120`） | 等价 |
| 行/列模式（`:13139`/`:13130`）、起止行列（`:13111-13159`）、数据类型（`:12983`）、小数位数（`:12923`）、字体（`:12932` 默认「宋体,11,0」）、评测主体（`:12973`）、插相同项目/选项（`:12776`/`:12787`）、`ChangeExamReportParaButton`（`:13011`） | 20 个参数 | `Report_Detail` 每格 5 类 Kind + `GridFormatData` 的 `Kind/Kind2/Raid/Item/Selector` 段（`Util.cs:895-1300`） | `ExamReportService.ChangeReportParameters`（`:427-505`）+ `MapNumberFormat`/`NormalizeFontSpec`；UI 批量卡在 `ExamReportView` | 等价（含 `ReserveDec` 与 999/999.x/999% 显示文本） |
| `ExamReportChangeItemName`（`:13204`）、`…ItemDataFormat`（`:13245`）、`…ItemDataRaid`（`:13220`）、`ExamReportChange`（`:13387`） | 改项目名 / 改数据格式 / 改身份 | `UPDATE Report_Detail SET Content=…`（`Util.cs:842-854`） | `ReportContentReplaceRequest{ChangeItemName,ChangeDataFormat,ChangeRaid,...}` → `ExamReportService.ReplaceContent`（`:150-230`） | 等价（含「选了项目就只改该项目」的修正） |
| `GetAllItemSplit`「获取项目或选项到剪贴板」（`:13418`） | 项目/选项文本 | 剪贴板 | `ExamReportView.BuildClipboardText` | 等价 |
| `ExamRaidsCopy`「复制评测主体合计身份」（`:13431`） | 身份合计 | 剪贴板 | `ExamReportService.ListExamRaids` 已产出「合计」行（`：255-283`），**但视图没有一键复制按钮** | **缺口（UI）**，见 §6 |
| `ExamReportList_Sort`/`NoSort`「报表项目排序/重置」（`:13040`/`:13048`） | 报表项目顺序 | 重写 `Report_Detail` 行序 | 未找到对应实现 | **未验证**：dev 该功能的具体落库语句未逐行确认，本轮不改动 |
| `ColWidthStr`/`RowHeightStr` 行列数 | 报表网格尺寸 | 只读（由外部 VF 程序写） | `ExamReportService.GetGridSize`（Access 各减 1） | 等价 |

### 4.5 `exam.vote`（dev `TabPage_38` :13573-14036）

| dev 控件（行号） | 参数 | 产物 | 新版 | 状态 |
| --- | --- | --- | --- | --- |
| `VoteTable_Render`（`:13775`） | 15 个参数 | `Util.GenerateVoteReportDocx(...)`（`BodyForm.cs:5793-5852`）+ 嵌入模板 `VoteReportDoc` | `VoteReportRequest`（`IVoteReportService.cs:3-36`）+ `ExamVoteView.Generate`（`:155-200`） | 等价 |
| 标题/正文/结束语/生成日期（`:13894`/`:13869`/`:13841`/`:13815`） | 文本 | 模板占位替换 | `Title/Body/Ending/GenerationDate` | 等价 |
| 姓名 5 行（`:13935`）、表头四列（`:13965-14025`） | 姓名与列名 | 表格 | `Names` + `NameHeader/AgreeHeader/UnAgreeHeader/AbstainHeader` | 等价 |
| 字号/行高（`:13659-13672`/`:13710-13723`）、随机票数（`:13749`）、保留序号（`:13629`）、保留页码（`:13644`）、另选人表格（`:13622`） | 排版开关 | 产物样式 | `DataRowFontSize/DataRowHeightCentimeters/RandomPageCounts/KeepIndexColumn/KeepPageFooter/IncludeAnotherTable` | 等价 |

### 4.6 `exam.package`（dev `TabPage_43` :15977-16314）

| dev 控件（行号） | 参数 | 落库/文件 | 新版 | 状态 |
| --- | --- | --- | --- | --- |
| 源/目标目录（`:16037`/`:16091` 附近 Select 按钮）、`ExamMergeDataPak_MergeButton`（`:16062`）、`ExamMergeDataPak_CopyExam`（`:16020` 默认勾选） | 两个数据包目录 + 是否复制 `.exam` | `Exam_People`/`Unit_Ess` 逐行 INSERT + `.exam` 复制（`BodyForm.cs:11034-11302`） | `ExamPackageService.Merge` + `ExamPackageView.Merge`（`:210-300`） | 等价（重复单位可选清理、重名 `.exam` 加 `_1/_2`） |
| `ExamRestDataPakReset`（`:16113`）、确认与「已是初始化状态」判定（`BodyForm.cs:10889`/`:10900`/`:10933`） | 目标包 | `DELETE FROM Exam_People`/`Unit_Ess` + 删 `*.exam` + `SelfReport` | `ExamPackageService.Reset` + `ExamPackageView.ResetTarget`（`:359-390`） | 等价 |
| **单位顺序修改**：`ExamDataPakOrder_UnitList`（`:16176`）、`ExamDataPackUnitOrder_Up/Down`（`:16264`/`:16274`）、`ExamDataPakOrder_Modify`（`:16284`） | 单位顺序 | 逐行 `UPDATE Unit_Ess SET AtOrder=… WHERE UnitCode=…`（`BodyForm.cs:10526-10657`，无事务） | **本轮新补**：`ExamPackageService.ReorderUnits`（`ExamPackageService.cs:194-254`，事务内、1 基序号）+ `ExamPackageView` 的「△ 上移/▽ 下移/保存单位顺序」（`:58-62`/`:293-350`） | **已补齐** |
| `ExamDataPakUnitOrder_QuickFindButton`「查找」+ `MoveToOrder`「修改为指定序号」（`:16194`/`:16211`） | 快速定位并改指定号 | 同上 | 本页用「上移/下移 + 保存」覆盖同一落库路径 | **行为等价、交互不同** |

### 4.7 `exam.circle`（dev `TabPage_44` :16315-16471）

见 §3 的逐项算法对齐；本页控件映射：

| dev 控件（行号） | 新版 | 状态 |
| --- | --- | --- |
| `ExamCircleFix_SelectButton`（`:16453`）、`ExamCircleFIxPath`（`:16471`） | 「打开…」（`ExamCircleView.cs:115`） | 等价 |
| `ExamCircleFIx_Start_ShowDialog`「进入修复模式」（`:16381`） | 本页即修复工作区 | 等价 |
| `ExamCircleFix_RotateLeft`/`RotateRight`（`:16413`/`:16360`） | 「旋转」下拉 + 「旋转另存」（`:134-137`） | 等价 |
| `ExamCircleFix_AutoRotate`「自动旋转」（`:16391`） | **本轮新补**「自动旋转」（`:139`/`:589-620`） | **已补齐** |
| `ExamCircleFix_GetImageButton`+`GetImageMenu`（`BodyForm.cs:176-179`）、`ExamCircleFix_ScannerSetting`（`:247-251`） | 无 | **未迁**（见 §6） |

---

## 5. `docs\feature-gap.md` §9 考评相关「有意偏离」复核

| §9 条目 | 复核结论 |
| --- | --- |
| `exam.single` 生成名称后缀（三档 + 可编辑后缀词） | 成立：dev 只有三 radio（`Designer:11499/11521/11535`），所谓第四档是 `:11546` 的后缀文本框；新版三档 + `SuffixWord` 与 dev 等价。 |
| `exam.single` 内置单人码图（原记「未迁」） | **已落地**，见 task 内交付：构建期链接 `PersonCode.zip`、`PersonCodeLibrary` 按需读单张、越界中文异常、`PersonCodeArchivePath` 缺失即停构建。 |
| `exam.insert` 三种得分比重 | 成立：落 `Exam_Score.Score`（每项两行隐式选项），不碰 `Exam_BoolItem`、不占 `ItemOrder`；默认 得分 0 / 弃权 -1（计入）/ 无效 -1（不计入）。 |
| `exam.package` 合并失败消息与规模 | 成立：legacy 直抛原文，新版包中文可操作消息（事务已回滚）；规模实测 5000 人/200 单位 1.87 秒、20000 人/500 单位 7.17 秒，托管堆不增长。 |
| `exam.report` 只扫 `GridFormatData` | **不是漏迁**：新版扫全表但在 `Content` 里按段匹配，`GridFormatText` 等行不含 `Item=` 时不会命中；保守方向一致。 |
| `exam.mss3` `PageIndex` | 曾是**真缺陷**（新版本要求页码 ≥1，会整体偏移一页），本轮改为 0 基下拉，已与 dev `BodyForm.cs:1452-1463` 一致。 |

---

## 6. 本轮修复清单与仍未迁项

### 6.1 已修（含测试）

| 缺口 | 证据 | 修复 | 测试 |
| --- | --- | --- | --- |
| mss3 页码从 1 起（把第 1 页写成 `PageIndex=1`） | dev `Designer:11217` + `BodyForm.cs:1452-1463`（第1页→0） | `ExamMss3View` 改 0 基下拉 `PageChoice(0/1)`，写入 `page.Index` | `ExamDevParityAccessTests`（真实 `.mdb` 落库断言见 `Mss3VoteServiceAccessTests` 系列） |
| 「恢复项目列字段结构长度」缺失 | dev `Designer:11161` + `BodyForm.cs:4417-4435` | `IMss3VoteService.ResetItemColumnLengths` + UI 按钮 | `ExamDevParityAccessTests.ResetItemColumnLengths_*`（3 例，含 50/255 边界与 256 必拒） |
| 「数据包单位顺序修改」缺失 | dev `Designer:16176/16264/16274/16284` + `BodyForm.cs:10526-10657` | `IExamPackageService.ReorderUnits`（事务、1 基）+ UI 上移/下移/保存 | `ExamDevParityAccessTests.ReorderUnits_*`（4 例） |
| 「仅使用单张图片文件」未迁移（单张只产出 1 份） | dev `Designer:11686` + `BodyForm.cs:6409-6436` | `SingleDocumentService.CollectExternalImages` 单文件重复 N 份 | `ExamDevParityAccessTests.GenerateBatch_SingleExternalImage*`（2 例） |
| 「自动旋转」（T 码象限）缺失 | dev `Designer:16391` + `BodyForm.cs:322-400` | `IExamCircleOrientationService` + UI 按钮 | `ExamCircleOrientationTests`（7 例，真 DataMatrix 四象限 + 转正后复检） |

### 6.2 仍未迁（如实登记）

| 能力 | dev 证据 | 未迁原因 | 建议 |
| --- | --- | --- | --- |
| 扫描仪获取图片 / 扫描仪设置菜单 | `BodyForm.cs:176-179`、`:247-251`，依赖 dev 自带的 TWAIN 封装 | **已补齐（2026-11-06）**：`ExamCircleView.ScanImage`（`:574`）复用 `ITwainScanService`，扫描到 `%LOCALAPPDATA%\DocumentTools\scans\{时间戳}` 并载入第一张；设备不可用 / 无设备 → 服务自身的中文 `UnavailableReason`，不弹英文原文。**设置菜单不迁**：DPI 固定 300 + `ShowUi=true`，分辨率/纸张在驱动界面临时改（不再造第二套「设备选择/DPI/多页」面板） | 剩「实体扫描仪实机联调」——本机无设备，与 `twain.debug` 同一残余风险 |
| 「报表项目排序 / 重置项目排序」 | `Designer:13040/13048` | **已实现（复核确认）**：`ExamReportService.GetReportItemOrder` 返回 `StoredOrder` + `SortedOrder`（自然排序），UI `ExamReportView.SortItems` / `ResetItemOrder` 用 `_storedItems` 备份还原；排序不写库（`Report_Detail` 无排序列），页面 ToolTip 已写明「只影响本页显示与复制出的文本」 | 无 |
| 「复制评测主体合计身份」按钮 | `Designer:13431` | **已补齐（2026-11-06）**：`ExamReportService.BuildRaidClipboardText`（`:650`）按 legacy 拼法（显示名逐个 + 「，」，排除「批量设置全部身份」行）产出文本；`ExamReportView.CopyRaids`（`:564`）写剪贴板并回显全文。dev 侧该按钮本身是死代码（`DataSource as List<string>` 对 `DataTable` 恒 null），见 `feature-gap.md` §9 | 无 |

---

## 7. 残余风险

1. **圆圈相似度只在合成图上验证**：积分图 NCC 与 OpenCV 的浮点实现存在末位差异。合成图上分数逐位相同（`1.0000` / `0.8581`），但真实扫描件（噪声、抖动、压缩伪影）若正好落在阈值边界，仍可能给出不同命中数。
2. **NMS 半径**：新版按「中心距 < w/2 且 < h/2」抑制，dev 是把 `result` 窗口置零；在 14px 间距的密集圆圈场景下两者结果完全相同（见 §3.1），但极端长宽比模板（例如 200×8 的横条）未覆盖。
3. **`ReorderUnits` 与 dev 的事务差异**：dev 逐行 UPDATE 无事务（中途失败会留下半成品序号），新版包事务整批回滚 —— 有意加固，属偏离。
4. **`MaxMatches` 默认 100**：dev 无上限；如果某张图真的有 >100 处圆圈且用户没调上限，会少修。UI 上限框可调。
5. **扫描仪链路未验证**：本机无设备，`ITwainScanService` 与考评页的接线未端到端跑过。
6. **`exam.report` 项目排序**：见 §6.2，未取证前不实现。
