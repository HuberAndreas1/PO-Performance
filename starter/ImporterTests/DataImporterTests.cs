using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class DataImporterTests
{
    private readonly IFileReader fileReader;
    private readonly ICyberLiftLogParser logParser;
    private readonly IImportDatabaseWriter databaseWriter;
    private readonly IBusinessLogic businessLogic;
    private readonly CyberLiftImporter importer;

    public DataImporterTests()
    {
        fileReader = Substitute.For<IFileReader>();
        logParser = Substitute.For<ICyberLiftLogParser>();
        databaseWriter = Substitute.For<IImportDatabaseWriter>();
        businessLogic = Substitute.For<IBusinessLogic>();
        importer = new CyberLiftImporter(fileReader, logParser, databaseWriter, businessLogic);
    }

    [Fact]
    public async Task ImportAsync_FileReaderThrows_Rethrows()
    {
        // Arrange
        var logFilePath = "test.txt";
        fileReader.ReadAllTextAsync(logFilePath).Throws(new FileNotFoundException("File not found"));

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await importer.ImportAsync(logFilePath));
    }

    [Fact]
    public async Task ImportAsync_ParserThrows_RollsBackAndRethrows()
    {
        // Arrange
        var logFilePath = "test.txt";
        var fileContent = "Invalid content";

        fileReader.ReadAllTextAsync(logFilePath).Returns(Task.FromResult(fileContent));
        logParser.Parse(fileContent).Throws(new CyberLiftParseException(CyberLiftParseError.EmptyFile));

        // Act & Assert
        await Assert.ThrowsAsync<CyberLiftParseException>(
            async () => await importer.ImportAsync(logFilePath));

        // Verify rollback was called
        await databaseWriter.Received(1).RollbackTransactionAsync();
    }
}
