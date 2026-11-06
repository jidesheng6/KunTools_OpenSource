using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentTools.Core.Features;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 检查器自检：用合成页面构造六类已知缺陷，断言 <see cref="VisualAudit"/> 全部命中。
/// 真实 47 页里某项为 0 条时，这个自检证明规则不是空转（否则报告里的 0 没有说服力）。
/// 只在本进程内构造离屏元素，不写仓库文件、不触业务。
/// </summary>
internal static class AuditSelfTest
{
    internal static int Run()
    {
        var root = new Grid
        {
            Width = SnapshotRunner.PageWidth,
            Height = SnapshotRunner.PageHeight,
            Background = Brushes.White
        };

        // ① 超出视口：Canvas 不裁剪，子元素放在 x=1250（视口宽 1200）。
        var canvas = new Canvas
        {
            Width = SnapshotRunner.PageWidth,
            Height = 100,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        var outside = new TextBlock { Text = "越界文本", Width = 80, Height = 20 };
        Canvas.SetLeft(outside, 1250);
        Canvas.SetTop(outside, 10);
        canvas.Children.Add(outside);
        root.Children.Add(canvas);

        var panel = new StackPanel
        {
            Margin = new Thickness(10, 120, 10, 10),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // ② 空文本按钮
        panel.Children.Add(new Button { Content = string.Empty, Width = 100, Height = 28 });

        // ③ 卡片内的空标签
        panel.Children.Add(new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Gray,
            Padding = new Thickness(4),
            Child = new TextBlock { Text = string.Empty, Width = 60, Height = 18 }
        });

        // ④ 文本溢出：真实页面形态是 TextBlock 放在固定宽度的容器里（直接设 TextBlock.Width 会被忽略）。
        var overflowHost = new Grid { Width = 80, Height = 18 };
        overflowHost.Children.Add(new TextBlock
        {
            Text = new string('W', 60),
            TextWrapping = TextWrapping.NoWrap
        });
        panel.Children.Add(overflowHost);

        // ⑤ 乱码文本
        panel.Children.Add(new TextBlock { Text = "?????????", Width = 80, Height = 18 });

        // ⑥ 键盘可达的输入控件
        panel.Children.Add(new TextBox { Width = 200, Height = 24 });

        // ⑦ 图标按钮：没有 ToolTip 的应被报出；有 ToolTip 的应被放过（唯一例外）。
        panel.Children.Add(new Button
        {
            Content = new Border { Width = 16, Height = 16, Background = Brushes.Gray },
            Width = 40,
            Height = 28
        });
        panel.Children.Add(new Button
        {
            Content = new Border { Width = 16, Height = 16, Background = Brushes.Gray },
            Width = 40,
            Height = 28,
            ToolTip = "图标按钮（有提示）"
        });

        root.Children.Add(panel);

        // ⑨~⑮ UX 规则反例（task-44）：每条造一个「必须命中」的形态，证明规则不是空转。
        var ux = new StackPanel
        {
            Margin = new Thickness(420, 120, 10, 10),
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 700
        };

        // ⑨ 只读路径框：可编辑的必须报 High；只读的必须放过（同页两例，顺带验证过滤）。
        ux.Children.Add(BuildPathRow(isReadOnly: false, width: 320));
        ux.Children.Add(BuildPathRow(isReadOnly: true, width: 320));

        // ⑩ 输入框过短 / ⑪ 路径框过短：放在没有按钮的容器里，避免与 ⑨ 混在一起。
        ux.Children.Add(new TextBox { Width = 50, Height = 24 });
        var pathOnly = new StackPanel();
        pathOnly.Children.Add(new Wpf.Ui.Controls.TextBox
        {
            Width = 200,
            Height = 24,
            PlaceholderText = "输出文件路径"
        });
        ux.Children.Add(pathOnly);

        // ⑫ 标签未对齐：同一卡片两行，第二行标签多 12px 左边距。
        ux.Children.Add(BuildMisalignedCard());

        // ⑬ 空态：卡片里一个 0 项列表，整页也没有「还没有 / 请先」这类引导语。
        ux.Children.Add(new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Gray,
            Padding = new Thickness(4),
            Margin = new Thickness(0, 6, 0, 6),
            Child = new ListView { Height = 100 }
        });

        // ⑭ Tab 顺序倒退：左列两个按钮（y 递增），右列一个按钮（y 回到第一行）。
        ux.Children.Add(BuildTabOrderTrap());

        // ⑮ 对比度：浅灰文字落在白底上。
        ux.Children.Add(new TextBlock
        {
            Text = "浅灰提示文字",
            Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
            Width = 200,
            Height = 18
        });

        root.Children.Add(ux);

        root.Measure(new Size(SnapshotRunner.PageWidth, SnapshotRunner.PageHeight));
        root.Arrange(new Rect(0, 0, SnapshotRunner.PageWidth, SnapshotRunner.PageHeight));
        root.UpdateLayout();

        var feature = new FeatureDescriptor("selftest", "检查器自检", FeatureCategory.Tools, "合成页面");
        var snapshot = new PageSnapshot(0, feature, "selftest.png");
        VisualAudit.Run(root, SnapshotRunner.PageWidth, SnapshotRunner.PageHeight, snapshot);

        // 排查用：QA_SELFTEST_VERBOSE=1 时把每条问题连元素一起打出来。
        if (Environment.GetEnvironmentVariable("QA_SELFTEST_VERBOSE") == "1")
        {
            foreach (AuditIssue issue in snapshot.Issues)
            {
                Console.WriteLine("SELFTEST|issue|" + issue.Kind + "|" + issue.Element + "|" + issue.Detail);
            }
        }

        (string Key, string Kind)[] expectations =
        {
            ("viewport", "超出页面视口"),
            ("empty-button", "空文本按钮"),
            ("empty-label", "空标签"),
            ("overflow", "文本溢出"),
            ("mojibake", "乱码文本"),
            ("icon-button", "无文本按钮"),
            ("path-box", "路径框可编辑"),
            ("input-narrow", "输入框过短"),
            ("path-narrow", "路径框过短"),
            ("label-align", "标签未对齐"),
            ("empty-state", "缺空态提示"),
            ("tab-order", "Tab 顺序倒退"),
            ("contrast", "对比度不足")
        };

        int passed = 0;
        foreach ((string key, string kind) in expectations)
        {
            int hits = snapshot.Issues.Count(issue => string.Equals(issue.Kind, kind, StringComparison.Ordinal));
            bool ok = hits > 0;
            if (ok)
            {
                passed++;
            }

            Console.WriteLine("SELFTEST|" + key + "|" + (ok ? "hit" : "MISS") + "|count=" + hits);
        }

        // 有 ToolTip 的图标按钮不计入，所以「无文本按钮」应恰好 1 条。
        int iconOnly = snapshot.Issues.Count(issue => string.Equals(issue.Kind, "无文本按钮", StringComparison.Ordinal));
        bool tipFiltered = iconOnly == 1;
        if (tipFiltered)
        {
            passed++;
        }

        Console.WriteLine("SELFTEST|tooltip-filter|" + (tipFiltered ? "hit" : "MISS") + "|count=" + iconOnly);

        // 只读的路径框必须放过：同页两例，若规则把 IsReadOnly=true 也报出来，这条会变成 2。
        int pathBoxHits = snapshot.Issues.Count(issue => string.Equals(issue.Kind, "路径框可编辑", StringComparison.Ordinal));
        bool readOnlyFiltered = pathBoxHits == 1;
        if (readOnlyFiltered)
        {
            passed++;
        }

        Console.WriteLine("SELFTEST|path-box-readonly-filter|" + (readOnlyFiltered ? "hit" : "MISS") + "|count=" + pathBoxHits);

        bool input = snapshot.FirstInput is not null;
        if (input)
        {
            passed++;
        }

        Console.WriteLine("SELFTEST|input|" + (input ? "hit" : "MISS") + "|type=" + (snapshot.FirstInput ?? "-"));

        // ⑧ 壳层检查自检：旧实现（Expander 套在 ScrollViewer→StackPanel 里）必须判红，
        //    现实现（CollapsibleSection）必须判绿。这条不空转，报告里的「导航可用」才算数。
        ShellAudit.Result legacy = CheckLegacyExpanderShell();
        bool legacyCaught = legacy.Problems.Count > 0;
        if (legacyCaught)
        {
            passed++;
        }

        Console.WriteLine(
            "SELFTEST|shell-legacy-expander|" + (legacyCaught ? "hit" : "MISS")
            + "|problems=" + legacy.Problems.Count.ToString()
            + "|minButtonH=" + legacy.MinButtonHeight.ToString("F1"));

        ShellAudit.Result current = CheckCollapsibleShell();
        bool currentClean = current.Problems.Count == 0 && current.Buttons > 0;
        if (currentClean)
        {
            passed++;
        }

        Console.WriteLine(
            "SELFTEST|shell-collapsible-section|" + (currentClean ? "hit" : "MISS")
            + "|problems=" + current.Problems.Count.ToString()
            + "|minButtonH=" + current.MinButtonHeight.ToString("F1"));

        // 门槛 34 不是空转：把同样的分组换成 30px 高的按钮，必须判红。
        ShellAudit.Result shortShell = CheckShortButtonShell();
        bool shortCaught = shortShell.Problems.Count > 0;
        if (shortCaught)
        {
            passed++;
        }

        Console.WriteLine(
            "SELFTEST|shell-min-height-34|" + (shortCaught ? "hit" : "MISS")
            + "|problems=" + shortShell.Problems.Count.ToString()
            + "|minButtonH=" + shortShell.MinButtonHeight.ToString("F1"));

        // 规则 5 的口径：显式 TabIndex 说了算，树序与视觉顺序相反但 TabIndex 正确时必须判绿。
        (bool tabIndexHit, int tabIndexIssues) = CheckTabIndexOrder();
        if (tabIndexHit)
        {
            passed++;
        }

        Console.WriteLine(
            "SELFTEST|tab-order-tabindex|" + (tabIndexHit ? "hit" : "MISS")
            + "|issues=" + tabIndexIssues.ToString());

        int total = expectations.Length + 7;
        Console.WriteLine("SELFTEST RESULT passed=" + passed + "/" + total);
        return passed == total ? 0 : 1;
    }

    /// <summary>旧实现：导航分组是 Expander，放在 ScrollViewer → StackPanel 里（真实 MainWindow 当年的形态）。</summary>
    private static ShellAudit.Result CheckLegacyExpanderShell()
    {
        var inner = new StackPanel();
        var navButtons = new List<System.Windows.Controls.Button>();
        for (int i = 0; i < 3; i++)
        {
            var button = new System.Windows.Controls.Button
            {
                Content = "功能 " + i.ToString(),
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            navButtons.Add(button);
            inner.Children.Add(button);
        }

        var expander = new Expander { Header = "WORD", Content = inner, IsExpanded = true };
        var stack = new StackPanel();
        stack.Children.Add(expander);
        var host = new Border { Background = Brushes.White, Child = new ScrollViewer { Content = stack } };

        void Relayout()
        {
            host.Measure(new Size(296, 600));
            host.Arrange(new Rect(0, 0, 296, 600));
            host.UpdateLayout();
        }

        Relayout();

        var groups = new[]
        {
            new ShellAudit.NavGroup(
                "WORD(legacy)",
                value => expander.IsExpanded = value,
                () => navButtons.ToList())
        };

        return ShellAudit.Inspect(groups, Relayout);
    }

    /// <summary>现实现：同一内容套进 CollapsibleSection，应该判绿。</summary>
    private static ShellAudit.Result CheckCollapsibleShell()
    {
        var section = new DocumentTools.App.Shell.CollapsibleSection { Title = "WORD" };
        var navButtons = new List<System.Windows.Controls.Button>();
        for (int i = 0; i < 3; i++)
        {
            var button = new Wpf.Ui.Controls.Button
            {
                Content = "功能 " + i.ToString(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                // 与 MainWindow.CreateNavButton 用同一组标记：自检如果比生产宽松，判绿就没有意义
                MinHeight = DocumentTools.App.Ui.UiTokens.RowMinHeight,
                Padding = DocumentTools.App.Ui.UiTokens.NavRowPadding
            };
            navButtons.Add(button);
            section.Body.Children.Add(button);
        }

        var stack = new StackPanel();
        stack.Children.Add(section);
        var host = new Border { Background = Brushes.White, Child = new ScrollViewer { Content = stack } };

        void Relayout()
        {
            host.Measure(new Size(296, 600));
            host.Arrange(new Rect(0, 0, 296, 600));
            host.UpdateLayout();
        }

        Relayout();

        var groups = new[]
        {
            new ShellAudit.NavGroup(
                "WORD",
                value => section.IsExpanded = value,
                () => navButtons.ToList())
        };

        return ShellAudit.Inspect(groups, Relayout);
    }

    /// <summary>门槛 34 的反例：同样的分组、30px 高的导航项，必须被判红。</summary>
    private static ShellAudit.Result CheckShortButtonShell()
    {
        var section = new DocumentTools.App.Shell.CollapsibleSection { Title = "WORD" };
        var navButtons = new List<System.Windows.Controls.Button>();
        for (int i = 0; i < 3; i++)
        {
            var button = new Wpf.Ui.Controls.Button
            {
                Content = "功能 " + i.ToString(),
                Height = 30,
                MinHeight = 0,
                Padding = new Thickness(10, 2, 10, 2),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            navButtons.Add(button);
            section.Body.Children.Add(button);
        }

        var stack = new StackPanel();
        stack.Children.Add(section);
        var host = new Border { Background = Brushes.White, Child = new ScrollViewer { Content = stack } };

        void Relayout()
        {
            host.Measure(new Size(296, 600));
            host.Arrange(new Rect(0, 0, 296, 600));
            host.UpdateLayout();
        }

        Relayout();

        var groups = new[]
        {
            new ShellAudit.NavGroup("WORD(short)", value => section.IsExpanded = value, () => navButtons.ToList())
        };

        return ShellAudit.Inspect(groups, Relayout);
    }

    /// <summary>
    /// 规则 5 的反向自证：树序与视觉顺序相反、但 TabIndex 显式声明了正确顺序时，必须判绿。
    /// 没有这条，规则 5 就只是在数「DockPanel 先挂底部操作条」，改完 TabIndex 口径也看不出区别。
    /// </summary>
    private static (bool Hit, int Issues) CheckTabIndexOrder()
    {
        var canvas = new Canvas();
        var first = new System.Windows.Controls.Button
        {
            Content = "先入树，视觉在下",
            TabIndex = 100,
            Width = 140,
            Height = 30
        };
        Canvas.SetLeft(first, 10);
        Canvas.SetTop(first, 200);
        canvas.Children.Add(first);

        var second = new System.Windows.Controls.Button
        {
            Content = "后入树，视觉在上",
            TabIndex = 0,
            Width = 140,
            Height = 30
        };
        Canvas.SetLeft(second, 10);
        Canvas.SetTop(second, 150);
        canvas.Children.Add(second);

        var host = new Border { Background = Brushes.White, Child = canvas };
        host.Measure(new Size(SnapshotRunner.PageWidth, SnapshotRunner.PageHeight));
        host.Arrange(new Rect(0, 0, SnapshotRunner.PageWidth, SnapshotRunner.PageHeight));
        host.UpdateLayout();
        var feature = new FeatureDescriptor("selftest-tabindex", "TabIndex 自检", FeatureCategory.Tools, "合成页面");
        var snapshot = new PageSnapshot(0, feature, "selftest-tabindex.png");
        VisualAudit.Run(host, SnapshotRunner.PageWidth, SnapshotRunner.PageHeight, snapshot);

        int tabIssues = snapshot.Issues.Count(issue => issue.Kind == "Tab 顺序倒退");

        // 顺带用机器事实记录「TabIndex 不是继承属性」：容器上设值不会传给子元素，
        // 所以把 TabIndex 打在 DockPanel 的 actionBar 上并不能改变键盘序（src 侧修复必须打在停靠点自身）。
        bool inherits = System.Windows.Input.KeyboardNavigation.TabIndexProperty
            .GetMetadata(typeof(DependencyObject)) is FrameworkPropertyMetadata frameworkMetadata
            && frameworkMetadata.Inherits;
        var container = new StackPanel();
        var child = new System.Windows.Controls.Button { Content = "子停靠点" };
        container.Children.Add(child);
        System.Windows.Input.KeyboardNavigation.SetTabIndex(container, 7);
        int childIndex = System.Windows.Input.KeyboardNavigation.GetTabIndex(child);
        bool containerPropagates = childIndex != int.MaxValue;
        Console.WriteLine(
            "SELFTEST|tabindex-inherits|" + (inherits ? "true" : "false")
            + "|container-propagates=" + (containerPropagates ? "true" : "false")
            + "|child-index=" + (containerPropagates ? childIndex.ToString() : "未设置"));

        return (tabIssues == 0 && !containerPropagates, tabIssues);
    }

    /// <summary>路径框一行：Wpf.Ui 的 TextBox（占位含「路径」）+「选择…」按钮。</summary>
    private static StackPanel BuildPathRow(bool isReadOnly, double width)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
        row.Children.Add(new Wpf.Ui.Controls.TextBox
        {
            Name = isReadOnly ? "pathReadOnly" : "pathEditable",
            Width = width,
            Height = 24,
            IsReadOnly = isReadOnly,
            PlaceholderText = "考评表图片路径"
        });
        row.Children.Add(new Button { Content = "选择…", Width = 80, Height = 24 });
        return row;
    }

    /// <summary>同卡片两行字段，第二行标签多 12px 左边距 → 左边界不一致。</summary>
    private static Border BuildMisalignedCard()
    {
        var stack = new StackPanel();
        stack.Children.Add(BuildFieldRow(0));
        stack.Children.Add(BuildFieldRow(12));
        return new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Gray,
            Padding = new Thickness(4),
            Margin = new Thickness(0, 6, 0, 6),
            Child = stack
        };
    }

    private static StackPanel BuildFieldRow(double leftMargin)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
        row.Children.Add(new TextBlock { Text = "字段", Width = 70, Margin = new Thickness(leftMargin, 0, 0, 0) });
        row.Children.Add(new TextBox { Width = 160, Height = 24 });
        return row;
    }

    /// <summary>Tab 顺序陷阱：左列两个按钮纵向排列，右列一个按钮回到第一行高度。</summary>
    private static Grid BuildTabOrderTrap()
    {
        var grid = new Grid { Height = 80, Margin = new Thickness(0, 6, 0, 6) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

        var left = new StackPanel { VerticalAlignment = VerticalAlignment.Top };
        left.Children.Add(new Button { Content = "第一行", Height = 28, HorizontalAlignment = HorizontalAlignment.Stretch });
        left.Children.Add(new Button { Content = "第二行", Height = 28, HorizontalAlignment = HorizontalAlignment.Stretch });
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        var right = new Button
        {
            Content = "右侧第一行",
            Height = 28,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);

        return grid;
    }
}
