using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public class TempAssetLocator : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            return new Asset("Some asset", "/asset/whatever.png");
        }
    }
}
