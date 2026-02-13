using AppServices.Importer;

namespace ImporterTests;

public class CyberLiftLogParserTests
{
    private readonly CyberLiftLogParser parser = new();

    // ========================
    // VALID FILE TESTS
    // ========================

    [Fact]
    public void Parse_ValidFile_ReturnsCorrectSessionAndSets()
    {
        // Arrange — sample from file-format-spec.md
        var content = """
            ### 2026-02-06
            Chest Press

            1. 85kg x 10
            2. 80kg x 9
            Preacher Curls

            1. 11.25kg x 8
            2. 8.75kg x 9 | easy
            """;

        // Act
        var (sessionDate, exercises) = parser.Parse(content);

        // Assert
        Assert.Equal(new DateTime(2026, 2, 6), sessionDate);
        Assert.Equal(2, exercises.Count);

        Assert.Equal("Chest Press", exercises[0].Name);
        Assert.Equal(2, exercises[0].Sets.Count);
        Assert.Equal(85.0, exercises[0].Sets[0].Weight);
        Assert.Equal(10.0, exercises[0].Sets[0].Reps);
        Assert.Null(exercises[0].Sets[0].Commentary);

        Assert.Equal("Preacher Curls", exercises[1].Name);
        Assert.Equal(2, exercises[1].Sets.Count);
        Assert.Equal(8.75, exercises[1].Sets[1].Weight);
        Assert.Equal(9.0, exercises[1].Sets[1].Reps);
        Assert.Equal("easy", exercises[1].Sets[1].Commentary);
    }

    [Fact]
    public void Parse_ValidFile_SingleExerciseSingleSet()
    {
        var content = """
            ### 2025-01-15
            Bicep Curl
            1. 12.5kg x 10
            """;

        var (sessionDate, exercises) = parser.Parse(content);

        Assert.Equal(new DateTime(2025, 1, 15), sessionDate);
        Assert.Single(exercises);
        Assert.Equal("Bicep Curl", exercises[0].Name);
        Assert.Single(exercises[0].Sets);
        Assert.Equal(12.5, exercises[0].Sets[0].Weight);
        Assert.Equal(10.0, exercises[0].Sets[0].Reps);
    }

    [Fact]
    public void Parse_ValidFile_WithCommentaryOnMultipleSets()
    {
        var content = """
            ### 2025-06-01
            Deadlift
            1. 100kg x 5 | felt strong
            2. 110kg x 3 | grip slipped
            3. 120kg x 1 | PR attempt
            """;

        var (_, exercises) = parser.Parse(content);

        Assert.Single(exercises);
        Assert.Equal(3, exercises[0].Sets.Count);
        Assert.Equal("felt strong", exercises[0].Sets[0].Commentary);
        Assert.Equal("grip slipped", exercises[0].Sets[1].Commentary);
        Assert.Equal("PR attempt", exercises[0].Sets[2].Commentary);
    }

    // ========================
    // ERROR CASE TESTS
    // ========================

    [Fact]
    public void Parse_EmptyFile_ThrowsEmptyFileError()
    {
        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(""));
        Assert.Equal(CyberLiftParseError.EmptyFile, ex.ErrorCode);
    }

    [Fact]
    public void Parse_WhitespaceOnlyFile_ThrowsEmptyFileError()
    {
        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse("   \n  \n  "));
        Assert.Equal(CyberLiftParseError.EmptyFile, ex.ErrorCode);
    }

    [Fact]
    public void Parse_MissingSessionHeader_ThrowsError()
    {
        var content = """
            Chest Press
            1. 85kg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.MissingSessionHeader, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidSessionDateFormat_ThrowsError()
    {
        var content = """
            ### 06-02-2026
            Chest Press
            1. 85kg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidSessionDateFormat, ex.ErrorCode);
    }

    [Fact]
    public void Parse_FutureDate_ThrowsError()
    {
        var content = """
            ### 2099-12-31
            Chest Press
            1. 85kg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.FutureDateError, ex.ErrorCode);
    }

    [Fact]
    public void Parse_MultipleSessionHeaders_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85kg x 10
            ### 2025-01-02
            Bicep Curl
            1. 10kg x 12
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.MultipleSessionsInFile, ex.ErrorCode);
    }

    [Fact]
    public void Parse_ExerciseNameTooShort_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Ab
            1. 20kg x 15
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.ExerciseNameTooShort, ex.ErrorCode);
    }

    [Fact]
    public void Parse_ExerciseNameTooLong_ThrowsError()
    {
        var longName = new string('A', 101);
        var content = $"""
            ### 2025-01-01
            {longName}
            1. 20kg x 15
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.ExerciseNameTooLong, ex.ErrorCode);
    }

    [Fact]
    public void Parse_DuplicateExercise_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85kg x 10
            Chest Press
            1. 80kg x 9
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.DuplicateExerciseInSession, ex.ErrorCode);
    }

    [Fact]
    public void Parse_MissingExerciseName_SetBeforeExercise_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            1. 85kg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.MissingExerciseName, ex.ErrorCode);
    }

    [Fact]
    public void Parse_EmptyExercise_NoSetsBeforeNextExercise_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            Preacher Curls
            1. 11.25kg x 8
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.EmptyExercise, ex.ErrorCode);
    }

    [Fact]
    public void Parse_EmptyExercise_NoSetsBeforeEndOfFile_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85kg x 10
            Preacher Curls
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.EmptyExercise, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidSetFormat_MissingDot_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1 85kg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidSetFormat, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidSetFormat_MissingXSeparator_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85kg 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidSetFormat, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidWeightFormat_MissingKg_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85 x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidWeightFormat, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidWeightFormat_NotANumber_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. abckg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidWeightFormat, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidRepsFormat_NotANumber_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85kg x abc
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidRepsFormat, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidSetSequence_SkipsIndex_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85kg x 10
            3. 80kg x 9
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidSetSequence, ex.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidSetSequence_StartsAtTwo_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            2. 85kg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.InvalidSetSequence, ex.ErrorCode);
    }

    [Fact]
    public void Parse_NegativeWeight_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. -5kg x 10
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.NegativeValueDetected, ex.ErrorCode);
    }

    [Fact]
    public void Parse_ZeroReps_ThrowsError()
    {
        var content = """
            ### 2025-01-01
            Chest Press
            1. 85kg x 0
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.NegativeValueDetected, ex.ErrorCode);
    }

    [Fact]
    public void Parse_CommentaryTooLong_ThrowsError()
    {
        var longComment = new string('x', 251);
        var content = $"""
            ### 2025-01-01
            Chest Press
            1. 85kg x 10 | {longComment}
            """;

        var ex = Assert.Throws<CyberLiftParseException>(() => parser.Parse(content));
        Assert.Equal(CyberLiftParseError.CommentaryTooLong, ex.ErrorCode);
    }

    [Fact]
    public void Parse_CommentaryExactly250Chars_Succeeds()
    {
        var comment250 = new string('x', 250);
        var content = $"""
            ### 2025-01-01
            Chest Press
            1. 85kg x 10 | {comment250}
            """;

        var (_, exercises) = parser.Parse(content);
        Assert.Equal(comment250, exercises[0].Sets[0].Commentary);
    }
}
