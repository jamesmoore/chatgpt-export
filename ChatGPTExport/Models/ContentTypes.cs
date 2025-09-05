namespace ChatGPTExport.Models
{
    internal class ContentTypes
    {
        public const string Text = "text";
        public const string MultimodalText = "multimodal_text";
        public const string Thoughts = "thoughts";
        public const string ReasoningRecap = "reasoning_recap";
        public const string Code = "code";
        public const string ExecutionOutput = "execution_output";
        public const string UserEditableContext = "user_editable_context";
        public const string TetherBrowsingDisplay = "tether_browsing_display";
        public const string ComputerOutput = "computer_output";
        public const string SystemError = "system_error";

        public static readonly IEnumerable<string> AllTypes = [
            Text,
            MultimodalText,
            Thoughts,
            ReasoningRecap,
            Code,
            ExecutionOutput,
            UserEditableContext,
            TetherBrowsingDisplay,
            ComputerOutput,
            SystemError,
        ];
    }
}
