using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using ChatGPTExport;
using ChatGPTExport.Exporters;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using ChatGPTExport.Exporters.HtmlTemplate;
using ChatGPTExport.Validators;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.CancelKeyPress += (sender, args) =>
{
    args.Cancel = true;
    ConsoleFeatures.ClearState();
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

var exportModeOption = new Option<ExportMode>("-e", "--export")
{
    Description = "Export mode.",
    Required = false,
    DefaultValueFactory = (ArgumentResult ar) => ExportMode.Latest,
};

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

var htmlOption = new Option<bool>("--html")
{
    Description = "Export to html files.",
    Required = false,
    DefaultValueFactory = (ArgumentResult ar) => true,
};

var htmlFormatOption = new Option<HtmlFormat>("-hf", "--htmlformat")
{
    Description = "Specify format for html exports.",
    Required = false,
    DefaultValueFactory = (ArgumentResult ar) => HtmlFormat.Tailwind,
};

var validateOption = new Option<bool>("--validate")
{
    Description = "Validate the json against the known and expected schema.",
    Required = false,
    DefaultValueFactory = (ArgumentResult ar) => false,
};

var rootCommand = new RootCommand("ChatGPT export reformatter")
{
    sourceDirectoryOption,
    destinationDirectoryOption,
    exportModeOption,
    jsonOption,
    markdownOption,
    htmlOption,
    htmlFormatOption,
    validateOption,
};


rootCommand.SetAction(parseResult =>
{
    try
    {
        foreach (ParseError parseError in parseResult.Errors)
        {
            Console.Error.WriteLine(parseError.Message);
        }

        ConsoleFeatures.StartIndeterminate();

        var sourceDirectoryInfos = parseResult.GetRequiredValue(sourceDirectoryOption);
        var destinationDirectoryInfo = parseResult.GetRequiredValue(destinationDirectoryOption);
        var exportMode = parseResult.GetRequiredValue(exportModeOption);
        var validate = parseResult.GetRequiredValue(validateOption);
        var json = parseResult.GetRequiredValue(jsonOption);
        var markdown = parseResult.GetRequiredValue(markdownOption);
        var html = parseResult.GetRequiredValue(htmlOption);
        var htmlFormat = parseResult.GetRequiredValue(htmlFormatOption);

        var fileSystem = new FileSystem();

        var destination = fileSystem.DirectoryInfo.Wrap(destinationDirectoryInfo);
        var sources = sourceDirectoryInfos.Select(p => fileSystem.DirectoryInfo.Wrap(p));

        // check that destination is not the same as the source, or one of the source subdirectories
        foreach (var source in sources)
        {
            var isSameOrSubdirectory = source.IsSameOrSubdirectory(destination);
            if (isSameOrSubdirectory)
            {
                Console.Error.WriteLine($"Destination {destination} is the same or a subdirectory of the source {source}");
                return 1;
            }
        }

        var conversationFiles = sources.Select(p => p.GetFiles(searchPattern, SearchOption.AllDirectories)).SelectMany(s => s).ToList();

        var conversationsFactory = new ConversationsParser(fileSystem, validate);
        var exporters = new List<IExporter>();
        if (json)
        {
            exporters.Add(new JsonExporter());
        }
        if (markdown)
        {
            exporters.Add(new MarkdownExporter());
        }
        if (html)
        {
            var headerProvider = new CompositeHeaderProvider(
                [
                    new HighlightHeaderProvider(),
                    new MathjaxHeaderProvider(),
                ]
            );
            
            var formatter = htmlFormat == HtmlFormat.Bootstrap ? new BootstrapHtmlFormatter(headerProvider) as IHtmlFormatter : new TailwindHtmlFormatter(headerProvider);
            exporters.Add(new HtmlExporter(formatter));
        }

        var exporter = new Exporter(fileSystem, exporters, exportMode);

        bool validationFail = false;

        Conversations? GetConversations(IFileInfo p)
        {
            try
            {
                Console.WriteLine($"Loading conversation " + p.FullName);
                return conversationsFactory.GetConversations(p);
            }
            catch (ValidationException)
            {
                validationFail = true;
                return null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error parsing file: {p.FullName} {ex.Message}");
                return null;
            }
        }

        var existingAssetLocator = new ExistingAssetLocator(fileSystem, destination);

        var directoryConversationsMap = conversationFiles
            .Select(file => new
            {
                AssetLocator = new AssetLocator(fileSystem, file.Directory, destination, existingAssetLocator) as IAssetLocator,
                Conversations = GetConversations(file)
            })
            .Where(x => x.Conversations != null)
            .OrderBy(x => x.Conversations.GetUpdateTime())
            .ToList();

        if (validationFail)
        {
            throw new ApplicationException("Validation errors found");
        }

        var groupedByConversationId = directoryConversationsMap
            .SelectMany(entry => entry.Conversations, (entry, Conversation) => (entry.AssetLocator, Conversation))
            .GroupBy(x => x.Conversation.conversation_id)
            .OrderBy(p => p.Key).ToList();

        var count = groupedByConversationId.Count;
        var position = 0;
        foreach (var group in groupedByConversationId)
        {
            var percent = (int)(position++ * 100.0 / count);
            ConsoleFeatures.SetProgress(percent);
            exporter.Process(group, destination);
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
    }
    finally
    {
        ConsoleFeatures.ClearState();
    }
    return 0;

});

var parseResult = rootCommand.Parse(args);
return parseResult.Invoke();
