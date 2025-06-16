
using Blazored.LocalStorage;
using FitnessTracker.V1.Models;


namespace FitnessTracker.V1.Services
{
    public class PoidsService
    {
        private const string StorageKey = "poids_entries";
        private readonly ILocalStorageService _localStorage;
        private const string EntriesKey = "poids_entries";
        private const string ExercicesKey = "exercise_list";
        public PoidsService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<List<PoidsEntry>> GetEntriesAsync() =>
            await _localStorage.GetItemAsync<List<PoidsEntry>>(StorageKey) ?? new();

        public async Task AddEntryAsync(PoidsEntry entry)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Exercice == entry.Exercice && e.Date.Date == entry.Date.Date);
            entries.Add(entry);
            await _localStorage.SetItemAsync(StorageKey, entries);
        }

        public async Task OverwriteEntriesAsync(List<PoidsEntry> entries) =>
            await _localStorage.SetItemAsync(StorageKey, entries);

        public async Task RemoveEntryAsync(string exercice, DateTime date)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Exercice == exercice && e.Date.Date == date.Date);
            await _localStorage.SetItemAsync(StorageKey, entries);
        }

        public async Task RemoveDateAsync(DateTime date)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Date.Date == date.Date);
            await _localStorage.SetItemAsync(StorageKey, entries);
        }

        public async Task RemoveExerciceAsync(string exercice)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Exercice == exercice);
            await _localStorage.SetItemAsync(StorageKey, entries);
        }
        public async Task RemoveEntriesForExerciceAsync(string exercice)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Exercice == exercice);
            await _localStorage.SetItemAsync("poids_entries", entries);
        }

        public async Task<List<string>> GetExercicesAsync()
        {
            return await _localStorage.GetItemAsync<List<string>>(ExercicesKey) ?? new()
        {
            "Développé couché", "Dips", "Squat"
        };
        }

        //  Sauvegarder la liste des exercices
        public async Task SaveExercicesAsync(List<string> list)
        {
            await _localStorage.SetItemAsync(ExercicesKey, list);
        }
        public async Task RemoveByIdAsync(Guid id)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Id == id);
            await _localStorage.SetItemAsync("poids_entries", entries);
        }

        public async Task<double> GetLastPoidsForExerciceAsync(string exercice)
        {
            var entries = await GetEntriesAsync();

            var dernier = entries
                .Where(e => e.Exercice == exercice)
                .OrderByDescending(e => e.Date)
                .FirstOrDefault();

            return dernier?.Poids ?? 0;
        }

    }
}
