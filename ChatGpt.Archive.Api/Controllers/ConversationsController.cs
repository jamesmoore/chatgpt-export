using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using ChatGPTExport.Formatters.Html;
using Microsoft.AspNetCore.Mvc;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationsController(IConversationsService conversationsService) : ControllerBase
    {

        /// <summary>
        /// Returns the latest conversation details for each instance.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult<IEnumerable<ConversationSummary>> Get()
        {
            var latestConversations = conversationsService.GetLatestConversations();

            var result = latestConversations.Select(p => new ConversationSummary()
            {
                Created = p.GetCreateTime(),
                GizmoId = p.gizmo_id,
                Id = p.id,
                Title = p.title,
                Updated = p.GetUpdateTime(),
            });

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
        public IActionResult GetConversationHtml(string id)
        {
            var content = GetContent(id, ExportType.Html);
            return Content(content, "text/html");
        }

        /// <summary>
        /// Returns the conversation in the Markdown format.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [HttpGet("{id}/markdown")]
        [Produces("text/markdown")]
        public IActionResult GetConversationMarkdown(string id)
        {
            var content = GetContent(id, ExportType.Markdown);
            return Content(content, "text/markdown");
        }

        /// <summary>
        /// Returns the conversation in the Markdown format.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [HttpGet("{id}/json")]
        [Produces("application/json")]
        public IActionResult GetConversationJson(string id)
        {
            var content = GetContent(id, ExportType.Json);
            return Content(content, "application/json");
        }

        private string GetContent(string id, ExportType exportType)
        {
            var formatter = new ConversationFormatterFactory().GetFormatters([exportType], HtmlFormat.Tailwind, false);
            var conversation = conversationsService.GetConversation(id);
            var formatted = formatter.First().Format(new TempAssetLocator(), conversation.GetLastestConversation());
            string content = string.Join(Environment.NewLine, formatted);
            return content;
        }
    }

    public class ConversationSummary
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string GizmoId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
    }
}

