using ChatGPTExport.Assets;
using System.Text.Encodings.Web;

namespace ChatGpt.Archive.Api.Services
{
    public class ApiAssetLocator(
        IConversationAssetsCache directoryCache
        ) : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var foundAsset = directoryCache.GetMediaAsset(assetRequest.SearchPattern);
            if (foundAsset == null)
            {
                return null; 
            }

            return new Asset(foundAsset.Name, $"/asset/{foundAsset.RootId}/{UrlEncoder.Default.Encode(foundAsset.RelativePath)}");
        }
    }
}
