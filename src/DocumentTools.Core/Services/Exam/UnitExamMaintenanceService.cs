using System.Data;
using System.Security.Cryptography;
using DocumentTools.Core.Data;
using Microsoft.Data.Sqlite;

namespace DocumentTools.Core.Services.Exam;

public enum UnitExamDatabaseKind
{
    Access,
    Sqlite
}

public sealed record UnitExamMaintenanceSnapshot(
    string DatabaseFile,
    UnitExamDatabaseKind Kind,
    IReadOnlyList<string> ComputerNames,
    IReadOnlyList<string> PicturePaths);

public sealed record UnitExamMaintenanceResult(int AffectedRows, string BackupFile);

public interface IUnitExamMaintenanceService
{
    UnitExamMaintenanceSnapshot Inspect(string databaseFile);

    string GenerateRandomComputerName(int randomLength);

    UnitExamMaintenanceResult ChangeComputerName(string databaseFile, string oldName, string newName);

    UnitExamMaintenanceResult ChangePicturePath(string databaseFile, string oldPath, string newPath);
}

/// <summary>
/// Maintains Result_Essinfo in a unit .exam/.mdb/.db file. Every mutation creates a sibling
/// backup first and executes inside a transaction, so a failed write cannot leave half a change.
/// </summary>
public sealed class UnitExamMaintenanceService : IUnitExamMaintenanceService
{
    private const string TableName = "Result_Essinfo";
    private const string RandomAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public UnitExamMaintenanceSnapshot Inspect(string databaseFile)
    {
        string file = ValidateFile(databaseFile, out UnitExamDatabaseKind kind);
        return kind == UnitExamDatabaseKind.Sqlite
            ? InspectSqlite(file)
            : InspectAccess(file);
    }

    public string GenerateRandomComputerName(int randomLength)
    {
        if (randomLength is < 2 or > 30)
        {
            throw new ArgumentOutOfRangeException(nameof(randomLength), "随机字符长度必须在 2–30 之间。");
        }

        string machine = string.IsNullOrWhiteSpace(Environment.MachineName) ? "PC" : Environment.MachineName;
        char[] random = new char[randomLength];
        for (int i = 0; i < random.Length; i++)
        {
            random[i] = RandomAlphabet[RandomNumberGenerator.GetInt32(RandomAlphabet.Length)];
        }

        int insertAt = RandomNumberGenerator.GetInt32(machine.Length + 1);
        string value = machine.Insert(insertAt, new string(random));
        return value.Length > 30 ? value[..30] : value;
    }

    public UnitExamMaintenanceResult ChangeComputerName(string databaseFile, string oldName, string newName)
        => ChangeValue(databaseFile, "CompName", oldName, newName, normalizeDirectory: false);

    public UnitExamMaintenanceResult ChangePicturePath(string databaseFile, string oldPath, string newPath)
        => ChangeValue(databaseFile, "PicPath", oldPath, newPath, normalizeDirectory: true);

    private static UnitExamMaintenanceSnapshot InspectAccess(string file)
    {
        using var database = new AccessDb(file);
        DataTable table = database.Query($"SELECT CompName, PicPath FROM {TableName}");
        return CreateSnapshot(file, UnitExamDatabaseKind.Access, table.Rows.Cast<DataRow>()
            .Select(row => (Text(row["CompName"]), Text(row["PicPath"]))));
    }

    private static UnitExamMaintenanceSnapshot InspectSqlite(string file)
    {
        using var connection = OpenSqlite(file);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT CompName, PicPath FROM {TableName}";
        using SqliteDataReader reader = command.ExecuteReader();
        var values = new List<(string Computer, string Picture)>();
        while (reader.Read())
        {
            values.Add((reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                reader.IsDBNull(1) ? string.Empty : reader.GetString(1)));
        }

        return CreateSnapshot(file, UnitExamDatabaseKind.Sqlite, values);
    }

    private static UnitExamMaintenanceSnapshot CreateSnapshot(
        string file,
        UnitExamDatabaseKind kind,
        IEnumerable<(string Computer, string Picture)> values)
    {
        var computers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pictures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach ((string computer, string picture) in values)
        {
            if (!string.IsNullOrWhiteSpace(computer))
            {
                computers.Add(computer.Trim());
            }

            if (!string.IsNullOrWhiteSpace(picture))
            {
                pictures.Add(picture.Trim());
            }
        }

        return new UnitExamMaintenanceSnapshot(
            file,
            kind,
            computers.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase).ToList(),
            pictures.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static UnitExamMaintenanceResult ChangeValue(
        string databaseFile,
        string column,
        string oldValue,
        string newValue,
        bool normalizeDirectory)
    {
        string file = ValidateFile(databaseFile, out UnitExamDatabaseKind kind);
        oldValue = RequireValue(oldValue, "请选择要修改的原值。");
        newValue = RequireValue(newValue, "新值不能为空。");
        if (normalizeDirectory)
        {
            if (!Directory.Exists(newValue))
            {
                throw new DirectoryNotFoundException("新的考评图片目录不存在：" + newValue);
            }

            newValue = Path.GetFullPath(newValue).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        }

        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("新值与原值相同，无需修改。");
        }

        return kind == UnitExamDatabaseKind.Sqlite
            ? ChangeSqlite(file, column, oldValue, newValue)
            : ChangeAccess(file, column, oldValue, newValue);
    }

    private static UnitExamMaintenanceResult ChangeAccess(string file, string column, string oldValue, string newValue)
    {
        string backup = CreateBackupPath(file);
        File.Copy(file, backup, overwrite: false);
        try
        {
            using var database = new AccessDb(file);
            database.BeginTransaction();
            int affected = database.Execute(
                $"UPDATE {TableName} SET [{column}]=? WHERE [{column}]=?",
                ("@new", newValue),
                ("@old", oldValue));
            if (affected == 0)
            {
                database.Rollback();
                throw new InvalidOperationException("数据库中没有找到要修改的原值，未写入任何内容。");
            }

            database.Commit();
            return new UnitExamMaintenanceResult(affected, backup);
        }
        catch
        {
            DeleteFailedBackup(backup);
            throw;
        }
    }

    private static UnitExamMaintenanceResult ChangeSqlite(string file, string column, string oldValue, string newValue)
    {
        string backup = CreateBackupPath(file);
        using SqliteConnection connection = OpenSqlite(file);
        using (var backupConnection = new SqliteConnection($"Data Source={backup};Mode=ReadWriteCreate"))
        {
            backupConnection.Open();
            connection.BackupDatabase(backupConnection);
        }

        using SqliteTransaction transaction = connection.BeginTransaction();
        try
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"UPDATE {TableName} SET \"{column}\"=@newValue WHERE \"{column}\"=@oldValue";
            command.Parameters.AddWithValue("@newValue", newValue);
            command.Parameters.AddWithValue("@oldValue", oldValue);
            int affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                transaction.Rollback();
                throw new InvalidOperationException("数据库中没有找到要修改的原值，未写入任何内容。");
            }

            transaction.Commit();
            return new UnitExamMaintenanceResult(affected, backup);
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Preserve the original database error.
            }

            DeleteFailedBackup(backup);
            throw;
        }
    }

    private static SqliteConnection OpenSqlite(string file)
    {
        var connection = new SqliteConnection($"Data Source={file};Mode=ReadWrite");
        connection.Open();
        return connection;
    }

    private static string ValidateFile(string databaseFile, out UnitExamDatabaseKind kind)
    {
        if (string.IsNullOrWhiteSpace(databaseFile))
        {
            throw new ArgumentException("请先选择单位考评文件。", nameof(databaseFile));
        }

        string file = Path.GetFullPath(databaseFile);
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("单位考评文件不存在：" + file, file);
        }

        kind = Path.GetExtension(file).ToLowerInvariant() switch
        {
            ".exam" or ".mdb" => UnitExamDatabaseKind.Access,
            ".db" => UnitExamDatabaseKind.Sqlite,
            _ => throw new NotSupportedException("仅支持 .exam / .mdb（Access）与 .db（SQLite）单位考评文件。")
        };
        return file;
    }

    private static string CreateBackupPath(string file)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", System.Globalization.CultureInfo.InvariantCulture);
        return file + "." + timestamp + ".bak";
    }

    private static string RequireValue(string value, string message)
    {
        string result = value?.Trim() ?? string.Empty;
        return result.Length == 0 ? throw new ArgumentException(message) : result;
    }

    private static string Text(object? value) => value is null or DBNull ? string.Empty : Convert.ToString(value) ?? string.Empty;

    private static void DeleteFailedBackup(string backup)
    {
        try
        {
            File.Delete(backup);
        }
        catch
        {
            // A backup is safe to leave behind; never hide the original database error.
        }
    }
}
