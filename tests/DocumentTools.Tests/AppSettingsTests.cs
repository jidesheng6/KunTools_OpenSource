using DocumentTools.Core.Settings;

namespace DocumentTools.Tests;

/// <summary>
/// 设置持久化测试。存储后端用内存实现（<see cref="MemorySettingsStore"/>），
/// 不读写 HKCU 与 %LOCALAPPDATA% 的真实配置；仅导出/导入与原子写用临时目录。
/// </summary>
public sealed class AppSettingsTests : IDisposable
{
    private readonly string _tempDir;

    public AppSettingsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DocumentTools.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 临时目录清理失败不影响测试结论
        }
    }

    [Fact]
    public void MissingKeys_UseDefaults_WithoutWarnings()
    {
        var store = new MemorySettingsStore("""{"StartFeatureId":"exam.insert"}""");
        var service = CreateService(store);

        Assert.Equal("exam.insert", service.Current.StartFeatureId);
        Assert.Equal(AppSettings.DefaultLineHeight, service.Current.ListViewLineHeight);
        Assert.True(service.Current.ShowOpenTargetDialog);
        Assert.True(service.Current.ListViewShowGridLines);
        Assert.True(service.Current.HighlightActiveNav);
        Assert.Equal(string.Empty, service.Current.ChromeDriverPath);
        Assert.Equal(string.Empty, service.Current.VfsoftExamDataPath);
        Assert.Equal(string.Empty, service.Current.VfsoftMssDataPath);
        Assert.Empty(service.LastLoadWarnings);
        Assert.True(service.Reload().Succeeded);
    }

    [Fact]
    public void EmptyStore_FallsBackToSeed_AndReportsUnreadable()
    {
        var store = new MemorySettingsStore(initial: null);
        var service = CreateService(store);

        Assert.Equal(AppSettings.DefaultStartFeatureId, service.Current.StartFeatureId);
        Assert.NotEmpty(service.LastLoadWarnings);
        Assert.False(service.Reload().Succeeded);
    }

    [Fact]
    public void BadValues_AreCoerced_AndRecordedInLastLoadWarnings()
    {
        const string json = """
            {
              "SettingsVersion": 99,
              "StartFeatureId": "   ",
              "ShowOpenTargetDialog": "maybe",
              "ListViewLineHeight": 9999,
              "HighlightActiveNav": "off",
              "VfsoftExamDataPath": "C:\\bad|path",
              "HideFeatureIds": " exam.insert , exam.mss ;; exam.insert "
            }
            """;

        var service = CreateService(new MemorySettingsStore(json));

        Assert.Equal(AppSettings.CurrentVersion, service.Current.SettingsVersion);
        Assert.Equal(AppSettings.DefaultStartFeatureId, service.Current.StartFeatureId);
        Assert.True(service.Current.ShowOpenTargetDialog); // 非法布尔 → 默认值
        Assert.Equal(AppSettings.MaxLineHeight, service.Current.ListViewLineHeight); // clamp 到上界
        Assert.False(service.Current.HighlightActiveNav); // on/off 容错
        Assert.Equal(string.Empty, service.Current.VfsoftExamDataPath); // 非法字符 → 清空
        Assert.Equal("exam.insert,exam.mss", service.Current.HideFeatureIds);

        Assert.True(service.LastLoadWarnings.Count >= 4, string.Join(" | ", service.LastLoadWarnings));
        Assert.Contains(service.LastLoadWarnings, w => w.Contains("SettingsVersion", StringComparison.Ordinal));
        Assert.Contains(service.LastLoadWarnings, w => w.Contains("ShowOpenTargetDialog", StringComparison.Ordinal));
        Assert.Contains(service.LastLoadWarnings, w => w.Contains("ListViewLineHeight", StringComparison.Ordinal));
        Assert.Contains(service.LastLoadWarnings, w => w.Contains("VfsoftExamDataPath", StringComparison.Ordinal));

        SettingsLoadResult reload = service.Reload();
        Assert.True(reload.Succeeded);
        Assert.True(reload.HasWarnings);
    }

    [Fact]
    public void BrokenJson_FallsBackToDefaults_WithoutThrowing()
    {
        var service = CreateService(new MemorySettingsStore("{ this is not json"));

        Assert.Equal(AppSettings.DefaultStartFeatureId, service.Current.StartFeatureId);
        SettingsLoadResult result = service.Reload();
        Assert.False(result.Succeeded);
        Assert.Contains(result.Warnings, w => w.Contains("JSON", StringComparison.Ordinal));
    }

    [Fact]
    public void UnknownKeys_AreIgnored_WithWarning()
    {
        var service = CreateService(new MemorySettingsStore("""{"TabPageHeighLight":"on","NotASetting":42}"""));

        Assert.Equal(AppSettings.DefaultStartFeatureId, service.Current.StartFeatureId);
        Assert.Contains(service.LastLoadWarnings, w => w.Contains("未知字段", StringComparison.Ordinal));
    }

    [Fact]
    public void ExportThenImport_RoundTripsEveryField()
    {
        var source = CreateService(new MemorySettingsStore("{}"));
        source.Current.StartFeatureId = "exam.mss3";
        source.Current.ShowOpenTargetDialog = false;
        source.Current.ListViewShowGridLines = false;
        source.Current.ListViewLineHeight = 32;
        source.Current.HighlightActiveNav = false;
        source.Current.ChromeDriverPath = @"C:\tools\chromedriver.exe";
        source.Current.VfsoftExamDataPath = @"D:\data\exam";
        source.Current.VfsoftMssDataPath = @"D:\data\mss";
        source.Current.HideFeatureIds = "exam.insert,exam.mss3";

        string file = Path.Combine(_tempDir, "export", "settings-export.json");
        SettingsExportResult export = source.ExportTo(file);
        Assert.True(export.Succeeded, export.Message);
        Assert.True(File.Exists(file));

        string json = File.ReadAllText(file);
        Assert.StartsWith("{", json);
        Assert.Contains("\"SettingsVersion\": 1", json); // 格式化输出
        Assert.True(
            json.IndexOf("StartFeatureId", StringComparison.Ordinal) > json.IndexOf("SettingsVersion", StringComparison.Ordinal),
            "导出字段顺序应与 schema 一致（SettingsVersion 在前）");

        var target = CreateService(new MemorySettingsStore("{}"));
        SettingsImportResult import = target.ImportFrom(file);

        Assert.True(import.Succeeded, import.Message);
        Assert.DoesNotContain(import.Fields, f => f.IsProblem);
        Assert.Equal(source.Current.StartFeatureId, target.Current.StartFeatureId);
        Assert.Equal(source.Current.ShowOpenTargetDialog, target.Current.ShowOpenTargetDialog);
        Assert.Equal(source.Current.ListViewShowGridLines, target.Current.ListViewShowGridLines);
        Assert.Equal(source.Current.ListViewLineHeight, target.Current.ListViewLineHeight);
        Assert.Equal(source.Current.HighlightActiveNav, target.Current.HighlightActiveNav);
        Assert.Equal(source.Current.ChromeDriverPath, target.Current.ChromeDriverPath);
        Assert.Equal(source.Current.VfsoftExamDataPath, target.Current.VfsoftExamDataPath);
        Assert.Equal(source.Current.VfsoftMssDataPath, target.Current.VfsoftMssDataPath);
        Assert.Equal(source.Current.HideFeatureIds, target.Current.HideFeatureIds);
    }

    [Fact]
    public void Import_InvalidValues_ReportsPerField_AndKeepsCurrentSettings()
    {
        var store = new MemorySettingsStore("""{"StartFeatureId":"home"}""");
        var service = CreateService(store);
        service.Current.StartFeatureId = "exam.insert";

        SettingsImportResult result = service.ImportJson("""
            {
              "StartFeatureId": "exam.mss",
              "ListViewLineHeight": "abc",
              "VfsoftExamDataPath": "relative\\exam",
              "ShowOpenTargetDialog": { "nested": true }
            }
            """);

        Assert.False(result.Succeeded);
        Assert.Equal(3, result.RejectedFields.Count);
        Assert.Contains(result.RejectedFields, f => f.Field == "ListViewLineHeight");
        Assert.Contains(result.RejectedFields, f => f.Field == "VfsoftExamDataPath");
        Assert.Contains(result.RejectedFields, f => f.Field == "ShowOpenTargetDialog");
        Assert.Contains(result.Fields, f => f.Field == "StartFeatureId" && f.Outcome == SettingsImportFieldOutcome.Applied);

        // 全有或全无：内存设置与存储文件都不应被改动
        Assert.Equal("exam.insert", service.Current.StartFeatureId);
        Assert.Equal(0, store.WriteCount);
        Assert.Contains("未改变", result.Message);
    }

    [Fact]
    public void Import_MissingFields_UseDefaults_AndWritesStore()
    {
        var store = new MemorySettingsStore("{}");
        var service = CreateService(store);

        SettingsImportResult result = service.ImportJson("""{"ListViewLineHeight":40}""");

        Assert.True(result.Succeeded, result.Message);
        Assert.Contains(result.Fields, f => f.Field == "ListViewLineHeight" && f.Outcome == SettingsImportFieldOutcome.Applied);
        Assert.Contains(result.Fields, f => f.Field == "StartFeatureId" && f.Outcome == SettingsImportFieldOutcome.Defaulted);
        Assert.Equal(40, service.Current.ListViewLineHeight);
        Assert.Equal(AppSettings.DefaultStartFeatureId, service.Current.StartFeatureId);
        Assert.Equal(1, store.WriteCount);
        Assert.Contains("ListViewLineHeight", store.Content ?? string.Empty);
    }

    [Fact]
    public void Import_UnknownKeys_AreIgnored_NotFailed()
    {
        var service = CreateService(new MemorySettingsStore("{}"));

        SettingsImportResult result = service.ImportJson("""{"StartFeatureId":"home","LegacyNoise":"on"}""");

        Assert.True(result.Succeeded, result.Message);
        Assert.Contains(result.Fields, f => f.Field == "LegacyNoise" && f.Outcome == SettingsImportFieldOutcome.Ignored);
    }

    [Fact]
    public void Import_MissingFile_ReturnsFailureMessage()
    {
        var service = CreateService(new MemorySettingsStore("{}"));

        SettingsImportResult result = service.ImportFrom(Path.Combine(_tempDir, "nope.json"));

        Assert.False(result.Succeeded);
        Assert.Contains("不存在", result.Message);
    }

    [Fact]
    public void Save_ReportsFailure_InsteadOfThrowing()
    {
        var store = new MemorySettingsStore("{}") { FailureOnWrite = true, WriteError = "目标磁盘只读，无法写入" };
        var service = CreateService(store);

        SettingsSaveResult result = service.Save();

        Assert.False(result.Succeeded);
        Assert.Contains("只读", result.Message);
    }

    [Fact]
    public void Save_Success_RaisesChangedWithOnlyActuallyChangedFields()
    {
        var service = CreateService(new MemorySettingsStore("{}"));
        var events = new List<SettingsChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Current.HideFeatureIds = "word.pdf,pdf.word";
        service.Current.HighlightActiveNav = false;
        service.Current.ListViewLineHeight = 36;

        SettingsSaveResult saved = service.Save();

        Assert.True(saved.Succeeded, saved.Message);
        SettingsChangedEventArgs changed = Assert.Single(events);
        Assert.True(changed.Contains("HideFeatureIds"));
        Assert.True(changed.Contains("HighlightActiveNav"));
        Assert.True(changed.Contains("ListViewLineHeight"));
        Assert.False(changed.Contains("ListViewShowGridLines"));

        // 同一份值重复保存不应让 Shell 重建导航或重复刷新缓存页。
        Assert.True(service.Save().Succeeded);
        Assert.Single(events);
    }

    [Fact]
    public void Save_Failure_DoesNotPublishUiChange_AndRetryPublishesIt()
    {
        var store = new MemorySettingsStore("{}") { FailureOnWrite = true };
        var service = CreateService(store);
        var events = new List<SettingsChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);
        service.Current.ListViewShowGridLines = false;

        Assert.False(service.Save().Succeeded);
        Assert.Empty(events);

        store.FailureOnWrite = false;
        Assert.True(service.Save().Succeeded);
        Assert.True(Assert.Single(events).Contains("ListViewShowGridLines"));
    }

    [Fact]
    public void Reload_PublishesRollbackOfUnsavedRuntimeValues()
    {
        var service = CreateService(new MemorySettingsStore("""{"ListViewLineHeight":31}"""));
        var events = new List<SettingsChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);
        service.Current.ListViewLineHeight = 55;

        SettingsLoadResult loaded = service.Reload();

        Assert.True(loaded.Succeeded, loaded.Message);
        Assert.Equal(31, service.Current.ListViewLineHeight);
        Assert.True(Assert.Single(events).Contains("ListViewLineHeight"));
    }

    [Fact]
    public void Import_Success_PublishesChangedFields()
    {
        var service = CreateService(new MemorySettingsStore("{}"));
        SettingsChangedEventArgs? observed = null;
        service.Changed += (_, e) => observed = e;

        SettingsImportResult imported = service.ImportJson(
            """{"HideFeatureIds":"exam.report","ListViewLineHeight":42}""");

        Assert.True(imported.Succeeded, imported.Message);
        Assert.NotNull(observed);
        Assert.True(observed.Contains("HideFeatureIds"));
        Assert.True(observed.Contains("ListViewLineHeight"));
    }

    [Fact]
    public void JsonFileStore_ReadOnlyFile_ReturnsFailureMessage()
    {
        string path = Path.Combine(_tempDir, "readonly.json");
        File.WriteAllText(path, "{}");
        var store = new JsonFileSettingsStore(path);
        File.SetAttributes(path, FileAttributes.ReadOnly);

        try
        {
            Assert.False(store.TryWrite("{}", out string error));
            Assert.Contains("只读", error);
        }
        finally
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }
    }

    [Fact]
    public void JsonFileStore_WritesAtomically_AndLeavesNoTempFile()
    {
        string path = Path.Combine(_tempDir, "nested", "settings.json");
        var store = new JsonFileSettingsStore(path);

        Assert.False(store.Exists);
        Assert.True(store.TryWrite("""{"SettingsVersion":1}""", out string error), error);
        Assert.True(store.Exists);
        Assert.False(File.Exists(path + ".tmp"));

        Assert.True(store.TryRead(out string content, out string readError), readError);
        Assert.Contains("SettingsVersion", content);
    }

    [Fact]
    public void Validation_ReportsDirectoryAndFileState_WithoutThrowing()
    {
        string existingDir = Path.Combine(_tempDir, "exam-data");
        Directory.CreateDirectory(existingDir);
        string existingFile = Path.Combine(existingDir, "chromedriver.exe");
        File.WriteAllText(existingFile, "stub");

        Assert.Equal(SettingsValueState.Ok, AppSettingsValidator.CheckDirectory("VfsoftExamDataPath", existingDir).State);
        Assert.Equal(SettingsValueState.Missing, AppSettingsValidator.CheckDirectory("VfsoftExamDataPath", Path.Combine(_tempDir, "gone")).State);
        Assert.Equal(SettingsValueState.Invalid, AppSettingsValidator.CheckDirectory("VfsoftExamDataPath", existingFile).State);
        Assert.Equal(SettingsValueState.Invalid, AppSettingsValidator.CheckDirectory("VfsoftExamDataPath", @"relative\dir").State);
        Assert.Equal(SettingsValueState.NotSet, AppSettingsValidator.CheckDirectory("VfsoftExamDataPath", "  ").State);
        Assert.Equal(SettingsValueState.Ok, AppSettingsValidator.CheckFile("ChromeDriverPath", existingFile).State);
        Assert.Equal(SettingsValueState.Invalid, AppSettingsValidator.CheckFile("ChromeDriverPath", existingDir).State);

        Assert.True(AppSettingsValidator.IsDirectoryReady(existingDir));
        Assert.False(AppSettingsValidator.IsDirectoryReady(existingFile));
        Assert.False(AppSettingsValidator.IsDirectoryReady(null));

        var settings = new AppSettings { VfsoftExamDataPath = existingDir, ChromeDriverPath = existingFile };
        SettingsValidationReport report = AppSettingsValidator.Validate(settings);
        Assert.Contains(report.Fields, f => f.Field == "VfsoftExamDataPath" && f.State == SettingsValueState.Ok);
        Assert.Contains(report.Problems, f => f.Field == "VfsoftMssDataPath"); // 未设置 → 需要关注
        Assert.False(string.IsNullOrWhiteSpace(report.Summary));

        var service = CreateService(new MemorySettingsStore("{}"));
        SettingsValidationReport serviceReport = service.Validate();
        Assert.Equal(report.Fields.Count, serviceReport.Fields.Count);
    }

    private static AppSettingsService CreateService(MemorySettingsStore store)
        => new(store, new AppSettings());

    /// <summary>内存存储：不碰真实文件，可模拟读写失败。</summary>
    private sealed class MemorySettingsStore : ISettingsStore
    {
        public MemorySettingsStore(string? initial)
        {
            Content = initial;
        }

        public string? Content { get; set; }

        public bool FailureOnRead { get; set; }

        public string ReadError { get; set; } = "读取失败（测试桩）";

        public bool FailureOnWrite { get; set; }

        public string WriteError { get; set; } = "写入失败（测试桩）";

        public int WriteCount { get; private set; }

        public string Location => "(memory)";

        public bool Exists => Content is not null;

        public bool TryRead(out string content, out string error)
        {
            if (FailureOnRead || Content is null)
            {
                content = string.Empty;
                error = FailureOnRead ? ReadError : "配置文件不存在：(memory)";
                return false;
            }

            content = Content;
            error = string.Empty;
            return true;
        }

        public bool TryWrite(string content, out string error)
        {
            WriteCount++;
            if (FailureOnWrite)
            {
                error = WriteError;
                return false;
            }

            Content = content;
            error = string.Empty;
            return true;
        }
    }
}
