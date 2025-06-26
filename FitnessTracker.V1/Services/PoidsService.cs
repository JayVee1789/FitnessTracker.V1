using Blazored.LocalStorage;
using FitnessTracker.V1.Models;
using System.Text.Json;

namespace FitnessTracker.V1.Services
{
    public class PoidsService
    {
        private const string StorageKey = "poids_entries";
        private const string ExercicesKey = "exercise_list";

        private readonly ILocalStorageService _localStorage;
        private readonly SupabaseService _supabase;
        private const string PoidsKeysListKey = "poids_keys_list";

        public PoidsService(ILocalStorageService localStorage, SupabaseService supabase)
        {
            _localStorage = localStorage;
            _supabase = supabase;
        }

        //public async Task<List<PoidsEntry>> GetEntriesAsync()
        //{
        //    var userId = _supabase.GetCurrentUserId();
        //    try
        //    {
        //        var local = await _localStorage.GetItemAsync<List<PoidsEntryLocal>>(StorageKey);
        //        return local?.Select(e => new PoidsEntry
        //        {
        //            Id = e.Id,
        //            Date = e.Date,
        //            Exercice = e.Exercice,
        //            Poids = e.Poids,
        //            UserId = userId ?? ""
        //        }).ToList() ?? new();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("❌ Erreur lecture localStorage, reset...");
        //        Console.WriteLine(ex);
        //        await _localStorage.RemoveItemAsync(StorageKey);
        //        return new();
        //    }
        //}
        public async Task<List<PoidsEntry>> GetEntriesAsync()
        {
            var userId = _supabase.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("❌ Aucun utilisateur connecté pour filtrer les poids.");
                return new();
            }

            try
            {
                var local = await _localStorage.GetItemAsync<List<PoidsEntryLocal>>(StorageKey);
                return local?
                    .Where(e => e.UserId == userId)
                    .Select(e => new PoidsEntry
                    {
                        Id = e.Id,
                        Date = e.Date,
                        Exercice = e.Exercice,
                        Poids = e.Poids,
                        UserId = e.UserId
                    })
                    .ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erreur lecture localStorage, reset...");
                Console.WriteLine(ex);
                await _localStorage.RemoveItemAsync(StorageKey);
                return new();
            }
        }


        public async Task AddEntryAsync(PoidsEntry entry, PoidsEntryLocal local)
        {
            var userId = _supabase.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("❌ Utilisateur non connecté. user_id requis pour Supabase.");
                return;
            }

            var key = $"entry_{entry.Exercice}_{entry.Date:yyyyMMdd}";

            local = new PoidsEntryLocal
            {
                Exercice = entry.Exercice,
                Date = entry.Date,
                Poids = entry.Poids
            };

            try
            {
                await _localStorage.SetItemAsync(key, local);
                Console.WriteLine($"✅ Local enregistré : {key}");

                // 🔐 Enregistrer la clé si elle n'existe pas déjà
                var allKeys = await _localStorage.GetItemAsync<List<string>>(PoidsKeysListKey) ?? new();
                if (!allKeys.Contains(key))
                {
                    allKeys.Add(key);
                    await _localStorage.SetItemAsync(PoidsKeysListKey, allKeys);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erreur localStorage : " + ex.Message);
            }

            entry.UserId = userId;

            try
            {
                entry.UserId = _supabase.GetCurrentUserId() ?? "";
                await _supabase.AddEntryAsync(entry);
                Console.WriteLine("✅ Synchro Supabase réussie");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erreur lors de l'ajout dans Supabase : " + ex.Message);
            }
        }
        public async Task<List<PoidsEntryLocal>> GetAllLocalPoidsAsync()
        {
            var list = new List<PoidsEntryLocal>();
            var keys = await _localStorage.GetItemAsync<List<string>>(PoidsKeysListKey) ?? new();

            foreach (var key in keys)
            {
                try
                {
                    var entry = await _localStorage.GetItemAsync<PoidsEntryLocal>(key);
                    if (entry != null)
                        list.Add(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur lecture clé {key} : {ex.Message}");
                }
            }

            return list;
        }


        public async Task OverwriteEntriesAsync(List<PoidsEntry> entries)
        {
            var locals = entries.Select(e => new PoidsEntryLocal
            {
                Id = e.Id,
                Exercice = e.Exercice,
                Date = e.Date,
                Poids = e.Poids,
                UserId = e.UserId
            }).ToList();

            await _localStorage.SetItemAsync(StorageKey, locals);
        }

        public async Task RemoveEntryAsync(string exercice, DateTime date)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Exercice == exercice && e.Date.Date == date.Date);
            await OverwriteEntriesAsync(entries);
        }

        public async Task RemoveDateAsync(DateTime date)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Date.Date == date.Date);
            await OverwriteEntriesAsync(entries);
        }

        public async Task RemoveExerciceAsync(string exercice)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Exercice == exercice);
            await OverwriteEntriesAsync(entries);
        }

        public async Task RemoveByIdAsync(Guid id)
        {
            var entries = await GetEntriesAsync();
            entries.RemoveAll(e => e.Id == id);
            await OverwriteEntriesAsync(entries);
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
            var remote = await _supabase.GetEntriesAsync();
            if (remote != null)
            {
                await OverwriteEntriesAsync(remote);
            }
        }

        public async Task<double?> GetPoidsForExerciceAtDateAsync(string exercice, DateTime date)
        {
            var entries = await GetEntriesAsync();
            return entries.FirstOrDefault(e => e.Exercice == exercice && e.Date.Date == date.Date)?.Poids;
        }

        public async Task SavePoidsLocalAsync(PoidsEntry entry)
        {
            var key = $"entry_{entry.Exercice}_{entry.Date:yyyyMMdd}";

            var local = new PoidsEntryLocal
            {
                Id = entry.Id,
                Exercice = entry.Exercice,
                Date = entry.Date,
                Poids = entry.Poids,
                UserId = entry.UserId
            };

            await _localStorage.SetItemAsync(key, local);
        }
        public async Task SupprimerDepuisSupabaseAsync(string exercice, DateTime date)
        {
            await _supabase.RemoveByExerciceAndDateAsync(exercice, date);
        }

        //public async Task SaveProgrammeLocallyAsync(ProgrammeModel programme)
        //{
        //    try
        //    {
        //        var json = JsonSerializer.Serialize(programme);
        //        await SecureStorage.Default.SetAsync($"programme_{programme.Id}", json);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("❌ Erreur enregistrement local programme : " + ex.Message);
        //    }
        //}
        public async Task SaveEntryUnifiedAsync(PoidsEntry remote, PoidsEntryLocal local)
        {
            try
            {
                await SupprimerDepuisSupabaseAsync(remote.Exercice, remote.Date);
                await AddEntryAsync(remote, local);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erreur SaveEntryUnifiedAsync : " + ex.Message);
            }
        }

    }
}
