using FitnessTracker.V1.Models.Enumeration;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar
{
    public class RockProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Rock";

        private static readonly Dictionary<int, string> split = new()
        {
            {1,"Chest"}, {2,"Back"}, {3,"LegsA"},
            {4,"Shoulders"}, {5,"Arms"}, {6,"LegsB"}
        };

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int w = 1; w <= 8; w++)
            {
                var week = new WorkoutWeek { WeekNumber = w };

                foreach (int d in Enumerable.Range(1, 7))
                {
                    if (!split.TryGetValue(d, out var tag))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }
                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.FullBody };

                    switch (tag)
                    {
                        case "Chest":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Chest", 4), 5, 10, 75);
                            break;
                        case "Back":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Back", 4), 5, 10, 75);
                            break;
                        case "LegsA":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Leg", 4), 6, 8, 90);
                            break;
                        case "Shoulders":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Shoulder", 4), 5, 10, 75);
                            break;
                        case "Arms":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Biceps", 2), 4, 12, 60);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Triceps", 2), 4, 12, 60);
                            break;
                        case "LegsB":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Leg", 4), 5, 12, 75);
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
