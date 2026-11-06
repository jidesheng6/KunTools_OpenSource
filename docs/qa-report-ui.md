# UI 自动化快照报告（task-18：回归 + 高 DPI + 交互态）

- 工具：`tools/UiSnapshot`（真 DI 容器 + 真视图工厂 + 真实 Window 宿主，非 mock）
- 一次跑全：`dotnet run --project tools\UiSnapshot`（47 基准快照 + 16 张高 DPI + 4 个交互态）
- 检查器自检：`dotnet run --project tools\UiSnapshot -- --self-test`（**20/20**；task-44 起含七条 UX 规则的逐条反例、规则 ⑤ 的 TabIndex 双向自证与壳层规则的红/绿自证）
- 七条 UX 规则的判定口径与命中清单另见 [`docs/qa-report-ux.md`](qa-report-ux.md)
- 产物：`docs/qa-screens/`（47 张 `*.png` + 4 张 `*@interact.png` + `hidpi/` 16 张 + 5 个 CSV + 2 张壳层/设置态证据）
- 数据来源：`_index.csv`（逐页计数）、`_issues.csv`（基准问题）、`_hidpi.csv` / `_hidpi-issues.csv`（高 DPI）、`_interact.csv`（交互态）
- 权威一轮时间：**2026-11-06 00:41:59**（task-44 口径，快照同时跑七条 UX 规则；`High 0 / Medium 0 / Low 6`）
- 数字对应运行：`tools\UiSnapshot` 全量运行 2026-11-06 00:41:59，`_index.csv` 48 行（表头 + 47 页）；§1 的 UX 数字与 `docs/qa-report-ux.md` 取自这一轮
- 历史轮次：2026-11-06 00:15:20（task-44 首轮，`H14/M39/L76`）、2026-11-06 00:25:51（`H14/M32/L59`）、2026-11-06 00:39:37（`H2/M28/L6`）、2026-11-06 00:40:59（`H0/M3/L6`）；2026-11-05 21:12（task-18 口径，未含七条 UX 规则，当时 `High 0 / Medium 0 / Low 6`）
- 仓库分支：`feat/wpfui-migration`

## 1. 结论

47 个功能页全部由 `AppHost.CreateServiceProvider()` + `IFeatureViewFactory.Create(id)` 真实创建、真实渲染成功（`fatal=0`），**High 0、Medium 0、Low 6**。相对 task-8 基线（Medium 1、Low 13）**净清除 8 条，无新增**：1 条 Medium（`code.datamatrix` 空占位）已修，7 条文本溢出全部消失（`system.network` 5、`exam.package` 1，以及回归期内新冒出的 `name.vertical` 1、`exam.insert` 1、`exam.vote` 3 也已由 Exam designer 修掉）。

余下 6 条 Low 全是同一观察项「页面只有按钮/列表、没有输入控件」（`home`、`about`、`word.merge`、`pdf.merge`、`code.datamatrix`、`other.misc`），属设计取舍，不是缺陷。

§8 问题清单合计 12 条：**已修 11 条**（应用缺陷 UI-01/02/03/05/06/10/11/12 共 8 条 + 测试设施 UI-07/08/09 共 3 条）、**保留 1 条**（UI-04，设计取舍）；当前**没有「待修」项**。

高 DPI：120/144 各 8 页共 16 张全部渲染正常，`H0/M0/L0`（`home`/`about` 仍是同一条无输入控件观察项），**文本溢出 0、负坐标 0、图像未渲染 0**，说明这一轮 UI 改造没有引入随 DPI 变化的布局问题。

交互态：4 页（`word.pdf`、`exam.vote`、`code.barcode`、`exam.circle`）全部 `ok`，`interactHang=0`。task-8 那次 `exam.circle` 挂起 7 分钟并锁死 `bin` 的根因已定位并修掉：不是应用 bug，是 harness 点到了会弹模态 `MessageBox` 的按钮，把离屏 Dispatcher 泵永久阻塞（详见 §5.2）。

## 2. 方法与可复现性

### 2.1 一条页面的成像链路

`DocumentTools.App.App` + `InitializeComponent()`（加载 Wpf.Ui Light 主题）→ `AppHost.CreateServiceProvider()` → 取 `IFeatureCatalog.All` → `IFeatureViewFactory.Create(id)` → 白底 `Border{1200×800}` 放进 `Window{WindowStyle=None, ShowInTaskbar=false, ShowActivated=False, Left=-4000, Top=-4000}` 并 `Show()` → `Measure/Arrange/UpdateLayout` → 泵 150ms Dispatcher →（可选）跑交互脚本 → 再泵一次 → `RenderTargetBitmap(1200×800, 96dpi, Pbgra32)` → `PngBitmapEncoder`（>300KB 自动降到 0.75/0.5 倍）。

### 2.2 三项机器可判定检查（全部基于渲染后的视觉树）

1. **裁剪/越界**：`TransformToAncestor(host).TransformBounds` 得元素在页面坐标系的真实矩形；无 `ScrollViewer` 祖先且越出 1200×800 → High「超出页面视口」；祖先 `ClipToBounds=true` 且子元素越界且不可滚动 → High「被父容器裁切」。
2. **空文本控件**：跳过 `TemplatedParent != null` 的模板部件；`Button` 内容为空 → Medium「空文本按钮」，内容为图形且无 `ToolTip`/`AutomationProperties.Name` → Low「无文本按钮」；卡片内空 `TextBlock`/`Label` → Medium「空标签」；`??`/`U+FFFD` → High「乱码文本」。
3. **键盘可达性**：统计 `Focusable && IsEnabled && Visible && IsTabStop` 数量并记录首个输入控件；`TabStop=0` → High「键盘不可达」。

task-18 新增两条判定（为高 DPI 服务）：**负坐标**（`bounds.Left/Top < -1.5` → Medium）与**图像未渲染**（有 `Source`、可见、`ActualWidth<1 || ActualHeight<1` → Low）。

### 2.3 文本溢出判定的坑（别把它当"没有溢出"的证据）

WPF 的 `TextBlock` 在 `TextWrapping=NoWrap` 溢出时，`ActualWidth`/`RenderSize` 会被撑成**自然宽度**（实测 `Width=80` 被忽略，`RenderSize=854.8`），用 `ActualWidth` 比较永远判不出溢出。本工具改用父容器分配的布局槽 `LayoutInformation.GetLayoutSlot(block).Width`，再与 `FormattedText.WidthIncludingTrailingWhitespace` 比，容差 1.5px。task-8 最初"47 页 0 条溢出"就是这个假阴性，修正后立刻报出 7 条真实裁剪。

### 2.4 三个 harness 缺陷（本轮修掉，都不是应用问题）

1. **主按钮"消失"**：离屏 `Border` 宿主里 WPF-UI 的 `ControlAppearance.Primary` 解析出 `Background=#00FFFFFF`、前景纯白 → 白底白字，像素扫描为 0；挂到真实 `Window` 后恢复 `#FF1D6978`（`--window-probe exam.circle` 可复现此结论）。**task-8 的 47 张图对主按钮存在系统性失真**，本轮已改在 Window 内渲染，像素复验：`exam.circle@interact.png` 主色像素 2402、`code.barcode@interact.png` 2414、`word.replace@interact.png` 3481（改前均为 0）。
2. **模态弹窗把 Dispatcher 卡死**：交互脚本点到 `IDialogService.Info` 时，真实现是 `MessageBox.Show`（`src/DocumentTools.App/Infrastructure/Dialogs/IDialogService.cs:55`）→ 泵不返回 → 子进程挂死 7 分钟并锁住 `bin\*.dll`（lead 现场抓到 PID 44228）。修法：harness 用假 `IDialogService`（`tools/UiSnapshot/QaDialogService.cs`，文件对话框返回 `%TEMP%\dtsnap-dialogs\<tag>-*`，`Info/Error` 只记录，`Confirm` 一律答"否"——答"是"会让业务真的去 Shell 打开 Word，实测拉起过 `WINWORD.EXE`），并加 10s 单步看门狗（超时判 `step-timeout`、`Environment.Exit(7)`）。
3. **并发运行互相覆写**：`docs/qa-screens` 是团队共享目录，别人用 `--only` 过滤跑一次就会把 `_index.csv` 从 47 行改写成 7 行（本轮实际发生两次）。修法：整套运行先抢输出目录独占锁 `.running.lock`，抢不到打印 `BUSY|…` 并以退出码 8 结束；子进程与 `--only` 单页模式不参与加锁。

### 2.5 检查器自证（`--self-test`）

合成页面构造六类已知缺陷（越界、空文本按钮、卡片内空标签、文本溢出、乱码 `?????????`、有/无 ToolTip 的图标按钮）+ 一个 `TextBox`，断言逐条命中并验证 ToolTip 过滤；再加两条壳层规则自证（旧 `Expander` 布局必须判红、现 `CollapsibleSection` 必须判绿，见 §2.6）：

```
SELFTEST|viewport|hit|count=1
SELFTEST|empty-button|hit|count=1
SELFTEST|empty-label|hit|count=1
SELFTEST|overflow|hit|count=1
SELFTEST|mojibake|hit|count=1
SELFTEST|icon-button|hit|count=1
SELFTEST|tooltip-filter|hit|count=1
SELFTEST|input|hit|type=TextBox
SELFTEST|shell-legacy-expander|hit|problems=3|minButtonH=0.0
SELFTEST|shell-collapsible-section|hit|problems=0|minButtonH=33.8
SELFTEST RESULT passed=10/10
```

### 2.6 壳层检查（页面全绿 ≠ 导航可用）

页面快照只量「页面」，量不到「壳」。本轮发现并修掉的一个真缺陷正好落在盲区里：侧边栏分组用 WPF `Expander` 时，展开后组内按钮的实际高度是 0（详见 §8 UI-10），而 47 页快照当时全绿——因为页面本身没问题，坏的是导航。

为此 `tools/UiSnapshot/ShellAudit.cs` 增加一项壳检查，默认全量运行时自动跑（不新开工程）：

1. 从 DI 取真实 `MainWindow`，`RaiseEvent(Loaded)` 走完真实启动流程，把内容树摘下来按 1440×900 离屏布局；
2. 对 `NavHost` 里每个导航分组，收起 → 布局 → 数可见按钮，展开 → 布局 → 再数一遍，**展开后的可见按钮数不得少于收起时**；
3. 展开态下，组内每个导航按钮必须有效可见且 `ActualHeight ≥ 20`；
4. 任一条不满足即打印 `SHELL|FAIL|…` 并计入退出码。

通过时的输出（2026-11-05 21:12 一轮）：

```
SHELL|groups=9|singleButtons=2|buttons=54|collapsedVisible=9|expandedVisible=54|minButtonH=28.5|problems=0
```

自证方式与 §2.5 一致：把同一份规则套到旧 `Expander` 布局上必须报 `problems=3`、`minButtonH=0.0`（真红），套到 `CollapsibleSection` 上必须 `problems=0`（真绿）。没有这两条，报告里的「导航可用」只是没测过而已。

数字口径：`groups=9` 是多项目分类的折叠分组数，`singleButtons=2` 是「首页 / 关于」这两个只含一项、直接裸按钮的分类；`buttons=54` = 9 个分组头按钮 + 45 个功能项按钮，所以 `collapsedVisible=9`（收起时只剩分组头）→ `expandedVisible=54`（全部展开），单调不减成立；`minButtonH=28.5` 是分组头按钮的实际高度（功能项 33.4）。

「有效可见」用祖先链上的 `Visibility` 判定，而不是 `UIElement.IsVisible`：离屏检查时元素没挂到 Window 上，`IsVisible` 恒为 `false`（第一版实现就是这样把 54 个健康按钮全判红的）。

CSV 口径（谁有权改权威文件）：

- 权威五份 `_index.csv` / `_issues.csv` / `_hidpi.csv` / `_hidpi-issues.csv` / `_interact.csv` **只由全量运行改写**；子集运行（`--only` / `--pages`）落 `partial-_index.csv`、`partial-_issues.csv`，`--interact` 单页落 `partial-_interact.csv`（`QaReportWriter.FileName(name, partial)`）。所以本报告引用的「47 行」只可能来自一次全量运行，单页跑不会再把 47 行改成 1 行。
- 本报告数字的取证副本冻结在 `%TEMP%\qa-freeze\`：`_index.csv` 与 `_issues.csv` 时间戳 `2026-11-05 21:12:07`，`_hidpi.csv` / `_hidpi-issues.csv` 为 `21:12:13`，`_interact.csv` 为 `21:12:25`。若发现 `docs/qa-screens/*.csv` 的 mtime 后来被改动，以这份冻结副本对账。

## 3. 回归结果（相对 task-8）

| 项 | task-8 基线 | 本轮 | 判定 |
| --- | --- | --- | --- |
| 页面数 / 渲染成功 | 47 / 47 | 47 / 47 | 不变 |
| fatal | 0 | 0 | 不变 |
| High | 0 | 0 | 不变 |
| Medium | 1（`code.datamatrix` 空占位） | 0 | **已修** |
| Low | 13 | 6 | **净减 7** |
| 文本溢出 | 7 | 0 | **全清** |
| 无输入控件（观察项） | 6 | 6 | 不变 |
| 键盘不可达 / 乱码 / 裁切 | 0 / 0 / 0 | 0 / 0 / 0 | 不变 |

三条 lead 分派的修复逐条按像素复验（不是听说法）：

- **UI-01 `code.datamatrix` 占位为空 → 已修**：`docs/qa-screens/code.datamatrix.png` 的"解析信息"卡片中央出现「还没有校验结果：选择 Word/PDF 后点「开始校验」，结果按「文件 → 页 → 分类」展开。」，`_issues.csv` 不再有该页 Medium。
- **UI-02 `system.network` 两列硬裁 → 已修**：`system.network.png` 中 `VMware Network Adapter VMnet1`、`Microsoft Wi-Fi Direct Virtual Adapter #2`、`Bluetooth Device (Personal Area Network)` 等完整显示，DHCP 列显示「否」，该页文本溢出从 5 条降到 0。
- **UI-03 `exam.package` 标签削边 → 已修**：`exam.package.png` 的「被合并（目标）」完整渲染（改前 98.0px 文本装进 96.0px 槽，差 2px）。

回归期内**新冒出来又已修掉**的 3 组（我的中间轮抓到，最终轮已消失，留档以免重复排查）：`name.vertical`「每行一个姓名…」占位 549.7>532.7；`exam.insert`「后缀（可选）」84.0>65.3（`ExamInsertView.cs:40` 的 `Width=90`）；`exam.vote`「同意（默认）/不同意（默认）/弃权（默认）」84.0/98.0/84.0 > 75.3。

## 4. 高 DPI（120 / 144）

8 个代表页 × 2 档 DPI，落 `docs/qa-screens/hidpi/<id>@<dpi>.png`：`word.pdf`、`image.compress`、`code.barcode`、`exam.circle`、`exam.package`、`system.network`、`home`、`about`。

16 张全部 `ok`，且 `_hidpi-issues.csv` 只有 4 行——都是 `home`/`about` 的「无输入控件」观察项：**文本溢出 0、负坐标 0、图像未渲染 0、裁切 0**。像素尺寸 120→1500×1000、144→1800×1200，单张 63–136KB，文字边缘明显更清晰、布局与 96 DPI 完全一致（对比 `word.pdf@144.png` 与 `word.pdf.png`，卡片边界、按钮行、列表列宽逐像素对齐）。

**能力边界（必须写清楚，别把它当"真机缩放已验证"）**：WPF 布局以 DIP 为单位，`--dpi` 只提高栅格化分辨率，不模拟操作系统的 125%/150% 缩放重排。也就是说本检查能发现"随像素密度变化的绘制问题"（模糊、位图、亚像素、图像尺寸为 0），但**不能替代真机改缩放比例的验证**；若要覆盖后者，需要在 `App.config`/`ApplicationManifest` 层模拟（不在本轮范围）。

## 5. 交互态

### 5.1 结果（`_interact.csv`，4/4 ok，无 hang）

| 页面 | 动作 | 判定 | 关键状态行 |
| --- | --- | --- | --- |
| `word.pdf` | 空列表点「开始处理」 | ok，status=1 | 请先点「选择文件」把要处理的文件加进列表。 |
| `exam.vote` | 填 3 个姓名 → 点「生成报告单」 | ok，status=2 | `SaveFile` + `Confirm(已生成。是否打开文件？)` → 已生成 3 人的报告单：`%TEMP%\dtsnap-dialogs\exam.vote-选举结果报告单.docx` |
| `code.barcode` | 填内容 → 生成预览 → 复制识别内容 → 选择图片… → 识别条码 | ok，status=6 | 已生成预览：QA-1234567890；`present preview-image: images=1 drawn=1 size=260×260`；还没有识别结果：请先「识别条码」；图片文件不存在：…`code.barcode-file.dat` |
| `exam.circle` | 断空态引导 → 点「打开…」→ 点「开始匹配」 | ok，status=2 | `present empty-hint: 尚未载入图片`；图片不存在：…；`Info(提示): 请先打开考评表图片。` |

`code.barcode` 那条 `images=1 drawn=1 size=260×260` 是本轮新增的图像判定：证明预览图**真的画出来了**，不是"有 Image 元素但 0 尺寸"。

### 5.2 `exam.circle` 挂起的根因（task-8 遗留，已闭环）

- 现象：点「开始匹配」后子进程不返回，7 分钟后仍持有 `bin\DocumentTools.UiSnapshot.dll`，任何人的 `dotnet run --project tools\UiSnapshot` 都报 `MSB3027/MSB3021`。
- 根因：`src/DocumentTools.App/Features/Modules/Exam/ExamCircleView.cs:392`（同类调用还有 `:428`、`:480`）走 `_dialogs.Info("请先打开考评表图片。")`，而 `Info` 的实现是 `src/DocumentTools.App/Infrastructure/Dialogs/IDialogService.cs:55` 的 `MessageBox.Show(...)`（模态）。离屏渲染的 Dispatcher 泵没有 UI 线程消息循环可退，模态框一起就永久阻塞。
- 定性：**测试设施缺陷，不是应用缺陷**。应用里用户点这个按钮理应看到提示框，行为正确。
- 处置：harness 注入假 `IDialogService`（记录调用不阻塞）+ 单步看门狗；本轮 `interactHang=0`，`exam.circle` 3 秒内完成、trace 里能看到 `dialog Info(提示): 请先打开考评表图片。` 被记录后继续往下走。`ExamCircleView` 一行未改。

### 5.3 交互态顺带暴露的一处文案问题（已修）

`code.barcode` 识别失败时曾把异常原文直接贴到状态行：`识别失败：Image loading failed.`（英文），定位在 `src/DocumentTools.App/Features/Modules/Tools/BarcodeFeatureModule.cs:304` 的 `ToolViewKit.ShowStatus(_status, "识别失败：" + ex.Message, StatusTone.Error)`。**已由 ui-tools-agent 修掉**（现 `BarcodeFeatureModule.cs:310-318`）：状态行改三句中文「识别条码失败：这张图读不出来（可能不是图片、扩展名与内容不符、或文件正被其他程序占用）。请换一张 PNG/JPG 后重试。」，英文原文只留在 `_readResult.ToolTip = "技术详情：" + ex.Message` 与 `Debug.WriteLine`，换图/成功后 `ToolTip = null` 清空，与本项目"界面全中文、细节可查"的口径一致。本轮最终脚本走的是"文件不存在"分支，所以 `_issues.csv` 里本来就没有这条，属人工观察（见 UI-06）。

## 6. 逐页结果（47 行，来自 `_index.csv`，2026-11-05 21:12 全量运行）

| # | 分类 | 功能 ID | 标题 | 元素 | 按钮 | Tab | 首个输入 | H/M/L |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Home | `home` | 首页 | 702 | 50 | 46 | — | 0/0/1 |
| 2 | About | `about` | 关于软件 | 147 | 8 | 4 | — | 0/0/1 |
| 3 | Word | `word.pdf` | WORD 转 PDF | 291 | 20 | 15 | CheckBox「保存到源目录」 | 0/0/0 |
| 4 | Word | `word.merge` | 合并 WORD | 140 | 11 | 6 | — | 0/0/1 |
| 5 | Word | `word.split` | 拆分 WORD | 227 | 16 | 11 | CheckBox「保存到源目录」 | 0/0/0 |
| 6 | Word | `word.excel` | WORD 转 EXCEL | 166 | 13 | 8 | CheckBox「保存到源目录」 | 0/0/0 |
| 7 | Word | `word.image` | WORD/PDF/EXCEL 转图片 | 229 | 16 | 11 | CheckBox「保存到源目录」 | 0/0/0 |
| 8 | Word | `word.epub` | WORD 转 EPUB | 215 | 16 | 11 | CheckBox「保存到源目录」 | 0/0/0 |
| 9 | Word | `word.replace` | WORD 内容批量替换 | 350 | 29 | 20 | TextBox | 0/0/0 |
| 10 | Pdf | `pdf.word` | PDF 转 WORD | 219 | 16 | 11 | CheckBox「保存到源目录」 | 0/0/0 |
| 11 | Pdf | `pdf.excel` | PDF 转 EXCEL | 166 | 13 | 8 | CheckBox「保存到源目录」 | 0/0/0 |
| 12 | Pdf | `pdf.ppt` | PDF 转 PPT | 184 | 15 | 10 | CheckBox「保存到源目录」 | 0/0/0 |
| 13 | Pdf | `pdf.merge` | 合并 PDF | 140 | 11 | 6 | — | 0/0/1 |
| 14 | Pdf | `pdf.split` | 拆分 PDF | 261 | 18 | 13 | CheckBox「保存到源目录」 | 0/0/0 |
| 15 | Pdf | `pdf.watermark.add` | 添加 PDF 水印 | 483 | 32 | 27 | CheckBox「保存到源目录」 | 0/0/0 |
| 16 | Pdf | `pdf.watermark.remove` | 移除 PDF 水印 | 153 | 12 | 7 | CheckBox「保存到源目录」 | 0/0/0 |
| 17 | Pdf | `pdf.compliance` | PDF 版本转换 | 184 | 13 | 8 | CheckBox「保存到源目录」 | 0/0/0 |
| 18 | Pdf | `pdf.compress` | 压缩 PDF | 294 | 23 | 18 | CheckBox「保存到源目录」 | 0/0/0 |
| 19 | Pdf | `html.pdf` | HTML 转 PDF | 345 | 24 | 19 | CheckBox「保存到源目录」 | 0/0/0 |
| 20 | Excel | `excel.pdf` | EXCEL 转 PDF | 253 | 20 | 15 | CheckBox「保存到源目录」 | 0/0/0 |
| 21 | Excel | `excel.word` | EXCEL 转 WORD | 153 | 12 | 7 | CheckBox「保存到源目录」 | 0/0/0 |
| 22 | Excel | `excel.replace` | EXCEL 内容批量替换 | 256 | 21 | 12 | TextBox | 0/0/0 |
| 23 | Ppt | `ppt.pdf` | PPT 转 PDF | 219 | 16 | 11 | CheckBox「保存到源目录」 | 0/0/0 |
| 24 | Ppt | `ppt.split` | 拆分 PPT | 276 | 17 | 12 | CheckBox「保存到源目录」 | 0/0/0 |
| 25 | Image | `image.pdf` | 图片转 PDF | 283 | 20 | 15 | CheckBox「保存到源目录」 | 0/0/0 |
| 26 | Image | `image.compress` | 压缩图片 | 340 | 20 | 15 | CheckBox「保存到源目录」 | 0/0/0 |
| 27 | Image | `webp.image` | WEBP 转图片 | 184 | 13 | 8 | CheckBox「保存到源目录」 | 0/0/0 |
| 28 | Image | `image.ico` | 图片转 ICO | 193 | 14 | 9 | CheckBox「保存到源目录」 | 0/0/0 |
| 29 | Epub | `epub.word` | EPUB 转 WORD | 153 | 12 | 7 | CheckBox「保存到源目录」 | 0/0/0 |
| 30 | Epub | `epub.pdf` | EPUB 转 PDF | 153 | 12 | 7 | CheckBox「保存到源目录」 | 0/0/0 |
| 31 | Tools | `code.barcode` | 简易二维码 | 432 | 28 | 24 | TextBox | 0/0/0 |
| 32 | Tools | `code.datamatrix` | 特殊二维码校验 | 100 | 7 | 2 | — | 0/0/1 |
| 33 | Tools | `idcard.check` | 身份证校验 | 74 | 4 | 4 | TextBox | 0/0/0 |
| 34 | Tools | `name.vertical` | 姓名横转竖 | 274 | 22 | 18 | CheckBox「移除数字」 | 0/0/0 |
| 35 | Tools | `twain.debug` | Twain 扫描仪调试 | 172 | 9 | 7 | TextBox | 0/0/0 |
| 36 | Tools | `other.misc` | 其他功能 | 91 | 6 | 6 | — | 0/0/1 |
| 37 | Tools | `other.extra` | 补充功能 | 127 | 8 | 7 | TextBox | 0/0/0 |
| 38 | Exam | `exam.mss3` | MSS3 模板修改 | 228 | 14 | 13 | TextBox | 0/0/0 |
| 39 | Exam | `exam.single` | 单人表生成 | 207 | 13 | 11 | TextBox | 0/0/0 |
| 40 | Exam | `exam.insert` | 考评表信息插入 | 611 | 43 | 29 | TextBox | 0/0/0 |
| 41 | Exam | `exam.report` | 考评报表修改 | 491 | 33 | 27 | TextBox | 0/0/0 |
| 42 | Exam | `exam.vote` | 选举结果报告单样式生成 | 273 | 20 | 15 | CheckBox「保留序号列」 | 0/0/0 |
| 43 | Exam | `exam.package` | 考评系统数据包管理 | 122 | 10 | 9 | CheckBox「同时复制 .exam 文件」 | 0/0/0 |
| 44 | Exam | `exam.circle` | 考评圆圈修复工具 | 226 | 13 | 11 | ComboBox | 0/0/0 |
| 45 | System | `system.settings` | 系统设置 | 724 | 74 | 65 | CheckBox「转换完成后询问是否打开目标目录」 | 0/0/0 |
| 46 | System | `system.tools` | 系统工具 | 99 | 7 | 7 | ComboBox | 0/0/0 |
| 47 | System | `system.network` | 网卡设置 | 386 | 17 | 20 | TextBox | 0/0/0 |

## 7. 截图索引

基准 47 张（`docs/qa-screens/<功能ID>.png`，合计 2,066,887 字节，平均 43,976，最大 85,712）：`home` `about` `word.pdf` `word.merge` `word.split` `word.excel` `word.image` `word.epub` `word.replace` `pdf.word` `pdf.excel` `pdf.ppt` `pdf.merge` `pdf.split` `pdf.watermark.add` `pdf.watermark.remove` `pdf.compliance` `pdf.compress` `html.pdf` `excel.pdf` `excel.word` `excel.replace` `ppt.pdf` `ppt.split` `image.pdf` `image.compress` `webp.image` `image.ico` `epub.word` `epub.pdf` `code.barcode` `code.datamatrix` `idcard.check` `name.vertical` `twain.debug` `other.misc` `other.extra` `exam.mss3` `exam.single` `exam.insert` `exam.report` `exam.vote` `exam.package` `exam.circle` `system.settings` `system.tools` `system.network`。

交互态 4 张：`code.barcode@interact.png`、`exam.circle@interact.png`、`exam.vote@interact.png`、`word.pdf@interact.png`（各配一份同名 `.txt` sidecar，记录每步点击、弹窗调用与新出现的状态行）。

壳层与设置持久化证据 2 张（不是快照工具产出，由离屏探针渲染，保留原尺寸 1200×800 / 1440×900）：

- `docs/qa-screens/main-window.png` — 真实 `MainWindow` 1440×900：侧边栏 9 个分组、分组内功能项全部真实排布（`WORD` 组 7 项、`PDF` 组 10 项），这是 §2.6 壳层检查的可视对照；修复前同位置每个功能项高度为 0。
- `docs/qa-screens/settings-import-rejected.png` — `system.settings` 导入被拒态：底部红字给出「导入已取消：2 个字段非法，当前设置未改变」并逐条列出被拒字段（`Chrome 路径（文件不存在：…）`、`ListViewLineHeight（应为 18–64 之间的整数，实际是 999）`），中性提示「当前设置保持不变，界面上的改动尚未保存。」，主操作「保存设置」保持唯一强调色。

高 DPI 16 张（`hidpi/`，合计 1,448,492 字节）：`word.pdf` `image.compress` `code.barcode` `exam.circle` `exam.package` `system.network` `home` `about`，各出 `@120.png`、`@144.png`。

CSV：`_index.csv`（47 行逐页计数）、`_issues.csv`（6 行）、`_hidpi.csv`（16 行）、`_hidpi-issues.csv`（4 行）、`_interact.csv`（4 行）。

## 8. 问题清单

| 编号 | 严重度 | 页面 | 问题 | 证据 | 状态 |
| --- | --- | --- | --- | --- | --- |
| UI-01 | Medium | `code.datamatrix` | 占位框初始为空，用户不知道下一步 | task-8 `_issues.csv` + 截图 | **已修**（`DataMatrixFeatureModule.cs` 占位文案常量化），本轮像素复验通过 |
| UI-02 | Low | `system.network` | 名称/描述两列固定宽度硬裁，无省略号 | task-8 `_issues.csv` 5 条溢出 + 截图 | **已修**（改 CellTemplate + `CharacterEllipsis` + ToolTip），本轮溢出归零 |
| UI-03 | Low | `exam.package` | 标签列 96px，装不下「被合并（目标）」98px | task-8 `_issues.csv` | **已修**（`ExamViewKit` 标签列改 `Auto + MinWidth=96`），像素复验完整 |
| UI-04 | Low（观察） | `home` `about` `word.merge` `pdf.merge` `code.datamatrix` `other.misc` | 页面无输入控件，纯按钮/列表 | `_issues.csv` 6 行 | 保留，属设计取舍；如后续要"可测性"可加筛选框 |
| UI-05 | Low | `exam.insert` / `exam.vote` | 占位文案比输入框宽（「后缀（可选）」、"同意（默认）"等） | 本轮中间轮 `_issues.csv`（84.0/98.0 > 65.3/75.3） | **已修**（Exam designer），最终轮溢出归零 |
| UI-06 | Low（人工） | `code.barcode` | 识别失败时把异常原文贴到界面：`识别失败：Image loading failed.` | 交互态 trace + 原 `BarcodeFeatureModule.cs:304` | **已修**（ui-tools-agent；`BarcodeFeatureModule.cs:310-318`：主文案改成中文三句式「识别条码失败：这张图读不出来（可能不是图片、扩展名与内容不符、或文件正被其他程序占用）。请换一张 PNG/JPG 后重试，或先用画图打开确认能正常显示。」，英文原文只进 `_readResult.ToolTip = "技术详情：" + ex.Message` 与 `Debug.WriteLine`，换图/成功时 `ToolTip = null` 清空（`:269` `:292`）） |
| UI-07 | 设施 | 全体 | task-8 的离屏 `Border` 宿主让 Primary 按钮解析出透明背景，47 张图对主按钮系统性失真 | `--window-probe` 输出 `#FF1D6978` vs 离屏 `#00FFFFFF` | **已修**（改真实 Window 宿主 + 像素复验） |
| UI-08 | 设施 | 交互态 | 模态 `MessageBox` 阻塞 Dispatcher，子进程挂死并锁 `bin` | lead 抓到 PID 44228；`ExamCircleView.cs:392` → `IDialogService.cs:55` | **已修**（假 `IDialogService` + 10s 看门狗），`interactHang=0` |
| UI-09 | 设施 | 快照目录 | 多人并发跑同一工具互相覆写 `_index.csv`（实测 47 行被改成 7 行） | 两次 `_index.csv` mtime 落在我的运行之后 | **已修**（`.running.lock` 独占，抢不到打印 `BUSY` 并退出码 8） |
| UI-10 | High（应用，快照盲区） | 壳层 / 侧边栏 | 导航分组用 WPF `Expander` 时，展开后组内功能项高度为 0，**侧边栏显示分组标题但点不到功能** | 探针实测：裸 `Expander` `h=41.8`，放进 `ScrollViewer→StackPanel` 后 `h=19.8 innerH=0.0`；同位置 `docs/qa-screens/main-window.png` 修复后每项 33.4px | **已修**（ui-shell-agent 改：`Shell/CollapsibleSection.cs`，折叠只切 `Visibility`；2026-11-05 21:12 全量运行验证 `SHELL\|…problems=0`，并加 §2.6 壳层检查做回归锁） |
| UI-11 | Medium（应用） | `system.settings` | 保存失败/导入被拒/启动修正都只改状态行或弹框，用户看不到「哪几项不合法、哪些字段被拒」；导入导出能力没接进界面 | `_issues.csv` 无此条（离屏探针才暴露）；`docs/qa-screens/settings-import-rejected.png` | **已修**（ui-shell-agent 改：`SystemFeatureModules.cs` 校验红字行 + 保存失败红字 + 导入/导出按钮 + 启动修正可折叠提示；2026-11-05 21:12 全量运行 `medium=0` 验证） |
| UI-12 | Medium（应用） | 壳层 / 无障碍 | 分组头内容不是纯文本，读屏与快照都读到空名称；导航项没有自动化名称 | 本页快照首次出现 Medium「空文本按钮」（`parent=CollapsibleSection`） | **已修**（ui-shell-agent 改：`CollapsibleSection.cs` / `MainWindow.xaml.cs` 补 `AutomationProperties.SetName/HelpText` 与导航项 `SetName/SetAutomationId`；2026-11-05 21:12 全量运行 `medium=0` 验证） |

## 9. 人工可读性抽查（8 张，均逐张看过像素）

1. `word.pdf.png` — 列表区占上半屏，下方「输出与后续」「转换参数」两卡片分段清楚，底部操作条主次分明；空列表时靠状态行引导，视觉重心合理。
2. `code.datamatrix.png` — 列表 + 「解析信息」大卡片，空态有居中灰字引导（UI-01 修复后）；右下角状态提示把两步操作讲清了。
3. `system.network.png` — 10 行网卡表格 + 「IP 设置」卡片 + 底部 5 个操作按钮；修复后长设备名可完整阅读，是本次可读性提升最明显的一页。
4. `exam.package.png` — 「待合并（源）」「被合并（目标）」两行并列，标签不再削边，配合说明文字「选择两个数据包目录后先「检查状态」」很清楚。
5. `exam.circle.png` — 画布占屏 40%，中央「尚未载入图片。点上方「打开…」选一张考评表图片，再在图上框选圆圈模板。」把下一步讲明白了；参数行（框选目标/缩放/相似度/上限/旋转）与底部「开始匹配」「修复并保存」分组合理。
6. `code.barcode.png` + `code.barcode@interact.png` — 左表单右预览的双栏布局；交互态下预览区出现 260×260 二维码、状态行变红给出「图片文件不存在：…」精确路径，属可用的错误反馈。
7. `exam.insert.png` — 大表单页（536 元素/38 按钮），分区明确（数据源/考评表/项目与选项），失效主按钮「插入信息」置灰 + 状态行「还缺：考评库、考评表名称。」，禁用理由可读。
8. `hidpi/word.pdf@144.png` 与 `hidpi/system.network@144.png` — 与 96 DPI 版本逐元素对齐，文字边缘锐利，144 DPI 下无新增裁切。

## 10. 残余风险

1. 只覆盖**初始态 + 4 个交互态**；点击后的多级中间态（选完文件、扫描完成、批量处理中、错误恢复）仍要靠 `tools/QaFunction` 或人工补。
2. `--dpi` 提升的是栅格化分辨率，不模拟 OS 125%/150% 缩放重排；真机缩放仍需人工看一次。
3. 文本溢出用 `FormattedText` 测算 + `LayoutSlot` 取槽宽，存在亚像素误差（容差 1.5px）；2px 级别的边缘问题会报得偏保守。
4. 图标按钮只判"有无 ToolTip/自动化名称"，语义是否正确（图标与功能是否匹配）机器判不了。
5. 假 `IDialogService` 让"确认类"弹窗一律答"否"，因此"确认后继续执行"的分支不在自动覆盖内；对真实用户流程这是盲区。
6. `docs/qa-screens` 是共享目录，本轮已加 `.running.lock`，但**用旧二进制或加 `--out` 的并发运行仍会覆盖别人产物**；对外发布结论时请以本报告标注的 21:03 一轮为准。
7. 交互脚本按按钮文案定位（`FindButton` 用 `Contains`），文案改动会导致脚本 `MISS button`——这是有意的硬失败（fail loud），不是静默跳过。
8. 壳层检查（§2.6）只覆盖 `NavHost` 里的导航分组与分组内按钮，不覆盖标题栏、面包屑、状态行等壳层细节；它证明的是「导航点得到」，不是「壳层每个像素都对」。
9. 离屏壳检查用祖先 `Visibility` 判可见，等价于「没被折叠」，**不等于真机上的遮挡关系**（如被弹层盖住）；那类问题仍需截图目视。