using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic.Fitness;
using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic.Mobility;
using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic.Pilates;
using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic.Yoga;
using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeSpecial;
using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic
{
    public class ProgrammeGeneratorService
    {
        private readonly Dictionary<string, IProgrammeStrategy> _strategies = new();
        public ProgrammeGeneratorService()
        {
            RegisterStrategy(new TbtProgrammeStrategy());
            RegisterStrategy(new PushPullProgrammeStrategy());
            RegisterStrategy(new Split5ProgrammeStrategy());
            RegisterStrategy(new GluteFocusProgrammeStrategy());
            RegisterStrategy(new FatLossProgrammeStrategy());
            RegisterStrategy(new StrengthProgrammeStrategy());
            RegisterStrategy(new MobilityProgrammeStrategy());  // <-- nouveau
            RegisterStrategy(new YogaProgrammeStrategy());      // <-- nouveau
            RegisterStrategy(new PilatesProgrammeStrategy());
            RegisterStrategy(new YogaAdvancedProgrammeStrategy());
            RegisterStrategy(new StrengthAdvancedProgrammeStrategy());
            RegisterStrategy(new MobilityAdvancedProgrammeStrategy());
            RegisterStrategy(new PilatesAdvancedProgrammeStrategy());
            RegisterStrategy(new WaterburyProgrammeStrategy());
            RegisterStrategy(new BradPittSnatchProgrammeStrategy());
            RegisterStrategy(new DrakeProgrammeStrategy());
            RegisterStrategy(new MeganStallionProgrammeStrategy());
            RegisterStrategy(new RockProgrammeStrategy());
            RegisterStrategy(new ArnoldProgrammeStrategy());
            RegisterStrategy(new CalisthenicsProgrammeStrategy());
            RegisterStrategy(new WageningenProgramme());
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
        // Sélectionne la meilleure stratégie en fonction des infos du profil.
       

    }
}
