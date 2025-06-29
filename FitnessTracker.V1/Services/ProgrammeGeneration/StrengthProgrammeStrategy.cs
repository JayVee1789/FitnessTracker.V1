using FitnessTracker.V1.Models;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration
{
    public class StrengthProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Strength";

        private readonly Random _rnd = new();

        #region ==== UTILS ====

        /// <summary>
        /// Teste si l'exercice correspond à au moins un des mots-clés.
        /// On scinde Category sur '/' pour gérer les catégories composées : « Chest/Triceps »…
        /// </summary>
        private static bool MatchAny(ExerciseDefinition ex, params string[] keys) =>
            keys.Any(k =>
                ex.Category.Split('/', StringSplitOptions.TrimEntries)
                           .Any(c => c.Contains(k, StringComparison.OrdinalIgnoreCase))
             || ex.Name.Contains(k, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Récupère le premier exercice qui matche les mots-clés.
        /// Si plusieurs : ordre aléatoire puis First().
        /// </summary>
        private ExerciseDefinition PickOne(
            List<ExerciseDefinition> pool,
            string[] keys)
        {
            return pool
                .Where(e => MatchAny(e, keys))
                .OrderBy(_ => _rnd.Next())
                .First(); // on considère que la base contient toujours au moins un match
        }

        #endregion

        #region ==== KEYWORDS DES LIFTS PRINCIPAUX ====

        private static readonly string[] SquatKeys = { "Squat" };
        private static readonly string[] BenchKeys = { "Bench Press" };
        private static readonly string[] RowKeys = { "Row" };
        private static readonly string[] DeadliftKeys = { "Deadlift" };
        private static readonly string[] OhPressKeys = { "Overhead Press", "Military Press", "Shoulder Press" };
        private static readonly string[] PullUpKeys = { "Pull-Up", "Pull Up", "Chin Up" };

        #endregion

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            var plan = new WorkoutPlan
            {
                TotalWeeks = 12
            };

            for (int w = 1; w <= 12; w++)
            {
                // === Paramètres progressifs ===
                int sets = 5;
                int reps = 5;
                if (w > 8) { sets = 3; reps = 3; }          // intensification
                else if (w > 4) { sets = 4; reps = 4; }

                int rest = 180;                              // repos long pour la force

                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 70 + w * 2,    // 70 % → 92 % sur 12 semaines
                    SeriesWeek = sets,
                    RepetitionsWeek = reps,
                    RestTimeWeek = rest
                };

                // === Séances Lundi (1) – Mercredi (3) – Vendredi (5) ===
                var dayTemplates = new Dictionary<int, (string[][] lifts, double intensityMod)>
                {
                    // intensityMod : multiplicateur sur %1RM pour les jours légers
                    {1, (new[]{ SquatKeys, BenchKeys, RowKeys },       1.0)},   // lourd
                    {3, (new[]{ DeadliftKeys, OhPressKeys, PullUpKeys },1.0)},   // lourd
                    {5, (new[]{ SquatKeys, BenchKeys, RowKeys },       0.8)}    // léger (80 %)
                };

                for (int d = 1; d <= 7; d++)
                {
                    if (!dayTemplates.TryGetValue(d, out var tpl))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    var (lifts, mod) = tpl;
                    var day = new WorkoutDay
                    {
                        DayIndex = d,
                        TypeProgramme = ProgrammeType.FullBody
                    };

                    foreach (var keyGroup in lifts)
                    {
                        var ex = PickOne(pool, keyGroup);

                        day.Exercises.Add(new ExerciseSession
                        {
                            ExerciseId = ex.Id,
                            ExerciseName = ex.Name,
                            Series = sets,
                            Repetitions = reps,
                            RestTimeSeconds = rest,
                            IsSuperset = false,
                            Pourcentage1RM = (int)Math.Round(week.ChargeIncrementPercent * mod)
                        });
                    }

                    // Accessoires optionnels (biceps/triceps + core) en superset si souhaité
                    if (profile.WantsSuperset)
                    {
                        var accessory = pool
                            .Where(e => e.Description.Contains("Single", StringComparison.OrdinalIgnoreCase)
                                     && !day.Exercises.Any(se => se.ExerciseId == e.Id))
                            .OrderBy(_ => _rnd.Next())
                            .Take(2);

                        foreach (var ex in accessory)
                        {
                            day.Exercises.Add(new ExerciseSession
                            {
                                ExerciseId = ex.Id,
                                ExerciseName = ex.Name,
                                Series = 3,
                                Repetitions = 12,
                                RestTimeSeconds = 60,
                                IsSuperset = true,
                                Pourcentage1RM = 0 // accessoires non %1RM
                            });
                        }
                    }

                    week.Days.Add(day);
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }
    }
}
