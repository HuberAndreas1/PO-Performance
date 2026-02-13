using Importer;

namespace ImporterTests;

public class CommandLineParserTests
{
    private readonly CommandLineParser parser = new();

    [Fact]
    public void Parse_ValidArguments_ReturnsCorrectResult()
    {
        // Arrange
        var args = new[] { "session.txt" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("session.txt", result.LogFilePath);
        Assert.False(result.IsDryRun);
    }

    [Fact]
    public void Parse_WithDryRunFlag_ReturnsDryRunTrue()
    {
        // Arrange
        var args = new[] { "session.txt", "--dry-run" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("session.txt", result.LogFilePath);
        Assert.True(result.IsDryRun);
    }

    [Fact]
    public void Parse_NoArguments_ThrowsArgumentException()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => parser.Parse(args));
        Assert.Contains("provide a log file path", exception.Message);
        Assert.Contains("Usage:", exception.Message);
    }

    [Fact]
    public void Parse_DryRunFlagInMiddle_ReturnsDryRunTrue()
    {
        // Arrange
        var args = new[] { "session.txt", "--dry-run", "extra" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("session.txt", result.LogFilePath);
        Assert.True(result.IsDryRun);
    }

    [Fact]
    public void Parse_WithInvalidFlag_IgnoresIt()
    {
        // Arrange
        var args = new[] { "session.txt", "--invalid-flag" };

        // Act
        var result = parser.Parse(args);

        // Assert
        Assert.Equal("session.txt", result.LogFilePath);
        Assert.False(result.IsDryRun);
    }

    [Fact]
    public void Parse_DryRunCaseSensitive_OnlyLowercaseWorks()
    {
        // Arrange
        var argsUpperCase = new[] { "session.txt", "--DRY-RUN" };
        var argsMixedCase = new[] { "session.txt", "--Dry-Run" };

        // Act
        var resultUpperCase = parser.Parse(argsUpperCase);
        var resultMixedCase = parser.Parse(argsMixedCase);

        // Assert
        Assert.False(resultUpperCase.IsDryRun);
        Assert.False(resultMixedCase.IsDryRun);
    }
}
