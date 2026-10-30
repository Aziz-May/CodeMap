using CodeMap.Cli; 
namespace CodeMap.Cli.Tests;
public sealed class AnalyzeCommandTests
{
    private static readonly string MissingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); // This line creates a random path that is unlikely to exist.
    [Fact]
    public void Run_PathDoesNotExist_ReturnsPathNotFound()
    {
        //Arrange
        using var output = new StringWriter(); 
        using var error = new StringWriter(); 
        var command = new AnalyzeCommand();
        //Act
        var exitCode = command.Run(MissingPath, output, error);
        //Assert
        Assert.Equal(ExitCodes.PathNotFound, exitCode);
    }
    [Fact]
    public void Run_PathDoesNotExist_WritesErrorMessage()
    {
        //Arrange
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new AnalyzeCommand();
        //Act
        command.Run(MissingPath, output, error);
        //Assert
        Assert.Contains(MissingPath, error.ToString());
    }

    [Fact]

    public void Run_PathDoesNotExist_WritesNothingToOutput()
    {
        //Arrange
        using var output = new StringWriter();
        using var error = new StringWriter();
        var command = new AnalyzeCommand();
        //Act
        command.Run(MissingPath, output, error);
        //Assert
        Assert.Empty(output.ToString());
    }
    
}