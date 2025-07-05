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

        public static readonly IEnumerable<string> AllTypes = [
            Text,
            MultimodalText,
            Thoughts,
            ReasoningRecap,
            Code,
            ExecutionOutput
        ];
    }
}
