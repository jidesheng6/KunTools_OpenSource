# 壳层与系统页 UX 打磨（task-42）

目标：把用户点名的两类问题落成可复验的规则——**侧边栏父级节点不许比子级矮**、**路径框只读**——并把宽度、失效提示一并收口，避免下一个页面重新发明。

## 1. 判定表：哪些框只读、哪些可编辑

判据只有一条：**值代表磁盘上的一件东西 → 只读；值代表用户创造的参数 → 可编辑**。路径由系统文件选择器产出，用户手打的收益为零、代价是斜杠/盘符/空格写错后一路传到 Aspose 才炸；标题、正则、分隔符、阈值没有「正确来源」，必须可编辑。

| 位置（文件:行） | 内容 | 判定 | 理由 |
| --- | --- | --- | --- |
| `SystemTools\SystemFeatureModules.cs:71-73` | Chrome 路径 / 考评数据目录 / MSS 数据目录 | **只读**（本轮改） | 磁盘实体，值只能来自「浏览…」或拖放 |
| `SystemTools\SystemFeatureModules.cs:60-67` | 列表行高 | 可编辑 | 数字参数，18–64 由校验兜底 |
| `SystemTools\SystemFeatureModules.cs:69` | 搜索功能名或 FeatureId | 可编辑 | 临时查询词，不落盘 |
| `SystemTools\SystemFeatureModules.cs:1051-1055` | IP / 子网掩码 / 网关 / DNS1 / DNS2 | 可编辑 | 网络参数，用户必须能改 |
| `Tools\BarcodeFeatureModule.cs:46` | LOGO 图片路径 | 建议只读（他人 scope，只登记） | 磁盘实体 |
| `Tools\BarcodeFeatureModule.cs:47` | 待识别图片路径 | 建议只读（同上） | 磁盘实体 |
| `Tools\MiscToolModules.cs:54` / `:583` | 扫描保存目录 / 监视目录 | 建议只读（同上） | 目录 |
| `Exam\ExamCircleView.cs:29-30` | 考评表图片 / 替换图 | 建议只读（同上） | 磁盘实体 |
| `Exam\ExamInsertView.cs:26` | 考评 Access 数据库 | 建议只读（同上） | 磁盘实体 |
| `Exam\ExamMss3View.cs:23-24` | MSS3/MDB 路径 / 选票图片 | 建议只读（同上） | 磁盘实体 |
| `Exam\ExamPackageView.cs:21-22` | 源 / 目标数据包目录 | 建议只读（同上） | 目录 |
| `Exam\ExamSingleView.cs:21-23` | Word 模板 / 人员图片目录 / 输出目录 | 建议只读（同上） | 磁盘实体 |
| 其余（条码内容、身份证号、金额、查找替换、姓名、VoteId、Raid 等） | 参数/文本 | 保持可编辑 | 用户创造的内容，没有替代输入源 |

配套两条硬规则：

1. **不许用 `IsEnabled = false` 代替只读**。禁用态连选中复制都做不到，用户既核对不了路径、也无法把它粘到别处排查；只读 + 可聚焦才是最省事的「能看能拷、不能误改」。
2. **失效路径要在触发动作时当场说清**，不要让它一路走到 Aspose 或进程启动参数才变成英文异常。系统设置页在 `Save()` 写盘前检查三条路径，命中即 `ShowActionError` 并放弃写盘：`文件已不存在：<路径>。请重新选择。` / `目录已不存在：<路径>。请重新选择。`

## 2. 改动点

### 2.1 侧边栏父级节点（A）

| 项 | 改前 | 改后 | 位置 |
| --- | --- | --- | --- |
| 分组头高度 | 28.5px（靠内容撑，`MinHeight = 0`） | `MinHeight = 34`（`UiTokens.RowMinHeight`） | `Shell\CollapsibleSection.cs` |
| 分组头内边距 | `6,5,6,5` | `10,0,8,0`（`UiTokens.GroupHeaderPadding`） | 同上 |
| 标题字号 | 13 | 14（`UiTokens.SectionTitleFontSize`） | 同上 |
| 图标 | 15，左侧无固定留白，标题落在 x≈48 | 15 号 + 右留 12（`UiTokens.SpaceMd`），标题落在 x≈37，与子项文字对齐 | 同上 |
| 箭头 | 12 号、随标题挤在左端 | 16 号方形槽（`Width=16`），`Dock.Right` 垂直居中 | 同上 |
| 悬停/按下反馈 | 仅光标手型 | `MouseEnter→ControlFillColorSecondaryBrush`、按下 `TertiaryBrush`、抬起回 Secondary、离开 `ClearValue(Background)` | 同上 |
| 子项高度 | 33.4px（`Padding=10,7,10,7`，无 MinHeight） | `MinHeight = 34` + 同 padding（`UiTokens.NavRowPadding`） | `Shell\MainWindow.xaml.cs` |
| 快照门槛 | `MinNavButtonHeight = 20` | 34 | `tools\UiSnapshot\ShellAudit.cs` |

父级 34 ≥ 子级 34 是同一条视觉规则的两个端点：父级再矮一点，侧边栏就会出现「小标题带着大条目」的失衡。

### 2.2 路径框与输入宽度（B/C）

- `Ui\PathInputs.cs`（新建）：`ForFile` / `ForFolder` 产出只读框——`IsReadOnly = true`、`IsTabStop = true`、`MinWidth = UiTokens.PathBoxMinWidth`（320）、`HorizontalAlignment = Stretch`（吃星号列）、`AllowDrop` + `Drop` 取第一个拖入项、`ToolTip` 写明「只读：点右侧「浏览…」选择，或直接拖放」。
- 设置页 `_chrome` / `_examPath` / `_mssPath` 全部改走 `PathInputs`；`_lineHeight` 从 `Width = 120` 改成 `MinWidth=70 / MaxWidth=110 / TextAlignment=Right`，说明文字加单位：「ListViewLineHeight · 单位：行，取值 18–64」。
- `Field` 的标签列由字面量 `96` 改成 `UiTokens.FieldLabelWidth`，标签本身允许换行（超长标签换行而不是挤压控件），保证标签列永远 ≤ 96。

### 2.3 失效提示（D）

`Save()` 在写回配置前检查：`ChromeDriverPath` 用 `PathInputs.IsMissingFile`，`VfsoftExamDataPath` / `VfsoftMssDataPath` 用 `IsMissingFolder`；空白值不算失效（必填交给校验行）。命中后 `ShowActionError(...)` 显示 `文件已不存在：…。请重新选择。`，`_feedback` 说明「设置没有写入配置文件」，函数直接返回——既不把死路径写进配置，也不会抛出底层异常。

## 3. 前后像素证据

探针：`%TEMP%\shell-metrics`（控制台 + `UseWPF`，`ProjectReference` 指向 `DocumentTools.App`，反射取 `CollapsibleSection` 的 `_header` / `_title` / `_chevron`，摘树后按 1440×900 离屏布局，量的是 `ActualHeight`、`TransformToAncestor` 坐标与触发鼠标事件后的 `Background`）。同一探针跑两次——改前一次、改后一次，数字才可比。

| 量 | 改前 | 改后 | 结论 |
| --- | --- | --- | --- |
| 分组头高度 `header.ActualHeight`（9 个分组同值） | **28.5px** | **34.0px** | 父级不再低于子项 |
| `header.MinHeight` | 0.0（靠内容撑） | 34.0（`UiTokens.RowMinHeight`） | 高度由规则决定，不由字体撑开 |
| `header.Padding` | 6,5,6,5 | 10,0,8,0 | 左右内边距显式化，竖向由 MinHeight 管 |
| 标题字号 `title.FontSize` | 13.0 | 14.0 | 与页面小标题同级 |
| 标题左坐标 `iconL` | **48** | **38** | 向子项文字看齐 |
| 箭头字号 / 宽度 / 高度 | 12.0 / — / 13.1 | 16.0 / 16.0 / 17.4 | 方形槽，垂直居中 `chevronVC=Center` |
| 箭头水平位置 `chevronX` / 距右边缘 | 无独立槽位 | 230 / 9 | 移到右端，不再和标题挤在一起 |
| 子项按钮高度 `NAV h` | 33.4 | **34.0** | 与父级齐平 |
| 子项文字左坐标 `navTextL` | 37 | 37 | 未动，改的是父级来对齐它 |
| 悬停背景 `hover` | `#00FFFFFF`（等于没有反馈） | `#4DF9F9F9` | 悬停可见 |
| 按下背景 `pressed` | `#00FFFFFF` | `#80F9F9F9` | 比悬停更深，方向正确 |
| 移出后 `left` | `#00FFFFFF` | `#00FFFFFF` | 正常清除 |

原始输出（改后）：

```
HDR|WORD|h=34.0|pad=10,0,8,0|minH=34.0|titleFont=14.0|iconL=38|chevronFont=16.0|chevronW=16.0|chevronH=17.4|chevronVC=Center|chevronX=230|chevronRightGap=9|hover=#4DF9F9F9|pressed=#80F9F9F9|left=#00FFFFFF
NAV|首页|h=34.0|pad=10,7,10,7|minH=34.0|textL=37
SUM|groups=9|minHeaderH=34.0|maxHeaderH=34.0|minNavH=34.0|maxNavH=88.0
UI|BodyFontSize=13 SectionTitleFontSize=14 GroupIconSize=15 NavIconSize=16 RowMinHeight=34 GroupHeaderPadding=10,0,8,0
```

改前原始输出：

```
HDR|WORD|h=28.5|pad=6,5,6,5|minH=0.0|titleFont=13.0|iconL=48|chevronFont=12.0|chevronH=13.1|chevronVC=Center
NAV|首页|h=33.4|pad=10,7,10,7|textL=37
```

两次探针之间只改样式，没动数据与控件数量，所以高度与坐标的差就是这几处样式改动的净效果。

### 3.1 这次量出来的两条教训

1. **反馈有没有做，只能看像素**：第一版 `SetHeaderFill` 里带了 `_header.IsMouseOver` 判断，`IsMouseOver` 是只读属性、只由真实输入驱动，离屏合成 `MouseEnter` 时恒为 false → 探针读到 `hover=#00FFFFFF`，按钮看起来「有代码但没效果」。去掉该判断后反馈立刻可见。
2. **方向也要量**：修完立刻暴露第二个问题——悬停用 `ControlFillColorSecondaryBrush`（alpha `#80`）、按下用 `TertiaryBrush`（`#4D`），按下反而更淡。对调成 hover=`#4D` / pressed=`#80` 才是「越用力越深」。这种事光看代码注释绝对发现不了。
3. **容器上的 `TabIndex` 不算数**：我先在本页给底部操作条设 `TabIndex=100`、内容设 `0`，以为键盘顺序就对了——Lead 复核后指出 `KeyboardNavigation.TabIndex` 只在同一父级的孩子间排序，**不会传给容器内部的按钮**，等于没设，已删。真正的修法在布局：`BuildBottomActionLayout` 改成 `Grid` 按「内容 → 选项 → 操作条」添加，逻辑顺序与视觉顺序自然一致。

## 4. 门槛命令

数字来自 **2026-11-06 00:10（Asia/Shanghai）** 这一轮，同一工作树多人在跑，构建偶发被 `testhost` 锁文件（`MSB3026/MSB3027`），等进程退出后重试即通过。

| 命令 | 结果 |
| --- | --- |
| `dotnet build DocumentTools.sln --no-incremental -v q --nologo` | **0 警告 0 错误** |
| `dotnet test tests\DocumentTools.Tests\... -v q --nologo` | 00:14 轮：**失败 0，通过 411，已跳过 3，总计 414**。之后用例数在并发期增长，同一棵树 Lead 复跑为 **失败 0 / 通过 505 / 跳过 3 / 总计 508**；期间出现过 22 条失败的快照，属并发编辑窗口期的产物（`ToolUiContractTests`、`WordReplaceStyleTests`、`WordLocalOfficeDiagnosticsTests`、`ConvertContentParityTests`、`VoteReportServiceTests`，均在他人 scope），**不是回归** |
| `dotnet run --project tools\FeatureSmoke -v q --nologo` | **功能总数 47，创建成功 47，布局通过 47**；启动落点 95 项 0 失败 |
| `dotnet run --project tools\UiSnapshot -- --only home --no-suite --no-interact` | `PAGE\|001\|Home\|home\|ok\|home.png\|68KB\|1.00\|el=702\|btn=50\|tab=46\|input=no\|H0/M0/L1`；`SUMMARY pages=1 rendered=1 fatal=0 high=0 medium=0 low=1`；`SHELL\|groups=9\|singleButtons=2\|buttons=54\|collapsedVisible=9\|expandedVisible=54\|minButtonH=34.0\|problems=0`（退出码 0） |
| 乱码自检（`Content = "???` / `base("???` / `PlaceholderText = "???`） | 0 命中 |
| GBK 二次编码自检（`锛|銆|鐨|鏂|鎴|鍥剧|鏂囨。|闇€`） | 0 命中 |
| `MessageBox.Show` 扫描（`Shell\` + `Features\`） | 1 命中，`Shell\CrashLog.cs` 的启动兜底（已批准保留） |

唯一 Low 是 `home` 的「页面只有按钮/列表，没有输入控件」观察项，属设计取舍，与本轮改动无关。

### 4.1 系统设置页（B/C/D 的落点）

```
PAGE|045|System|system.settings|ok|system.settings.png|68KB|1.00|el=724|btn=74|tab=65|input=yes|H0/M0/L2
UXBOX|system.settings|considered=3 readonly=3 flagged=0
SUMMARY pages=1 rendered=1 fatal=0 high=0 medium=0 low=2 oversized=0
```

`UXBOX … readonly=3 flagged=0` 是本轮 B 的机器判据：三个路径框（Chrome 路径、考评数据目录、Mss 目录）都被识别为路径类且**全部只读**，没有一个漏成可编辑或 `IsEnabled=false`。

两条 Low 都是新规则「Tab 顺序倒退」，逐条说明，不是这批改动引入的缺陷：

1. `CheckBox「转换完成后询问是否打开目标目录」` 报「排在第 6 位却比第 5 位的 `Button「打开配置文件」` 更靠上」。成因是 `FeaturePageBase.BuildBottomActionLayout` 当时用 `DockPanel` + `LastChildFill`，**先挂底部操作条、后挂内容**，逻辑顺序里底部按钮排在滚动内容之前。Lead 已把该方法改成 `Grid` 按「内容 → 选项 → 操作条」添加，逻辑顺序与视觉顺序一致（我先前在本页加的 `SetTabIndex(bottom, 100)`/`SetTabIndex(fill, 0)` 无效——容器上的 `TabIndex` 不传给内部按钮——已随改版删除）。
2. 隐藏功能列表的第 47 个 `CheckBox`（中心 y=2134）之后是路径卡的 `TextBox`（y=800）。这是**滚动容器内部的坐标**：列表高 200，47 项在内部被排到 2134，后面的输入框自然“更靠上”。任何可滚动的多选列表都会触发这条。

第 2 条属检查器口径问题（对滚动容器后代应按视口内坐标裁剪），已归 `task-44`（`circle-fix-dev`）修，不在本任务范围；第 1 条随 `Grid` 改版已从根上消失。



