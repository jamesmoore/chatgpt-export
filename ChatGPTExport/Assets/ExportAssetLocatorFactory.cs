using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    internal class ExportAssetLocatorFactory(IFileSystem fileSystem)
    {
        public IAssetLocator GetAssetLocator(IEnumerable<IDirectoryInfo> parentDirectories, IDirectoryInfo destination)
        {
            var existingAssetLocator = new ExistingAssetLocator(fileSystem, destination);
            var assetLocators = parentDirectories.Select(p => new AssetLocator(fileSystem, p, destination, existingAssetLocator) as IAssetLocator).ToList();
            assetLocators.Insert(0, existingAssetLocator);

            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return compositeAssetLocator;
        }
    }
}
