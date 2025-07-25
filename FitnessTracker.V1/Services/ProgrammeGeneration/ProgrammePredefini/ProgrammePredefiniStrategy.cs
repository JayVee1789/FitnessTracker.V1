using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammePredefini
{
    public static class ProgrammePredefiniStrategy
    {
        public static List<string> ProgrammesDisponibles => new()
        {
            "wageningen",
            "10x3"
        };

        public static List<ProgrammePredefiniInfo> GetProgrammesInfos() => new()
        {
            new ProgrammePredefiniInfo("Wageningen", 6, "Remise en forme générale pour senior"),
            new ProgrammePredefiniInfo("10x3", 8, "Programme de force athlétique")
        };

        public static async Task<WorkoutPlan> GetProgrammeByNameAsync(string nom, HttpClient http, ILocalStorageService localStorage)
        {
            return nom.ToLower() switch
            {
                "wageningen" => new ProgrammeWageningen().GeneratePlan(),
                "10x3" => await new ProgrammeTnation(http, localStorage).GeneratePlanAsync(),
                _ => throw new System.Exception($"Programme inconnu : {nom}")
            };
        }

        public record ProgrammePredefiniInfo(string Nom, int Duree, string Objectif);
    }
}
