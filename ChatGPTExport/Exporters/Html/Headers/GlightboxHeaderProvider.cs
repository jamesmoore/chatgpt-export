namespace ChatGPTExport.Exporters.Html.Headers
{
    internal class GlightboxHeaderProvider : IHeaderProvider
    {
        public string GetHeaders(HtmlPage htmlPage)
        {
            if (htmlPage.HasImage)
            {
                return """
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/glightbox/dist/css/glightbox.min.css">
                <script src="https://cdn.jsdelivr.net/npm/glightbox/dist/js/glightbox.min.js"></script>
                
                <style>
                img {
                  max-width: 300px;
                  max-height: 300px;
                  cursor: zoom-in;
                  object-fit: contain;
                }
                </style>
                
                <script>
                document.addEventListener("DOMContentLoaded", () => {
                  document.querySelectorAll("img").forEach(img => {
                    img.outerHTML = `
                      <a href="${img.src}" class="glightbox">
                        ${img.outerHTML}
                      </a>`;
                  });
                
                  GLightbox();
                });
                </script>
                """;

            }
            else
            {
                return null;
            }
        }
    }
}
