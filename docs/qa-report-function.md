# DocumentTools 功能端到端验收报告（测试 1：真实文档 + 真实转换）

- 生成时间：2026-11-06 01:00（本地；task-35 并入 P2-1/P2-7/P2-8 产物断言：§13；发布闸门六门全过、测试集串行化后的定稿跑：§14；task-46 并入 dev 对照回归与 UX 政策行为断言：§15；两条历史 SKIP 按实测翻 PASS、SKIP 归零：§16）
- 分支 / 提交：`feat/wpfui-migration` @ `ce4cc88`（工作树含未提交改动）
- SDK / TFM：.NET SDK 10.0.300，`net8.0-windows`
- harness：`tools\QaFunction\`（net8.0-windows、`UseWPF`，`ProjectReference` 引用 `src\DocumentTools.Core\DocumentTools.Core.csproj`、`src\DocumentTools.App\DocumentTools.App.csproj`（UX 政策要在真实控件上量）与 `tools\ExamFixture\ExamFixture.csproj`）
- 明细：`artifacts\qa-function\result.json`、`artifacts\qa-function\result.md`（每次运行自动重写，输出目录会先清空）

**目录**

1. [结论摘要](#1-结论摘要)
2. [Aspose 真实转换能力探测（结论先行）](#2-aspose-真实转换能力探测结论先行)
3. [环境限制：无 license 的评估模式（非产品缺陷）](#3-环境限制无-license-的评估模式非产品缺陷)
4. [夹具清单与再生方式](#4-夹具清单与再生方式)
5. [harness 用法与判定规则](#5-harness-用法与判定规则)
6. [必测 16 项结果（每项附最小证据）](#6-必测-16-项结果每项附最小证据)
7. [追加观察项（非 16 项清单）](#7-追加观察项非-16-项清单)
8. [缺陷清单与修复状态](#8-缺陷清单与修复状态)
9. [复现步骤](#9-复现步骤)
10. [残余局限与待确认项](#10-残余局限与待确认项)
11. [第二轮扩项：姓名纵排 / 考评 / 身份证 / 扫描 / 系统](#11-第二轮扩项姓名纵排--考评--身份证--扫描--系统)
12. [TIF / GIF 产物覆盖（task-27，随 task-20 收口并入矩阵）](#12-tif--gif-产物覆盖task-27随-task-20-收口并入矩阵)
13. [P2-1 / P2-7 / P2-8 产物断言（task-35）](#13-p2-1--p2-7--p2-8-产物断言task-35)
14. [收口验证记录（发布闸门）](#14-收口验证记录发布闸门)
15. [dev 对照回归与 UX 政策行为断言（task-46）](#15-dev-对照回归与-ux-政策行为断言task-46)
16. [SKIP 归零复核与门 2 白名单（2026-11-06）](#16-skip-归零复核与门-2-白名单2026-11-06)

## 1. 结论摘要

**能真转。** 16 项必测功能全部用真实文档（DOCX/XLSX/PPTX/PDF/PNG/JPG/HTML/EPUB）跑通真实 Aspose 转换，必测 **16 PASS / 0 FAIL**，追加 4 项 **4 PASS / 0 FAIL**，`QaFunction.exe --bonus` 退出码 `0`（退出码语义 = 失败数）。

第二轮扩项（姓名纵排 / 考评 / 身份证 / 扫描 / 系统）连同 TIF/GIF、条码外观、`image.compress` 细节、`ppt.pdf` 备注位置、dev 对照回归与 UX 政策行为断言共 **42 项扩展用例：42 PASS / 0 FAIL / 0 SKIP**，同一命令、同一退出码 `0`（两条历史 SKIP 已于 2026-11-06 00:58 按实测复核后翻成 PASS，见第 16 节；**不是**靠放宽判定换来的绿色）。其中考评 7 项全部跑在 `tools\ExamFixture` 生成的合成库上（含 `exam.package.reset` 的「清空数据包」路径），并用它的 `DOCUMENTTOOLS_SIMULATE_NO_ACE=1` 通道在独立进程里验证了「无 ACE 环境不崩溃、给可执行文案」（`exam.no-ace`）。`ppt.pdf` 备注位置一项在缺陷 C 修复后改为**「拒绝而不是静默」**断言：不支持的档位必须抛「不支持的备注导出位置」且不落产物，实测 Schema 只剩 `None` / `BottomFull` / `BottomTruncated` 三档、`RightFull` 被拒绝且写出 0 字节（见第 8 节与第 13 节）。逐项证据见第 11 节、第 12 节与第 13 节。

本报告同时记录三条在本轮测试中发现的缺陷，均已完成修复并在修复后复验：

- 缺陷 A：`excel.pdf` 勾选「每个工作表单独成文件」时崩溃（`A workbook must contain at least a visible worksheet`），且返回的输出路径不存在。
- 缺陷 B：`pdf.watermark.add` 使用默认字体「宋体」直接失败（`Font 宋体 was not found`），产出为空。
- 缺陷 C：`ppt.pdf` 的 `exportNotes=RightFull` 会静默丢备注（Schema 提供了 Aspose.Slides 25.12 并不支持的档位）。修复方式是**收窄 Schema + 失败显式化**，不是继续兜底。

三条缺陷的定位证据、最小修法与修复后实测见第 8 节。除这三项外，其余功能在本次运行中没有发现功能性失败；体积类观察（`pdf.compress`、`image.compress` 输出变大）已在第 3.1 节给出结论式判定（低熵夹具所致，非缺陷），`pdf.watermark.add` 的体积/耗时属第 3.2 节的已知性能特征。

**TIF/GIF 产物两项已随 task-20 收口并入矩阵**（`image.convert.tif` / `image.convert.gif`，属扩展区）：断言不止「扩展名对了」，而是魔数 + 解码 + 取色三重校验，专门防「静默回退成 PNG 却报成功」。逐项证据与校验口径（含为什么 TIF 不用 `SKBitmap.Decode`）见第 12 节。

## 2. Aspose 真实转换能力探测（结论先行）

探测由 harness 启动时打印（`PrintEnvironmentProbe()`），每次运行都会输出，可复现：

```text
hookDll             : libs/Aspose.Hook/Aspose.Hook.dll exists=True size=32256
licenseFiles(.lic)  : 0 in assets/licenses
ASPOSE_LICENSE_PATH : (null)
AsposeLicenseLoader : IsLicensed=False path=(none)
Aspose.Words        : 26.1.0.0
Aspose.Pdf          : 25.12.0.0
Aspose.Cells        : 25.12.0.0
Aspose.Slides       : 25.12.0.0
Aspose.BarCode      : 25.3.0.0
Aspose.Html         : 25.12.0.0
operations(registered): 28
```

事实与含义：

1. `src\DocumentTools.Core\DocumentTools.Core.csproj` 直接引用 Aspose.Words/Pdf/Cells/Slides/BarCode/Html 六个包，所以只要 `ProjectReference` Core 就能调用真实 Aspose 引擎，**不需要** WPF 外壳、不需要 `Aspose.Hook.dll` 也能量测转换能力。`operations(registered)=28` 说明 DI（`AddDocumentToolsCore()`）解析出的 `IDocumentOperation` 全部就位。
2. `libs\Aspose.Hook\Aspose.Hook.dll` 存在（32256 B），但 `assets\licenses\` 下 **0 个 `.lic`**、环境变量 `ASPOSE_LICENSE_PATH` 未设置，`AsposeLicenseLoader.TryLoad()` 返回 `IsLicensed=False`。
3. 因此本次全部转换运行在 **Aspose 评估模式**：能真转、结果可打开、文本层可提取，但受评估限制（见第 3 节）。这一点是本报告所有结论的前提，也是唯一无法由代码侧消除的外因。
4. 真实产物证据（不是"接口返回成功"就算数）：PDF 用页数 + 文本层提取双重校验（`FileProbe.PdfPageCount` + `PdfTextProbe.Extract`），Office 产物用 ZIP 条目 + XML 文本校验（`FileProbe.IsOfficePackage` / `ReadZipText`），图片用 PNG 头 + IHDR 尺寸校验，条码用 PNG 头 + 像素尺寸校验。例如 `word.pdf` 输出 77188 B、3 页、提取文本含 `DocumentTools`；`pdf.word` 回读 DOCX 得 16 个 `<w:p>` 段落且文本保留。

## 3. 环境限制：无 license 的评估模式（非产品缺陷）

**依据**：第 2 节四行探测结果（0 个 `.lic`、`ASPOSE_LICENSE_PATH` 未设、`AsposeLicenseLoader.IsLicensed=False`、六包版本齐全）。评估模式的核心限制是「文档内元素数量上限」，实测报错原文：

```text
At most 4 elements (for any collection) can be viewed in evaluation mode.
```

**受影响的用例与处理**：

| 用例 | 评估模式下的表现 | 本次处理 |
| --- | --- | --- |
| `pdf.merge`（3 页 + 3 页） | 直接失败，报上面那条 4 元素限制；产出 0 文件 | 改用 2+2 页夹具（`testdata\pdf\merge_a.pdf` + `merge_b.pdf`，共 4 页 = 上限内）验证合并逻辑本身，见第 7 节 |
| `word.pdf` / `html.pdf` | 能转，但输出带 Aspose 评估水印 | 判定"转换成功 + 文本层存在"，另记录 `evalWatermark=True` |
| 大文档 / 多页批处理 | 未验证 | 需要在有 `.lic` 的环境复跑；harness 无需改动 |

**结论**：`>4 元素` 属环境限制，不计入产品缺陷；但「`pdf.merge` 合并逻辑是否正确」已通过 2+2 页夹具独立证明（4 页、两个来源内容都在、顺序 A→B），所以这条限制没有吃掉合并功能的验证结论。

### 3.1 压缩比不具代表性（结论性说明，勿当缺陷追查）

`pdf.compress` 66.9 KB → 90.0 KB（134.6%）、`image.compress` 13.7 KB → 19.4 KB（141.4%）这两个数字**不代表压缩功能有缺陷**：本机夹具全部是 LibreOffice / Pillow 直接产出的低熵文件（无扫描噪点、无照片纹理、字体与图像已高度压缩），可压缩空间本就极小，重写容器与重采样反而带来额外开销。判定依据始终是页数、结构与签名；**扫描件 / 照片类样本的压缩比未验证**，需要另备高熵夹具（如 300dpi 扫描 PDF、手机照片 PNG）才能度量真实压缩率。后续若看到这两个百分比，请按「夹具特性」处理，不要当成回归缺陷。

**task-29 之后的实际行为（用户侧已不会拿到变大的文件）**：`pdf.compress` 会先把产物写到临时文件并比较字节数，**产物 ≥ 原始就回退原始内容**，结果对象 `PdfCompressionResult.KeptOriginal = true`，操作提示追加「其中 N 个文件无可压缩空间，已保留原始内容（…）」。实测：本仓库 `testdata\pdf\sample.pdf` 68466 B → **68466 B（回退命中，ratio 1.000）**；高熵噪声扫描件 461682 B → **255464 B（0.553，真实压缩）**。也就是说低熵夹具的 134.6% 已被回退策略吸收，只有确实能压小的文件才会被改写。

### 3.2 已知性能特征：平铺水印的 artifact 数量与耗时（已优化，留档对比）

`pdf.watermark.add` 在默认 25pt 间距下会对每页铺满约 816 个 artifact（A4），耗时曾经是这条路径的绝对瓶颈。task-29 把「每格 `new WatermarkArtifact` + `Artifacts.Add`」改成「复用一个 `TextStamp`、只改 `XIndent`/`YIndent`」后实测（夹具：`%TEMP%\dt-verify-data\four.pdf`，4 页 A4、文本「QA 水印」、间距 25pt、rotate 45°、opacity 0.5、宋体/SimSun）：

| 指标 | 优化前（每格 artifact） | 优化后（复用 stamp） | 比值 |
| --- | --- | --- | --- |
| 4 页总耗时 | 39 904 ms | 1 248–1 675 ms | ≈24–32× |
| 输出体积 | 2 420 278 B | 1 134 100 B | ≈0.47× |
| artifact 计数 | 3264 | 3264 | 1.00 |
| 逐页渲染差异 | 基准 | 0.000% × 4 页 | 等价 |
| 10 页外推 | ≈78 s | ≈3.0–3.6 s | 满足 ≤5 s 目标 |

`Artifacts.Add` 单次约 9.6 ms 且随格数超线性增长（单页 816 格就 8 s），而复用一个 stamp 每格约 0.4 ms；`TextStamp` 的旋转支点比 `WatermarkArtifact` 高 **1.1×字号**（8/12/24/36/100/200pt × SimSun/SimHei/Microsoft YaHei/Arial/Times New Roman 全部实测 1.1，与字族无关），所以 `XIndent` 要减去 `1.1×字号×sin(旋转角)` 才能与旧输出重合——忽略它会让整片水印沿 x 平移（25pt 间距下渲染差异 41%）。

仍存在的体积代价与「CJK 字库全量嵌入」相关：3 页真实文档产出的 `sample_水印.pdf` 为 1.79 MB，与本次提速无关（字体子集化仍是后续可选优化项，见第 3 节缺陷记录）。

**用户在意体积/耗时的现成规避方式**（不需要改代码）：①在「字体」里显式选系统已安装的字体名（如 `SimSun` / `SimHei` / `Microsoft YaHei` / `Arial`），修复后这些名字都能正常解析；②水印内容用英文或符号而不含中文，就不会触发 CJK 字库嵌入；③对体积极敏感的场景，先用 `pdf.watermark.add` 生成带水印文件再走 `pdf.compress`（两者可串联）。反之，若用户选的是系统未安装的字体名，Aspose 仍会报 `Font ... was not found`——这属于输入问题，不是缺陷。

### 3.3 校验工具链限制：SkiaSharp 3.119 没有 TIFF 解码器（非产品缺陷）

TIF 产物不能用 `SKBitmap.Decode` 校验——不是产物坏了，是 SkiaSharp 根本没有 TIFF 解码器。取证（`%TEMP%\dt-tiff-probe`，独立工程引用 harness 输出目录里的 `SkiaSharp.dll` + `libSkiaSharp.dll`）：

| 输入 | `SKData` | `SKCodec.EncodedFormat` | `SKBitmap.Decode` |
| --- | --- | --- | --- |
| Pillow 生成的参考 TIFF（`compression="raw"`，18572 B） | 加载成功 | `null`（格式不支持） | `null` |
| 本产品输出的 `tifgif_src_out.tif`（18624 B） | 加载成功 | `null`（格式不支持） | `null` |
| `testdata\image\tifgif_src.png`（216 B） | 加载成功 | `Png` | 96x64，像素正确 |

连第三方参考实现的 TIFF 都解不出来，说明是解码器缺失而非编码器问题；同一份产物用 Pillow（`Image.open` → `TIFF (96, 64) RGB`，像素 `(203, 52, 61)`）与 GDI+（`System.Drawing.Bitmap`）都能正常读回。因此 harness 的 TIF/GIF 校验统一用 **GDI+** 解码（`new Bitmap(path)`），并把 `decode=gdi+` 记进结果行；PNG/JPEG 类断言仍可用 SkiaSharp。GIF 不受影响（Skia 有 GIF 解码器）。

## 4. 夹具清单与再生方式

全部夹具位于 `testdata\`，单文件均 < 200 KB（最大 `sample.pdf` 68466 B）。Office 文档用 python-docx / openpyxl / python-pptx 生成，PDF 用 LibreOffice Kit 从 DOCX 导出，EPUB 按 EPUB 3 规范组装（LibreOffice Kit 不支持 `docx → epub`，返回 `unsupported-format`），**没有任何手写 zip / 手拼 OOXML**。

| 夹具 | 字节 | 用途 | 生成脚本 |
| --- | --- | --- | --- |
| `testdata\word\sample.docx` | 37165 | 3 页 DOCX：标题、中英正文、`{{NAME}}`/`{{DATE}}` 占位符、3x3 表格、两处分页符 | `tools\QaFunction\fixtures\make_docx.py` |
| `testdata\word\sample2.docx` | 36707 | 合并用第二份 DOCX（含「合并附录」） | 同上 |
| `testdata\word\merge_a.docx` / `merge_b.docx` | 36760 / 36760 | 2 页 DOCX，带 ASCII 标记 `MERGE-A` / `MERGE-B` | `tools\QaFunction\fixtures\make_merge_pdf.py` |
| `testdata\excel\sample.xlsx` | 5798 | 两个工作表「成绩」「汇总」，含公式 `=ROUND(AVERAGE(C2:C5),2)` | `tools\QaFunction\fixtures\make_xlsx.py` |
| `testdata\ppt\sample.pptx` | 35791 | 2 页 PPTX（13.333×7.5 英寸），含原生图表 | `tools\QaFunction\fixtures\make_pptx.py` |
| `testdata\image\sample.png` / `sample.jpg` | 14055 / 25278 | 800×600 测试图（PNG 无压缩、JPEG q88） | `tools\QaFunction\fixtures\make_images.py` |
| `testdata\image\tifgif_src.png` | 216 | 96×64、4 色块 + 深色外框：TIF/GIF 编码校验用（取 (8,8) 处 `#CB343D` 比色，容差 8/通道） | `tools\QaFunction\fixtures\make_tifgif.py` |
| `testdata\exam\circle_sheet.png` | 27735 | 900×1200，规则 5×5 圆圈（半径 26，中心 x=140+150i / y=240+180j），预期 25 个匹配 | 同上（同时输出 `circle_sheet.json`） |
| `testdata\exam\circle_sheet.json` | 1184 | 记录模板矩形 `{left:114,top:214,width:52,height:52}` 与 `expectedMatches=25` | 同上 |
| `testdata\html\sample.html` | 1289 | 中文 HTML + 表格 + 内联 CSS | `tools\QaFunction\fixtures\make_html.py` |
| `testdata\epub\sample.epub` | 1673 | 合法 EPUB 3（`mimetype` 为 STORED 首条目 + `container.xml` + `content.opf` + 章节） | `tools\QaFunction\fixtures\make_epub.py` |
| `testdata\pdf\sample.pdf` | 68466 | 3 页 PDF（由 `word\sample.docx` 经 LibreOffice 导出，`%PDF-1.7`） | `tools\QaFunction\fixtures\make_pdf.ps1` |
| `testdata\pdf\merge_a.pdf` / `merge_b.pdf` | 57040 / 57022 | 2 页 + 2 页，合并用例（评估上限 4 元素内） | `tools\QaFunction\fixtures\make_merge_pdf.py` |
| `testdata\name\table_copy.docx` | 36932 | 3×4 Word 表格（姓名），作为「表格粘贴文本」的真实来源 | `tools\QaFunction\fixtures\make_name.py` |
| `testdata\name\table_tabs.txt` | 115 | 上表复制产物原样（9 个 TAB / 3 行），走 legacy 表格分支 | 同上 |
| `testdata\name\table_copy_crlf.txt` | 115 | 同表的 Windows 复制形态：`\r\n` 行尾 + TAB 混合，验证「行尾先归一、再 TAB→换行」的顺序 | 同上 |
| `testdata\name\numbers_and_brackets.txt` | 87 | 7 行：数字 + 半角/全角括号 + 首尾空白 | 同上 |
| `testdata\name\duplicates_and_two_chars.txt` | 70 | 8 行：重名（张三/李四/周八）+ 2 字名（双字对齐） | 同上 |
| `testdata\exam\person_template.docx` | 37361 | 单人文档模板：含内联图片，`docPr descr="照片"`（占位图 200×260，与替换图尺寸不同以便判定） | `tools\QaFunction\fixtures\make_exam.py` |
| `testdata\exam\person_photo.png` | 879 | 替换用照片 240×300 纯色 PNG | 同上 |
| `testdata\ppt\notes.pptx` | 35122 | 2 页 16:9，备注页分别写 `NOTES-MARKER-ALPHA` / `NOTES-MARKER-BETA`（正文不含标记），用于验证 `exportNotes` 各档 | `tools\QaFunction\fixtures\make_pptx_notes.py` |
| `testdata\image\bmp_src.png` | 135 | 32×32 三条纯色带，用于 `bmpPixelBits` 位深校验（8/24/32 位各转一次） | `tools\QaFunction\fixtures\make_bmp_src.py` |

再生（python 为仓库外部 bundled 解释器，LibreOffice Kit 由 node 驱动；路径见各脚本默认值，可用 `--node/--cli` 覆盖）：

```powershell
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_docx.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_xlsx.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_pptx.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_images.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_html.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_epub.py
powershell -ExecutionPolicy Bypass -File tools\QaFunction\fixtures\make_pdf.ps1
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_merge_pdf.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_name.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_exam.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_tifgif.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_pptx_notes.py
& "C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe" tools\QaFunction\fixtures\make_bmp_src.py
```

## 5. harness 用法与判定规则

```powershell
dotnet build tools\QaFunction\QaFunction.csproj -v q --nologo
& .\tools\QaFunction\bin\Debug\net8.0-windows\QaFunction.exe            # 16 项必测 + 30 项扩展
& .\tools\QaFunction\bin\Debug\net8.0-windows\QaFunction.exe --bonus    # 追加 4 项观察
& .\tools\QaFunction\bin\Debug\net8.0-windows\QaFunction.exe --filter "word.pdf,excel.pdf"
& .\tools\QaFunction\bin\Debug\net8.0-windows\QaFunction.exe --filter "name.vertical.*"   # 支持前缀通配
& .\tools\QaFunction\bin\Debug\net8.0-windows\QaFunction.exe --filter "image.convert.*"   # TIF/GIF 产物两项
& .\tools\QaFunction\bin\Debug\net8.0-windows\QaFunction.exe --font-probe   # 字体族名解析探针
```

- **退出码 = 失败数**（必测 + 扩展 + 追加三段之和），可重复运行：每次运行先清空 `artifacts\qa-function\<featureId>`，再重建输出，不存在"读到上次产物"的假阳性。
- 三种状态：`[PASS]` / `[FAIL]` / `[SKIP]`。`[SKIP]` 表示**确实没验证到**，既不计 PASS 也不计 FAIL（因此不污染退出码）。当前唯一的 SKIP 场合是缺 ACE 通道的 `exam.*`（本机有 ACE，这类 SKIP 不出现）；此前「库能力不支持」的占位项（`ppt.pdf.exportNotes` 的 `RightFull`）在缺陷 C 修复后不再是 SKIP，而是转成**拒绝断言**——不支持的值必须失败且不落产物，见第 13 节。汇总行会把 SKIP 单独列出。
- 每项输出一行：`[PASS|FAIL] 功能 | in=… | out=… | 字节 | 耗时 ms | 详情`，同时写 `result.json` / `result.md`；结果按「必测 / 扩展（第二轮：姓名/考评/工具/系统）/ 追加观察项」三段汇总。
- 缺 ACE/Jet 通道时 `exam.*` 打印 `[SKIP] 跳过（无 ACE 通道）` 并写清原因（不抛异常、不算失败）；`--child-no-ace` 是 `exam.no-ace` 内部使用的子进程开关（强制只跑 `exam.*`、且不清空 artifacts），正常使用不需要手写。
- 判定不只信任 `DocumentOperationResult.Success`：`Success=true` 之后仍会打开产物校验（PDF 页数 + 文本层、Office ZIP 条目 + XML 文本、PNG/JPEG 头 + 尺寸、条码像素尺寸），**输出路径不存在直接判 FAIL**（缺陷 A 就是这样暴露的）。
- 诊断通道：`TryRun()` 把单次试验跑进 `artifacts\qa-function\_diag\<tag>`，只打印 `↳ 诊断：…`，不参与 PASS/FAIL 矩阵。

### 5.1 方法学：三处反直觉的断言选择（都是实测逼出来的）

写产物断言时，最贵的错误不是断言太严，而是**断言了一个测不到的量**。这一轮有四处是被实测推翻后改写的，记下来免得以后重犯：

1. **方向别用「哪条边更黑」，用「哪个角是空的」**。条码 `rotate` 最初用 caption 造成的底边文字带去判方向，实测 `caption=below` 时顶边墨量 9934、底边 751 —— 完全反向。原因是 Code128 是满幅条状图形，四条边都黑，任何"边带灰度差"都会被条码本身淹没。改用 QR 的定位角：QR 只有左上/右上/左下三个角有 7 模块实心块、右下角为空，「空角位置」就是方向（0°→右下、90°→左下、180°→左上、270°→右上）。四张图的角墨量实测分别为 0.42/0.41/0.38/0.41 三组轮转，与预期一一吻合。**教训：找结构性特征（缺失/存在），别找统计性特征（多少）。**
2. **行数是 Core 的实现细节，语义才是契约**。`exam.insert` 最初钉死「`Exam_Score` 恰好 3 行」，Core 侧补齐 legacy 选项后变成 5 行（项目 1 + 优秀 + 弃权 `Score=0` + 不计入得票率/无效 + 无效 `Score=-1`），断言立刻假红。改为语义断言：`Kind='项目'` 恰 1、`SelName='优秀'` ≥1、`SelName='弃权' AND Score=0` ≥1、`SelName='无效'` ≥1、`(Kind,ItemName,SelName)` 无重复组合；行数只进 detail。**教训：断言"必须存在什么"，而不是"必须只有几个"。**
3. **"不支持"也必须可测，且要断言它失败得干净**。缺陷 C 修复后，`exportNotes=RightFull` 的正确行为是**抛异常**，而不是产出降级文件。断言因此变成三件：失败消息含「不支持的备注导出位置」、`OutputFiles` 为空、目录里没有非空 PDF（判定口径：0 字节残留文件不算产物）。另外还断言 Schema 的档位列表里不再出现 `RightFull` —— 行为与面板的一致性两头都钉住。**教训：拒绝路径同样需要产物级断言，否则"静默降级"会以另一种形式回来。**
4. **体积/耗时不是判据**。`pdf.compress`（66.9→90.0 KB）、`image.compress`（13.7→19.4 KB）体积反涨是低熵夹具所致（第 3.1 节），判定始终落在页数、容器结构、文本层、图像尺寸、像素值上。**教训：能被"正确但奇怪"解释的数字，不能当回归信号。**

## 6. 必测 16 项结果（每项附最小证据）

| # | 功能 | 结果 | 输入 | 产物 / 字节 | 耗时 | 可复核的凭据 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `word.pdf` | PASS | `testdata\word\sample.docx` | `sample.pdf` 77188 B | 1541 ms | 3 页；提取文本含 `DocumentTools`；`evalWatermark=True` |
| 2 | `pdf.word` | PASS | `testdata\pdf\sample.pdf` | `sample.docx` 11860 B | 1390 ms | DOCX 含 `word/document.xml`；16 个 `<w:p>`；正文文本保留 |
| 3 | `excel.pdf` | PASS | `testdata\excel\sample.xlsx` | `sample_成绩.pdf; sample_汇总.pdf` 合计 74087 B | 884 ms | 2 个工作表各 1 个 PDF，每个 1 页；`message=完成 2 个文件。` |
| 4 | `ppt.pdf` | PASS | `testdata\ppt\sample.pptx` | `sample.pdf` 68416 B | 1389 ms | 2 页（与 2 页源一致） |
| 5 | `image.pdf` | PASS | `testdata\image\sample.png` + `sample.jpg` | `images.pdf` 112464 B | 596 ms | 2 张图 → 2 页 |
| 6 | `pdf.excel` | PASS | `testdata\pdf\sample.pdf` | `sample.xlsx` 6996 B | 353 ms | XLSX 含 `xl/workbook.xml`；1 个工作表 |
| 7 | `word.image` | PASS | `testdata\word\sample.docx` | `sample_第1页.png` … `第3页.png` 合计 203635 B | 596 ms | PNG 数 = 3；首图 1275×1650 |
| 8 | `pdf.compress` | PASS | `testdata\pdf\sample.pdf` | `sample_压缩.pdf` 92177 B | 90 ms | 3 页；66.9 KB → 90.0 KB（体积变化见第 3.1 节） |
| 9 | `pdf.watermark.add` | PASS | `testdata\pdf\sample.pdf` | `sample_水印.pdf` 1876348 B | 26862 ms | 3 页；`watermarkTextFound=True`（能提取到水印文字） |
| 10 | `pdf.split` | PASS | `testdata\pdf\sample.pdf` | `sample_第1页.pdf` … `第3页.pdf` 合计 267947 B | 53 ms | 3 个分片，各 1 页 |
| 11 | `word.merge` | PASS | `testdata\word\sample.docx` + `sample2.docx` | `merged.docx` 22997 B | 341 ms | 产物含两份输入的文本（含「合并附录」） |
| 12 | `word.replace` | PASS | `testdata\word\sample.docx` | `sample_替换.docx` 35026 B | 171 ms | `{{NAME}}` 已替换为 `QA_REPLACED`；未参与替换的 `{{DATE}}` 保留 |
| 13 | `html.pdf` | PASS | `testdata\html\sample.html` | `sample.pdf` 63718 B | 974 ms | 1 页；文本层提取 312 字符（真实 Chromium 渲染） |
| 14 | `image.compress` | PASS | `testdata\image\sample.png` | `sample_out.png` 19870 B | 26 ms | PNG 800×600；13.7 KB → 19.4 KB（体积变化见第 3.1 节） |
| 15 | `code.barcode` | PASS | 内存生成（QR + Code128） | `barcode_qr.png` + `barcode_code128.png` 合计 27260 B | 666 ms | QR 163×190 / 8923 B（带下方文字）；Code128 415×172 / 18337 B |
| 16 | `exam.circle` | PASS | `testdata\exam\circle_sheet.png`（模板 52×52） | `circle_fixed.png` 35552 B | 306 ms | 原图 900×1200；25/25 匹配（`minSim=1.000`）；修复后尺寸不变 |

必测小计：**16 PASS / 0 FAIL**。

> 表中的字节数与耗时取自一次完整运行（多次运行实测一致，仅因 Aspose 写入时间戳、字体缓存冷热而有小幅波动，例如 `word.pdf` 在两次运行间为 75.4–77.2 KB、耗时 1.1–1.5 s）。断言与判定依据**不是**精确字节，而是页数、容器结构（ZIP 条目 / PDF 对象）、文本层提取结果与图像尺寸——所以 byte/ms 有小幅差异不影响结论，PASS/FAIL 也不会因它们翻转。

## 7. 追加观察项（非 16 项清单）

| 功能 | 结果 | 产物 / 字节 | 耗时 | 凭据 |
| --- | --- | --- | --- | --- |
| `word.epub` | PASS | `sample.epub` 100344 B | 394 ms | ZIP 容器 + `mimetype` 条目存在 |
| `pdf.merge` | PASS | `merged.pdf` 129980 B | 16 ms | 4 页（2+2）；`Extract(4)` 同时含 `MERGE-A`/`MERGE-B`；`Extract(1)` 含 `MERGE-A`（顺序 A→B） |
| `epub.word` | PASS | `sample.docx` 4737 B | 1149 ms | 含 `word/document.xml` |
| `epub.pdf` | PASS | `sample.pdf` 56261 B | 120 ms | 1 页 |

`pdf.merge` 的 3+3 页尝试作为环境限制现场证据保留在诊断通道：

```text
↳ 诊断：3+3 页合并（评估限制演示）success=False message=At most 4 elements (for any collection) can be viewed in evaluation mode.
```

## 8. 缺陷清单与修复状态

### 缺陷 A：`excel.pdf`「每个工作表单独成文件」崩溃 / 返回不存在的路径（已修复并复验）

1. `src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:138-156` 的 `ExcelToPdf` 在 eachSheet 分支按表循环，单段写法 `workbook.Worksheets[j].IsVisible = j == i;` 会在把第 1 个表隐藏的那一瞬间让工作簿出现 **0 个可见工作表**，Aspose.Cells 在该赋值处抛 `A workbook must contain at least a visible worksheet`。
2. 同函数把每个表存成 `<name>_<sheetName>.pdf` 后 `return`，从不创建 `<name>.pdf`；而 `src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:65-67` 无条件 `outputs.Add(output)`（`output` 是 `<name>.pdf`），于是 `Success=true` 但 `OutputFiles` 指向不存在的文件，`Message` 仍是「完成 1 个文件。」。
3. 独立复现：`%TEMP%\dt-cells-probe`（纯 Aspose.Cells 25.12.0，输入 `testdata\excel\sample.xlsx`）对四种策略实测——单段切换 FAIL（1 个 PDF）、先置 `Worksheets.ActiveSheetIndex` 再单段切换仍 FAIL、每表复制到新 Workbook OK（丢 PageSetup）、**两段式（先 `IsVisible=true` + `ActiveSheetIndex=i`，再隐藏 `j != i`）OK 且保留 PageSetup**。
4. 修复后状态（开发 3 实施）：eachSheet=true 产出 `sample_成绩.pdf; sample_汇总.pdf`，`message=完成 2 个文件。`；诊断通道三个组合（eachSheet 缺省 / `=true` / `=true+fitOnePage`）全部 `success=True`。

### 缺陷 B：`pdf.watermark.add` 默认字体「宋体」不可解析（已修复并复验）

1. 默认值链路：`src\DocumentTools.Core\Services\IPdfWatermarkService.cs:41` `public const string DefaultFontName = "宋体";` → `:61` `FontName = DefaultFontName`；`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:168` `Text("fontName", "字体", "宋体", 80)`；`src\DocumentTools.Core\Services\Office\PdfWatermarkService.cs:117` `Font = FontRepository.FindFont(options.FontName)`。
2. 现象：默认参数直接 `Success=false, Message=Font 宋体 was not found`，0 输出；显式传 `fontName=SimSun` 则成功。
3. 独立复现：`%TEMP%\dt-font-probe`（Aspose.PDF 25.12.0，`FontRepository.Sources.Count=2`）——`宋体`/`黑体`/`微软雅黑` 全部解析失败；`SimSun`/`SimHei`/`Microsoft YaHei`/`Arial`/`Times New Roman`/`Courier New` 全部成功；`TextStamp` 实测宋体抛 `FontNotFoundException`，SimSun 输出 161291 B。
4. 修复后状态（开发 3 实施：中文族名→英文族名映射 + 兜底 SimSun，保留中文默认值文案）：`pdf.watermark.add` 输出 `sample_水印.pdf` 1876348 B、3 页、`watermarkTextFound=True`；耗时 26.9–28.4 s、体积膨胀至 1.79 MB，原因是成功嵌入 CJK 字体（此前失败路径根本没产出），见第 3.2 节。

### 缺陷 C：`ppt.pdf` 的 `exportNotes=RightFull` 静默丢备注（已修复：Schema 收窄 + 失败显式化）

1. 原症状：Schema/面板承诺四档，`src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:217-237` 的 `MapNotesPosition` 把 `BottomFull` / `BottomTruncated` / `RightFull` 三个名字原样透传；`:118-123` 塞进 `PptToPdfOptions.NotesPosition`；`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs` 再用 `Enum.TryParse(options.NotesPosition, ignoreCase: true, out Aspose.Slides.Export.NotesPositions notes)` 决定是否设置 `NotesCommentsLayoutingOptions`——解析失败时整块被跳过。
2. 关键事实（取证依据，保留）：**Aspose.Slides 25.12 的 `NotesPositions` 只有 `None`、`BottomFull`、`BottomTruncated`**（`%TEMP%\dt-notes-probe` 反射枚举成员打印为此三者，程序集版本 25.12.0.0），`RightFull` 在类型层面就不存在。因此旧实现 `TryParse("RightFull")` 返回 `false`，PDF 按 `None` 导出——**不报错、不提示，备注消失**。这不是评估模式限制（同样无 license 的 `BottomFull` 工作正常）。
3. 修复前实测（保留了当时的反向证据）：`None`=2 页 960.0×540.0pt、无标记、382 字符；`BottomFull`=2 页 540.0×720.0pt、有标记、1223 字符；`BottomTruncated`=2 页 540.0×720.0pt、有标记；**`RightFull`=2 页 960.0×540.0pt、无标记、382 字符（与 `None` 完全一致）**。
4. 修复（两处，均由 lead 在 Core 侧实施）：
   - `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:55-65`：`PptNotePositions` 删除 `RightFull`，只留 `None` / `BottomFull` / `BottomTruncated`，注释写明「Aspose.Slides 25.12 的真实枚举能力」与「早先那档会让导出静默退回 None」。
   - `src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:339-357`：解析不出（或解析成 `None`）时抛 `InvalidOperationException`：`不支持的备注导出位置：「{值}」。Aspose.Slides 只支持 None / BottomFull / BottomTruncated。`，不再静默跳过。
5. 修复后验证（harness `ppt.pdf.exportNotes`，同一夹具、只改 `exportNotes` 一个键）：`None` / `BottomFull` / `BottomTruncated` 三档断言照旧 PASS；`RightFull` 现在断言**被拒绝**——`Success=false`、消息含「不支持的备注导出位置」、`OutputFiles` 为空、目录内非空 PDF 0 个 0 字节；同时断言 `FeatureOptionSchemas.GetSections("ppt.pdf")` 的 `exportNotes` 档位 === `None/BottomFull/BottomTruncated`（不含 `RightFull`）。该检查项由 `[SKIP]` 转为 `[PASS]`。

### 说明

本轮我按职责只做定位与报告，未改动 `src\`、`tests\` 下任何既有文件；缺陷 A、B 两处修复由开发 3 在其 scope 内实施，缺陷 C 由 lead 在 Core 侧实施（Schema 收窄 + 失败显式化），我在每次修复后复跑复验（第 6 节数据即修复后运行结果；缺陷 C 的拒绝断言见第 13 节）。

## 9. 复现步骤

```powershell
cd D:\sharp_project\source\repos\DocumentTools

# 1) 夹具（一次性；已生成则跳过）
#    见第 4 节命令

# 2) 构建 harness
dotnet build tools\QaFunction\QaFunction.csproj -v q --nologo

# 3) 全量运行（16 必测 + 30 扩展 + 4 追加），退出码应为 0
& .\tools\QaFunction\bin\Debug\net8.0-windows\QaFunction.exe --bonus
$LASTEXITCODE

# 4) 与报告比对：逐项一行，或直接看 artifacts\qa-function\result.md
Get-Content artifacts\qa-function\result.md -Encoding UTF8
```

抽查对照（报告 ↔ harness 输出；耗时逐次小幅波动——复跑常见 ±10%，例如 `excel.pdf` 651/734/753 ms、`exam.circle` 302/307/311 ms 都出现过，判定不看耗时；字节以当次 `result.md` 为准）：

| 报告项 | harness 行 |
| --- | --- |
| `excel.pdf` 两个产物、`message=完成 2 个文件。` | `[PASS] excel.pdf | … | 72.4 KB | 734 ms | pdf pages=1 (workbook sheets=2)` |
| `pdf.merge` 4 页、A→B 顺序 | `[PASS] pdf.merge | … | 126.9 KB | 24 ms | pdf pages=4 (2+2) bothSourcesKept=True order=AB` |
| `exam.circle` 25/25 匹配 | `[PASS] exam.circle | … | 34.7 KB | 307 ms | image=900x1200 matches=25/25 minSim=1.000` |
| `exam.no-ace` 无 ACE 降级 | `[PASS] exam.no-ace | … | 3.2 KB | 452 ms | exit=0 skipRows=ok unhandled=none` |
| `name.vertical.table` 双字对齐两空格 | `[PASS] name.vertical.table | … | names=12 aligned=「李  四」(2 空格)` |
| `name.vertical.table.crlf` 3 行→12 名、对齐行各 2 空格 | `[PASS] name.vertical.table.crlf | … | 2 ms | 夹具 3 行→12 名，与 LF 逐行一致；对齐行 5 个（各长度 4/空格恰 2）；无 \r 残留；竖排 41 行（一字一行）` |
| `exam.package.reset` 清空后计数归零 | `[PASS] exam.package.reset | … | 249 ms | resetRemoved=4 人员/单位/.exam/SelfReport=已清零 IsAlreadyInitialized=false→true 二次reset=0` |
| `image.convert.tif` TIF 魔数 + 解码 + 取色（第 12 节） | `[PASS] image.convert.tif | … | 17996 B | 7 ms | .tif magic=49 49 2A 00 decode=gdi+ 96x64 (8,8) 源 #CB343D → 产物 #CB343D（单通道最大色差 0）｜链路 image.compress format=TIF` |
| `image.convert.gif` GIF89a + 解码 + 取色（第 12 节） | `[PASS] image.convert.gif | … | 1232 B | 5 ms | .gif magic=47 49 46 38 39 61 decode=gdi+ 96x64 (8,8) 源 #CB343D → 产物 #CB343D（单通道最大色差 0）｜链路 image.compress format=GIF` |
| `code.barcode.appearance` fixed 像素尺寸 + rotate 宽高互换 + QR 空角方向（第 13 节） | `[PASS] code.barcode.appearance | … | 188.0 KB | 221 ms | sizeMode=fixed 520x180（auto=580x172）; rotate 90→180x520 180→520x180 270→180x520; QR 空角方向：0° 角墨量 TL/TR/BR/BL=0.42/0.41/0.38/0.41; 90° …=0.41/0.42/0.41/0.38; 180° …=0.38/0.41/0.42/0.41; 270° …=0.41/0.38/0.41/0.42` |
| `image.compress.bmpPixelBits` 直读 BMP 头 `biBitCount`（第 13 节） | `[PASS] image.compress.bmpPixelBits | … | 9.2 KB | 12 ms | 仅改 bmpPixelBits：8→8(2102 B) 24→24(3126 B) 32→32(4150 B)，biBitCount 直读 BMP 头 offset 28` |
| `ppt.pdf.exportNotes` 三档备注位置 + 不支持档位被拒绝（第 13 节） | `[PASS] ppt.pdf.exportNotes | … | 190.7 KB | 2492 ms | 仅改 exportNotes：None→无 NOTES-MARKER（2 页 960.0x540.0pt）; BottomFull→含 NOTES-MARKER-ALPHA（2 页 540.0x720.0pt）; BottomTruncated→含 NOTES-MARKER-ALPHA（2 页 540.0x720.0pt）; RightFull→被拒绝（「不支持的备注导出位置」，产物 0 个 / 0 B）; Schema 档位=None/BottomFull/BottomTruncated` |

## 10. 残余局限与待确认项

1. **评估模式**（第 3 节）导致的未验证面：>4 元素的大文档合并、大文档批处理、以及评估水印对输出视觉质量的影响。需在有 `.lic` 的环境复跑同一 harness（无需改代码）。
2. **`pdf.compress` 体积反增**：66.9 KB → 90.0 KB（134.6%），3 页一致、签名正确，判定依据是页数与结构不变，但「压缩」未体现为体积下降。可能原因：源 PDF 本身已是 LibreOffice 输出、图像重采样策略或 `webOptimize` 默认值。建议后续用"扫描版/大图 PDF"夹具单独度量压缩比，本条不作为本次失败项。
3. **`image.compress` 体积反增**：13.7 KB → 19.4 KB（141.4%），源图是 Pillow 生成的低熵 PNG，PNG 无格式损失压缩空间；同样建议补一张照片类 PNG 作对比样本。
4. **`pdf.watermark.add` 耗时 26.9–28.4 s、产物 1.79 MB**：源于首次成功嵌入 CJK 字体（缺陷 B 修复后的新路径）。功能正确，但性能与体积需要产品侧确认是否可接受（可考虑子集化字体）。
5. **`code.barcode` 无 `IDocumentOperation`**：`code.barcode` / `code.datamatrix` 直接以 `IBarcodeService` 调用验证，不经过 operation 分发层；若 UI 侧有独立调用链，需另测。
6. **`exam.*` / `name.vertical` / `idcard.check` / `twain.debug` / `system.*`**：已在第二轮扩项中纳入矩阵（第 11 节，23 项全 PASS，其中 `exam.package.reset` 覆盖数据包「清空」路径）。仍未覆盖：`system.*` 的**写操作**（第 11.5 节的「未验证」清单，本报告不为其背书）。
7. **`exportNotes` 的可选档位只到三档**（缺陷 C 已修复，第 8 节 / 第 13 节）：Aspose.Slides 25.12 的 `NotesPositions` 只有 `None`/`BottomFull`/`BottomTruncated`，Schema 已收窄到这三档，传入其它值会被显式拒绝（`不支持的备注导出位置：「…」…`）而不是静默降级。因此「右侧整版备注」这一能力**不存在**，不是未验证。
8. **夹具局限**：所有夹具均为脚本生成的小样本（< 70 KB），未覆盖真实业务文档的复杂版式、加密文档、超大页数文档。
9. **考评夹具是合成库**：`Exam_Base.mdb` 由 `tools\ExamFixture` 按源码里真实出现过的 SQL 建表，不含真实业务数据；`Exam_People` / `Unit_Ess` 在源码中只以 `SELECT *` 出现，列集以 legacy 语句为准，因此"真实库里还有别的列"这一情形未覆盖（合并逻辑对未知列是按源表列逐列搬，不受影响）。
10. **条码/图像细节的验证范围**：`sizeMode=fixed`、`rotate`、`useSourceExtension`、`noCompress`、`bmpPixelBits`、`exportNotes` 六项已按「同一输入只改一个键、产物必须不同」验证（第 13 节）；条码的其它外观键（`ResolutionDpi`、`CaptionFontSize`、`LogoFile`）尚无产物级断言。

## 11. 第二轮扩项：姓名纵排 / 考评 / 身份证 / 扫描 / 系统

命令：`QaFunction.exe --bonus`，本节这一组 23 项 **23 PASS / 0 FAIL**（扩展区当前共 30 项，另 5 项属第 13 节、2 项属第 12 节），退出码 `0`。所有断言直接打在 Core 服务契约上（`INameVerticalService`、`IExamInsertService`、`IExamReportService`、`IMss3VoteService`、`IVoteReportService`、`ISingleDocumentService`、`IExamPackageService`（含 `Reset`）、`ITwainScanService`、`IWindowsSystemToolsService`/`INetworkAdapterService`、`ChineseIdCard`、`AppSettings`），不经 UI（`name.vertical` 的 App 接线即 task-16）。

### 11.1 姓名纵排（`name.vertical.*`，10 项）

| 用例 | 输入 | 期望 | 实际 | 断言结果 |
| --- | --- | --- | --- | --- |
| `name.vertical.table` | `table_tabs.txt`（9 TAB / 3 行，含 `○` `£`） | 12 个名字；符号清空；两字名对齐成「李  四」；三字名不对齐 | names=12，`aligned=「李  四」`（长度 4、半角空格 2），`王小明` 未对齐，`○ 赵七`→`赵  七` | PASS |
| `name.vertical.table.crlf` | `table_copy_crlf.txt`（CRLF+TAB 混合；原样 3 行 × 每行 4 个单元格） | 3 行拆成 12 个名字，与 LF 版逐行等价；每个两字名各恰 2 个半角空格（长度 4）；行内无 `\r`；竖排一字一行 | 夹具 3 行→12 名，与 LF 逐行一致；对齐行 5 个（李四/钱八/孙九/周十/赵七，各长度 4、半角空格恰 2）；无 `\r` 残留；竖排 41 行 | PASS |
| `name.vertical.clean` | `numbers_and_brackets.txt`（7 行） | 去数字、去半/全角括号、去空括号、去首尾空白 | 序列 = 张三/李四/王五六/赵七/钱八/孙九/周十 | PASS |
| `name.vertical.dupe` | `duplicates_and_two_chars.txt`（8 行） | `Distinct=false` 保留 8 行并标记重名行号；`Distinct=true` 得 5 行无标记 | `duplicates=0,1,2,4,5,6`；`distinct=5` | PASS |
| `name.vertical.pinyin0`…`5` | 源 `张三\n李四` | 六种拼音模式依次 `ZS/LS`、`zs/ls`、`ZHANG SAN/LI SI`、`ZHANGSAN/LISI`、`zhang san/li si`、`zhangsan/lisi` | 逐模式精确匹配 | PASS ×6 |

双字对齐的判据不是"看起来对齐"，而是硬断言 `aligned.Length == 4 && 半角空格数 == 2`——用全角空格或单个空格会直接失败。CRLF 那条用例把这条判据**逐行**再跑一遍：夹具原样 3 行 × 每行 4 个单元格 → 12 个名字，其中 5 个两字名（李四/钱八/孙九/周十/赵七）必须各成长度 4 + 恰 2 个半角空格，且 12 行结果与 LF 版逐行相等。它锁定的是 legacy 处理顺序：**先归一行尾，再把 TAB 换成换行**；顺序颠倒时 `\r` 会残留进名字里（`ToVerticalText()` 里的 `\r` 只是 `Environment.NewLine`，不算残留，故断言落在"每一行内容不含 `\r`"上）。

### 11.2 考评（`exam.*`，7 项）——全部跑在 `tools\ExamFixture` 合成库上

夹具不再由 harness 自己拼 DDL，统一走 `tools\ExamFixture`（`ExamFixtureBuilder.Create()`，13 张表 + 样例数据；它是唯一 DDL 真相，注释里标了每条 SQL 的源码行号）。

| 用例 | 输入 | 期望 | 实际 | 断言结果 |
| --- | --- | --- | --- | --- |
| `exam.insert` | 夹具库 + 1 项目 2 选项（优秀 / 弃权） | 新建考评表；`Exam_Score` 按语义齐备（项目 1 行 + 优秀选项 + 弃权选项 `Score=0` + 无效若干）、`Exam_BoolItem` 1 行（弃权是隐式选项）、`Exam_Ruler` 5 条默认规则、无重复 `(Kind,ItemName,SelName)` 组合；同名二次插入只追加 | `examId=6 score=5 boolItem=1 ruler=5 dupe=0 append=ok exams=3`（夹具自带 2 个 + 新建 1 个） | PASS |
| `exam.report` | 夹具 Report_Ess 7/8 + Report_Detail 3 行 | `ListReports` 关联出考评表名；`ReplaceContent(7, 所有项目, Item→新项目, Raid→9)` 改 2 行；别的报表不动；`ReportId=0` 抛 `ArgumentException` | `reports=2 scanned=2 changed=2 其他报表未动 guard=ok` | PASS |
| `exam.mss3` | `testdata\image\sample.png` + 夹具 Vote V-001/V-002 | 新增 1 张选票页；`PicContent` 是灰度 BMP；`SetItemColumnsToMemo` 返回改动列数 | `votes=3(夹具 2 + 1) bmp=481078 B(BM 头) memoAltered=5` | PASS |
| `exam.vote` | `assets\embedded\VoteReportDoc.doc`(40375 B) | 生成 DOCX：标题/名单写入，主表 + 另选人表共 2 张表 | `docx=ok tables=2 names=3` | PASS |
| `exam.single` | `person_template.docx`（占位图 200×260）+ `person_photo.png`（240×300） | 按 `AlternativeText="照片"` 换图，产物 media 里出现 240×300 | `replaced=240x300` | PASS |
| `exam.package` | 两个夹具库（`IncludeAutoIncrementProbeColumn=true`，`Exam_People`/`Unit_Ess` 带 `AutoId COUNTER PRIMARY KEY`） | 检出重复单位；清理 1 个；人员 +2；单位 +3（源库全量）；`.exam` +2（重名以 `_1` 另存不覆盖）；源单位按目标 `MAX(AtOrder)=4` 顺延为 5/6/7 | `人员 +2，单位 +3，.exam +2，清理重复单位 1`；目标单位 4 行、人员 3 行、`AutoId>0` 全部由目标库生成、`a.exam` 内容仍是 `target-package` | PASS |

`exam.insert` 的断言在这轮从"钉死行数"改成了**语义断言**：`Exam_Score` 的行数取决于 Core 侧对「选项/无效」等 legacy 细节的处理，并发改动把它从 3 行变成 5 行（项目 1 + 优秀 + 弃权 `Score=0` + 不计入得票率/无效 + 无效 `Score=-1`）。钉总数会变成"别人改 Core 就红"的假失败，所以现在断的是：`Kind='项目'` 恰 1 行、`SelName='优秀'` 至少 1 行、`SelName='弃权' AND Score=0` 至少 1 行、`SelName='无效'` 至少 1 行、`(Kind,ItemName,SelName)` 无重复组合（`GROUP BY … HAVING COUNT(*)>1` 计数为 0）。行数只作为 detail 输出，不参与判定。

`exam.package` 同一条用例还覆盖了数据包的只读能力（UI 列表页依赖）：`Inspect(源包)` 返回 `DatabaseExists=true / PeopleCount=2 / UnitCount=3 / ExamFileCount=2 / IsAlreadyInitialized=false`，`ListUnits(源包)` 按 `AtOrder` 升序返回 3 个单位（首项 `Order=1, Code=U1`）——合并通过但列表空的回归会在这里被抓到。

| 用例 | 输入 | 期望 | 实际 | 断言结果 |
| --- | --- | --- | --- | --- |
| `exam.package.reset` | 夹具（`SeedSampleRows=false` + `CreateSelfReportFolder=true`）+ 灌 2 人 / 2 单位 / 2 个 `.exam` | `Reset()` 清空人员、单位、`.exam`、SelfReport 四类内容，但保留 `Exam_Base.mdb` 本体；返回值 = 删除行数 | 清空前 `PeopleCount=2 / UnitCount=2 / ExamFileCount=2 / HasSelfReport=true`；`Reset()` 返回 `4`；清空后四项全 0、`SelfReport` 目录不存在、`IsAlreadyInitialized` 由 `false` 翻成 `true`、mdb 仍在；再次 `Reset()` 返回 `0` | PASS |

`exam.package.reset` 的断言落在**真实状态**上，不看文案：`Inspect()` 的四项计数 + `SELECT COUNT(*)` 直接查 `Exam_People`/`Unit_Ess` + `Directory.GetFiles("*.exam")` + `Directory.Exists(SelfReport)`。注意 `ExamPackageStatus.IsAlreadyInitialized` 的语义是「库在、内容空」，所以**灌数据时为 `false`、清空后为 `true`**（任务描述里的先后顺序与这个定义相反，实现按定义断言）。二次 `Reset()` 返回 0 也一并锁住，防的是"清空后再点一次报错"。合成库是一次性的，`ExamFixtureSet.Dispose()` 会删目录，所以这条用例对真实环境零影响。

`exam.package` 的自增列夹具是刻意加的：旧实现会把源自增列一起写回目标库，直接撞主键；本夹具让这条路径**必然**触发，从而验证"跳过目标自增列"的修正真的生效（3 行 `AutoId>0` 且无主键冲突即为证据）。

### 11.3 无 ACE 环境降级（`exam.no-ace`）

`AceAvailability` 是 Lazy 静态，同进程无法伪造两种环境，故 harness 起子进程带 `DOCUMENTTOOLS_SIMULATE_NO_ACE=1` 跑 `--child-no-ace`（只跑 `exam.*`、不清空 artifacts）：

```text
[PASS] exam.no-ace | in=--child-no-ace + DOCUMENTTOOLS_SIMULATE_NO_ACE=1 | out=artifacts/qa-function/exam.no-ace/child.log | 3.2 KB | 452 ms | exit=0 skipRows=ok unhandled=none
```

断言四项：子进程退出码 `0`、日志无 `Unhandled exception`、出现「跳过（无 ACE 通道）」显式跳过文案、文案里点明是被哪个环境变量模拟的。也就是说缺 ACE 的机器上考评用例会**显式跳过并说明原因**，不会变成红或崩溃。

### 11.4 身份证（`idcard.check`）

| 输入 | 期望 | 实际 | 断言结果 |
| --- | --- | --- | --- |
| `11010519491231002X` | 合法；1949年12月31日 / 女 / 顺序码 002 | 完全一致 | PASS |
| `110105194912310021` | 校验位错；期望校验位 `X` | `IsChecksumValid=False`、`ExpectedChecksum=X` | PASS |
| `12345` | 长度非法 → 空结果 | `Birthday` 为空 | PASS |
| `320102198512310037` | 合法；1985年12月31日 / 男 | 一致 | PASS |
| `440524188001010014` | 校验位合法，但生日早于 1900 年 → `日期不正确` | 一致（这是规则，不是缺陷） | PASS |

### 11.5 扫描与系统（`twain.debug` / `system.*`，4 项）

| 用例 | 期望 | 实际 | 断言结果 |
| --- | --- | --- | --- |
| `twain.debug` | 不可用时 `IsAvailable=False` 且 `UnavailableReason` 是一句可执行的中文提示；`ListSources()` 为空；`Scan()` 抛 `NotSupportedException` 且消息即该提示 | `IsAvailable=False`；文案「无法加载 Twain（NTwain/DSM）：Unable to load DLL 'twaindsm.dll' … (0x8007007E)。请安装扫描仪驱动与 TWAIN Data Source Manager。」；`sources=0`；`scan=NotSupportedException：<同一文案>` | PASS（**已对齐**：接口 `ITwainScanService.Scan` 的 XML 注释已按实现改写为「无扫描仪抛 `NotSupportedException(UnavailableReason)`，扫到 0 张返回空集合」，harness 照实现断言） |
| `system.network` | `ListAdapters()` 非空且每项 `Name` 非空 | `adapters=10`，首个 `Tailscale`/Up/type=53/ipv4=169.254.83.107 | PASS |
| `system.tools` | 只读探测 `IsAdministrator()` | `True` | PASS |
| `system.settings` | `AppSettings.Load()` + `GetHiddenFeatureIds()` + `FromLegacyConfig()` | `start=home hidden=0 store=C:/Users/pw/AppData/Local/DocumentTools/settings.json` | PASS |

**未验证：需在隔离机人工点测，本报告不为其背书。** 下列写操作本轮**刻意没有执行**——本机没有隔离环境，注册表 / 系统参数 / 网卡配置的写入会污染真实机器，且部分操作没有一键回滚。它们的实现只做了静态阅读，未参与 PASS/FAIL 统计（当前扩展 30 项里不含任何一项写操作）。

| 项（Core 方法） | 会改的位置 | 恢复方式 |
| --- | --- | --- |
| `IWindowsSystemToolsService.SetPauseUpdateDays` | `HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings` → DWORD `FlightSettingsMaxPauseDays`（`WindowsSystemToolsService.cs:29-32`） | 删该值或改回原 DWORD（默认不存在，删值即恢复） |
| `IWindowsSystemToolsService.HideShortcutArrow` | `HKLM\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons` → 字符串值 `29 = %systemroot%\system32\imageres.dll,197`（`:47-51`），随后跑一次 `ie4uinit.exe -show` 刷图标缓存 | 调 `RestoreShortcutArrow()`（`:54-63`，删除值 `29` 并刷缓存） |
| `IWindowsSystemToolsService.RestoreShortcutArrow` | 同上，删除值 `29` | 本身就是恢复操作；若要回到隐藏状态需重新 `HideShortcutArrow()` |
| `IWindowsSystemToolsService.DisableStickyKeys` | 未走注册表，调 `SystemParametersInfo(SPI_SETSTICKYKEYS, SPIF_UPDATEINIFILE\|SPIF_SENDCHANGE)` 清 `SKF_STICKYKEYS_ON` 位（`:65-78`，会持久化到用户配置） | 控制面板「轻松使用 → 键盘 → 使用粘滞键」重新开启，或用 `SPI_SETSTICKYKEYS` 把该位置回 1 |
| `INetworkAdapterService.SetEnabled` | WMI `Win32_NetworkAdapter.Enable()/Disable()`；失败回落 `netsh interface set interface name="…" admin=enabled\|disabled`（`NetworkAdapterService.cs:54-65`） | 用同一方法把 `enabled` 设回原值（断网时需本地控制台） |
| `INetworkAdapterService.ApplyStatic` | WMI `Win32_NetworkAdapterConfiguration.EnableStatic/SetGateways/SetDNSServerSearchOrder`；回落 `netsh interface ip set address/dns`（`:67-95`） | 记录改前 `ListAdapters()` 输出的 IP/掩码/网关/DNS，改完用 `ApplyStatic` 回填；或直接 `ApplyDhcp` |
| `INetworkAdapterService.ApplyDhcp` | 同上，`netsh interface ip set address/dns … dhcp`（`:97-107`） | 需要回到静态配置时用 `ApplyStatic` 传入记录值 |
| `IWindowsSystemToolsService.OpenWindowsUpdateSettings` / `OpenKeyboardEaseOfAccess` | 只打开 `ms-settings:` URI，不改状态 | 无需恢复（但会弹出系统设置窗口，自动化里不宜跑） |

## 12. TIF / GIF 产物覆盖（task-27，随 task-20 收口并入矩阵）

`image.compress` 的 `format=TIF/GIF` 两项检查（`image.convert.tif` / `image.convert.gif`）已随 task-20（自写 `TiffEncoder` / `GifEncoder`）收口正式入矩阵，属扩展区。实测（2026-11-05 20:07，`QaFunction.exe --bonus`）：

| 用例 | 输入 | 产物 | 字节 | 断言 |
| --- | --- | --- | --- | --- |
| `image.convert.tif` | `testdata\image\tifgif_src.png` | `tifgif_src_out.tif` | 17996 B | 魔数 `49 49 2A 00`（小端 `II` + 42）；前 8 字节非 PNG、前 3 字节非 JPEG；GDI+ 解码 96×64；(8,8) `#CB343D` 色差 0 |
| `image.convert.gif` | 同上 | `tifgif_src_out.gif` | 1232 B | 魔数 `47 49 46 38 39 61`（`GIF89a`）；前 8 字节非 PNG、前 3 字节非 JPEG；GDI+ 解码 96×64；(8,8) `#CB343D` 色差 0 |

四点口径说明：

1. Core 没有 `image.convert` 这个 FeatureId，真实链路是 `image.compress` → `ImageConvertOperation` → `ImageConversionService.Convert`，产物名由服务固定为 `<源名>_out<扩展名>`（用例名沿用任务卡里的 `image.convert.*` 便于和 UI 功能页对应）。
2. 正因为 `NormalizeFormat` 会把未知格式静默落成 `.png`，两项都同时断言**扩展名**与**魔数**；只断「有产物 / Success=true」会假通过。
3. TIF 用 GDI+ 解码而非 `SKBitmap.Decode`，原因见第 3.3 节（SkiaSharp 3.119 无 TIFF 解码器，连 Pillow 生成的参考 TIFF 都返回 `SKCodec=null`）。
4. 范围如实：**单页 / 单帧**。多页 TIFF 与 APNG 不在 task-20 范围内，本报告不作覆盖声明。产物实际写法为 `Compression=32773`（PackBits）、`Photometric=2`（RGB）、`BitsPerSample=(8,8,8)`（用 Pillow 读 tag 核实），GDI+ 与 Pillow 都能正确读回像素。

## 13. P2-1 / P2-7 / P2-8 产物断言（task-35）

这三项在原账本里的共同症状是「Schema 有、面板有、产物无证据」：选项键确实存在于 `FeatureOptionSchemas`，UI 上也点得到，但没人验证过**产物真的按这个键变了**。补充的 5 个用例全部遵守一条纪律：**同一输入、只改一个键、产物必须不同**；没有产物差异的键就算不上被验证。实测（2026-11-05 21:01，`QaFunction.exe --bonus`）：扩展段 **30 PASS / 0 FAIL / 0 SKIP**，退出码 `0`。

| 用例 | 输入 | 只改的键 | 期望 | 实际 | 断言结果 |
| --- | --- | --- | --- | --- | --- |
| `code.barcode.appearance` | 内存生成（Code128 `QA-BARCODE-0001` + QR `https://example.com/qa`） | `sizeMode` / `rotate` / `caption` | `fixed` → 520×180；`auto` 尺寸不同；`rotate` 90/270 宽高互换、180 不变；加 caption 不改尺寸；方向必须真的顺时针 | `fixed`=520×180、`auto`=580×172；`rotate` 90→180×520、180→520×180、270→180×520；caption 尺寸不变；**QR 空角**：0°→右下（BR 墨量 0.38 最低）、90°→左下、180°→左上、270°→右上 | PASS |
| `image.compress.useSourceExtension` | `testdata\image\sample.png`（同一次操作 `format=JPG`） | `useSourceExtension` | `true` 沿用源后缀 `.png`；`false` 用 `format` 的 `.jpg`；两者字节不同 | `true`→`sample_out.png` 19870 B；`false`→`sample_out.jpg` 22376 B | PASS |
| `image.compress.noCompress` | 同上 | `noCompress` | `true` 体积 ≈ 源（不压）；`false` 体积不同；两档体积必须不同，否则开关等于没生效 | `true`→14055 B **与源逐字节一致**；`false`→19870 B | PASS |
| `image.compress.bmpPixelBits` | `testdata\image\bmp_src.png`（`format=BMP`） | `bmpPixelBits` | 8/24/32 各转一次，读 BMP 头 `biBitCount`（offset 28，自写解析不引包）必须等于设定值 | `8→8`(2102 B)、`24→24`(3126 B)、`32→32`(4150 B) | PASS |
| `ppt.pdf.exportNotes` | `testdata\ppt\notes.pptx`（2 页，备注含 `NOTES-MARKER-ALPHA`） | `exportNotes` | `None` 无备注文本；`BottomFull` / `BottomTruncated` 有；页数仍 2；`BottomFull` 为竖版页；**不支持的 `RightFull` 必须被拒绝** | `None`=960.0×540.0pt 无标记；`BottomFull`=540.0×720.0pt 有标记（1223 字符）；`BottomTruncated`=540.0×720.0pt 有标记；`RightFull` 被拒绝（消息含「不支持的备注导出位置」、`OutputFiles` 空、目录内非空 PDF 0 个 / 0 B）；Schema 档位 = `None/BottomFull/BottomTruncated` | PASS |

四点口径说明：

1. **条码方向不能靠「哪条边更黑」**：Code128 是满幅条状图形，四条边都黑（实测加 caption 后 top=9934 / bottom=751，反而顶边更黑），用它判方向会误判。改用 QR 的定位角当方向指示器——QR 只有左上/右上/左下三个角有 7 模块实心定位块，右下角是空的；「空角在哪」就是方向证据：0°→右下、顺时针 90°→左下、180°→左上、270°→右上。四张图实测空角位置与预期逐一吻合，因此 `rotate` 的方向被证明是**顺时针**（Skia 正角度语义），而不是只证明了「宽高换了」。
2. **`useSourceExtension` 与 `format` 的优先级被显式验证**：同一张 PNG 故意同时给 `format=JPG`，`true` 时产物仍是 `.png`，说明源后缀确实压过 `format`；`false` 时才按 `format` 落 `.jpg`。
3. **`noCompress` 的断言比「体积差不多」更硬**：直接逐字节比较，实测与源文件完全一致（说明走的是原样落盘路径而非重编码），同时要求 `false` 档体积必须不同，否则该键等于没接线。
4. **`bmpPixelBits` 直读文件头**：`FileProbe.BmpPixelBits` 先校验 `BM` 签名，再取 `header[28] | header[29] << 8`，不依赖任何第三方库，避免「库自己报告的位深」自证。三种位深的产物字节数依次增大（2102 / 3126 / 4150 B），与 8/24/32 bpp 的像素数据量关系一致。

**缺陷 C 修复后的拒绝断言**（`[PASS] ppt.pdf.exportNotes` 行的原文）：

```text
[PASS] ppt.pdf.exportNotes | in=testdata/ppt/notes.pptx | out=artifacts/qa-function/ppt.pdf.exportNotes/BottomFull/notes.pdf | 190.7 KB | 2492 ms | 仅改 exportNotes：None→无 NOTES-MARKER（2 页 960.0x540.0pt）; BottomFull→含 NOTES-MARKER-ALPHA（2 页 540.0x720.0pt）; BottomTruncated→含 NOTES-MARKER-ALPHA（2 页 540.0x720.0pt）; RightFull→被拒绝（「不支持的备注导出位置」，产物 0 个 / 0 B）; Schema 档位=None/BottomFull/BottomTruncated
```

判定口径写清楚：`RightFull` 这一档**期望的就是失败**，因此断言是「`Success=false` + 消息含『不支持的备注导出位置』+ `OutputFiles` 为空 + 目录内无非空 PDF」；0 字节残留文件不算产物（若将来真出现非空文件，这条会红）。同时断言 `FeatureOptionSchemas.GetSections("ppt.pdf")` 的 `exportNotes` 档位只有三档——面板不会再提供这个值，行为与 Schema 两头都钉住。

取证依据保留：`%TEMP%\dt-notes-probe`（只引 `Aspose.Slides` 25.12.0.0 + `Aspose.Pdf`）打印 `NotesPositions 成员：None, BottomFull, BottomTruncated`，即 `RightFull` 在类型层面就不存在；修复前 harness 实测 `RightFull` 与 `None` 产物完全一致（2 页 960.0×540.0pt、无标记、382 字符）。这条不是评估模式限制（`BottomFull` 在同样无 license 的环境下正常工作），属**实现与 Schema 的能力错配**，修复方式见第 8 节缺陷 C。

## 14. 收口验证记录（发布闸门）

### 14.1 一条命令

```powershell
# 仓库根目录，等人停手（无 DocumentTools/testhost 进程、且最近 4 分钟没有源码改动）后执行
& .\tools\gate.ps1 -MaxSkips 3
```

`tools\gate.ps1` 顺序跑六门，**任一门失败立即停**，退出码语义：`0` 六门全过 / `1` 某门失败 / `3` 进程预检 ABORT（`DocumentTools` 或 `testhost` 在跑，先提示占用再退出，不让人误以为代码坏了）。

### 14.2 交付定稿这一跑（2026-11-05 21:59:38 → 22:02:18，总耗时 160 s；在测试集串行化之后）

| # | 门 | 命令 | 结论 | 关键数字 | 耗时 |
|---|----|------|------|----------|------|
| 1 | build | `dotnet build DocumentTools.sln --no-incremental -v q --nologo` | PASS | 警告 0 / 错误 0 / 退出码 0 | 5.7 s |
| 2 | test | `dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo` | PASS | 通过 363 / 失败 0 / 跳过 3（上限 3）/ 总计 366 | 69.4 s |
| 3 | FeatureSmoke | `dotnet run --project tools\FeatureSmoke` | PASS | 功能 47 / 创建 47 / 布局 47 / 启动落点 95 项失败 0 / 退出码 0 | 8.6 s |
| 4 | UiSnapshot | `dotnet run --project tools\UiSnapshot` | PASS | pages 47 / rendered 47 / fatal 0 / high 0 / medium 0 / low 6（观察项）/ oversized 0 | 44.5 s |
| 5 | QaFunction | `dotnet run --project tools\QaFunction -- --bonus` | PASS | 必测 16 项 16 PASS / 扩展 30 项 30 PASS / 追加 4 项 4 PASS / FAIL 行 0 / SKIP 行 0 / 退出码 0 | 24.5 s |
| 6 | QaCompat | `dotnet run --project tools\QaCompat -- --all` | PASS | PASS 14 / FAIL 0（新增 0，基线项 0）/ 无法验证 2 | 7.3 s |

**总判定：PASS（六道门全过，退出码 0）。** 原始日志：`%TEMP%\dt-gate-20261105-215938.log`（每门尾部 5 行，不在此贴长日志）。

三跑对照（同一台机、同一套夹具，差别只有测试集是否串行与用例总数）：

| 项 | 21:35:22（并行） | 21:51:20（并行） | **21:59:38（串行，定稿）** |
|----|------------------|------------------|----------------------------|
| test 通过 / 跳过 / 总计 | 332 / 2 / 334 | 363 / 3 / 366 | **363 / 3 / 366** |
| test 耗时 | 24.0 s | 26.0 s | **69.4 s**（串行化的代价，非退化） |
| 跳过上限 | `-MaxSkips 2` | `-MaxSkips 3` | `-MaxSkips 3` |
| QaCompat | PASS 11 / FAIL 3（基线项 3） | PASS 14 / FAIL 0 | **PASS 14 / FAIL 0** |
| 整条闸门耗时 | 94 s | 104 s | **160 s** |

三处变化的来由：①用例 332→363 是 S3 的 `OperationOptionKeyMappingTests`（32 例）+ 复审补的 13 例；②跳过 2→3 是 S3 新加的一条真 Word opt-in，**上限改 3 是纠正过时数字，不是放宽标准**；③test 26→69 s 是 `xunit.runner.json` 关掉测试类并行 + `ConversionLog` 双类挂 `[CollectionDefinition]` 的结果，属于**为确定性付的固定成本**（原因与代价见 §14.5 第 5 条）。

门 4 的 `low 6` 是**观察项**：只打印、不判失败，但必须进汇总，否则容易变成没人看的垃圾桶。

门 2 的 3 条跳过全部只认 `DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1`（会真拉起 `WINWORD.EXE`）：`WordLocalOfficeTests.WordToPdf_UseLocalOffice_ProducesPdfMadeByWord`、`WordLocalOfficeDiagnosticsTests.WordToPdf_UseLocalOffice_RealWord_LogsEngineAndElapsed`、`OperationOptionKeyMappingTests.WordToPdf_LocalOffice_ExportsOneBookmarkPerWordBookmark`。本机装有 Word（`Word.Application` = Word.Application.14）但未设该变量，故跳过。这 3 条的「跳过」现在只是默认路径；把 opt-in 打开后它们的健康状态见 §14.6（曾暴露一条红用例，已修）。

**ABORT 记录**：本轮调试期共 1 次 ABORT（21:12:16，预检命中 `testhost`（PID 39664），属另一位 teammate 正在跑测试；等到 21:12:50 自行清空后重跑）。最终这次运行 0 次 ABORT。全程没有 kill 任何人的 testhost。

### 14.3 复查一：5 个 P2 产物断言项当前状态

全部 PASS（取自本轮 `artifacts\qa-function\result.md`，逐项只改一个键）：

| 用例 | 证据（本轮实测） | 结果 |
|------|------------------|------|
| `code.barcode.appearance` | `sizeMode=fixed` 520×180（auto=580×172）；`rotate` 90→180×520、180→520×180、270→180×520；QR 空角方向 0°/90°/180°/270° 分别落在右下/左下/左上/右上 | PASS |
| `image.compress.useSourceExtension` | 同一 PNG 且 `format=JPG`：`true`→`sample_out.png` 19870 B；`false`→`sample_out.jpg` 22376 B | PASS |
| `image.compress.noCompress` | `true`→14055 B 与源**逐字节一致**；`false`→19870 B | PASS |
| `image.compress.bmpPixelBits` | `8→8`(2102 B)、`24→24`(3126 B)、`32→32`(4150 B)，`biBitCount` 直读 BMP 头 offset 28 | PASS |
| `ppt.pdf.exportNotes` | `None`→无 `NOTES-MARKER`（2 页 960.0×540.0pt）；`BottomFull` / `BottomTruncated`→含标记（540.0×720.0pt）；`RightFull`→**被拒绝**（消息含「不支持的备注导出位置」、`OutputFiles` 空、目录内非空 PDF 0 个 / 0 B）；Schema 档位仅三档 | PASS |

即缺陷 C 修复后，原先记 SKIP 的那条已转为**断言式 PASS**（断言的是「拒绝而不是静默」），不再是「未验证」。

### 14.4 复查二：QaCompat 现状（三条 FAIL 是断言与实现不同步，不是产品回归）

本轮实测（定稿这一跑，退出码 0）：`[QaCompat] PASS 14 / FAIL 0 / 无法验证 2`，报告 `docs\qa-report-compat.md`，`gate.ps1` 的 `$knownCompatFailures` 已清空。

| 用例 | 当前状态 | 说明 |
|------|----------|------|
| `path.long` | **PASS** | 「转换失败，但消息给出了具体路径并指出长度原因：「路径过长（312 字符，超过 Windows 260 字符限制）…」」 |
| `ace.missing-provider` | **PASS**（原 FAIL） | 断言已与 `AccessDb` 三级归因后的新文案同步（旧断言还匹配「无法打开 Access 数据库」）。 |
| `ace.simulate-switch` | **PASS**（原 FAIL） | 原断言假定「`AccessDb` 能打开、只有服务调用降级」，而开关生效时 `new AccessDb(...)` 直接抛 `InvalidOperationException`；现已按新行为改写。旁证：`ace.examfixture-sim` 一直 PASS（子进程退出码 2、stderr 是可读的 ACE 提示）。 |
| `html.no-browser` | **PASS**（原 FAIL） | 断言文案已对齐现行为。 |
| `ace.real-missing` / `twain.no-source` | 无法验证（不变） | 本机 ACE 可用（卸载属破坏性系统改动）、本机无 DSM（分支不可达），如实记为无法验证，**不计 PASS**。 |

闸门口径仍是「**无新增 FAIL**」而不是 `FAIL=0`：基线名单在 `tools\gate.ps1` 的 `$knownCompatFailures` 里，现在为空数组，等价于最严口径；将来若临时需要豁免，把用例名加进名单即可，闸门会打印「新增 FAIL」或「基线项已转 PASS，记得从名单里删掉」。

### 14.5 闸门本身踩到的五个坑（前四条修在 `tools\gate.ps1` 里）

1. **`dotnet run` 会把不认识的选项转给应用**：`dotnet run --project tools\QaCompat -v q --nologo` 会让 QaCompat 收到 `--nologo` → 打印用法并退出 2，看起来像产品失败。四个 `dotnet run` 门因此一律不附带 `-v q --nologo`，应用参数前显式加 `--`。
2. **Windows PowerShell 5.1 把原生命令的 stderr 当 terminating error**：`$ErrorActionPreference='Stop'` 时 testhost 崩溃的第一行 stderr 会让闸门当场中断、连汇总都不打印。`Invoke-Dotnet` 内改为临时 `Continue` 并在 `finally` 还原，主循环再加 `try/catch` 兜底打印「闸门被异常打断」。
3. **UiSnapshot 的独占锁**：上一轮被强杀会留下 `docs\qa-screens\.running.lock`，下一轮误报 BUSY。闸门开跑前先删残留锁；删不掉（说明另一个实例真在写）才判失败。
4. **脚本必须存成 UTF-8 带 BOM**：PowerShell 5.1 按 ANSI 解码无 BOM 的 UTF-8，脚本里的中文注释会把引号一起吃进字符序列，`powershell -File tools\gate.ps1` 直接报一批语法错误（换个工具重写文件时很容易把 BOM 丢掉）。写完记得核 `首三字节 = EF BB BF`；`pwsh` 里同样别用 `Get-Content` 默认编码去看这个文件。
5. **测试集必须整集串行——这是为确定性付的固定成本，不是退化**：这套测试大量依赖**进程级共享状态**（ACE/Jet OLE DB 提供者、Aspose 的字体/许可缓存、`ConversionLog.LogDirectoryVariable` 环境变量），xUnit 默认并行跑不同测试类时会互相踩，表现为「每次红的不一样、单跑全过」。修法两层：`tests\DocumentTools.Tests\ConversionLogCollection.cs` 里 `[CollectionDefinition("ConversionLog", DisableParallelization = true)]`，`WordLocalOfficeTests` 与 `WordLocalOfficeDiagnosticsTests` 都挂上；再加 `tests\DocumentTools.Tests\xunit.runner.json` 的 `{"parallelizeTestCollections": false}`（csproj 里 `<None Update="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />`）。代价：test 门 **24 / 26 s → 69.4 s**，整条闸门 **104 s → 160 s**。发布闸门的第一职责是「同样的输入给同样的结论」，这个交换值得；反过来说，**在串行化之前，322→366 用例那段时间一直是「运气不错的绿」**。

### 14.6 曾经被跳过基线掩盖的红用例（已修复：Lead 收口，双层串行 + 两次连续绿）

**原始症状（我这一轮取证）**：把 opt-in 打开后（`$env:DOCUMENTTOOLS_TEST_LOCAL_OFFICE='1'`，会真拉起 `WINWORD.EXE`），三条 opt-in 里有一条红：

```text
已通过 OperationOptionKeyMappingTests.WordToPdf_LocalOffice_ExportsOneBookmarkPerWordBookmark [3 s]
已通过 WordLocalOfficeTests.WordToPdf_UseLocalOffice_ProducesPdfMadeByWord [5 s]
失败   WordLocalOfficeDiagnosticsTests.WordToPdf_UseLocalOffice_RealWord_LogsEngineAndElapsed [7 s]
错误消息: Assert.Single() Failure: The collection contained 3 items
```

复现命令：

```powershell
$env:DOCUMENTTOOLS_TEST_LOCAL_OFFICE='1'
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v n --filter "FullyQualifiedName~WordLocalOffice|FullyQualifiedName~OperationOptionKeyMappingTests.WordToPdf_LocalOffice"
```

**它只是同一个病的一个症状。** Lead 在补 `[Collection]` 时调整并行调度，立刻暴露出**随机红**——同一份代码连跑两次，红的用例不同、单跑全过：

- 第一次：`ExamReportExtrasAccessTests.ListExamRaids_NoRaids_UsesPlaceholderLabel`
- 第二次：`ExamIndexServiceAccessTests.ChangeExamIndex_RebuildPhysicalOrder_OrdersRowsByExamId` + `ExamReportExtrasAccessTests.GetGridSize_SingleSegmentRowHeight_YieldsZeroRows`

病根：这套测试依赖进程级共享状态（ACE/Jet OLE DB 提供者、Aspose 字体/许可缓存、`ConversionLog` 的日志目录环境变量），并行跑不同测试类互相踩。我先前观察到的 `Assert.Single() 收到 3 条` 正是 `ConversionLog.LogDirectoryVariable` 被 `WordLocalOfficeTests` 改写后写进别人的日志目录。

**修复（已落地，两层）**：①`[CollectionDefinition("ConversionLog", DisableParallelization = true)]` + 两个改环境变量的测试类挂上；②`xunit.runner.json` 关闭测试类并行（csproj 注释写明理由：串行代价远小于红绿不确定）。

**修复后证据**：Lead 连跑两次全量 = `通过 363 / 跳过 3 / 失败 0`（64 s、61 s）；本报告定稿这一跑（§14.2）= `通过 363 / 失败 0 / 跳过 3 / 总计 366`，test 门 69.4 s、整条闸门 160 s、退出码 0。**未再出现任何随机红。**

**定稿总判定：PASS** —— 六道门全过（build / test / FeatureSmoke / UiSnapshot / QaFunction / QaCompat），`-MaxSkips 3`，退出码 `0`，总耗时 160 s，完成于 **2026-11-05 22:02:18**（Asia/Shanghai）。

## 15. dev 对照回归与 UX 政策行为断言（task-46）

**口径。** 新版要与 `dev`（WinForms + `DocumentTools/Services/`）的业务与 UI 行为一致，所以这里的断言不是「新版的输出自洽」，而是「与 dev 同一输入应得的结果比对」。dev 侧基准全部来自工作树里的 dev 源码实读（`DocumentTools/Util.cs`、`BodyForm.cs`、`FormAccessor.cs`），行号写在每项后面。**凡是新版与 dev 已知行为不一致、或库/环境挡住的，一律记 `SKIP` 并写明「差异待 task-38 收口」，不当 PASS 记。**

### 15.1 dev 对照回归（6 项：5 PASS / 1 SKIP）

| 用例 | 输入 | 只改的键 | dev 基准 | 实测 | 结论 |
|---|---|---|---|---|---|
| `dev-parity.word.pdf` | `testdata\word\six_pages.docx`（6 页、2 处 Word 书签） | 转换器（本机 Word / Aspose） | `Util.cs:2727-2731` `CreateBookmarks = wdExportCreateWordBookmarks`（=2，每处书签一条） | 本机 Word：`pages=6 bookmarks=2`；Aspose：`pages=6 bookmarks=0`（dev 的 Aspose 分支同样没设 `CreateBookmarks`，`Util.cs:2600-2690` 的 `OutlineOptions.DefaultBookmarksOutlineLevel=0`） | PASS |
| `dev-parity.excel.pdf` | `testdata\excel\three_sheets.xlsx`（3 表 × 4 行） | `eachSheet` | `BodyForm.cs` 的 `ExportEachSheet` 复选框：勾上则每表一份 | `eachSheet=true` → 3 份 PDF 合计 3 页；`false` → 1 份 3 页 | PASS |
| `dev-parity.html.pdf.paper` | `testdata\html\sample.html` | `paper`（A4 / Letter / Legal） | `BodyForm.cs:1305-1325` 纸型表，`SelectedIndex = 4` ⇒ 出厂默认 A4 | `Page.Rect`：A4 `595.92x841.92`、Letter `612.00x792.00`、Legal `612.00x1008.00`（单位 pt，用 `Page.Rect` 不用 `PageInfo`） | PASS |
| `dev-parity.pdf.compliance` | `testdata\pdf\sample.pdf` | `version`（`v_1_4` / `v_1_7`） | `BodyForm.cs:1223` 直接给 Aspose `PdfFormat` 枚举名（`v_1_7` 这种形式） | 输出头版本 `v_1_4→1.4`、`v_1_7→1.7`（`MapPdfVersion` 兜底原样透传，所以 dev 的写法也通） | PASS |
| `dev-parity.webp.image` | `testdata\image\sample.webp` | `quality`（40 / 92） | `BodyForm.cs:1420-1432` 只有「WebP→图片」，`Util.cs` 的 `ConvertWeBp2Image` 固定 `JpegQuality=100`，**没有质量档位** | 40 与 92 输出都是 `49181 B` | **SKIP**：新版同样不吃 `quality`，与 dev 行为一致；**差异已在 task-38 收口，非缺陷** |
| `dev-parity.pdf.compress` | 文字样本 `sample.pdf` + 图片密集样本 | 压缩档位 | dev 体积基准本机无实测，只记「是否回退」 | 文字 `68466→68466 B`（`KeptOriginal=True`）；图片密集 `112464→111626 B`（99.3%，`KeptOriginal=False`） | PASS（断言口径：报 `KeptOriginal` 时字节必须一字不差） |

`word.pdf` 这一项同时钉住了常量契约：`OfficeConvertService.WordBookmarksExportMode == 2` 与 dev 的 `wdExportCreateWordBookmarks` 数值一致（`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:34`、`:170`）；书签数 2 来自夹具里注入的两处 `w:bookmarkStart`，**不是按标题生成**——按标题会得到别的数字，所以这条能区分两种语义。

### 15.2 UX 政策行为断言（4 项：3 PASS / 1 SKIP）

用户点名的三个问题要用机器证明不再回退，因此在**真实 `Window` 宿主**里点控件、拖放、量高度，而不是读视觉树快照。

| 用例 | 做法 | 实测 | 结论 |
|---|---|---|---|
| `ui.policy.pathbox` | 遍历全部 47 个视图：筛出「政策路径框」（`IsReadOnly` 且 `AutomationProperties.HelpText` 以「只读：点右侧按钮选择」开头），逐框断 `IsReadOnly`/`IsTabStop`/`AllowDrop`/`MinWidth ≥ 320`；再真的构造 `DataFormats.FileDrop` 触发 `Drop`、点「选择…」看 `Text` 是否变化 | 4 个路径框全部只读 + 可 Tab 聚焦 + 可拖放 + 最小宽 320；拖放写入验证 3 个视图、「选择…」写入验证 3 个视图 | PASS |
| `ui.policy.pathbox.coverage` | 反向统计：占位文本含 路径/目录/文件/原图/替换图/浏览器 的输入框里，有多少**没走** `PathInputs`（因而可编辑） | 12 个路径语义输入框里 4 个视图用普通 `TextBox`：`exam.mss3(2/2)`、`exam.single(2/2)`、`exam.package(2/2)`、`exam.circle(2/2)` | **SKIP（已派 owner，未完成前不记 PASS）**：`exam.mss3`/`exam.single`/`exam.package` → `dev-exam-access`（task-40 考评域），`exam.circle` → `circle-fix-dev`（task-44 之后）；**验收标准 = 同样满足 `IsReadOnly + IsTabStop + AllowDrop + MinWidth 320` + 路径失效中文提示**，四项全部改完后本节据此翻成 PASS |
| `ui.policy.navrow` | 宿主真实 `MainWindow`（`Show()` 后按 `DispatcherPriority.ContextIdle` 泵 4 轮再量）：分组头取 `CollapsibleSection.Children[0]`，功能项取 `Button.Tag is string` | 9 个分组头 + 47 个功能项，`ActualHeight` 全部 `≥ 34`（`UiTokens.RowMinHeight`） | PASS |
| `ui.policy.missing-path` | 条码页写一个不存在的路径再点「识别」 | 状态行出现「文件已不存在：…\已被删除-不存在.png。请重新选择。」（`PathInputs.MissingFileNotice`），没有抛异常 | PASS |

**「选择…」这一步必须先把路径框清空再点**：拖放那一步已经把同一个夹具路径写进去了，不清空的话「点按钮后 Text 变了」会因为值本来就相同而假失败。这是断言自己的坑，不是产品问题。

### 15.3 这一节踩到的三个坑（对后续写 UI 行为断言的人有用）

- **裸 `Application` 量不出真实行高。** 一开始只 `new Application()`，`MainWindow`（Wpf.Ui 的 `FluentWindow`）模板不实现、视觉树为空，分组头 `ActualHeight` 全是 `0.0`，看起来像「行高缺陷」。实际是缺资源：`src\DocumentTools.App\App.xaml` 里合并了 `ui:ThemesDictionary` + `ui:ControlsDictionary`，必须 `new DocumentTools.App.App { ShutdownMode = ShutdownMode.OnExplicitShutdown }` 并调 `InitializeComponent()` 载入，再 `Show()`。
- **`DragEventArgs` 没有公开的 5 参构造函数**，构造拖放事件得反射取 `ConstructorInfo`，并显式设 `RoutedEvent = DragDrop.DropEvent`。
- **`UseWPF` 会带来 `System.IO` 的双义**（`CS0104`），harness 里补一个 `GlobalUsings.cs`（`global using System.IO;`）即可；另外 `TextBox.PlaceholderText` 是 Wpf.Ui 的扩展属性，裸 WPF `TextBox` 没有，读占位文本要走反射。

三条同时抄进了 `docs\qa-clickthrough.md` 的「自动化死角」小节 —— 那是人要照着点的清单，机器测不到的地方必须在那里留痕。

### 15.4 本节的计数

`QaFunction.exe --bonus` 全量：**必测 16 PASS / 0 FAIL，扩展 41 项 = 39 PASS + 0 FAIL + 2 SKIP，追加 4 PASS / 0 FAIL，退出码 0**。两个 SKIP：`dev-parity.webp.image`（webp 本身没有质量档位，dev 也没有，非缺陷）、`ui.policy.pathbox.coverage`（考评页 4 个视图仍是普通 `TextBox`，已派 `dev-exam-access` / `circle-fix-dev`，改完才翻 PASS）。

> **（已被第 16 节取代）** 上段是本轮当时的快照：两条 SKIP 随后都按实测翻成 PASS，扩展区现为 **42 项 / 42 PASS / 0 SKIP**，见第 16 节。

`twain.debug.reason-text` 原先记 SKIP，Lead 修掉后改为 **PASS**，并且断言补全成两个分支：Twain **可用时 `UnavailableReason == string.Empty`**（可用还给人「未初始化 Twain。」会误导，设备列表就在旁边），**不可用时消息里必须含 `Twain|扫描仪`**。之前只有不可用分支，等于让「可用时为空串」这条契约没人守。

## 16. SKIP 归零复核与门 2 白名单（2026-11-06）

**结论：`QaFunction.exe --bonus` = 必测 16 PASS / 0 FAIL，扩展 42 项 = 42 PASS / 0 FAIL / 0 SKIP，追加 4 PASS / 0 FAIL，退出码 0（2026-11-06 00:58 实测）。SKIP 归零，且这两条都不是靠放宽判定换来的。**

### 16.1 两条 SKIP 为什么可以翻 PASS

| 原 SKIP | 原 SKIP 理由 | 复核后的事实 | 新断言 |
| --- | --- | --- | --- |
| `ui.policy.pathbox.coverage` | 4 个考评视图仍用普通 `TextBox`，「已派 owner，改完再翻 PASS」 | `ExamCircleView.cs:31-32` 已改 `PathInputs.ForFile`；`dev-exam-access` 把 mss3/single/package/report/insert 迁移完。扫描 15 个路径语义框 **全部合规** | 收紧为硬断言：`Require(uncovered.Count == 0, …)`，失败行点名「哪个视图、几分之几」 |
| `dev-parity.webp.image` | quality 40 与 92 都是 49181 B，「差异待 task-38 收口」 | task-38 把 webp 目标格式从 4 项补到 5 项（新增独立 `JPEG`），Schema 与 dev `webp2image_combox` 同序同项；`WebpToImage` 仍固定 `JpegQuality=100`（dev 也是 100、UI 无质量档） | 拆成两条真断言：`…formats`（Schema 五档 + 默认 JPG）、`…outputs`（五档逐个跑，后缀 + 魔数 + 尺寸） |

`dev-parity.webp.image.outputs` 实测：`JPG→.jpg 49181 B；JPEG→.jpeg 49181 B；PNG→.png 45934 B；BMP→.bmp 1440054 B；GIF→.gif 18779 B`，`JPG`/`JPEG` 体积相同正是「两者编码参数一致、只有后缀不同」的证据。**quality 键一律不断言**——dev 没有这个维度，断言它等于把新参数写成 dev 契约。

### 16.2 期间修掉的两个 harness 假阴性（都是我自己的判定写窄了，不是产品缺陷）

1. `PolicyPathBoxes` 原先要求 `AutomationProperties.HelpText` 以「只读：点右侧按钮选择」开头——只有 `ToolViewKit.PathBox` 这条入口会写 HelpText，直接调 `PathInputs.ForFile` 的视图（`exam.circle`）本来合规却被判成「未迁移」。改为按政策本体判定：`IsReadOnly && IsTabStop && AllowDrop`。
2. `PickButtons` 原先只认「选择 / 浏览」，漏掉图片页的「打开…」按钮，于是 `exam.circle` 被判「点了按钮却没写入」。补词后：路径框 **21 个**全部合规、拖放写入验证 13 个视图、「选择…」写入验证 13 个视图、`notes=0`。

### 16.3 门 2 的跳过白名单（`tools\gate.ps1`）

- `-MaxSkips` 默认值 **1 → 3**，头部注释写明：3 = 本机装了 Word 时启用三个 opt-in 用例（`WordLocalOfficeTests`、`WordLocalOfficeDiagnosticsTests`、`OperationOptionKeyMappingTests.WordToPdf_LocalOffice_*`，都靠 `DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1`）；没装 Word 的机器它们本来就 Skip，传 `-MaxSkips 0` 也不该红。
- 门 2 改为 `-v n` 跑并**逐条打印被跳过用例的全名**（`已跳过 <FullName>` 与 xUnit 的 `<FullName> [SKIP]` 两种写法都收），超限时再补一句「若确认是新加的真 opt-in 再调上限」。上限是白名单、不是免检额度——第 4 个静默跳过必须立刻暴露，而不是藏在汇总行的一个数字里。

### 16.4 计数口径（本轮）

| 区段 | 项数 | 结果 |
| --- | --- | --- |
| 必测 | 16 | 16 PASS / 0 FAIL |
| 扩展 | 42 | 42 PASS / 0 FAIL / **0 SKIP** |
| 追加 | 4 | 4 PASS / 0 FAIL |
| 退出码 | — | 0（= 失败数） |

扩展区 42 项 = 第 11 节 23 项 + 第 12 节 2 项（TIF/GIF）+ 第 13 节 5 项（P2-1/7/8）+ 第 14 节 1 项（`exam.no-ace`）+ 第 15 节 dev 对照 7 项（原 6 项，webp 拆成 2）+ 第 15 节 UX 政策 4 项。Lead 预估的「41 → 43」按实际拆分是 **42**：webp 由 1 项拆成 2 项（+1），coverage 保持 1 项只是由 SKIP 变 PASS（不增项），所以是 41 + 1 = 42。

### 16.5 同一轮的六门实测（`& .\tools\gate.ps1`，默认 `-MaxSkips 3`）

`2026-11-06 00:57:06 → 00:59:26`（140 s），**总判定 PASS，退出码 0**，日志 `%TEMP%\dt-gate-20261106-005706.log`：

| 门 | 结论 | 关键数字 | 耗时 |
| --- | --- | --- | --- |
| 1 build | PASS | 警告 0 / 错误 0 | 4.2 s |
| 2 test | PASS | 通过 505 / 失败 0 / 跳过 3（上限 3）/ 总计 508 | 52.6 s |
| 3 FeatureSmoke | PASS | 功能 47 / 创建 47 / 布局 47 / 启动落点 95 项失败 0 | 6.1 s |
| 4 UiSnapshot | PASS | pages 47 / rendered 47 / fatal 0 / high 0 / medium 0 / low 6（观察项）/ oversized 0 | 29.9 s |
| 5 QaFunction | PASS | 必测 16 / 扩展 42 / 追加 4 全 PASS，FAIL 行 0，**SKIP 行 0** | 42.5 s |
| 6 QaCompat | PASS | PASS 14 / FAIL 0（新增 0，基线项 0）/ 无法验证 2 | 4.8 s |

门 2 实际打印出的三条跳过（就是白名单里的三条，没有第四条）：

```text
跳过用例：DocumentTools.Tests.OperationOptionKeyMappingTests.WordToPdf_LocalOffice_ExportsOneBookmarkPerWordBookmark
跳过用例：DocumentTools.Tests.WordLocalOfficeDiagnosticsTests.WordToPdf_UseLocalOffice_RealWord_LogsEngineAndElapsed
跳过用例：DocumentTools.Tests.WordLocalOfficeTests.WordToPdf_UseLocalOffice_ProducesPdfMadeByWord
```

换 `-v n` 之后 vstest 的汇总行从「`失败: 0，通过: …，已跳过: …，总计: …`」变成「`测试总数: 508` / `通过数: 505` / `跳过数: 3`」，全绿时**不打** `失败数` 行。因此门 2 的解析改成四个独立计数（两种写法都认），`失败数` 缺失时先按退出码归零、再由 `总数 − 通过 − 跳过` 反推——只为把数字显示对，判定仍以退出码 + 失败数 + 跳过上限三者共同成立为准。





