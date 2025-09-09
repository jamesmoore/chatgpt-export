using System.IO.Abstractions;

namespace ChatGPTExport
{
    internal static class FileSystemExtensions
    {
        private static bool? _isCaseSensitive;

        public static bool IsFileSystemCaseSensitive(this IFileSystem fileSystem, string? path = null)
        {
            if (_isCaseSensitive != null)
                return _isCaseSensitive.Value;

            path ??= fileSystem.Path.GetTempPath();
            var testFile = fileSystem.Path.Combine(path, Guid.NewGuid().ToString("N") + ".tmp");

            try
            {
                fileSystem.File.WriteAllText(testFile, "x");
                // Check if same file is accessible with uppercased name
                _isCaseSensitive = !fileSystem.File.Exists(testFile.ToUpperInvariant());
            }
            finally
            {
                if (fileSystem.File.Exists(testFile))
                    fileSystem.File.Delete(testFile);
            }

            return _isCaseSensitive.Value;
        }

        public static bool IsSameOrSubdirectory(this IDirectoryInfo baseDir, IDirectoryInfo candidate)
        {
            var fileSystem = baseDir.FileSystem;

            var basePath = baseDir.FullName.TrimEnd(fileSystem.Path.DirectorySeparatorChar, fileSystem.Path.AltDirectorySeparatorChar);
            var candidatePath = candidate.FullName.TrimEnd(fileSystem.Path.DirectorySeparatorChar, fileSystem.Path.AltDirectorySeparatorChar);

            var comparison = fileSystem.IsFileSystemCaseSensitive()
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            if (string.Equals(basePath, candidatePath, comparison))
            {
                return true;
            }

            return candidatePath.StartsWith(basePath + fileSystem.Path.DirectorySeparatorChar, comparison);
        }
    }
}
