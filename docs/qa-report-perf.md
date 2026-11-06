# 批量处理性能与内存基线（QaPerf）

> 本报告由 `tools\QaPerf` 实测生成，无手工填写的数据。
> 生成时间：2026-11-05 21:12:16 +08:00 · 主数据 session：`20261105-210626-224`（40 次运行）· 历史 session：8 个
> 数据文件：`artifacts\qa-perf\sessions\*.json`；每场景输出：`artifacts\qa-perf\out\<场景>\round-<n>`。

## 1. 结论摘要

- 单文件平均耗时区间 13.2 ms（merge-2）~ 1246.2 ms（large-pdf-200p），差 94.4 倍（只统计热态轮次）。
- word.pdf 从 10 个扩到 50 个（输入 5.0 倍，热态第 2 轮）：总耗时 1926.2 -> 5184.5 ms（2.69 倍），单文件均时 192.6 -> 103.7 ms，亚线性（固定开销被大批量摊薄，单文件均时下降）。
- 冷启动代价（wordpdf-10，session `20261105-210812-605`）：第一轮 2746.2 ms，第二轮 2748.7 ms，第二轮没有更快（+0.1%），说明该场景的预热收益被同批次测量噪声掩盖。
- 峰值工作集最高 404.3 MB（image-50 第 1 轮，Process.PeakWorkingSet64 = 404.6 MB），最低 126.6 MB。
- 主数据 40 轮中 30 轮产出完整（失败数 0），其余 10 轮被评估版阻断、没有产物（不是实现失败）。
- 评估版还直接阻断了 2 个场景（merge-4、merge-20）：这些轮次没有产出文件，原因是 Aspose 评估版的元素数量上限，不是操作实现的问题，原文见第 2、6 节。
- 5 轮重复运行未发现单调上升的托管堆趋势（阈值：增幅 >30% 或斜率 >2 MB/轮且 R²>0.9）。
- 本次测量运行在 Aspose 评估版（未找到 .lic）：输出会带评估版水印/页数限制，`At most 4 elements ... in evaluation mode` 一类限制会影响绝对耗时，不能直接外推到授权版。

## 2. 环境与授权

| 项目 | 值 |
| --- | --- |
| 操作系统 | Microsoft Windows 10.0.26200（X64） |
| CPU | 13th Gen Intel(R) Core(TM) i7-13650HX |
| 逻辑核 | 20 |
| 物理内存 | 63.6 GB |
| .NET 运行时 | .NET 8.0.27 |
| 目标框架 | .NETCoreApp,Version=v8.0 |
| 进程架构 | X64 |
| Server GC | 未启用（工作站 GC） |
| Aspose 版本 | Words 26.1.0.0 / Pdf 25.12.0.0 / Cells 25.12.0.0 / Slides 25.12.0.0 |
| 授权状态 | 评估版（未找到 .lic） |
| 主机名 | PWNINT |

评估版限制（如实标注，未做任何规避）：

- Aspose 在评估模式下会插入水印，并对部分集合访问施加 4 元素上限。本次实测到的错误原文逐条列在下面，不做改写；另一种常见措辞是 `At most 4 elements (for example, a paragraph or a table row) can be processed in evaluation mode`（本次未出现）。
- 该限制属于库自身行为，本次不绕过、不破解；因此**绝对耗时与内存只能作为同一评估版环境下的横向对比**。
- 另一条实测到的评估版行为：Aspose.Words 26.1 **加载即截断到 202 段**（300 段纯文本 docx 探查结果 `paragraphs : 202`），超出的内容既不排版也不入 PDF。30 页大件夹具已按此重排为 188 段，`--probe` 可复核 `paragraphs : 192 / pages : 30`。
- 实测触发的评估版错误：`合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。请把 assets\licenses 下的授权文件放齐，或把文件拆分后再试。`
- 结论：`pdf.merge` 输入超过 4 个时被评估版直接拒绝（累计 16 轮），因此 20 份输入的合并场景只留下「被拒证据」，可测的合并耗时来自 2 输入（4 页，已校验）与 4 输入（被拒）。

## 3. 夹具

| 组 | 路径 | 文件数 | 单文件最大 | 合计 | 生成方式 |
| --- | --- | ---: | ---: | ---: | --- |
| docx-batch | `testdata\perf\word\batch` | 50 | 39.1 KB | 1952.6 KB | python-docx：每份 2 页（中英文段落 + 4x3 表格 + 内联 PNG） |
| docx-large | `testdata\perf\large` | 1 | 39.6 KB | 39.6 KB | python-docx：单文件 30 页（显式分页，每页 1 标题 + 4 段，每 15 页一张表，首页含图；共 188 段，低于评估版 202 段截断线） |
| pdf-200p | `testdata\perf\pdf` | 1 | 108.5 KB | 108.5 KB | PdfFixtureWriter 手写 PDF 1.4（200 页，Helvetica） |
| pdf-merge20 | `testdata\perf\pdf\merge20` | 20 | 1.5 KB | 30.6 KB | PdfFixtureWriter 手写 PDF 1.4（20 个 2 页文档） |
| png | `testdata\perf\image` | 50 | 182.4 KB | 8961.4 KB | SkiaSharp 现场生成（1600x1200 渐变+方块噪声+斑点） |

预算校验（按全部夹具组累计）：122 个文件；单文件上限 200 KB，实际最大 182.4 KB；总量上限 20 MB，实际 10.8 MB。均在预算内。

夹具生成说明：

- docx 夹具（python-docx 生成：中英文段落 + 4x3 表格 + 内联 PNG，显式分页）已存在，直接复用
- PNG 夹具已存在，直接复用（保证多轮测量输入一致）
- 200 页 PDF 已存在，匹配当前页数则复用
- 20 个 2 页 PDF 已存在，直接复用
- docx 由 `tools\QaPerf\fixtures\make_docx_fixtures.py`（python-docx）生成；PNG 由 QaPerf 用 SkiaSharp 绘制像素后由内置 `PngWriter` 编码（SkiaSharp 3.119 的 SKImage 只暴露 `Encode(SKEncodedImageFormat, int)`，默认 PNG 编码在同一图像上恒定输出约 224 KB，超单文件预算）；PDF 由 `PdfFixtureWriter` 手写 PDF 1.4 并 zlib 压缩内容流，全程不使用 Aspose 生成输入，避免预热与评估版水印污染输入。

## 4. 场景与参数

| 场景 | 说明 | FeatureId | 输入数 | 参数 | 输出命名 |
| --- | --- | --- | ---: | --- | --- |
| wordpdf-10 | 批量 word.pdf：10 个 docx（各 2 页） | `word.pdf` | 10 | compliance=Pdf17（其余默认） | `<原名>.pdf` |
| wordpdf-50 | 批量 word.pdf：50 个 docx（各 2 页） | `word.pdf` | 50 | compliance=Pdf17（其余默认） | `<原名>.pdf` |
| image-50 | 批量 image.compress：50 张 1600x1200 PNG | `image.compress` | 50 | format=PNG, pngLevel=3, pngColorType=Truecolor | `<原名>_out.png` |
| large-word-30p | 单文件大件 word.pdf：1 个 30 页 docx | `word.pdf` | 1 | compliance=Pdf17（其余默认） | `large_30p.pdf` |
| large-pdf-200p | 单文件大件 pdf.compress：1 个 200 页 PDF | `pdf.compress` | 1 | quality=75（其余默认） | `pages200_压缩.pdf` |
| merge-2 | pdf.merge：2 个 2 页 PDF | `pdf.merge` | 2 | outputName=merged.pdf | `merged.pdf` |
| merge-4 | pdf.merge：4 个 2 页 PDF | `pdf.merge` | 4 | outputName=merged.pdf | `merged.pdf` |
| merge-20 | pdf.merge：20 个 2 页 PDF 合并成 1 个 40 页 PDF | `pdf.merge` | 20 | outputName=merged.pdf | `merged.pdf` |

以上场景全部通过 `AddDocumentToolsCore()` 解析出的 `IDocumentOperation` 执行，与 App 调用同一条代码路径（不由 QaPerf 重新实现）。

## 5. 冷启动 vs 第二次运行

冷启动定义：**进程内第一次执行 Aspose 操作**（含 CLR JIT、Aspose 程序集加载与内部初始化、字体缓存建立）。
为拿到每场景真实的冷启动数据，下表的「冷启动」列取自**该场景单独进程**的 session（`--scenario <场景 id> --rounds 2`）；若该场景没有独立 session，则退回主 session 的第 1 轮并标注为 warm。

| 场景 | 冷启动轮 总耗时 ms | 冷启动 峰值WS MB | 第二次 总耗时 ms | 第二次 峰值WS MB | Δ耗时 | Δ峰值WS | 来源 session |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| wordpdf-10 | 2746.2 | 133.9 | 2748.7 | 134.3 | +0.1% | +0.4% | `20261105-210812-605` |
| wordpdf-50 | 9860.1 | 152.1 | 5570.6 | 160.4 | -43.5% | +5.4% | `20261105-210820-901` |
| image-50 | 3419.7 | 135.0 | 4307.6 | 138.4 | +26.0% | +2.5% | `20261105-210839-326` |
| large-word-30p | 2693.7 | 118.5 | 484.8 | 118.5 | -82.0% | 0% | `20261105-210849-785` |
| large-pdf-200p | 1307.2 | 86.9 | 465.7 | 91.3 | -64.4% | +5.0% | `20261105-210857-496` |
| merge-2 | 1253.6 | 83.6 | 119.4 | 86.1 | -90.5% | +3.0% | `20261105-210902-420` |
| merge-4 | 16.7 | 86.1 | 12.2 | 86.1 | — | — | `20261105-210902-420`（该轮非进程首跑）（评估版阻断，不是合并耗时） |
| merge-20 | 13.2 | 86.1 | 11.9 | 86.1 | — | — | `20261105-210902-420`（该轮非进程首跑）（评估版阻断，不是合并耗时） |

结论口径：Δ 为负表示第二次更快（预热收益），为正表示第二次更慢（多为输出体积或内存压力变化）。带「评估版阻断」的行只记录了一个失败返回的时间，不能当作 `pdf.merge` 的真实吞吐。

## 6. 批量运行明细（5 轮主表）

每场景取**轮次最多的 session**，逐轮列出真实测量值。`PeakWorkingSet64` 为 `Process.GetCurrentProcess().PeakWorkingSet64`（只增不减，含 QaPerf 自身）；`轮内峰值WS` 为测量期间 25 ms 采样的轮内峰值。

失败数一列若带「评估版阻断」标注，表示该轮的所有输入都因 Aspose 评估版的元素上限而没有产出文件（错误原文见本节末尾与第 2 节），不是文档工具自身的失败。

| 场景 | 轮 | 冷/热 | 文件数 | 总耗时 ms | 单文件均时 ms | PeakWorkingSet64 MB | 轮内峰值WS MB | GC(true) MB | 输出 KB | 失败数 |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| wordpdf-10 | 1 | 冷 | 10 | 2339.4 | 233.9 | 131.0 | 126.6 | 14.0 | 816.3 | 0 |
| wordpdf-10 | 2 | 热 | 10 | 1926.2 | 192.6 | 134.8 | 133.9 | 14.0 | 816.3 | 0 |
| wordpdf-10 | 3 | 热 | 10 | 1815.3 | 181.5 | 142.3 | 142.3 | 14.1 | 816.3 | 0 |
| wordpdf-10 | 4 | 热 | 10 | 1451.6 | 145.2 | 144.6 | 142.5 | 14.1 | 816.3 | 0 |
| wordpdf-10 | 5 | 热 | 10 | 1226.7 | 122.7 | 149.9 | 146.3 | 14.1 | 816.3 | 0 |
| wordpdf-50 | 1 | 热 | 50 | 5386.1 | 107.7 | 158.6 | 155.1 | 14.1 | 4088.3 | 0 |
| wordpdf-50 | 2 | 热 | 50 | 5184.5 | 103.7 | 159.4 | 159.4 | 14.1 | 4088.3 | 0 |
| wordpdf-50 | 3 | 热 | 50 | 5108.1 | 102.2 | 159.4 | 147.4 | 14.2 | 4088.3 | 0 |
| wordpdf-50 | 4 | 热 | 50 | 5292.8 | 105.9 | 159.4 | 152.2 | 14.2 | 4088.3 | 0 |
| wordpdf-50 | 5 | 热 | 50 | 5308.9 | 106.2 | 159.4 | 149.7 | 14.1 | 4088.3 | 0 |
| image-50 | 1 | 热 | 50 | 3558.7 | 71.2 | 404.6 | 404.3 | 14.1 | 16301.8 | 0 |
| image-50 | 2 | 热 | 50 | 3521.6 | 70.4 | 404.6 | 401.1 | 14.1 | 16301.8 | 0 |
| image-50 | 3 | 热 | 50 | 3634.4 | 72.7 | 404.6 | 396.7 | 14.1 | 16301.8 | 0 |
| image-50 | 4 | 热 | 50 | 3511.2 | 70.2 | 404.6 | 393.1 | 14.1 | 16301.8 | 0 |
| image-50 | 5 | 热 | 50 | 3666.5 | 73.3 | 404.6 | 391.3 | 14.1 | 16301.8 | 0 |
| large-word-30p | 1 | 热 | 1 | 172.7 | 172.7 | 404.6 | 132.3 | 14.1 | 114.1 | 0 |
| large-word-30p | 2 | 热 | 1 | 167.4 | 167.4 | 404.6 | 133.6 | 14.1 | 114.1 | 0 |
| large-word-30p | 3 | 热 | 1 | 138.2 | 138.2 | 404.6 | 133.0 | 14.1 | 114.1 | 0 |
| large-word-30p | 4 | 热 | 1 | 145.9 | 145.9 | 404.6 | 133.5 | 14.1 | 114.1 | 0 |
| large-word-30p | 5 | 热 | 1 | 139.1 | 139.1 | 404.6 | 133.1 | 14.1 | 114.1 | 0 |
| large-pdf-200p | 1 | 热 | 1 | 1246.2 | 1246.2 | 404.6 | 161.7 | 16.9 | 108.5 | 0 |
| large-pdf-200p | 2 | 热 | 1 | 644.6 | 644.6 | 404.6 | 166.0 | 17.0 | 108.5 | 0 |
| large-pdf-200p | 3 | 热 | 1 | 370.0 | 370.0 | 404.6 | 167.2 | 17.0 | 108.5 | 0 |
| large-pdf-200p | 4 | 热 | 1 | 284.9 | 284.9 | 404.6 | 167.2 | 17.0 | 108.5 | 0 |
| large-pdf-200p | 5 | 热 | 1 | 207.4 | 207.4 | 404.6 | 167.7 | 17.0 | 108.5 | 0 |
| merge-2 | 1 | 热 | 2 | 49.4 | 24.7 | 404.6 | 168.1 | 17.0 | 31.8 | 0 |
| merge-2 | 2 | 热 | 2 | 27.3 | 13.7 | 404.6 | 167.1 | 17.0 | 31.8 | 0 |
| merge-2 | 3 | 热 | 2 | 26.3 | 13.2 | 404.6 | 167.1 | 17.0 | 31.8 | 0 |
| merge-2 | 4 | 热 | 2 | 26.6 | 13.3 | 404.6 | 167.1 | 17.0 | 31.8 | 0 |
| merge-2 | 5 | 热 | 2 | 54.5 | 27.2 | 404.6 | 164.9 | 17.0 | 31.8 | 0 |
| merge-4 | 1 | 热 | 4 | 10.3 | 2.6 | 404.6 | 164.9 | 17.0 | 0.0 | 1（评估版阻断） |
| merge-4 | 2 | 热 | 4 | 6.3 | 1.6 | 404.6 | 164.9 | 17.0 | 0.0 | 1（评估版阻断） |
| merge-4 | 3 | 热 | 4 | 6.0 | 1.5 | 404.6 | 162.3 | 17.0 | 0.0 | 1（评估版阻断） |
| merge-4 | 4 | 热 | 4 | 9.0 | 2.2 | 404.6 | 159.7 | 17.0 | 0.0 | 1（评估版阻断） |
| merge-4 | 5 | 热 | 4 | 6.5 | 1.6 | 404.6 | 159.3 | 17.1 | 0.0 | 1（评估版阻断） |
| merge-20 | 1 | 热 | 20 | 5.7 | 0.3 | 404.6 | 159.3 | 17.1 | 0.0 | 1（评估版阻断） |
| merge-20 | 2 | 热 | 20 | 5.6 | 0.3 | 404.6 | 156.8 | 17.1 | 0.0 | 1（评估版阻断） |
| merge-20 | 3 | 热 | 20 | 4.6 | 0.2 | 404.6 | 154.2 | 17.1 | 0.0 | 1（评估版阻断） |
| merge-20 | 4 | 热 | 20 | 4.7 | 0.2 | 404.6 | 154.2 | 17.1 | 0.0 | 1（评估版阻断） |
| merge-20 | 5 | 热 | 20 | 4.4 | 0.2 | 404.6 | 154.2 | 17.1 | 0.0 | 1（评估版阻断） |

失败明细（原文照录；其中 10 轮属于评估版阻断）：

- `merge-4` 第 1–5 轮各失败 1 个（评估版阻断）：`合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。请把 assets\licenses 下的授权文件放齐，或把文件拆分后再试。`；`1 个输出文件缺失（期望 1 个，实际 0 个）`
- `merge-20` 第 1–5 轮各失败 1 个（评估版阻断）：`合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。请把 assets\licenses 下的授权文件放齐，或把文件拆分后再试。`；`1 个输出文件缺失（期望 1 个，实际 0 个）`

## 7. 产物校验

测量结束后（不参与计时）用 Aspose 重新打开产物核对页数/体积，证明输出是真实文件而非空壳。同一场景可能出现在多个 session 里，这里每个场景取「PDF 页数最多」的那次校验结果。

| 场景 | 输出文件数 | PDF 个数 | PDF 页数合计 | 读取失败 | 样例 |
| --- | ---: | ---: | ---: | ---: | --- |
| wordpdf-10 | 10 | 10 | 20 | 0 | batch_001.pdf = 2 页 |
| wordpdf-50 | 50 | 50 | 100 | 0 | batch_001.pdf = 2 页 |
| image-50 | 50 | 0 | 0 | 0 | — |
| large-word-30p | 1 | 1 | 30 | 0 | large_30p.pdf = 30 页 |
| large-pdf-200p | 1 | 1 | 200 | 0 | pages200_压缩.pdf = 200 页 |
| merge-2 | 1 | 1 | 4 | 0 | merged.pdf = 4 页 |
| merge-4 | 0 | 0 | 0 | 0 | — |
| merge-20 | 0 | 0 | 0 | 0 | — |

输入/读取探测：

- `large-word-30p: 输入 large_30p.docx 由 Aspose.Words 打开成功，PageCount = 30`
- `large-pdf-200p: 输入 pages200.pdf 由 Aspose.Pdf 打开成功，共 200 页`

## 8. 线性度：10 个 vs 50 个 docx

对比口径：同一进程内 `wordpdf-10` 与 `wordpdf-50` 的第 1、2 轮，来自 session `20261105-210626-224`。第 1 轮带冷启动成分（`wordpdf-10` 进程首跑，含 JIT 与 Aspose 初始化），第 2 轮是热态；判定用热态轮，避免 JIT/Aspose 初始化干扰规模结论。

| 指标 | 10 个 | 50 个 | 倍数 | 说明 |
| --- | ---: | ---: | ---: | --- |
| 第 1 轮 总耗时 ms | 2339.4 | 5386.1 | 2.30 | 仅参考：含冷启动成分，不作判定 |
| 第 2 轮 总耗时 ms | 1926.2 | 5184.5 | 2.69 | 判定依据：接近 5.0 才是线性 |
| 单文件均时 ms | 192.6 | 103.7 | 0.54 | 接近 1.0 = 每份成本稳定，明显 <1.0 = 固定开销被摊薄 |
| 轮内峰值WS MB | 133.9 | 159.4 | 1.19 | 接近 1.0 = 不随批量累积 |
| 输出体积 KB | 816.3 | 4088.3 | 5.01 | 应接近输入倍数 5.0（用来证明 50 份确实都转换了） |

判定（第 2 轮总耗时 1926.2 ms -> 5184.5 ms，×2.69）：亚线性：5 倍输入的耗时增长明显低于 5 倍，说明固定开销（进程/库初始化、字体缓存）在小批量里占比很高，被大批量摊薄；单文件均时随之下降。

## 9. 内存泄漏嗅探（同场景连跑）

判定规则：轮次 ≥ 3 才判趋势；托管堆增幅 > 30% 或（斜率 > 2.0 MB/轮 且 R² > 0.90）判为「疑似泄漏」。托管堆取每轮结束后的 `GC.GetTotalMemory(true)`。

| 场景 | 轮次 | 托管堆起止 MB | 增幅 | 斜率 MB/轮 | R² | 工作集起止 MB | 判定 | 备注 |
| --- | ---: | --- | ---: | ---: | ---: | --- | --- | --- |
| wordpdf-10 | 5 | 13.99 -> 14.10 | +0.8% | 0.03 | 0.94 | 125.81 -> 144.13 | 未见单调上升 | 托管堆 13.99 -> 14.10 MB（+0.8%），斜率 0.03 MB/轮，R²=0.94 |
| wordpdf-50 | 5 | 14.13 -> 14.09 | -0.3% | -0.01 | 0.06 | 151.16 -> 141.60 | 未见单调上升 | 托管堆 14.13 -> 14.09 MB（-0.3%），斜率 -0.01 MB/轮，R²=0.06 |
| image-50 | 5 | 14.10 -> 14.14 | +0.3% | 0.01 | 1.00 | 248.58 -> 234.91 | 未见单调上升 | 托管堆 14.10 -> 14.14 MB（+0.3%），斜率 0.01 MB/轮，R²=1.00 |
| large-word-30p | 5 | 14.15 -> 14.15 | 0% | 0.00 | 0.00 | 132.35 -> 133.13 | 未见单调上升 | 托管堆 14.15 -> 14.15 MB（0%），斜率 0.00 MB/轮，R²=0.00 |
| large-pdf-200p | 5 | 16.88 -> 16.96 | +0.5% | 0.02 | 0.50 | 161.70 -> 167.75 | 未见单调上升 | 托管堆 16.88 -> 16.96 MB（+0.5%），斜率 0.02 MB/轮，R²=0.50 |
| merge-2 | 5 | 16.97 -> 16.99 | +0.1% | 0.00 | 0.80 | 168.07 -> 164.89 | 未见单调上升 | 托管堆 16.97 -> 16.99 MB（+0.1%），斜率 0.00 MB/轮，R²=0.80 |
| merge-4 | 5 | 17.03 -> 17.05 | +0.1% | 0.01 | 0.89 | 164.93 -> 159.30 | 未见单调上升 | 托管堆 17.03 -> 17.05 MB（+0.1%），斜率 0.01 MB/轮，R²=0.89 |
| merge-20 | 5 | 17.05 -> 17.05 | 0% | 0.00 | 0.00 | 159.36 -> 154.21 | 未见单调上升 | 托管堆 17.05 -> 17.05 MB（0%），斜率 0.00 MB/轮，R²=0.00 |

说明：工作集在工作站 GC 下不保证归还操作系统，短时间不降并不等于泄漏；判定以强制 GC 后的托管堆为准。

## 10. IDisposable 审计（只读扫描，未改动 src/）

扫描 `src/DocumentTools.Core/**/*.cs` 中所有 Aspose 文档对象创建点，并按反射结果标注该类型是否真的实现 `IDisposable`：

- 34 处 Aspose 文档对象创建：21 处显式 using，13 处未 using；其中 1 处的类型确实实现 IDisposable

| 位置 | 代码 | 类型 | 实现 IDisposable |
| --- | --- | --- | --- |
| `src/DocumentTools.Core/Services/Code/DataMatrixService.cs:23` | `var doc = new Document(documentFile);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Exam/SingleDocumentService.cs:42` | `var document = new Document(templatePath);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Exam/VoteReportService.cs:29` | `var document = new Document(request.Template);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/DocumentInfoService.cs:47` | `var document = new Aspose.Words.Document(filePath);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/DocumentMergeSplitService.cs:28` | `var merged = new Aspose.Pdf.Document();` | `Aspose.Pdf.Document` | 是（未释放即泄漏候选） |
| `src/DocumentTools.Core/Services/Office/DocumentMergeSplitService.cs:55` | `var merged = new Aspose.Words.Document();` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/DocumentMergeSplitService.cs:58` | `var part = new Aspose.Words.Document(file);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/DocumentMergeSplitService.cs:82` | `var document = new Aspose.Words.Document(sourceFile);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/OfficeConvertService.cs:62` | `Aspose.Words.Document doc = new(sourceFile, loadOptions);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/OfficeConvertService.cs:511` | `var document = new Aspose.Words.Document(sourceFile);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/OfficeConvertService.cs:553` | `var document = new Aspose.Words.Document(sourceFile);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/OfficeConvertService.cs:680` | `var document = new Aspose.Words.Document(sourceFile);` | `Aspose.Words.Document` | 否（无 using 也无害） |
| `src/DocumentTools.Core/Services/Office/OfficeConvertService.cs:827` | `var document = new Aspose.Words.Document(sourceFile);` | `Aspose.Words.Document` | 否（无 using 也无害） |

结论：1 处创建点的类型实现 IDisposable 却未释放，是需要优先处理的对象生命周期问题。

## 11. 结论

- `wordpdf-10`：第 2 轮，10 个文件 1926.2 ms（单文件 192.6 ms），轮内峰值 133.9 MB，输出 816.3 KB，失败 0。
- `wordpdf-50`：第 2 轮，50 个文件 5184.5 ms（单文件 103.7 ms），轮内峰值 159.4 MB，输出 4088.3 KB，失败 0。
- `image-50`：第 2 轮，50 个文件 3521.6 ms（单文件 70.4 ms），轮内峰值 401.1 MB，输出 16301.8 KB，失败 0。
- `large-word-30p`：第 2 轮，1 个文件 167.4 ms（单文件 167.4 ms），轮内峰值 133.6 MB，输出 114.1 KB，失败 0。
- `large-pdf-200p`：第 2 轮，1 个文件 644.6 ms（单文件 644.6 ms），轮内峰值 166.0 MB，输出 108.5 KB，失败 0。
- `merge-2`：第 2 轮，2 个文件 27.3 ms（单文件 13.7 ms），轮内峰值 167.1 MB，输出 31.8 KB，失败 0。
- `merge-4`：第 2 轮，4 个文件 6.3 ms（单文件 1.6 ms），轮内峰值 164.9 MB，输出 0.0 KB，失败 1（评估版阻断，非实现问题）。
- `merge-20`：第 2 轮，20 个文件 5.6 ms（单文件 0.3 ms），轮内峰值 156.8 MB，输出 0.0 KB，失败 1（评估版阻断，非实现问题）。

线性度（session `20261105-210626-224` 热态第 2 轮）：word.pdf 输入 ×5.0 → 总耗时 ×2.69、单文件均时 ×0.54、轮内峰值WS ×1.19。耗时增长明显低于输入增长：固定开销（进程/JIT/Aspose 初始化）在小批量里占主导，被大批量摊薄。

瓶颈判断（按单文件均时排序，取每场景第 2 轮热态，只有 1 轮时取第 1 轮）：

- `large-pdf-200p`：644.6 ms/文件（总 644.6 ms / 1 个，第 2 轮）。
- `wordpdf-10`：192.6 ms/文件（总 1926.2 ms / 10 个，第 2 轮）。
- `large-word-30p`：167.4 ms/文件（总 167.4 ms / 1 个，第 2 轮）。
- `wordpdf-50`：103.7 ms/文件（总 5184.5 ms / 50 个，第 2 轮）。
- `image-50`：70.4 ms/文件（总 3521.6 ms / 50 个，第 2 轮）。
- `merge-2`：13.7 ms/文件（总 27.3 ms / 2 个，第 2 轮）。

## 12. 建议（可执行）

1. **逐文件释放文档对象**：静态审计发现 1 处创建点的类型实现了 `IDisposable` 却没用 `using`，例如 `src/DocumentTools.Core/Services/Office/DocumentMergeSplitService.cs:28`；批量循环里会按文件数累积。改为 `using var doc = ...`（或 `try/finally { doc.Dispose(); }`）即可，风险最低、收益最直接。
2. **合并时避免重复读盘**：`pdf.merge` 现在是逐份打开输入再写入同一个目标文档（`DocumentMergeSplitService.MergePdfFiles`），20 份输入就是 20 次读盘 + 20 次页拷贝；若输入格式统一，可改成一次 `Document` 打开后按页拼接，或流式追加并复用 `MemoryStream`。
3. **image.compress 可并行**：单张 1600×1200 PNG 平均 71.2 ms，纯 CPU/内存操作且每张互相独立（`ImageConversionService` 内部已是 `using` + `finally Dispose` 的干净写法），在 20 逻辑核上用 `Parallel.ForEach`（`MaxDegreeOfParallelism = min(4, 核数/2)`）通常能拿到接近线性的吞吐提升，代价是峰值内存按并发数上升（当前 50 张一轮的轮内峰值 404.3 MB，8.1 MB/张推算）。
4. **批量失败隔离**：`OfficeConvertOperation` 整个批次包在一个 try/catch 里，一份文档异常会让整批返回 Fail 且丢弃已产出的文件（本次测量未触发，但失败数统计口径就是按此设计的）。建议循环体内逐文件 try/catch，失败计数并把成功产物继续返回。
5. **冷启动预热**：`wordpdf-50` 冷启动 9860.1 ms，明显高于后续同场景轮次；同进程第二轮降到 5570.6 ms（-43.5%）。桌面端可在启动后空闲线程做一次「最小文档转换」预热（Aspose 初始化 + 字体缓存），把首次用户操作的首字节时间从秒级降到百毫秒级。
6. **批量并发上限**：当前所有批量操作串行执行。按本轮数据，word.pdf 热态单文件约 107.7 ms、内存增量有限，可用 2~4 路并发换取吞吐；上线前需用 `--rounds 5` 复测确认峰值工作集不随并发线性上升。

## 13. 复现命令

```powershell
dotnet build tools\QaPerf\QaPerf.csproj -v q
dotnet run --project tools\QaPerf -- --scenario all --rounds 1
dotnet run --project tools\QaPerf -- --scenario all --rounds 5
dotnet run --project tools\QaPerf -- --scenario wordpdf-10 --rounds 2
dotnet run --project tools\QaPerf -- --scenario wordpdf-50 --rounds 2
dotnet run --project tools\QaPerf -- --scenario image --rounds 2
dotnet run --project tools\QaPerf -- --scenario large-word-30p --rounds 2
dotnet run --project tools\QaPerf -- --scenario large-pdf-200p --rounds 2
dotnet run --project tools\QaPerf -- --scenario merge --rounds 2
# 单文件事实核对（页数/段落数，不运行场景）
dotnet run --project tools\QaPerf -- --probe testdata\perf\large\large_30p.docx
dotnet run --project tools\QaPerf -- --probe testdata\perf\pdf\pages200.pdf
# 只重建报告、不重新测量（读取 artifacts\qa-perf\sessions\*.json）
dotnet run --project tools\QaPerf -- --report-only
```

每次运行都会：生成/复用 `testdata\perf` 夹具 → 跑场景 → 写出产物到 `artifacts\qa-perf\out` → 追加 session JSON → 用**全部** session 重新生成本报告。`--reset` 可清空历史 session 从零复测。

本报告使用的 session：

| session | 开始时间 | 参数 | 轮次 | 覆盖场景 |
| --- | --- | --- | ---: | --- |
| `20261105-210626-224` | 21:06:26 | `--scenario all --rounds 5` | 40 | wordpdf-10、wordpdf-50、image-50、large-word-30p、large-pdf-200p、merge-2、merge-4、merge-20 |
| `20261105-210812-605` | 21:08:12 | `--scenario wordpdf-10 --rounds 2` | 2 | wordpdf-10 |
| `20261105-210820-901` | 21:08:20 | `--scenario wordpdf-50 --rounds 2` | 2 | wordpdf-50 |
| `20261105-210839-326` | 21:08:39 | `--scenario image-50 --rounds 2` | 2 | image-50 |
| `20261105-210849-785` | 21:08:49 | `--scenario large-word-30p --rounds 2` | 2 | large-word-30p |
| `20261105-210857-496` | 21:08:57 | `--scenario large-pdf-200p --rounds 2` | 2 | large-pdf-200p |
| `20261105-210902-420` | 21:09:02 | `--scenario merge --rounds 2` | 6 | merge-2、merge-4、merge-20 |
| `20261105-211158-403` | 21:11:58 | `--scenario all --rounds 1` | 8 | wordpdf-10、wordpdf-50、image-50、large-word-30p、large-pdf-200p、merge-2、merge-4、merge-20 |

## 14. 限制与未覆盖

- 单进程、串行执行：不反映 App 里 UI 线程/多任务并发时的真实表现，也不含磁盘冷缓存以外的 IO 竞争。
- `Process.PeakWorkingSet64` 是进程级只增指标，包含 QaPerf 自身；判断单轮内存请以「轮内峰值WS」列为准。
- 夹具为合成文档（短段落 + 表格 + 小图），不代表真实业务文档的复杂度；版式复杂的文档转换耗时通常更高。
- 评估版 Aspose 会插入水印并限制部分 API，绝对耗时不可直接外推到授权版；本报告的横向对比与趋势结论不受影响。
- Aspose.Words 26.1 评估版会把单文档的加载**截断在 202 段**：用 300 段纯文本 docx 探查，引擎只报 `paragraphs : 202`；最初 300 段的 30 页大件因此被渲染成 16 页。现夹具压到 188 段（引擎读入 192 段，含表格单元格），才能在评估版下真实渲染 30 页 —— 该数字可用 `--probe` 复核。
- 冷启动数据依赖「该场景单独进程」的 session；若未运行对应 session，第 5 节会标注该轮并非进程首跑。
- IDisposable 审计为静态正则 + 反射，跨行构造或别名变量可能漏检；结论以实测泄漏趋势为准。

报告由 8 个 session 汇总生成；重新运行 QaPerf 会用新数据覆盖本文件。

