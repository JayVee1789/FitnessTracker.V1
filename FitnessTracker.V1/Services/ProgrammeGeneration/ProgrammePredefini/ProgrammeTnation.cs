using static FitnessTracker.V1.Models.Model;
using System.Collections.Generic;
using FitnessTracker.V1.Models.Enumeration;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammePredefini
{
    public class ProgrammeTnation
    {
        public WorkoutPlan GeneratePlan()
        {
            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int week = 1; week <= 8; week++)
            {
                var w = new WorkoutWeek
                {
                    WeekNumber = week,
                    ChargeIncrementPercent = 0,
                    SeriesWeek = 4,
                    RepetitionsWeek = 6,
                    RestTimeWeek = 60,
                    Days = new List<WorkoutDay>()
                };

                for (int day = 1; day <= 7; day++)
                {
                    var d = new WorkoutDay
                    {
                        DayIndex = day,
                        Exercises = new List<ExerciseSession>()
                    };

                    switch (day)
                    {
                        case 1:
                            d.TypeProgramme = ProgrammeType.Strength;
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "BB Back Squats", Series = 10, Repetitions = 3, RestTimeSeconds = 70 });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Dips", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Bent Over BB or DB Rows", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Skull Crushers", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Standing BB Curls", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Hanging Leg Raises", Series = 4, Repetitions = 6, RestTimeSeconds = 60 });
                            break;
                        case 2:
                            d.TypeProgramme = ProgrammeType.Cardio;
                            break;
                        case 3:
                            d.TypeProgramme = ProgrammeType.UpperBody;
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "BB or DB Bench Press", Series = 10, Repetitions = 3, RestTimeSeconds = 60 });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Partial Deadlift (Romanian)", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Standing BB Military Press", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Standing Calf Raise", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Upright Rows", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Triceps Pressdowns", Series = 4, Repetitions = 6, RestTimeSeconds = 60 });
                            break;
                        case 4:
                            d.TypeProgramme = ProgrammeType.Cardio;
                            break;
                        case 5:
                            d.TypeProgramme = ProgrammeType.FullBody;
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Chin-Ups", Series = 10, Repetitions = 3, RestTimeSeconds = 70 });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Decline BB or DB Bench Press", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Standing Hammer Curls", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Seated Calf Raises", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Glute/Ham Raise or Leg Curl", Series = 4, Repetitions = 6, RestTimeSeconds = 60, IsSuperset = true });
                            d.Exercises.Add(new ExerciseSession { ExerciseName = "Lunges or Step-Ups", Series = 4, Repetitions = 6, RestTimeSeconds = 60 });
                            break;
                        case 6:
                            d.TypeProgramme = ProgrammeType.Cardio;
                            break;
                        case 7:
                            d.TypeProgramme = ProgrammeType.Rest;
                            break;
                    }

                    w.Days.Add(d);
                }

                plan.Weeks.Add(w);
            }

            return plan;
        }
    }
}
