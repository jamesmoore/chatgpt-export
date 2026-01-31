using Microsoft.AspNetCore.Mvc;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationsController : ControllerBase
    {
        /// <summary>
        /// Returns the latest conversation details for each instance.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult<IEnumerable<ConversationSummary>> Get()
        {
            List<ConversationSummary> value = [
                new ConversationSummary()
                {
                    Created = DateTime.Now.AddDays(-365),
                    GizmoId = "12345",
                    Id = "1092348203948",
                    Title = "Conversation 123",
                    Updated = DateTime.Now.AddDays(-364),
                },
            ];
            return Ok(value);
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
            return Content("SOME HTML FORMATTED CONTENT", "text/html");
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
            return Content("SOME MD FORMATTED CONTENT", "text/markdown");
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
            return Content("SOME JSON", "application/json");
        }
    }

    public class ConversationSummary
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string GizmoId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}

