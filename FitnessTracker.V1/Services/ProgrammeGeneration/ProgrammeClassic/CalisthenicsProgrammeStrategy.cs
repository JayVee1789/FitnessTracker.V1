using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammeClassic
{
    /// <summary>
    /// Générateur “body-weight only” inspiré des recommandations
    /// Coach Bachmann (split Push/Pull/Legs/Core) :contentReference[oaicite:0]{index=0},
    /// StrengthLog 2025 :contentReference[oaicite:1]{index=1} et VeryWellHealth (progressive overload sans charge) :contentReference[oaicite:2]{index=2}.
    /// – 8 semaines
    /// – Progression 3×8-12 → 4×10-15
    /// – Adapté à profile.SeancesPerWeek (3 à 5) et BodyweightOnly
    /// </summary>
    public class CalisthenicsProgrammeStrategy : IProgrammeStrategy
    {
        public string Name => "Calisthenics";

        private readonly Random _rnd = new();

        public WorkoutPlan GeneratePlan(UserProfile profile, List<ExerciseDefinition> pool)
        {
            // filtre « body-weight » si demandé
            if (profile.BodyweightOnly)
                pool = pool.Where(e =>
                    e.Equipment.Contains("Bodyweight", StringComparison.OrdinalIgnoreCase) ||
                    e.Equipment.Contains("Bar", StringComparison.OrdinalIgnoreCase) || // Pull-up bar
                    e.Equipment.Contains("Ring", StringComparison.OrdinalIgnoreCase) ||
                    e.Equipment.Contains("Parallette", StringComparison.OrdinalIgnoreCase)
                ).ToList();

            int sessions = Math.Clamp(profile.SeancesPerWeek, 3, 5);
            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int w = 1; w <= 8; w++)
            {
                var week = new WorkoutWeek
                {
                    WeekNumber = w,
                    ChargeIncrementPercent = 0          // non pertinent en calisthenics
                };

                // progression sets/reps
                int sets = w <= 4 ? 3 : 4;
                int reps = w <= 4 ? 10 : 12;
                int rest = 60;

                var pattern = BuildPattern(sessions);

                foreach (int dayIdx in Enumerable.Range(1, 7))
                {
                    if (!pattern.TryGetValue(dayIdx, out var block))
                    {
                        week.Days.Add(new WorkoutDay { DayIndex = dayIdx, TypeProgramme = ProgrammeType.Rest });
                        continue;
                    }

                    var day = new WorkoutDay
                    {
                        DayIndex = dayIdx,
                        TypeProgramme = block switch
                        {
                            "Push" or "Pull" or "Legs" => ProgrammeType.FullBody,
                            "Core" or "Skills" => ProgrammeType.Core,
                            _ => ProgrammeType.FullBody
                        }
                    };

                    day.Exercises.AddRange(SelectExercises(block, pool, sets, reps, rest));

                    week.SeriesWeek = sets;
                    week.RepetitionsWeek = reps;
                    week.RestTimeWeek = rest;

                    week.Days.Add(day);
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }

        // ─────────────────────────────────────────────────────────────
        // helpers
        // ─────────────────────────────────────────────────────────────
        private Dictionary<int, string> BuildPattern(int sessions) =>
            sessions switch
            {
                3 => new() { { 1, "PushPullLegs" }, { 3, "Core" }, { 5, "PushPullLegs" } },

                4 => new() { { 1, "Push" }, { 3, "Pull" }, { 5, "Legs" }, { 6, "Core" } },

                _ /* 5 */ => new()
                {
                    { 1, "Push" },
                    { 2, "Pull" },
                    { 3, "Legs" },
                    { 5, "Push" },
                    { 6, "Core" }
                },
            };


        private IEnumerable<ExerciseSession> SelectExercises(
            string block,
            List<ExerciseDefinition> pool,
            int sets, int reps, int rest)
        {
            IEnumerable<ExerciseDefinition> chosen = block switch
            {
                "Push" => RandomPick(pool, 4, "Chest", "Shoulder", "Triceps"),
                "Pull" => RandomPick(pool, 4, "Back", "Biceps"),
                "Legs" => RandomPick(pool, 5, "Leg", "Glute", "Quad", "Ham"),
                "Core" => RandomPick(pool, 5, "Core", "Abs", "Oblique", "Isometric"),
                "PushPullLegs" => RandomPick(pool, 2, "Chest", "Shoulder")
                                     .Concat(RandomPick(pool, 2, "Back"))
                                     .Concat(RandomPick(pool, 2, "Leg")),
                "Skills" => RandomPick(pool, 4, "Handstand", "Planche", "Lever", "Muscle-Up"),
                _ => RandomPick(pool, 4, "Full Body")
            };

            foreach (var ex in chosen)
            {
                yield return new ExerciseSession
                {
                    ExerciseId = ex.Id,
                    ExerciseName = ex.Name,
                    Series = sets,
                    Repetitions = reps,
                    RestTimeSeconds = rest,
                    IsSuperset = false,
                    Pourcentage1RM = 0
                };
            }
        }

        private IEnumerable<ExerciseDefinition> RandomPick(
            List<ExerciseDefinition> pool,
            int count,
            params string[] keys) =>
            pool.Where(e =>
                    keys.Any(k => e.Name.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                                  e.Category.Contains(k, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(_ => _rnd.Next())
                .Take(count);
    }
}