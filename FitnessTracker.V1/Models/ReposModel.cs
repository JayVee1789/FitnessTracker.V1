using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FitnessTracker.V1.Models
{
    [Table("reposmodel")]
    public class ReposModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("userid")]
        public string UserId { get; set; } = "";

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("activite")]
        public string Activite { get; set; } = "";

        [Column("dureeminutes")]
        public int DureeMinutes { get; set; }
    }
}
