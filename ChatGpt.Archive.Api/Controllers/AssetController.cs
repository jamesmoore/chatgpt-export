using ChatGpt.Archive.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssetController(IConversationAssetsCache conversationAssets) : ControllerBase
    {
        [HttpGet("{rootId}/{**path}")]
        public IActionResult Index(int rootId, string path, [FromQuery(Name = "sig")] string? signature)
        {
            if (!AssetSignature.IsValid(rootId, path, signature))
                return Unauthorized();

            var fullPath = conversationAssets.GetMediaAssetPath(rootId, path);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(fullPath, GetMimeType(fullPath));
        }

        private static string GetMimeType(string path)
        {
            var ext = System.IO.Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
                return "application/octet-stream";

            switch (ext.ToLowerInvariant())
            {
                case ".txt": return "text/plain";
                case ".html":
                case ".htm": return "text/html";
                case ".css": return "text/css";
                case ".js": return "application/javascript";
                case ".json": return "application/json";
                case ".xml": return "application/xml";
                case ".csv": return "text/csv";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".svg": return "image/svg+xml";
                case ".webp": return "image/webp";
                case ".ico": return "image/x-icon";
                case ".bmp": return "image/bmp";
                case ".pdf": return "application/pdf";
                case ".zip": return "application/zip";
                case ".gz": return "application/gzip";
                case ".mp3": return "audio/mpeg";
                case ".wav": return "audio/wav";
                case ".mp4": return "video/mp4";
                case ".webm": return "video/webm";
                case ".woff": return "font/woff";
                case ".woff2": return "font/woff2";
                case ".ttf": return "font/ttf";
                case ".otf": return "font/otf";
                default: return "application/octet-stream";
            }
        }
    }
}