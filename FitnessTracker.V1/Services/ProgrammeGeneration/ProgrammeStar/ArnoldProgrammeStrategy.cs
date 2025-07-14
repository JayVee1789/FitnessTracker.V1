using FitnessTracker.V1.Models.Enumeration;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar
{
    public class ArnoldProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Arnold";

        private static readonly Dictionary<int, string> split = new()
        {
            {1,"ChestBack"}, {2,"ShouldersArms"}, {3,"LegsAbs"},
            {4,"ChestBack"}, {5,"ShouldersArms"}, {6,"LegsAbs"}
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
                        case "ChestBack":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Chest", 3), 5, 10, 60,0, true);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Back", 3), 5, 10, 60, 0, true);
                            break;
                        case "ShouldersArms":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Shoulder", 2), 5, 10, 60, 0, true);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Biceps", 1), 4, 12, 45, 0, true);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Triceps", 1), 4, 12, 45, 0, true);
                            break;
                        case "LegsAbs":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Leg", 3), 5, 10, 75);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Abs", 2), 3, 20, 30);
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
