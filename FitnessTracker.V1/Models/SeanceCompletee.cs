using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace FitnessTracker.V1.Models
{
    [Table("seances_completees")]
    public class SeanceCompletee : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [Column("programme_id")]
        [JsonPropertyName("programme_id")]
        public Guid ProgrammeId { get; set; }

        [Column("semaine_index")]
        [JsonPropertyName("semaine_index")]
        public int SemaineIndex { get; set; }

        [Column("jour_index")]
        [JsonPropertyName("jour_index")]
        public int JourIndex { get; set; }

        [Column("completed_at")]
        [JsonPropertyName("completed_at")]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
