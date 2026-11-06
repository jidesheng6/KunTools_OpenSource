# 转换域 dev 对齐审计（task-38）

基准：`dev` 分支 tip **ce4cc88**（WinForms + 服务层重构分支，文件在 `DocumentTools/`）。
新版：`src\DocumentTools.Core\Services\Office\`、`src\DocumentTools.Core\Operations\`、`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs`。

**取证前提（重要）**：`git diff dev -- DocumentTools/` 输出为空 —— 工作树里的
`DocumentTools\Util.cs`、`DocumentTools\BodyForm.cs`、`DocumentTools\UI.cs`、`DocumentTools\BodyForm.Designer.cs`
与 `dev` 分支**逐字节相同**。因此下文「dev 证据」直接给 `dev:DocumentTools/...` 行号，这些行号在工作树里同样成立；
也说明此前 task-6（转换/合并/拆分/水印逐项复核）的结论对 dev 同样有效。

---

## 1. HTML→PDF

### 1.1 dev 侧事实

| 位置 | 事实 |
| --- | --- |
| `dev:DocumentTools/UI.cs:598-612` | 调用 `Util.ConvertHtml2PDFAsync(文件, 输出, chrome路径, 纸张索引, 横向, 用CSS, 上, 左, 下, 右边距, 超时, 等待超时, 等待全部资源)` |
| `dev:DocumentTools/BodyForm.cs:1305-1331` | 纸张下拉 11 项（A0…Tabloid），`SelectedIndex = 4` → **默认 A4**；方向下拉「竖向/横向」 |
| `dev:DocumentTools/BodyForm.Designer.cs:6496-6500` | `html2pdf_timeout.Value = 3`（秒）→ 传给 `page.DefaultTimeout` |
| `dev:DocumentTools/BodyForm.Designer.cs:6473-6477` | `html2pdf_wait_timeout.Value = 5`（秒）→ 传给 `page.DefaultNavigationTimeout` |
| `dev:DocumentTools/BodyForm.Designer.cs:6597/6615/6624` | 四边距文本 **"50"** |
| `dev:DocumentTools/BodyForm.Designer.cs:6697/6727` | 「等待所有页面元素加载完成」「使用内部CSS样式设置页面」两框**默认未勾** |
| `dev:DocumentTools/Util.cs:1692` | `if (!string.IsNullOrEmpty(ChromeDriverPath) && File.Exists(ChromeDriverPath))` —— **路径无效时整个方法静默返回，不产出文件也无提示**（外层 `if` 没有 `else`） |
| `dev:DocumentTools/Util.cs:1694-1699` | `LaunchOptions { Headless = true, IgnoreHTTPSErrors = true, ExecutablePath = chrome路径 }` |
| `dev:DocumentTools/Util.cs:1711-1719` | `WaitAllResouce` 为真 → `page.WaitForFrameAsync(page.Url)`；为假 → 设 `DefaultTimeout`/`DefaultNavigationTimeout` |
| `dev:DocumentTools/Util.cs:1720` | `page.GoToAsync(DocumentFileName)`（本地路径**不转 `file://`**，交给 Chromium 容错） |
| `dev:DocumentTools/Util.cs:1724-1738` | `PdfOptions.Format` 11 项映射，越界回 A4 |
| `dev:DocumentTools/Util.cs:1740-1760` | `DisplayHeaderFooter = false`、`Landscape = isLand`、`PrintBackground = true`、`PreferCSSPageSize = isUseCss`；**仅当不用 CSS 时**才设四边距 `MarginOptions`（`$"{px}px"`） |
| `dev:DocumentTools/PublicNoticeDialog.cs:20-36` | **WebView2 只用于公告弹窗**（`NoticeWeb.CoreWebView2.Navigate`），与 HTML→PDF 无关 |

> 关于 WebView2：`dev:DocumentTools/DocumentTools.csproj:584` 的 `Microsoft.Web.WebView2` 被 `PublicNoticeDialog`
> 使用；转换链路上没有任何 WebView2 代码。HTML→PDF 在 dev 里是 100% PuppeteerSharp（`csproj:635`）。

### 1.2 新版行为

| 位置 | 行为 |
| --- | --- |
| `src\DocumentTools.Core\Operations\HtmlToPdfOperation.cs:27-50` | 键映射：`chromePath`/`browserPath`（缺省自动探测 Chrome→Edge）、`paper`/`paperSize`、`landscape`、`useCss`/`preferCss`、`waitResources`、四条 `margin*`、`timeout`、`navTimeout` |
| `src\DocumentTools.Core\Services\Office\HtmlToPdfService.cs:27-38` | 浏览器路径空/不存在 → 抛中文「转换 HTML 失败：未找到可用的 Chrome/Edge 可执行文件（当前指向：…）。请安装 Chrome 或 Edge，或在选项里把「浏览器路径」(chromePath) 指向 chrome.exe。」 |
| `HtmlToPdfService.cs:46-54` | `LaunchOptions { Headless = true, AcceptInsecureCerts = true, ExecutablePath }` |
| `HtmlToPdfService.cs:58-62` | `if (!options.WaitAllResources)` 才设 `DefaultTimeout`/`DefaultNavigationTimeout`（与 dev 分支方向一致） |
| `HtmlToPdfService.cs:64-65, 90-100` | 本地路径 → `new Uri(...).AbsoluteUri`（`file://`），http/https/file 原样 |
| `HtmlToPdfService.cs:67-85` | `DisplayHeaderFooter = false`、`Landscape`、`PrintBackground = true`、`PreferCSSPageSize = useCss`；仅非 CSS 时设四边距 |
| `HtmlToPdfService.cs:102-117` | 纸张 0..10 → A0…Tabloid，越界 A4 |

### 1.3 逐项对照

| 项 | dev 证据 | 新版行为（src 行号） | 是否一致 |
| --- | --- | --- | --- |
| 引擎 | `Util.cs:1694-1706` PuppeteerSharp | `HtmlToPdfService.cs:53` PuppeteerSharp | ✅ 一致 |
| 无头模式 | `Util.cs:1696` Headless=true | `HtmlToPdfService.cs:48` | ✅ |
| 忽略 HTTPS 证书错误 | `Util.cs:1697` IgnoreHTTPSErrors | `HtmlToPdfService.cs:49` `AcceptInsecureCerts` | ✅（同一语义，PuppeteerSharp 20 的 API 名） |
| 纸张集合 | `Util.cs:1724-1738` 11 项 | `HtmlToPdfService.cs:102-117` 11 项 | ✅ |
| 纸张默认 | `BodyForm.cs:1325` 索引 4=A4 | `FeatureOptionSchemas.cs:443` 默认 4 | ✅ |
| 方向 | `Util.cs:1741` Landscape=isLand | `HtmlToPdfOperation.cs:40` | ✅ |
| 页眉页脚 | `Util.cs:1740` 恒 false | `HtmlToPdfService.cs:70` 恒 false | ✅（两侧都不提供开关） |
| 背景打印 | `Util.cs:1742` true | `HtmlToPdfService.cs:72` | ✅ |
| 缩放 | 两侧都未设置（Chromium 默认 1.0） | 无 `Scale` 赋值 | ✅（都不支持，属共同缺口） |
| 用 CSS 页面尺寸 | `Util.cs:1744-1747` | `HtmlToPdfService.cs:73` | ✅ 逻辑一致；**默认值本轮已修**（见 1.4-1） |
| 边距 | `Util.cs:1752-1760` 仅非 CSS 时生效；默认 50px | `HtmlToPdfService.cs:76-85` 同条件；默认**本轮改为 50** | ✅ 逻辑一致；**默认值本轮已修** |
| 超时 → DefaultTimeout | `Util.cs:1717` `DefaultTimeOut*1000` | `HtmlToPdfService.cs:60` | ✅ 映射一致 |
| 等待超时 → NavigationTimeout | `Util.cs:1718` `WaitTimeOut*1000` | `HtmlToPdfService.cs:61` | ✅ 映射一致 |
| 超时默认值 | 3s / 5s（`Designer.cs:6496/6473`） | 30s / 30s | ⚠️ **不一致，记录不改**（见 1.4-4） |
| 等待策略 | `Util.cs:1711-1714` `WaitForFrameAsync(page.Url)` | 不调用该分支 | ⚠️ **不一致，记录**（见 1.4-5） |
| 本地路径 | `Util.cs:1720` 直接 `GoToAsync(路径)` | `HtmlToPdfService.cs:90-100` 转 `file://` | ⚠️ 有意不同（见 1.4-6） |
| 无浏览器 | `Util.cs:1692` 静默不产出 | `HtmlToPdfService.cs:27-38` 抛中文 | ⚠️ 有意不同（见 1.4-7） |
| 输出目录 | dev 由 SaveFileDialog 保证 | `HtmlToPdfService.cs:40-44` 自动建目录 | ✅ 不影响结果 |

### 1.4 差异与本轮处置

1. **边距默认 0 → 50（已修）**：dev 面板默认四边距 50px；新版原为 "0"，同一份 HTML 出来的版心不同。
   已改 `FeatureOptionSchemas.cs:437-441`（四个 `Text(... "50" ...)`）与 `HtmlToPdfOperation.cs:44-47`（回退值 "50"）。
2. **`useCss` 默认 true → false（已修）**：dev 未勾。原默认 true 会让 `PreferCSSPageSize=true`，此时 dev 与新版都会**忽略边距**，
   与「默认 50px 边距」自相矛盾，等于把 dev 的默认输出改掉了。已改 `FeatureOptionSchemas.cs:447`。
3. **`waitResources` 默认 true → false（已修）**：dev 未勾。该开关决定「是否设置超时」这一反向分支，
   默认值不同会让超时设置分支与 dev 相反。已改 `FeatureOptionSchemas.cs:448`。
4. **超时默认 30s/30s（保留，记录理由）**：dev 是 3s/5s。实测口径上，3 秒对含外链图片/大表格的页面基本必然失败，
   而失败提示是 PuppeteerSharp 英文异常；此处不复刻 dev 的低可靠性默认值。用户仍可在选项里填 3/5 复现 dev 行为。
5. **`WaitForFrameAsync(page.Url)` 不复刻（记录理由）**：dev 在 `GoToAsync` **之前**调用它，此时 `page.Url` 还是
   `about:blank`，该分支实际是空操作；在 PuppeteerSharp 20 下等待不存在的 frame 会抛 `WaitTaskTimeoutException`。
   新版保持「等待全部资源时不额外设短超时」这一可见效果，不复制空调用。
6. **本地路径转 `file://`（保留，记录理由）**：dev 依赖 Chromium 自己把 `C:\a b\c.html` 当 URL 处理，
   含空格/中文时行为取决于版本；新版显式 `Uri.AbsoluteUri`，等价且稳定。
7. **无浏览器时抛中文（保留，记录理由）**：dev 静默返回，用户看到的是「点了没反应、也没有输出」。
   新版给可操作提示并自动探测 Chrome→Edge（`HtmlToPdfOperation.cs:27-33`）。

---

## 2. 本机 Office / WPS 分支

| 项 | dev 证据 | 新版行为（src 行号） | 是否一致 |
| --- | --- | --- | --- |
| Word COM 入口 | `dev:DocumentTools/Util.cs:2721` `new Word.Application()` | `OfficeConvertService.cs:144` `Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"))` | ✅（晚绑定，等价） |
| 打开文档与密码 | `Util.cs:2724-2725` `Documents.Open(..., PasswordDocument:)` | `OfficeConvertService.cs:149-155` 同名参数 | ✅ |
| 导出格式 | `Util.cs:2729` `wdExportFormatPDF`（=17） | `OfficeConvertService.cs:163` `ExportFormat: 17` | ✅ |
| 优化目标 | `Util.cs:2730` `wdExportOptimizeForPrint`（=0） | `OfficeConvertService.cs:165` `OptimizeFor: 0` | ✅ |
| 书签 | `Util.cs:2731` `wdExportCreateWordBookmarks`（=2） | `OfficeConvertService.cs:170` + 常量 `WordBookmarksExportMode`（`:34`，值 2） | ✅（并由 `WordToPdf_LocalOffice_ExportsOneBookmarkPerWordBookmark` 用真实 Word 断言 `Outlines.Count == 2`） |
| COM 释放 | `Util.cs:2737` 只释放 document，**application 未释放** | `OfficeConvertService.cs:194-195` 两者都释放 | ✅（修掉 dev 的句柄泄漏） |
| 弹窗干扰 | dev 未设 `Visible`/`DisplayAlerts` | `OfficeConvertService.cs:146-147` `Visible=false`、`DisplayAlerts=0` | ✅（更稳） |
| **WPS 互操作** | `dev:DocumentTools/DocumentTools.csproj:156-159` 引用 `Kingsoft.Office.Interop.Wpsapi`，但 **51 个 .cs 文件里零处调用** `Wpsapi`/`KWps`/`Kingsoft` API | 未引 WPS | ✅ **不需要补**（见下） |

### 2.1 WPS 结论（需要补 / 不需要补）

**不需要补**，依据四类证据：

1. **没有调用**：`git ls-tree -r dev -- DocumentTools/` 的 51 个 `.cs` 里检索 `Wpsapi|KWps|Kingsoft`，命中 0 处 API 调用；
   `.wps` 只作为**文件后缀**出现 —— `dev:DocumentTools/Util.cs:3618-3620`（`case ".wps"` 走 `Aspose.Words.Document` 取页数）、
   `dev:DocumentTools/UI.cs:189/210/218/221/227/230` 与 `dev:DocumentTools/Services/UIHelperService.cs:144-211`
   的文件过滤（`"*.doc;*.docx;*.wps"`）。
2. **WPS 在 dev 里是「修复建议」不是「转换引擎」**：`dev:DocumentTools/BodyForm.Designer.cs:1745`
   的文案是「如果本机Office出错，点击链接安装WPS进行修复」，点击走 `dev:DocumentTools/BodyForm.cs:7259`
   的 `Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_WPS"))` —— 打开下载页。
3. **本机导出只有一条路径**：`dev:DocumentTools/Util.cs:2717-2738` 是 `UseLocalOffice` 的唯一分支，ProgID 固定 `Word.Application`，
   没有 ProgID 探测、没有第二种引擎。
4. **`.wps` 文件的转换走 Aspose**：dev 里 `.wps` 与 `.doc/.docx` 并列进列表，转换时由 Aspose.Words 打开，
   与本机 Office 分支无关；新版 `word.pdf` 的关键选项 `useLocalOffice` 只影响「用不用本机 Word 导出 PDF」，
   对 `.wps` 输入的处理与 dev 相同（都交给 Aspose.Words）。

> 若将来要补 WPS 引擎，触发条件应当是「`Word.Application` 未注册且 `KWps.Application` 已注册」，
> 且导出参数需另测（WPS 的 `ExportAsFixedFormat` 参数兼容性未验证）。本轮无 dev 证据表明需要，故不实现。

---

## 3. PDF 版本转换（dev `TabPage_22` ↔ `pdf.compliance`）

| 项 | dev 证据 | 新版行为（src 行号） | 是否一致 |
| --- | --- | --- | --- |
| 版本来源 | `dev:DocumentTools/BodyForm.cs:1223` `DataSource = Enum.GetNames(typeof(Aspose.Pdf.PdfFormat))` → **27 项** | `FeatureOptionSchemas.cs:356-...` Choice 5 项（1.4/1.5/1.6/1.7/PDF-A-1b） | ⚠️ UI 子集（见 3.1） |
| 默认值 | 同上，`GetNames` 顺序第 0 项 = `PDF_A_1A` | 默认索引 3 = `1.7` → `v_1_7` | ⚠️ 不同（见 3.1） |
| 传参 | `dev:DocumentTools/UI.cs:533-541` `Enum.Parse(typeof(PdfFormat), 选中项)` → `ConvertPdfCompliance(..., PdfFormat)` | `OfficeConvertOperation.cs:171-174` + `MapPdfVersion`（`:328-336`，别名 1.4→v_1_4 … PDF/A-1b→PDF_A_1B，其余原样） | ✅ 能力等价 |
| 实现 | `dev:DocumentTools/Util.cs:2150-2159` `Document.Convert(stream, format, ConvertErrorAction.Delete)` → `Save` | `OfficeConvertService.cs:800` 同构（`Convert` + `Delete` → `Save`） | ✅ 逐项一致 |
| 失败文案 | dev 直接抛 Aspose 英文异常（`UI.cs` 无 try/catch 语义包装） | `OfficeConvertOperation` 的 `catch` → `OperationErrors.Translate`（评估模式/权限给中文） | ✅ 更好，不改变成功路径 |

### 3.1 差异与处置

- **能力层已等价**：27 个枚举名（含 `PDF_A_1A`、`v_1_3`、`ZUGFeRD`、`PDF_X_1A_2001`、`PDF_A_4F` 等）
  都能被 `ParsePdfFormat` 解析；回归测试 `OperationOptionKeyMappingTests.PdfCompliance_AcceptsEveryDevVersionName`
  （6 个代表值）与 `DevParityConvertTests.PdfCompliance_OffersEveryDevVersionInEnumOrder`（27 项逐个 `Enum.TryParse`）。
- **已修（UI 已扩到 dev 全量）**：`FeatureOptionSchemas.cs` 的 `pdf.compliance` 下拉由 5 项扩到 **27 项**，
  顺序与 `Enum.GetNames(typeof(Aspose.Pdf.PdfFormat))` 一致、默认第 0 项 `PDF_A_1A`；
  键缺失时的回退值也从 `"1.7"` 改为 `"PDF_A_1A"`（`OfficeConvertOperation.cs:172`）。
  逐项标签=枚举名（dev 亦是枚举名直接显示，`Enum.Parse` 回读）。

---

## 4. WEBP 格式转换（dev `TabPage_35` ↔ `webp.image`）

| 项 | dev 证据 | 新版行为（src 行号） | 是否一致 |
| --- | --- | --- | --- |
| 目标格式集合 | `dev:DocumentTools/BodyForm.cs:1423-1427` JPG/.jpg、JPEG/.jpeg、PNG/.png、BMP/.bmp、GIF/.gif（**5 项**） | `FeatureOptionSchemas.cs:496-508` PNG/JPG/BMP/GIF（**4 项**） | ⚠️ 少独立的 JPEG 项（见 4.1） |
| 默认格式 | 同上，`DataSource` 第 0 行 = JPG | 默认索引 0 = PNG | ⚠️ 不同（见 4.1） |
| JPEG 质量 | `dev:DocumentTools/Util.cs:2203/2206` `JpegOptions { Quality = 100 }` | `ImageConversionService.cs:144-156` `JpegQuality = 100` | ✅ 一致 |
| PNG | `Util.cs:2209` `PngOptions()`（默认 TruecolorWithAlpha） | `ImageConversionService.cs:150` `PngColor = TruecolorWithAlpha` | ✅ 一致 |
| BMP / GIF | `Util.cs:2212/2215` `BmpOptions`/`GifOptions` | 自研 BMP/GIF 编码器（SkiaSharp 无 TIFF/GIF） | ✅ 产物等价（GIF 为单帧 GIF89a） |
| 输出命名 | dev 由保存对话框给名（`UI.cs:644-645` 的 `ConvertedFileName`） | `ImageConversionService.cs:498` `{源名}_out{ext}` | ✅ 规则不同、来源一致（新版无对话框） |
| 失败文案 | dev 抛 Aspose.Imaging 英文异常 | `ImageConvertOperation.cs:56-66` → 中文（含输出目录建议） | ✅ 更好 |
| 后缀映射 | `Util.cs:2200-2219` 按扩展名分支，`.jpeg` → `.jpeg` | `NormalizeFormat`（`ImageConversionService.cs:494-507`）`jpg → .jpg`、`jpeg → .jpeg` | ✅ 一致（本轮修） |

### 4.1 差异与处置

- **已修（UI 已扩到 dev 全量）**：`FeatureOptionSchemas.cs` 的 `webp.image` 格式下拉扩为 dev 的 **5 项同序**
  `JPG / JPEG / PNG / BMP / GIF`、默认第 0 项 `JPG`；`NormalizeFormat` 拆出 `"jpeg" => ".jpeg"`，
  使 `JPG` 与 `JPEG` 两项产出不同后缀（dev 的 `BodyForm.cs:1420-1432` 两行数据本就只差后缀，`Util.cs:2200-2219`
  也按 `.jpg`/`.jpeg` 分别分支）。
- **回归锁**：`DevParityConvertTests.WebpImage_OffersDevFiveFormatsInPanelOrder`（项名/顺序/默认）、
  `DevParityConvertTests.WebpToImage_KeepsDevDistinctJpegExtension`（`JPG→_out.jpg`、`JPEG→_out.jpeg`）；
  `OperationOptionKeyMappingTests.WebpImage_PassesFormatLabelThrough` 继续保证标签直传。
- 附带确认：`image.compress` 的格式下拉本就含 `JPG` 与 `JPEG` 两项，拆分后缀后该面板同样与 dev 一致；
  既有 3 条用 `"JPG"` 的断言仍得到 `.jpg`，未受影响。

---

## 5. 其余转换功能（复核结论）

以下功能在 task-6 已按同一份 `DocumentTools/Util.cs` 逐项对照（本文件开头已证明该文件与 `dev` 逐字节相同），
逐项明细见 `docs/feature-gap.md` 第 1 节各行的备注，此处只给结论：

| dev TabPage | 功能 | 新版 | 结论 | 主要证据 |
| --- | --- | --- | --- | --- |
| TabPage_01 | WORD转PDF | `word.pdf` | 一致（含本机 Office 分支） | `Util.cs:2589-2739` ↔ `OfficeConvertService.cs:20-104` |
| TabPage_02 | PDF转WORD | `pdf.word` | 一致 | `Util.cs:2110-2142` ↔ `OfficeConvertService.cs:68-91` |
| TabPage_03 | EXCEL转PDF | `excel.pdf` | 一致 | `Util.cs:1571-1632` ↔ `OfficeConvertService.cs:128-199` |
| TabPage_07 | PDF转EXCEL | `pdf.excel` | 一致（同名同向） | `Util.cs:2059-2078` ↔ `OfficeConvertService.cs:204-218` |
| TabPage_20 / 21 | PPT转PDF / PDF转PPT | `ppt.pdf` / `pdf.ppt` | 一致（备注三个位置，`RightFull` 因 Aspose 无该枚举已收窄并抛中文） | `Util.cs:2170-2189` / `2088-2103` |
| TabPage_06 / 14 | 合并 PDF / 合并 WORD | `pdf.merge` / `word.merge` | 一致；**pdf.merge 有意不同**：非 PDF 输入显式报错（dev 静默丢弃） | `Util.cs:5066-5089` / `5097-5122` |
| TabPage_17 / 31 / 41 | 拆分 PDF / WORD / PPT | `pdf.split` / `word.split` / `ppt.split` | 一致；`deleteStart`/`deleteEnd` 独立键（dev 亦为独立控件）、ppt 尺寸/缩放已接 | `Util.cs:5736-5928` |
| TabPage_18 / 19 | 添加/移除 PDF 水印 | `pdf.watermark.add` / `.remove` | 一致（20 参数全覆盖；平铺改 stamp 复用，逐像素 0.000%） | `Util.cs:515-735` / `5316-5333` |
| TabPage_23 | 图片转PDF | `image.pdf` | 一致（方向/边距/自动优化/压缩已接） | `Util.cs:1805-1899` |
| TabPage_04 | WORD/PDF/EXCEL转图片 | `word.image` | 一致（`pageExport` 合并导出实现路径不同、结果等价） | `Util.cs:2330-2574` |
| TabPage_24 | 压缩PDF | `pdf.compress` | 一致（另加「压后不变大」回退，dev 没有） | `Util.cs:1460-1522` |
| TabPage_27 / 28 / 29 | WORD↔EPUB、EPUB→PDF | `word.epub` / `epub.word` / `epub.pdf` | 一致 | `Util.cs:2228-2293` / `1552-1564` / `1529-1545` |
| TabPage_15 / 16 | WORD转EXCEL / EXCEL转WORD | `word.excel` / `excel.word` | 一致 | `Util.cs:2300-2318` / `1639-1653` |

---

## 6. 本轮修复记录

| # | 文件 | 改动 | 理由 |
| --- | --- | --- | --- |
| 1 | `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:437-441` | html.pdf 四边距默认 `"0"` → `"50"` | 对齐 dev `BodyForm.Designer.cs:6597/6615/6624` |
| 2 | `FeatureOptionSchemas.cs:447` | `useCss` 默认 `true` → 未勾 | 对齐 dev（`Designer.cs:6727`），且避免「默认真值 = 忽略边距」与 dev 默认输出冲突 |
| 3 | `FeatureOptionSchemas.cs:448` | `waitResources` 默认 `true` → 未勾 | 对齐 dev（`Designer.cs:6697`）；该键决定是否设置超时的反向分支 |
| 4 | `src\DocumentTools.Core\Operations\HtmlToPdfOperation.cs:44-47` | 四边距回退值 `"0"` → `"50"`，并注明超时保持 30s 的原因 | 键缺失时也要与 dev 一致 |
| 5 | `tests\DocumentTools.Tests\OperationOptionKeyMappingTests.cs` | 新增 `PdfCompliance_AcceptsEveryDevVersionName`（6 例 Theory）、`WebpImage_PassesFormatLabelThrough` | 锁住 dev 下拉来源带来的「枚举名直传」「扩展名直传」两类输入 |
| 6 | `FeatureOptionSchemas.cs`（`pdf.compliance` 段） | 版本下拉 5 项 → **dev 的 27 项枚举名**、默认索引 3(`1.7`) → 0(`PDF_A_1A`) | dev `BodyForm.cs:1223` 直接用 `Enum.GetNames(typeof(Aspose.Pdf.PdfFormat))` |
| 7 | `FeatureOptionSchemas.cs`（`webp.image` 段） | 格式 4 项 → dev 的 5 项（补独立 `JPEG`）、默认 PNG → `JPG` | dev `BodyForm.cs:1420-1432` 数据行同序 |
| 8 | `OfficeConvertOperation.cs:171-175` | `pdf.compliance` 键缺失回退 `"1.7"` → `"PDF_A_1A"` | 与 dev 下拉第 0 项一致，避免「UI 默认 A1A、缺键却产出 1.7」 |
| 9 | `src\DocumentTools.Core\Services\Image\ImageConversionService.cs` | `NormalizeFormat` 拆 `jpg → .jpg` / `jpeg → .jpeg`，`Encode` 分支同步 | dev 的 JPG/JPEG 两行只差后缀（`Util.cs:2200-2219` 分 `.jpg`/`.jpeg` 两支） |
| 10 | `tests\DocumentTools.Tests\DevParityConvertTests.cs`（新增，4 Fact） | 27 项顺序/默认、webp 5 项顺序/默认、html.pdf 面板默认值、JPG/JPEG 后缀区分 | 把本轮 dev 对齐钉成回归断言 |

## 7. 未修项（需键表 owner 决定）

| 项 | 现状 | 结论 |
| --- | --- | --- |
| HTML→PDF 默认超时 30s / 等待超时 30s（dev 为 3s / 5s） | 有意不复刻：dev 的 3 秒对含外链资源的页面基本必失败，且只把 PuppeteerSharp 英文异常抛给用户；新版在选项里填 `3`/`5` 即可复现 dev 行为 | 保留（记录，不改） |
| `pdf.compliance` 下拉、`webp.image` 格式项 | 本轮已按 dev 扩到 27 项 / 5 项（见 §6 第 6~10 行），`DevParityConvertTests` 锁住 | 已修，无剩余 |

## 8. 遗留风险

- HTML→PDF 的 `waitResources=true` 分支两侧都没有「真正等待网络空闲」的实现（dev 是空调用，新版不设超时），
  重资源页面可能提前出图；如需根治应改 `WaitForNetworkIdleAsync`，属功能增强而非 dev 对齐。
- 缩放（Chromium `scale`）两侧都未暴露；dev 也没有对应控件，保持一致。
- 本机 Office 只在装了 Word 的机器上验证；WPS 路径未验证（结论是不需要）。
- 测试环境为 Aspose 评估模式：批量 >4 文件/集合受限，相关行为只能小规模实测。
