namespace FitnessTracker.V1.Models
{
    public class Model
    
        
    {
        // ================= ENUMS =================
        public enum TrainingObjective
        {
            Force,
            Endurance,
            Hypertrophy,
            Fat_loss,
            Yoga,
            Mobility,
            Pilates,
            Strength

        }

        public enum UserLevel
        {
            Debutant,
            Intermediaire,
            Avance
        }

        public enum ProgrammeType
        {
            FullBody,
            Split,
            Half,
            PushPull,
            Mixte,
            Rest,
            Mobility,
            Yoga,
            Pilates

        }

        // ================= MODELES =================

        public class ExerciseDefinition
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Category { get; set; } = "";
            public string Equipment { get; set; } = "";
            public string Description { get; set; } = ""; // "Compound" / "Single-Joint"
                                                          // Champs additionnels présents dans ton JSON
            public string? Origin { get; set; }
            public string? Type { get; set; }      // Certains objets ont ce champ
            public string? Muscle { get; set; }    // Même chose

        }

        public class UserProfile
        {
            public int Age { get; set; }
            public string Sexe { get; set; } = "";           // 👈 NOUVEAU
            public UserLevel Level { get; set; }
            public TrainingObjective Objective { get; set; }
            public int SeancesPerWeek { get; set; }
            public int ProgramDurationMonths { get; set; }
            public bool WantsSuperset { get; set; }
            public bool BodyweightOnly { get; set; }
            public string PathologieMuscle { get; set; } = "";
            public string PriorityMuscle { get; set; } = "";

            public int RotationCycleWeeks =>
                Level switch
                {
                    UserLevel.Debutant => 8,
                    UserLevel.Intermediaire => 4,
                    UserLevel.Avance => 2,
                    _ => 4
                };
        }

        public class WorkoutPlan
        {
            public int TotalWeeks { get; set; }
            public List<WorkoutWeek> Weeks { get; set; } = new();
        }

        public class WorkoutWeek
        {
            public int WeekNumber { get; set; }
            public double ChargeIncrementPercent { get; set; }
            public int SeriesWeek { get; set; }
            public int RepetitionsWeek { get; set; }
            public int RestTimeWeek { get; set; }
            public List<WorkoutDay> Days { get; set; } = new();
        }

        public class WorkoutDay
        {
            public int DayIndex { get; set; }
            public ProgrammeType TypeProgramme { get; set; }
            public bool IsRest => (TypeProgramme == ProgrammeType.Rest);
            public List<ExerciseSession> Exercises { get; set; } = new();
        }

        public class ExerciseSession
        {
            public int ExerciseId { get; set; }
            public string ExerciseName { get; set; } = "";
            public int Series { get; set; }
            public int Repetitions { get; set; }
            public int RestTimeSeconds { get; set; }
            public bool IsSuperset { get; set; }
            public double Pourcentage1RM { get; set; }
        }
    }

}
