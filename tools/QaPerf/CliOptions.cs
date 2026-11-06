using System.Globalization;

namespace DocumentTools.QaPerf;

/// <summary>
/// Command line contract:
/// <c>dotnet run --project tools\QaPerf -- --scenario all|wordpdf|image|large|merge --rounds 1|5 --out &lt;dir&gt;</c>.
/// A scenario id (for example <c>wordpdf-50</c>) is also accepted so a single scenario can be
/// measured in a cold process without paying for the others.
/// </summary>
internal sealed class CliOptions
{
    public const string Usage = """
        QaPerf - batch conversion / memory baseline for DocumentTools.Core

          dotnet run --project tools\QaPerf -- [options]

        Options:
          --scenario <all|wordpdf|image|large|merge|scenario-id>   default: all
          --rounds   <n>                                          default: 1  (use 5 for leak sniffing)
          --out      <dir>                                        default: artifacts\qa-perf
          --report   <path>                                        default: docs\qa-report-perf.md
          --probe    <file>                                        print page/paragraph facts for one file and exit
          --report-only                                           rebuild the report from recorded sessions and exit
          --reset                                                 drop recorded sessions before running
          --help

        Exit codes: 0 = every scenario produced all outputs, 1 = at least one scenario failed,
                    2 = bad arguments or missing fixtures.
        """;

    public string Scenario { get; private init; } = "all";

    public int Rounds { get; private init; } = 1;

    public string? OutDirectory { get; private init; }

    public string? ReportPath { get; private init; }

    public string? ProbePath { get; private init; }

    public bool ReportOnly { get; private init; }

    public bool ResetSessions { get; private init; }

    public bool ShowHelp { get; private init; }

    public static CliOptions Parse(string[] args)
    {
        string scenario = "all";
        int rounds = 1;
        string? outDirectory = null;
        string? reportPath = null;
        string? probePath = null;
        bool reportOnly = false;
        bool reset = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg is "-h" or "--help" or "/?")
            {
                return new CliOptions { ShowHelp = true };
            }

            (string name, string? inlineValue) = SplitInline(arg);
            switch (name)
            {
                case "--scenario":
                    scenario = inlineValue ?? NextValue(args, ref i, name);
                    break;
                case "--rounds":
                    string roundsText = inlineValue ?? NextValue(args, ref i, name);
                    rounds = int.TryParse(roundsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                        ? parsed
                        : throw new FormatException($"--rounds 需要整数，收到 \"{roundsText}\"。");
                    if (rounds is < 1 or > 50)
                    {
                        throw new FormatException("--rounds 取值范围 1..50。");
                    }

                    break;
                case "--out":
                    outDirectory = inlineValue ?? NextValue(args, ref i, name);
                    break;
                case "--report":
                    reportPath = inlineValue ?? NextValue(args, ref i, name);
                    break;
                case "--probe":
                    probePath = inlineValue ?? NextValue(args, ref i, name);
                    break;
                case "--report-only":
                    reportOnly = true;
                    break;
                case "--reset":
                    reset = true;
                    break;
                default:
                    throw new FormatException($"未知参数 \"{arg}\"。");
            }
        }

        return new CliOptions
        {
            Scenario = scenario,
            Rounds = rounds,
            OutDirectory = outDirectory,
            ReportPath = reportPath,
            ProbePath = probePath,
            ReportOnly = reportOnly,
            ResetSessions = reset
        };
    }

    private static (string Name, string? Value) SplitInline(string arg)
    {
        int eq = arg.IndexOf('=');
        return eq <= 0 ? (arg, null) : (arg[..eq], arg[(eq + 1)..]);
    }

    private static string NextValue(string[] args, ref int index, string name)
    {
        if (index + 1 >= args.Length)
        {
            throw new FormatException($"{name} 缺少取值。");
        }

        index++;
        return args[index];
    }
}
