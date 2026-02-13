namespace AppServicesTests;

using AppServices;

public class BusinessLogicTests
{
    private readonly BusinessLogic logic = new();

    // ========================
    // 1RM CALCULATION TESTS — Epley Formula: Weight × (1 + Reps / 30)
    // ========================

    [Fact]
    public void CalculateOneRepMax_85kg_10Reps_ReturnsCorrectValue()
    {
        // 85 × (1 + 10/30) = 85 × 1.3333... ≈ 113.33
        var result = logic.CalculateOneRepMax(85.0, 10.0);
        Assert.Equal(113.33, result, 2);
    }

    [Fact]
    public void CalculateOneRepMax_ZeroReps_ReturnsWeight()
    {
        // 100 × (1 + 0/30) = 100
        var result = logic.CalculateOneRepMax(100.0, 0.0);
        Assert.Equal(100.0, result, 2);
    }

    [Fact]
    public void CalculateOneRepMax_OneRep_ReturnsSlightlyAboveWeight()
    {
        // 50 × (1 + 1/30) ≈ 51.667
        var result = logic.CalculateOneRepMax(50.0, 1.0);
        Assert.Equal(51.67, result, 2);
    }

    [Fact]
    public void CalculateOneRepMax_DecimalWeight_ReturnsCorrectValue()
    {
        // 11.25 × (1 + 8/30) = 11.25 × 1.2667 ≈ 14.25
        var result = logic.CalculateOneRepMax(11.25, 8.0);
        Assert.Equal(14.25, result, 2);
    }

    [Theory]
    [InlineData(80.0, 9.0, 104.0)]      // 80 × 1.3 = 104
    [InlineData(60.0, 12.0, 84.0)]       // 60 × 1.4 = 84
    [InlineData(100.0, 5.0, 116.67)]     // 100 × 1.1667 ≈ 116.67
    [InlineData(40.0, 15.0, 60.0)]       // 40 × 1.5 = 60
    [InlineData(200.0, 3.0, 220.0)]      // 200 × 1.1 = 220
    public void CalculateOneRepMax_VariousCombinations(double weight, double reps, double expected)
    {
        var result = logic.CalculateOneRepMax(weight, reps);
        Assert.Equal(expected, result, 2);
    }

    [Fact]
    public void CalculateOneRepMax_HighReps_ReturnsHighValue()
    {
        // 30 × (1 + 30/30) = 30 × 2 = 60
        var result = logic.CalculateOneRepMax(30.0, 30.0);
        Assert.Equal(60.0, result, 2);
    }

    // ========================
    // PLATEAU DETECTION TESTS
    // ========================

    [Fact]
    public void DetectPlateau_CurrentBelowThreshold_ReturnsTrue()
    {
        // README example: Avg 100, Threshold 101, Current 100.5 → plateau
        var previousMaxes = new List<double> { 100.0, 102.0, 98.0 };
        Assert.True(logic.DetectPlateau(100.5, previousMaxes));
    }

    [Fact]
    public void DetectPlateau_CurrentAboveThreshold_ReturnsFalse()
    {
        // Avg 100, Threshold 101, Current 110 → no plateau
        var previousMaxes = new List<double> { 100.0, 102.0, 98.0 };
        Assert.False(logic.DetectPlateau(110.0, previousMaxes));
    }

    [Fact]
    public void DetectPlateau_ExactlyAtThreshold_ReturnsTrue()
    {
        // Avg 100, Threshold 101, Current 101 → ≤ threshold → plateau
        var previousMaxes = new List<double> { 100.0, 100.0, 100.0 };
        Assert.True(logic.DetectPlateau(101.0, previousMaxes));
    }

    [Fact]
    public void DetectPlateau_JustAboveThreshold_ReturnsFalse()
    {
        // Avg 100, Threshold 101, Current 101.01 → above → no plateau
        var previousMaxes = new List<double> { 100.0, 100.0, 100.0 };
        Assert.False(logic.DetectPlateau(101.01, previousMaxes));
    }

    [Fact]
    public void DetectPlateau_LessThanThreeSessions_ReturnsFalse()
    {
        var twoSessions = new List<double> { 100.0, 102.0 };
        Assert.False(logic.DetectPlateau(50.0, twoSessions));
    }

    [Fact]
    public void DetectPlateau_ZeroPreviousSessions_ReturnsFalse()
    {
        Assert.False(logic.DetectPlateau(100.0, new List<double>()));
    }

    [Fact]
    public void DetectPlateau_OnePreviousSession_ReturnsFalse()
    {
        Assert.False(logic.DetectPlateau(100.0, new List<double> { 95.0 }));
    }

    [Fact]
    public void DetectPlateau_ExactlyThreeSessions_IsEvaluated()
    {
        // Avg 50, Threshold 50.5, Current 49 → plateau
        var previousMaxes = new List<double> { 50.0, 50.0, 50.0 };
        Assert.True(logic.DetectPlateau(49.0, previousMaxes));
    }

    [Fact]
    public void DetectPlateau_SignificantImprovement_ReturnsFalse()
    {
        // Avg 80, Threshold 80.8, Current 95 → big improvement → no plateau
        var previousMaxes = new List<double> { 75.0, 80.0, 85.0 };
        Assert.False(logic.DetectPlateau(95.0, previousMaxes));
    }

    [Fact]
    public void DetectPlateau_CurrentEqualToAverage_ReturnsTrue()
    {
        // Avg 100, Threshold 101, Current 100 → below threshold → plateau
        var previousMaxes = new List<double> { 100.0, 100.0, 100.0 };
        Assert.True(logic.DetectPlateau(100.0, previousMaxes));
    }
}
