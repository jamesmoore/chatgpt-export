using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public record ContentVisitorContext(
        string Role, 
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate,
        MessageMetadata MessageMetadata,
        string Recipient
        );
}
