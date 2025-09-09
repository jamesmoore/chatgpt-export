using System.Runtime.InteropServices;

namespace ChatGTPExportTests
{
    public class WindowsOnlyFactAttribute : FactAttribute
    {
        public WindowsOnlyFactAttribute()
        {
            // Apply the OSPlatform constraint here
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Skip = "This test is only for Windows OS";
            }
        }
    }
}
