using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public interface IContentVisitor<T>
    {
        T Visit(ContentText content, ContentVisitorContext context);
        T Visit(ContentMultimodalText content, ContentVisitorContext context);
        T Visit(ContentCode content, ContentVisitorContext context);
        T Visit(ContentThoughts content, ContentVisitorContext context);
        T Visit(ContentExecutionOutput content, ContentVisitorContext context);
        T Visit(ContentReasoningRecap content, ContentVisitorContext context);
        T Visit(ContentBase content, ContentVisitorContext context);
        T Visit(ContentUserEditableContext content, ContentVisitorContext context);
        T Visit(ContentTetherBrowsingDisplay content, ContentVisitorContext context);
        T Visit(ContentComputerOutput content, ContentVisitorContext context);
        T Visit(ContentSystemError content, ContentVisitorContext context);
    }
}
