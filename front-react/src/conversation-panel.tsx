import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getConversationHtml, getConversationJson, getConversationMarkdown } from "./api-client";

export function ConversatrionPanel() {

    const { id, format } = useParams();

    const { data: content, error } = useQuery({
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

    if (!id) {
        return <>No conversation ID provided.</>;
    }

    if (error) {
        return <>{error instanceof Error ? error.message : "Failed to load conversation."}</>;
    }

    return (<>{content || ""}</>);
}
