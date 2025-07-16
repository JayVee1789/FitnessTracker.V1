using static FitnessTracker.V1.Models.Model;
using System.Collections.Generic;
using FitnessTracker.V1.Models.Enumeration;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeSpecial
{
    public class WageningenProgramme : IProgrammeStrategy
    {
        public string Name => "ForceMobiliteWageningen";

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        
        {
            var plan = new WorkoutPlan { TotalWeeks = 4 };

            for (int week = 1; week <= 4; week++)
            {
                var workoutWeek = new WorkoutWeek
                {
                    WeekNumber = week,
                    SeriesWeek = week <= 2 ? 3 : 3,
                    RepetitionsWeek = week <= 2 ? 10 : 12,
                    RestTimeWeek = 60,
                    ChargeIncrementPercent = week <= 2 ? 0 : 5
                };

                for (int day = 1; day <= 7; day++)
                {
                    var workoutDay = new WorkoutDay
                    {
                        DayIndex = day,
                        TypeProgramme = GetProgrammeType(week, day),
                        Exercises = new List<ExerciseSession>()
                    };

                    if (week <= 2)
                    {
                        if (day == 1 || day == 4) // Lundi/Jeudi
                        {
                            AddWeek12Exos(workoutDay);
                        }
                        else if (day == 2) // Mardi Cardio léger
                        {
                            workoutDay.Exercises.Add(new ExerciseSession
                            {
                                ExerciseName = "Marche rapide ou vélo léger",
                                Series = 1,
                                Repetitions = 30,
                                RestTimeSeconds = 0,
                                IsSuperset = false,
                                Pourcentage1RM = 0
                            });
                        }
                        else // Marche quotidienne
                        {
                            workoutDay.Exercises.Add(new ExerciseSession
                            {
                                ExerciseName = "Marche quotidienne 30 min",
                                Series = 1,
                                Repetitions = 30,
                                RestTimeSeconds = 0,
                                IsSuperset = false,
                                Pourcentage1RM = 0
                            });
                        }
                    }
                    else // Semaines 3-4
                    {
                        if (day == 1 || day == 4)
                        {
                            AddWeek34Exos(workoutDay);
                        }
                        else if (day == 2)
                        {
                            workoutDay.Exercises.Add(new ExerciseSession
                            {
                                ExerciseName = "Marche rapide / Vélo / Aquagym",
                                Series = 1,
                                Repetitions = 40,
                                RestTimeSeconds = 0,
                                IsSuperset = false,
                                Pourcentage1RM = 0
                            });
                        }
                        else
                        {
                            workoutDay.Exercises.Add(new ExerciseSession
                            {
                                ExerciseName = "Marche active 30 min",
                                Series = 1,
                                Repetitions = 30,
                                RestTimeSeconds = 0,
                                IsSuperset = false,
                                Pourcentage1RM = 0
                            });
                        }
                    }

                    workoutWeek.Days.Add(workoutDay);
                }

                plan.Weeks.Add(workoutWeek);
            }

            return plan;
        }

        private ProgrammeType GetProgrammeType(int week, int day)
        {
            if (day == 1 || day == 4)
                return ProgrammeType.FullBody;
            if (day == 2)
                return ProgrammeType.Half;
            return ProgrammeType.Rest;
        }

        private void AddWeek12Exos(WorkoutDay day)
        {
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Marche rapide ou vélo léger", Series = 1, Repetitions = 10, RestTimeSeconds = 0 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Squats assistés sur chaise", Series = 3, Repetitions = 10, RestTimeSeconds = 60 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Tirage élastique", Series = 3, Repetitions = 10, RestTimeSeconds = 60 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Montées sur marche basse (10–15 cm)", Series = 3, Repetitions = 8, RestTimeSeconds = 60 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Équilibre sur une jambe", Series = 3, Repetitions = 20, RestTimeSeconds = 30 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Extensions mollets", Series = 3, Repetitions = 15, RestTimeSeconds = 30 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Gainage debout contre table", Series = 3, Repetitions = 20, RestTimeSeconds = 30 });
        }

        private void AddWeek34Exos(WorkoutDay day)
        {
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Échauffement cardio", Series = 1, Repetitions = 10, RestTimeSeconds = 0 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Squats sans chaise si possible", Series = 3, Repetitions = 12, RestTimeSeconds = 60 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Tirage élastique ou petites haltères", Series = 3, Repetitions = 12, RestTimeSeconds = 60 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Montées sur marche moyenne (15–20 cm)", Series = 3, Repetitions = 10, RestTimeSeconds = 60 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Équilibre dynamique bras", Series = 3, Repetitions = 25, RestTimeSeconds = 30 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Extensions mollets", Series = 3, Repetitions = 15, RestTimeSeconds = 30 });
            day.Exercises.Add(new ExerciseSession { ExerciseName = "Gainage debout ou table basse", Series = 3, Repetitions = 25, RestTimeSeconds = 30 });
        }
    }
}

