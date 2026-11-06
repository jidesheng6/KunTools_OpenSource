# DocumentTools WPF 迁移 — 功能完成度与乱码治理

> 对照：`docs/feature-ux-spec.md`、`BuiltInFeatures`、`BodyForm` Tab。  
> 更新：2026-11-05（考评 7 页全部接 Core 真实现：MSS3/插入/报表/数据包含合并/圆圈修复）

## 状态图例

| 标记 | 含义 |
|------|------|
| Done | UI + Core 主路径可用 |
| Partial | 有 UI，参数或业务未全接到 Aspose/系统 API |
| Shell | 仅有布局/占位按钮，点按只弹提示 |
| Missing | 未注册菜单或无模块 |

---

## 1. 转换类（`GenericConvertView` + `IDocumentOperation`）

列表右键已对齐 `ListViewRightMenu`（含转换后路径记忆 + `IDocumentInfoService`）。

| FeatureId | UI 底栏布局 | Options→Core | 备注 |
|-----------|-------------|--------------|------|
| word.pdf | Done | Done | 7 个参数全接；另按 legacy `Util.cs:2589-2739` 补齐服务层固定渲染设置：`PdfFontEmbeddingMode.EmbedAll`/`PdfPageMode.UseOutlines`/`PdfZoomBehavior.None`/`UseHighQualityRendering`/`UseAntiAliasing`/`PreblendImages`/`JpegQuality=100`/`AllowEmbeddingPostScriptFonts`/`ExportGeneratorName=false`/`HeaderFooterBookmarksExportMode.All`/`OutlineOptions`(0/9/3)/`RemovePersonalInformation`/`BalanceSingleByteDoubleByteWidth`/`LoadOptions.LanguagePreferences=ChinesePRC`。`ImageColorSpaceExportMode` 该版本 `PdfSaveOptions` 无此属性（legacy 设 `Auto`，默认即 `Auto`，无行为差异）；`useLocalOffice` 走晚绑定 Word COM（`WordToPdfViaLocalOffice`，见第 4 节），未装 Word 时降级为中文可操作提示并在 `logs\conversion.log` 写引擎归因（`引擎=Word.Application` + 源/输出/耗时），便于事后区分「Word 导出的 PDF」与「Aspose 导出的 PDF」 |
| pdf.word | Done | Done | OCR/项目符号/Mode/密码；另按 legacy `Util.cs:2130-2134` 补 `Info.Author/Producer/Creator="DocumentTools"` 与 `Document.Decrypt()` |
| excel.pdf | Done | Done | 工作表/适应一页/居中/缩放；另按 legacy `Util.cs:1583-1589` 设 `PdfSaveOptions.OnePagePerSheet`/`AllColumnsInOnePagePerSheet`/`CalculateFormula`，逐工作表导出改用 legacy 的「可见性切换」（`Util.cs:1618-1628`）而非复制工作表，避免丢 `PageSetup` |
| ppt.pdf | Done | Done | 备注页 NotesPositions + 隐藏页 + 黑框 + 密码；与 `Util.cs:2170-2189` 逐项一致，无缺口；`exportNotes` 由布尔升级为四值 Choice（`None`/`BottomFull`/`BottomTruncated`/`RightFull`，映射 `PptxSaveOptions.NotesPositions`），`"true"`/`"false"` 旧值仍兼容 |
| image.pdf | Done | Done | `onePdfPerImage` 关 → 多图合并一个 PDF；另接 legacy `Util.cs:1805-1899` 的 `compress`（`ImageCompressionOptions` + Flate）/`autoOptimize`（`Optimize()`）/`orientation`（0=按图片像素定页、1=横向、2=纵向）/四 `margin*`；每图一页改用 `Paragraph.Image`（与 legacy 同语义），不再用 `ImageStamp` |
| pdf.excel | Done | Done | `oneSheet` 键已对齐 |
| pdf.ppt | Done | Done | OCR/拆分形状/按图片三参全接 `PptxSaveOptions`（`ExtractOcrSublayerOnly`/`SeparateImages`/`SlidesAsImages`） |
| word.excel / excel.word | Done | Done | 与 legacy `Util.ConvertWord2Excel`/`ConvertExcel2Word` 逐项对齐：`SectionMode` 选项 + 固定 `DocxSaveOptions`，无缺参 |
| word.epub / epub.* | Done | Done | word.epub：作者/封面图片/封面DPI 已接（DPI → `ImageSaveOptions.Resolution`，自定义封面读原图字节）；epub.word/pdf 与 legacy 同参数（`AdjustToWidestPage`，PDF 另 `JpegQuality=100`） |
| word.image | Done | Done | extension/dpi/`gray`→ColorMode.Grayscale；`pageExport` 关时按 legacy「合并一页导出」语义纵向拼接。**实现路径与 legacy 不同、结果等价**：legacy 走 `SaveFormat.Svg` + Aspose.Imaging `SvgRasterizationOptions` 光栅化（`Util.cs:2505-2548`），本项目禁加 Aspose.Imaging 包，改为 word→PDF→`PngDevice(Resolution=dpi)` 逐页渲染后用 `System.Drawing` 纵向拼接，宽=最宽页、高=各页高之和；legacy 的 `pageExport=false` 对 pdf/excel 不产出任何文件，本项目仅对 .doc/.docx 生效、其它类型仍逐页导出（更可用，有意不同） |
| pdf.compliance | Done | Done | version 映射 |
| pdf.merge / word.merge | Done | Done | SaveFile + outputFile；word.merge 与 `Util.cs:5097-5122` 逐项一致（`AppendDocument`+`FirstSection.Remove()`+`OoxmlSaveOptions`）。pdf.merge **有意与 legacy 不同**：legacy `Util.cs:5066-5089` 静默丢弃非 PDF 输入，本项目显式报错并列出被忽略的文件。对象生命周期：`MergePdfFiles` 的 `merged` 已改 `using` 立即释放（`DocumentMergeSplitService.cs:28`，与同文件其它 `Aspose.Pdf.Document` 写法一致）；`MergeWordFiles` 的 `Aspose.Words.Document` **不能加** `using`——该类型未实现 `IDisposable`，加了编译报 CS1674。实测（同进程连续 40 轮、每轮合 2 份 2 页 PDF）：改前托管内存 +2.9MB 后持平、峰值工作集 84MB；改后 +2.9MB、峰值 83MB——**无可测差异**；合并规模受评估版 4 元素限制（4 份输入即被阻断）未能测得大批量，如实记录不美化。该改动的价值是确定性释放（不再依赖终结器），不是内存数字；行为锁见 `tests\DocumentTools.Tests\MergePdfDisposalTests.cs`（输入释放后页数/文本完整、连续 20 轮产物完整） |
| pdf.split / word.split / ppt.split | Done | Done | 每页/按页数/范围 + PDF 书签；`pdf.split` 的删除页用独立键 `deleteStart`/`deleteEnd`（legacy `Util.cs:5736-5823` 的 `DeletePdfPages_Start/End`，两键都填即删除、无需 `deletePages` 开关；老配置给了 `deletePages`+`range` 时仍回退解析 `range`），输出沿用 legacy 的源文件同名 `{name}.pdf`；`ppt.split` 补 `slideSize`/`slideScale`（legacy `SplitPdfSizeCombox`/`SplitPdfScaleCombox`，缺省 OnScreen/Maximize）；**`refreshPageNumber` 只在 `word.split` 暴露**——`pdf.split`/`ppt.split` 侧 Core 从未消费该键，属假参数，已裁除；**ppt.split 输出名 `_第{N}份.pptx` 与 legacy `_第{N}页.pptx` 有意不同**（PPT 拆分单位是幻灯片块，且与 word.split 的 `_第{N}页.docx` 区分，避免同目录混淆） |
| pdf.watermark.add | Done | Done | 字体/字号/`fullScreen`/`underContent`/`useFileName`；另接 `fontColor`(#RRGGBB)/`horizontal`/`vertical`/`imageWidth`+`imageHeight`/`customPosition`+`positionX`+`positionY`/`repeatSpacingX`+`repeatSpacingY`（legacy `Util.cs:598-735` 的 20 参全覆盖）。间距向后兼容：旧键 `repeatSpacing`/`fullScreenSpacing` 存在时优先且 X/Y 同值，其次新键，最后回退 25。中文字族名先经 `FontAliases`（宋体→SimSun、黑体→SimHei、微软雅黑→Microsoft YaHei 等）解析，失败回退 SimSun，仅 SimSun 也失败才抛（该版本 `FontRepository.FindFont("宋体")` 直接抛 `Font 宋体 was not found`）。**平铺性能（task-29）**：由「每格 `new WatermarkArtifact` + `Artifacts.Add`（单次 9.6ms、超线性）」改为「复用一个 `TextStamp` 只改 `XIndent`/`YIndent`」，4 页 A4/25pt 间距实测 39 904ms → 1 248–1 675ms（artifact 计数仍 3264、逐页渲染差异 0.000%），10 页外推 3.0–3.6s；`TextStamp` 旋转支点比 `WatermarkArtifact` 高 1.1×字号，`XIndent` 已按 `1.1×字号×sin(角度)` 补偿 |
| pdf.watermark.remove | Done | Done | 删除 Watermark/Background 子类型的 artifact，与 legacy `Util.cs:5316-5333` 等价。**副作用（task-29 平铺优化带来）**：平铺水印改用 `TextStamp`/`AddStamp` 后，生成的 artifact 不带 `Watermark`/`Background` 子类型（规范要求 artifact 必须带 `/Type`，实际极少见），为让「加水印→去水印」闭环成立，`Remove` 会把 `Type`/`Subtype` 都是 `Undefined` 的**匿名 artifact 一并删除**；第三方 PDF 若恰好含匿名 artifact 也会被删掉（去水印等价性已用两条路径渲染 diff 0.000% 验证） |
| pdf.compress | Done | Done | quality/maxDpi/webOptimize/unembedFonts/flattenForms/flate/skipImages；`imageVersion`（Standard/Fast/Mixed → `Aspose.Pdf.Optimization.ImageCompressionVersion`，legacy `Util.cs:1460-1522` 的 `CompressPDF_CompressImageVersion`）已接；资源精简三开关（`removeUnusedStreams`/`linkDuplicateStreams`/`removeUnusedObjects`）由 legacy 的独立控件改为可单独关闭（`ParseBoolDefaultTrue`，缺省仍为 true，老配置行为不回退）——默认值与 legacy 未勾默认不同，理由：保持迁移前 DocTools 的实际压缩强度，避免用户升级后压缩率下降。**体积回退（task-29）**：Aspose.Pdf 对低熵文本 PDF 即使只 load+save 也膨胀到 134.9%（实测 68466→92356 B，且与任何参数组合无关），故服务改为先写临时文件再比字节数，产物 ≥ 原始即回退原始内容并置 `PdfCompressionResult.KeptOriginal = true`，操作提示追加「其中 N 个文件无可压缩空间，已保留原始内容」；实测 `testdata\pdf\sample.pdf` 68466→68466 B（回退命中）、噪声扫描件 461682→255464 B（0.553，真实压缩） |
| html.pdf | Done | Done | Chrome/纸张/CSS/四边距/超时 → HtmlToPdfOptions；纸张扩到 legacy 11 种（`Util.cs:1724-1738`：0=A0…7=Ledger、8=Legal、9=Letter、10=Tabloid），`landscape`、`navTimeout` 已接；`MapPaper` 的字符串分支按 legacy 修正（原先 `LETTER→1`、`LEGAL→2` 是错的，现为 9/8） |

---

## 2. 图片批处理（SkiaSharp，无 Aspose.Imaging）

| FeatureId | UI | Core | 缺口 |
|-----------|----|------|------|
| image.compress | Done | Done | 格式（PNG/JPG/JPEG/BMP/TIF/TIFF/GIF）+ JPG 质量 1-100/颜色模式 + PNG 级别/颜色类型 + BMP 色彩位数（4/8/16/24/32，自写 BMP 编码器；SkiaSharp 的 `Encode(Bmp)` 返回 null）+ TIF（自写 TIFF 6.0 单页 RGB 写入器，条带压缩默认 LZW、可选 PackBits/无压缩，`SKEncodedImageFormat` 无 Tif）+ GIF（自写 GIF89a 单帧 8bpp + 中位切分 + LZW，`Encode(Gif)` 返回 null）+ `useSourceExtension`（legacy `CompressImage_NotUseSelfExt`）+ `noCompress`（legacy `CompressIamge_NoCompress`，字节级复制）。**deviation**：`useSourceExtension` 默认 `false`（按所选格式输出），legacy 是「默认沿用源后缀 + 同时禁用格式下拉」（`BodyForm.cs:3899-3903`）；本面板无跨字段禁用能力，「可点但无效」等于静默错，故默认按格式转换，勾选「沿用源文件后缀名」即恢复 legacy 行为 |
| webp.image | Done | Done | WEBP→PNG/JPG/BMP/GIF（JPEG 质量 100，对齐 legacy；GIF 走自写 GIF89a 编码器） |
| image.ico | Done | Done | PNG-in-ICO；`multiSize` 一次输出 16/32/64/128/256 多目录项（legacy 仅单尺寸，属增强） |

---

## 3. 替换 / DataMatrix

| FeatureId | UI | Core |
|-----------|----|------|
| word.replace | Done（动态查找对 + Schema 选项） | Done（`DocumentReplaceService`）；「匹配选项」补 `ignoreFootnotes`（→ `FindReplaceOptions.IgnoreFootnotes`）；「替换后字体样式」`replaceUseNewStyle`/`replaceFontName`/`replaceFontSize`/`replaceBold` → `FindReplaceOptions.ApplyFont`（字体名/字号/加粗，仅在 `UseNewStyle=true` 时生效，默认 false = 沿用被替换处格式） |
| excel.replace | Done（动态查找对 + Schema 选项） | Done；Excel 工作簿无脚注概念，故不提供 `ignoreFootnotes`（避免假参数） |
| code.datamatrix | Done（列表右键 + 结果树右键：展开本节点全部内容 / 折叠本节点全部内容 / 展开全部节点 / 折叠全部节点 / 打开文件 / 复制节点文本；菜单按选中态置灰。**另含「图片目录批量分类」**：源/目标目录、包含子目录、保留源文件、线程数（自动/1/8…），按 DataMatrix 页码码归入 `页码-XXX`，无码图片归入 `异常-无法识别的图片`；**OCR 分支为可选组件**：勾「图像内容分类（OCR）」+ 匹配正则 + 分类目录名 + 内容标注，组件缺失时三项禁用并给安装提示） | Done（`DataMatrixService` 逐页解码；`DataMatrixImageClassifyService.Classify` 目录分类；OCR 由 `DocumentTools.Ocr` 插件提供，2026-11-06 补齐） |

---

## 4. 工具页（无假文件列表）

| FeatureId | UI | Core |
|-----------|----|------|
| code.barcode | Done（内容 + Schema 外观/标题/尺寸与旋转 + LOGO + 批量 + 识别条码/复制识别内容） | Done（`BarcodeRenderOptions`：DPI/模块/标题/LOGO + `Rotate` 0/90/180/270、`SizeMode` auto/fixed、`BarcodeWidth`/`BarcodeHeight`（0=该维自动）；`IBarcodeService.ReadTexts` 对齐全 legacy `Util.ReadBarCodeContent`，解析全部受支持类型并排除 MicrE13B） |
| idcard.check | Done（校验 4 行 + **随机身份证号码生成**：省/市/区县三级级联、数量、仅年满 18 岁、同时生成姓名、复制结果） | Done（`ChineseIdCard.AppendChecksum` + `IRandomIdCardService`：内嵌行政区数据 + 随机生日/顺序码/姓名；生成号全部通过 `ChineseIdCard.IsValid`） |
| name.vertical | Done | Done（`NameVerticalOptions` + `NamePinyinMode` 6 模式；拼音包用 `NPinyin.Core` 3.0.0 而非 legacy 的 `NPinyin` 0.2.6321，前者对 U+4E00–U+9FA5 共 20,902 字与后者逐字比对 `GetPinyin`/`GetInitials` 差异 0，且不产生 `NU1701`；`NameStorke`（姓氏笔画排序）在 legacy `Util.cs:497-501` 排序调用已注释禁用、无实际行为，故不迁；legacy 表格分支会跨行并名，本实现有意修正为保留行边界） |
| twain.debug | Partial | Partial（`TwainScanService` 用 NTwain 3.7.5 真驱动：`GetNativeImageStream` → SkiaSharp 落盘；本机无扫描仪，未做实机联调） |
| other.misc | Done | Done（`IEmbeddedAssetService` → `embedded/`；缺文件时提示 README） |
| other.extra | Done（金额大写 + `FileSystemWatcher` 监视 jpg/tif/bmp + **文件副本生成**：源文件/输出目录/数量/后缀名/序号位宽，预览并接续已有副本序号；说明卡写明 .jpg.err/.jpg.han/.tif.err/.tif.han 会被忽略） | Done（金额大写 + `FileSystemWatcher` 监视 jpg/tif/bmp + `IFileCopyService`：命名规则下沉 Core，接续目标目录已有副本序号、不覆盖） |

> 交互入口（右键菜单 / 双击 / 快捷键）的 legacy 取证结论：全仓 `ShortcutKeys` 出现 **0 次**、文件列表无 `DoubleClick` 处理，右键入口只有三处 —— `ListViewRightMenu`（文件列表，已迁，菜单项按状态置灰）、`TreeViewContentMenu`（DataMatrix 结果树，已迁）、`WatcherMakeTestFileMenu`（目录监视的测试文件生成器，有意不迁：属开发者自测按钮，`BodyForm.Designer.cs:14142` 默认 `Enabled = false`，正式界面不暴露）。因此不新增 `Ctrl+O` / `Delete` / 双击打开这类 legacy 里不存在的入口。

---

## 5. 考评 Exam

| FeatureId | UI | Core |
|-----------|----|------|
| exam.mss3 | Done | Done（`IMss3VoteService`：选票列表 / 插入选票页灰度 BMP 写 `Vote_PageInfo` / `Item` 列转 Memo）。**文件过滤器对齐 dev 的 `*.mss3`**（`ExamViewKit.Mss3Filter`，legacy `BodyForm.cs:4319-4323`）；选错文件时 `ListVotes` 给「不是 MSS3 选票模板（缺少表 Vote_EssInfo）…请选择 .mss3 文件」 |
| exam.single | Done | Done（`ISingleDocumentService` Alt 换图批量；三档名称后缀 + `PersonCodeLibrary` 读内置 001–999 码图） |
| exam.insert | Done | Done（`IExamInsertService`：Exam_EssInfo/Exam_Score/Exam_BoolItem/Exam_Ruler + 默认 5 条判分规则，含事务回滚；`ExamBatchInsertTemplates` 提供批量项目/选项模板、11 组内置选项名、中文序号与剪贴板解析；`ExamIndexService` 提供改号换位与 Exam_EssInfo 物理顺序重建；项目与选项卡新增「清空项目」，对齐 legacy「移除全部项目」） |
| exam.report | Done | Done（`IExamReportService`：`Report_Detail.Content` 的 `Item=`/`Raid=`/`Kind=` 定点替换，支持 `OldRaid` 限定；`GetGridSize` 报表行列数、`ListExamRaids` 测评身份）。**2026-11-06 真实库修正**：`ListReportItems` 改为解析 `Content` 的 `Item=` 段（原先读 `Item` 列拿到的是单元格坐标），自然排序后「所有项目」置首；新增 `ListExamTableItems`/`ListItemSelectors` 支撑「考评表项目 → 项目选项」联动；报表项目列表右键菜单、「获取…」复制菜单、项目筛选改为可编辑下拉 |
| exam.vote | Done | Done（`IVoteReportService` + `embedded/VoteReportDoc.doc`） |
| exam.package | Done | Done（`Inspect`/`Reset`/`ListUnits` + `Merge`/`HasDuplicateUnits`：事务内合并人员与单位、跳过目标自增列、`.exam` 重名另存 `_1`） |
| exam.circle | Done | Done（`IExamCircleFixService`：SkiaSharp + FFT 归一化互相关 NCC、NMS、替换图写回；旋转另存 `IExamCircleImageService`） |

Access（ACE/Jet）路径全部接通。两个已知实现约束：
1. ACE 下 `GetSchemaTable()["IsAutoIncrement"]` 对 COUNTER 列恒 `False`，数据包合并用「标准属性 + ADOX `AutoIncrement`」并集判定自增列。
2. 圆圈修复图面按 1 像素 = 1 DIP 归一（`Stretch.Fill` + 显式像素尺寸），避免扫描件 300dpi 元数据让框选坐标与匹配坐标错位。

---

## 6. 系统

| FeatureId | UI | Core |
|-----------|----|------|
| system.settings | Done | Done（JSON；排除窗体大小/锚点；坏值兜底 + 目录/文件校验 + 导入导出 + 原子写已实现，详见 §9） |
| system.tools | Done | Done（`IWindowsSystemToolsService`：暂停天数/箭头/粘滞键/打开设置；需管理员项已校验） |
| system.network | Done | Done（`INetworkAdapterService`：列表/启停/静态 IP/DHCP；WMI→netsh；需管理员。另接**配置导出/加载（`.KUNNET`，`NetworkConfigFile`）+ 复制 MAC/IPv4 + 打开网络连接**；`NetworkAdapterInfo` 增 `Mac`） |
| about | Done | Done |
| home | Done | — |

---

## 7. 乱码治理

### 已出现问题

`SystemFeatureModules.cs` 中文曾被写成 ASCII `?`（高字节≈0），系统设置页显示乱码。

### 修复

- 已用 **UTF-8 with BOM** 重写该文件。
- 含中文的 `.cs` 应用 UTF-8（建议 BOM），避免部分工具按系统 ANSI 覆盖。

### 规范（写入仓库约定）

1. 编辑器：UTF-8；Cursor/VS 保存勿转 code page 936 且丢字符。
2. 用脚本写中文源码时：`UTF8Encoding(encoderShouldEmitUTF8Identifier: true)`。
3. CI/本地可扫：`Content = "???` / `base("???` 等模式，命中即失败。
4. 权威文案若再坏：以本文件与 `feature-ux-spec.md` 中文为准重写，勿手工猜 `?`。

### 自检命令

```powershell
Get-ChildItem src -Recurse -Filter *.cs |
  Where-Object FullName -notmatch '\\(bin|obj)\\' |
  Select-String -Pattern 'Content = "\?{3,}|base\("\?{3,}|PlaceholderText = "\?{3,}'
```

无输出即为通过。

---

## 8. 推荐实施顺序（下一轮）

1. **P0（持续）** 符合 [architecture.md](architecture.md)「设计模式硬约束」+「控件≠业务」  
2. **P2 已做** 条码标题/DPI/批量；pdf.compress 细参；金额大写 Core  
3. **P3 已完成** exam 7 页（mss3/insert/report/package/circle 的 Access 与图像能力全部落地）  
4. **待验** `twain.debug` 实机扫描（本机无设备）；Access 相关页面需装 ACE 才能跑通
5. **冒烟** `dotnet run --project tools\FeatureSmoke` —— 真 DI + 真视图工厂逐页创建并 Measure/Arrange（当前 47/47 通过），改视图后先跑它
6. **导航** 侧边栏按分类 `Expander` 可折叠；搜索时强制展开匹配组；激活功能自动展开所在组  
7. **图标** WPF-UI `SymbolIcon`（`FeatureIcons`）；不迁 WinForms PNG  



交互与分区仍以 `feature-ux-spec.md` 为准：**列表在上，操作区在下**。

---

## 9. 有意偏离 legacy 的项及理由

迁移不是逐字复刻：以下每一条都是**看过 legacy 实现后有意不同**，改动前请先读理由，不要以「legacy 不是这样」为由回退。

| 项 | legacy 行为 | 本实现 | 理由 |
|---|---|---|---|
| `name.vertical` 表格分支 | `Util.cs:399-402` 先删**全部** `\n`，再把 `\t` 换成 `\n` 后按 `\n` 切分 → 多行表格会被跨行并名（`张三\t李四\n王五\t赵六` → `李四王五`） | `\r\n`/`\r`/`\t` 统一当行分隔，不删任何换行（`NameVerticalService.cs:165-181`） | 该功能的真实用途是「从 Word 表格复制一列名字 → 可打印竖排名单」，并名直接毁数据；legacy 的顺序属缺陷 |
| `name.vertical` 的 `NameStorke`（姓氏笔画排序） | `Util.cs:497-501` 里排序调用**被注释掉**，开关空转 | 不建键、不实现 | 迁一个无行为的开关比不迁更糟 |
| `name.vertical` 拼音包 | `NPinyin` 0.2.6321（只带 .NETFramework 资产，`NU1701`） | `NPinyin.Core` 3.0.0（netstandard，同命名空间同 API） | 对 U+4E00–U+9FA5 共 **20,902** 字逐字比对 `GetPinyin`/`GetInitials`，两包差异 **0**；且不产生警告。压 `NU1701` 会掩盖以后真正的还原问题 |
| `image.compress` 的 `useSourceExtension` | `CompressImage_NotUseSelfExt` **默认未勾 = 沿用源后缀**，同时把格式下拉**禁用**（`BodyForm.cs:3899-3903`） | 键存在，**默认 `false`（按所选格式输出）**，勾选即恢复 legacy 行为 | 本面板没有跨字段禁用能力，「格式可点但被静默忽略」比默认值偏离 legacy 更糟 |
| `image.compress` TIF 输出 | `Util.cs:1343-1442` 用 `TiffOptions(BitsPerSample=8, Photometric=MinIsBlack)` 存**8 位灰度** | 自写 TIFF 6.0 单页、24bpp RGB、**按条带压缩**（`TiffEncoder.cs`）：默认 LZW，可选 PackBits / 不压缩；条带行数按「目标 64KB/条」算并夹在 8–256 行；透明像素按白底合成。**A4 300dpi（2480×3508，8.7MP）实测**，未压缩 24.89MB：纯白页 LZW 0.17MB（0.7%，PackBits 0.39MB/1.6%）、表格线+文字块 LZW 0.38MB（1.5%，PackBits 0.68MB/2.7%）、彩色噪声渐变 LZW 25.55MB（102.6%，PackBits 25.08MB/100.8%）；编码耗时 0.44s / 0.50s / 1.25s，12MP（4000×3000）彩色 1.77s。小图矩阵（灰阶 PackBits 46.8%/LZW 59.5%、白页 98.3%/99.2%、彩色渐变 −0.8%/15.7%）见 `tests\DocumentTools.Tests\ImageTiffGifEncoderTests.cs` | SkiaSharp 3.119 没有 TIFF 编码器（`SKCodec.Create` 也解不了 TIFF），只能自写字节；写基线 RGB 是为了不把彩色素材压成灰度（legacy 那条灰度是它自己的限制，不是需求），基线 TIFF 无 alpha 通道故先合成白底；PackBits 在 24bpp RGB 上几乎无效（三通道交错后字节流没有 3 连重复），所以默认取 LZW——扫描件（白页/表格/二值）能压到 0.7%–1.5%，而彩色照片 TIF 本来就该用 JPEG 分发，102.6% 如实记录、不设阈值。**使用者口径**：扫描件/二值文档选 TIF 收益最大（A4 300dpi 实测 0.7%–1.5%）；彩色或照片类素材请选 JPEG，TIF 的 LZW 对高频内容无压缩收益（实测 102.6%）。 |
| `image.compress` BMP 编码 | Aspose 原生 `BmpOptions` | 自写 24/16/8/4/32 位未压缩 BMP 写入器（`ImageConversionService.cs:236-360`） | SkiaSharp `Encode(Bmp)` 返回 **null**，沿用旧实现会直接抛「编码失败：.bmp」 |
| `image.ico` | `IcoSizeDrop` 单选 16/32/64/128/256，一次一个尺寸 | 保留单尺寸 + `multiSize` 一次输出五个目录项的 PNG-in-ICO | 多尺寸 ICO 是 Windows 图标的标准做法，纯增强、不改变单尺寸语义 |
| `image.compress` / `webp.image` GIF 输出 | `image.compress` 的格式下拉**没有 GIF**（`Util.cs:1254-1265` 只有 JPG/JPEG/PNG/BMP/TIF）；webp 转换的保存对话框可出 GIF（`Util.cs:2191-2221` 的 `GifOptions` 分支） | 自写 GIF89a 单帧 8bpp（中位切分调色板 + LZW，`GifEncoder.cs`），两个功能的格式下拉都提供 GIF；透明像素按白底合成、不带透明索引 | SkiaSharp `Encode(Gif)` 返回 null；补齐编码器后 webp→GIF 才真正对齐 legacy 的保存对话框能力，image.compress 顺带获得同一能力。不做透明索引是为了避开「透明 + 调色板排序」这类兼容坑，图标类素材白底更稳 |
| `pdf.compress` 资源精简三开关 | `CompressPDF_RemoveUnUseStream/LinkeSameStream/RemoveUnUseObject` 全**未勾** | `removeUnusedStreams`/`linkDuplicateStreams`/`removeUnusedObjects` 默认 **true** | 保持迁移前 DocTools 的实际压缩强度，避免升级后压缩率下降 |
| `pdf.compress` 的 `quality` | 第 5 参是 `int CompressPDF_CompressImageQuailty`（1-100） | Schema 用 Text 1-100（默认 80） | 原来是自造的「低/中/高」Choice，与 legacy 的整数语义不符 |
| `pdf.watermark.add` 默认值 | 不透明度下拉 `SelectedIndex=5` → 0.5；平铺间距 `WaterDmi_Hor/Ver=25` | `opacity` 默认 `"0.5"`；`repeatSpacing` 拆成 `repeatSpacingX`/`repeatSpacingY`（默认 25） | 对齐 legacy 初始值；X/Y 分开是因为 Core DTO 本就分轴，旧键仍由 operation 兜底读取 |
| `pdf.split` / `ppt.split` 的 `refreshPageNumber` | 只有 word 分支消费该勾选 | 从这两个分支裁掉，仅 `word.split` 保留 | 假参数：pdf 侧页码重排要改页面标签/页脚，风险远高于收益，隐藏比假生效诚实 |
| `excel.replace` 的 `ignoreFootnotes` | 无此概念（Excel 工作簿没有脚注） | 不提供该键（`word.replace` 有） | 加上即新假参数，与 P1-7 的目标直接冲突 |
| `ppt.pdf` 的 `exportNotes` | 勾选框（仅开/关） | Choice `None`/`BottomFull`/`BottomTruncated`/`RightFull`，`"true"`/`"false"` 旧值仍兼容 | `PptxSaveOptions.NotesPositions` 本来就有四值，勾选框丢失三种合法排版 |
| `ppt.split` 输出名 | `_第{N}页.pptx` | `_第{N}份.pptx` | PPT 拆分单位是幻灯片块；也与 `word.split` 的 `_第{N}页.docx` 区分，避免同目录混淆 |
| `system.settings` 坏值/写盘 | `App.config` 原样读：`ListViewLineHeight=9999`、`ShowOpenTargetDialog=maybe` 都直接生效；迁到 JSON 后写盘用 `File.WriteAllText`，权限失败直接抛；无导入导出 | 读盘逐字段修正（lineHeight clamp 18–64、非法布尔→默认、含 `"<>\|` 的路径清空、相对路径保留但记警告），修正明细进 `IAppSettings.LastLoadWarnings`；写盘失败返回 `SettingsSaveResult`；导入「全有或全无」并逐字段报告（`AppSettingsService`） | 设置页是本机唯一状态入口，一个坏值不该让启动后行为不可解释；导入若半应用，用户无法判断当前到底哪几项生效。相对路径**不清空**是有意的：读盘时清空等于用户下次保存就把配置吃掉，宁可在 `Validate()` 里标 Missing/Invalid |
| `system.settings` 目录型设置 | 只存字符串，路径不存在也照用（exam 页面打开时才报错） | `AppSettingsValidator.CheckDirectory/CheckFile` 给出 `Ok/NotSet/Missing/Invalid` + 中文说明，`Validate()` 汇总成报告；**不因目录缺失拒绝保存** | 用户在未装 VFSoft 的机器上编辑设置是合法场景，校验只做提示不做阻断 |
| 错误消息规范化（评估模式/权限/路径）已统一 | `MergeDocumentOperation`/`SplitDocumentOperation`/`ImageConvertOperation`/`HtmlToPdfService` 把 Aspose 与 .NET 原文直出：`At most 4 elements (for any collection) can be viewed in evaluation mode.`、`Access to the path '...' is denied.`、`没有找到浏览器可执行文件：<空>` | 统一走 `OperationErrors.Translate(ex, action, outputDirectory)`，输出「做了什么：为什么失败。怎么修」——评估模式 → 「评估模式，集合超过 4 个元素时会被限制。请把 `assets\licenses` 下的授权文件放齐，或把文件拆分后再试」；输出目录 `UnauthorizedAccessException` → 「输出目录不可写：…请换一个目录，或去掉该目录的只读属性」、`IOException` → 「输出目录无法写入：…请换一个目录，或关闭占用该文件的程序」；`html.pdf` 缺浏览器 → 「请安装 Chrome 或 Edge，或在选项里把「浏览器路径」(chromePath) 指向 chrome.exe」。**已带中文的消息原样透出**，不再二次包裹 | 用户看不懂英文原文也不知道下一步该做什么；而二次包裹会把 Service 层的精确提示（如「合并 PDF 只接受 .pdf 文件，以下输入不会被合并：…」）糊成「合并失败：合并 PDF 只接受…」。内层异常一律保留在 `InnerException`，便于排查 |
| `AccessDb` 打开失败归因 | legacy `AccessSession` 打开失败直抛 `OleDbException` 原文；迁移初期把所有打不开的情况统一写成「无法打开 Access 数据库。请安装 Microsoft Access Database Engine (ACE)。」 | 三级归因（`src\DocumentTools.Core\Data\AccessDb.cs`）：①三个 provider 内层**全部**是 `provider is not registered` → 「未检测到可用的 Microsoft Access Database Engine (ACE) OLE DB 提供者（当前进程 64/32 位：…）」；②任一内层命中 `不可识别的数据库格式`/`unrecognized database format`/`not a valid password`/`cannot open`/`找不到文件` 等 → 「数据库文件可能损坏、被占用或不是 Access 2003/2007+ 格式：<文件名>（底层错误：…）」；③其它 → 「无法打开数据库 <路径>：<内层首行>」。文件不存在仍是 `FileNotFoundException`。归因前先剔除 Jet 的「未注册」噪音 | QA 实测坏文件时 ACE 报「不可识别的数据库格式」而 Jet 只报「未注册」，取最后一条异常就会把损坏文件误判成缺 ACE，用户照提示装完 ACE 依然打不开。内层异常一律保留在 `InnerException` |
| `DOCUMENTTOOLS_SIMULATE_NO_ACE` 开关位置 | tools 侧自用（`AceAvailability` 自己读环境变量） | 下沉到 Core：`AccessDb.SimulateMissingAceVariable` + `AccessDb.IsMissingAceSimulated()`（值 `1` 或 `true`），打开任何存在的文件即抛与「真缺 provider」同一条消息；`tools\ExamFixture\AceAvailability` 改为复用该判定，不再维护第二份逻辑 | 开关只在 tools 生效时，`AccessDb` 在装了 ACE 的机器上永远走不到「缺 provider」分支，等于这条路径没有回归；现在 `AccessDbFailureMessageTests` 能在任何机器上验证它 |
| `word.pdf` 的 `removeTempDocx` | `UI.cs:706` 在 Word2Pdf 完成后按勾选删除 `TempSaveDocxPathString` 目录 | **删除该键**，不提供 | 本实现的 `OfficeConvertService.WordToPdf`（`:20`）直接读源文件写目标 PDF，全程不产生 temp docx 目录；唯一建临时目录的是 `MergeWordPagesToSingleImage`（`:535`），它无条件 `try/finally` 清理。留着就是「勾了也没东西可删」的假参数 |
| `word.pdf` 的 `useLocalOffice` | `BodyForm.cs:7002-7019` 勾选时禁用合规/颜色/字体/更新域 5 个控件，`Util.cs:2717-2738` 走 Word COM `ExportAsFixedFormat` | 真接 Word COM（晚绑定 `Word.Application`，不引用 Office Interop 程序集）：`OfficeConvertService.WordToPdf` 开头分流到 `WordToPdfViaLocalOffice`，导出参数照抄 legacy（`wdExportFormatPDF`/`OptimizeForPrint`/`WordBookmarks`）；未装 Word 时先探 `Type.GetTypeFromProgID` 再抛「未检测到 Microsoft Word。请安装 Word，或取消「使用本机 Office 转换」勾选后重试。（已尝试：Word.Application）」；成功/失败都写一行 `logs\conversion.log`（源、输出、耗时 ms、`引擎=Word.Application`），COM 失败转中文并保留 `InnerException` | 原先是 `_ = options.UseLocalOffice;` 的空操作——勾了与不勾结果一致，是最典型的假参数。legacy 这条路径真实可用（本机 Office 排版更准），删键等于丢功能，故选择补实现 |
| `word.replace` 的 `regexSub` 语义 | `Util.cs:5461` 只认 `UseRegexSub` | `ReplaceDocumentOperation.cs:45` 只取 `regexSub`（曾误写 `regexSub \|\| regex`） | 「使用正则」与「使用 $1 捕获替换」是两件事：写成或运算后，勾了正则就再也关不掉捕获替换，用户想搜 `$` 字面量永远失败 |
| `excel.replace` 的删行区间 | `Util.cs:5452` 把结束行当 Aspose `DeleteRows` 的 `totalRows` 传（同源缺陷） | `DocumentReplaceService.cs:101` 改为 `totalRows = end - start + 1`；清空分支改用 `cells.MaxColumn` 而非硬编码 255 列 | Aspose 第二参是「删除行数」，沿用 legacy 写法会多删行（3~7 行删掉 6 行只剩 2 行），静默破坏数据；255 列上限会让宽表右侧漏清 |
| `exam.package` 合并失败的消息与规模 | legacy `BodyForm.cs:11034-11302` 直抛 ACE/OleDb 原文；人员逐行 INSERT，规模未测 | `ExamPackageService.Merge` 的 catch 包成「合并失败：写入目标数据包 <目录> 的 Exam_Base.mdb 时出错（事务已回滚，目标库保持合并前状态，源数据包未改动）。底层错误：<原文>」，原文保留在 `InnerException`。规模实测见 `tools\ExamFixture --bench-merge`：5000 人/200 单位 1.87 秒、进程峰值 85.5 MB；20000 人/500 单位 7.17 秒、峰值 88.4 MB，托管堆两轮均只涨约 0.5 MB（快照表随 Merge 返回即回收） | 只读/被占用的目标库上，ACE 只报「操作必须使用一个可更新的查询。」——不说是哪一端、有没有回滚、源库动没动。合并不可逆，这条消息必须能照做。性能侧未做批量事务优化：5 千人 1.87 秒、按行线性增长（4 倍数据 3.8 倍耗时），离 60 秒阈值还差一个量级，先记录口径（`MergeBench` 以进程 PeakWorkingSet64 为峰值口径、GC.GetTotalMemory(true) 差为托管堆口径），需要时再改流式 |
| `exam.single` 生成名称后缀 | 三个单选（纯数字+后缀 / 第X人 / 第X页，`BodyForm.Designer.cs:11499-11535`）+ 一个「生成名称后缀」文本框（`:11546`，默认「测评人」）；文件名 = 模板名 + 后缀 + `.docx`，序号 1 基（`BodyForm.cs:6286-6311`） | `SingleDocumentRequest.NameSuffixMode`（枚举 `SingleDocumentNameSuffixMode`）+ `SuffixWord`，拼装在 `SingleDocumentNaming.BuildSuffix/BuildFileName`；UI 下拉三项 + 后缀词输入（`ExamSingleView`） | legacy 实际是三档 radio + 一个可编辑后缀词（没有第四档）。后缀词输入框只对「纯数字+后缀」档生效并随之禁用——legacy 里它也只在那一档参与拼接，启用着会让人误以为对「第1人」也管用 |
| `exam.single` 内置单人码图片 | 「使用内置单人码图片」本地默认勾选（Designer `:11616`），图源是程序内嵌 `PersonCode.zip`：`DocumentTools\Resources\PersonCode.zip` 2 288 286 字节、1000 条目（1 个目录项 + `001测评人.bmp`…`999测评人.bmp`，每张 200 758 字节），由 `Util.GetPersonCodeZipFileStream()`（`Util.cs:4615-4647`）解压成 999 个流 | **Done**。资产单一真相源仍是 `DocumentTools\Resources\PersonCode.zip`（git 已跟踪），App 与 tests 通过 csproj `Content` + `Link="assets\embedded\PersonCode.zip"` + `CopyToOutputDirectory=PreserveNewest` 构建期链接到输出，`EnsurePersonCodeArchive` 目标在文件缺失时直接 `Error` 停构建；运行时 `AppContext.BaseDirectory\assets\embedded\PersonCode.zip`。Core 侧 `PersonCodeLibrary` 按序号取单个条目（只缓存「序号→条目名」元数据，不一次解压 999 张），越界抛「内置测评人码图只有 001–999，收到 {n}」；`SingleDocumentRequest.UseBuiltInPersonCode`/`PersonCodeStartIndex` 接线，`ExamSingleView` 提供勾选 + 起始序号（未勾选时序号禁用、图片源恢复必填） | 与 legacy 的差异只有「按需读单张」——legacy 一次性把 999 个流读进内存。判定依据：`PersonCodeLibraryTests` 断言 zip 1000 条目 / 999 张图、001 与 999 可 `SKBitmap.Decode`、0 与 1000 抛中文异常、条目名容忍前缀与大小写，以及同一模板同一后缀模式下内置码图与外部素材产出的文件名逐字一致（命名规则与图片来源无关） |
| `word.replace` 的 `UseNewStyle`（更改替换的字体和大小） | **实为死开关**：`Util.cs:5373` 形参 `UseNewStyle = false`，`Util.cs:5412-5416` 只在它为 true 时设 `ApplyFont.Name`/`.Size`（**无颜色、无加粗**），但调用点 `UI.cs:394-409` 从未传该命名参数 → 分支永远 false，勾了勾选框（`BodyForm.Designer.cs:7732`）也不生效 | 真接线：`DocumentReplaceOptions.UseNewStyle` + `FontName`（空=不改）+ `FontSize`（0=不改）+ `Bold`（`bool?`，null=不改）→ `FindReplaceOptions.ApplyFont`（`DocumentReplaceService.cs:44-62`）；Schema 四键 `replaceUseNewStyle`/`replaceFontName`/`replaceFontSize`/`replaceBold`（`FeatureOptionSchemas.cs:563-567`）。**默认 false 时与 legacy 实跑完全等价，开启后实现的是勾选框声明的意图**。颜色项**不加** | legacy 参数表里没有颜色（`Util.cs:5412-5416` 只有 `Name` 与 `Size`），加了就是自造参数——与删 `removeTempDocx`、`RightFull`、`excel.replace` 脚注同一把尺子；产品若要「替换后变红」是另一个需求，进 backlog。加粗做成三态是我们补的能力（legacy 无此参数），用 Choice 而非 Check 才能表达「不改/加粗/不加粗」。证据：`WordReplaceStyleTests.cs` 断言产物 run 的实际值——开启时 `SimSun`/`18`/`true`，关闭时保持源格式 `Arial`/`10`/`false` |
| `code.datamatrix` 图片目录分类：异常目录 | `Util.cs:6338-6364` 的 `if (TempBarcodeReader.FoundCount == 0)` 被写在 `foreach (ReadBarCodes())` **内部**，0 结果时循环体不执行 → 「异常-无法识别的图片」永不产出；同时非 `T` 码图片（如 `A01`/`B`）被静默留在原目录 | 真正落盘：没有 `T` 码的图片（含 0 结果与只有非页码码两种）统一归入 `异常-无法识别的图片`，子目录模式下同样保留相对层级 | legacy 那个分支是**不可达死代码**，声明意图（目录名都取好了）与实跑行为不一致；迁移选「实现意图」，让用户知道哪些图没分进去，而不是让它们既不在源目录预期位置也不在异常目录 |
| `code.datamatrix` 图片目录分类：子目录模式的目标路径 | `Util.cs:6306-6319` 用 `Directory.GetParent(...).FullName.Replace(GetDirectoryRoot(...), "")` 取到**以 `\` 开头**的相对段，再 `Path.Combine(target, "\考点A", …)`；`Path.Combine` 遇到根路径第二参会丢弃第一参 → 实际写到**盘符根**（`D:\考点A\页码-001`），不是目标目录 | `Path.GetRelativePath(sourceRoot, fileDir)` 后拼在 `targetRoot` 下（`DataMatrixImageClassifyService.cs:152`） | 照抄 legacy 会把文件散到盘符根、还可能因权限失败；这是明确的路径拼接缺陷，不是业务口径 |
| `code.datamatrix` 图片目录分类：「保留源目录内文件」 | `BodyForm.cs:7318` 调用 `RecognitionPictureDataMatrix` 时**没传** `SaveSrcFile`（用默认 `true`），勾选框 `Exam_Picture_Store_ScrFile` 是死控件（默认已勾，取消勾选也无效果） | 真接线：`KeepSourceFiles` 由勾选值驱动（默认 `true`，与 legacy 默认一致；取消勾选才是移动） | 假参数比删控件更糟；默认值不变，所以默认行为与 legacy 完全一致，仅「取消勾选」从无效变为生效 |
| `code.datamatrix` 图片目录分类：后缀大小写 | `Util.cs:6204-6209` 用 `Path.GetExtension(FileName) == ".jpg"` 等**大小写敏感**比较，`.JPG`/`.PNG` 会被漏掉 | 忽略大小写比较（`DataMatrixImageClassifyService.cs:14`） | 扫描仪/相机导出的后缀常是大写；legacy 漏文件属缺陷 |
| `code.datamatrix` 图片目录分类：一张图多个 `T` 码 | 对每个 `T` 码各复制一份到不同 `页码-*` 目录 | 取第一个 `T` 码，一张图只归一个目录 | 同一张图散落多份会让后续核对无法判断哪份是原件 |
| `exam.report`「复制评测主体合计身份」 | `ExamRaidsCopy_Click`（`BodyForm.cs:6585-6604`）把 `DataSource as List<string>`，而 `DataSource` 在 `:6068` 被赋成 `DataTable` → 强转恒为 `null`，按钮点击**什么都不发生** | 真实现：`ExamReportService.BuildRaidClipboardText`（`:650`）按 legacy 拼法（显示名逐个 + 「，」）产出文本，`ExamReportView.CopyRaids`（`:564`）写剪贴板；排除「批量设置全部身份」行 | 死按钮用户的感知是「点了没反应」；声明的意图（复制身份清单含合计）是明确的，按意图实现并保留 legacy 的拼接格式 |
| `exam.circle` 扫描仪取图 | dev 该页有「扫描仪获取图片 / 扫描仪设置」菜单（`BodyForm.cs:176-179/247-251`），取图走它自带的 TWAIN 封装 | 复用 `ITwainScanService`：`ExamCircleView.ScanImage`（`:574`）扫描到 `%LOCALAPPDATA%\DocumentTools\scans\{时间戳}` 并载入第一张；`ShowUi=true` 让驱动界面临时改 DPI/纸张 | 不重造 TWAIN 取图流程；DPI 固定 300 是为了让驱动界面成为唯一设置入口，避免再造一套「设备选择/DPI/多页」面板 |
| `exam.insert` 三种得分比重 | 得分比重 / 弃权得分比重 / 无效得分比重（`BodyForm.Designer.cs:12558/12357/12391`，默认 0 / -1 / -1）；每插一个项目必然追加 `无效`、`弃权` 两条隐式选项（`BodyForm.cs:9254-9268`），只写 `Exam_Score`（`Kind=选项`、`Score=比重`），不写 `Exam_BoolItem`、不占 `ItemOrder`（`Util.cs:4208-4211/4261-4265`） | `ExamInsertRequest` 增 `ItemScoreWeight`/`AbstainScoreWeight`/`AbstainCalcAvg`/`InvalidScoreWeight`/`InvalidCalcAvg`（默认值与 legacy 一致）；`InsertImplicitSelectors` 按 legacy 顺序（先无效、后弃权）补两行，调用方已显式给出同名选项时不重复；`ExamInsertResult.ImplicitSelectorCount` 单独计数，`SelectorCount` 仍只统计调用方给的选项 | 弃权默认计入得票率、无效默认不计入，因此无效会多一行「选项不计入得票率=1」——这是 legacy 既有行为，不是新口径。项目分值留空时才回落到请求级「得分」，避免覆盖逐项分值 |
| `word.pdf` 本机 Office 导出的书签枚举 | `Util.cs:2731` 传 `Word.WdExportCreateBookmarks.wdExportCreateWordBookmarks`（枚举值 **2**） | `OfficeConvertService.WordToPdfViaLocalOffice` 的 `CreateBookmarks: 2`（代码复审时发现原写成 `1`，那是 `wdExportCreateHeadingBookmarks`） | 传 1 不会报错，只会让产物目录按「标题样式」生成而非按 Word 手工书签——一条静默的功能差异，用户只在对比旧版 PDF 目录时才会发现。同一处复审还发现失败归因只 catch `COMException`：ProgID 指向别的自动化服务器时抛 `RuntimeBinderException`，那时「这份 PDF 为什么是 Aspose 出的」在 `logs\conversion.log` 里查不到，已放宽为 `catch (Exception) when (ex is not OperationCanceledException)` 并按类型给不同修复提示 |
| `excel.replace` 删行区间的两种脏输入 | legacy 把 0/负数直接传给 Aspose（抛英文异常） | ① 结束行 < 起始行 → 抛 `ArgumentException`「结束行不能小于起始行（起始行 N，结束行 M）。」并不产出文件；② 结束行超出该表最大行 → 夹到最大行再删（原实现把结束行当 `totalRows` 传会抛 `CellsException` 英文原文） | 旧实现把 `totalRows` 算成负数后 `continue`，结果「什么都没删」却照常保存成功——比报错更坏，用户以为删掉了。越界夹紧则是把「Aspose 会抛英文异常」换成「按用户显然的意图删到最后一行」。回归锁：`ToolUiContractTests.ExcelReplace_ReversedRowRange_ThrowsActionableMessage`（断言不产出文件）与 `ExcelReplace_EndRowBeyondLastRow_DeletesOnlyExistingRows`（8 行、6~50 → 只剩 R1..R5） |
| `exam.insert` 的分值解析 | legacy 直接 `Convert.ToDouble` 原文（脏值抛异常） | `ExamInsertRules.ParseScore`（`ExamInsertModels.cs`，纯函数可单测）：空串 → 0，非空且不是数字 → 抛「分值必须是数字，收到「abc」。请填 0、1 或 1.5 这样的数字。」 | 原实现对 `TryParse` 失败静默返回 0，「10分」「abc」都会变成 0 分写进 `Exam_Score`，错误要等成绩汇总对不上时才暴露。放在 `ExamInsertRules` 而不是留在 Service 私有方法，是为了让它能被不依赖 ACE 的单元测试直接覆盖（`ExamInsertScoreParsingTests` 11 例） |
| `html.pdf` 的边距 / `useCss` / `waitResources` 默认值 | dev 面板：四边距默认 **50px**（`BodyForm.Designer.cs:6597/6615/6624`）、「等待所有页面元素加载完成」与「使用内部CSS样式设置页面」两框**默认未勾**（`:6697/6727`） | 已对齐 dev：`FeatureOptionSchemas.cs:437-448` 四边距 `"50"`、`useCss`/`waitResources` 均为未勾；`HtmlToPdfOperation.cs:44-47` 键缺失时回退 `"50"` | 迁移初期把边距默认写成 `"0"`、两个勾选框默认勾上，等于把 dev 的默认版心换掉；更糟的是 `useCss=true` 会让 `PreferCSSPageSize` 生效并**忽略边距**，与「默认 50px 边距」自相矛盾。超时默认仍保留 30s/30s（dev 是 3s/5s），理由：3 秒对含外链资源的页面基本必然失败，且失败只会露出 PuppeteerSharp 英文异常；用户要复现 dev 行为在选项里填 3/5 即可。对照明细见 `docs/dev-parity-convert.md` §1 |
| `pdf.compliance` 版本下拉项数、`webp.image` 格式项与默认值 | dev 用 `Enum.GetNames(typeof(Aspose.Pdf.PdfFormat))` 铺 **27 项**、默认第 0 项 `PDF_A_1A`（`BodyForm.cs:1223`；`PDFComplianceBox` 无显式 `SelectedIndex`，WinForms 绑定后即选首项 —— 见 `BodyForm.cs:1208`、`Designer.cs:1719/1931-1936`）；WEBP 目标格式 **5 项**（JPG/JPEG/PNG/BMP/GIF）默认 JPG（`BodyForm.cs:1423-1432`，走 `DisplayMember`/`ValueMember` 双值绑定） | **已对齐 dev，且改变了用户可感知的默认值**：`pdf.compliance` 默认 `1.7` → **`PDF_A_1A`**（项数 5 → 27）；`webp.image` 默认 `PNG` → **`JPG`**（项数 4 → 5，补独立 `JPEG`）。实现面：`FeatureOptionSchemas.cs` 两段键、`OfficeConvertOperation` 键缺失回退改为 `"PDF_A_1A"`、`ImageConversionService.NormalizeFormat` 拆 `jpeg → .jpeg` | 初版只做了能力层（`ParsePdfFormat` 能解析 27 个枚举名）而 UI 仍是 5 项/4 项，等于「能转但选不到」，与用户「和 dev 一毛一样」的要求不符。**默认值变更须知**：没填 `version` 的旧配置，产物从 PDF 1.7 变成 PDF/A-1a（更严格的归档格式、嵌入全部字体、体积偏大）；没填 `format` 的 WEBP 转换从 PNG 变成 JPG（有损）。这是照 dev 抄的必然结果，在此明写，避免「只看到已对齐就以为行为没变」。回归锁 `DevParityConvertTests`（27 项**逐项序列**比对 + 5 项序列 + `JPG≠JPEG` 后缀），另保留 `OperationOptionKeyMappingTests` 的标签直传断言。对照明细见 `docs/dev-parity-convert.md` §3/§4 |
| `code.datamatrix` 的 OCR 分支（legacy「考试图片分类」`groupBox67`） | dev 用 `Sdcb.PaddleOCR` 对图片目录做「按 OCR 文字匹配归档 + 图上画框」（`Util.cs:6216-6280`，入口 `BodyForm.cs:7307-7346`）。**native 推理库既不在仓库、也不在 csproj 引用、也不在构建输出**：`paddle_inference_c.dll` 全盘扫（`%TEMP%`、用户目录、`Program Files`、`D:\sharp_project`）0 命中；dev 的 `bin\{Debug,Release,x64\Debug,X64_Release,x86\Debug,X86_Release}` 里只有托管 `Sdcb.PaddleInference.dll`(<0.1 MB) + 三个模型 DLL（20.8 / 132 / 122.6 MB）。真正的分发渠道是 `Resources.resx:223-225` 的网盘链接「OCR离线算法包的分享链接」（`BodyForm.cs:8658-8659` 打开） | **不迁**：OCR 不进主安装包；`code.datamatrix` 只保留「Word/PDF 逐页 DataMatrix 解码」。不含 OCR 的「图片目录批量分类」列为待迁（`docs/dev-parity-tools.md` §5 第 1 项，~1.5 人日，无新依赖） | 三条事实决定这个判断：① 光模型就 ≈275 MB（`Models.LocalV3.dll` 单文件 132 MB）；② native 运行时来自第三方网盘分享链接，我们既不掌握也不可验证（链接未下载）；③ **该能力在干净环境本就不可用**——首次 `PaddleOcrAll` 就会缺 `paddle_inference_c.dll`，迁它等于替 legacy 修一个从未真正交付的功能，风险与收益不成比例。若产品要恢复：先拿到并内化算法包，再按「可选组件」（缺包时该页禁用并提示）实现，见 `docs/dev-parity-tools.md` §2.1 |
> **授权口径（2026-11-05 定稿）**：本构建**不内置 Aspose 授权**——`assets\licenses` 不再随产物复制，运行时也不再读 `ASPOSE_LICENSE_PATH`，只保留「exe 目录 `licenses\*.lic` 若存在则被动加载」这一条路径。因此上表「错误消息规范化」一行里评估模式的实际文案已改为「本构建不内置 Aspose 授权（`assets\licenses` 已不再随程序分发），请把文件拆分后再试」，评估限制的识别与中文解释仍全部保留、见本节。发布与目录布局见 `docs\packaging.md`。

---

## 10. exam.report 批量参数（task-33）：已实现与保留缺口

legacy 唯一入口 `Util.ChangeExamReportParaContent`（`DocumentTools\Util.cs:864`，21 个参数）已拆成 Core 侧 `IExamReportService.ApplyDataFormat` / `ListReportItems` / `GetReportItemOrder` / `BuildClipboardText`（`src\DocumentTools.Core\Services\Exam\ExamReportService.cs`）。落库位置与 legacy 逐字对齐：`INSERT INTO Report_Detail (ReportID, Kind, Item, Content) VALUES (?, ?, ?, ?)`，每格 5 行（`GridFormatText` / `GridFormatFont` / `GridFormatTextAlign` / `GridFormatBorder` / `GridFormatData`），cell 键 `{行}|{列}`（legacy `Util.cs:953`/`1034`）。

### 10.1 本轮已实现

| 能力 | legacy 证据 | 本实现 | 备注 |
|---|---|---|---|
| 数据类型与身份 | 5 项下拉（`BodyForm.cs:1513`）+ `RaidInfo_Combox.SelectedValue` | `ExamReportParaRequest.DataFormatKind` + `Raid`；列模式 `Raid` 用 `\|` 拆多身份 | 多身份按身份逐行写 `GridFormatData` |
| 小数位数 | `Util.cs:914-942`：`"0"→999/ReserveDec=0` … `"5"→999.99%/5` | `ExamReportNumberFormat` 六值枚举 → `("999",0) … ("999.99%",5)` | 显示文本进 `GridFormatText`、`ReserveDec` 进 `Bool:…` 段，位置与 legacy 相同 |
| 项目排序 / 重置排序 | `BodyForm.cs:9949-9964`（`StrCmpLogicalW` 自然排序，重置用内存备份 `TempExamReportItemLists_NoSort`） | `GetReportItemOrder` 返回 `StoredOrder` + `SortedOrder`；UI 的「重置项目排序」用 `_storedItems` 备份还原 | legacy **不写库**（`Report_Detail` 无排序列，`tools\ExamFixture\ExamFixtureBuilder.cs:318`），故"写库前备份原顺序"落在 UI 内存，Core 只读不写序 |
| 复制到剪贴板 | `BodyForm.cs:7923-7965` 两处 `Clipboard.SetText(...)`，`\|` 拼接 | `BuildClipboardText(items)` 返回拼接串；`Clipboard.SetText` 留在 `ExamReportView.CopyItems` | Core 不碰 UI 剪贴板 |
| 字体（落库部分） | `ExamReportParaDataFormatFont_Box` 默认 `宋体,11,0`，提示"字体,大小,加粗"（`BodyForm.Designer.cs:12933`） | `FontSpec` → `GridFormatFont`；空或非三段 → `宋体,11,0` | legacy 兜底判断写错（`Util.cs:909-912` 只在段数≠3 时兜底，tooltip 却说"为空则默认"），本实现按 tooltip 语义补齐 |

### 10.2 保留缺口（只取证，不写实现）

| 缺口 | legacy 证据 | 难点 | 风险 | 下一步建议 |
|---|---|---|---|---|
| 插入相同项目 / 插入相同选项 | `BodyForm.Designer.cs:12776`/`:12787` 两个勾选 + `:13133` 说明；落到 `RowMode_InsertSameItemSelector` / `ColMode_InsertSameItemItem` | 语义只体现在去重口径（`Util.cs:1099-1116`）：true = 按 cell 键去重；false = 键+值双去重（不同身份**塌成一行**）。而 `GridFormatData` 的 `Raid=` 段目前是单身份 | 改口径会让同一份库在两种模式下渲染出不同报表；用户既有数据是按 legacy 口径写的 | 把布尔升级成显式 `RaidCollapseMode { PerRaid, Collapsed }`，默认 `PerRaid`（即当前行为），`Collapsed` 复刻 legacy 双去重；对两种口径各写一条 `Content` 串对拍测试 |
| X 行设为项目 | legacy 无独立控件（grep `设为项目` 0 命中）；能力由 `RowMode_ItemName` 逐次调用合成（`Util.cs:953`/`1034` 的 `Item=` 段） | 需要"一次调用写 N 行、每行不同项目"，现在 `Line` 是单值、`ItemNames` 是"同一行内多列"的列表 | 分隔符与对齐语义歧义（`\|` 还是 `,`；行数与项目数不等时怎么办），legacy 靠用户逐次点按钮绕开了这个问题 | 新增 `Lines`（`IReadOnlyList<int>`）+ `ItemsPerLine`，长度不等即抛中文 `ArgumentException`；UI 用 ListView 多选行 × 项目下拉，不新造分隔符 |
| 字体（UI 选择器） | 同上：文本框 + 三段式手工输入 | Windows 字体枚举（`InstalledFontCollection`）属表现层，Core 不该引入 UI 依赖 | 只做半套会出现"选了不生效"的假参数 | **已补齐（2026-11-06）**：`ExamReportView` 用字体 `ComboBox`（App 层 `InstalledFontCollection` 枚举，与 legacy 字体对话框同源）+ 字号 `TextBox` + 加粗 `CheckBox` 拼 `FontSpec`（`BuildFontSpec`）；字号非 1–200 数字时拦截并给中文提示；Core 的 `NormalizeFontSpec` 兜底不变 |
| 批量身份合计 | `Util.cs:4393-4411`：`合计` 行 Value = 全部身份用 `，`（Sqlite 用 `,`）连接；`批量设置全部身份` Value = `A\|B\|C\|` + 合计串 | 组合身份要展开成"每身份一行 + 一行合计"；合计串分隔符随 provider 变；`ListExamRaids` 已造好该 Value，但 `ApplyDataFormat` 未消费"末段是合计" | 把合计当普通身份写成 `Raid=A，B` 会让 legacy 报表渲染找不到身份；provider 判断错则合计永不生效 | **已补齐（2026-11-06，复核结论修正）**：`ApplyDataFormat` 的列模式本就按 `\|` 拆分（`ExamReportService.BuildCellTargets`，与 legacy `Util.cs:1153` 一致），`A\|B\|A，B` 会写成 3 行（逐身份 + 合计串一行）——**缺的只是 UI 入口**：批量参数卡新增身份下拉 `_bulkRaidPick`（`ExamReportView`，含「合计」「批量设置全部身份」），选中即回填文本框。回归锁 `ExamReportParaServiceTests.ApplyDataFormat_BatchAllRaidValue_WritesEachIdentityPlusTotalRow`。**Sqlite 分隔符差异不适用**：本实现只支持 Access（`AccessDb`），无 Sqlite provider |
| 项目顺序落库 | `Report_Detail` 无排序列（`ExamFixtureBuilder.cs:318`） | 要持久化必须加列或另建 `Report_ItemOrder` 表，而 legacy 报表渲染器（`VF_Report`）不会读新列 | 改表结构会破坏与 legacy 库的互操作（拿改过的库回 legacy 会用错顺序） | **不建议做**：维持"排序只影响当前会话与复制出的文本"，在 `ExamReportView` 用 Hint 明说 |

### 10.3 与 legacy 的有意偏差

> 两条均由 Lead 在 task-33 验收时批准（2026-11-05）：**不要以「legacy 不是这样」为由回退**。

| 项 | legacy 行为 | 本实现 | 理由（Lead 口径） |
|---|---|---|---|
| 重复执行的残留行 | 只有 sqlite 分支先 `DELETE FROM Report_Detail WHERE Item='…'`（`Util.cs:957-962`），Access 分支直接追加，同一格重复点会累积多行 | 有意修复：本实现两种 provider 都先按 `(ReportID, Item)` 清理旧行，可用 `ReplaceExistingRows = false` 回到旧行为 | legacy 在 Access 分支重复执行会累积重复格式行，这是 legacy 的缺陷、不是需要保持的行为；迁移的价值之一就是不让用户点两次得到两倍数据 |
| 多身份写库形态 | `Dictionary` 双去重后同键只保留首条（`Util.cs:1093-1138`），不同身份被塌成一行 | 有意偏差：按身份逐行写 `GridFormatData` | 折叠等于丢信息；若将来确实需要 legacy 折叠口径，用 `RaidCollapseMode { PerRaid, Collapsed }` 显式选择并做 Content 对拍——**本轮不做**（取证见 10.2 第 1 行） |

---

## 11. 相关文档

| 文档 | 用途 |
|------|------|
| [feature-ux-spec.md](feature-ux-spec.md) | 交互/布局/参数权威规范 |
| [architecture.md](architecture.md) | **分层 + 设计模式硬约束**（禁止瞎写） |
| [migration.md](migration.md) | 分支迁移状态摘要 |
