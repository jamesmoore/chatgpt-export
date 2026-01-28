using ChatGPTExport;
using ChatGPTExport.Formatters.Html;
using ChatGPTExport.Validators;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;

FileSystem fileSystem = new();
ConversationFinder conversationFinder = new ConversationFinder();

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.CancelKeyPress += (sender, args) =>
{
    args.Cancel = true;
    ConsoleFeatures.ClearState();
    Environment.Exit(0);
};

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
        if (directoryInfos != null)
        {
            foreach (var directoryInfo in directoryInfos)
            {
                var conversations = conversationFinder.GetConversationFiles(fileSystem.DirectoryInfo.Wrap(directoryInfo));
                if (conversations.Any() == false)
                {
                    result.AddError($"Source directory does not have a conversations.json file.");
                }
            }
        }
        else
        {
            result.AddError($"Source directory absent.");
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

var showHiddenOption = new Option<bool>("--showhidden")
{
    Description = "Inclues hidden content (thinking, web searches etc.) in markdown and html.",
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
    showHiddenOption,
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

        var sourceDirs = parseResult.GetRequiredValue(sourceDirectoryOption);
        var destinationDir = parseResult.GetRequiredValue(destinationDirectoryOption);
        var exportMode = parseResult.GetRequiredValue(exportModeOption);
        var validate = parseResult.GetRequiredValue(validateOption);
        var json = parseResult.GetRequiredValue(jsonOption);
        var markdown = parseResult.GetRequiredValue(markdownOption);
        var html = parseResult.GetRequiredValue(htmlOption);
        var htmlFormat = parseResult.GetRequiredValue(htmlFormatOption);
        var showHidden = parseResult.GetRequiredValue(showHiddenOption);


        var destination = fileSystem.DirectoryInfo.Wrap(destinationDir);
        var sources = sourceDirs.Select(p => fileSystem.DirectoryInfo.Wrap(p)).ToArray();

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

        var exportTypes = new List<ExportType>();
        if (html) exportTypes.Add(ExportType.Html);
        if (json) exportTypes.Add(ExportType.Json);
        if (markdown) exportTypes.Add(ExportType.Markdown);

        ExportArgs exportArgs = new(
            sources,
            destination,
            exportMode,
            exportTypes,
            htmlFormat,
            showHidden);
        var formatters = new ConversationFormatterFactory().GetFormatters(exportArgs);

        var validators = new List<IConversationsValidator>();
        if (validate)
        {
            validators.Add(new ConversationsJsonSchemaValidator());
            validators.Add(new ConversationsContentTypeValidator());
        }
        var conversationFiles = conversationFinder.GetConversationFiles(sources);
        var result = new ExportBootstrap(
            fileSystem,
            formatters,
            new ConversationsParser(validators)).RunExport(exportArgs, conversationFiles);
        return result;
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

