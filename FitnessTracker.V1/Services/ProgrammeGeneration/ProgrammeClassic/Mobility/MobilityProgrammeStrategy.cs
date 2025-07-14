using FitnessTracker.V1.Models.Enumeration;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic.Mobility
{
    /// <summary>
    /// Programme Mobilité – 6 semaines – 3 séances (Lun-Mer-Ven)
    /// Progression : +1 exercice / 2 semaines.
    /// </summary>
    public class MobilityProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Mobility";

        private readonly Random _rnd = new();

        private static bool MatchAny(ExerciseDefinition ex, params string[] keys) =>
            keys.Any(k =>
                ex.Category.Split('/', StringSplitOptions.TrimEntries)
                           .Any(c => c.Contains(k, StringComparison.OrdinalIgnoreCase))
             || ex.Name.Contains(k, StringComparison.OrdinalIgnoreCase));

        private static readonly string[] DynamicKeys = { "Mobility", "Dynamic", "Plyometric", "CARs", "Wave" };
        private static readonly string[] StaticKeys = { "Stretch", "Isometric", "Pose", "Hold" };

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 6 };

            for (int w = 1; w <= 6; w++)
            {
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 0,          // non pertinent pour la mobilité
                    SeriesWeek = 2,
                    RepetitionsWeek = 8,
                    RestTimeWeek = 30
                };

                int exercisesPerDay = 6 + (w - 1) / 2;   // 6 → 8 sur le cycle

                foreach (int d in new[] { 1, 3, 5 })
                {
                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Mobility };

                    var dyn = Pick(pool, DynamicKeys, exercisesPerDay / 2);
                    var sta = Pick(pool, StaticKeys, exercisesPerDay - dyn.Count);

                    foreach (var ex in dyn.Concat(sta))
                    {
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = 2,
                            Repetitions = MatchAny(ex, StaticKeys) ? 30 : 8, // 30 s tenue ou 8 rep dynamiques
                            RestTimeSeconds = 20,
                            IsSuperset = true,
                            Pourcentage1RM = 0
                        });
                    }

                    week.Days.Add(day);
                }

                // Jours sans séance = repos léger
                week.Days.AddRange(Enumerable.Range(1, 7)
                    .Except(new[] { 1, 3, 5 })
                    .Select(i => new WorkoutDay { DayIndex = i, TypeProgramme = ProgrammeType.Rest }));

                plan.Weeks.Add(week);
            }

            return plan;
        }

        private List<ExerciseDefinition> Pick(List<ExerciseDefinition> pool, string[] keys, int count) =>
            pool.Where(e => MatchAny(e, keys))
                .OrderBy(_ => _rnd.Next())
                .Take(count)
                .ToList();
    }
}
