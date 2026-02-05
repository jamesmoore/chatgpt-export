using ChatGPTExport.Assets;
using System.IO.Abstractions;

namespace ChatGpt.Exporter.Cli.Assets
{
    internal class ExportAssetLocatorFactory()
    {
        public IAssetLocator GetAssetLocator(IEnumerable<ConversationAssets> conversationAssets, IDirectoryInfo destination)
        {
            var existingAssetLocator = new ExistingAssetLocator(destination);
            var assetLocators = conversationAssets.Select(asset => new AssetLocator(asset, destination, existingAssetLocator) as IAssetLocator).ToList();
            assetLocators.Insert(0, existingAssetLocator);

            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);
            return compositeAssetLocator;
        }
    }
}
