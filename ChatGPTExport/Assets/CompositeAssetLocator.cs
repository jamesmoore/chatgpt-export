namespace ChatGPTExport.Assets
{
    public class CompositeAssetLocator(IEnumerable<IAssetLocator> assetLocators) : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            return assetLocators
                .Select(locator => locator.GetMarkdownMediaAsset(assetRequest))
                .FirstOrDefault(asset => asset != null);
        }
    }
}
