using System.ComponentModel.DataAnnotations;

namespace AppServices;

public class Exercise
{
    public int ExerciseId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string MuscleGroup { get; set; } = string.Empty;
}

public class TrainingSession
{
    public int SessionId { get; set; }

    public DateTime Date { get; set; }

    public List<SetRecord> Sets { get; set; } = [];
}

public class SetRecord
{
    public int SetRecordId { get; set; }

    public int SessionId { get; set; }
    public TrainingSession Session { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public double Weight { get; set; }

    public double Reps { get; set; }

    [MaxLength(250)]
    public string? Commentary { get; set; }

    public double Calculated1RM { get; set; }

    public bool IsPlateau { get; set; }
}
