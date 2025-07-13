using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic
{
    /// <summary>
    /// Yoga Flow – 8 semaines – 5 séances (Lun-Mar-Jeu-Ven-Sam)
    /// Progression : +5 s de tenue toutes les 2 semaines.
    /// </summary>
    public class YogaProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Yoga";

        private readonly Random _rnd = new();

        private static bool IsYoga(ExerciseDefinition ex) =>
            ex.Category.Contains("Yoga", StringComparison.OrdinalIgnoreCase);

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            var yogaPool = pool.Where(IsYoga).ToList();
            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int w = 1; w <= 8; w++)
            {
                int hold = 25 + (w - 1) / 2 * 5; // 25 s → 40 s
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 0,
                    SeriesWeek = 1,
                    RepetitionsWeek = hold,
                    RestTimeWeek = 0
                };

                foreach (int d in new[] { 1, 2, 4, 5, 6 })
                {
                    var day = new WorkoutDay
                    {
                        DayIndex = d,
                        TypeProgramme = ProgrammeType.Yoga
                    };

                    var poses = yogaPool.OrderBy(_ => _rnd.Next()).Take(10).ToList();

                    foreach (var ex in poses)
                    {
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = 1,
                            Repetitions = hold,
                            RestTimeSeconds = 0,
                            IsSuperset = true,
                            Pourcentage1RM = 0
                        });
                    }

                    week.Days.Add(day);
                }

                week.Days.AddRange(Enumerable.Range(1, 7)
                    .Except(new[] { 1, 2, 4, 5, 6 })
                    .Select(i => new WorkoutDay { DayIndex = i, TypeProgramme = ProgrammeType.Rest }));

                plan.Weeks.Add(week);
            }

            return plan;
        }
    }
}
