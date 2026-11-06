# UI/UX 常态化检查报告（task-44）

- 数据来源：`dotnet run --project tools\UiSnapshot`（真 DI + 真视图工厂 + 真实 `Window` 宿主离屏渲染）
- **权威一轮时间：2026-11-06 00:41:59**（本报告全部数字只来自这一次运行；`docs/qa-screens/*.png`、`_index.csv`、`_issues.csv` 同一时刻生成）
- 覆盖面：`SUMMARY pages=47 rendered=47 fatal=0 high=0 medium=0 low=6 oversized=0`；`SUITE|hidpi=16|interact=4|interactOk=4|interactHang=0`
- 检查器自检：`dotnet run --project tools\UiSnapshot -- --self-test` → `SELFTEST RESULT passed=20/20`
- 索引表 `docs/qa-screens/_index.csv` 47 数据行 + 表头 = **48 行**，格式未变，仍被下游脚本消费
- 退出码 0（= `fatal + High` 条数）

## 1 结论

七条 UX 规则已经是**每次全量快照都会跑**的常态检查，不再依赖人工抽查。本轮全量 47 页 **High 0 / Medium 0 / Low 6**，6 条 Low 全是「页面本来就没有输入控件」的观察项（`home`/`about`/`word.merge`/`pdf.merge`/`code.datamatrix`/`other.misc`），不是缺陷。

这个 0 不是"放宽规则凑出来的"：同一批规则在本轮之前的四次运行里分别报出 `High 14 / Medium 39 / Low 76`（00:15:20）、`High 14 / Medium 32 / Low 59`、`High 2 / Medium 28 / Low 6`、`High 0 / Medium 3 / Low 6`。每一轮的下降都能指到一次具体修复（见 §7），规则自身的口径只有两处**收紧**（规则 ③ 从"同卡片所有标签"改为"纵向堆叠的标签"；规则 ⑤ 从"纯树序"改为"TabIndex 优先 + 视口内比较"），没有一处放松。

## 2 方法与可复现性

### 2.1 规则落在哪

全部判定集中在 `tools/UiSnapshot/VisualAudit.cs` 的 `Run(host, pageWidth, pageHeight, snapshot)`，对每页已渲染的视觉树逐元素判定，**不读源码、不看 XML，只认渲染后的几何与属性**；壳层规则在 `tools/UiSnapshot/ShellAudit.cs`。

### 2.2 命令

```powershell
dotnet run --project tools\UiSnapshot                       # 47 页 + hidpi 16 + 交互态 4，写 docs\qa-screens（带 .running.lock，抢不到锁打印 BUSY 并退出 8）
dotnet run --project tools\UiSnapshot -- --self-test        # 规则自证：每条规则一个反例 + 3 条独立断言，必须 20/20
dotnet run --project tools\UiSnapshot -- --out $env:TEMP\qa --only word.replace,exam.single --no-suite --no-interact   # 子集跑到私有目录（只写 partial-*.csv，不覆盖权威产物）
```

### 2.4 检查器自己骗自己的两条教训（比多报十条命中更该记住）

1. **`ContainsBrowseButton` 向下无限递归 → 假阳性泛滥**。规则 ① 的语义是「同一行的输入框 + 选择按钮」，但最初实现从输入框向上找祖先、再对该祖先向下递归找按钮；而页面根节点也在「3 层祖先内」，于是**任何页面**的路径框都会被页面上**别处**某个「选择…」按钮误伤（`exam.single` 三个框全红，`system.settings` 也跟着红）。修法：向上最多 2 层（`HasBrowseButtonNearby` 的 `depth >= 2` 截止），且容器内只查**直接子级**（`ContainsBrowseButton(container, self, level)` 在 `level >= 1` 截止），把「同一行」限定成结构事实而不是几何猜测。
2. **`block.Inlines.Count == 0 ? block.Text : null` 取不到 `TextBlock.Text` → 假阴性空转**。`TextBlock.Text = "…"` 内部会生成 1 个 `Run`，所以 `Inlines.Count` 恒为 1，判定把所有标签都当成「没有文本」跳过，规则 ③ 表面上 0 命中、实则从未参与判定。修法：直接读 `string.IsNullOrWhiteSpace(block.Text) ? null : block.Text`，并在自检里保留一行偏 12px 的反例（`SELFTEST|label-align|hit|count=1`）作为「规则还活着」的看门狗。

第三条教训是本轮新增的，属于**跨角色协作**而不是检查器缺陷：`KeyboardNavigation.TabIndex` 是**非继承**的附加属性，把 `SetTabIndex` 打在 `DockPanel`/`Border` 这类容器上**不会**传给里面的按钮，所以「给容器加 TabIndex」这种修法对键盘序无效。这一条由自检里的机器事实固定下来（见 §4.1 的 `SELFTEST|tabindex-inherits|false|container-propagates=false|child-index=未设置`），避免以后再用口头争论 WPF 语义。

### 2.5 证据行格式

每条命中都写「元素 + 关键属性」，不是只报数字，例如：

```
exam.mss3   TextBox                与「选择/浏览/打开/清空」按钮同容器，但 IsReadOnly=false：手敲路径与按钮选择会互相覆盖
html.pdf    TextBox                可编辑输入框宽度 60px < 70px
twain.debug TextBlock「未检测到扫描仪…」 文字 #FF6347 对背景 #FAFAFA 对比度 2.82:1，低于正文 4.5:1门槛
```

## 3 七条规则与命中汇总（权威一轮）

| # | 规则 | 判定口径（实现常量） | 严重度 | 本轮命中 | 本轮覆盖页 |
|---|------|----------------------|--------|---------|-----------|
| ① | 只读路径框 | 向上 2 层内找到「选择/浏览/打开/清空」按钮同容器，且 `IsReadOnly=false` | High | 0 | 0 |
| ② | 输入宽度 | 可编辑 `TextBox` `ActualWidth<70`（`MinEditableInputWidth`）；路径类（占位含 路径/目录/文件）`<300`（`MinPathInputWidth`） | Medium | 0 | 0 |
| ③ | 标签对齐 | 同卡片、**纵向堆叠**的字段标签左边界差 `>1.5px`（`LabelAlignmentTolerance`）；横向并排的多列表单不算 | Low | 0 | 0 |
| ④ | 空态提示 | `ListView/ListBox/DataGrid/ItemsControl(非 ComboBox)` 0 项、或 `Canvas` 无子元素，且整页没有「还没有/暂无/尚未/请先/拖入/生成后」 | Medium | 0 | 0 |
| ⑤ | Tab 顺序 | 停靠点按 `KeyboardNavigation.GetTabIndex`（未设置 = `int.MaxValue`）优先、同值再按树序排序；落在 `ScrollViewer` 里的用**视口内**坐标比较，视口外不参与；连续两点中心 y 倒退 `>16px` 记一条，每页最多 3 条 | Low | 0 | 0 |
| ⑥ | 壳层按钮高度 | 导航/分组头按钮 `minButtonH` 门槛 **34**（`ShellAudit.MinNavButtonHeight`） | High | 0（自检反例命中） | 0 |
| ⑦ | 文字对比度 | 文字色 vs 实际背景 < 4.5:1（`MinTextContrast`）；装饰性中灰按 3:1（`MinMutedContrast`） | Medium | 0 | 0 |

七条规则本轮全 0 命中，因此每条都需要「不空转」的证据 —— 见 §4；同时 §7 记录了它们各自**曾经**打中过什么（14 / 28 / 0 / 3 / 53 / 0 / 6 条），一张规则表的全绿才有意义。

## 4 规则不空转的证据

### 4.1 自检（每条规则一个反例 + 三条独立断言）

`SELFTEST RESULT passed=20/20`：`viewport`、`empty-button`、`empty-label`、`overflow`、`mojibake`、`icon-button`、`tooltip-filter`（图标按钮有 ToolTip 必须放过）、`input`、`path-box`、`input-narrow`、`path-narrow`、`label-align`、`empty-state`、`tab-order`、`contrast`、`path-box-readonly-filter`（可编辑/只读两个路径框，红 1 绿 1）、`shell-legacy-expander`、`shell-collapsible-section`、`shell-min-height-34`、`tab-order-tabindex`。

规则 ⑤ 有**两条互为反向**的自证，缺一不可：

- `SELFTEST|tab-order|hit|count=2` —— 两个按钮**无** TabIndex、树序与视觉 y 相反 → 必须判红（否则规则等于关掉了）。
- `SELFTEST|tab-order-tabindex|hit|issues=0` —— 两个按钮**有** TabIndex（0 / 100）但树序与视觉相反 → 必须判绿（否则"显式 TabIndex 能纠正顺序"这条口径没落地）。
- `SELFTEST|tabindex-inherits|false|container-propagates=false|child-index=未设置` —— 机器事实：`KeyboardNavigation.TabIndex` 非继承，容器设 7 之后子元素的 `GetTabIndex` 仍是「未设置」。

### 4.2 规则 ① 的覆盖面计数

每次运行在 `PAGE|…` 之后多打一行 `UXBOX|<featureId>|considered=… readonly=… flagged=…`，统计「扫到几个路径框 / 几个因只读放过 / 几个判红」。权威一轮：

```
UXBOX|system.settings|considered=3 readonly=3 flagged=0
UXBOX|exam.circle|considered=2 readonly=2 flagged=0
UXBOX|twain.debug|considered=1 readonly=1 flagged=0
UXBOX|other.extra|considered=1 readonly=1 flagged=0
```

即：规则仍然在扫同一批 19 个路径框，只是本轮它们**全部**走了 `src/DocumentTools.App/Ui/PathInputs.cs` 的 `ForFile`/`ForFolder`（`:51` `box.IsReadOnly = true`、`:53` `IsTabStop = true`），所以 `flagged=0`。`considered` 不减、`readonly` 增加，这才叫"修好了"，而不是"检查器不看了"。

### 4.3 其他收敛

- 规则 ③ 由「同卡片所有标签」收敛为「同卡片纵向堆叠的标签」（`SharesVisualRow` 检测垂直区间重叠即跳过），把多列并排表单的 25 条误报清零，自检反例仍命中。
- 规则 ⑤ 只比较**同一滚动容器**内的相邻停靠点，并跳过视口外的元素：早期版本把「可滚动列表下方的路径卡」也拉进来比 y，制造了一批假阳性。
- 规则 ⑤ 每页最多记 3 条，避免同一处系统性成因把清单刷爆。

## 5 命中清单（权威一轮）

### 5.1 High / Medium

空。上一轮（00:40:59）仅剩的 `Medium 3` 是 `exam.report` 批量参数区三个 64px 行列号输入框（`< 70`），本轮已把它们加宽到 72px（`ExamReportView.cs:44-46`）后归零。

### 5.2 Low：无输入控件 6 条（观察项，非缺陷）

`home`、`about`、`word.merge`、`pdf.merge`、`code.datamatrix`、`other.misc` —— 这些页面要么只做展示（主页/关于/内嵌工具释放），要么整页只有一个「选择文件 + 开始」的最小交互（合并类）。保留它们是为了让"页面有可键盘操作的入口"这条不被悄悄丢掉；真正该改的是哪天这几个页面加了表单却忘了给输入框。

## 6 真问题清单（含本轮已修，供后续回归核对）

| ID | 严重度 | 问题 | 证据 / 位置 | 状态 |
|----|--------|------|-------------|------|
| UX-01 | High | 9 页路径输入框可编辑，与「选择…」按钮互相覆盖 | `UXBOX\|exam.single\|considered=3 readonly=0 flagged=3` 等 12 行（00:15:20 轮） | **已修**：各页改走 `Ui/PathInputs.cs`；`exam.circle` 两框由本任务改（原图/替换图），`exam.report` 三个 64px 框一并处理 |
| UX-02 | Medium | 错误/成功状态文字对比度 2.82:1 / 4.21:1 | `#FF6347`、`#228B22` 对 `#FAFAFA` | **已修**：`Ui/Controls/ToolViewKit.cs:187-194` 换成 `#C62828` / `#1B5E20` / `#1565C0`（注释写明 4.5:1 达标、勿改回） |
| UX-03 | Medium | `word.replace`/`excel.replace`/`other.extra` 列表 0 项无空态引导 | 快照 `word.replace.png` 列表区整片空白 | **已修**：转换线补齐空态提示 |
| UX-04 | Medium | 28 个 60–64px 数字输入框（页码/份数/行列号/旋转角） | 00:15:20 轮 §5.2 | **已修**：路径类改只读框后不再计入；纯数字框按 70px 下限加宽（本任务修 `exam.report` 三个） |
| UX-05 | Low | Tab 顺序从底部操作条跳回内容区（42 页 / 53–70 条） | `BuildBottomActionLayout` 把操作条先加入逻辑树 | **已修**：`FeaturePageBase` 由 `DockPanel` 改 `Grid`，按「内容 → 选项 → 操作条」的视觉顺序添加子元素，Tab 序天然正确（不再需要 TabIndex） |
| UX-06 | High | 27 页 `创建视图失败：元素已经是另一个元素的逻辑子元素` | `Ui/Controls/BorderedFileList.cs` `BuildEmptyOverlay()` 复用字段级 `TextBlock` | **已修**：Add 前判断并先从旧父级摘除（详见 §7） |

## 7 已修与本次回归（每一轮数字变化都能指到原因）

| 轮次（2026-11-06） | High / Medium / Low | 相对上一轮变化的原因 |
|--------------------|--------------------|----------------------|
| 00:15:20 | 14 / 39 / 76 | 规则 ⑤ 未改口径前的基线（Tab 70 条、对比度 6 条） |
| 00:25:51 | 14 / 32 / 59 | 规则 ⑤ 改 TabIndex + 视口口径（70 → 53 条）；`ToolViewKit` 语义色修好后对比度 6 → 0；`other.extra` 空态随 `MiscToolModules.cs` 重写消失 |
| 00:39:37 | 2 / 28 / 6 | `FeaturePageBase` 改 `Grid` 后 Tab **53 → 0**；路径框 14 → 2（各页改走 `PathInputs`，只剩 `exam.circle`）；空标签/空态清零 |
| 00:40:59 | 0 / 3 / 6 | `exam.circle` 原图/替换图改用 `PathInputs.ForFile` → 路径框 High 清零；三类数字框随只读化/加宽 28 → 3 |
| **00:41:59** | **0 / 0 / 6** | `exam.report` 三个行列号框 64 → 72px，Medium 归零 |

逐条说明：

- **27 页「创建视图失败」**：`InvalidOperationException: 指定的元素已经是另一个元素的逻辑子元素`。根因在 `src/DocumentTools.App/Ui/Controls/BorderedFileList.cs` 的 `BuildEmptyOverlay()`：每次 `stack.Children.Add(_emptyHintBlock)` 复用同一个字段级 `TextBlock`，而 `_emptyOverlay.Child = stack` 只换 Border 子元素、不会把该 `TextBlock` 从旧 `StackPanel` 摘除，于是同一控件第二次构造/重建就抛异常（`--only word.pdf` 单页隔离也能复现：进程里前面已建过页面）。修法：Add 前 `if (LogicalTreeHelper.GetParent(_emptyHintBlock) is Panel previousParent) previousParent.Children.Remove(_emptyHintBlock);`（`:513-521`）。修后 `FeatureSmoke` 47/47、启动落点 95 项 0 失败。
- **Tab 顺序（53 条 → 0）**：我先把规则口径改成「TabIndex 优先 + 视口内比较」，随后实测发现 `FeaturePageBase` 那版「给容器设 TabIndex」的修复**对键盘序无效**（非继承属性，实测修复前后数字完全一致），据此给出根因；`FeaturePageBase.BuildBottomActionLayout` 最终从 `DockPanel` 换成 `Grid`，让逻辑子元素顺序 = 视觉顺序，问题从根上消失，且不再依赖任何 TabIndex。
- **路径框（14 → 0）**：`exam.single`/`exam.package`/`exam.mss3`/`exam.report`/`exam.insert` 与 `html.pdf`/`pdf.watermark.add`/`word.epub` 改走 `PathInputs`；`exam.circle` 的「原图」「替换图」由本任务改（`ExamCircleView.cs:31-32`），并按"替换图可空"的语义只对「填了但文件不存在」给提示（`PathInputs.IsMissingFile` / `MissingFileNotice`）。
- **检查器自身两处缺陷**：见 §2.4，自检里各留一条反例作看门狗。
- **键表变更复核（`pdf.compliance` 5→27 项、`webp.image` 4→5 项）**：变更后两页 `H0/M0/L0`，新增命中 0。「下拉变宽把同行控件挤走」没有发生 —— 像素证据：`docs/qa-screens/pdf.compliance.png` 里「目标版本」下拉显示 `PDF_A_1A`、卡片内只有这一个字段；`docs/qa-screens/webp.image.png` 里「输出格式」显示 `JPG`，同样单字段。

## 8 人工抽查（3 张，与自动结论对照）

1. `docs/qa-screens/word.replace.png`：上一轮列表区整片空白，本轮已有空态引导 —— 与 UX-03 已修一致（早期版本该图为空，是本条缺陷的原始证据）。
2. `docs/qa-screens/exam.single.png`：「Word 模板 / 图片源 / 输出目录」三行现在是只读路径框 + 右侧「选择…」按钮 —— 与 UX-01 已修一致。
3. `docs/qa-screens/exam.circle.png`：「原图」「替换图」两行同样变成只读路径框，替换图仍标注「可空：用模板内容自身覆盖」—— 与 `UXBOX|exam.circle|considered=2 readonly=2 flagged=0` 互相印证。

## 9 残余风险

1. 规则 ③ 与 ⑤ 依赖几何阈值（1.5px / 16px）；页面结构大改（横向排布转多列）时可能重新误报，需靠 `--self-test` 与抽查复核。
2. 规则 ⑤ 现在尊重 `KeyboardNavigation.TabIndex`（非继承，只认停靠元素自身），但没有模拟 `KeyboardNavigation.TabNavigation="Cycle"/"Local"` 这类容器级策略；若将来有页面启用，需扩展。
3. 对比度按「最近的实色祖先背景」估算，遇到半透明叠层或渐变背景会退化为近似值。
4. 规则 ④ 的「整页有无空态文案」用的是文案关键词表（`EmptyStateMarkers`），新增文案风格（例如「空空如也」）需同步补词。
5. 快照在离屏 `Window`（`Left/Top=-4000`）里渲染，WPF-UI 的 `ControlAppearance` 解析已验证与真实窗口一致（`#FF1D6978` 主色可绘制），但悬停/焦点态样式仍不覆盖。
6. 本轮全 0 的前提是**同一时刻**的源码；并发开发期间他人中间态会让快照红或绿（本任务期间真实发生过两次：27 页创建异常、`MiscToolModules.cs` 编码损坏导致整解决方案无法构建）。判定某条缺陷"已修"必须附当轮时间戳与命令。
