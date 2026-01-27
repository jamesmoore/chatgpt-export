using ChatGPTExport.Models;
using System.IO;

namespace ChatGPTExport.Validators
{
    internal interface IConversationsValidator
    {
        bool Validate(Stream text, Conversations conversations);
    }
}