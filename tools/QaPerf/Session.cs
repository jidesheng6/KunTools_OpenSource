using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentTools.QaPerf;

/// <summary>Everything one process invocation measured. Persisted so the report can merge runs.</summary>
internal sealed class Session
{
    public string Id { get; set; } = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);

    public DateTimeOffset StartedAt { get; set; }

    public string CommandLine { get; set; } = string.Empty;

    public string ScenarioRequest { get; set; } = "all";

    public int RequestedRounds { get; set; } = 1;

    public HostInfo Host { get; set; } = new();

    public bool Licensed { get; set; }

    public string? LicensePath { get; set; }

    public string AsposeFacts { get; set; } = string.Empty;

    public FixtureSummary Fixtures { get; set; } = new();

    public List<RunMetrics> Runs { get; set; } = new();

    public List<ScenarioVerification> Verifications { get; set; } = new();

    public List<string> VerificationNotes { get; set; } = new();

    public List<LeakFinding> Leaks { get; set; } = new();

    public List<DisposeSite> DisposeSites { get; set; } = new();

    [JsonIgnore]
    public string FileName => Id + ".json";

    [JsonIgnore]
    public IEnumerable<string> ScenarioIds => Runs.Select(run => run.ScenarioId).Distinct(StringComparer.Ordinal);
}

internal static class SessionStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static void Save(RepoLayout layout, Session session)
    {
        Directory.CreateDirectory(layout.SessionsRoot);
        string path = Path.Combine(layout.SessionsRoot, session.FileName);
        File.WriteAllText(path, JsonSerializer.Serialize(session, SerializerOptions), new UTF8Encoding(false));
    }

    public static IReadOnlyList<Session> LoadAll(RepoLayout layout)
    {
        if (!Directory.Exists(layout.SessionsRoot))
        {
            return Array.Empty<Session>();
        }

        var sessions = new List<Session>();
        foreach (string path in Directory.GetFiles(layout.SessionsRoot, "*.json"))
        {
            try
            {
                Session? session = JsonSerializer.Deserialize<Session>(File.ReadAllText(path), SerializerOptions);
                if (session is not null)
                {
                    sessions.Add(session);
                }
            }
            catch (Exception exception) when (exception is JsonException or IOException)
            {
                Console.Error.WriteLine("skip unreadable session " + Path.GetFileName(path) + ": " + exception.Message);
            }
        }

        return sessions.OrderBy(session => session.StartedAt).ToList();
    }

    public static void Reset(RepoLayout layout)
    {
        if (!Directory.Exists(layout.SessionsRoot))
        {
            return;
        }

        foreach (string path in Directory.GetFiles(layout.SessionsRoot, "*.json"))
        {
            File.Delete(path);
        }
    }
}
