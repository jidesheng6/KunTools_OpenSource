using System.IO;
using DocumentTools.Core.Services.System;

namespace DocumentTools.Tests;

public class NetworkConfigFileTests
{
    private static string TempFile()
        => Path.Combine(Path.GetTempPath(), "dt-netcfg-" + Guid.NewGuid().ToString("N") + NetworkConfigFile.Extension);

    [Fact]
    public void SaveThenLoad_RoundTrips()
    {
        string path = TempFile();
        try
        {
            var original = new NetworkConfig("192.168.1.10", "255.255.255.0", "192.168.1.1", "8.8.8.8", "8.8.4.4");
            NetworkConfigFile.Save(path, original);

            NetworkConfig loaded = NetworkConfigFile.Load(path);
            Assert.Equal(original, loaded);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_LegacySingleQuotedFile_Parses()
    {
        string path = TempFile();
        try
        {
            // legacy 用单引号写 JSON，换行嵌在值之间；读取端要兼容。
            File.WriteAllText(path,
                "{IPADDRESS:'10.0.0.5',\nIPMASK:'255.255.0.0',\nIPGATEWAY:'10.0.0.1',\n'DNS1':'114.114.114.114',\n'DNS2':'223.5.5.5'}");

            NetworkConfig loaded = NetworkConfigFile.Load(path);
            Assert.Equal("10.0.0.5", loaded.Ip);
            Assert.Equal("255.255.0.0", loaded.Mask);
            Assert.Equal("10.0.0.1", loaded.Gateway);
            Assert.Equal("114.114.114.114", loaded.Dns1);
            Assert.Equal("223.5.5.5", loaded.Dns2);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_EmptyFile_Throws()
    {
        string path = TempFile();
        try
        {
            File.WriteAllText(path, string.Empty);
            Assert.Throws<InvalidDataException>(() => NetworkConfigFile.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
