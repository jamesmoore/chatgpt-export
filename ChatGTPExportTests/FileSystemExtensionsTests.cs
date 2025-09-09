using System.IO.Abstractions.TestingHelpers;

using ChatGPTExport;

namespace ChatGTPExportTests;

public class FileSystemExtensionsTests
{
    private static void ResetCaseSensitivityCache()
    {
        FileSystemExtensions.ResetCaseSensitivityCache();
    }
    private static MockFileSystem CreateFileSystem(bool caseSensitive)
    {
        var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        return new MockFileSystem(new Dictionary<string, MockFileData>(comparer));
    }

    [Fact]
    public void IsSameOrSubdirectory_SamePath_CaseSensitive()
    {
        ResetCaseSensitivityCache();
        var fs = CreateFileSystem(caseSensitive: true);
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/root");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }

    [Fact]
    public void IsSameOrSubdirectory_Subdirectory_CaseSensitive()
    {
        ResetCaseSensitivityCache();
        var fs = CreateFileSystem(caseSensitive: true);
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/root/sub");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }

    [Fact]
    public void IsSameOrSubdirectory_SamePath_CaseInsensitive()
    {
        ResetCaseSensitivityCache();
        var fs = CreateFileSystem(caseSensitive: false);
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/ROOT");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }

    [Fact]
    public void IsSameOrSubdirectory_Subdirectory_CaseInsensitive()
    {
        ResetCaseSensitivityCache();
        var fs = CreateFileSystem(caseSensitive: false);
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/ROOT/sub");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }
}

