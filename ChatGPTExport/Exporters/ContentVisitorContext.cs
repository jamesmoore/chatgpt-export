using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    public record ContentVisitorContext(
        string Role, 
        DateTimeOffset? CreatedDate,
        DateTimeOffset? UpdatedDate,
        MessageMetadata MessageMetadata,
        string Recipient
        );
}
