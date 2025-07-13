using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic
{
    public class GluteFocusProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "GluteFocus";
        private readonly Random _rnd = new();

        #region === KEYWORDS & HELPERS ===

        private static readonly string[] GluteKeys =
            { "Glute", "Hip Thrust", "Bridge", "Kickback", "Deadlift", "Lunge", "Squat" };

        private static readonly string[] LowerBodyKeys =
            { "Quad", "Ham", "Leg", "Glute", "Hip", "Calf" };

        private static readonly string[] BodyweightTags =
            { "Bodyweight", "Body Weight", "Resistance Band", "Band", "Wall", "Floor", "Mat" };

        private static bool MatchAny(ExerciseDefinition ex, string[] keys) =>
            keys.Any(k =>
                ex.Category.Split('/', StringSplitOptions.TrimEntries)
                           .Any(c => c.Contains(k, StringComparison.OrdinalIgnoreCase))
             || ex.Name.Contains(k, StringComparison.OrdinalIgnoreCase));

        private static bool IsBodyweightFriendly(ExerciseDefinition ex) =>
            BodyweightTags.Any(tag => ex.Equipment.Contains(tag, StringComparison.OrdinalIgnoreCase));

        #endregion

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            int sessions = Math.Clamp(profile.SeancesPerWeek, 2, 5);

            // ——————————————————————————————————————
            // Filtrage en fonction de BodyweightOnly
            // ——————————————————————————————————————
            var sourcePool = profile.BodyweightOnly
                ? pool.Where(IsBodyweightFriendly).ToList()
                : pool;

            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int w = 1; w <= 8; w++)
            {
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 55 + w * 3
                };

                var used = new HashSet<int>();
                var pattern = BuildPattern(sessions);

                foreach (int d in Enumerable.Range(1, 7))
                {
                    if (!pattern.TryGetValue(d, out var cfg))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    var (type, gluteBias) = cfg;
                    int sets = gluteBias ? 4 : 3;
                    int reps = gluteBias ? 12 : 10;

                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = type };

                    var exos = sourcePool
                        .Where(e => MatchAny(e, gluteBias ? GluteKeys : LowerBodyKeys))
                        .Where(e => !used.Contains(e.Id))
                        .OrderBy(_ => _rnd.Next())
                        .Take(gluteBias ? 6 : 5)
                        .ToList();

                    foreach (var ex in exos)
                    {
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = sets,
                            Repetitions = reps,
                            RestTimeSeconds = 75,
                            IsSuperset = profile.WantsSuperset,
                            Pourcentage1RM = week.ChargeIncrementPercent
                        });
                        used.Add(ex.Id);
                    }

                    week.SeriesWeek = sets;
                    week.RepetitionsWeek = reps;
                    week.RestTimeWeek = 75;
                    week.Days.Add(day);
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }

        /// <summary>
        /// Renvoie la carte (jour -> (type, gluteBias)) pour 2 à 5 séances.
        /// </summary>
        private static Dictionary<int, (ProgrammeType, bool)> BuildPattern(int sessions) => sessions switch
        {
            2 => new()
            {
                { 1, (ProgrammeType.FullBody, true) },
                { 4, (ProgrammeType.FullBody, true) }
            },

            3 => new()
            {
                { 1, (ProgrammeType.FullBody, true) },
                { 3, (ProgrammeType.FullBody, false) },
                { 5, (ProgrammeType.FullBody, true) }
            },

            4 => new()
            {
                { 1, (ProgrammeType.FullBody, true) },
                { 3, (ProgrammeType.FullBody, false) },
                { 5, (ProgrammeType.FullBody, true) },
                { 6, (ProgrammeType.FullBody, false) }
            },

            5 => new()
            {
                { 1, (ProgrammeType.FullBody, true) },
                { 2, (ProgrammeType.FullBody, false) },
                { 4, (ProgrammeType.FullBody, true) },
                { 5, (ProgrammeType.FullBody, false) },
                { 6, (ProgrammeType.FullBody, true) }
            },

            _ => new()   // fallback 3 séances
            {
                { 1, (ProgrammeType.FullBody, true) },
                { 3, (ProgrammeType.FullBody, false) },
                { 5, (ProgrammeType.FullBody, true) }
            }
        };
    }
}
