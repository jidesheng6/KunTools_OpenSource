# 文档真伪审计：`docs/feature-gap.md` 的 Done 承诺 vs 代码 vs 测试

> 目的：`feature-gap.md` 的 Done 格是对外承诺。本文件抽 20 行，逐行验证「承诺的能力在 Core 有真实消费点」「Schema 键真被读取」「测试能失败」。
> 结论口径：**一致** = 三件事都成立；**文档超前** = 文档写了但代码没有；**代码超前** = 代码有但文档没写；**无测试** = 代码成立但没有能失败的测试。
> 范围：只报告，不改 `feature-gap.md`（避免与 dev-image 抢文件）。所有结论都带 `文件:行号`，无行号的判断一律未写。
> 抽样时间：2026-11-05；抽样方式：对 20 行逐个 `grep` 键名 → 读消费点 → 读测试断言体（不只看测试名）。

## 1. 判定口径：三级测试

| 级别 | 含义 | 例子 |
|---|---|---|
| L1 声明 | 只断言 Schema 里键存在、`Kind`/`DefaultValue`/`Choices` 正确，不触发任何行为 | `FeatureOptionSchemaAddedKeysTests.cs:20-22`、`Task13OptionTests.cs:35-48` |
| L2 接线 | 用 Recording 假服务跑 Operation，断言「键 → DTO 字段」的映射语义 | `WordReplaceStyleTests.cs:144-160`（`"18磅"` → `18`、`" SimSun "` → `SimSun`） |
| L3 产物 | 读回产物内容/结构断言（DB 行、DOCX 的 `w:sz`、PNG 头、文件大小） | `VoteReportServiceTests.cs:47-57`、`PdfCompressWatermarkTests.cs:43-55` |

L1 不能替代 L2/L3：键名拼错、映射写反、默认值兜底失效，L1 全绿。

### 1.1 检索硬规则（本审计踩过坑，2026-11-05 加）

**grep 必须大小写不敏感，且对同一个能力同时搜三种形态**：① 类名/属性名（PascalCase，如 `ExcelRemoveRows`、`CaptionPosition`）；② Schema 键名（camelCase，如 `excelRemoveRows`、`captionPosition`）；③ 界面文案（中文，如「标题位置」「隐藏」「报表项目排序」）。

只搜 camelCase 会漏掉「测试直接用 DTO 属性构造请求」这一类——它们恰恰是最常见的产物级测试写法。本文件初版就因此把 `excel.replace` 判成「无测试」、把条码标题三键判成「无测试」，两条都是假阴性（见 §3 S4/S5）。**假阴性比漏报更浪费：它会让我们去新建一条本来已经存在的测试，再花时间发现重复。**

## 2. 抽样结果（20 行）

| gap.md 行 | 承诺（摘要） | 代码锚点 | Schema 键 | 测试锚点 | 判定 |
|---|---|---|---|---|---|
| 23 | `word.pdf` 7 参全接 + 服务层固定渲染设置；§9:181 声称 `useLocalOffice` 真接 Word COM | `OfficeConvertOperation.cs:93-103`（`:96` embedFonts、`:97` updateFields、`:98` useLocalOffice、`:99` compliance、`:100` colorMode、`:101` msWordVersion、`:102` password）；`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:30`（`WordToPdf`）→ `:41` 分流 `WordToPdfViaLocalOffice`（定义 `:118`）；`:58`/`:80` 固定渲染设置 | `FeatureOptionSchemas.cs:214-223`（saveSource/openPdfOptions/embedFonts/updateFields/useLocalOffice/compliance/colorMode/msWordVersion/password） | L2：`ConvertOptionWiringTests.cs:50`；L3：`WordLocalOfficeTests.cs:37-40`（**opt-in 默认跳过**，即 1 条 skipped） | 一致 |
| 24 | `pdf.word` OCR/项目符号/模式/密码；补 `Info.Author/Producer/Creator` | `OfficeConvertOperation.cs:106-112`（pdf.word 分支：`:108` useBullet、`:109` useOcr、`:110` mode、`:111` password）、`:160`（`Author = GetOption(options, "author", "DocumentTools")`） | `FeatureOptionSchemas.cs:312`（`author`）+ pdf.word 分支 | L2：`ConvertOptionWiringTests.cs:50`（`["author"]="李雷"`）；L3：`ConvertContentParityTests.cs` | 一致 |
| 26 | `ppt.pdf` 备注/隐藏页/黑框/密码；`exportNotes` 四值收窄为三值 | `OfficeConvertOperation.cs:117-125`（`:120` drawBlackLine、`:121` hiddenSlides、`:123` → `MapNotesPosition` 在 `:219` 读 `exportNotes`，默认 `None`） | `FeatureOptionSchemas.cs:263-268` | L1：`Task13OptionTests.cs:222-233`（断言 Choice 值集 + 不含 `RightFull`）；L2：`ConvertOptionWiringTests.cs`（5 hits） | 一致（`exportNotes` 无 L2/L3） |
| 35 | `pdf.split` 删除页用独立键、`refreshPageNumber` 只在 word.split 暴露 | `SplitDocumentOperation.cs:63`（`ParseBool(GetOption(request,"refreshPageNumber"))`）；pdf 分支不读该键 | `FeatureOptionSchemas.cs:642`（只在 `splitFields`/word 分支添加）；pdf/ppt 分支无 | L1：`Task13OptionTests.cs:22-27`（断言 pdf/ppt **没有**该键——反向断言，能失败） | 一致 |
| 36 | `pdf.watermark.add` 20 键全覆盖（含 fontColor/x-y/imageW-H/customPosition/spacing/underContent/useFileName） | `PdfWatermarkOperation.cs:66-128`（逐键读取：text `:66`、image/imageFile `:67`、useFileName `:68`、opacity `:71`、rotate `:77`、fontName `:82`、fontSize `:84`、fullScreen `:89`、repeatSpacingX/Y `:104-105`、horizontal/vertical `:115-116`、fontColor `:117`、imageWidth/Height `:120-121`、underContent `:125`、customPosition/x/y `:126-128`） | `FeatureOptionSchemas.cs:331-353`（19 键 + saveSource） | L1：`FeatureOptionSchemaAddedKeysTests.cs:66-83`；L3：`PdfCompressWatermarkTests.cs:12-13`（平铺水印几何与旧实现逐像素一致） | 一致 |
| 38 | `pdf.compress` quality/maxDpi/webOptimize/unembedFonts/flattenForms/flate/skipImages + imageVersion + 资源精简三开关 | `PdfCompressOperation.cs:34-45` 全部读取 | `FeatureOptionSchemas.cs:372-386` | L1：`FeatureOptionSchemaAddedKeysTests.cs:41-58`；L3：`PdfCompressWatermarkTests.cs:43-55`（产物不小于原文件即回退，断言文件大小） | 一致（BuildOptions 无 L2） |
| 47 | `image.compress` 格式/沿用后缀/不压缩/质量/颜色模式/PNG 级别与色彩类型/BMP 位数 | `ImageConvertOperation.cs:35`（format）、`:37`（multiSize）、`:84-93`（`:93` noCompress） | `FeatureOptionSchemas.cs:468-493` | L3：`Task13OptionTests.cs:140-141`（`noCompress` 逐字节复制源文件）、`ImageConversionServiceTests.cs:39`（`Convert_Png_HonoursZLibLevel`）、`:19-31`（`Convert_Png_WritesRequestedIhdrColorType` 断言文件偏移 12 处为 `IHDR` 与色彩类型）；L1：`FeatureOptionSchemaAddedKeysTests.cs:130-140`、`Task13OptionTests.cs:126-137` | 一致 |
| 49 | `image.ico` `multiSize` 一次输出 16/32/64/128/256 | `ImageConvertOperation.cs:37`（`ParseBool(GetOption(request,"multiSize","false"))`） | `FeatureOptionSchemas.cs:522` | L3：`ImageConversionServiceTests.cs` 的 `ToIco_MultiSize_WritesOneEntryPerSize` / `ImageConvertOperation_IcoMultiSize_UsesMultiSizeContainer` | 一致 |
| 57 | `word.replace` 匹配选项 + `ignoreFootnotes` + 替换后字体（`replaceUseNewStyle`/`replaceFontName`/`replaceFontSize`/`replaceBold`） | `ReplaceDocumentOperation.cs:36-56`（regex/matchCase/wholeWord/ignoreFootnotes/ignoreDeleted/ignoreFieldCodes/ignoreFields/ignoreInserted/ignoreShapes/regexSub/`replaceUseNewStyle` `:53`/`replaceFontName` `:54`/`replaceFontSize` `:55`/`replaceBold` `:56`） | `FeatureOptionSchemas.cs:553-570` | L2：`WordReplaceStyleTests.cs:144-160`（断言 DTO：Trim、「18磅」→18、Bold）；L1：`WordReplaceStyleTests.cs:227-243`；`ToolUiContractTests.cs:322-323`（regex/regexSub 接线） | 一致（无 L3） |
| 58 | `excel.replace` 无 `ignoreFootnotes`（避免假参数）；§9:183 声称修复删行 `totalRows = end - start + 1` | `ReplaceDocumentOperation.cs:49-51`（`ExcelRemoveRows`/`ExcelStartRow`/`ExcelEndRow` 均读取）；`src\DocumentTools.Core\Services\Office\DocumentReplaceService.cs:118`（`int totalRows = options.ExcelEndRow - options.ExcelStartRow + 1;`）、删行在 `:127-129`、清空分支 `:135` | `FeatureOptionSchemas.cs:576-582` | L3：`ToolUiContractTests.cs:426-451`（`ExcelReplace_RemoveRows_DeletesExactlyTheRequestedRange`：8 行源、删第 3~7 行 → `Assert.Equal(2, cells.MaxRow)` + `R1`/`R2`/`R8`，注释「旧实现把结束行当行数传，实际删 6 行只剩两行——本断言即那次缺陷的回归锁」）、`:453-474`（`ExcelReplace_ClearRange_KeepsRowsAndBlanksCells`：`Assert.Equal(5, cells.MaxRow)`）；反证用的 `ignoreFootnotes` 有 `Task13OptionTests.cs:245` | 一致 |
| 67 | `code.barcode` 外观 10 键 + `ReadTexts` 对齐 legacy | `src\DocumentTools.Core\Services\Code\BarcodeService.cs:32`（Symbology）、`:34`（ModuleSize）、`:36`（ResolutionDpi）、`:45` → `:87-114`（`ApplyCaption`：`:89` 取 Caption、`:90` 取 CaptionPosition、`:91` 字号兜底 12、`:100` 隐藏三态、`:108`/`:114` 上/下置）、`:51-53`（LogoFile）；映射层 `BarcodeFeatureModule.cs:338-365` | `FeatureOptionSchemas.cs:392-429` | L3：`Task13OptionTests.cs:51-60`（`Barcode_Rotate90_SwapsOutputDimensions` 读 PNG 尺寸）、`BarcodeServiceTests.cs`（ReadTexts 往返）；标题三键 L3：`ToolUiContractTests.cs:142-161`（`Barcode_CaptionAndLogo_ReachTheRenderedPixels`：`Caption`/`CaptionPosition="above"`/`CaptionFontSize=18` → 宽不变、有标题图**更高**）、`:205-223`（`Barcode_CaptionFontSize_ChangesRenderedCaptionHeight`：8 号 vs 28 号比高度） | 一致 |
| 69 | `name.vertical` 表格分支不并名（有意修正 legacy） | `src\DocumentTools.Core\Services\Text\NameVerticalService.cs:167`（`bool fromTable = source.Contains('\t')`）、`:173-179`（注释直接点名 `DocumentTools\Util.cs:399-402` 的并名缺陷 + 表格分支切分）、6 模式 `:10`、`:237-243` | `FeatureOptionSchemas.cs:525` 分支 | L3：`NameVerticalServiceTests.cs`（含 `TableBranch_SplitsOnTab_AndDropsNewlines`） | 一致 |
| 82 | `exam.mss3` 选票列表 / 灰度 BMP 写 `Vote_PageInfo` / `Item` 列转 Memo | `Mss3VoteService.cs:43`（ListVotes）、`:85-93`（`SELECT COUNT(*) FROM Vote_PageInfo WHERE VoteID=? AND PageIndex=?` 后接 `DELETE FROM Vote_PageInfo …`，即重复插入先删旧）、`:117`（`INSERT INTO Vote_PageInfo (VoteID,PageIndex,MatrixCode,PicContent)`） | 无 Schema（手写视图 + `IMss3VoteService` DTO） | L3：`Mss3VoteServiceAccessTests.cs`（`InsertVotePage_NewVote_...`、`..._ReplacesInsteadOfDuplicating`、`SetItemColumnsToMemo_AltersAllFiveItemTablesAndAcceptsLongText`） | 一致 |
| 83 | `exam.single` Alt 换图批量 + 命名后缀 | `SingleDocumentService.cs:14`（`altTexts`）、`:27-36`（按 `AlternativeText` 命中）、`:51`（`GenerateBatch`） | 无 Schema（`SingleDocumentRequest` DTO） | L3：`SingleDocumentNamingTests.cs`（`BuildSuffix_...`、`GenerateBatch_NamingModes_WriteDistinctFilesPerImage`） | 一致 |
| 84 | `exam.insert` 四表 + 5 条默认规则 + 事务回滚 + 批量模板 + 改号换位 | `ExamInsertService.cs:26`（Insert）、`:138`（隐式选项只落 `Exam_Score`）、`:199`（`INSERT INTO Exam_Ruler`）、`:207`（`MAX(ItemOrder)`）、`:278-285`（`Exam_BoolItem`）；`ExamIndexService.cs:81-87`（改号涉及表） | 无 Schema（`ExamInsertRequest` DTO） | L3：`ExamInsertServiceAccessTests.cs`（10 个 `[AceFact]`，含 `Insert_MissingBoolItemTable_RollsBackExamAndScores`）、`ExamInsertScoreWeightAccessTests.cs`、`ExamBatchInsertTemplatesTests.cs` | 一致 |
| 85 | `exam.report` `Content` 的 `Item=`/`Raid=`/`Kind=` 定点替换 + 网格 + 身份 | `ExamReportService.cs:431`（`ApplyDataFormat`）、`:528`（`ListReportItems`）、`:562`（`GetReportItemOrder`）、`:577`（`BuildClipboardText`） | 无 Schema（`ExamReportParaRequest` DTO） | L3：`ExamReportParaServiceTests.cs`（读回 `Report_Detail` 的 5 类 Kind 行与 `ReserveDec`/`Item`/`Raid` 串）、`ExamReportServiceAccessTests.cs` | 一致 |
| 86 | `exam.vote` `IVoteReportService` + 模板；字号/行高/生成日期落 DOCX | `VoteReportService.cs:45`（`%Report_Date%` → `ResolveGenerationDate`）、`:89-92`/`:99-101`（主表数据行行高与字号）、`:158-167`（另选人表） | `FeatureOptionSchemas.cs:587-592`（voteFontSize/voteRowHeight/voteGenDate）；三键在 `ExamVoteView.cs:64`/`:70`/`:76` 经 `SchemaDefault` 读取，提交映射 `:190` | L3：`VoteReportServiceTests.cs:56-57`（`Assert.Contains(28, sizes)` / `(18, sizes)`）、`:62-63`（`heights` 含 `ToTwips(1.2)`/`ToTwips(0.5)`）、`:88-89`（手填日期替换占位符）、`:114`（留空回退当天），读的都是产物 `word/document.xml`；死键护栏：`FeatureOptionSchemaReachabilityTests.cs`（`EveryDeclaredSchemaKey_HasAConsumerOutsideTheSchemaFile`） | 一致（S1 已修） |
| 87 | `exam.package` Inspect/Reset/ListUnits + 合并（跳过自增列、重名 `_1`） | `ExamPackageService.cs:68`（Inspect）、`:214`（Merge） | 无 Schema（`ExamPackageMergeRequest` DTO） | L3：`ExamPackageServiceAccessTests.cs`（12 个 `[AceFact]`，含 `Merge_TargetAutoIncrementColumn_IsNotCopiedFromSource`、`Merge_ExamFileWithSameName_IsCopiedAsRenamedCopy`） | 一致 |
| 88 | `exam.circle` NCC 匹配 + NMS + 替换图写回 + 旋转另存 | `ExamCircleFixService.cs:93`（GetImageSize）、`:99`（Match）、`:227`（Fix）；`ExamCircleImageService.cs:13`（Rotate） | 无 Schema（`ExamCircleFixRequest` DTO） | L3：`ExamCircleFixServiceTests.cs`（10 个用例：`Match_FindsEveryIdenticalPattern`、`Match_SearchRectLimitsResult`、`Match_MaxMatchesTruncates`、`Match_FlatTemplateReturnsEmpty`、`Fix_WritesFixedCopyAndKeepsSource`、`Fix_UsesManualMatchesAndReplacementImage`、`Fix_RejectsSamePath` 等） | 一致 |
| 100 | `system.settings` JSON、排除窗体大小/锚点、坏值兜底、目录校验、导入导出、原子写 | `src\DocumentTools.Core\Settings\AppSettingsService.cs:125`（`LastLoadWarnings`）、`:159`（`Save`）、`:167`（`Validate` → `AppSettingsValidator.Validate`）；`SettingsStore.cs:36`（`TempSuffix = ".tmp"`）、`:112`（`File.Move(tempPath, Location, overwrite: true)`）；`SettingsValidation.cs:26`（`SettingsValidationReport`，`Problems` 汇总） | 无 Schema（JSON 属性，`AppSettings.cs`） | L3：`AppSettingsTests.cs`（14 个 `[Fact]`：`BadValues_AreCoerced_AndRecordedInLastLoadWarnings`、`JsonFileStore_WritesAtomically_AndLeavesNoTempFile`、`Import_InvalidValues_ReportsPerField_AndKeepsCurrentSettings`、`Validation_ReportsDirectoryAndFileState_WithoutThrowing`） | 一致 |

统计：**一致 20 / 无测试 0 / 文档超前 0 / 代码超前 0**。审计查出的唯一真缺陷 S1（`voteGenDate` 死键）已在本轮修复并加护栏测试，见 §3 S1。

## 3. 5 类系统性偏差 + 1 条假阴性复盘

### S1 Schema 键声明了但无人读取（真·第二类假参数）——**本轮已修**

初版发现：`voteGenDate` 在 `FeatureOptionSchemas.cs:592` 声明，全仓仅此 1 处；`ExamVoteView` 的生成日期走 `PlaceholderText="留空=当天"` + `ExamViewKit.Value(_genDate)` 直接塞进 `VoteReportRequest.GenerationDate`。后果：Schema 是「参数权威」，但这一键既不是默认值来源、也没有消费点，谁按 Schema 渲染面板就会得到一个「填了不生效」的输入框。同一文件的两个兄弟键（`voteFontSize`/`voteRowHeight`）**有**消费点（`ExamVoteView.cs:64`/`:70` 用 `SchemaDefault`），所以不是整段脱离 Schema，而是单键漂移——最难发现的一种。

修复：
- `src\DocumentTools.App\Features\Modules\Exam\ExamVoteView.cs:73-79` 的 `_genDate` 改为 `Text = SchemaDefault("voteGenDate", string.Empty)`，与另两个输入同源；提交仍映射 `VoteReportRequest.GenerationDate`（`:190`），空串由 `VoteReportService.ResolveGenerationDate` 回退当天。
- 新增护栏 `tests\DocumentTools.Tests\FeatureOptionSchemaReachabilityTests.cs`：`EveryDeclaredSchemaKey_HasAConsumerOutsideTheSchemaFile` 解析 Schema 的 137 个键，逐个在 `src\**\*.cs`（排除 Schema 文件与 `bin`/`obj`）里找 `"键名"` 字面量（`OrdinalIgnoreCase`），任一键零命中即失败并在消息里列出孤儿键；另有 `ExamVoteSurfaceKeys_AreTheThreeLayoutInputs` 钉住 exam.vote 的三键与「留空」标签。
- **该护栏做过负向验证**：临时把 `_genDate` 改回 `Text = string.Empty`，测试立刻失败（`失败: 1，通过: 1`），改回后恢复通过——证明它真的会拦住「以后又有人把键声明了却不接线」。

### S2 存在大量「只测声明」的测试，容易把 L1 当行为护栏

`FeatureOptionSchemaAddedKeysTests` 全类只断言 `Kind`/`DefaultValue`/`Choices`（如 `:46-58`），`Task13OptionTests` 里同型断言占多数（`:35-48`、`:225-233`、`:239-245`）。
这批测试能挡住「键被误删/默认值被改」，但挡不住「键名拼错在 Operation 里没人读」「读到但映射写反」。

### S3 「键 → DTO」的映射层覆盖不均

有 L2 的：`word.replace`/`excel.replace`（`WordReplaceStyleTests.cs:144-160` 用 `RecordingReplaceService`）。
无 L2 的：`word.pdf`/`ppt.pdf`/`pdf.compress`/`image.compress`/`code.barcode` 的 `BuildOptions`（`OfficeConvertOperation.cs:94-102`、`PdfCompressOperation.cs:34-45`、`ImageConvertOperation.cs:35-93`、`BarcodeFeatureModule.cs:338-365`）——这些位置把键名写成字符串字面量，拼错只能靠人眼（`FeatureOptionSchemaReachabilityTests` 现在能挡住「整键无人读」，但挡不住「读错了位置」）。
建议：给 `OfficeConvertOperation`/`PdfCompressOperation` 补一个 Recording 假服务，断言 DTO 字段，与 `WordReplaceStyleTests` 同型。

### S4 假阴性复盘：`excel.replace` 删行**有**回归护栏（初版误判为「无测试」）

初版结论：§9:183 记「`totalRows = end - start + 1`」为已修，但 tests 中 `excelRemoveRows`/`excelStartRow`/`excelEndRow` 0 命中，判为「已修缺陷缺护栏」。

**错在哪**：只搜了 camelCase 的 Schema 键名；测试直接用 PascalCase 的 DTO 属性构请求（`DocumentReplaceOptions { ExcelRemoveRows = true, ExcelStartRow = 3, ExcelEndRow = 7 }`），所以零命中。按 §1.1 三种形态重搜，护栏存在且是 L3 产物级：
- `tests\DocumentTools.Tests\ToolUiContractTests.cs:426-451` `ExcelReplace_RemoveRows_DeletesExactlyTheRequestedRange`：8 行源、删第 3~7 行 → 打开产物 `Assert.Equal(2, cells.MaxRow)` + `R1`/`R2`/`R8`；注释原文「旧实现把结束行当行数传，实际删 6 行只剩两行——本断言即那次缺陷的回归锁」。
- `tests\DocumentTools.Tests\ToolUiContractTests.cs:453-474` `ExcelReplace_ClearRange_KeepsRowsAndBlanksCells`：`Assert.Equal(5, cells.MaxRow)` + 被清空区间。

判定改为**一致**（§2 第 58 行已更新）。

### S5 假阴性复盘：条码标题三键**有**产物级测试（初版误判为「无测试」）

初版结论：`caption`/`captionPosition`/`captionFontSize` 在 `src\DocumentTools.Core\Services\Code\BarcodeService.cs:87-114` 有实现，tests 0 命中。

**错在哪**：同 S4，只搜 camelCase 键名；测试用 `BarcodeRenderOptions { Caption = …, CaptionPosition = "above", CaptionFontSize = 18 }`（PascalCase 属性）。实际情况：
- `tests\DocumentTools.Tests\ToolUiContractTests.cs:142-161` `Barcode_CaptionAndLogo_ReachTheRenderedPixels`：解码 PNG 后断言宽不变、有标题图**更高**（标题占掉一行高度）。
- `tests\DocumentTools.Tests\ToolUiContractTests.cs:205-223` `Barcode_CaptionFontSize_ChangesRenderedCaptionHeight`：8 号 vs 28 号比高度，证明字号真进渲染。
- 未覆盖的维度只剩「`隐藏`/`none` 时与无标题图逐像素一致」与「上置 vs 下置的重心 y」——属可选加强，不是缺口。

判定改为**一致**（§2 第 67 行已更新）。

### S6 产物级测试分布倾斜：考评/图像密、转换类疏

L3 集中在 `Exam*Tests`（读 Access 行）与图像编码（读文件字节/PNG 头）；转换类主要靠 `ConvertContentParityTests`/`ConvertOptionWiringTests` 的 DTO 断言，唯一真产物级断言是 `WordLocalOfficeTests`（opt-in、默认 skipped）与 `PdfCompressWatermarkTests`（大小不增）。
这不是缺陷，但意味着「转换类回归」目前主要靠 Aspose 自身行为，而不是我们的断言。

## 4. 像素证据（`%TEMP%` 不可作为唯一证据）

task-33 的两张快照已落到仓库：

| 文件 | 说明 |
|---|---|
| `docs/qa-screens/exam.report@task33.png` | 46,181 B，1200×800。可见区是「数据库 / 报表列表 / 报表结构 / 内容替换」四张卡；task-33 新增的「批量参数」「报表项目」两卡在 `ExamViewKit.FormArea` 滚动区下方，**本图不显示**。本图证明的是页面整体渲染正常，不证明新卡视觉。 |
| `docs/qa-screens/exam.vote@task33.png` | 53,980 B，1200×800。**「报告单排版」卡片可见**：字号 `12`、数据行高 `0.8`、生成日期占位「留空=当天」，下方说明「字号（磅）与数据行高（厘米）同时作用于表头和表数据行；生成日期留空时用当天」。 |

新卡「不裁剪、不含空文本、无乱码、可键盘到达」由审计行给出（同一次渲染）：

```
PAGE|041|Exam|exam.report|ok|exam.report.png|45KB|1.00|el=495|btn=33|tab=27|input=yes|H0/M0/L0
PAGE|042|Exam|exam.vote|ok|exam.vote.png|52KB|1.00|el=273|btn=20|tab=15|input=yes|H0/M0/L0
```

`VisualAudit` 遍历整棵视觉树（滚动区内的元素按「可滚动到达」处理），因此 `H0/M0` 覆盖了屏幕外的两张新卡。

## 5. 复现方式

```powershell
# 1) 键是否有消费点（示例：voteGenDate —— 现在 ExamVoteView 有 SchemaDefault 消费点，不再是孤儿）
#    注意 -CaseSensitive:$false 与三种形态（键名 camelCase / DTO 属性 PascalCase / 中文文案）
Get-ChildItem src,tests -Recurse -Include *.cs | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
    Select-String -Pattern 'voteGenDate', 'GenerationDate', '生成日期' -CaseSensitive:$false

# 2) 死键全量护栏（137 个键零孤儿；把任一消费点删掉即失败）
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj --filter 'FullyQualifiedName~FeatureOptionSchemaReachabilityTests'

# 3) 是否只有 L1（示例：schema 测试全文只有 Kind/DefaultValue/Choices 断言）
Select-String -Path tests\DocumentTools.Tests\FeatureOptionSchemaAddedKeysTests.cs -Pattern 'Assert\.'

# 4) excel.replace 删行是否有测试（PascalCase 才是 DTO 属性名）
Select-String -Path tests\DocumentTools.Tests\*.cs -Pattern 'ExcelRemoveRows', 'ExcelStartRow', 'ExcelEndRow' -CaseSensitive:$false

# 5) 条码标题是否有产物级测试（同上，搜 PascalCase）
Select-String -Path tests\DocumentTools.Tests\*.cs -Pattern 'Caption', 'CaptionPosition', 'CaptionFontSize' -CaseSensitive:$false

# 6) 重新出这两张像素证据
dotnet run --project tools\UiSnapshot -v q --nologo -- --out $env:TEMP\qa33 --only 'exam.report,exam.vote' --no-suite --no-interact
```
