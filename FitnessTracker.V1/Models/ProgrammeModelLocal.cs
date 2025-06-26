using System.Text.Json.Serialization;

namespace FitnessTracker.V1.Models
{
    public class ProgrammeModelLocal
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("nom")]
        public string Nom { get; set; } = "";

        [JsonPropertyName("date_debut")]
        public DateTime DateDebut { get; set; }

        [JsonPropertyName("contenu")]
        public string Contenu { get; set; } = "";

        [JsonPropertyName("source")]
        public string Source { get; set; } = "auto";
    }
}
