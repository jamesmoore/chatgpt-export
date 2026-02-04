import { useParams } from "react-router-dom";
import { useConversation } from "./hooks/use-conversation";

export function ConversationPanel() {

    const { id, format } = useParams();
    const { data: content, error } = useConversation(id, format);

    if (!id) {
        return <>No conversation ID provided.</>;
    }

    if (error) {
        return <>{error instanceof Error ? error.message : "Failed to load conversation."}</>;
    }

    if (format === "html" && content) {
        return (
            <iframe
                srcDoc={content}
                className="flex-1 w-full border-none"
                title="Conversation HTML"
            />
        );
    }

    if (format === 'markdown' || format === 'json') {
        return (
            <pre className="p-4 overflow-x-auto max-w-full">
                <code>{content}</code>
            </pre>
        );
    }

    return (<>{content || ""}</>);
}
