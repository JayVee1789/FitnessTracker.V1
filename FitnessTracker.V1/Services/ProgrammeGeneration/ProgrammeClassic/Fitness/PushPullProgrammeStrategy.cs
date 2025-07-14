using FitnessTracker.V1.Models;
using FitnessTracker.V1.Models.Enumeration;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic.Fitness
{
    public class PushPullProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "PushPull4";
        private readonly Random _rnd = new();

        #region === UTILS ===
        private static bool MatchAny(ExerciseDefinition ex, params string[] keys) =>
            keys.Any(k =>
                ex.Category.Split('/', StringSplitOptions.TrimEntries)
                           .Any(c => c.Contains(k, StringComparison.OrdinalIgnoreCase))
             || ex.Name.Contains(k, StringComparison.OrdinalIgnoreCase));
        #endregion

        #region === KEYWORDS ===
        private static readonly string[] PushKeys = { "Chest", "Tricep", "Deltoid", "Shoulder", "Anterior Delt" };
        private static readonly string[] PullKeys = { "Back", "Lat", "Row", "Bicep", "Trap", "Rear Delt", "Lower Back" };
        #endregion

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int w = 1; w <= 8; w++)
            {
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 60 + w * 2
                };

                var used = new HashSet<int>();

                // Lundi(Mon=1)=PUSH – Mardi= PULL – Jeudi= PUSH – Vendredi= PULL
                var activeDays = new Dictionary<int, bool> // bool == isPush
                {
                    {1, true}, {2, false}, {4, true}, {5, false}
                };

                for (int d = 1; d <= 7; d++)
                {
                    if (!activeDays.TryGetValue(d, out bool isPush))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    // volume & intensité progressifs
                    int sets = w <= 4 ? 4 : 5;
                    int reps = d == 1 || d == 2 ? w <= 4 ? 6 : 8 : w <= 4 ? 10 : 12;
                    int rest = reps <= 6 ? 150 : 90;

                    var day = new WorkoutDay
                    {
                        DayIndex = d,
                        TypeProgramme = ProgrammeType.PushPull
                    };

                    // -- Sélection des exos
                    var comp = Pick(pool, isPush ? PushKeys : PullKeys, used, true, 4);
                    var iso = Pick(pool, isPush ? PushKeys : PullKeys, used, false, 2);

                    foreach (var ex in comp.Concat(iso))
                    {
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = sets,
                            Repetitions = reps,
                            RestTimeSeconds = rest,
                            IsSuperset = profile.WantsSuperset && reps >= 10,
                            Pourcentage1RM = week.ChargeIncrementPercent
                        });
                        used.Add(ex.Id);
                    }

                    week.SeriesWeek = sets;
                    week.RepetitionsWeek = reps;
                    week.RestTimeWeek = rest;
                    week.Days.Add(day);
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }
        private List<ExerciseDefinition> Pick(
            List<ExerciseDefinition> pool,
            string[] keys,
            HashSet<int> used,
            bool compound,
            int count)
        {
            return pool
                .Where(e => MatchAny(e, keys))
                .Where(e => e.Description.Contains(compound ? "Compound" : "Single", StringComparison.OrdinalIgnoreCase))
                .Where(e => !used.Contains(e.Id))
                .OrderBy(_ => _rnd.Next())
                .Take(count)
                .ToList();
        }
    }
}
