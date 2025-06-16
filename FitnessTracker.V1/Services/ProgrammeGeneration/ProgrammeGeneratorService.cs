
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class ProgrammeGeneratorService
    {
        private readonly Dictionary<string, IProgrammeStrategy> _strategies = new();
        public ProgrammeGeneratorService()
        {
            RegisterStrategy(new TbtProgrammeStrategy());
        }
        public void RegisterStrategy(IProgrammeStrategy strategy)
        {
            _strategies[strategy.Name] = strategy;
        }

        public WorkoutPlan Generate(string strategyName, UserProfile profile, List<ExerciseDefinition> exos)
        {
            if (!_strategies.TryGetValue(strategyName, out var strategy))
                throw new InvalidOperationException($"Programme strategy '{strategyName}' non trouvée.");

            return strategy.GeneratePlan(profile, exos);
        }

        public List<string> GetAvailableStrategies() => _strategies.Keys.ToList();
    }
}
