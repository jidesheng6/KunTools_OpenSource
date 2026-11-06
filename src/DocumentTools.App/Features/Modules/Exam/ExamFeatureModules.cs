using System.Windows;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.Core.Features;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// Exam suite — each page is a single-DB / template / image workflow (NO batch file ListView shell).
/// Modules only declare metadata; all layout and behaviour live in the per-feature views.
/// </summary>
public static class ExamFeatureModules
{
    public static IEnumerable<IFeatureModule> CreateAll()
    {
        yield return new ExamModule("exam.mss3", (d, s) => new ExamMss3View(d, s));
        yield return new ExamModule("exam.single", (d, s) => new ExamSingleView(d, s));
        yield return new ExamModule("exam.insert", (d, s) => new ExamInsertView(d, s));
        yield return new ExamModule("exam.report", (d, s) => new ExamReportView(d, s));
        yield return new ExamModule("exam.vote", (d, s) => new ExamVoteView(d, s));
        yield return new ExamModule("exam.package", (d, s) => new ExamPackageView(d, s));
        yield return new ExamModule("exam.circle", (d, s) => new ExamCircleView(d, s));
    }

    private sealed class ExamModule : FeatureModuleBase
    {
        private readonly Func<FeatureDescriptor, IServiceProvider, FrameworkElement> _factory;

        public ExamModule(string id, Func<FeatureDescriptor, IServiceProvider, FrameworkElement> factory)
            : base(BuiltInFeatures.All.First(f => f.Id == id))
        {
            _factory = factory;
        }

        public override FrameworkElement CreateView(IServiceProvider services)
            => _factory(Descriptor, services);
    }
}
