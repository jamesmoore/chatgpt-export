using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using ChatGPTExport;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;

var supportsOSCProgress = ConsoleFeatures.EnableOscTabProgress();

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.CancelKeyPress += (sender, args) =>
{
    args.Cancel = true;
    TerminateProgram();
    Environment.Exit(0);
};

const string searchPattern = "conversations.json";

var sourceDirectoryOption = new Option<DirectoryInfo[]>("--source", "-s")
{
    Description = """
    The source directory/directories containing the unzipped ChatGPT exported files.
    Must contain at least one conversations.json, in the folder or one of its subfolders.
    You can specify a parent directory containing multiple exports.
    You can also specify multiple source directories (eg, -s dir1 -s dir2), and they will be processed in sequence.
    """,
    Required = true,
}.AcceptExistingOnly();
sourceDirectoryOption.Validators.Add(result =>
{
    try
    {
        var directoryInfos = result.GetValue(sourceDirectoryOption);
        foreach (var directoryInfo in directoryInfos)
        {
            if (directoryInfo.GetFiles(searchPattern, SearchOption.AllDirectories).Length == 0)
            {
                result.AddError($"Source directory does not have a conversations.json file.");
            }
        }
    }
    catch { }
});

var destinationDirectoryOption = new Option<DirectoryInfo>("--destination", "-d")
{
    Description = "The the destination directory where markdown files and assets are to be created.",
    Required = true,
}.AcceptExistingOnly();

var jsonOption = new Option<bool>("--json", "-j")
{
    Description = "Export to json files.",
    Required = false,
    DefaultValueFactory = (ArgumentResult ar) => false,
};

var markdownOption = new Option<bool>("--markdown", "-m")
{
    Description = "Export to markdown files.",
    Required = false,
    DefaultValueFactory = (ArgumentResult ar) => true,
};

var rootCommand = new RootCommand("ChatGPT export reformatter")
{
    sourceDirectoryOption,
    destinationDirectoryOption,
    jsonOption,
    markdownOption,
};

rootCommand.SetAction(parseResult =>
{
    try
    {
        foreach (ParseError parseError in parseResult.Errors)
        {
            Console.Error.WriteLine(parseError.Message);
        }

        if (supportsOSCProgress)
        {
            Console.Write("\x1b]9;4;3\x07"); // https://learn.microsoft.com/en-us/windows/terminal/tutorials/progress-bar-sequences
        }

        var sourceDirectoryInfos = parseResult.GetRequiredValue(sourceDirectoryOption);
        var fileSystem = new FileSystem();
        var destination = fileSystem.DirectoryInfo.Wrap(parseResult.GetRequiredValue(destinationDirectoryOption));
        var sources = sourceDirectoryInfos.Select(p => fileSystem.DirectoryInfo.Wrap(p));

        var conversationFiles = sources.Select(p => p.GetFiles(searchPattern, SearchOption.AllDirectories)).SelectMany(s => s).ToList();

        var conversationsFactory = new ConversationsParser(fileSystem);
        var exporters = new List<IExporter>();
        if (parseResult.GetRequiredValue(jsonOption))
        {
            exporters.Add(new JsonExporter(fileSystem, destination));
        }
        if (parseResult.GetRequiredValue(markdownOption))
        {
            exporters.Add(new MarkdownExporter(fileSystem, destination));
        }
        var exporter = new Exporter(fileSystem, exporters);

        Conversations GetConversations(IFileInfo p)
        {
            try
            {
                Console.WriteLine($"Loading conversation " + p.FullName);
                return conversationsFactory.GetConversations(p);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error parsing file: {p.FullName} {ex.Message}");
                return null;
            }
        }

        var directoryConversationsMap = conversationFiles
            .Select(file => new
            {
                file.Directory,
                Conversations = GetConversations(file)
            })
            .Where(x => x.Conversations != null)
            .OrderBy(x => x.Conversations.GetUpdateTime())
            .ToList();

        var groupedByConversationId = directoryConversationsMap
            .SelectMany(entry => entry.Conversations, (entry, Conversation) => (entry.Directory, Conversation))
            .GroupBy(x => x.Conversation.conversation_id)
            .OrderBy(p => p.Key).ToList();

        var count = groupedByConversationId.Count;
        var position = 0;
        foreach (var group in groupedByConversationId)
        {
            if (supportsOSCProgress)
            {
                var percent = (int)(position++ * 100.0 / count);
                Console.Write($"\x1b]9;4;1;{percent}\x07");
            }
            exporter.Process(group, destination);
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
    }
    finally
    {
        TerminateProgram();
    }
    return 0;

});

var parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

void TerminateProgram()
{
    if (supportsOSCProgress)
    {
        Console.Write("\x1b]9;4;0;\x07");
    }
}