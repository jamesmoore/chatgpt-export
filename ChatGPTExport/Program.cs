using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using ChatGPTExport;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var sourceDirectoryOption = new Option<DirectoryInfo>("--source", "-s")
{
    Description = "The source directory containing the unzipped ChatGTP exported files. Must have conversations.json.",
    Required = true,
}.AcceptExistingOnly();
sourceDirectoryOption.Validators.Add(result =>
{
    try
    {
        var directoryInfo = result.GetValue(sourceDirectoryOption);
        if (directoryInfo.GetFiles("conversations.json").Length == 0)
        {
            result.AddError($"Source directory does not have a conversations.json file.");
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

    var fileSystem = new System.IO.Abstractions.FileSystem();
    var source = fileSystem.DirectoryInfo.Wrap(parseResult.GetRequiredValue(sourceDirectoryOption));
    var destination = fileSystem.DirectoryInfo.Wrap(parseResult.GetRequiredValue(destinationDirectoryOption));
    new Exporter(source, fileSystem).Process(destination);
    return 0;
});

var parseResult = rootCommand.Parse(args);
return parseResult.Invoke();
