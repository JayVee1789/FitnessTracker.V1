using Blazored.LocalStorage;
using FitnessTracker.V1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessTracker.V1.Services.Gamification
{
    public class GamificationState
    {
        public int TotalXP { get; set; } = 0;
        public int Level => (int)Math.Sqrt(TotalXP / 100);
        public List<string> Badges { get; set; } = new();
    }

    public class GamificationManager
    {
        private const string StorageKey = "gamification_state";

        private readonly ILocalStorageService _localStorage;
        private readonly SupabaseService2 _supabase;

        public GamificationState State { get; private set; } = new();

        public GamificationManager(ILocalStorageService localStorage, SupabaseService2 supabase)
        {
            _localStorage = localStorage;
            _supabase = supabase;
        }

        public async Task InitializeAsync()
        {
            var saved = await _localStorage.GetItemAsync<GamificationState>(StorageKey);
            State = saved ?? new GamificationState();
        }

        public async Task AddXP(int xp, string reason = "")
        {
            State.TotalXP += xp;
            Console.WriteLine($"✅ XP +{xp} pour {reason}");
            await SaveStateAsync();
        }

        public async Task UnlockBadge(string badge)
        {
            if (!State.Badges.Contains(badge))
            {
                State.Badges.Add(badge);
                Console.WriteLine($"🏅 Badge débloqué : {badge}");
                await SaveStateAsync();
            }
        }

        private async Task SaveStateAsync()
        {
            await _localStorage.SetItemAsync(StorageKey, State);
        }

        public async Task ResetGamification()
        {
            State = new GamificationState();
            await _localStorage.RemoveItemAsync(StorageKey);
            Console.WriteLine("🎯 Gamification reset");
        }
    }
}
