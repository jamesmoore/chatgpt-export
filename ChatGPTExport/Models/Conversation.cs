using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Exporters;

namespace ChatGPTExport.Models
{
    public class Conversations : List<Conversation>
    {

    }

    public class Conversation
    {
        public string title { get; set; }
        public decimal create_time { get; set; }
        public decimal update_time { get; set; }
        public Dictionary<string, MessageContainer> mapping { get; set; }
        public List<object> moderation_results { get; set; }
        public string current_node { get; set; }
        public object plugin_ids { get; set; }
        public string conversation_id { get; set; }
        public object conversation_template_id { get; set; }
        public object gizmo_id { get; set; }
        public object gizmo_type { get; set; }
        public bool is_archived { get; set; }
        public object is_starred { get; set; }
        public List<string> safe_urls { get; set; }
        public List<string> blocked_urls { get; set; }
        public string default_model_slug { get; set; }
        public object conversation_origin { get; set; }
        public object voice { get; set; }
        public object async_status { get; set; }
        public List<object> disabled_tool_ids { get; set; }
        public bool? is_do_not_remember { get; set; }
        public string memory_scope { get; set; }
        public string sugar_item_id { get; set; }
        public string id { get; set; }

        public DateTimeOffset GetCreateTime() => create_time.ToDateTimeOffset();

        public DateTimeOffset GetUpdateTime() => update_time.ToDateTimeOffset();

        private string GetLastLeaf() => mapping.Where(p => p.Value.IsLeaf()).Last().Key;

        private Dictionary<string, MessageContainer> GetLatestBranch()
        {
            var temp = new List<MessageContainer>();
            var currentId = GetLastLeaf();
            do
            {
                var messageContainer = this.mapping[currentId];
                temp.Insert(0, messageContainer);
                currentId = messageContainer.parent;
            } while (currentId != null);
            return temp.ToDictionary(p => p.id, p => p);
        }

        public Conversation GetLastestConversation()
        {
            var clone = this.MemberwiseClone() as Conversation;
            clone.mapping = GetLatestBranch();
            return clone;
        }

        public bool HasMultipleBranches()
        {
            return mapping.Where(p => p.Value.IsLeaf()).Count() > 1;
        }
    }

    public class MessageContainer
    {
        public string id { get; set; }
        public Message message { get; set; }
        public string parent { get; set; }
        public List<string> children { get; set; }

        public bool IsLeaf() => this.children.Count == 0;
    }

    public class Message
    {
        public string id { get; set; }
        public Author author { get; set; }
        public decimal? create_time { get; set; }
        public decimal? update_time { get; set; }
        public ContentBase content { get; set; }
        public string status { get; set; }
        public bool? end_turn { get; set; }
        public decimal weight { get; set; }
        public MessageMetadata metadata { get; set; }
        public string recipient { get; set; }
        public string channel { get; set; }

        public class MessageMetadata
        {
            public string[] selected_sources { get; set; }
            public string[] selected_github_repos { get; set; }
            public SerializationMetadata serialization_metadata { get; set; }
            public FinishDetails finish_details { get; set; }
            public bool? rebase_system_message { get; set; }
            public string[] safe_urls { get; set; }
            public Search_Result_Groups[] search_result_groups { get; set; }
            public bool? is_complete { get; set; }
            public object[] citations { get; set; }
            public Content_References[] content_references { get; set; }
            public string reasoning_status { get; set; }
            public string gizmo_id { get; set; }
            public int? search_turns_count { get; set; }
            public string search_source { get; set; }
            public string client_reported_search_source { get; set; }
            public object message_type { get; set; }
            public string model_slug { get; set; }
            public string default_model_slug { get; set; }
            public string parent_id { get; set; }
            public string request_id { get; set; }
            public string timestamp_ { get; set; }
            public bool? is_visually_hidden_from_conversation { get; set; }
            public string[] followup_prompts { get; set; }
            public object[] search_queries { get; set; }
            public object[] image_results { get; set; }
            public bool? real_time_audio_has_video { get; set; }
            public string image_gen_title { get; set; } // TODO add in caption
            public bool? image_gen_async { get; set; }
            public bool? trigger_async_ux { get; set; }
            public bool? is_error { get; set; }
            public object paragen_variants_info { get; set; }
            public string paragen_variant_choice { get; set; }
            public string status { get; set; }
            public string command { get; set; }
            public string debug_sonic_thread_id { get; set; }
            public object sonic_classification_result { get; set; }
            public string requested_model_slug { get; set; }
            public Attachment[] attachments { get; set; }
            public string search_display_string { get; set; }
            public string searched_display_string { get; set; }
            public string message_locale { get; set; }
            public bool? rebase_developer_message { get; set; }
            public int? finished_duration_sec { get; set; }
            public object aggregate_result { get; set; }
            public string exclusive_key { get; set; }
            public object canvas { get; set; }
            public class SerializationMetadata
            {
                public object[] custom_symbol_offsets { get; set; }
            }

            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtraData { get; set; }
        }

        public class Attachment
        {
            public string id { get; set; }
            public int size { get; set; }
            public string name { get; set; }
            public string mime_type { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class FinishDetails
        {
            public string type { get; set; }
            public int[] stop_tokens { get; set; }
            public string reason { get; set; }
        }

        public class Content_References
        {
            public string matched_text { get; set; }
            public int start_idx { get; set; }
            public int end_idx { get; set; }
            public string[] safe_urls { get; set; }
            public object[] refs { get; set; }
            public string alt { get; set; }
            public object prompt_text { get; set; }
            public string type { get; set; }
            public bool? invalid { get; set; }
            public string attributable_index { get; set; }
            public object attributions { get; set; }
            public object attributions_debug { get; set; }
            public Item[] items { get; set; }
            public Fallback_Items[] fallback_items { get; set; }
            public string status { get; set; }
            public object error { get; set; }
            public string style { get; set; }
            public Source[] sources { get; set; }
            public bool? has_images { get; set; }
            public object[] images { get; set; }

            public string display_title { get; set; }
            public string page_title { get; set; }
            public string url { get; set; }
            public string leaf_description { get; set; }
            public string snippet { get; set; }
            public string[] breadcrumbs { get; set; }
            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtraData { get; set; }
        }

        public class Item
        {
            public string title { get; set; }
            public string url { get; set; }
            public decimal? pub_date { get; set; }
            public string snippet { get; set; }
            public string[] attribution_segments { get; set; }
            public Supporting_Websites[] supporting_websites { get; set; }
            public Ref[] refs { get; set; }
            public object hue { get; set; }
            public object attributions { get; set; }
            public string attribution { get; set; }
        }

        public class Supporting_Websites
        {
            public string title { get; set; }
            public string url { get; set; }
            public decimal? pub_date { get; set; }
            public string snippet { get; set; }
            public string attribution { get; set; }
        }

        public class Ref
        {
            public int turn_index { get; set; }
            public string ref_type { get; set; }
            public int ref_index { get; set; }
        }

        public class Fallback_Items
        {
            public string title { get; set; }
            public string url { get; set; }
            public decimal? pub_date { get; set; }
            public string snippet { get; set; }
            public object attribution_segments { get; set; }
            public object[] supporting_websites { get; set; }
            public Ref[] refs { get; set; }
            public object hue { get; set; }
            public object attributions { get; set; }
        }

        public class Source
        {
            public string title { get; set; }
            public string url { get; set; }
            public string attribution { get; set; }
        }

        public class Search_Result_Groups
        {
            public string type { get; set; }
            public string domain { get; set; }
            public Entry[] entries { get; set; }
        }

        public class Entry
        {
            public string type { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public string snippet { get; set; }
            public Ref_Id ref_id { get; set; }
            public decimal? pub_date { get; set; }
            public string attribution { get; set; }
        }

        public class Ref_Id
        {
            public int turn_index { get; set; }
            public string ref_type { get; set; }
            public int ref_index { get; set; }
        }

        public T Accept<T>(IContentVisitor<T> visitor)
        {
            return this.content.Accept(visitor, new ContentVisitorContext(this.author.role, GetCreateTime(), GetUpdateTime(), metadata));
        }

        public DateTimeOffset? GetCreateTime() => create_time.HasValue ? create_time.Value.ToDateTimeOffset() : null;

        public DateTimeOffset? GetUpdateTime() => update_time.HasValue ? update_time.Value.ToDateTimeOffset() : null;
    }

    public class Author
    {
        public string role { get; set; }
        public string name { get; set; }
        public AuthorMetadata metadata { get; set; }

        public class AuthorMetadata
        {

            public string? real_author { get; set; }

            public string? sonicberry_model_id { get; set; }

            public string? source { get; set; }
        }
    }

    [JsonPolymorphic(
        TypeDiscriminatorPropertyName = "content_type",
        IgnoreUnrecognizedTypeDiscriminators = true
        )]
    [JsonDerivedType(typeof(ContentText), ContentTypes.Text)]
    [JsonDerivedType(typeof(ContentMultimodalText), ContentTypes.MultimodalText)]
    [JsonDerivedType(typeof(ContentThoughts), ContentTypes.Thoughts)]
    [JsonDerivedType(typeof(ContentReasoningRecap), ContentTypes.ReasoningRecap)]
    [JsonDerivedType(typeof(ContentCode), ContentTypes.Code)]
    [JsonDerivedType(typeof(ContentExecutionOutput), ContentTypes.ExecutionOutput)]
    public class ContentBase
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtraData { get; set; }

        public virtual T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentText : ContentBase
    {
        public List<string> parts { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentMultimodalText : ContentBase
    {
        /// <summary>
        /// This could be a string or a ContentMultimodalTextParts
        /// </summary>
        [JsonConverter(typeof(ItemListConverter))]
        public List<ContentMultimodalTextPartsContainer> parts { get; set; }
        public class ContentMultimodalTextPartsContainer
        {
            public string StringValue { get; set; }
            public ContentMultimodalTextParts ObjectValue { get; set; }

            public bool IsString => StringValue != null;
            public bool IsObject => ObjectValue != null;
        }

        public class ContentMultimodalTextParts
        {
            public string content_type { get; set; }
            public string asset_pointer { get; set; }
            public int size_bytes { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public object fovea { get; set; }
            public Metadata metadata { get; set; }

            public class Metadata
            {
                public Dalle dalle { get; set; }
                public object gizmo { get; set; }
                public Generation generation { get; set; }
                public int? container_pixel_height { get; set; }
                public int? container_pixel_width { get; set; }
                public object emu_omit_glimpse_image { get; set; }
                public object emu_patches_override { get; set; }
                public bool sanitized { get; set; }
                public object asset_pointer_link { get; set; }
                public object watermarked_asset_pointer { get; set; }
            }

            public class Dalle
            {
                public string gen_id { get; set; }
                public string prompt { get; set; }
                public object seed { get; set; }
                public object parent_gen_id { get; set; }
                public object edit_op { get; set; }
                public string serialization_title { get; set; }
            }

            public class Generation
            {
                public string gen_id { get; set; }
                public string gen_size { get; set; }
                public object seed { get; set; }
                public object parent_gen_id { get; set; }
                public int height { get; set; }
                public int width { get; set; }
                public bool transparent_background { get; set; }
                public string serialization_title { get; set; }
            }

        }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentThoughts : ContentBase
    {
        public List<Thoughts> thoughts { get; set; }
        public string source_analysis_msg_id { get; set; }

        public class Thoughts
        {
            public string summary { get; set; }
            public string content { get; set; }
        }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentReasoningRecap : ContentBase
    {
        public string content { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentCode : ContentBase
    {
        public string language { get; set; }
        public string response_format_name { get; set; }
        public string text { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentExecutionOutput : ContentBase
    {
        public string text { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }
}
