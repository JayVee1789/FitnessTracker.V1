using System.Collections.Generic;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammePredefini
{
    public static class ProgrammePredefiniStrategy
    {
        public static List<string> ProgrammesDisponibles => new()
        {
            "wageningen"
        };

        public static List<ProgrammePredefiniInfo> GetProgrammesInfos() => new()
        {
            new ProgrammePredefiniInfo("Wageningen", 6, "Remise en forme générale pour senior")
        };

        public static WorkoutPlan GetProgrammeByName(string nom)
        {
            return nom.ToLower() switch
            {
                "wageningen" => new ProgrammeWageningen().GeneratePlan(),
                _ => throw new System.Exception($"Programme inconnu : {nom}")
            };
        }

        public record ProgrammePredefiniInfo(string Nom, int Duree, string Objectif);
    }
}
