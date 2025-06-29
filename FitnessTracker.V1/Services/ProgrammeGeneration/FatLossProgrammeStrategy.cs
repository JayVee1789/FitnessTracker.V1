using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class FatLossProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "FatLoss";

        private readonly Random _rnd = new();

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 6 };

            for (int w = 1; w <= 6; w++)
            {
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 50 + w * 3
                };

                var used = new HashSet<int>();

                for (int d = 1; d <= 7; d++)
                {
                    if (d == 2 || d == 5) // Cardio HIIT 25-30 min
                    {
                        week.Days.Add(new WorkoutDay
                        {
                            DayIndex = d,
                            TypeProgramme = ProgrammeType.Mixte,
                            Exercises = new List<ExerciseSession>
                            {
                                new ExerciseSession
                                {
                                    ExerciseId = 0,
                                    ExerciseName = "HIIT Cardio (rameur, vélo ou sprint)",
                                    Series = 1,
                                    Repetitions = 0,
                                    RestTimeSeconds = 0,
                                    IsSuperset = false,
                                    Pourcentage1RM = 0
                                }
                            }
                        });
                        continue;
                    }

                    if (d == 1 || d == 3 || d == 6) // circuits
                    {
                        var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.FullBody };

                        var exo = pool.Where(e =>
                                e.Description.Contains("Compound", StringComparison.OrdinalIgnoreCase) &&
                                !used.Contains(e.Id))
                            .OrderBy(_ => _rnd.Next())
                            .Take(6)
                            .ToList();

                        foreach (var e in exo)
                        {
                            day.Exercises.Add(new ExerciseSession
                            {
                                ExerciseId = e.Id,
                                ExerciseName = e.Name,
                                Series = 3,
                                Repetitions = 15,
                                RestTimeSeconds = 30,
                                IsSuperset = true,
                                Pourcentage1RM = week.ChargeIncrementPercent
                            });
                            used.Add(e.Id);
                        }

                        week.Days.Add(day);
                    }
                    else
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                    }
                }

                week.SeriesWeek = 3;
                week.RepetitionsWeek = 15;
                week.RestTimeWeek = 30;
                plan.Weeks.Add(week);
            }

            return plan;
        }
    }
}
