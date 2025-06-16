using FitnessTracker.V1.Models;


using static FitnessTracker.V1.Models.Model;


namespace FitnessTracker.V1.Services.ProgrammeGeneration
{



    public class TbtProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "TBT";

        private class WeekTemplate
        {
            public int Week { get; set; }
            public int Day { get; set; }
            public int Sets { get; set; }
            public int Reps { get; set; }
            public int RestSeconds { get; set; }
        }

        private readonly List<WeekTemplate> templates = new();
        private readonly Random rnd = new();

        public TbtProgrammeStrategy()
        {
            for (int w = 1; w <= 8; w++)
            {
                for (int d = 1; d <= 3; d++)
                {
                    int sets = (w % 2 == 1) ? 3 : 4;
                    int reps = d switch { 1 => 5, 2 => 8, 3 => 15, _ => 10 };
                    int rest = d switch { 1 => 60, 2 => 90, 3 => 120, _ => 90 };

                    if (w >= 5)
                    {
                        reps = d switch { 1 => 18, 2 => 8, 3 => 12, _ => 10 };
                        rest = d switch { 1 => 120, 2 => 60, 3 => 90, _ => 90 };
                        sets = (w % 2 == 1) ? (d == 1 ? 2 + w % 4 : 2) : 3;
                    }

                    templates.Add(new WeekTemplate { Week = w, Day = d, Sets = sets, Reps = reps, RestSeconds = rest });
                }
            }
        }

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

                var usedThisWeek = new HashSet<int>();
                int workoutIndex = 0;
                int totalWorkouts = 3;

                for (int dayIndex = 1; dayIndex <= 7; dayIndex++)
                {
                    if (workoutIndex < totalWorkouts && dayIndex % 2 == 1)
                    {
                        var tpl = templates.First(t => t.Week == w && t.Day == workoutIndex + 1);

                        var workoutDay = new WorkoutDay
                        {
                            DayIndex = dayIndex,
                            TypeProgramme = ProgrammeType.FullBody
                        };

                        var compound = PickExercises(pool, "Compound", usedThisWeek, 4);
                        var single = PickExercises(pool, "Single-Joint", usedThisWeek, 2);

                        foreach (var ex in compound.Concat(single))
                        {
                            workoutDay.Exercises.Add(new ExerciseSession
                            {
                                ExerciseId = ex.Id,
                                ExerciseName = ex.Name,
                                Series = tpl.Sets,
                                Repetitions = tpl.Reps,
                                RestTimeSeconds = tpl.RestSeconds,
                                IsSuperset = (w % 2 == 0),
                                Pourcentage1RM = week.ChargeIncrementPercent
                            });
                            usedThisWeek.Add(ex.Id);
                        }

                        week.SeriesWeek = tpl.Sets;
                        week.RepetitionsWeek = tpl.Reps;
                        week.RestTimeWeek = tpl.RestSeconds;

                        week.Days.Add(workoutDay);
                        workoutIndex++;
                    }
                    else
                    {
                        week.Days.Add(new WorkoutDay
                        {
                            DayIndex = dayIndex,
                            TypeProgramme = ProgrammeType.Rest
                        });
                    }
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }

        private List<ExerciseDefinition> PickExercises(List<ExerciseDefinition> pool, string type, HashSet<int> used, int count)
        {
            var filtered = pool
                .Where(e => e.Description.Contains(type, StringComparison.OrdinalIgnoreCase))
                .Where(e => !used.Contains(e.Id))
                .OrderBy(_ => rnd.Next())
                .ToList();

            return filtered.Take(count).ToList();
        }
    }


}
