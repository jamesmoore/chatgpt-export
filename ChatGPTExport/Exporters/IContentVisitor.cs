﻿using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
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
    }
}
