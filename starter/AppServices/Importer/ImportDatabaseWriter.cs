using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing imported workout data to the database.
/// </summary>
public interface IImportDatabaseWriter
{
    /// <summary>
    /// Finds an existing exercise by name or creates a new one.
    /// </summary>
    Task<Exercise> GetOrCreateExerciseAsync(string name);

    /// <summary>
    /// Creates a new training session in the database.
    /// </summary>
    Task<TrainingSession> CreateSessionAsync(DateTime date);

    /// <summary>
    /// Writes a collection of SetRecords to the database.
    /// </summary>
    Task WriteSetRecordsAsync(IEnumerable<SetRecord> setRecords);

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync();
}

/// <summary>
/// TODO: Implement the database writer for imported workout data.
/// </summary>
public class ImportDatabaseWriter(ApplicationDataContext context) : IImportDatabaseWriter
{
    private IDbContextTransaction? transaction;

    public Task<Exercise> GetOrCreateExerciseAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<TrainingSession> CreateSessionAsync(DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task WriteSetRecordsAsync(IEnumerable<SetRecord> setRecords)
    {
        throw new NotImplementedException();
    }

    public async Task BeginTransactionAsync()
    {
        transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.CommitAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }
}
