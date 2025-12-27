
namespace ChatGPTExport.Assets
{
    public interface IAssetLocator
    {
        Asset? GetMarkdownMediaAsset(AssetRequest assetRequest);
    }
}