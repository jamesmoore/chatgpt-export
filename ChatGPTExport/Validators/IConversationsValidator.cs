using ChatGPTExport.Models;
using System.IO;

namespace ChatGPTExport.Validators
{
    public interface IConversationsValidator
    {
        bool Validate(Stream stream, Conversations conversations);
    }
}