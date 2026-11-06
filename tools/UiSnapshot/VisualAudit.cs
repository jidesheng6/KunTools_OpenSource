using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 对已 Measure/Arrange 的页面做机器可判定的视觉树检查，不依赖人工目视：
/// ① 控件是否超出页面视口或被不可滚动的父容器裁掉；② 空文本按钮/空标签/乱码文本；③ 键盘可达的输入控件是否存在；
/// ④ 只读路径框（与「选择/浏览/打开/清空」按钮同容器的 TextBox 必须只读）；⑤ 输入框与路径框过短；
/// ⑥ 同卡片字段标签左边界是否对齐；⑦ 列表/画布 0 项时是否有空态文案；⑧ Tab 顺序是否跨行倒退；⑨ 文字对比度不足。
/// 只读视觉树，不改布局、不触发业务。
/// </summary>
internal static class VisualAudit
{
    private const double Tolerance = 1.5;

    /// <summary>可编辑输入框的最小可用宽度：再窄就装不下三个汉字加光标。</summary>
    private const double MinEditableInputWidth = 70;

    /// <summary>路径/目录/文件类输入框的最小宽度：要能看全一个中等长度的绝对路径。</summary>
    private const double MinPathInputWidth = 300;

    /// <summary>同卡片内字段标签左边界允许的偏差。</summary>
    private const double LabelAlignmentTolerance = 1.5;

    /// <summary>正文文字的最小对比度（WCAG AA 普通文本）。</summary>
    private const double MinTextContrast = 4.5;

    /// <summary>装饰性提示文字（灰度 Muted 文案）放宽到 3:1（WCAG AA 大字号/非正文）。</summary>
    private const double MinMutedContrast = 3.0;

    /// <summary>Tab 顺序跨行倒退的纵向容差：同一行内控件因高度不同会有几像素基线差。</summary>
    private const double TabOrderRowTolerance = 16;

    /// <summary>与输入框同容器时，按钮文本命中这些词即视为「路径选择」按钮。</summary>
    private static readonly string[] BrowseButtonMarkers = ["选择", "浏览", "打开", "清空"];

    /// <summary>占位文案命中这些词即视为路径类输入框。</summary>
    private static readonly string[] PathPlaceholderMarkers = ["路径", "目录", "文件"];

    /// <summary>空态文案的常见措辞：命中任意一个就算「有提示」。</summary>
    private static readonly string[] EmptyStateMarkers = ["还没有", "暂无", "尚未", "请先", "拖入", "生成后"];

    private static readonly Regex MojibakePattern = new(@"\?{2,}|\uFFFD", RegexOptions.Compiled);

    private static readonly Dictionary<Type, PropertyInfo?> PlaceholderProperties = new();

    /// <summary>按类型名识别的输入控件集合（不直接引用 WPF-UI 类型，避免其改名导致本工具编译失败）。</summary>
    private static readonly HashSet<string> InputTypeNames = new(StringComparer.Ordinal)
    {
        "TextBox",
        "RichTextBox",
        "PasswordBox",
        "ComboBox",
        "CheckBox",
        "RadioButton",
        "Slider",
        "DatePicker",
        "NumberBox",
        "ListBox",
        "DataGrid",
        "ToggleSwitch",
        "AutoSuggestBox"
    };

    /// <summary>规则 1 的覆盖面计数：扫到几个路径框、其中几个只读放过、几个判红。</summary>
    internal static string PathBoxStats
    {
        get { return "considered=" + PathBoxConsidered + " readonly=" + PathBoxReadOnly + " flagged=" + PathBoxFlagged; }
    }

    private static int PathBoxConsidered;
    private static int PathBoxReadOnly;
    private static int PathBoxFlagged;

    internal static void Run(FrameworkElement host, int pageWidth, int pageHeight, PageSnapshot snapshot)
    {
        PathBoxConsidered = 0;
        PathBoxReadOnly = 0;
        PathBoxFlagged = 0;

        var elements = new List<FrameworkElement>();
        Collect(host, elements);

        snapshot.ElementCount = elements.Count;

        var viewport = new Rect(0, 0, pageWidth, pageHeight);
        int textBlocks = 0;
        int buttons = 0;
        bool pageHasEmptyStateText = PageHasEmptyStateText(elements);

        foreach (FrameworkElement element in elements)
        {
            if (element is TextBlock)
            {
                textBlocks++;
            }

            if (element is ButtonBase)
            {
                buttons++;
            }

            AuditGeometry(host, element, viewport, snapshot);
            AuditEmptyText(element, snapshot);
            AuditMojibake(element, snapshot);
            AuditPathBox(element, snapshot);
            AuditInputWidth(element, snapshot);
            AuditEmptyState(element, snapshot, pageHasEmptyStateText);
            AuditContrast(host, element, snapshot);

            if (element is System.Windows.Controls.Image image)
            {
                AuditImage(image, snapshot);
            }
        }

        snapshot.TextBlockCount = textBlocks;
        snapshot.ButtonCount = buttons;
        AuditKeyboard(elements, snapshot);
        AuditLabelAlignment(host, elements, snapshot);
        AuditTabOrder(host, elements, snapshot);
    }

    private static void Collect(DependencyObject root, List<FrameworkElement> sink)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            if (child is FrameworkElement element)
            {
                sink.Add(element);
            }

            Collect(child, sink);
        }
    }

    private static void AuditGeometry(FrameworkElement host, FrameworkElement element, Rect viewport, PageSnapshot snapshot)
    {
        if (ReferenceEquals(element, host) || element.Visibility != Visibility.Visible)
        {
            return;
        }

        if (element.ActualWidth < 1 || element.ActualHeight < 1)
        {
            return;
        }

        Rect bounds = BoundsInHost(host, element);
        if (bounds.IsEmpty)
        {
            return;
        }

        // 高 DPI 判定口径之一：元素的起点被推到页面左上之外（负坐标），多为宿主尺寸/边距算错。
        if (bounds.Left < -Tolerance || bounds.Top < -Tolerance)
        {
            snapshot.AddIssue(new AuditIssue(
                Severity.Medium,
                "负坐标",
                Describe(element),
                "元素起点在页面左上之外：" + FormatRect(bounds)));
        }

        var ancestors = new List<DependencyObject>();
        bool hasScrollViewer = false;
        bool scrollReachable = false;
        for (DependencyObject? node = VisualTreeHelper.GetParent(element); node is not null; node = VisualTreeHelper.GetParent(node))
        {
            ancestors.Add(node);
            if (node is ScrollViewer viewer)
            {
                hasScrollViewer = true;
                if (viewer.ScrollableHeight > 0.5 || viewer.ScrollableWidth > 0.5)
                {
                    scrollReachable = true;
                }
            }
        }

        if (!hasScrollViewer && Exceeds(bounds, viewport))
        {
            snapshot.AddIssue(new AuditIssue(
                Severity.High,
                "超出页面视口",
                Describe(element),
                "渲染区 " + FormatRect(bounds) + " 超出 0,0," + viewport.Width + "," + viewport.Height));
        }

        foreach (DependencyObject node in ancestors)
        {
            if (node is not FrameworkElement parent || !parent.ClipToBounds)
            {
                continue;
            }

            Rect parentBounds = BoundsInHost(host, parent);
            if (parentBounds.IsEmpty || !Exceeds(bounds, parentBounds))
            {
                continue;
            }

            if (!scrollReachable)
            {
                snapshot.AddIssue(new AuditIssue(
                    Severity.High,
                    "被父容器裁切",
                    Describe(element),
                    "父容器 " + Describe(parent) + " 区域 " + FormatRect(parentBounds) + " 小于子元素渲染区 " + FormatRect(bounds)));
            }

            break;
        }

        if (element is TextBlock block)
        {
            AuditTextOverflow(block, snapshot);
        }
    }

    /// <summary>
    /// 规则 1：与「选择/浏览/打开/清空」按钮同处一个容器（3 层祖先内）的 TextBox 必须是只读展示框。
    /// 可编辑时用户会以为手敲的路径生效，实际点按钮就被选择结果覆盖。
    /// </summary>
    private static void AuditPathBox(FrameworkElement element, PageSnapshot snapshot)
    {
        if (element.TemplatedParent is not null || element is not TextBoxBase box)
        {
            return;
        }

        if (box.Visibility != Visibility.Visible || box.ActualWidth < 1)
        {
            return;
        }

        if (!HasBrowseButtonNearby(box, 0))
        {
            return;
        }

        // 只读的也要计数：否则无法证明规则 1 真的扫到了路径框，而不是一条永不触发的空规则。
        PathBoxConsidered++;
        if (box.IsReadOnly)
        {
            PathBoxReadOnly++;
            return;
        }

        PathBoxFlagged++;
        snapshot.AddIssue(new AuditIssue(
            Severity.High,
            "路径框可编辑",
            Describe(box),
            "与「选择/浏览/打开/清空」按钮同容器，但 IsReadOnly=false：手敲路径与按钮选择会互相覆盖"));
    }

    /// <summary>
    /// 向上最多两层找「同一行/同一字段」容器：再往上就是页面级容器，
    /// 那里任何一处「选择…」按钮都会把不相干的输入框误判成路径框。
    /// </summary>
    private static bool HasBrowseButtonNearby(DependencyObject element, int depth)
    {
        if (depth >= 2)
        {
            return false;
        }

        DependencyObject? parent = VisualTreeHelper.GetParent(element);
        if (parent is null)
        {
            return false;
        }

        return ContainsBrowseButton(parent, element, 0) || HasBrowseButtonNearby(parent, depth + 1);
    }

    /// <summary>
    /// 只在容器自身与其直接子级里找按钮：整页根节点也满足「3 层祖先内」，
    /// 若向下无限递归，任何页面的路径框都会被页面上某个「选择…」按钮误伤。
    /// </summary>
    private static bool ContainsBrowseButton(DependencyObject container, DependencyObject self, int level)
    {
        if (container is Button button && !ReferenceEquals(container, self))
        {
            return IsBrowseLabel(TextOf(button.Content));
        }

        if (level >= 1)
        {
            return false;
        }

        int count;
        try
        {
            count = VisualTreeHelper.GetChildrenCount(container);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(container, i);
            if (ReferenceEquals(child, self))
            {
                continue;
            }

            if (ContainsBrowseButton(child, self, level + 1))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBrowseLabel(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (string marker in BrowseButtonMarkers)
        {
            if (text.Contains(marker, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>规则 2：可编辑输入框 &lt; 70px、路径类输入框 &lt; 300px 都是「填不进去也校对不了」。</summary>
    private static void AuditInputWidth(FrameworkElement element, PageSnapshot snapshot)
    {
        if (element.TemplatedParent is not null || element is not TextBoxBase box)
        {
            return;
        }

        if (box.Visibility != Visibility.Visible || box.IsReadOnly || box.ActualWidth < 1)
        {
            return;
        }

        // 表格单元格里的编辑框宽度由列宽决定，不按表单输入框的尺度判定。
        if (IsInsideType(box, "DataGridCell", 3))
        {
            return;
        }

        string placeholder = PlaceholderOf(box);
        bool pathLike = ContainsAny(placeholder, PathPlaceholderMarkers);
        double width = box.ActualWidth;

        if (pathLike && width < MinPathInputWidth)
        {
            snapshot.AddIssue(new AuditIssue(
                Severity.Medium,
                "路径框过短",
                Describe(box),
                "路径类占位「" + Trim(placeholder, 18) + "」宽度 " + N(width) + "px < " + N(MinPathInputWidth) + "px，长路径会被截断、无法校对"));
            return;
        }

        if (!pathLike && width < MinEditableInputWidth)
        {
            snapshot.AddIssue(new AuditIssue(
                Severity.Medium,
                "输入框过短",
                Describe(box),
                "可编辑输入框宽度 " + N(width) + "px < " + N(MinEditableInputWidth) + "px"));
        }
    }

    /// <summary>规则 3：同一卡片内的字段标签左边界必须对齐（容差 1.5px）。</summary>
    private static void AuditLabelAlignment(FrameworkElement host, List<FrameworkElement> elements, PageSnapshot snapshot)
    {
        var byCard = new Dictionary<FrameworkElement, List<FrameworkElement>>();

        foreach (FrameworkElement element in elements)
        {
            if (!IsFieldLabel(element))
            {
                continue;
            }

            FrameworkElement? card = CardOf(element);
            if (card is null)
            {
                continue;
            }

            if (!byCard.TryGetValue(card, out List<FrameworkElement>? labels))
            {
                labels = [];
                byCard[card] = labels;
            }

            labels.Add(element);
        }

        foreach (KeyValuePair<FrameworkElement, List<FrameworkElement>> pair in byCard)
        {
            if (Environment.GetEnvironmentVariable("QA_SELFTEST_VERBOSE") == "1")
            {
                Console.WriteLine("SELFTEST|labels|" + Describe(pair.Key) + "|count=" + pair.Value.Count.ToString(CultureInfo.InvariantCulture) + "|lefts=" + string.Join(",", pair.Value.Select(item => N(BoundsInHost(host, item).Left))));
            }

            if (pair.Value.Count < 2)
            {
                continue;
            }

            var labelBounds = new List<Rect>();
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (FrameworkElement label in pair.Value)
            {
                Rect bounds = BoundsInHost(host, label);
                if (bounds.IsEmpty)
                {
                    continue;
                }

                labelBounds.Add(bounds);
                min = Math.Min(min, bounds.Left);
                max = Math.Max(max, bounds.Left);
            }

            // 横向并排的多列表单（同一个 WrapPanel/Grid 行里铺开多个字段）左边界本来就不同，
            // 那不是「没对齐」；只有纵向堆叠的行之间左边界差异才算缺陷。
            if (SharesVisualRow(labelBounds))
            {
                continue;
            }

            if (min == double.MaxValue || max - min <= LabelAlignmentTolerance)
            {
                continue;
            }

            snapshot.AddIssue(new AuditIssue(
                Severity.Low,
                "标签未对齐",
                Describe(pair.Value[0]),
                "同一卡片内 " + pair.Value.Count.ToString(CultureInfo.InvariantCulture) + " 个字段标签左边界不一致：最小 " + N(min) + "px、最大 " + N(max) + "px，偏差 " + N(max - min) + "px"));
        }
    }

    /// <summary>横向并排判定：任意两个标签的垂直区间重叠，说明它们铺在同一视觉行里（多列表单）。</summary>
    private static bool SharesVisualRow(List<Rect> bounds)
    {
        for (int i = 0; i < bounds.Count; i++)
        {
            for (int j = i + 1; j < bounds.Count; j++)
            {
                double top = Math.Max(bounds[i].Top, bounds[j].Top);
                double bottom = Math.Min(bounds[i].Bottom, bounds[j].Bottom);
                if (bottom - top > 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>字段标签候选：文本非空的 Label/TextBlock，且同一个父容器里紧邻的可见兄弟是输入控件或按钮。</summary>
    private static bool IsFieldLabel(FrameworkElement element)
    {
        if (element.TemplatedParent is not null || element.Visibility != Visibility.Visible)
        {
            return false;
        }

        string? text = element switch
        {
            Label label => TextOf(label.Content),
            TextBlock block => string.IsNullOrWhiteSpace(block.Text) ? null : block.Text,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        DependencyObject? parent = VisualTreeHelper.GetParent(element);
        if (parent is null or ButtonBase)
        {
            return false;
        }

        int index = IndexInParent(parent, element);
        if (index < 0)
        {
            return false;
        }

        // 行首的才是「字段标签」；行中间的说明文字（输入框后面跟的「空=不改」这类）不算，
        // 否则同一个卡片里多列并排的表单会被误判成「标签没对齐」。
        for (int i = 0; i < index; i++)
        {
            if (VisualTreeHelper.GetChild(parent, i) is FrameworkElement before && before.Visibility == Visibility.Visible)
            {
                return false;
            }
        }

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = index + 1; i < count; i++)
        {
            if (VisualTreeHelper.GetChild(parent, i) is not FrameworkElement sibling || sibling.Visibility != Visibility.Visible)
            {
                continue;
            }

            return IsInput(sibling) || sibling is Button;
        }

        return false;
    }

    private static int IndexInParent(DependencyObject parent, DependencyObject child)
    {
        int count;
        try
        {
            count = VisualTreeHelper.GetChildrenCount(parent);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }

        for (int i = 0; i < count; i++)
        {
            if (ReferenceEquals(VisualTreeHelper.GetChild(parent, i), child))
            {
                return i;
            }
        }

        return -1;
    }

    private static FrameworkElement? CardOf(DependencyObject element)
    {
        int depth = 0;
        for (DependencyObject? node = VisualTreeHelper.GetParent(element); node is not null && depth < 4; node = VisualTreeHelper.GetParent(node), depth++)
        {
            if (node is Border or GroupBox)
            {
                return node as FrameworkElement;
            }
        }

        return null;
    }

    /// <summary>规则 4：列表/画布 0 项且整页没有一句空态引导语 → 用户只看到一片空白。</summary>
    private static void AuditEmptyState(FrameworkElement element, PageSnapshot snapshot, bool pageHasEmptyStateText)
    {
        if (pageHasEmptyStateText || element.TemplatedParent is not null || element.Visibility != Visibility.Visible)
        {
            return;
        }

        string? kind = null;
        if (element is ListBox or ListView or DataGrid)
        {
            if (((ItemsControl)element).Items.Count == 0 && element.ActualHeight >= 40)
            {
                kind = "列表";
            }
        }
        else if (element is ItemsControl items && element is not ComboBox && items.Items.Count == 0 && element.ActualHeight >= 60)
        {
            kind = "列表";
        }
        else if (element is Canvas canvas && canvas.Children.Count == 0 && canvas.ActualHeight >= 80)
        {
            kind = "画布";
        }

        if (kind is null)
        {
            return;
        }

        snapshot.AddIssue(new AuditIssue(
            Severity.Medium,
            "缺空态提示",
            Describe(element),
            kind + "在 0 项时没有任何占位/引导文案（整页也没有「还没有 / 暂无 / 请先 …」这类提示），用户只看到空白区域"));
    }

    private static bool PageHasEmptyStateText(List<FrameworkElement> elements)
    {
        foreach (FrameworkElement element in elements)
        {
            if (element.TemplatedParent is not null || element.Visibility != Visibility.Visible)
            {
                continue;
            }

            if (element is TextBlock block && ContainsAny(block.Text, EmptyStateMarkers))
            {
                return true;
            }

            if (element is TextBoxBase box && ContainsAny(PlaceholderOf(box), EmptyStateMarkers))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 规则 5：键盘顺序倒退。排序键优先取 <see cref="KeyboardNavigation.GetTabIndex"/>（未显式设置时该值是
    /// <see cref="int.MaxValue"/>，此时退回视觉树顺序），同一排序键内再按树序稳定排序；落在 <see cref="ScrollViewer"/>
    /// 后代里的元素改用「视口内坐标」比较，视口外的元素（键盘用户要先滚动才看得见）不参与判定。
    /// 这两条把「DockPanel 先挂底部操作条」「可滚动列表后面的路径卡」这两类正常布局排除在外，
    /// 只留下真正的同屏倒退。
    /// </summary>
    private static void AuditTabOrder(FrameworkElement host, List<FrameworkElement> elements, PageSnapshot snapshot)
    {
        var stops = new List<TabStop>();
        int treeIndex = 0;

        foreach (FrameworkElement element in elements)
        {
            treeIndex++;
            if (!element.Focusable || !element.IsEnabled || element.Visibility != Visibility.Visible)
            {
                continue;
            }

            if (!KeyboardNavigation.GetIsTabStop(element) || element.ActualHeight < 1)
            {
                continue;
            }

            ScrollViewer? scroller = FindScroller(element);
            double centerY;
            if (scroller is null)
            {
                Rect bounds = BoundsInHost(host, element);
                if (bounds.IsEmpty)
                {
                    continue;
                }

                centerY = bounds.Top + (bounds.Height / 2);
            }
            else if (!TryGetViewportCenterY(element, scroller, out centerY))
            {
                continue;
            }

            stops.Add(new TabStop(
                element,
                treeIndex,
                KeyboardNavigation.GetTabIndex(element),
                centerY,
                scroller));
        }

        // 显式 TabIndex 优先（数值升序），同值或未设置（int.MaxValue）按树序，保证「本来就对」的页面不乱跳。
        List<TabStop> ordered = stops
            .OrderBy(stop => stop.TabIndex)
            .ThenBy(stop => stop.TreeIndex)
            .ToList();

        int reported = 0;
        var lastByGroup = new Dictionary<object, TabStop>();

        foreach (TabStop current in ordered)
        {
            if (reported >= 3)
            {
                break;
            }

            // 不同滚动容器之间没有共同坐标原点，跨容器比较 y 没有意义。
            object group = current.Scroller is null ? host : current.Scroller;
            if (lastByGroup.TryGetValue(group, out TabStop last)
                && current.CenterY < last.CenterY - TabOrderRowTolerance)
            {
                snapshot.AddIssue(new AuditIssue(
                    Severity.Low,
                    "Tab 顺序倒退",
                    Describe(current.Element),
                    "键盘顺序上它排在 " + Describe(last.Element) + " 之后（TabIndex="
                    + FormatTabIndex(current.TabIndex) + "），中心 y=" + N(current.CenterY)
                    + " 却比前者的 y=" + N(last.CenterY) + " 更靠上（同一屏内可见）"));
                reported++;
            }

            lastByGroup[group] = current;
        }
    }

    private static string FormatTabIndex(int tabIndex)
    {
        return tabIndex == int.MaxValue ? "未设置" : tabIndex.ToString(CultureInfo.InvariantCulture);
    }

    private static ScrollViewer? FindScroller(DependencyObject element)
    {
        DependencyObject? node = VisualTreeHelper.GetParent(element);
        while (node is not null)
        {
            if (node is ScrollViewer scroller)
            {
                return scroller;
            }

            node = VisualTreeHelper.GetParent(node);
        }

        return null;
    }

    /// <summary>把元素换算到最近 ScrollViewer 的视口坐标；完全在视口外时返回 false（不参与 Tab 顺序判定）。</summary>
    private static bool TryGetViewportCenterY(FrameworkElement element, ScrollViewer scroller, out double centerY)
    {
        centerY = 0;
        try
        {
            Rect content = element
                .TransformToAncestor(scroller)
                .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

            double top = content.Top - scroller.VerticalOffset;
            double bottom = top + content.Height;
            if (bottom <= 0 || top >= scroller.ViewportHeight)
            {
                return false;
            }

            centerY = top + (content.Height / 2);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private readonly struct TabStop
    {
        public TabStop(FrameworkElement element, int treeIndex, int tabIndex, double centerY, ScrollViewer? scroller)
        {
            Element = element;
            TreeIndex = treeIndex;
            TabIndex = tabIndex;
            CenterY = centerY;
            Scroller = scroller;
        }

        public FrameworkElement Element { get; }

        public int TreeIndex { get; }

        public int TabIndex { get; }

        public double CenterY { get; }

        public ScrollViewer? Scroller { get; }
    }

    /// <summary>规则 6：文字前景/背景对比度 &lt; 4.5:1 报出；灰度装饰文案放宽到 3:1。</summary>
    private static void AuditContrast(FrameworkElement host, FrameworkElement element, PageSnapshot snapshot)
    {
        if (element is not TextBlock block || element.TemplatedParent is not null)
        {
            return;
        }

        if (element.Visibility != Visibility.Visible || string.IsNullOrWhiteSpace(block.Text) || block.ActualWidth < 1)
        {
            return;
        }

        // 按钮/表头等控件模板内的文字由主题色决定，不在本规则范围内（模板部件已在上面被 TemplatedParent 过滤）。
        if (IsInsideType(block, "ButtonBase", 3) || IsInsideType(block, "DataGridColumnHeader", 3))
        {
            return;
        }

        if (!TryGetColor(block.Foreground, out Color foreground))
        {
            return;
        }

        Color background = ResolveBackground(block);
        double ratio = ContrastRatio(foreground, background);
        bool muted = IsMutedGray(foreground);
        double required = muted ? MinMutedContrast : MinTextContrast;

        if (ratio >= required - 0.01)
        {
            return;
        }

        snapshot.AddIssue(new AuditIssue(
            Severity.Medium,
            "对比度不足",
            Describe(block),
            "文字 " + Hex(foreground) + " 对背景 " + Hex(background) + " 对比度 " + ratio.ToString("0.00", CultureInfo.InvariantCulture)
            + ":1，低于" + (muted ? "装饰文案 3:1" : "正文 4.5:1") + "门槛"));
    }

    private static bool TryGetColor(Brush? brush, out Color color)
    {
        switch (brush)
        {
            case SolidColorBrush solid when solid.Color.A > 0:
                color = solid.Color;
                return true;
            default:
                color = Colors.Transparent;
                return false;
        }
    }

    private static Color ResolveBackground(DependencyObject element)
    {
        for (DependencyObject? node = element; node is not null; node = VisualTreeHelper.GetParent(node))
        {
            if (node is Panel or Border or Control)
            {
                Brush? brush = node switch
                {
                    Panel panel => panel.Background,
                    Border border => border.Background,
                    Control control => control.Background,
                    _ => null
                };

                if (TryGetColor(brush, out Color color) && color.A == 255 && color != Colors.White)
                {
                    return color;
                }
            }
        }

        return Colors.White;
    }

    private static double ContrastRatio(Color foreground, Color background)
    {
        double first = RelativeLuminance(foreground);
        double second = RelativeLuminance(background);
        double lighter = Math.Max(first, second);
        double darker = Math.Min(first, second);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(Color color)
    {
        return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));

        static double Channel(byte value)
        {
            double scaled = value / 255.0;
            return scaled <= 0.03928 ? scaled / 12.92 : Math.Pow((scaled + 0.055) / 1.055, 2.4);
        }
    }

    private static bool IsMutedGray(Color color)
    {
        if (Math.Abs(color.R - color.G) > 8 || Math.Abs(color.G - color.B) > 8)
        {
            return false;
        }

        return color.R is >= 0x50 and <= 0x99;
    }

    private static string Hex(Color color)
        => "#" + color.R.ToString("X2", CultureInfo.InvariantCulture)
           + color.G.ToString("X2", CultureInfo.InvariantCulture)
           + color.B.ToString("X2", CultureInfo.InvariantCulture);

    private static string PlaceholderOf(FrameworkElement element)
    {
        if (element.GetType().Name != "TextBox")
        {
            return string.Empty;
        }

        PropertyInfo? property = PlaceholderPropertyOf(element.GetType());
        return property?.GetValue(element) as string ?? string.Empty;
    }

    private static bool ContainsAny(string? text, string[] markers)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (string marker in markers)
        {
            if (text.Contains(marker, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInsideType(DependencyObject element, string typeName, int maxDepth)
    {
        int depth = 0;
        for (DependencyObject? node = VisualTreeHelper.GetParent(element); node is not null && depth < maxDepth; node = VisualTreeHelper.GetParent(node), depth++)
        {
            string name = node.GetType().Name;
            if (string.Equals(name, typeName, StringComparison.Ordinal) || InheritsFrom(node.GetType(), typeName))
            {
                return true;
            }
        }

        return false;
    }

    private static bool InheritsFrom(Type type, string baseTypeName)
    {
        for (Type? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.Name, baseTypeName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string N(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static bool HasAccessibleName(FrameworkElement element)
    {
        if (!string.IsNullOrWhiteSpace(System.Windows.Automation.AutomationProperties.GetName(element)))
        {
            return true;
        }

        return element.ToolTip is string tip && tip.Trim().Length > 0;
    }

    /// <summary>有 Source 却渲染成 0 尺寸的 Image：高 DPI 判定口径之一，通常是图没加载或被布局压扁。</summary>
    private static void AuditImage(System.Windows.Controls.Image image, PageSnapshot snapshot)
    {
        if (image.TemplatedParent is not null || image.Source is null || image.Visibility != Visibility.Visible)
        {
            return;
        }

        if (image.ActualWidth >= 1 && image.ActualHeight >= 1)
        {
            return;
        }

        snapshot.AddIssue(new AuditIssue(
            Severity.Low,
            "图像未渲染",
            Describe(image),
            "Image 有 Source 但渲染尺寸为 "
            + image.ActualWidth.ToString("0.#", CultureInfo.InvariantCulture) + "×"
            + image.ActualHeight.ToString("0.#", CultureInfo.InvariantCulture)));
    }

    /// <summary>无换行、无省略号的 TextBlock 若自然宽度超过可用宽度，文字会溢出压到相邻控件上。</summary>
    private static void AuditTextOverflow(TextBlock block, PageSnapshot snapshot)
    {
        if (string.IsNullOrEmpty(block.Text) || block.TextWrapping != TextWrapping.NoWrap || block.TextTrimming != TextTrimming.None)
        {
            return;
        }

        // 关键：WPF 的 TextBlock 不裁剪时会把 RenderSize/ActualWidth 撑成自然宽度，
        // 用 ActualWidth 永远判不出溢出。真正可用的是父容器分配的布局槽（LayoutSlot）。
        double slotWidth = block.ActualWidth;
        try
        {
            Rect slot = LayoutInformation.GetLayoutSlot(block);
            if (slot.Width > 0)
            {
                slotWidth = slot.Width;
            }
        }
        catch (InvalidOperationException)
        {
            // 未参与布局时退回 ActualWidth。
        }

        double available = slotWidth
            - block.Padding.Left
            - block.Padding.Right;

        if (available < 4)
        {
            return;
        }

        double pixelsPerDip = 1.0;
        try
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(block);
            pixelsPerDip = dpi.PixelsPerDip;
        }
        catch (InvalidOperationException)
        {
            // 未连接到窗口时按 96dpi 处理，与本工具的渲染分辨率一致。
        }

        var typeface = new Typeface(block.FontFamily, block.FontStyle, block.FontWeight, block.FontStretch);
        var formatted = new FormattedText(
            block.Text,
            CultureInfo.CurrentUICulture,
            block.FlowDirection,
            typeface,
            block.FontSize,
            Brushes.Black,
            pixelsPerDip);

        double natural = formatted.WidthIncludingTrailingWhitespace;
        if (natural <= available + Tolerance)
        {
            return;
        }

        snapshot.AddIssue(new AuditIssue(
            Severity.Low,
            "文本溢出",
            Describe(block),
            "自然宽度 " + natural.ToString("0.0", CultureInfo.InvariantCulture)
            + "px 超过可用宽度 " + available.ToString("0.0", CultureInfo.InvariantCulture) + "px，未设置换行或省略号"));
    }

    private static void AuditEmptyText(FrameworkElement element, PageSnapshot snapshot)
    {
        // 只检查页面作者写的控件；控件模板里的内部部件（ScrollBar 的 RepeatButton 等）无文本属正常。
        if (element.TemplatedParent is not null)
        {
            return;
        }

        switch (element)
        {
            // 只把真正的按钮类当按钮：GridViewColumnHeader/DataGridColumnHeader 也派生自 ButtonBase，
            // 但它们本来就没有文本内容，纳入会产生大量噪声。
            case Button button:
                string? content = TextOf(button.Content);
                if (content is null)
                {
                    // 图标按钮是合理设计，但必须有 ToolTip 或 AutomationProperties.Name 提供可访问名称。
                    if (HasAccessibleName(button))
                    {
                        break;
                    }

                    snapshot.AddIssue(new AuditIssue(Severity.Low, "无文本按钮", Describe(button), "按钮内容为图形/容器，且没有 ToolTip 或自动化名称"));
                }
                else if (content.Trim().Length == 0)
                {
                    snapshot.AddIssue(new AuditIssue(Severity.Medium, "空文本按钮", Describe(button), "按钮文本为空，用户无法判断该按钮的功能"));
                }

                break;

            case Label label:
                string? labelText = TextOf(label.Content);
                if (labelText is null || labelText.Trim().Length == 0)
                {
                    snapshot.AddIssue(new AuditIssue(Severity.Medium, "空标签", Describe(label), "Label 内容为空"));
                }

                break;

            case TextBlock block:
                if (string.IsNullOrWhiteSpace(block.Text) && block.Inlines.Count == 0 && IsInsideCard(block))
                {
                    snapshot.AddIssue(new AuditIssue(Severity.Medium, "空标签", Describe(block), "卡片/容器内的 TextBlock 文本为空，通常是漏填的标签"));
                }

                break;
        }
    }

    private static bool IsInsideCard(DependencyObject element)
    {
        int depth = 0;
        for (DependencyObject? node = VisualTreeHelper.GetParent(element); node is not null && depth < 3; node = VisualTreeHelper.GetParent(node), depth++)
        {
            if (node is ButtonBase)
            {
                // 按钮内的空标签已经计入「空文本按钮」，不重复计。
                return false;
            }

            if (node is Border or GroupBox)
            {
                return true;
            }
        }

        return false;
    }

    private static void AuditMojibake(FrameworkElement element, PageSnapshot snapshot)
    {
        if (element.TemplatedParent is not null)
        {
            return;
        }

        foreach (string text in TextsOf(element))
        {
            if (text.Length < 2 || !MojibakePattern.IsMatch(text))
            {
                continue;
            }

            snapshot.AddIssue(new AuditIssue(Severity.High, "乱码文本", Describe(element), "文本疑似乱码：" + Trim(text, 40)));
            return;
        }
    }

    private static IEnumerable<string> TextsOf(FrameworkElement element)
    {
        if (element is TextBlock block && !string.IsNullOrEmpty(block.Text))
        {
            yield return block.Text;
        }

        if (element is ContentControl control && control.Content is string content && content.Length > 0)
        {
            yield return content;
        }

        if (element is HeaderedContentControl headered && headered.Header is string header && header.Length > 0)
        {
            yield return header;
        }

        if (element is HeaderedItemsControl headeredItems && headeredItems.Header is string itemsHeader && itemsHeader.Length > 0)
        {
            yield return itemsHeader;
        }

        if (element.GetType().Name == "TextBox")
        {
            PropertyInfo? property = PlaceholderPropertyOf(element.GetType());
            if (property?.GetValue(element) is string placeholder && placeholder.Length > 0)
            {
                yield return placeholder;
            }
        }
    }

    private static PropertyInfo? PlaceholderPropertyOf(Type type)
    {
        lock (PlaceholderProperties)
        {
            if (PlaceholderProperties.TryGetValue(type, out PropertyInfo? cached))
            {
                return cached;
            }

            PropertyInfo? property = type.GetProperty("PlaceholderText", typeof(string));
            PlaceholderProperties[type] = property;
            return property;
        }
    }

    private static void AuditKeyboard(List<FrameworkElement> elements, PageSnapshot snapshot)
    {
        int tabStops = 0;
        FrameworkElement? firstInput = null;

        foreach (FrameworkElement element in elements)
        {
            if (!element.Focusable || !element.IsEnabled || element.Visibility != Visibility.Visible)
            {
                continue;
            }

            if (!KeyboardNavigation.GetIsTabStop(element))
            {
                continue;
            }

            tabStops++;
            if (firstInput is null && IsInput(element))
            {
                firstInput = element;
            }
        }

        snapshot.TabStopCount = tabStops;
        snapshot.FirstInput = firstInput is null ? null : Describe(firstInput);

        if (tabStops == 0)
        {
            snapshot.AddIssue(new AuditIssue(Severity.High, "键盘不可达", "-", "页面没有任何可通过 Tab 聚焦的控件"));
        }
        else if (firstInput is null)
        {
            snapshot.AddIssue(new AuditIssue(Severity.Low, "无输入控件", "-", "页面只有按钮/列表，没有文本、下拉、复选等输入控件"));
        }
    }

    private static bool IsInput(FrameworkElement element)
        => element is TextBoxBase || InputTypeNames.Contains(element.GetType().Name);

    private static Rect BoundsInHost(Visual host, FrameworkElement element)
    {
        try
        {
            return element.TransformToAncestor(host).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        }
        catch (InvalidOperationException)
        {
            return Rect.Empty;
        }
        catch (ArgumentException)
        {
            return Rect.Empty;
        }
    }

    private static bool Exceeds(Rect inner, Rect outer)
        => inner.Left < outer.Left - Tolerance
           || inner.Top < outer.Top - Tolerance
           || inner.Right > outer.Right + Tolerance
           || inner.Bottom > outer.Bottom + Tolerance;

    private static string FormatRect(Rect rect)
        => rect.X.ToString("0", CultureInfo.InvariantCulture)
           + "," + rect.Y.ToString("0", CultureInfo.InvariantCulture)
           + "," + rect.Width.ToString("0", CultureInfo.InvariantCulture)
           + "×" + rect.Height.ToString("0", CultureInfo.InvariantCulture);

    /// <summary>取出控件里第一个可见文本（按钮内容可能是字符串、TextBlock、或包着 TextBlock 的容器）。</summary>
    private static string? TextOf(object? content)
    {
        switch (content)
        {
            case null:
                return null;
            case string text:
                return text;
            case TextBlock block:
                return block.Text;
            case AccessText accessText:
                return accessText.Text;
            case HeaderedContentControl headered:
                return TextOf(headered.Content) ?? TextOf(headered.Header);
            case ContentControl control:
                return TextOf(control.Content);
            case Panel panel:
                var parts = new List<string>();
                foreach (UIElement child in panel.Children)
                {
                    string? part = child switch
                    {
                        TextBlock blockChild => blockChild.Text,
                        ContentControl controlChild => TextOf(controlChild.Content),
                        Panel panelChild => TextOf(panelChild),
                        _ => null
                    };

                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        parts.Add(part.Trim());
                    }
                }

                return string.Join(" ", parts);
            default:
                return null;
        }
    }

    private static string Describe(FrameworkElement element)
    {
        string? text = element switch
        {
            TextBlock block => block.Text,
            ContentControl control => TextOf(control.Content),
            _ => null
        };

        string type = element.GetType().Name;
        string name = string.IsNullOrEmpty(element.Name) ? string.Empty : "#" + element.Name;
        string trimmed = Trim(text, 24);

        return trimmed.Length == 0 ? type + name : type + name + "「" + trimmed + "」";
    }

    private static string Trim(string? text, int max)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string flat = text.Replace("\r", " ").Replace("\n", " ").Trim();
        return flat.Length <= max ? flat : flat[..max] + "…";
    }
}
