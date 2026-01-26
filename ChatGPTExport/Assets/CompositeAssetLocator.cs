namespace ChatGPTExport.Assets
{
    internal class CompositeAssetLocator(IEnumerable<IAssetLocator> assetLocators) : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            foreach (var assetLocator in assetLocators)
            {
                var result = assetLocator.GetMarkdownMediaAsset(assetRequest);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
