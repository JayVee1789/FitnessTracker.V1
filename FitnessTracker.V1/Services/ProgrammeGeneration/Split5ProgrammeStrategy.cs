using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class Split5ProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Split5";
        private readonly Random _rnd = new();

        private static bool MatchAny(ExerciseDefinition ex, params string[] keys) =>
            keys.Any(k =>
                ex.Category.Split('/', StringSplitOptions.TrimEntries)
                           .Any(c => c.Contains(k, StringComparison.OrdinalIgnoreCase))
             || ex.Name.Contains(k, StringComparison.OrdinalIgnoreCase));

        private static readonly (string Label, string[] Keys)[] Map = new[]
        {
            ("Chest",     new[]{ "Chest", "Pec" }),
            ("Back",      new[]{ "Back", "Row", "Lat", "Trap" }),
            ("Legs",      new[]{ "Leg", "Quad", "Ham", "Glute", "Hip", "Calf" }),
            ("Shoulders", new[]{ "Deltoid", "Shoulder", "Trap" }),
            ("Arms",      new[]{ "Bicep", "Tricep", "Forearm", "Curl", "Extension" })
        };

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            int totalWeeks = profile.ProgramDurationMonths <= 0 ? 12 : profile.ProgramDurationMonths * 4;
            var plan = new WorkoutPlan { TotalWeeks = totalWeeks };

            for (int w = 1; w <= totalWeeks; w++)
            {
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 65 + w
                };

                var used = new HashSet<int>();

                for (int d = 1; d <= 7; d++)
                {
                    if (d > 5)
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    var (label, keys) = Map[d - 1];

                    int sets = w <= 4 ? 4 : w <= 8 ? 5 : 6;
                    int reps = w <= 4 ? 10 : w <= 8 ? 8 : 6;
                    int rest = reps <= 8 ? 120 : 90;

                    var day = new WorkoutDay
                    {
                        DayIndex = d,
                        TypeProgramme = ProgrammeType.Split
                    };

                    var exercises = pool
                        .Where(e => MatchAny(e, keys))
                        .Where(e => !used.Contains(e.Id))
                        .OrderBy(_ => _rnd.Next())
                        .Take(6)
                        .ToList();

                    foreach (var ex in exercises)
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
    }
}
