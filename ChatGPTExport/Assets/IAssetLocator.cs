
namespace ChatGPTExport.Assets
{
    public interface IAssetLocator
    {
        string? GetMarkdownMediaAsset(AssetRequest assetRequest);
    }
}