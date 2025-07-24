using System.Collections.Generic;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammePredefini
{
    public static class ProgrammePredefiniStrategy
    {
        public static List<string> ProgrammesDisponibles => new()
        {
            "wageningen",
               "tnation"
        };

        public static List<ProgrammePredefiniInfo> GetProgrammesInfos() => new()
        {
            new ProgrammePredefiniInfo("Wageningen", 6, "Remise en forme générale pour senior"),
         new ProgrammePredefiniInfo("tnation", 8, "Programme T-Nation sur 8 semaines")
        };

        public static WorkoutPlan GetProgrammeByName(string nom)
        {
            return nom.ToLower() switch
            {
                "wageningen" => new ProgrammeWageningen().GeneratePlan(),
                "tnation" => new ProgrammeTnation().GeneratePlan(),
                _ => throw new System.Exception($"Programme inconnu : {nom}")
            };
        }

        public record ProgrammePredefiniInfo(string Nom, int Duree, string Objectif);
    }
}
