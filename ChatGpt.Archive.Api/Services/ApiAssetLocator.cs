using ChatGPTExport.Assets;
using System.Text.Encodings.Web;

namespace ChatGpt.Archive.Api.Services
{
    public class ApiAssetLocator(IConversationAssetsCache directoryCache) : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var foundAsset = directoryCache.FindMediaAsset(assetRequest.SearchPattern);
            if (foundAsset == null)
            {
                return null;
            }

            string signature = AssetSignature.Create(foundAsset.RootId, foundAsset.RelativePath);
            string assetUrl = $"/asset/{foundAsset.RootId}/{UrlEncoder.Default.Encode(foundAsset.RelativePath)}?sig={signature}";
            return new Asset(foundAsset.Name, assetUrl);
        }
    }
}