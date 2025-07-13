using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic
{
    /// <summary>
    /// Pilates Mat – 8 semaines – 3 séances (Lun-Mer-Ven)
    /// Semaine 5 +: on passe de 3×12 à 4×15 pour accroître le volume.
    /// </summary>
    public class PilatesProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Pilates";

        private readonly Random _rnd = new();

        private static bool IsPilates(ExerciseDefinition ex) =>
            ex.Category.Contains("Pilates", StringComparison.OrdinalIgnoreCase);

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            var pilatesPool = pool.Where(IsPilates).ToList();
            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int w = 1; w <= 8; w++)
            {
                bool volumeUp = w >= 5;
                int sets = volumeUp ? 4 : 3;
                int reps = volumeUp ? 15 : 12;

                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 0,
                    SeriesWeek = sets,
                    RepetitionsWeek = reps,
                    RestTimeWeek = 45
                };

                foreach (int d in new[] { 1, 3, 5 })
                {
                    var day = new WorkoutDay
                    {
                        DayIndex = d,
                        TypeProgramme = ProgrammeType.Pilates
                    };

                    var moves = pilatesPool.OrderBy(_ => _rnd.Next()).Take(8).ToList();

                    foreach (var ex in moves)
                    {
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = sets,
                            Repetitions = reps,
                            RestTimeSeconds = 45,
                            IsSuperset = profile.WantsSuperset,
                            Pourcentage1RM = 0
                        });
                    }

                    week.Days.Add(day);
                }

                week.Days.AddRange(Enumerable.Range(1, 7)
                    .Except(new[] { 1, 3, 5 })
                    .Select(i => new WorkoutDay { DayIndex = i, TypeProgramme = ProgrammeType.Rest }));

                plan.Weeks.Add(week);
            }

            return plan;
        }
    }
}
