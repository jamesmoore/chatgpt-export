using ChatGPTExport.Models;
using System.IO;

namespace ChatGPTExport.Validators
{
    internal interface IConversationsValidator
    {
        bool Validate(Stream stream, Conversations conversations);
    }
}