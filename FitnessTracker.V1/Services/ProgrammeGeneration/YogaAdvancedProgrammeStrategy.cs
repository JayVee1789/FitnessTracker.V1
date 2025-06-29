using FitnessTracker.V1.Models;
using static FitnessTracker.V1.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    /// <summary>
    /// Power / Ashtanga-inspired : 12 semaines • 6 séances / semaine
    /// Progression : durée de tenue + variétés avancées (inversions / arm-balances).
    /// </summary>
    public class YogaAdvancedProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "YogaAdv";
        private readonly Random _rnd = new();

        private static bool IsYoga(ExerciseDefinition e) =>
            e.Category.Contains("Yoga", StringComparison.OrdinalIgnoreCase);

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var poses = pool.Where(IsYoga).ToList();
            var plan = new WorkoutPlan { TotalWeeks = 12 };

            for (int w = 1; w <= 12; w++)
            {
                int hold = 40 + 5 * ((w - 1) / 2);   // 40 s → 70 s
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    SeriesWeek = 1,
                    RepetitionsWeek = hold,
                    RestTimeWeek = 0,
                    ChargeIncrementPercent = 0
                };

                foreach (int d in new[] { 1, 2, 3, 5, 6, 7 })  // repos jeudi
                {
                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Yoga };

                    // 75 % standing / balance, 25 % “floor / inversion”
                    var standing = poses
                        .Where(x => x.Name.Contains("Warrior", StringComparison.OrdinalIgnoreCase)
                                 || x.Name.Contains("Pose", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(_ => _rnd.Next()).Take(10);

                    var special = poses
                        .Except(standing)
                        .OrderBy(_ => _rnd.Next()).Take(4);

                    foreach (var ex in standing.Concat(special))
                    {
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = 1,
                            Repetitions = hold,   // secondes
                            RestTimeSeconds = 0,
                            IsSuperset = true
                        });
                    }
                    week.Days.Add(day);
                }

                // jeudi repos
                week.Days.Add(new WorkoutDay { DayIndex = 4, TypeProgramme = ProgrammeType.Rest });
                plan.Weeks.Add(week);
            }
            return plan;
        }
    }
}
