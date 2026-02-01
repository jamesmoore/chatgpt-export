using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    /// <summary>
    /// TODO: Obtain real assets and generate API source address.
    /// </summary>
    public class TempAssetLocatons(
        IConversationAssetsCache directoryCache
        ) : IAssetLocator
    {
        // TODO: Somehow inject conversation parent directories
        // NOTE: Source directories != parent directories
        //  * Parent directories contain a valid deserializable conversations.json. 
        //  * Source directories could potentially contain multiple conversations.json or zero.

        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var foundAsset = directoryCache.GetConversationAssets().Select(p => new { 
                ConversationAssets = p, 
                Asset = p.GetAsset(assetRequest.SearchPattern) }).FirstOrDefault(p => p.Asset != null);

            // TODO recurse source directories that had a valid conversation, find matching asset, generate URL.
            // Need to devise some 2-way-encoding to relate URL to absolute location on disk.
            return new Asset("Some asset", "/asset/whatever.png");
        }
    }
}
