using static FitnessTracker.V1.Models.Model;
using System.Collections.Generic;
using FitnessTracker.V1.Models.Enumeration;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammePredefini
{
    public class ProgrammeWageningen
    {
        public WorkoutPlan GeneratePlan()
        {
            var plan = new WorkoutPlan { TotalWeeks = 4 };

            for (int week = 1; week <= 4; week++)
            {
                var w = new WorkoutWeek { WeekNumber = week };

                for (int day = 1; day <= 7; day++)
                {
                    var d = new WorkoutDay
                    {
                        DayIndex = day,
                        TypeProgramme = (day == 1 || day == 4) ? ProgrammeType.FullBody : ProgrammeType.Rest,
                        Exercises = new List<ExerciseSession>()
                    };

                    if (day == 1 || day == 4)
                    {
                        d.Exercises.Add(new ExerciseSession { ExerciseName = "Squats", Series = 3, Repetitions = 10 });
                        d.Exercises.Add(new ExerciseSession { ExerciseName = "Tirage élastique", Series = 3, Repetitions = 10 });
                    }

                    w.Days.Add(d);
                }

                plan.Weeks.Add(w);
            }

            return plan;
        }
    }
}
