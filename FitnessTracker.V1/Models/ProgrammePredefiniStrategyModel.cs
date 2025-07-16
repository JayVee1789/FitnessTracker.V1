using static FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammePredefini.ProgrammePredefiniStrategy;

namespace FitnessTracker.V1.Models
{
    public class ProgrammePredefiniStrategyModel
    {
        public static List<ProgrammePredefiniInfo> GetProgrammesInfosPredefini() => new()
        {
                 new("Wageningen", 6, "Remise en forme générale"),
        };
    }
}
