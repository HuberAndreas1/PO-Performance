namespace AppServices.Importer;

#region CyberLift Parse Exception

public enum CyberLiftParseError
{
    EmptyFile,
    MissingSessionHeader,
    InvalidSessionDateFormat,
    FutureDateError,
    MultipleSessionsInFile,
    ExerciseNameTooShort,
    ExerciseNameTooLong,
    DuplicateExerciseInSession,
    MissingExerciseName,
    EmptyExercise,
    InvalidSetFormat,
    InvalidWeightFormat,
    InvalidRepsFormat,
    InvalidSetSequence,
    NegativeValueDetected,
    CommentaryTooLong
}

public class CyberLiftParseException(CyberLiftParseError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<CyberLiftParseError, string> ErrorMessages = new()
    {
        { CyberLiftParseError.EmptyFile, "The file has no content." },
        { CyberLiftParseError.MissingSessionHeader, "The first non-empty line does not start with ###." },
        { CyberLiftParseError.InvalidSessionDateFormat, "The ### date is malformed or invalid." },
        { CyberLiftParseError.FutureDateError, "The session date is in the future." },
        { CyberLiftParseError.MultipleSessionsInFile, "A second ### header is found. Only one session per file is allowed." },
        { CyberLiftParseError.ExerciseNameTooShort, "Exercise name is less than 3 characters." },
        { CyberLiftParseError.ExerciseNameTooLong, "Exercise name exceeds 100 characters." },
        { CyberLiftParseError.DuplicateExerciseInSession, "The same exercise name appears twice within the file." },
        { CyberLiftParseError.MissingExerciseName, "A set appears before an exercise is defined." },
        { CyberLiftParseError.EmptyExercise, "An exercise name is defined, but no sets follow before the next exercise begins or the file ends." },
        { CyberLiftParseError.InvalidSetFormat, "Set line is missing the '.' or the 'x' separator." },
        { CyberLiftParseError.InvalidWeightFormat, "Weight is missing 'kg' or isn't a valid number." },
        { CyberLiftParseError.InvalidRepsFormat, "Repetition count is not a valid number." },
        { CyberLiftParseError.InvalidSetSequence, "Set index is not exactly 1 higher than the previous." },
        { CyberLiftParseError.NegativeValueDetected, "Weight or Reps are less than or equal to 0." },
        { CyberLiftParseError.CommentaryTooLong, "Text after the '|' exceeds 250 characters." }
    };

    public CyberLiftParseError ErrorCode { get; } = errorCode;
}

#endregion

#region Parser DTOs

/// <summary>
/// Represents a parsed exercise with its sets from the log file.
/// </summary>
public record ParsedExercise(string Name, List<ParsedSet> Sets);

/// <summary>
/// Represents a single parsed set from the log file.
/// </summary>
public record ParsedSet(int Index, double Weight, double Reps, string? Commentary);

#endregion

#region Parser Interface & Implementation

/// <summary>
/// Interface for parsing CyberLift log file content.
/// See file-format-spec.md for the full specification.
/// </summary>
public interface ICyberLiftLogParser
{
    /// <summary>
    /// Parses a CyberLift log file content string and returns the parsed session data.
    /// Must validate every line and throw <see cref="CyberLiftParseException"/> if the format is violated.
    /// </summary>
    /// <param name="fileContent">Raw content of the CyberLift log file</param>
    /// <returns>Parsed session date, and a list of exercises with their sets</returns>
    (DateTime sessionDate, List<ParsedExercise> exercises) Parse(string fileContent);
}

/// <summary>
/// TODO: Implement the CyberLift log parser.
/// </summary>
public class CyberLiftLogParser : ICyberLiftLogParser
{
    public (DateTime sessionDate, List<ParsedExercise> exercises) Parse(string fileContent)
    {
        throw new NotImplementedException();
    }
}

#endregion
