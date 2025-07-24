using System.Text.Json.Serialization;

namespace FitnessTracker.V1.Models
{
    public class BadgeModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "bi-award";

        [JsonPropertyName("emoji")]
        public string Emoji { get; set; } = ""; // 👈 NOUVEAU

        [JsonPropertyName("obtained")]
        public bool Obtained { get; set; } = false;

        [JsonPropertyName("obtained_at")]
        public DateTime? ObtainedAt { get; set; }
    }
}
