namespace FitnessTracker.V1.Models
{
    public static class TempDataStore
    {
        public class ProgrammeTemporaire
        {
            public string Nom { get; set; } = "";
            public Model.WorkoutPlan Plan { get; set; } = new();
        }

        public static ProgrammeTemporaire? CurrentProgramme { get; set; }
    }
}
