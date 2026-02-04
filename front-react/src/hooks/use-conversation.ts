import { useQuery } from "@tanstack/react-query";
import { getConversationHtml, getConversationJson, getConversationMarkdown } from "../api-client";

export function useConversation(id: string | undefined, format: string | undefined) {
    return useQuery({
        queryKey: ['conversation', id, format],
        queryFn: () => {
            if (format === 'html') {
                return getConversationHtml(id!);
            }
            else if (format === 'markdown') {
                return getConversationMarkdown(id!);
            }
            else if (format === 'json') {
                return getConversationJson(id!);
            }
            else {
                throw new Error(`Unsupported format: ${format}`);
            }
        },
        enabled: !!id,
    });
}
