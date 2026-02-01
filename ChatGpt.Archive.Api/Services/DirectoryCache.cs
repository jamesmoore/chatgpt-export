using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class DirectoryCache : IDirectoryCache
    {
        private IEnumerable<IDirectoryInfo>? directoryInfos = null;

        public IEnumerable<IDirectoryInfo>? GetDirectories()
        {
            return directoryInfos;
        }

        public void SetDirectories(IEnumerable<IDirectoryInfo> directoryInfos)
        {
            this.directoryInfos = directoryInfos;
        }
    }
}
