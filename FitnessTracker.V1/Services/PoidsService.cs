using Blazored.LocalStorage;
using FitnessTracker.V1.Models;
using FitnessTracker.V1.Services.Data;
using FitnessTracker.V1.Services.Gamification;

namespace FitnessTracker.V1.Services
{
    public class PoidsService
    {
        private const string StorageKey = "poids_entries";
        private const string ExercicesKey = "exercise_list";
        private const string PoidsKeysListKey = "poids_keys_list";

        private readonly ILocalStorageService _localStorage;
        private readonly SupabaseService2 _supabase;

        public PoidsService(ILocalStorageService localStorage, SupabaseService2 supabase)
        {
            _localStorage = localStorage;
            _supabase = supabase;
        }

        public async Task<List<PoidsEntry>> GetEntriesAsync()
        {
            if (!_supabase.IsCurrentUserAuthenticated)
            {
                Console.WriteLine("Utilisateur non connecté : lecture locale des poids.");
                return (await GetEntriesFromLocalAsync()).ToList();
            }

            var remoteEntries = await _supabase.GetPoidsEntriesFromSupabaseAsync();
            if (remoteEntries.Any())
            {
                Console.WriteLine($"{remoteEntries.Count} entrées synchronisées depuis Supabase.");
                await OverwriteEntriesAsync(remoteEntries);
                return remoteEntries;
            }

            var localEntries = await GetEntriesFromLocalAsync();
            if (localEntries.Any())
            {
                Console.WriteLine("Aucune entrée Supabase trouvée : conservation des poids locaux filtrés pour l'utilisateur courant.");
                return localEntries;
            }

            await OverwriteEntriesAsync(new());
            return new();
        }

        private async Task<List<PoidsEntry>> GetEntriesFromLocalAsync()
        {
            var locals = await GetAllLocalPoidsAsync();
            var userGuid = _supabase.GetCurrentUserIdAsGuid();

            if (userGuid is not null)
                locals = locals.Where(e => e.UserId == userGuid.Value).ToList();

            return locals.Select(ToRemote).ToList();
        }

        public async Task ResetAllPoidsLocalAsync()
        {
            var keys = await _localStorage.KeysAsync();
            var entryKeys = keys.Where(k => k.StartsWith("entry_")).ToList();

            foreach (var key in entryKeys)
                await _localStorage.RemoveItemAsync(key);

            await _localStorage.RemoveItemAsync(StorageKey);
            await _localStorage.RemoveItemAsync(PoidsKeysListKey);
        }

        public async Task AddEntryAsync(PoidsEntry entry, PoidsEntryLocal local)
        {
            var userGuid = _supabase.GetCurrentUserIdAsGuid();
            if (userGuid is not null)
            {
                entry.UserId = userGuid.Value;
                local.UserId = userGuid.Value;
            }

            local.Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id;
            entry.Id = local.Id;
            local.Exercice = entry.Exercice;
            local.Date = entry.Date;
            local.Poids = entry.Poids;
            local.EnLb = entry.EnLb;
            local.ObjectifAtteint = entry.ObjectifAtteint;

            await AddOrUpdateLocal(local);

            if (userGuid is null)
            {
                Console.WriteLine("Poids sauvegardé localement seulement : utilisateur non connecté.");
                return;
            }

            try
            {
                await _supabase.AddEntryAsync(entry);
                Console.WriteLine("Synchro Supabase réussie");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur Supabase, poids conservé en local : " + ex.Message);
            }
        }

        public async Task<List<PoidsEntryLocal>> GetAllLocalPoidsAsync()
        {
            var all = new List<PoidsEntryLocal>();

            try
            {
                var global = await _localStorage.GetItemAsync<List<PoidsEntryLocal>>(StorageKey);
                if (global is not null)
                    all.AddRange(global);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lecture poids_entries impossible, reset de la clé : " + ex.Message);
                await _localStorage.RemoveItemAsync(StorageKey);
            }

            var keys = await _localStorage.GetItemAsync<List<string>>(PoidsKeysListKey) ?? new();
            foreach (var key in keys.ToList())
            {
                try
                {
                    var entry = await _localStorage.GetItemAsync<PoidsEntryLocal>(key);
                    if (entry is not null)
                        all.Add(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lecture impossible pour {key} : {ex.Message}");
                }
            }

            var merged = all
                .Where(e => !string.IsNullOrWhiteSpace(e.Exercice))
                .GroupBy(e => LocalKey(e.Exercice, e.Date, e.UserId))
                .Select(g => g.Last())
                .OrderBy(e => e.Exercice)
                .ThenBy(e => e.Date)
                .ToList();

            await SaveAllLocalPoidsAsync(merged);
            return merged;
        }

        public async Task OverwriteEntriesAsync(List<PoidsEntry> entries)
        {
            var locals = entries.Select(e => new PoidsEntryLocal
            {
                Id = e.Id,
                Exercice = e.Exercice,
                Date = e.Date,
                Poids = e.Poids,
                UserId = e.UserId,
                EnLb = e.EnLb,
                ObjectifAtteint = e.ObjectifAtteint
            }).ToList();

            await SaveAllLocalPoidsAsync(locals);
        }

        public async Task RemoveEntryAsync(string exercice, DateTime date)
        {
            var entries = await GetAllLocalPoidsAsync();
            entries.RemoveAll(e => e.Exercice == exercice && e.Date.Date == date.Date && BelongsToCurrentUser(e));
            await SaveAllLocalPoidsAsync(entries);

            if (_supabase.IsCurrentUserAuthenticated)
            {
                try { await _supabase.RemoveByExerciceAndDateAsync(exercice, date); }
                catch (Exception ex) { Console.WriteLine("Suppression Supabase impossible : " + ex.Message); }
            }
        }

        public async Task RemoveDateAsync(DateTime date)
        {
            var entries = await GetAllLocalPoidsAsync();
            var toRemove = entries.Where(e => e.Date.Date == date.Date && BelongsToCurrentUser(e)).ToList();
            entries.RemoveAll(e => toRemove.Any(r => SameEntry(r, e)));
            await SaveAllLocalPoidsAsync(entries);

            if (_supabase.IsCurrentUserAuthenticated)
            {
                foreach (var entry in toRemove)
                {
                    try { await _supabase.RemoveByExerciceAndDateAsync(entry.Exercice, entry.Date); }
                    catch (Exception ex) { Console.WriteLine("Suppression Supabase impossible : " + ex.Message); }
                }
            }
        }

        public async Task RemoveExerciceAsync(string exercice)
        {
            var entries = await GetAllLocalPoidsAsync();
            var toRemove = entries.Where(e => e.Exercice == exercice && BelongsToCurrentUser(e)).ToList();
            entries.RemoveAll(e => toRemove.Any(r => SameEntry(r, e)));
            await SaveAllLocalPoidsAsync(entries);

            if (_supabase.IsCurrentUserAuthenticated)
            {
                foreach (var entry in toRemove)
                {
                    try { await _supabase.RemoveByExerciceAndDateAsync(entry.Exercice, entry.Date); }
                    catch (Exception ex) { Console.WriteLine("Suppression Supabase impossible : " + ex.Message); }
                }
            }
        }

        public async Task RemoveByIdAsync(Guid id)
        {
            var entries = await GetAllLocalPoidsAsync();
            entries.RemoveAll(e => e.Id == id && BelongsToCurrentUser(e));
            await SaveAllLocalPoidsAsync(entries);
        }

        public async Task RemoveEntriesForExerciceAsync(string exercice)
        {
            await RemoveExerciceAsync(exercice);
        }

        public async Task<List<string>> GetExercicesAsync()
        {
            return await _localStorage.GetItemAsync<List<string>>(ExercicesKey) ?? new()
            {
                "Développé couché", "Dips", "Squat"
            };
        }

        public async Task SaveExercicesAsync(List<string> list)
        {
            await _localStorage.SetItemAsync(ExercicesKey, list);
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

        public async Task SyncFromSupabaseAsync()
        {
            var remote = await _supabase.GetPoidsEntriesFromSupabaseAsync();
            if (remote.Any())
                await OverwriteEntriesAsync(remote);
        }

        public async Task<List<PoidsEntry>> GetEntriesFromSupabaseAsync()
        {
            return await _supabase.GetPoidsEntriesFromSupabaseAsync();
        }

        public async Task<double?> GetPoidsForExerciceAtDateAsync(string exercice, DateTime date)
        {
            var entries = await GetEntriesAsync();
            return entries.FirstOrDefault(e => e.Exercice == exercice && e.Date.Date == date.Date)?.Poids;
        }

        public async Task SavePoidsLocalAsync(PoidsEntry entry)
        {
            await AddOrUpdateLocal(new PoidsEntryLocal
            {
                Id = entry.Id,
                Exercice = entry.Exercice,
                Date = entry.Date,
                Poids = entry.Poids,
                UserId = entry.UserId,
                EnLb = entry.EnLb,
                ObjectifAtteint = entry.ObjectifAtteint
            });
        }

        public async Task SupprimerDepuisSupabaseAsync(string exercice, DateTime date)
        {
            await _supabase.RemoveByExerciceAndDateAsync(exercice, date);
        }

        public async Task SaveEntryUnifiedAsync(PoidsEntry remote, PoidsEntryLocal local)
        {
            await AddOrUpdateLocal(local);

            if (!_supabase.IsCurrentUserAuthenticated)
                return;

            try
            {
                await SupprimerDepuisSupabaseAsync(remote.Exercice, remote.Date);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ancienne entrée Supabase non supprimée : " + ex.Message);
            }

            await AddEntryAsync(remote, local);
        }

        public async Task AddEntryAndGamifyAsync(PoidsEntry entry, PoidsEntryLocal local, GamificationManager gamification)
        {
            await AddEntryAsync(entry, local);
            await gamification.AddXP(100, "Séance complétée");
        }

        public async Task AddOrUpdateLocal(PoidsEntryLocal entry)
        {
            var all = await GetAllLocalPoidsWithoutPersistAsync();
            var existing = all.FirstOrDefault(e => SameEntry(e, entry));

            if (existing is not null)
            {
                existing.Id = entry.Id == Guid.Empty ? existing.Id : entry.Id;
                existing.Poids = entry.Poids;
                existing.EnLb = entry.EnLb;
                existing.ObjectifAtteint = entry.ObjectifAtteint;
                existing.UserId = entry.UserId;
            }
            else
            {
                if (entry.Id == Guid.Empty)
                    entry.Id = Guid.NewGuid();
                all.Add(entry);
            }

            await SaveAllLocalPoidsAsync(all);
        }

        public async Task SaveAllLocalPoidsAsync(List<PoidsEntryLocal> all)
        {
            var merged = all
                .Where(e => !string.IsNullOrWhiteSpace(e.Exercice))
                .GroupBy(e => LocalKey(e.Exercice, e.Date, e.UserId))
                .Select(g => g.Last())
                .OrderBy(e => e.Exercice)
                .ThenBy(e => e.Date)
                .ToList();

            await _localStorage.SetItemAsync(StorageKey, merged);

            var oldKeys = await _localStorage.GetItemAsync<List<string>>(PoidsKeysListKey) ?? new();
            foreach (var key in oldKeys)
                await _localStorage.RemoveItemAsync(key);

            var newKeys = new List<string>();
            foreach (var entry in merged)
            {
                var key = EntryStorageKey(entry);
                await _localStorage.SetItemAsync(key, entry);
                newKeys.Add(key);
            }

            await _localStorage.SetItemAsync(PoidsKeysListKey, newKeys.Distinct().ToList());
        }

        private async Task<List<PoidsEntryLocal>> GetAllLocalPoidsWithoutPersistAsync()
        {
            var all = new List<PoidsEntryLocal>();

            try
            {
                var global = await _localStorage.GetItemAsync<List<PoidsEntryLocal>>(StorageKey);
                if (global is not null)
                    all.AddRange(global);
            }
            catch
            {
                await _localStorage.RemoveItemAsync(StorageKey);
            }

            var keys = await _localStorage.GetItemAsync<List<string>>(PoidsKeysListKey) ?? new();
            foreach (var key in keys)
            {
                try
                {
                    var entry = await _localStorage.GetItemAsync<PoidsEntryLocal>(key);
                    if (entry is not null)
                        all.Add(entry);
                }
                catch { }
            }

            return all
                .Where(e => !string.IsNullOrWhiteSpace(e.Exercice))
                .GroupBy(e => LocalKey(e.Exercice, e.Date, e.UserId))
                .Select(g => g.Last())
                .ToList();
        }

        private bool BelongsToCurrentUser(PoidsEntryLocal entry)
        {
            var userGuid = _supabase.GetCurrentUserIdAsGuid();
            return userGuid is null || entry.UserId == Guid.Empty || entry.UserId == userGuid.Value;
        }

        private static bool SameEntry(PoidsEntryLocal a, PoidsEntryLocal b) =>
            a.Exercice == b.Exercice && a.Date.Date == b.Date.Date && a.UserId == b.UserId;

        private static string LocalKey(string exercice, DateTime date, Guid userId) =>
            $"{userId:N}|{exercice.Trim().ToLowerInvariant()}|{date:yyyyMMdd}";

        private static string EntryStorageKey(PoidsEntryLocal entry)
        {
            var safeExercise = Uri.EscapeDataString(entry.Exercice.Trim().ToLowerInvariant());
            return $"entry_{entry.UserId:N}_{safeExercise}_{entry.Date:yyyyMMdd}";
        }

        private static PoidsEntry ToRemote(PoidsEntryLocal local) => new()
        {
            Id = local.Id,
            Exercice = local.Exercice,
            Date = local.Date,
            Poids = local.Poids,
            UserId = local.UserId,
            EnLb = local.EnLb,
            ObjectifAtteint = local.ObjectifAtteint
        };
    }
}
