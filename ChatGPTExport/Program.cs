using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using ChatGPTExport;
using ChatGPTExport.Models;

Console.OutputEncoding = System.Text.Encoding.UTF8;

const string searchPattern = "conversations.json";

var sourceDirectoryOption = new Option<DirectoryInfo[]>("--source", "-s")
{
    Description = "The source directory containing the unzipped ChatGTP exported files.\nMust contain a conversations.json.\nYou can specify multiple source directories (eg, -s dir1 -s dir2), and they will be processed in sequence.",
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

var rootCommand = new RootCommand("ChatGPT export reformatter")
{
    sourceDirectoryOption,
    destinationDirectoryOption,
};


rootCommand.SetAction(parseResult =>
{
    foreach (ParseError parseError in parseResult.Errors)
    {
        Console.Error.WriteLine(parseError.Message);
    }

    var sourceDirectoryInfos = parseResult.GetRequiredValue(sourceDirectoryOption);
    var fileSystem = new FileSystem();
    var destination = fileSystem.DirectoryInfo.Wrap(parseResult.GetRequiredValue(destinationDirectoryOption));
    var sources = sourceDirectoryInfos.Select(p => fileSystem.DirectoryInfo.Wrap(p));

    var conversationFiles = sources.Select(p => p.GetFiles(searchPattern, SearchOption.AllDirectories)).SelectMany(s => s).ToList();

    var conversationsFactory = new ConversationsParser(fileSystem);
    var exporter = new Exporter(fileSystem);

    Conversations GetConversations(IFileInfo p)
    {
        try
        {
            return conversationsFactory.GetConversations(p);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing file: {p.FullName} {ex.Message}");
            return null;
        }
    }

    var conversationsList = conversationFiles.Select(p => new
    {
        FileInfo = p,
        Conversations = GetConversations(p)
    }
    ).Where(p => p.Conversations != null).OrderBy(p => p.Conversations.GetUpdateTime()).ToList();

    foreach (var conversations in conversationsList)
    {
        exporter.Process(conversations.FileInfo, conversations.Conversations, destination);
    }
    return 0;
});

var parseResult = rootCommand.Parse(args);
return parseResult.Invoke();
