using System.IO.Abstractions.TestingHelpers;
using ChatGPTExport;

namespace ChatGTPExportTests;

public class FileSystemExtensionsTests
{
    private static MockFileSystem CreateFileSystem()
    {
        return new MockFileSystem(new Dictionary<string, MockFileData>());
    }

    [Fact]
    public void IsSameOrSubdirectory_SamePath_CaseSensitive()
    {
        FileSystemExtensions.OverrideCaseSensitivityCache(true);
        var fs = CreateFileSystem();
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/root");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }

    [Fact]
    public void IsSameOrSubdirectory_Subdirectory_CaseSensitive()
    {
        FileSystemExtensions.OverrideCaseSensitivityCache(true);
        var fs = CreateFileSystem();
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/root/sub");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }

    [Fact]
    public void IsSameOrSubdirectory_SamePath_CaseInsensitive()
    {
        FileSystemExtensions.OverrideCaseSensitivityCache(false);
        var fs = CreateFileSystem();
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/ROOT");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }

    [Fact]
    public void IsSameOrSubdirectory_Subdirectory_CaseInsensitive()
    {
        FileSystemExtensions.OverrideCaseSensitivityCache(false);
        var fs = CreateFileSystem();
        var baseDir = fs.DirectoryInfo.New("/root");
        var candidate = fs.DirectoryInfo.New("/ROOT/sub");

        Assert.True(baseDir.IsSameOrSubdirectory(candidate));
    }
}

