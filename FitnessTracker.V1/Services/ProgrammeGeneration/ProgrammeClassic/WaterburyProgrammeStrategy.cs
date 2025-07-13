using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic
{
    /// <summary>
    /// Générateur « Waterbury‐style » : full-body 4 à 6 séances / semaine,
    /// schémas 10×3 / 4×6 / 3×8, progression +2 % 1 RM toutes les 2 semaines.
    /// </summary>
    public sealed class WaterburyProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Waterbury";

        // ──────────────────────────────────────────────────────────
        #region Key words & helpers
        // ──────────────────────────────────────────────────────────
        private static readonly string[] PushKeys = { "Bench", "Push", "Dip", "Press" };
        private static readonly string[] PullKeys = { "Row", "Pull", "Chin", "Face Pull", "Meadows" };
        private static readonly string[] QuadKeys = { "Squat", "Leg Press", "Split", "Lunge" };
        private static readonly string[] HipKeys = { "Deadlift", "RDL", "Hip Thrust", "Good Morning" };

        private static bool MatchAny(ExerciseDefinition ex, string[] keys) =>
            keys.Any(k => ex.Name.Contains(k, StringComparison.OrdinalIgnoreCase));

        private static readonly string[] BodyweightAllowed =
            { "Bodyweight", "Body Weight", "Resistance Band", "Band", "Wall", "Floor", "Mat" };

        private static bool IsBodyweightFriendly(ExerciseDefinition e) =>
            BodyweightAllowed.Any(tag =>
                e.Equipment.Contains(tag, StringComparison.OrdinalIgnoreCase));

        #endregion
        // ──────────────────────────────────────────────────────────

        private readonly Random _rnd = new();

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            // 1. fréquence Waterbury : ≥4
            int freq = Math.Clamp(profile.SeancesPerWeek, 4, 6);
            int totalWeeks = 6;

            // 2. filtrage poids du corps
            var source = profile.BodyweightOnly
                ? pool.Where(IsBodyweightFriendly).ToList()
                : pool;

            // 3. listes d’exercices
            var pushList = source.Where(e => MatchAny(e, PushKeys)).ToList();
            var pullList = source.Where(e => MatchAny(e, PullKeys)).ToList();
            var quadList = source.Where(e => MatchAny(e, QuadKeys)).ToList();
            var hipList = source.Where(e => MatchAny(e, HipKeys)).ToList();

            // 4. caractéristiques suivant niveau
            (int keySets, int keyReps, int keyPct,
             int secSets, int secReps, int rest) cfg = profile.Level switch
             {
                 UserLevel.Debutant => (8, 3, 75, 3, 8, 60),
                 UserLevel.Intermediaire => (10, 3, 80, 4, 6, 75),
                 _ => (10, 3, 83, 4, 6, 90) // Avancé
             };

            var plan = new WorkoutPlan { TotalWeeks = totalWeeks };

            // 5. répartition des jours ex. [1,3,5,7] pour 4 séances
            var activeDays = PickTrainingDays(freq);

            for (int w = 1; w <= totalWeeks; w++)
            {
                // progression : +2 % toutes les 2 semaines
                int incr = (w - 1) / 2 * 2;          // 0,2,4…
                int pct = cfg.keyPct + incr;

                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = pct,
                    SeriesWeek = cfg.keySets,          // info récap
                    RepetitionsWeek = cfg.keyReps,
                    RestTimeWeek = cfg.rest
                };

                // éviter de répéter 2 fois le même exo dans la semaine
                var usedThisWeek = new HashSet<int>();

                foreach (int d in Enumerable.Range(1, 7))
                {
                    if (!activeDays.Contains(d))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = d, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    var day = new WorkoutDay
                    {
                        DayIndex = d,
                        TypeProgramme = ProgrammeType.FullBody
                    };

                    // -------- paire A = Push / Pull ------------
                    var push = PickUnique(pushList, usedThisWeek);
                    var pull = PickUnique(pullList, usedThisWeek);

                    AddSession(push, cfg.keySets, cfg.keyReps, cfg.rest, pct, day, superset: true);
                    AddSession(pull, cfg.keySets, cfg.keyReps, cfg.rest, pct, day, superset: true);

                    // -------- paire B = Quad / Hip -------------
                    var quad = PickUnique(quadList, usedThisWeek);
                    var hip = PickUnique(hipList, usedThisWeek);

                    AddSession(quad, cfg.secSets, cfg.secReps, cfg.rest, 0, day, superset: true);
                    AddSession(hip, cfg.secSets, cfg.secReps, cfg.rest, 0, day, superset: true);

                    // -------- accessoire prioritaire -----------
                    if (!string.IsNullOrWhiteSpace(profile.PriorityMuscle))
                    {
                        var acc = source
                            .Where(e => e.Description.Contains("Single", StringComparison.OrdinalIgnoreCase)
                                     && e.Name.Contains(profile.PriorityMuscle, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(_ => _rnd.Next())
                            .FirstOrDefault();

                        if (acc != null) AddSession(acc, 2, 12, 45, 0, day, superset: false);
                    }

                    week.Days.Add(day);
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }

        // ──────────────────────────────────────────────────────────
        #region Small helpers
        // ──────────────────────────────────────────────────────────
        private ExerciseDefinition PickUnique(List<ExerciseDefinition> list, HashSet<int> used)
        {
            var notUsed = list.Where(e => !used.Contains(e.Id)).ToList();
            if (!notUsed.Any()) { used.Clear(); notUsed = list; }
            var ex = notUsed.OrderBy(_ => _rnd.Next()).First();
            used.Add(ex.Id);
            return ex;
        }

        private static void AddSession(
            ExerciseDefinition ex, int sets, int reps, int rest, int pct,
            WorkoutDay day, bool superset)
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

        private static List<int> PickTrainingDays(int freq)
        {
            // Disperse équitablement sur 7 jours : ex. 5 → [1,2,4,5,6]
            return freq switch
            {
                4 => new() { 1, 3, 5, 7 },
                5 => new() { 1, 2, 4, 5, 6 },
                _ => new() { 1, 2, 3, 5, 6, 7 } // 6 séances
            };
        }
        #endregion
    }
}
