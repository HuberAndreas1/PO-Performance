using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace AppServicesTests;

public class DatabaseTestsWithClassFixture(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CanAddAndRetrieveExercise()
    {
        // Arrange & Act
        int exerciseId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var exercise = new Exercise
            {
                Name = "Chest Press",
                MuscleGroup = "Chest"
            };
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();
            exerciseId = exercise.ExerciseId;
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var exercise = await context.Exercises.FindAsync(exerciseId);
            Assert.NotNull(exercise);
            Assert.Equal("Chest Press", exercise.Name);
            Assert.Equal("Chest", exercise.MuscleGroup);
        }
    }

    [Fact]
    public async Task CanAddSessionWithSetRecords()
    {
        // Arrange
        int sessionId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var exercise = new Exercise { Name = "Preacher Curls", MuscleGroup = "Biceps" };
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            var session = new TrainingSession { Date = new DateTime(2026, 2, 6) };
            context.TrainingSessions.Add(session);
            await context.SaveChangesAsync();

            var setRecord = new SetRecord
            {
                SessionId = session.SessionId,
                ExerciseId = exercise.ExerciseId,
                Weight = 11.25,
                Reps = 8,
                Commentary = "easy",
                Calculated1RM = 14.25,
                IsPlateau = false
            };
            context.SetRecords.Add(setRecord);
            await context.SaveChangesAsync();
            sessionId = session.SessionId;
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var session = await context.TrainingSessions
                .Include(s => s.Sets)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
            Assert.NotNull(session);
            Assert.Single(session.Sets);
            Assert.Equal(11.25, session.Sets[0].Weight);
            Assert.Equal("easy", session.Sets[0].Commentary);
        }
    }

    [Fact]
    public async Task ExerciseNameIsUnique()
    {
        // Arrange
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            context.Exercises.Add(new Exercise { Name = "Unique Test Exercise", MuscleGroup = "Test" });
            await context.SaveChangesAsync();

            // Act & Assert — adding another exercise with the same name should fail
            context.Exercises.Add(new Exercise { Name = "Unique Test Exercise", MuscleGroup = "Test" });
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }
}
