using Blazored.LocalStorage;
using FitnessTracker.V1.Models.Enumeration;
using System.Net.Http.Json;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services.ProgrammeGeneration.ProgrammePredefini
{
    public class ProgrammeTnation
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private List<ExerciseDefinition> _allExercises = new(); // pas readonly
        const string version = "1.1.0";

        public ProgrammeTnation(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<WorkoutPlan> GeneratePlanAsync()
        {
            _allExercises = await LoadExerciseListAsync();

            var plan = new WorkoutPlan { TotalWeeks = 8 };

            for (int week = 1; week <= 8; week++)
            {
                var w = new WorkoutWeek
                {
                    WeekNumber = week,
                    ChargeIncrementPercent = 0,
                    SeriesWeek = 4,
                    RepetitionsWeek = 6,
                    RestTimeWeek = 60,
                    Days = new List<WorkoutDay>()
                };

                for (int day = 1; day <= 7; day++)
                {
                    var d = new WorkoutDay
                    {
                        DayIndex = day,
                        Exercises = new List<ExerciseSession>()
                    };

                    switch (day)
                    {
                        case 1:
                            d.TypeProgramme = ProgrammeType.FullBody;
                            Add(d, "Squat", 10, 3, 70);
                            Add(d, "Dips", 4, 6, 60, true);
                            Add(d, "Bent Over Rows", 4, 6, 60, true);
                            Add(d, "Skull Crushers", 4, 6, 60, true);
                            Add(d, "BB Curls", 4, 6, 60, true);
                            Add(d, "Hanging Leg Raises", 4, 6, 60);
                            break;

                        case 2:
                            d.TypeProgramme = ProgrammeType.Rest;
                            break;

                        case 3:
                            d.TypeProgramme = ProgrammeType.FullBody;
                            Add(d, "Bench Press", 10, 3, 60);
                            Add(d, "Romanian Deadlift", 4, 6, 60, true);
                            Add(d, "Standing Military Presses", 4, 6, 60, true);
                            Add(d, "Standing Calf Raises", 4, 6, 60, true);
                            Add(d, "Upright Rows", 4, 6, 60, true);
                            Add(d, "Tricep Press-Downs", 4, 6, 60);
                            break;

                        case 4:
                            d.TypeProgramme = ProgrammeType.Rest;
                            break;

                        case 5:
                            d.TypeProgramme = ProgrammeType.FullBody;
                            Add(d, "Chin-Ups", 10, 3, 70);
                            Add(d, "Incline Bench Presses", 4, 6, 60, true);
                            Add(d, "Standing Hammer Curls", 4, 6, 60, true);
                            Add(d, "Seated Calf Raises", 4, 6, 60, true);
                            Add(d, "Leg Curls", 4, 6, 60, true);
                            Add(d, "Lunges", 4, 6, 60);
                            break;

                        case 6:
                            d.TypeProgramme = ProgrammeType.Rest;
                            break;

                        case 7:
                            d.TypeProgramme = ProgrammeType.Rest;
                            break;
                    }

                    w.Days.Add(d);
                }

                plan.Weeks.Add(w);
            }

            return plan;
        }

        private async Task<List<ExerciseDefinition>> LoadExerciseListAsync()
        {
            const string cacheKey = "exercise_defs";

            var defs = await _localStorage.GetItemAsync<List<ExerciseDefinition>>(cacheKey);
            if (defs is not null && defs.Any()) return defs;

            try
            {
                defs = await _http.GetFromJsonAsync<List<ExerciseDefinition>>($"data/ExercicesListeLocal.json?v={version}")
                       ?? new();
                await _localStorage.SetItemAsync(cacheKey, defs);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("❌ Erreur chargement JSON : " + ex.Message);
                defs = new();
            }

            return defs;
        }

        private void Add(WorkoutDay day, string nameQuery, int sets, int reps, int rest, bool isSuperset = false)
        {
            //var found = _allExercises.FirstOrDefault(e =>
            //    nameQuery.ToLower().Split(" or ").Any(part =>
            //        e.Name.ToLower().Contains(part.Trim())));

            var found = FindExerciseByName(nameQuery);

            if (found != null)
            {
                day.Exercises.Add(new ExerciseSession
                {
                    ExerciseId = found.Id,
                    ExerciseName = found.Name,
                    Series = sets,
                    Repetitions = reps,
                    RestTimeSeconds = rest,
                    IsSuperset = isSuperset,
                    Pourcentage1RM = 0
                });
            }
            else
            {
                day.Exercises.Add(new ExerciseSession
                {
                    ExerciseId = 0,
                    ExerciseName = nameQuery,
                    Series = sets,
                    Repetitions = reps,
                    RestTimeSeconds = rest,
                    IsSuperset = isSuperset,
                    Pourcentage1RM = 0
                });
            }
        }
        private ExerciseDefinition? FindExerciseByName(string nameQuery)
        {
            var options = nameQuery.ToLower().Split(" or ").Select(p => p.Trim()).ToList();

            // 1. Match exact
            var exact = _allExercises.FirstOrDefault(e =>
                options.Any(opt => e.Name.Equals(opt, StringComparison.OrdinalIgnoreCase)));

            if (exact != null) return exact;

            // 2. Match StartsWith
            var start = _allExercises.FirstOrDefault(e =>
                options.Any(opt => e.Name.StartsWith(opt, StringComparison.OrdinalIgnoreCase)));

            if (start != null) return start;

            // 3. Match Contains
            var contains = _allExercises.FirstOrDefault(e =>
                options.Any(opt => e.Name.Contains(opt, StringComparison.OrdinalIgnoreCase)));

            return contains; // peut être null
        }

    }
}
