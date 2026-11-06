# DocumentTools 功能交互规范（WPF 迁移权威口径）

> 权威链：`docs\architecture.md`（分层与设计模式硬约束）＞ **本文件**（壳层 / 布局 / 参数 / 反馈）＞ `docs\feature-gap.md`（完成度现状）。
> 写法约定：每条规范都给出**实现位置**（`文件:行号`）与**为什么这样规定**。规范与代码冲突时，先改代码并留下证据，再同步本文件；不允许「照着文档改代码」却拿不出证据。
> 引用约定：正文里的 `文件:行号` 一律可回溯（同一段落内后续出现的 `:行号` 指向前一个完整路径）；改代码后行号会漂移，改完必须回到本节自查。
> 本次核对：2026-11-05，对照工作树 `feat/wpfui-migration`，`tools\FeatureSmoke` 实测 47/47。

---

## 1 全局框架

### 1.1 窗口与壳层

| 元素 | 规定 | 实现位置 |
|------|------|----------|
| 主窗口 | `FluentWindow`，1440×900，最小 1180×740，居中启动，Mica 背景，内容延伸进标题栏 | `src\DocumentTools.App\Shell\MainWindow.xaml:6-13` |
| 标题栏 | `ui:TitleBar` + `DocumentPdf24` 图标（18） | `src\DocumentTools.App\Shell\MainWindow.xaml:20-24` |
| 左栏宽度 | 固定 296，右列 `*` | `src\DocumentTools.App\Shell\MainWindow.xaml:28-29` |
| 左栏外观 | `ControlFillColorDefaultBrush` 底 + 右侧 1px 描边 + `Padding 12,10,12,8` | `src\DocumentTools.App\Shell\MainWindow.xaml:33-37` |
| 导航标题 | 「功能导航」，`BodyStrong` | `src\DocumentTools.App\Shell\MainWindow.xaml:39-42` |
| 搜索框 | `PlaceholderText="搜索功能  Ctrl+K"`，`Search24` 图标 | `src\DocumentTools.App\Shell\MainWindow.xaml:43-49` |
| 底部版本 / 启动提示 | `NavVersion`、`NavStatus`（默认 `Collapsed`，一次性说明启动回退） | `src\DocumentTools.App\Shell\MainWindow.xaml:50-62` |
| 导航滚动区 | 纵向 `Auto`、横向 `Disabled`（左栏永不出现横向滚动条） | `src\DocumentTools.App\Shell\MainWindow.xaml:63-74` |
| 内容区 | 白底 + `ContentHost`，`Padding 16,4,16,16` | `src\DocumentTools.App\Shell\MainWindow.xaml:78`、`:96-98` |
| 面包屑 | 分类图标（14）+ 分类名（12、弱色），`Margin 20,14,20,0` | `src\DocumentTools.App\Shell\MainWindow.xaml:84-95` |

**为什么**：左栏定宽、内容区取 `*`，窗口拉伸只放大内容区，导航不会跟着变宽把功能名挤成两行。面包屑**只标分类**（`src\DocumentTools.App\Shell\MainWindow.xaml:83` 注释、`src\DocumentTools.App\Shell\MainWindow.xaml.cs:296`），因为页面自身已有 22px 大标题（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:21`）；两级大标题会白占一行高度。

### 1.2 导航行为（分类 Expander / 搜索 / 激活）

1. **分组**：按 `FeatureCategory` 分组并按分类排序（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:134-138`）；组头 = 分类图标 + 分类名（13、SemiBold）+ 右侧计数（12、弱色）（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:198-225`）。
2. **单入口分类不套折叠壳**：该分类只有一个功能时直接渲染导航按钮（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:147-152`）。**为什么**：否则会出现「首页 / 首页」这种父分类与子项同名的重复层级。
3. **搜索**：`Ctrl+K` 聚焦并全选搜索框（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:74-81`）；输入即重建列表（`:98-99`）；`Enter` 打开第一条匹配（`:102-111`）；`Esc` 有文本先清空、已空则退出搜索态（`:82-95`）。
4. **搜索态强制展开**命中的分组，且**不覆盖**用户手动的折叠状态（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:155-160`；状态字典见 `:26-27`）。**为什么**：搜索结束后左栏要回到用户原来的形状，而不是被搜索「顺带改瞎」。
5. **空态文案**：搜索无命中「没有匹配「{query}」的功能，试试更短的关键词。」；无任何注册「功能清单尚未注册：请检查功能模块是否已加载。」（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:140-142`）。
6. **激活**：清掉一次性启动提示（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:260-262`）→ 取/建视图（`:326-347`，命中缓存；创建异常弹 `Error`）→ 更新面包屑（`:296-308`）→ 换内容（`:274`）→ **强制展开所属分类**（`:277-288`）→ 高亮当前项（`:310-324`）。
7. **高亮**：当前项 `Secondary`、其余 `Transparent`，并在 `DispatcherPriority.Loaded` 时 `BringIntoView`（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:310-323`）。**为什么**：折起来的分类刚展开时还没完成布局，立刻滚动会滚不到位。
8. **启动落点**：`NavigationFallback.Resolve(StartFeatureId, 模块 id, 隐藏 id)`，回退原因写进 `NavStatus`（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:54-70`）。
9. 导航按钮：透明外观、`ListCornerRadius` 圆角、`Padding 10,7,10,7`、`ToolTip` 取功能描述（`src\DocumentTools.App\Shell\MainWindow.xaml.cs:239-251`）。

**验收方式**：`dotnet run --project tools\FeatureSmoke -v q --nologo` → 输出「功能总数 47，创建成功 47，布局通过 47」（该工具会真实实例化 `MainWindow`，见 `tools\FeatureSmoke\Program.cs:66-73`）；再目视 `docs\qa-screens\home.png` 的分类分组、以及任意页面截图里的导航高亮。

### 1.3 页面原型目录

每种原型都有明确的布局入口，新建页面必须落在其中一种，不要自创第三种纵向顺序。

| 原型 | 布局入口 | 代表实现 |
|------|----------|----------|
| A 转换列表（列表 + 参数 + 操作条） | `BuildBottomActionLayout` | `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:134` |
| B 合并（同 A，收尾用 SaveFile 而非文件夹） | `BuildBottomActionLayout` | `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:340-349` |
| C 分区转换（同 A，分区由 Core Schema 给） | `BuildBottomActionLayout` | `src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:55` |
| D 替换（列表 + 查找/替换对 + 选项区限高） | `BuildToolLayout`（4 参） | `src\DocumentTools.App\Features\Modules\Tools\ReplaceFeatureModule.cs:120`、限高 `src\DocumentTools.App\Features\Modules\Tools\ReplaceFeatureModule.cs:47` |
| E 工具页（无文件列表，内容 + 状态 + 操作条） | `BuildToolLayout` | `src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:124`、`src\DocumentTools.App\Features\Modules\Tools\NameVerticalFeatureModule.cs:169`、`src\DocumentTools.App\Features\Modules\Tools\IdCardFeatureModule.cs:63`、`src\DocumentTools.App\Features\Modules\Tools\DataMatrixFeatureModule.cs:92` |
| F 考评页（路径/模板/进度多分区，不是通用转换壳） | `BuildBottomActionLayout` 变体 | `src\DocumentTools.App\Features\Modules\Exam\ExamCircleView.cs:145`、`src\DocumentTools.App\Features\Modules\Exam\ExamPackageView.cs:63`、`src\DocumentTools.App\Features\Modules\Exam\ExamVoteView.cs:79`、`src\DocumentTools.App\Features\Modules\Exam\ExamSingleView.cs:59`、`src\DocumentTools.App\Features\Modules\Exam\ExamMss3View.cs:98` |
| G 首页 / 设置（页面自管内容区） | 自组装滚动区 | `src\DocumentTools.App\Features\Modules\Home\HomeFeatureModule.cs:90`（`SetBody(new ScrollViewer…)`）、卡片 `src\DocumentTools.App\Features\Modules\Home\HomeFeatureModule.cs:126-169`；`src\DocumentTools.App\Features\Modules\SystemTools\SystemFeatureModules.cs:127-135` |

功能清单以 `src\DocumentTools.Core\Features\BuiltInFeatures.cs` 为唯一来源（47 条）；「哪些功能属于 A/B/C」由 `ConvertFeatures` 决定（`src\DocumentTools.Core\Features\BuiltInFeatures.cs:77-88`）。

**验收方式**：`tools\FeatureSmoke` 47/47（每页都真实 `Create` + `Measure/Arrange/UpdateLayout`）；抽查 `docs\qa-screens\word.pdf.png`（A）、`pdf.merge.png`（B）、`pdf.watermark.add.png`（C）、`word.replace.png`（D）、`code.barcode.png`（E）、`exam.package.png`（F）、`home.png`（G）。

### 1.4 不迁移项（故意不做）

- `FormWidth` / `FormHeight` / `WidgetAnchorMode` **不进设置页、不进配置**：窗体尺寸与控件锚点交给 WPF 布局（`src\DocumentTools.Core\Settings\AppSettings.cs:7`；设置页文案 `src\DocumentTools.App\Features\Modules\SystemTools\SystemFeatureModules.cs:76`）。
- 旧 `StartPageIndex`（Tab 序号）**不按序号映射页面**，只回落 `home`（`src\DocumentTools.Core\Settings\AppSettings.cs:101-102`）。
- 参见 `docs\backlog-next.md` 中「`system.settings` 不含窗体大小/锚点」条目（本节即其引用目标）。

**为什么**：Tab 序号与窗体像素都是旧框架的布局补偿；在 WPF 里保留它们只会在换 DPI/换主题时制造假精度，并让设置项与真实布局脱钩。

**验收方式**：打开 `system.settings` 页（快照 `docs\qa-screens\system.settings.png`）确认只有「交互 / 启动与隐藏 / 路径」三张卡片，没有窗体尺寸或锚点字段。

---

## 2 设计标记（UiTokens 单源）

`src\DocumentTools.App\Ui\UiTokens.cs` 是间距、字号、圆角、中性色的**唯一来源**；视图里不要再散落硬编码值（该文件 `:6-10` 的注释即此规定）。

### 2.1 间距（4 的倍数）

| Token | 值 | 位置 |
|-------|----|------|
| `SpaceXs` | 4 | `src\DocumentTools.App\Ui\UiTokens.cs:14` |
| `SpaceSm` | 8 | `src\DocumentTools.App\Ui\UiTokens.cs:15` |
| `SpaceMd` | 12 | `src\DocumentTools.App\Ui\UiTokens.cs:16` |
| `SpaceLg` | 16 | `src\DocumentTools.App\Ui\UiTokens.cs:17` |
| `SpaceXl` | 20 | `src\DocumentTools.App\Ui\UiTokens.cs:18` |

### 2.2 字号（四级 + 页标题）

| Token | 值 | 用途 | 位置 |
|-------|----|------|------|
| `TitleFontSize` | 22 | 页面标题（每页一个） | `src\DocumentTools.App\Ui\UiTokens.cs:21` |
| `SectionTitleFontSize` | 14 | 分区卡片标题 | `src\DocumentTools.App\Ui\UiTokens.cs:22` |
| `SubtitleFontSize` | 13 | 页面描述 | `src\DocumentTools.App\Ui\UiTokens.cs:23` |
| `BodyFontSize` | 13 | 正文 / 导航项 / 卡片标题 | `src\DocumentTools.App\Ui\UiTokens.cs:24` |
| `CaptionFontSize` | 12 | 辅助文字 / 计数 / 面包屑 / 版本号 | `src\DocumentTools.App\Ui\UiTokens.cs:25` |

### 2.3 厚度与圆角

| Token | 值 | 位置 |
|-------|----|------|
| `PagePadding` | `4` | `src\DocumentTools.App\Ui\UiTokens.cs:28` |
| `CardPadding` | `14` | `src\DocumentTools.App\Ui\UiTokens.cs:29` |
| `CardMargin` | `0,0,0,12` | `src\DocumentTools.App\Ui\UiTokens.cs:30` |
| `SectionTitleMargin` | `0,0,0,8` | `src\DocumentTools.App\Ui\UiTokens.cs:31` |
| `RowGap` | `0,0,0,8` | `src\DocumentTools.App\Ui\UiTokens.cs:32` |
| `ButtonPadding` | `14,6,14,6` | `src\DocumentTools.App\Ui\UiTokens.cs:33` |
| `ButtonGap` | `0,0,8,0` | `src\DocumentTools.App\Ui\UiTokens.cs:34` |
| `CardCornerRadius` | `8` | `src\DocumentTools.App\Ui\UiTokens.cs:36` |
| `ListCornerRadius` | `6` | `src\DocumentTools.App\Ui\UiTokens.cs:37` |
| `ContentMaxWidth` | `980`（表单/卡片区最大宽度） | `src\DocumentTools.App\Ui\UiTokens.cs:40` |

### 2.4 中性色

| Token | 值 | 位置 |
|-------|----|------|
| `BorderBrush` | `#D9D9D9` | `src\DocumentTools.App\Ui\UiTokens.cs:42` |
| `DividerBrush` | `#ECECEC` | `src\DocumentTools.App\Ui\UiTokens.cs:43` |
| `MutedBrush` | `#6E6E6E` | `src\DocumentTools.App\Ui\UiTokens.cs:44` |
| `SurfaceBrush` | White | `src\DocumentTools.App\Ui\UiTokens.cs:45` |
| `SubtleBrush` | `#F7F7F7` | `src\DocumentTools.App\Ui\UiTokens.cs:46` |

页面底色恒为白（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:35`），因此中性色按浅色主题取值；所有画刷在 `src\DocumentTools.App\Ui\UiTokens.cs:48-54` 由 `Rgb(...)` 建好后立即 `Freeze()`，避免跨线程复用时报错。

### 2.5 允许的特例（写清理由，别随手加）

1. **状态四色**：`Tomato` / `ForestGreen` / `SteelBlue` / `MutedBrush`（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:137-143`）——语义色不属于中性 token。
2. **转换页主操作按钮**：`Padding 18,8,18,8`、`MinWidth 120`、`MinHeight 36`（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:446-453`）——转换页操作条只有两个按钮，放大触控目标；工具页按钮仍用 `ButtonPadding`（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:308-319`）。
3. **图标字号**：`FeatureIcons.Create` 的默认参数（`src\DocumentTools.App\Ui\FeatureIcons.cs:98`、`:107`）与 `NavIconSize=16` / `CardIconSize=20` / `GroupIconSize=15`（`:12-16`）。
4. **身份证输入框**：`FontSize = 16`（`src\DocumentTools.App\Features\Modules\Tools\IdCardFeatureModule.cs:26`）——18 位身份证号需要更宽的字距可读性。

XAML 侧必须走 `{x:Static tokens:UiTokens.*}`，不要写字面量（示例 `src\DocumentTools.App\Shell\MainWindow.xaml:53-54`、`:70`、`:78`、`:90`）。

**验收方式**：`Select-String -Path src\DocumentTools.App\**\*.cs -Pattern 'FontSize\s*=\s*\d'`，命中只应落在上表第 3、4 条；`Select-String -Path src\DocumentTools.App\**\*.xaml -Pattern 'FontSize="\d'` 只应有标题栏图标（`src\DocumentTools.App\Shell\MainWindow.xaml:22`）。目视任取两页快照，卡片圆角/边框/内边距一致。

---

## 3 页面骨架与布局规则

### 3.1 页头契约

`FeaturePageBase` 统一提供标题 + 描述（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:18-37`）：

- 标题 22 / SemiBold（`:21-22`）；描述 13 / 弱色、可换行（`:25-28`）。
- 描述缺省值「选择文件后执行。」（`:24`）——**这是兜底而不是范本**，每个功能都应写清「产出什么」，见 §7.1。
- 页头 `Margin 0,0,0,8`、`Dock.Top`；页面 `Padding = PagePadding`、底色白（`:14`、`:32`、`:35-36`）。
- `SetBody` 只允许一个 body：先删掉索引 >1 的子元素再挂新 body（`:39-47`）。**为什么**：防止「换一次内容就多一层容器」，布局深度一涨，`UpdateLayout` 与 Tab 序列都会跟着坏。

### 3.2 两种底部布局（纵向顺序是硬约束）

**A. 转换壳**（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:52-70`）：

```
[页头：标题 / 描述]
[内容填满：文件列表（右侧可挂竖排侧钮）]
[参数区（可选，贴底之上）]
[操作条（永远最底）：选择文件 | 状态 …… 开始处理]
```

实现顺序即规矩：先 `Dock.Bottom` 挂操作条，再挂参数区，最后把内容作为填充子元素（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:58-68`）。

**为什么操作区必须在底部**：列表是「会变长」的那一半。列表在上、按钮贴底时，加 200 个文件只会让列表内部滚动，按钮始终在同一屏幕位置；反过来把操作条放在列表上方，文件一多按钮就被顶出视口，用户必须先滚回顶部才能开始处理。代码里靠 Dock 顺序保证这件事：先 `Dock.Bottom` 挂操作条、再挂参数区，最后挂进去的列表成为唯一填充项（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:58-68`，`:67` 即 `// Last child fills remaining space (list).`）。

**B. 工具壳**（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:75-91`）：

```
[页头]
[内容填满（FormArea / Grid）]
[可选参数带]
[状态行]
[操作条]
```

状态行与操作条被包进同一个底部 `StackPanel`（`Margin 0,4,0,0`，先状态后按钮，`:87-89`），因此**状态行永远在按钮上方一行**，不会随内容高度漂移。

### 3.3 分区卡片

`SectionCard(title, body)`（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:93-115`）：标题 14 / SemiBold / `SectionTitleMargin`；外框 1px `BorderBrush`、`CardCornerRadius`、`CardPadding`、`CardMargin`、白底。`ToolViewKit.Card` 是同一套 token 的另一份实现（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:30-56`），两处必须保持视觉一致。

**为什么保留分区**：旧的 `GroupBox.Text` 是业务分区名（「输出」「转换参数」「拆分设置」），它是用户找参数的第一层索引；揉成一张无名表单后，参数一多就只能靠肉眼扫。

### 3.4 转换页三段式（A 型的实际构成）

- 列表：`BorderedFileList`，最小高 180，网格线/行高读设置（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:77-82`）。
- 侧钮：右侧竖排、`Margin 8,0,0,0`、垂直居中（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:159-162`），四个按钮顺序固定「上移 / 下移 / 移除选中 / 清空列表」（`:148-151`），按钮 36×40 + `ToolTip`（构造函数 `:680-693`，四处调用都在 `:159-162` 传 `toolTip`）。
- 参数区：Core Schema 渲染进 `_optionsHost`（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:120`）。
- 操作条：`DockPanel`，左「选择文件」（Primary + `FolderOpen24`）、右「开始处理」（Success + `Play24`）、中间状态文字（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:122-132`）。状态文字 `Margin 12,0,12,0` 且自动换行（`:54-59`）。

**为什么侧钮竖排在列表右侧**：上移/下移直接对应「行序即合并顺序」，贴在列表右边缘，视线不用离开列表；图标 + ToolTip 换取列表宽度（列宽见 `src\DocumentTools.App\Ui\Controls\BorderedFileList.cs:48-50`：序号 56 / 文件名 280 / 完整路径 520）。

### 3.5 工具页两栏（可选，不是默认）

内容多时允许左右两栏（左表单、右预览）：`1.25*` / `1*`，右列 `Margin 12,0,0,0`（`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:107-114`）。左栏用 `FormArea` 包裹以保证纵向滚动（`:106`）。**不要**在两栏里再各放一套底部操作条——操作条只有一条。

**验收方式**：`docs\qa-screens\word.pdf.png` 目视「列表占满、参数区贴底之上、操作条在最底」；`code.barcode.png` 目视两栏与单条操作条；`tools\FeatureSmoke` 全部 47 页布局通过（布局异常会出现在它的「布局失败」清单里）。

---

## 4 构件与控件约定

所有页共用 `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs`；它是构件的**唯一实现**，考评页的 `ExamViewKit` 只是转发薄适配（保留 96px 标签列与 bool 错误位，见 `:20-23`）。新页面不要复制粘贴构件代码。

### 4.1 字段与行

| 构件 | 规定 | 位置 |
|------|------|------|
| `Field(label, control, trailing?, labelWidth=88)` | 3 列 `Auto / * / Auto`；标签 `MinWidth=88`、弱色；控件拉伸；行 `Margin 0,0,0,8` | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:59-98` |
| `Row(params)` | 横向控件组（勾选项），`WrapPanel`，`Margin 0,0,0,8` | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:101-110` |

**为什么用 `Auto + MinWidth` 而不是固定宽度**：固定宽度会把「被合并（目标）」这类长标签削边；只用 `Auto` 又会让各行标签列宽度不一、控件左边缘参差（理由见 `:63` 注释）。

### 4.2 状态行与四态

`StatusTone` 只有四个值：`Neutral / Busy / Success / Error`（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:11-18`）。

| 语气 | 颜色 | 何时用 |
|------|------|--------|
| `Neutral` | `MutedBrush` | 初始引导、已取消 |
| `Busy` | `SteelBlue` | 长任务进行中 |
| `Success` | `ForestGreen` | 全部成功 |
| `Error` | `Tomato` | 失败 / 被拒收 / 部分失败 |

实现：`ToolViewKit.Status(text)` 建行（弱色、可换行、`Margin 0,0,0,8`，`:113-127`），`ShowStatus(target, text, tone)` 只改文字与颜色（`:129-144`）。**页面的状态行必须复用同一个 `TextBlock`**（转换页即 `_status`，`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:54-59`），不要每条消息新建一个 TextBlock 往页面里塞。

### 4.3 按钮层级：一页只有一个主操作

- `ToolViewKit.Primary` / `Secondary`（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:156-160`），`ActionBar` 的规定写在 `:162` 注释里：**主操作放第一个，其余一律次级**。
- 工具页范例：`生成预览` 为 Primary，`批量生成 / 保存 PNG / 批量保存 / 复制图片` 全为 Secondary（`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:116-122`）。
- 转换页范例：`选择文件` 为 Primary、`开始处理` 为 Success 色（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:123-129`）——`Success` 在这里表示「执行/产出」，是**语气**而非第二个主按钮；同页不得再出现其他 Primary。
- 设置页范例：`保存` Primary，`重新加载` / `打开配置文件` Secondary（`src\DocumentTools.App\Features\Modules\SystemTools\SystemFeatureModules.cs:113-121`）。

**为什么**：一页两个 Primary 等于没有主操作；用户看到两个同权重按钮只能靠读文字来决定下一步。「主操作 = 这一页的产出动作」（转换页的产出由「开始处理」承担，但「选择文件」是进入流程的唯一入口，故取 Primary 语汇、把执行色留给 Success）。

### 4.4 内容区 / 预览框 / 空态

| 构件 | 规定 | 位置 |
|------|------|------|
| `FormArea` | 纵向 `Auto`、横向 `Disabled`、`Padding 0,0,4,0`；自身 `IsTabStop=false`，Tab 导航 `Continue` | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:175-197` |
| `Frame` | 1px `BorderBrush` + `ListCornerRadius` + 白底 + `MinHeight 160` | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:239-248` |
| `Placeholder` | `Frame` + `SubtleBrush(#F7F7F7)` 底 + 居中弱色提示 | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:251-257`、`:296-306` |
| `ShowPlaceholder` | 同一个框在「空态」与「真实内容」之间切换，并切换底色 | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:260-274` |

**为什么 `FormArea` 不进食 Tab 序列**：否则 Tab 的第一个落点会停在滚动容器本身，用户要多按一次才能进第一个输入框（`:190-192` 注释）。

**空态文案要求**：必须写成「下一步能做什么」，而不是「没有内容」。现有合格样例：「生成后在此预览条码：单行显示 1 个，批量显示第 1 个。」（`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:95`）、「还没有校验结果：选择 Word/PDF 后点「开始校验」，结果按「文件 → 页 → 分类」展开。」（`src\DocumentTools.App\Features\Modules\Tools\DataMatrixFeatureModule.cs:34`）。

### 4.5 文件选择与路径字段

- 页面级单选/多选一律走 `IDialogService`（`src\DocumentTools.App\Infrastructure\Dialogs\IDialogService.cs:8-18`），视图内不得直接 `new OpenFileDialog`。
- 侧栏式「选文件」按钮（工具页里的小按钮）用 `ToolViewKit.PickFile/PickFolder` 写回 TextBox（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:278-294`）。
- 转换页「选择文件」用同一份过滤串：按钮与列表拖放都读 `FeatureOptionSchemas.GetFileFilter(id)`（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:91` 设 `FileFilter`、`:111` 传给 `OpenFiles`）。

### 4.6 键盘语义

`ToolViewKit.AttachPageKeyboard(page, primary, onEscape)`（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:203-236`）：

- 无修饰键 `Enter` = 触发本页主操作；焦点在 `TextBoxBase / ButtonBase / ComboBox / TreeViewItem / ListViewItem / MenuItem` 时**不拦**（`:228-231`）。
- `Esc` = 收起/复位当前提示，**绝不退出页面、绝不改数据**（`:212-221`）。
- 工具页在构造末尾挂上，并给 `Esc` 一个初始提示文案（`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:126-130`）；替换页同样（`src\DocumentTools.App\Features\Modules\Tools\ReplaceFeatureModule.cs:123`）。

### 4.7 无障碍（读屏）

- 状态行：`AutomationProperties.Name = "状态提示"`、`LiveSetting = Polite`（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:124-125`）；`ShowStatus` 按语气切成 `Assertive`（失败/进行中）或 `Polite`（成功/中性）（`:131-134`）。
- 空态框：`Name = "空： " + text`；有内容时清空名称，避免读屏继续念空态（`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:255`、`:265-273`）。
- 只有图标的按钮必须给 `ToolTip`（转换页侧钮 `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:468`）。

### 4.8 设置页控件

设置页用与转换页相同的底部布局：内容区可滚动 + 底部「反馈行 + 按钮条」（`src\DocumentTools.App\Features\Modules\SystemTools\SystemFeatureModules.cs:127-135`）。内容区 `MaxWidth = ContentMaxWidth`、左对齐（`:88-92`），四张卡片：「交互」「启动与隐藏」「路径」+ 配置文件位置说明（`:93-111`）。

**验收方式**：`dotnet run --project tools\UiSnapshot -- --self-test`（自检期望 `SELFTEST RESULT passed=8/8`，见 `docs\qa-report-ui.md:30-40`）；随后 `docs\qa-screens\_issues.csv` 中不得出现 High。`_issues.csv` 现有的 Low 项（「无输入控件」类观察项、`name.vertical`/`exam.insert` 文本溢出）在处理前逐条判断，不要机械清零。

### 4.9 输入控件政策（判定表）

本节是**判定依据**，不是建议。落地与逐页整改见 task-42 产出的 `docs\ux-shell-polish.md`（壳层）与 task-43 产出的 `docs\ux-tools-polish.md`（工具/转换页）；自动化审查结果见 task-44 产出的 `docs\qa-report-ux.md`。

**四条政策**

1. **路径类输入只读**：凡是「指向磁盘上某个文件/目录」的输入框，`IsReadOnly = true`，只能由「选择…」按钮或拖放写入，不允许手打。理由：手打路径必然产生不存在/无权限/相对路径三类脏输入，且与 `PathGuard` 的判定口径重复。
2. **路径框必须有选择按钮且宽度自适应**：只读框右侧紧邻一个「选择…/打开…」按钮（实现走 `ToolViewKit.PickFile` / `ToolViewKit.PickFolder`，`src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:350` / `:359`），框本身 `HorizontalAlignment = Stretch`，不设固定 `Width`。
3. **最小宽度**：任何输入控件的可见宽度不得小于下表下限；文本被裁切且无省略号视为不合格。
4. **父级导航行高 ≥ 34px**：左栏分类父节点的整行点击高度不小于 34px（含内边距），文字与展开指示器垂直居中。

**判定表**

| 场景 | 判据（可机械核对） | 合格 | 不合格 |
| --- | --- | --- | --- |
| 文件路径框 | `IsReadOnly == true` 且同一行存在选择按钮 | 只能通过按钮/拖放改值 | 可手打、或没有选择按钮 |
| 目录路径框 | 同上 | 同上 | 同上 |
| 例外：正文字段 | 输入的是内容而非路径（如 HTML 源码/URL、查找与替换词、姓名列表、条码内容） | 可编辑，多行时给占位提示 | 被误设为只读 |
| 数字框 | 可见宽度 ≥ 64px，且单位写进标签（如「字号」「超时秒」「最大DPI」「缩放%」） | 数值完整可见、单位不靠猜 | 只写「缩放」不给单位；数字被裁 |
| 字典/短文本 | 可见宽度 ≥ 120px | 常见值完整可见 | 显示成「宋…」「居中对…」 |
| 路径框宽度 | 可见宽度 ≥ 240px 且随窗口拉伸 | 长路径可见尾部 | 固定 120px 导致只剩盘符 |
| 侧栏父节点 | 行高（含 padding）≥ 34px | 点击命中整行 | 行高 < 34px、必须精确点文字 |
| 文本溢出 | `docs\qa-screens\_issues.csv` 中该页无新增 Medium/High 类溢出项 | 无裁切或带省略号 | 截断且无省略号（如 `system.network` 的列宽项） |

**逐页落实情况**以 `docs\qa-report-ux.md` 的逐条结论为准；本节只定义「什么算不合格」，避免各页自行发明标准。

---

## 5 反馈与状态

### 5.1 三阶段：初始 → 进行中 → 结束

每一个会跑业务的页面都要有明确的这三段，且**都写在同一个状态行**里。

1. **初始态**：告诉用户从哪一步开始。转换页：「先添加文件并设置参数，再点「开始处理」。也可把文件拖进列表。」（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:98`）。
2. **进行中**：`Busy` 语气的进度文案 + 禁用会重复触发的按钮。
3. **结束**：`Success` / `Error` 语气的汇总文案，并保留结果（不要 3 秒后自动消失）。

### 5.2 进行中：必须 Busy、必须禁用、必须报粒度

- 进入运行先禁用「开始处理」，`finally` 里恢复（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:153-156`、`:200-206`）。
- 逐文件进度：「正在处理第 {i+1} 个文件 / 共 {total} 个：{fileName}」，写回源目录时追加「（保存到本文件所在目录：{dir}）」（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:272-277`）。
- 批处理（合并成 1 个 PDF）：「正在合并 {n} 张图片为 1 个 PDF…」（`:327`）；合并：「正在合并 {n} 个文件…」（`:360`）。
- 每轮之间必须让出 UI 线程（`await Task.Delay(1)`，`:278`、`:328`），否则状态行不会重绘，用户看到的是「点了没反应」。

**为什么必须有粒度**：一批 200 个文件时，只有「正在处理…」等于没有反馈，用户无法判断是卡住还是在进行；「第 N / 共 M + 文件名」让用户知道当前卡在哪个文件。

### 5.3 结束：汇总模板

`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:301-304`：

- 全成功：`处理完毕，成功 {n} 个文件{writtenBack}。`
- 有失败：`处理完毕，成功 {n} 个{writtenBack}，失败 {m} 个。{前 3 条原因}`，超出部分追加 `（另有 {k} 个失败未列出）`。
- `writtenBack` = `，已分别写回各自源目录`，只在写回多个源目录时出现（`:299`）。
- 颜色：全部成功 `Success`，有失败转 `Error`（`:390-393`）。

**为什么只列 3 条原因**：状态行是单行区域，完整失败清单会把它撑成一段无法阅读的墙；3 条足够让人判断是哪一类问题（权限 / 加密 / 格式），细节交给日志与重跑。

### 5.4 取消

取消任何目录/保存选择都静默返回，只留状态行「已取消，没有处理任何文件。」（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:215-216`；调用点 `:175`、`:194`）。**不要**为取消弹提示框，也不要留在「正在处理」。

### 5.5 `Confirm` 只能用于两类

1. **破坏性 / 不可撤销**：
   - 「将清空目标包的 Exam_People / Unit_Ess，删除 *.exam 与 SelfReport 目录，无法撤销。是否继续？」（`src\DocumentTools.App\Features\Modules\Exam\ExamPackageView.cs:241`）
   - 「将把 5 张表的 Item 列改为备注类型，无法自动还原，是否继续？」（`src\DocumentTools.App\Features\Modules\Exam\ExamMss3View.cs:190`）
   - 「按序号重写 Exam_EssInfo 的物理顺序？…」（`src\DocumentTools.App\Features\Modules\Exam\ExamInsertView.cs:443`）
   - 「确定禁用「{name}」？」（`src\DocumentTools.App\Features\Modules\SystemTools\SystemFeatureModules.cs:879`）
2. **收尾询问（默认开启、可在设置里关）**：「处理完毕，是否需要打开转换后目录？」（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:410-412`，受 `ShowOpenTargetDialog` 控制；替换页同款 `src\DocumentTools.App\Features\Modules\Tools\ReplaceFeatureModule.cs:180`）。

除这两类，任何「成功/失败/已取消」都走状态行，不弹框。

### 5.6 列表与拖放反馈

- 拖入/选择被过滤掉的文件：状态行 `Error` 语气「已忽略 {rejected} 个不支持的文件。」（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:92-95`）。
- 过滤规则单源：`BorderedFileList.FileFilter` 为 `null`/空白表示**不限后缀、一律放行**（`src\DocumentTools.App\Ui\Controls\BorderedFileList.cs:86-91`）；拖入目录只展开一层（`src\DocumentTools.App\Ui\Controls\BorderedFileList.cs:393-404`）。
- 重复添加同一路径不产生重复行（`:132-141`，`OrdinalIgnoreCase` 去重）。

**验收方式**：跑一批多文件转换，确认状态行按「初始 → 第 N/共 M → 汇总」走完且按钮全程可点性正确；`docs\qa-screens\_issues.csv` 无 High；破坏性操作逐个点开确认弹 `Confirm`（清单见 §5.5，可用 `Select-String -Pattern '\.Confirm\('` 复核调用点）。

---

## 6 参数面板与选项键

### 6.1 契约：Schema 在 Core，视图只按 Kind 渲染

`OptionField`（`src\DocumentTools.Core\Features\Options\OptionField.cs:17-24`）：

| 属性 | 含义 |
|------|------|
| `Key` | 进 `Options` 字典的键，**Operation/Service 按它取值** |
| `Label` | 仅显示 |
| `Kind` | `Check / Text / Choice / FilePath`（`:4-11`） |
| `DefaultValue` | 初始值 |
| `Choices` | `Choice` 用；`OptionChoice(Value, Label)`——`Value` 进 Options，`Label` 只显示（`:14-15`） |
| `TextWidth` | 文本框宽度，默认 160（`:23`） |
| `FileFilter` | `FilePath` 用，直接作为对话框过滤串 |

分区由 `OptionSection(Title, Fields)` 表达（`src\DocumentTools.Core\Features\Options\OptionField.cs:26`），一个 Section 渲染成一张 `SectionCard`（`src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:55`）。

**视图不许知道业务规则**：`OptionPanelBuilder` 只判断 `Kind`（`src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:10-13`、`:38-52`）。视图直接读 `Options["某个业务键"]` 只在「页面行为由该键驱动」时允许（例：`saveSource` 决定是否弹目录，`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:166`）。

### 6.2 四种 Kind 的渲染规格

| Kind | 控件 | 规格 | 位置 |
|------|------|------|------|
| `Check` | `CheckBox` | `Margin 0,4,16,4`，`Content = Label` | `src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:101-112` |
| `Choice` | 标签 + `ComboBox` | 标签 `Margin 0,0,6,0`；`MinWidth 120`；**`IsEditable = false`**；`DisplayMemberPath=Label`、`SelectedValuePath=Value`；初始索引夹取 | `src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:114-164`（`:137` 为不可编辑） |
| `Text` | 标签 + `TextBox` | `Width = TextWidth`（默认 160） | `src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:211-233` |
| `FilePath` | 标签 + `TextBox` + 「浏览…」 | 按钮 `ToolTip = FileFilter`；`IsEnabled = 有 IDialogService`；点击 `OpenFiles(filter, multiSelect:false)` 取首项 | `src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:166-209`（按钮 `:189-206`） |

**为什么下拉不可编辑**：可编辑下拉允许自由文本，值既可能不在 `Choices` 里、也无法回填 `SelectedValue`，等于把校验责任推给每个 Service。需要自由输入的场景用 `Text`，不要放宽 `Choice`。

`FilePath` 的「浏览…」在缺 `IDialogService` 时禁用而不是隐藏（`src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:195`），这是为了让「这里可以选文件」这件事在任何宿主里都可见；无 dialog 时退化为纯文本框，Schema 与业务语义不变（`:19-23` 注释）。

### 6.3 收集规则

`OptionPanelBuilder.Collect()`（`src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:59-82`）：

- `CheckBox` → `"true"` / `"false"`（`:66-68`）
- `ComboBox` → `SelectedValue`，回退到 `ComboBoxItem.Tag`、再回退到选中项文本（`:69-74`）
- `TextBox` → `Text.Trim()`（`:75-77`）
- 键字典 `OrdinalIgnoreCase`（`:16`、`:61`）；`GetBool(key)` 只认勾选（`:84-85`）。

初始值优先级：`overrides`（运行时偏好，如 `chromePath` 来自设置）＞ `DefaultValue`（`src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:87-95`；调用点 `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:113-120`）。布尔解析接受 `"true"` 和 `"1"`（`src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:96-98`）。

### 6.4 对话框过滤器：单源

过滤串只能来自 `FeatureOptionSchemas.GetFileFilter(featureId)`（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:575`），视图不得内联拼 `*.docx`。例外只有两类：

- 合并结果另存为（`PDF 文档|*.pdf` / `WORD 文档|*.docx`，`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:342`）——它不是「本页可接受输入」。
- 输入类过滤器里的图片/可执行文件常量：`ImageFilter`（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:661`）与 `chromePath` 的 `可执行文件|*.exe|所有文件|*.*`（`:426`）。

### 6.5 路径型参数：必填 vs 可选

| 场景 | 规则 | 证据 |
|------|------|------|
| 可选附件（封面图、水印图等） | 用 `PathGuard.OptionalExistingFile`：空或不存在 → `null`，回退默认行为，**不打断主任务** | `src\DocumentTools.Core\Common\PathGuard.cs:24-29`；消费方 `src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:413-414` |
| 必填文件（源文件、输出目录） | `PathGuard.RequireExistingFile` / `RequirePath`，缺就抛带中文说明的异常 | `src\DocumentTools.Core\Common\PathGuard.cs:6-22` |
| 必填但由业务特判（如浏览器路径） | 显式校验并抛明确文案：「没有找到浏览器可执行文件：…」 | `src\DocumentTools.Core\Services\Office\HtmlToPdfService.cs:27-32` |

**为什么可选附件不能抛异常**：封面图/水印图是锦上添花；路径写错就让整篇转换失败，等于用一个可选字段否决主任务（`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:413` 注释即此理由）。

现有 `FilePath` 字段（全量）：`word.epub.coverImage`（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:303`）、`pdf.watermark.add.image`（`:324`）、`html.pdf.chromePath`（`:426`）。新增附件字段必须同时给出 `FileFilter`、`TextWidth` 与「可选/必填」判定。

### 6.6 键位现状（以 Core Schema 为单源）

本文件**不再复制全量键表**（过去的副本已与代码脱节，见附录 A）；键的真实来源是 `FeatureOptionSchemas.GetSections`（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:208`）。这里只固定三条跨功能约束：

1. **输出开关**：除 `pdf.merge` / `word.merge`（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:543` 返回空 Schema）外，转换 / 拆分 / 水印类功能都有「输出」分区的 `saveSource`（`word.pdf` 的首个样例见 `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:213`；拆分经 `SplitSections` 共用 `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:623`）。替换类是例外：Schema 只有「匹配选项 / 替换选项」，没有 `saveSource`（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:550-605`），输出目录由页面自己问（`src\DocumentTools.App\Features\Modules\Tools\ReplaceFeatureModule.cs:151`）。
2. **命名**：键用小驼峰、语义化（`saveSource`、`embedFonts`、`exportNotes`），不写控件名或旧控件名（别再按 legacy 的 `ExportNotePageCombox` 记）。**特别注意 `ppt.pdf` 的备注键仍叫 `exportNotes`**——它已经由「布尔勾选」升级为 `NotesPositions` 位置 Choice，但**键名没改**，代码里不存在 `notesPosition` 这个键（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:265` 对照 `src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:219`）；文档早先写成 `notesPosition` 是错的。
3. **Choice 的值域写进 Schema**，且第一项即默认（`compliance` 默认索引 2 → `Pdf17`，`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:220` 对照 `src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:99`）。

已核对的完整示例（`word.pdf`，`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:211-224`）：

| Key | Label | Kind | 默认 |
|-----|-------|------|------|
| `saveSource` | 保存到源目录 | Check | 关 |
| `openPdfOptions` | 处理完毕后打开PDF文件 | Check | 关 |
| `embedFonts` | 嵌入字体 | Check | 关 |
| `updateFields` | 自动更新域 | Check | 关 |
| `useLocalOffice` | 使用本机Office转换 | Check | 关（**已真接，见 §6.8.2**） |
| `compliance` | PDF标准 | Choice | `Pdf17` |
| `colorMode` | PDF颜色模式 | Choice | `Normal` |
| `msWordVersion` | 设置兼容版本 | Choice | `Word2016` |
| `password` | WORD密码 | Text（宽 120） | 空 |

另一份核对样例（`pdf.word`，`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:226-239`）：`saveSource`、`useOcr`（启用OCR子层）、`useBullet`（识别项目符号）、`mode`（`Flow`/`Textbox`/`EnhancedFlow`，默认 `Flow`）、`password`（PDF密码）。

**已被删除的键（不要照旧文档去找）**：`removeTempDocx` 已从 `word.pdf` 的 Schema 中删除——legacy 只在一个我们从不产生的 temp docx 目录上用它（见附录 A），键没了自然也没有「勾了不生效」的假参数问题。

### 6.7 键 → 强类型 Options 的映射层

视图只给字符串字典；映射发生在 Operation：`src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:89-124`（`word.pdf` 见 `:93-103`，`pdf.word` 见 `:105-112`，`ppt.pdf` 见 `:117-124`）。默认值在映射层兜底（如 `GetOption(options, "compliance", "Pdf17")`），因此 Schema 里改默认时**必须同步**映射层默认，否则 UI 显示与实际执行不一致。

**验收方式**：`dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo`（`ToolUiContractTests` 钉住 `name.vertical`、`code.barcode`、`word.replace`、`excel.replace` 的键与 Core 服务调用）；改动 Schema 后再跑 `dotnet build DocumentTools.sln --no-incremental -v q --nologo`，并目视该页参数卡片（`docs\qa-screens\<featureId>.png`）。

### 6.8 四个功能的行为细则（2026-11-05 按代码核订）

本节记录四条「界面上看得见、但只从键名看不出来」的行为，用于回答用户与验收提问。每条都给出界面位置、控件类型、默认值、用户可见行为、失败时看到什么。

#### 6.8.1 `ppt.pdf` 备注导出位置

- **界面位置**：参数卡「转换参数」第 1 行（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:265`）。
- **控件类型**：Choice「备注导出位置」，值域三档（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:55-65`）：`None` 不导出（默认）、`BottomFull` 页底完整、`BottomTruncated` 页底截断。
- **用户可见行为**：选「不导出」时不设置 `SlidesLayoutOptions`，导出页数等于幻灯片数；选页底完整/截断时按备注内容加高页面，页数随之变化（`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:368-383`）。旧布尔值仍然收：`true` → `BottomFull`、`false` → `None`（`src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:219-237`）。
- **失败时看到什么**：值不在三档之内整份转换失败，状态行给 `不支持的备注导出位置：「RightFull」。Aspose.Slides 只支持 None / BottomFull / BottomTruncated。`（抛出处 `src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:376-377`）——**不静默按「不导出」出一份没有备注的 PDF**。映射层仍保留历史值 `RightFull` 的显式分支（`src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:234`），就是为了让旧配置走到这条明确报错，而不是被兜底成 `None`。
- **为什么删掉 `RightFull`**：本仓库锁定的 Aspose.Slides 25.12 枚举只有三个成员，早先「右侧占位」会让导出静默退回 `None`、备注全丢；Schema 注释里写了这条依据（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:55-59`）。

#### 6.8.2 `word.pdf` 使用本机 Office 转换

- **界面位置**：参数卡「转换参数」第 3 行（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:219`）。
- **控件类型**：Check「使用本机Office转换」，默认关。
- **用户可见行为**：关 = 内置引擎（Aspose.Words 渲染，`compliance`/`colorMode`/`embedFonts`/`updateFields` 全部生效）；开 = 晚绑定 `Word.Application` 调 `ExportAsFixedFormat`（`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:118-190`），此时上面那些 Aspose 专属参数不参与，产出与 legacy 的 Word COM 路径一致。开关值在映射层读（`src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:98`）。
- **失败时看到什么（没装 Word）**：`Type.GetTypeFromProgID("Word.Application")` 为 null 时给 `转换 Word 文档失败：未检测到 Microsoft Word。请安装 Word，或取消「使用本机 Office 转换」勾选后重试。（已尝试：Word.Application；该 ProgID 在本机注册表查不到。）`（`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:125-128`）。
- **失败时看到什么（Word 报错）**：文件被 Word 打开、文档加密等 COM 失败给 `本机 Word 转换失败（…）：请确认该文件未被 Word 打开，且文档未加密。`（`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:170-173`）。
- **可观测性**：成功与失败都往 `logs\conversion.log` 追加**一行**归因记录，形如 `2026-11-05 21:05:12 [word.pdf] 结果=成功 引擎=Word.Application 源=… 输出=… 耗时=1234ms`（`src\DocumentTools.Core\Common\ConversionLog.cs:33-55`，目录可用环境变量 `DOCUMENTTOOLS_LOG_DIR` 覆盖，见 `:15`）。日志写入失败一律吞掉，**不影响转换结果**。
- **为什么**：用户问「为什么这份 PDF 的目录/书签和 Word 里不一样」时，只有这行能区分「Word 导出的」与「内置引擎导出的」。
- **怎么测**：置 `DOCUMENTTOOLS_SIMULATE_NO_WORD=1` 可在装了 Word 的机器上稳定复现降级文案（`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:26`，思路同 `src\DocumentTools.Core\Data\AccessDb.cs:14` 的缺 ACE 模拟）；真实导出走 opt-in 环境变量 `DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1`。

#### 6.8.3 `image.compress` 输出 TIF / GIF

- **界面位置**：参数卡「保存参数设置」第 1 行「输出格式」（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:470-481`）。
- **控件类型**：Choice，七项 `PNG`（默认）/`JPG`/`JPEG`/`BMP`/`TIF`/`TIFF`/`GIF`。
- **用户可见行为**：`TIF`/`TIFF` 统一产出 `.tif`、`GIF` 产出 `.gif`（`src\DocumentTools.Core\Services\Image\ImageConversionService.cs:494-505`）；两种都由仓库自带的编码器写字节——TIFF 默认 LZW 压缩（`src\DocumentTools.Core\Services\Image\TiffEncoder.cs:68`），GIF 是 8bpp 索引色 + LZW 的单帧 GIF89a（`src\DocumentTools.Core\Services\Image\GifEncoder.cs:20`）。选这两项时参数卡底部固定显示一行提示：`TIF 适合扫描件/二值文档；彩色照片请选 JPEG。`（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:122-127`）。
- **失败时看到什么**：正常不报错；输出目录不可写走统一中文文案（`src\DocumentTools.Core\Services\Office\OperationErrors.cs:63`）。GIF 只有 256 色，渐变与半透明会退化——这是格式限制，界面不额外告警。
- **为什么提示里点名 JPEG**：实测 A4 300dpi 扫描页 TIF/LZW 只比未压缩小 0.7%–1.5%，而彩色照片转 TIF 反而涨到 102.6%（同一处注释 `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:122-123`）。

#### 6.8.4 `word.replace` 替换后字体样式

- **界面位置**：参数卡「替换后字体样式」四行（`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:564-570`）。
- **控件类型与默认值**：`replaceUseNewStyle` Check「更改替换的字体和大小」默认**关**；`replaceFontName` Text「字体名(空=不改)」默认 `SimSun`；`replaceFontSize` Text「字号(0=不改)」默认 `12`；`replaceBold` Choice「加粗」默认「不改」（`keep`，另两项 `bold` 加粗 / `normal` 不加粗，`src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:200-206`）。
- **用户可见行为**：只有勾了「更改替换的字体和大小」才动字体——服务端此时才给 `FindReplaceOptions.ApplyFont` 设 `Name`/`Size`/`Bold`（`src\DocumentTools.Core\Services\Office\DocumentReplaceService.cs:44-62`）；字体名留空、字号 0、加粗选「不改」分别表示「这一项不动」，所以「只改字号、保留原字体」是可表达的组合（映射见 `src\DocumentTools.Core\Operations\ReplaceDocumentOperation.cs:52-56`）。
- **失败时看到什么**：字体名写了本机没有的字体不报错，由渲染端回退；字号写了非数字按 0（不改）处理；未勾开关时四个键完全不起作用，也不报错。
- **为什么默认关**：这是 legacy 的实际行为——legacy 那个勾选框从未被传进转换函数（`UseNewStyle` 恒 false），等价于死开关；本项目保留默认关，同时把它做成真开关。

---

## 7 文案规范

### 7.1 页面标题与描述

- 标题 = 功能名，与 `BuiltInFeatures` 里的 `Title` 一致（`src\DocumentTools.Core\Features\BuiltInFeatures.cs`），不要在页面里另起别名。
- 描述一句话说「产出什么」，不写操作教学。「选择文件后执行。」只是兜底（`src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:24`），合格样例：「简易二维码 / 生成 / 预览 / 保存 / 批量（外观参数来自 Core Schema）」结构（`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:65`）。

### 7.2 按钮

- 动词 + 对象：`选择文件`、`开始处理`、`生成预览`、`保存 PNG`、`批量保存`、`复制图片`（`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:116-122`）。
- 不写「确定」「执行」这类无对象动词；破坏性按钮要把后果写进 `Confirm` 正文（§5.5）。
- 只用图标时必须有 `ToolTip`（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:468`）。

### 7.3 状态与进度

- 进行中：`正在 {动作} 第 {i} 个 / 共 {n} 个：{文件名}`（`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:272-277`）。
- 结束：`处理完毕，成功 {n} 个…`（`:301-304`）。
- 取消：`已取消，没有处理任何文件。`（`:216`）。
- 引导：`先添加文件并设置参数，再点「开始处理」。`（`:93`）。

### 7.4 错误与空态

- 错误必须包含**下一步动作**或**具体对象**：`请先点「选择文件」把要处理的文件加进列表。`（`:149`）、`该功能没有注册处理流程：{id}`（`:141`）、`已忽略 {n} 个不支持的文件。`（`:89`）、`无法确定输出目录，请换一个保存位置：{path}`（`:354`）。
- 空态必须给下一步：`还没有可以进入的功能：功能模块未注册，或已在「系统设置」里被隐藏。`（`src\DocumentTools.App\Features\Modules\Home\HomeFeatureModule.cs:82`）、`生成后在此预览条码：单行显示 1 个，批量显示第 1 个。`（`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:95`）。

### 7.5 术语与标点

- 中文用全角标点；引号用「」；同一概念只用一种说法：`源目录`（写回原地）、`转换后目录`（输出位置）、`输出目录`（页面状态里的通用称呼）。
- 功能 id 出现在用户可见文案里时必须包在「」里（`该功能没有注册处理流程：word.pdf` 会读成一句废话），示例见 `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:141`。

### 7.6 禁止

- 禁止连续半角问号乱码（检测：`Select-String -Pattern '\?{3,}'`）或 `U+FFFD`（历史事故记录见 `docs\feature-gap.md:124`）；中文写 `.cs` 必须 UTF-8。
- 禁止无信息量文案：「处理中…」「出错了」「请稍候」。
- 禁止把异常堆栈直接塞进状态行；面向用户的是「发生了什么 + 能做什么」，技术细节进日志。

**验收方式**：`Select-String -Path docs\feature-ux-spec.md -Pattern '\?\?\?'` 应无输出；`Select-String -Path src\**\*.cs -Pattern '\?\?\?'` 应无输出；`tools\UiSnapshot` 的乱码检查（`??` / `U+FFFD`）不得报 High。

---

## 8 反例（不要这样做）

| # | 反例 | 后果 | 正确做法 | 证据 |
|---|------|------|----------|------|
| 1 | 工具条/参数卡放在文件列表上方 | 文件一多就把「开始处理」挤到视口外 | 参数与操作条 `Dock.Bottom`，列表做唯一填充 | `src\DocumentTools.App\Ui\Controls\FeaturePageBase.cs:52-70` |
| 2 | 列表为空就弹目录选择框 | 用户先被问「存哪」再被告知「没文件」 | 空列表只改状态行、直接返回 | `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:146-151` |
| 3 | 一页放两个 Primary 按钮 | 主操作失去唯一性 | 只留一个 Primary；执行动作用 `Success` 语气 | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:162`、`src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs:116-122` |
| 4 | 破坏性操作不 `Confirm` 直接执行 | 不可撤销的数据丢失 | `Confirm` 里写清后果与不可逆性 | `src\DocumentTools.App\Features\Modules\Exam\ExamPackageView.cs:241` |
| 5 | 长任务只禁用按钮、不给进度，或只给「正在处理…」 | 用户以为卡死并强杀进程 | `Busy` + 「第 N/共 M + 文件名」+ 每轮让出线程 | `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:153-156`、`:272-278` |
| 6 | 视图里硬编码字号/颜色/边距 | 换主题或改密度时全站不一致 | 取 `UiTokens`；只有 §2.5 的特例可写值 | `src\DocumentTools.App\Ui\UiTokens.cs:6-10` |
| 7 | 视图内联拼过滤串 `*.docx` | 按钮与拖放两套规则，互相打架 | `FeatureOptionSchemas.GetFileFilter` | `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:91`、`:111` |
| 8 | 视图直接调 Aspose/Service | 破坏分层，页面与业务耦合 | 视图只收集 `Options`，交给 `IDocumentOperation` | `src\DocumentTools.Core\Features\Options\OptionField.cs:3`、`src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:89-124` |
| 9 | 给无列表页套选文件壳 | 出现一个永远为空的列表 | 用工具壳 `BuildToolLayout` | `src\DocumentTools.App\Features\Modules\Tools\IdCardFeatureModule.cs:63` |
| 10 | 下拉允许自由输入 | 值不在 `Choices` 里，回填失败，Service 拿到脏值 | `IsEditable = false` | `src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:137` |
| 11 | 把 Choice 的 `Label` 当值用 | 业务收到中文显示名 | 只把 `Value` 写进 Options | `src\DocumentTools.Core\Features\Options\OptionField.cs:14-15`、`src\DocumentTools.App\Ui\Options\OptionPanelBuilder.cs:70-73` |
| 12 | 每条消息新建一个状态 TextBlock | 页面越长、状态越乱，读屏反复播报 | 复用同一个 `TextBlock` + `ShowStatus` | `src\DocumentTools.App\Ui\Controls\ToolViewKit.cs:129-144` |
| 13 | 可选附件路径写错就抛异常 | 一个可选字段否决整篇转换 | `PathGuard.OptionalExistingFile` | `src\DocumentTools.Core\Common\PathGuard.cs:24-29` |
| 14 | 取消操作后弹提示框 | 打断用户，且取消本就是无声操作 | 只改状态行 | `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:215-216` |
| 15 | 在 `MainWindow` 里 `new` 具体页面类型 | 壳层依赖所有页面，导航无法扩展 | 通过 `IFeatureViewFactory` 建视图并缓存 | `src\DocumentTools.App\Shell\MainWindow.xaml.cs:302-315`（`GetOrCreate`，`:311` 调 `_views.Create(featureId)`；工厂字段注入见 `:19`/`:33`） |

**验收方式**：code review 时按表逐条对 diff 检索 `Primary(`、`\.Confirm\(`、`ShowStatus(`、`GetFileFilter`、`PathGuard` 五类调用点；新增页面必须能指到 §1.3 的原型之一。

---

## 9 检查清单

### 9.1 新页面

- [ ] 能指到 §1.3 的原型，并用对应的 `BuildBottomActionLayout` / `BuildToolLayout`（不手写第三种纵向顺序）。
- [ ] 只有一条操作条，且只有一个 Primary（转换页可另有 `Success` 语气的执行按钮）。
- [ ] 参数来自 Core `FeatureOptionSchemas`，视图只按 `Kind` 渲染，没有内联业务规则。
- [ ] 所有路径输入走 `IDialogService`；可选附件走 `PathGuard.OptionalExistingFile`。
- [ ] 状态行复用单一 `TextBlock`，覆盖「初始 / 进行中 / 结果 / 取消」四段，长任务有粒度文案。
- [ ] 破坏性操作有 `Confirm` 且正文写明不可逆后果。
- [ ] 空态文案是「下一步做什么」，不是「没有内容」。
- [ ] 无硬编码字号/颜色（除 §2.5 特例）；无连续问号 / `U+FFFD` 乱码。
- [ ] 键盘：`Esc` 不复位数据、`Enter` 命中主操作；图标按钮有 `ToolTip`。

### 9.2 提交前命令

```powershell
dotnet build DocumentTools.sln --no-incremental -v q --nologo
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo
dotnet run --project tools\FeatureSmoke -v q --nologo        # 期望 47/47
dotnet run --project tools\UiSnapshot -- --self-test          # 期望 passed=8/8
Select-String -Path docs\feature-ux-spec.md -Pattern '\?\?\?'
```

`dotnet run --project tools\UiSnapshot`（不带 `--self-test`）会重写 `docs\qa-screens\*`：只在确实要刷新快照时运行，并把 `_issues.csv` 的 High 当作阻断项。

### 9.3 文档自身

- [ ] 新增/改动的每条规范都带 `文件:行号`，且行号指向真实存在的代码（改动代码后同步本文件）。
- [ ] 不再复制全量键表；键以 `FeatureOptionSchemas` 为单源，本文只保留跨功能约束与已核对样例。
- [ ] 与 `docs\architecture.md` 的交叉引用仍然成立：本文件 **§6 = 参数/选项键**（`docs\architecture.md:39`、`:61` 指向它）；**§4 = 构件与控件约定**（含设置页不迁移项，`docs\backlog-next.md:223` 指向它）。

**验收方式**：上面 9.2 的命令全绿；本文件里每个 `文件:行号` 都能在对应文件里看到匹配内容；`docs\qa-screens\_issues.csv` 无 High。

---

## 附录 A 本次核对修正的过时断言（2026-11-05）

| 旧断言 | 实测 | 证据 |
|--------|------|------|
| `RemoveTempDocx` 可删临时目录 | 键已从 Schema 删除（legacy 只用它删一个我们从不产生的 temp docx 目录），`word.pdf` 现在只有 9 个键 | `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:211-224` |
| 拖放「页与列表均可落盘」 | 只有文件列表接受拖放 | `src\DocumentTools.App\Ui\Controls\BorderedFileList.cs:61-62` |
| 页面原型按「原 Tab 编号 `TabPage_XX`」组织 | 现按布局 API 分 A–G，功能清单来自 `BuiltInFeatures`（47 条，含 `home`） | `src\DocumentTools.Core\Features\BuiltInFeatures.cs:77-88` |
| 参数清单按旧控件名（`ExportNotePageCombox` 等） | 键名以 Schema 为准；`ppt.pdf` 的键仍叫 `exportNotes`，值域已升级为三档位置 Choice，代码里没有 `notesPosition` 这个键 | `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:265`、`src\DocumentTools.Core\Operations\OfficeConvertOperation.cs:219-237` |
| `ppt.pdf` 备注可选 `RightFull`（右侧占位） | 该档位已删除：Aspose.Slides 25.12 枚举只有 `None`/`BottomFull`/`BottomTruncated`，选其它值整份转换抛中文异常而不是静默按「不导出」 | `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:55-65`、`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:378-383` |
| `word.pdf` 的「使用本机Office转换」勾了不生效 | 已真接 `Word.Application`（晚绑定 COM）；缺 Word 给可操作中文，成功/失败各写一行 `logs\conversion.log` 归因 | `src\DocumentTools.Core\Services\Office\OfficeConvertService.cs:118-190`、`src\DocumentTools.Core\Common\ConversionLog.cs:33-55` |
| `image.compress` 只能输出 PNG/JPG/BMP | 另有 TIF/TIFF（自写 TIFF 编码器，默认 LZW）与 GIF（自写 GIF89a 编码器，8bpp 索引色），并在该页给一行 TIF/JPEG 取舍提示 | `src\DocumentTools.Core\Services\Image\ImageConversionService.cs:222-233`、`src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs:122-127` |
| `word.replace` 无「替换后字体样式」相关参数 | 四个键 `replaceUseNewStyle`/`replaceFontName`/`replaceFontSize`/`replaceBold`；开关默认关**正是 legacy 行为**（其勾选框从未被传参），本项目把它做成真开关 | `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs:564-570`、`src\DocumentTools.Core\Services\Office\DocumentReplaceService.cs:44-62` |
| 分区标题表按 legacy `GroupBox.Text` 手抄 | 分区标题来自 Core Schema 的 `OptionSection.Title` | `src\DocumentTools.Core\Features\Options\OptionField.cs:26` |
| `StartPageIndex` ✅（映射为页面） | 只回落 `home`，不按序号映射 | `src\DocumentTools.Core\Settings\AppSettings.cs:101-102` |
| 检查清单泛指「Debug 编译 + 三页目视」 | 固化为可执行命令：FeatureSmoke 47/47、UiSnapshot 自检 8/8、`_issues.csv` 无 High | `tools\FeatureSmoke\Program.cs:32-79`、`docs\qa-report-ui.md:30-40` |

## 附录 B 修订记录

- 2026-11-05：全文按当前实现重写（壳层 → 布局 → 构件 → 反馈 → 参数 → 文案 → 反例 → 清单），每条规范绑定 `文件:行号` 与「为什么」；补入 `ToolViewKit.AttachPageKeyboard`、读屏 live region、`PathGuard.OptionalExistingFile`、`Confirm` 两类用法、`_issues.csv` 现状与 FeatureSmoke/UiSnapshot 的验收命令。
- 2026-11-05（补充）：新增 §6.8「四个功能的行为细则」——`ppt.pdf` 备注三档与 `RightFull` 删除后的抛错文案、`word.pdf` 本机 Office 真实现与 `logs\conversion.log` 归因、`image.compress` 的 TIF/GIF 输出与取舍提示、`word.replace` 的「替换后字体样式」四键；§6.6 全部行号按当前 `src\` 重核（`GetSections` 在 `:208`、`SplitSections` 在 `:623`、合并空 Schema 在 `:543`），并更正早先「`exportNotes` 已改名 `notesPosition`」的错误说法。
