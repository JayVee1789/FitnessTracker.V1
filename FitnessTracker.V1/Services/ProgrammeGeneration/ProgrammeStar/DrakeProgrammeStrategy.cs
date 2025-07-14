using FitnessTracker.V1.Models.Enumeration;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar
{
    public class DrakeProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Drake";

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 8 };
            var split = new Dictionary<int, string>
            { {1,"Push"}, {2,"Pull"}, {3,"Legs"}, {5,"UpperHIIT"}, {6,"LowerHIIT"} };

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

                    var day = new WorkoutDay
                    {
                        DayIndex = d,
                        TypeProgramme = tag.Contains("HIIT")
                                                   ? ProgrammeType.Cardio
                                                   : ProgrammeType.FullBody
                    };

                    switch (tag)
                    {
                        case "Push":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Chest", 2), 4, 8, 60);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Shoulder", 1), 4, 8, 60);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Triceps", 1), 3, 10, 45);
                            break;

                        case "Pull":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Back", 3), 4, 8, 60);
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Biceps", 1), 3, 10, 45);
                            break;

                        case "Legs":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Leg", 4), 4, 8, 75);
                            break;

                        case "UpperHIIT":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Full Body", 4), 3, 15, 30);
                            break;

                        case "LowerHIIT":
                            CelebHelpers.AddExos(day, CelebHelpers.Pick(pool, "Leg", 4), 3, 15, 30);
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
