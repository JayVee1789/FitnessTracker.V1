using BlazorBootstrap;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FitnessTracker.V1.Models.Gamification
{
    [Table("gamification")]
    public class GamificationDbModel : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("total_xp")]
        public int TotalXP { get; set; }

        [Column("streak_days")]
        public int StreakDays { get; set; }

        [Column("last_session_date")]
        public DateTime? LastSessionDate { get; set; }

        [Column("total_seances")]
        public int TotalSeances { get; set; }

        [Column("total_training_time_minutes")]
        public int TotalTrainingTimeMinutes { get; set; }

        [Column("total_calories_burned")]
        public decimal TotalCaloriesBurned { get; set; }

        [Column("best_lift_record")]
        public decimal BestLiftRecord { get; set; }

        [Column("best_walking_distance")]
        public decimal BestWalkingDistance { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // ✅ BadgeModel est maintenant mappé directement comme jsonb[]
        [Column("badges")]
        public List<BadgeModel> Badges { get; set; } = new();
    }
}
