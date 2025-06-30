using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar
{
    public class BradPittSnatchProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "PittSnatch";

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 6 };
            int[] active = { 1, 2, 4, 6 };      // Lun, Mar, Jeu, Sam

            for (int w = 1; w <= 6; w++)
            {
                var week = new WorkoutWeek { WeekNumber = w };

                foreach (int d in Enumerable.Range(1, 7))
                {
                    if (!active.Contains(d))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.FullBody };

                    switch (d)
                    {
                        case 1: // Chest/Abs
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Chest", 3), 3, 8, 60);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Abs", 2), 3, 12, 45);
                            break;

                        case 2: // Back
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Back", 4), 4, 6, 75);
                            break;

                        case 4: // Shoulders/Arms
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Shoulder", 2), 4, 6, 75);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Biceps", 1), 3, 10, 60);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Triceps", 1), 3, 10, 60);
                            break;

                        case 6: // Legs
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Leg", 4), 4, 8, 75);
                            break;
                    }

                    week.Days.Add(day);
                }
                plan.Weeks.Add(week);
            }
            return plan;
        }
    }
}
