using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 三个交互态脚本：在真视图上点真按钮，记录「点击前没有、点击后出现」的状态行文案。
/// 容器里的 <see cref="DocumentTools.App.Infrastructure.Dialogs.IDialogService"/> 已被
/// <see cref="QaDialogService"/> 替换，因此连「开始匹配」「选择图片…」这类原本会弹模态框的按钮
/// 也可以放心点，弹窗调用会作为 trace 里的 dialog 行留证。
/// </summary>
internal static class InteractionScripts
{
    /// <summary>交互态覆盖的页面：转换/考评/工具各一页，另加曾挂起的 exam.circle 作为回归。</summary>
    internal static readonly string[] PageIds = ["word.pdf", "exam.vote", "code.barcode", "exam.circle"];

    /// <summary>在真视图上执行脚本；trace 直接写进调用方集合，便于异常中断时仍保留已执行步骤。</summary>
    internal static void Run(string featureId, FrameworkElement view, List<string> trace, QaDialogService dialogs)
    {
        var script = new ScriptContext(view, trace, dialogs);
        switch (featureId)
        {
            case "word.pdf":
                WordToPdf(script);
                break;

            case "exam.vote":
                Vote(script);
                break;

            case "code.barcode":
                Barcode(script);
                break;

            case "exam.circle":
                Circle(script);
                break;

            default:
                script.Note("MISS script: 没有为 " + featureId + " 定义交互脚本");
                break;
        }
    }

    /// <summary>word.pdf：一个文件都没加就点「开始处理」，应是状态行提示而不是真去转换。</summary>
    private static void WordToPdf(ScriptContext script)
    {
        script.Reset();
        script.Click("开始处理");
    }

    /// <summary>exam.vote：填三个候选人后点「生成报告单」，观察中途状态（模板缺失时给错误态文案）。</summary>
    private static void Vote(ScriptContext script)
    {
        script.Reset();
        script.SetText(
            "候选人姓名，每行一个",
            "张三" + Environment.NewLine + "李四" + Environment.NewLine + "王五");
        script.Click("生成报告单");
    }

    /// <summary>code.barcode：输入内容 → 生成预览 → 断言预览区真的画出了图，再走识别失败与文件对话框。</summary>
    private static void Barcode(ScriptContext script)
    {
        script.Reset();
        script.SetText("条码内容（多行时按行批量生成可另存）", "QA-1234567890");
        script.Click("生成预览");
        script.CheckImages("preview-image");

        script.Click("复制识别内容");
        script.Click("选择图片…");

        script.SetText("要识别的条码图片路径", script.FixedFile);
        script.Reset();
        script.Click("识别条码");
    }

    /// <summary>exam.circle：空态引导是否存在、没载图时点「打开…」与「开始匹配」（都该走弹窗服务，不该弹真窗口）。</summary>
    private static void Circle(ScriptContext script)
    {
        script.Reset();
        script.HasText("尚未载入图片", "empty-hint");
        script.Click("打开…");
        script.Click("开始匹配");
    }

    /// <summary>脚本上下文：收集 trace 行、维护点击前的文本快照、记录弹窗调用。</summary>
    private sealed class ScriptContext
    {
        private readonly FrameworkElement _view;

        private readonly List<string> _trace;

        private readonly QaDialogService _dialogs;

        private HashSet<string> _before = new(StringComparer.Ordinal);

        private int _reportedDialogs;

        internal ScriptContext(FrameworkElement view, List<string> trace, QaDialogService dialogs)
        {
            _view = view;
            _trace = trace;
            _dialogs = dialogs;
        }

        internal void Note(string line) => _trace.Add(line);

        /// <summary>假对话框返回的文件路径，脚本要复用它（避免脚本里硬编码路径）。</summary>
        internal string FixedFile => _dialogs.File;

        internal void Reset() => _before = InteractionKit.CollectTexts(_view);

        /// <summary>断言页面初始文本里含某段文字（用于证明空态引导确实存在）。</summary>
        internal void HasText(string fragment, string name)
        {
            bool found = InteractionKit.CollectTexts(_view)
                .Any(text => text.Contains(fragment, StringComparison.Ordinal));
            _trace.Add((found ? "present " : "MISS ") + name + ": " + fragment);
        }

        /// <summary>断言页面上确实有可见的图（预览区出图是 code.barcode 的成功信号）。</summary>
        internal void CheckImages(string name)
        {
            List<Image> images = InteractionKit.Descendants(_view)
                .OfType<Image>()
                .Where(image => image.Visibility == Visibility.Visible)
                .ToList();

            int drawn = images.Count(image => image.Source is not null && image.ActualWidth >= 8 && image.ActualHeight >= 8);
            _trace.Add(
                (drawn > 0 ? "present " : "MISS ") + name + ": images=" + images.Count
                + " drawn=" + drawn
                + (drawn > 0
                    ? " size=" + images.First(image => image.Source is not null && image.ActualWidth >= 8)
                        .ActualWidth.ToString("0", CultureInfo.InvariantCulture) + "×"
                        + images.First(image => image.Source is not null && image.ActualWidth >= 8)
                        .ActualHeight.ToString("0", CultureInfo.InvariantCulture)
                    : string.Empty));
        }

        internal void SetText(string placeholder, string text)
        {
            TextBox? box = InteractionKit.FindTextBox(_view, placeholder);
            if (box is null)
            {
                _trace.Add("MISS textbox: " + placeholder);
                return;
            }

            box.Text = text;
            _trace.Add("set " + placeholder + " = " + text);
        }

        internal void Click(string label)
        {
            Button? button = InteractionKit.FindButton(_view, label);
            if (button is null)
            {
                _trace.Add("MISS button: " + label);
                return;
            }

            InteractionKit.Click(button);
            _trace.Add("click " + label);

            // 状态行可能是 async void 里的续体更新的，泵一次消息再取差集。
            SnapshotRunner.PumpDispatcher(150);
            ReportDialogs();
            Report(label);
        }

        /// <summary>把这一步里发生的弹窗调用记下来：假实现不会阻塞，所以"有 dialog 行"即为已处理，不是缺陷。</summary>
        private void ReportDialogs()
        {
            for (int i = _reportedDialogs; i < _dialogs.Calls.Count; i++)
            {
                _trace.Add("dialog " + _dialogs.Calls[i]);
            }

            _reportedDialogs = _dialogs.Calls.Count;
        }

        private void Report(string label)
        {
            HashSet<string> after = InteractionKit.CollectTexts(_view);
            List<string> added = after
                .Where(text => !_before.Contains(text))
                .OrderBy(text => text, StringComparer.Ordinal)
                .ToList();

            if (added.Count == 0)
            {
                _trace.Add("nostatus " + label + ": 点击后没有出现新文案");
            }

            foreach (string text in added)
            {
                _trace.Add("status + " + text);
            }

            _before = after;
        }
    }
}
