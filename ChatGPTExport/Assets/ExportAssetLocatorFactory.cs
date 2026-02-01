using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    internal class ExportAssetLocatorFactory()
    {
        public IAssetLocator GetAssetLocator(IEnumerable<ConversationAssets> conversationAssets, IDirectoryInfo destination)
        {
            var existingAssetLocator = new ExistingAssetLocator(destination);
            var assetLocators = conversationAssets.Select(p => new AssetLocator(p, destination, existingAssetLocator) as IAssetLocator).ToList();
            assetLocators.Insert(0, existingAssetLocator);

            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return compositeAssetLocator;
        }
    }
}
