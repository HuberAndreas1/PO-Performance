namespace AppServices.Importer;

/// <summary>
/// Interface for the full import pipeline: file read → parse → business logic → DB write.
/// </summary>
public interface ICyberLiftImporter
{
    /// <summary>
    /// Imports a CyberLift log file into the database.
    /// </summary>
    /// <param name="logFilePath">Path to the CyberLift log file</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of set records imported</returns>
    Task<int> ImportAsync(string logFilePath, bool isDryRun = false);
}

/// <summary>
/// Orchestrates the full import pipeline: file read → parse → 1RM calculation → DB write.
/// </summary>
public class CyberLiftImporter(
    IFileReader fileReader,
    ICyberLiftLogParser logParser,
    IImportDatabaseWriter databaseWriter,
    IBusinessLogic businessLogic) : ICyberLiftImporter
{
    public async Task<int> ImportAsync(string logFilePath, bool isDryRun = false)
    {
        // 1. Read the file
        var fileContent = await fileReader.ReadAllTextAsync(logFilePath);

        // 2. Parse the log file content (throws CyberLiftParseException on invalid format)
        var (sessionDate, parsedExercises) = logParser.Parse(fileContent);

        // 3. Begin transaction
        await databaseWriter.BeginTransactionAsync();

        try
        {
            // 4. Create the training session
            var session = await databaseWriter.CreateSessionAsync(sessionDate);

            // 5. Process each exercise and its sets
            var setRecords = new List<SetRecord>();

            foreach (var parsedExercise in parsedExercises)
            {
                var exercise = await databaseWriter.GetOrCreateExerciseAsync(parsedExercise.Name);

                foreach (var parsedSet in parsedExercise.Sets)
                {
                    var oneRepMax = businessLogic.CalculateOneRepMax(parsedSet.Weight, parsedSet.Reps);

                    setRecords.Add(new SetRecord
                    {
                        SessionId = session.SessionId,
                        ExerciseId = exercise.ExerciseId,
                        Weight = parsedSet.Weight,
                        Reps = parsedSet.Reps,
                        Commentary = parsedSet.Commentary,
                        Calculated1RM = oneRepMax,
                        IsPlateau = false // TODO: Implement plateau detection here (see README Chapter 4.2)
                    });
                }
            }

            // 6. Write all set records to the database
            await databaseWriter.WriteSetRecordsAsync(setRecords);

            // 7. Commit or rollback depending on dry-run mode
            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return setRecords.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }
}
