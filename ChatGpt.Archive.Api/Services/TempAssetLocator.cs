using ChatGPTExport.Assets;
using System.Text.Encodings.Web;

namespace ChatGpt.Archive.Api.Services
{
    /// <summary>
    /// TODO: Obtain real assets and generate API source address.
    /// </summary>
    public class TempAssetLocator(
        IConversationAssetsCache directoryCache
        ) : IAssetLocator
    {
        // TODO: Somehow inject conversation parent directories
        // NOTE: Source directories != parent directories
        //  * Parent directories contain a valid deserializable conversations.json. 
        //  * Source directories could potentially contain multiple conversations.json or zero.

        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var foundAsset = directoryCache.GetConversationAssets().Select(p => new
            {
                p.ParentDirectory,
                Asset = p.GetAsset(assetRequest.SearchPattern)
            }).FirstOrDefault(p => p.Asset != null);

            if (foundAsset == null)
            {
                return null;
            }

            // TODO recurse source directories that had a valid conversation, find matching asset, generate URL.
            // Need to devise some 2-way-encoding to relate URL to absolute location on disk.
            return new Asset(foundAsset.Asset!.Name, "/asset/" + UrlEncoder.Default.Encode(foundAsset.Asset!.FullName));
        }
    }
}
