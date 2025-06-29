using FitnessTracker.V1.Models;
using static FitnessTracker.V1.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class PilatesAdvancedProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "PilatesAdv";
        private readonly Random _rnd = new();

        private static bool IsPilates(ExerciseDefinition e) =>
            e.Category.Contains("Pilates", StringComparison.OrdinalIgnoreCase);

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var moves = pool.Where(IsPilates).ToList();
            var plan = new WorkoutPlan { TotalWeeks = 10 };

            for (int w = 1; w <= 10; w++)
            {
                int sets = w <= 5 ? 4 : 5;
                int reps = w <= 5 ? 15 : 18;
                int tempo = w <= 5 ? 4 : 6;   // excentrique contrôlée

                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    SeriesWeek = sets,
                    RepetitionsWeek = reps,
                    RestTimeWeek = 30
                };

                foreach (int d in new[] { 1, 2, 4, 5, 6 })  // repos mercredi & dimanche
                {
                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Pilates };

                    foreach (var ex in moves.OrderBy(_ => _rnd.Next()).Take(12))
                    {
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = sets,
                            Repetitions = reps,
                            RestTimeSeconds = 30,
                            IsSuperset = p.WantsSuperset,
                            //Tempo = $"{tempo}-0-{tempo}"
                        });
                    }
                    week.Days.Add(day);
                }

                week.Days.Add(new WorkoutDay { DayIndex = 3, TypeProgramme = ProgrammeType.Rest });
                week.Days.Add(new WorkoutDay { DayIndex = 7, TypeProgramme = ProgrammeType.Rest });

                plan.Weeks.Add(week);
            }
            return plan;
        }
    }
}
