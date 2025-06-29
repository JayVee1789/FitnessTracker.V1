using FitnessTracker.V1.Models;
using static FitnessTracker.V1.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class MobilityAdvancedProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "MobilityAdv";
        private readonly Random _rnd = new();

        private static bool Qualify(ExerciseDefinition e) =>
            e.Description.Contains("Mobility", StringComparison.OrdinalIgnoreCase) ||
            e.Description.Contains("Stretch", StringComparison.OrdinalIgnoreCase) ||
            e.Description.Contains("Isometric", StringComparison.OrdinalIgnoreCase);

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var list = pool.Where(Qualify).ToList();
            var plan = new WorkoutPlan { TotalWeeks = 10 };

            for (int w = 1; w <= 10; w++)
            {
                int hold = 40 + w * 2;   // 40 → 60 s
                int dynRep = 12 + w / 2;   // 12 → 17 reps
                int perDay = 12 + w / 3;   // 12 → 15 exos

                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    SeriesWeek = 2,
                    RepetitionsWeek = dynRep,
                    RestTimeWeek = 15
                };

                for (int d = 1; d <= 7; d++)
                {
                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Mobility };
                    var exos = list.OrderBy(_ => _rnd.Next()).Take(perDay);

                    foreach (var ex in exos)
                    {
                        bool holdPose = ex.Description.Contains("Isometric", StringComparison.OrdinalIgnoreCase)
                                     || ex.Description.Contains("Stretch", StringComparison.OrdinalIgnoreCase);

                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = 2,
                            Repetitions = holdPose ? hold : dynRep,
                            RestTimeSeconds = 15,
                            IsSuperset = true
                        });
                    }
                    week.Days.Add(day);
                }
                plan.Weeks.Add(week);
            }
            return plan;
        }
    }
}
