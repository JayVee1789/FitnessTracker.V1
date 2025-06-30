using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar
{
    public class MeganStallionProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "MeganStallion";

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 8 };
            int[] active = { 1, 3, 5, 6 }; // Lu, Me, Ve, Sa

            for (int w = 1; w <= 8; w++)
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

                    // 3 exos Glute/Hip + 2 Quad + 1 Core/HIIT
                    CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Glute", 3), 4, 12, 60);
                    CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Quad", 2), 3, 12, 60);
                    CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Core", 1), 3, 15, 45);

                    week.Days.Add(day);
                }
                plan.Weeks.Add(week);
            }
            return plan;
        }
    }
}
