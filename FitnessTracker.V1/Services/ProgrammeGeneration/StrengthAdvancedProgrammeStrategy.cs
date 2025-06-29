using FitnessTracker.V1.Models;
using static FitnessTracker.V1.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class StrengthAdvancedProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "StrengthAdv";
        private readonly Random _rnd = new();

        private static bool Match(ExerciseDefinition e, params string[] k) =>
            k.Any(x => e.Name.Contains(x, StringComparison.OrdinalIgnoreCase));

        string[] S = { "Squat" }, BP = { "Bench Press" }, DL = { "Deadlift" },
                 OHP = { "Overhead Press", "Military Press" }, ROW = { "Row" }, PU = { "Pull-Up", "Chin Up" };

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 16 };

            for (int w = 1; w <= 16; w++)
            {
                // blocs
                int sets, reps; double pct;
                if (w <= 6) { sets = 5; reps = 5; pct = 0.72; }
                else if (w <= 10) { sets = 4; reps = 3; pct = 0.80; }
                else if (w <= 12) { sets = 3; reps = 2; pct = 0.87; }
                else if (w == 13) { sets = 2; reps = 5; pct = 0.60; } // deload
                else { sets = 2; reps = 1; pct = 0.92 + 0.02 * (w - 14); }

                int rest = reps <= 2 ? 210 : 180;

                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    SeriesWeek = sets,
                    RepetitionsWeek = reps,
                    RestTimeWeek = rest,
                    ChargeIncrementPercent = (int)(pct * 100)
                };

                var template = new Dictionary<int, (string[][] lifts, double mod)>
                {
                    {1,(new[]{S,BP},1.0)},
                    {2,(new[]{DL,OHP},1.0)},
                    {4,(new[]{S,ROW},0.9)},
                    {5,(new[]{BP,PU},0.9)}
                };

                foreach (int d in Enumerable.Range(1, 7))
                {
                    if (!template.TryGetValue(d, out var t))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.FullBody };
                    var (lifts, mod) = t;

                    foreach (var k in lifts)
                    {
                        var ex = pool.Where(e => Match(e, k)).OrderBy(_ => _rnd.Next()).First();
                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = sets,
                            Repetitions = reps,
                            RestTimeSeconds = rest,
                            Pourcentage1RM = (int)Math.Round(pct * 100 * mod)
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
