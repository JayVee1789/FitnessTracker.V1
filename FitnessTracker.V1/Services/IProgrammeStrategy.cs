using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services
{
    public interface IProgrammeStrategy
    {
        string Name { get; }
        WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> exercisePool);
    }
}
