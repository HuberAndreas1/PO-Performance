namespace AppServices;

/// <summary>
/// Interface for non-trivial business logic calculations.
/// </summary>
public interface IBusinessLogic
{
    /// <summary>
    /// Calculates the Estimated One-Rep Max using the Epley Formula.
    /// Formula: 1RM = Weight × (1 + Reps / 30)
    /// </summary>
    double CalculateOneRepMax(double weight, double reps);

    /// <summary>
    /// Determines if the user has plateaued based on their recent history.
    /// A plateau is detected when the current session max is ≤ (baseline average + 1%).
    /// Returns false if fewer than 3 previous sessions exist.
    /// </summary>
    /// <param name="currentSessionMax">The highest Calculated1RM in the current session</param>
    /// <param name="previousSessionMaxes">The Session Max 1RM values of the 3 most recent previous sessions</param>
    bool DetectPlateau(double currentSessionMax, List<double> previousSessionMaxes);
}

/// <summary>
/// TODO: Implement the business logic calculations.
/// </summary>
public class BusinessLogic : IBusinessLogic
{
    public double CalculateOneRepMax(double weight, double reps)
    {
        throw new NotImplementedException();
    }

    public bool DetectPlateau(double currentSessionMax, List<double> previousSessionMaxes)
    {
        throw new NotImplementedException();
    }
}
