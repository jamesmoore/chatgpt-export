using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using ChatGPTExport.Formatters.Html;
using Microsoft.AspNetCore.Mvc;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationsController(
        IConversationsService conversationsService,
        ApiAssetLocator apiAssetLocator
        ) : ControllerBase
    {
        /// <summary>
        /// Returns the latest conversation details for each instance.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult<IEnumerable<ConversationSummary>> Get()
        {
            var latestConversations = conversationsService.GetLatestConversations();

            var result = latestConversations.Select(p => new ConversationSummary(p.id!, p.title ?? "No title", p.gizmo_id, p.GetCreateTime(), p.GetUpdateTime()));

            return Ok(result);
        }

        /// <summary>
        /// Returns the conversation in the HTML format.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [HttpGet("{id}/html")]
        [Produces("text/html")]
        public ActionResult<string> GetConversationHtml(string id) => GetActionResult(id, ExportType.Html, "text/html");

        /// <summary>
        /// Returns the conversation in the Markdown format.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [HttpGet("{id}/markdown")]
        [Produces("text/markdown")]
        public ActionResult<string> GetConversationMarkdown(string id) => GetActionResult(id, ExportType.Markdown, "text/markdown");

        /// <summary>
        /// Returns the conversation in the Markdown format.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [HttpGet("{id}/json")]
        [Produces("application/json")]
        public ActionResult<string> GetConversationJson(string id) => GetActionResult(id, ExportType.Json, "application/json");

        private ActionResult<string> GetActionResult(string id, ExportType exportType, string contentType)
        {
            var content = GetContent(id, exportType);
            if (content == null)
            {
                return NotFound();
            }
            return Content(content, contentType);
        }

        private string? GetContent(string id, ExportType exportType)
        {
            var formatter = new ConversationFormatterFactory().GetFormatters([exportType], HtmlFormat.Tailwind, false);
            var conversation = conversationsService.GetConversation(id);
            if (conversation == null)
            {
                return null;
            }
            var formatted = formatter.First().Format(apiAssetLocator, conversation.GetLastestConversation());
            string content = string.Join(Environment.NewLine, formatted);
            return content;
        }
    }

    public record ConversationSummary(string Id, string Title, string? GizmoId, DateTimeOffset Created, DateTimeOffset Updated);
}

