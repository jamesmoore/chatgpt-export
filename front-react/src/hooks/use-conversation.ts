import { useQuery } from "@tanstack/react-query";
import { getConversationHtml, getConversationJson, getConversationMarkdown } from "../api-client";

export function useConversation(id: string | undefined, format: string | undefined) {
    return useQuery({
        queryKey: ['conversation', id, format],
        queryFn: () => {
            switch (format) {
                case 'html':
                    return getConversationHtml(id!);
                case 'markdown':
                    return getConversationMarkdown(id!);
                case 'json':
                    return getConversationJson(id!);
                default:
                    throw new Error(`Unsupported format: ${format}`);
            }
        },
        enabled: !!id && !!format && ['html', 'markdown', 'json'].includes(format),
    });
}
