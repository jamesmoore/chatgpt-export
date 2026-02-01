using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public interface IDirectoryCache
    {
        IEnumerable<IDirectoryInfo>? GetDirectories();
        void SetDirectories(IEnumerable<IDirectoryInfo> directoryInfos);
    }
}