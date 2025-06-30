using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeStar
{
    public static class CelebHelpers
    {
        private static readonly Random _rnd = new();

        // Filtre + tirage aléatoire d'exercices
        public static List<ExerciseDefinition> Pick(
            List<ExerciseDefinition> pool, string cat, int n) =>
            pool.Where(e => e.Category.Contains(cat, StringComparison.OrdinalIgnoreCase))
                .OrderBy(_ => _rnd.Next())
                .Take(n).ToList();

        // Ajoute une série d'exos au day
        public static void AddExos(
            WorkoutDay day, IEnumerable<ExerciseDefinition> list,
            int sets, int reps, int rest, int pct = 0, bool superset = false)
        {
            foreach (var ex in list)
            {
                day.Exercises.Add(new ExerciseSession
                {
                    ExerciseId = ex.Id,
                    ExerciseName = ex.Name,
                    Series = sets,
                    Repetitions = reps,
                    RestTimeSeconds = rest,
                    Pourcentage1RM = pct,
                    IsSuperset = superset
                });
            }
        }
    }
}
