namespace ChatGPTExport.Assets
{
    internal class CompositeAssetLocator(IEnumerable<IAssetLocator> assetLocators) : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            return assetLocators
                .Select(assetLocator => assetLocator.GetMarkdownMediaAsset(assetRequest))
                .FirstOrDefault(result => result != null);
        }
    }
}
