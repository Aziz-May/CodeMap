using System.CommandLine;

using CodeMap.Analysis.CSharp;
using CodeMap.Cli;
var pathArgument = new Argument<string>("path")
{
    Description = "Folder containing the C# code to analyze",
};

var analyzeCommand = new Command("analyze", "Analyze a codebase and print its dependency map as JSON")
{
    pathArgument,
};

analyzeCommand.SetAction(parseResult =>
{
    // Required argument: System.CommandLine rejects the call before we get here if it's missing
    var path = parseResult.GetValue(pathArgument)!;

    var command = new AnalyzeCommand(new CSharpAnalyzer());
    return command.Run(path, Console.Out, Console.Error);
});

var rootCommand = new RootCommand("CodeMap: dependency maps for your codebase")
{
    analyzeCommand,
};

return rootCommand.Parse(args).Invoke();
