using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Exporters;

namespace ChatGPTExport.Models
{
    public class Conversations : List<Conversation>
    {
        public DateTimeOffset GetUpdateTime() => this.Max(p => p.GetUpdateTime());
    }

    public class Conversation
    {
        public string? title { get; set; }
        public decimal create_time { get; set; }
        public decimal update_time { get; set; }
        public Dictionary<string, MessageContainer>? mapping { get; set; }
        public List<object>? moderation_results { get; set; }
        public string? current_node { get; set; }
        public object? plugin_ids { get; set; }
        public string? conversation_id { get; set; }
        public object? conversation_template_id { get; set; }
        public object? gizmo_id { get; set; }
        public object? gizmo_type { get; set; }
        public bool is_archived { get; set; }
        public object? is_starred { get; set; }
        public List<string>? safe_urls { get; set; }
        public List<string>? blocked_urls { get; set; }
        public string? default_model_slug { get; set; }
        public object? conversation_origin { get; set; }
        public object? voice { get; set; }
        public object? async_status { get; set; }
        public List<object>? disabled_tool_ids { get; set; }
        public bool? is_do_not_remember { get; set; }
        public string? memory_scope { get; set; }
        public string? sugar_item_id { get; set; }
        public string? id { get; set; }

        public DateTimeOffset GetCreateTime() => create_time.ToDateTimeOffset();

        public DateTimeOffset GetUpdateTime() => update_time.ToDateTimeOffset();

        private string? GetLastLeaf()
        {
            if (mapping == null)
            {
                return null;
            }

            var leaves = mapping.Where(p => p.Value.IsLeaf()).ToList();

            var allHaveTimestamp = leaves.All(p => p.Value.message?.create_time.HasValue ?? false);
            if (!allHaveTimestamp)
            {
                Console.WriteLine("Warning: Not all messages have create_time.");
            }

            var lastLeaf = allHaveTimestamp ?
                leaves.OrderBy(p => p.Value.message!.create_time).Last().Key :
                leaves.Last().Key;
            return lastLeaf;
        }

        private Dictionary<string, MessageContainer> GetLatestBranch()
        {
            if (mapping == null)
            {
                return [];
            }

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

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraData { get; set; }
    }

    public class MessageContainer
    {
        public string? id { get; set; }
        public Message? message { get; set; }
        public string? parent { get; set; }
        public List<string>? children { get; set; }

        public bool IsLeaf() => this.children == null || this.children.Count == 0;
    }

    public class Message
    {
        public string? id { get; set; }
        public Author? author { get; set; }
        public decimal? create_time { get; set; }
        public decimal? update_time { get; set; }
        public ContentBase? content { get; set; }
        public string? status { get; set; }
        public bool? end_turn { get; set; }
        public decimal weight { get; set; }
        public MessageMetadata? metadata { get; set; }
        public string? recipient { get; set; }
        public string? channel { get; set; }

        public decimal GetMessageTimestamp()
        {
            return update_time ?? create_time ?? 0;
        }

        public T? Accept<T>(IContentVisitor<T> visitor)
        {
            if (this.content != null &&
                this.author?.role != null &&
                this.metadata != null &&
                this.recipient != null
                )
            {
                return this.content.Accept(visitor, new ContentVisitorContext(this.author.role, GetCreateTime(), GetUpdateTime(), metadata, this.recipient));
            }
            else
            {
                return default;
            }
        }

        public DateTimeOffset? GetCreateTime() => create_time.HasValue ? create_time.Value.ToDateTimeOffset() : null;

        public DateTimeOffset? GetUpdateTime() => update_time.HasValue ? update_time.Value.ToDateTimeOffset() : null;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraData { get; set; }
    }

    public class Author
    {
        public string? role { get; set; }
        public string? name { get; set; }
        public AuthorMetadata? metadata { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraData { get; set; }

        public class AuthorMetadata
        {

            public string? real_author { get; set; }

            public string? sonicberry_model_id { get; set; }

            public string? source { get; set; }
            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtraData { get; set; }
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
    [JsonDerivedType(typeof(ContentUserEditableContext), ContentTypes.UserEditableContext)]
    [JsonDerivedType(typeof(ContentTetherBrowsingDisplay), ContentTypes.TetherBrowsingDisplay)]
    [JsonDerivedType(typeof(ContentComputerOutput), ContentTypes.ComputerOutput)]
    [JsonDerivedType(typeof(ContentSystemError), ContentTypes.SystemError)]
    public class ContentBase
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraData { get; set; }

        public virtual T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentText : ContentBase
    {
        public List<string>? parts { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentMultimodalText : ContentBase
    {
        /// <summary>
        /// This could be a nullable string or a ContentMultimodalTextParts
        /// </summary>
        [JsonConverter(typeof(ItemListConverter))]
        public List<ContentMultimodalTextPartsContainer>? parts { get; set; }
        public class ContentMultimodalTextPartsContainer
        {
            public string? StringValue { get; set; }
            public ContentMultimodalTextParts? ObjectValue { get; set; }

            public bool IsString => StringValue != null;
            public bool IsObject => ObjectValue != null;
        }

        public class ContentMultimodalTextParts
        {
            public string? content_type { get; set; }
            public string? asset_pointer { get; set; }
            public int? size_bytes { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
            public object? fovea { get; set; }
            public Metadata? metadata { get; set; }
            public AudioAssetPointer? audio_asset_pointer { get; set; }
            public string? text { get; set; } // for audio transcription

            public class Metadata
            {
                public Dalle? dalle { get; set; }
                public object? gizmo { get; set; }
                public Generation? generation { get; set; }
                public int? container_pixel_height { get; set; }
                public int? container_pixel_width { get; set; }
                public object? emu_omit_glimpse_image { get; set; }
                public object? emu_patches_override { get; set; }
                public bool? sanitized { get; set; }
                public object? asset_pointer_link { get; set; }
                public object? watermarked_asset_pointer { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
            }

            public class Dalle
            {
                public string? gen_id { get; set; }
                public string? prompt { get; set; }
                public object? seed { get; set; }
                public object? parent_gen_id { get; set; }
                public object? edit_op { get; set; }
                public string? serialization_title { get; set; }
            }

            public class Generation
            {
                public string? gen_id { get; set; }
                public string? gen_size { get; set; }
                public object? seed { get; set; }
                public object? parent_gen_id { get; set; }
                public int height { get; set; }
                public int width { get; set; }
                public bool transparent_background { get; set; }
                public string? serialization_title { get; set; }
                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
            }

            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtraData { get; set; }

            public class AudioAssetPointer
            {
                public string? asset_pointer { get; set; }

                [JsonExtensionData]
                public Dictionary<string, JsonElement>? ExtraData { get; set; }
            }
        }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentThoughts : ContentBase
    {
        public List<Thoughts>? thoughts { get; set; }
        public string? source_analysis_msg_id { get; set; }

        public class Thoughts
        {
            public string? summary { get; set; }
            public string? content { get; set; }
            public object? chunks { get; set; }
            public bool? finished { get; set; }
        }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentReasoningRecap : ContentBase
    {
        public string? content { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentCode : ContentBase
    {
        public string? language { get; set; }
        public string? response_format_name { get; set; }
        public string? text { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentExecutionOutput : ContentBase
    {
        public string? text { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentUserEditableContext : ContentBase
    {
        public string? user_profile { get; set; }
        public string? user_instructions { get; set; }

        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentTetherBrowsingDisplay : ContentBase
    {
        public string? result { get; set; }
        public string? summary { get; set; }
        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentComputerOutput : ContentBase
    {
        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }

    public class ContentSystemError : ContentBase
    {
        public string? name { get; set; }
        public string? text { get; set; }
        public override T Accept<T>(IContentVisitor<T> visitor, ContentVisitorContext context)
        {
            return visitor.Visit(this, context);
        }
    }
}
