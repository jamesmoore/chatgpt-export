namespace ChatGPTExport.Assets
{
    public record AssetRequest(
        string SearchPattern, 
        string Role, 
        DateTimeOffset? CreatedDate, 
        DateTimeOffset? UpdatedDate);
}
