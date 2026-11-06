using System.IO;
using DocumentTools.Core.Services.Files;

namespace DocumentTools.Tests;

public class FileCopyServiceTests
{
    private readonly FileCopyService _sut = new();

    private static string NewTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "dt-filecopy-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string NewSourceFile(string dir, string name = "报告.docx")
    {
        string path = Path.Combine(dir, name);
        File.WriteAllText(path, "hello");
        return path;
    }

    [Fact]
    public void BuildCopyName_UsesDashSuffixAndZeroPad()
    {
        string name = _sut.BuildCopyName(@"C:\x\报告.docx", "副本", 7, 3);
        Assert.Equal("报告 - 副本 (007).docx", name);
    }

    [Fact]
    public void BuildCopyName_BlankSuffix_FallsBackToDefaultAndNoPadWhenZeroDigits()
    {
        string name = _sut.BuildCopyName(@"C:\x\a.png", "  ", 12, 0);
        Assert.Equal("a - 副本 (12).png", name);
    }

    [Fact]
    public void Generate_CreatesRequestedNumberOfFiles_WithContinuousNames()
    {
        string dir = NewTempDir();
        try
        {
            string src = NewSourceFile(dir);

            var result = _sut.Generate(src, dir, 3, "副本", 1);

            Assert.Equal(3, result.Created);
            Assert.Equal(1, result.FirstIndex);
            Assert.Equal(3, result.LastIndex);
            foreach (int i in new[] { 1, 2, 3 })
            {
                string expected = Path.Combine(dir, $"报告 - 副本 ({i}).docx");
                Assert.True(File.Exists(expected), expected);
            }

            Assert.Equal("hello", File.ReadAllText(Path.Combine(dir, "报告 - 副本 (1).docx")));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Generate_ContinuesAfterExistingCopies()
    {
        string dir = NewTempDir();
        try
        {
            string src = NewSourceFile(dir);
            // 先造两个已存在的副本（序号 1、2），新建应从 3 开始。
            File.WriteAllText(Path.Combine(dir, "报告 - 副本 (1).docx"), "x");
            File.WriteAllText(Path.Combine(dir, "报告 - 副本 (2).docx"), "x");

            var result = _sut.Generate(src, dir, 2, "副本", 1);

            Assert.Equal(3, result.FirstIndex);
            Assert.Equal(4, result.LastIndex);
            Assert.True(File.Exists(Path.Combine(dir, "报告 - 副本 (3).docx")));
            Assert.True(File.Exists(Path.Combine(dir, "报告 - 副本 (4).docx")));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void PreviewName_ShowsNextAvailableName()
    {
        string dir = NewTempDir();
        try
        {
            string src = NewSourceFile(dir);
            File.WriteAllText(Path.Combine(dir, "报告 - 副本 (5).docx"), "x");

            Assert.Equal("报告 - 副本 (6).docx", _sut.PreviewName(src, dir, "副本", 1));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Generate_MissingSource_Throws()
    {
        string dir = NewTempDir();
        try
        {
            Assert.Throws<FileNotFoundException>(() =>
                _sut.Generate(Path.Combine(dir, "nope.docx"), dir, 1, "副本", 1));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
