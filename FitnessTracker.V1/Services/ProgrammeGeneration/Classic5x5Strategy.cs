using FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class Classic5x5Strategy : IProgrammeStrategy
    {
        public string Name => "5x5";

        public WorkoutPlan GeneratePlan(UserProfile p, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan { TotalWeeks = 12 };
            var exos = CelebHelpers.Pick(pool, "Compound", 5);

            for (int w = 1; w <= 12; w++)
            {
                var week = new WorkoutWeek { WeekNumber = w };
                for (int d = 1; d <= 3; d++)
                {
                    var day = new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.FullBody };
                    CelebHelpers.AddExos(day, exos, 5, 5, 90, 80);
                    week.Days.Add(day);
                }
                plan.Weeks.Add(week);
            }
            return plan;
        }
    }

}
