using CodeMap.Cli; 
using CodeMap.Analysis;
using CodeMap.Analysis.Model;
namespace CodeMap.Cli.Tests;
public sealed class AnalyzeCommandTests : IDisposable
{
    private static readonly string MissingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); // This line creates a random path that is unlikely to exist.
    private readonly string _tempDir;
    public AnalyzeCommandTests()
    {
        // This constructor is called before each test method is run.
        // it is used to set up any shared state or resources needed for the tests.
         _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        File.WriteAllText(Path.Combine(_tempDir, "Order.cs"), "class Order { }");
        File.WriteAllText(Path.Combine(_tempDir, "readme.md"), "# not code");
    }
    public void Dispose()
    {
        // This method is called after each test method is run.
        // it is used to clean up any shared state or resources used in the tests.
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive:true);
        }
    }
    private sealed class FakeAnalyzer : ILanguageAnalyzer
    {
        public string? ReceivedRootPath { get; private set; }
        public IReadOnlyList<string> ReceivedFiles { get; private set; } =[];
        public AnalysisResult Analyze(string rootPath, IReadOnlyList<string> files, CancellationToken cancellationToken = default)
        {
            ReceivedRootPath = rootPath;
            ReceivedFiles = files;
            return new AnalysisResult(
                [],
                [new CodeRelation("M:A.Run", "M:B.Go", "B.Go", RelationKind.Calls, Confidence.Certain,
                    new CodeLocation("A.cs", 3, 3))],
                []
            );
        }
    }
    [Fact]
    public void Run_PathDoesNotExist_ReturnsPathNotFound()
    {
        //Arrange
        var fake = new FakeAnalyzer();
        var command = new AnalyzeCommand(fake);
        using var output = new StringWriter();
        using var error = new StringWriter();
        //Act
        var exitCode = command.Run(MissingPath, output, error);
        //Assert
        Assert.Equal(ExitCodes.PathNotFound, exitCode);
    }
    [Fact]
    public void Run_PathDoesNotExist_WritesErrorMessage()
    {
        //Arrange
     var fake = new FakeAnalyzer();
        var command = new AnalyzeCommand(fake);
        using var output = new StringWriter();
        using var error = new StringWriter();
        //Act
        command.Run(MissingPath, output, error);
        //Assert
        Assert.Contains(MissingPath, error.ToString());
    }

    [Fact]

    public void Run_PathDoesNotExist_WritesNothingToOutput()
    {
        //Arrange
        var fake = new FakeAnalyzer();
        var command = new AnalyzeCommand(fake);
        using var output = new StringWriter();
        using var error = new StringWriter();
        //Act
        command.Run(MissingPath, output, error);
        //Assert
        Assert.Empty(output.ToString());
    }
    [Fact] 
        public void Run_RelativePath_PathIsResolvedToFullPath()
        {
            //Arrange 
            var fake = new FakeAnalyzer();
            var command = new AnalyzeCommand(fake);
            // This line creates a relative path from the current directory to the temporary directory.
            var relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempDir);
            using var output = new StringWriter();
            using var error = new StringWriter();
            //Act 
            command.Run(relativePath, output, error);
            //Assert
            Assert.Equal(_tempDir, fake.ReceivedRootPath);
        }
    
    [Fact]
    public void Run_ValidPath_ReturnsSuccess()
    {
        //Arrange
        var fake = new FakeAnalyzer();
        var command = new AnalyzeCommand(fake);
        using var output = new StringWriter();
        using var error = new StringWriter();
        //Act
        var exitCode = command.Run(_tempDir, output, error);
        //Assert
        Assert.Equal(ExitCodes.Success, exitCode);
    }
    [Fact]
    public void Run_ValidPath_PassesOnlyCsFilesToAnalyzer()
    {
        //Arrange
        var fake = new FakeAnalyzer();
        var command = new AnalyzeCommand(fake);
        using var output = new StringWriter();
        using var error = new StringWriter();
        //Act
        command.Run(_tempDir, output, error);
        //Assert
        
        var file = Assert.Single(fake.ReceivedFiles);
        Assert.EndsWith("Order.cs", file);
    }
    [Fact]
    public void Run_ValidPath_WritesResultAsJson()
    {
        //Arrange
        var fake = new FakeAnalyzer();
        var command = new AnalyzeCommand(fake);
        using var output = new StringWriter();
        using var error = new StringWriter();
        //Act
        command.Run(_tempDir, output, error);
        //Assert
        var jsonOutput = output.ToString();
        Assert.Contains("\"sourceId\": \"M:A.Run\"", jsonOutput);
        Assert.Contains("\"kind\": \"Calls\"", jsonOutput);

    }

    [Fact]
    public void Run_ValidPath_WritesNothingToError()
    {
        //Arrange
        var fake = new FakeAnalyzer();
        var command = new AnalyzeCommand(fake);
        using var output = new StringWriter();
        using var error = new StringWriter();
        //Act
        command.Run(_tempDir, output, error);
        //Assert
        Assert.Empty(error.ToString());
    }
    
}
