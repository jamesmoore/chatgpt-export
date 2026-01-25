using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatGPTExport.Models
{
    public class MessageMetadata
    {
        public string[]? selected_sources { get; set; }
        public string[]? selected_github_repos { get; set; }
        public SerializationMetadata? serialization_metadata { get; set; }
        public FinishDetails? finish_details { get; set; }
        public bool? rebase_system_message { get; set; }
        public string[]? safe_urls { get; set; }
        public Search_Result_Groups[]? search_result_groups { get; set; }
        public bool? is_complete { get; set; }
        public object[]? citations { get; set; }
        public Content_References[]? content_references { get; set; }
        public string? reasoning_status { get; set; }
        public string? gizmo_id { get; set; }
        public int? search_turns_count { get; set; }
        public string? search_source { get; set; }
        public string? client_reported_search_source { get; set; }
        public object? message_type { get; set; }
        public string? model_slug { get; set; }
        public string? default_model_slug { get; set; }
        public string? parent_id { get; set; }
        public string? request_id { get; set; }
        public string? timestamp_ { get; set; }
        public bool? is_visually_hidden_from_conversation { get; set; }
        public string[]? followup_prompts { get; set; }
        public object[]? search_queries { get; set; }
        public object[]? image_results { get; set; }
        public bool? real_time_audio_has_video { get; set; }
        public string? image_gen_title { get; set; } // TODO add in caption
        public bool? image_gen_async { get; set; }
        public bool? trigger_async_ux { get; set; }
        public bool? is_error { get; set; }
        public object? paragen_variants_info { get; set; }
        public string? paragen_variant_choice { get; set; }
        public string? status { get; set; }
        public string? command { get; set; }
        public string? debug_sonic_thread_id { get; set; }
        public object? sonic_classification_result { get; set; }
        public string? requested_model_slug { get; set; }
        public Attachment[]? attachments { get; set; }
        public string? search_display_string { get; set; }
        public string? searched_display_string { get; set; }
        public string? message_locale { get; set; }
        public bool? rebase_developer_message { get; set; }
        public int? finished_duration_sec { get; set; }
        public AggregateResult? aggregate_result { get; set; }
        public string? exclusive_key { get; set; }
        public object? canvas { get; set; }
        public class SerializationMetadata
        {
            public object[]? custom_symbol_offsets { get; set; }
        }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraData { get; set; }
        public class Attachment
        {
            public string? id { get; set; }
            public int? size { get; set; }
            public string? name { get; set; }
            public string? mime_type { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
            public string? mimeType { get; set; }
            public object? fileSizeTokens { get; set; }

            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtraData { get; set; }
        }

        public class FinishDetails
        {
            public string? type { get; set; }
            public int[]? stop_tokens { get; set; }
            public string? reason { get; set; }

            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtraData { get; set; }
        }

        public class Content_References
        {
            public string? matched_text { get; set; }
            public int start_idx { get; set; }
            public int end_idx { get; set; }
            public string[]? safe_urls { get; set; }
            public object[]? refs { get; set; }
            public string? alt { get; set; }
            public object? prompt_text { get; set; }
            public string? type { get; set; }
            public bool? invalid { get; set; }
            public string? attributable_index { get; set; }
            public object? attributions { get; set; }
            public object? attributions_debug { get; set; }
            public Item[]? items { get; set; }
            public Fallback_Items[]? fallback_items { get; set; }
            public string? status { get; set; }
            public string? name { get; set; }
            public object? error { get; set; }
            public string? style { get; set; }
            public Source[]? sources { get; set; }
            public bool? has_images { get; set; }
            public object[]? images { get; set; }

            public string? display_title { get; set; }
            public string? page_title { get; set; }
            public string? url { get; set; }
            public string? leaf_description { get; set; }
            public string? snippet { get; set; }
            public string[]? breadcrumbs { get; set; }
            public string? title { get; set; }
            public string? thumbnail_url { get; set; }
            public ExtraParams? extra_params { get; set; }

            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtraData { get; set; }

            public class Item
            {
                public string? title { get; set; }
                public string? url { get; set; }
                public decimal? pub_date { get; set; }
                public string? snippet { get; set; }
                public string[]? attribution_segments { get; set; }
                public Supporting_Websites[]? supporting_websites { get; set; }
                public Ref[]? refs { get; set; }
                public object? hue { get; set; }
                public object? attributions { get; set; }
                public string? attribution { get; set; }
                public string? thumbnail_url { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
                public class Supporting_Websites
                {
                    public string? title { get; set; }
                    public string? url { get; set; }
                    public decimal? pub_date { get; set; }
                    public string? snippet { get; set; }
                    public string? attribution { get; set; }
                    [JsonExtensionData]
                    public Dictionary<string, JsonElement>? ExtraData { get; set; }
                }
            }
            public class Fallback_Items
            {
                public string? title { get; set; }
                public string? url { get; set; }
                public decimal? pub_date { get; set; }
                public string? snippet { get; set; }
                public object? attribution_segments { get; set; }
                public object[]? supporting_websites { get; set; }
                public Ref[]? refs { get; set; }
                public object? hue { get; set; }
                public object? attributions { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
            }

            public class Source
            {
                public string? title { get; set; }
                public string? url { get; set; }
                public string? attribution { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
            }

            public class ExtraParams
            {
                public string? disambiguation { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
            }
        }

        public class Ref
        {
            public int turn_index { get; set; }
            public string? ref_type { get; set; }
            public int ref_index { get; set; }
            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtraData { get; set; }
        }

        public class Search_Result_Groups
        {
            public string? type { get; set; }
            public string? domain { get; set; }
            public Entry[]? entries { get; set; }
            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtraData { get; set; }

            public class Entry
            {
                public string? type { get; set; }
                public string? url { get; set; }
                public string? title { get; set; }
                public string? snippet { get; set; }
                public Ref_Id? ref_id { get; set; }
                public decimal? pub_date { get; set; }
                public string? attribution { get; set; }
                public object? content_type { get; set; }
                public object? attributions { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
                public class Ref_Id
                {
                    public int turn_index { get; set; }
                    public string? ref_type { get; set; }
                    public int ref_index { get; set; }
                    [JsonExtensionData]
                    public Dictionary<string, JsonElement>? ExtraData { get; set; }
                }
            }
        }

        public class AggregateResult
        {
            public string? status { get; set; }
            public string? run_id { get; set; }
            public double start_time { get; set; }
            public double update_time { get; set; }
            public string? code { get; set; }
            public double? end_time { get; set; }
            public object? final_expression_output { get; set; }
            public object? in_kernel_exception { get; set; }
            public object? system_exception { get; set; }
            public List<Message>? messages { get; set; }
            public List<JupyterMessage>? jupyter_messages { get; set; }
            public double? timeout_triggered { get; set; }

            public class JupyterMessage
            {
                public string? msg_type { get; set; }
                public ParentHeader? parent_header { get; set; }
                public Content? content { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }

                public class ParentHeader
                {
                    public string? msg_id { get; set; }
                    public string? version { get; set; }
                }

                public class Content
                {
                    public string? execution_state { get; set; }
                    public Dictionary<string, object>? data { get; set; }

                    [JsonExtensionData]
                    public Dictionary<string, JsonElement>? ExtraData { get; set; }
                }
            }

            public class Message
            {
                public string? message_type { get; set; }
                public double time { get; set; }
                public string? sender { get; set; }
                public object? image_payload { get; set; }
                public string? image_url { get; set; }
                public int? width { get; set; }
                public int? height { get; set; }
                public double? timeout_triggered { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
            }
        }
    }
}
