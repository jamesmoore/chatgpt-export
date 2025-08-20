
namespace ChatGPTExport.Assets
{
    public interface IAssetLocator
    {
        string? GetMarkdownImage(AssetRequest assetRequest);
    }
}