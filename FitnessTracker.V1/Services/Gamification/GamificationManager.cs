using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.V1.Models;
using FitnessTracker.V1.Services.Data;

namespace FitnessTracker.V1.Services.Gamification
{
    public class GamificationState
    {
        public int TotalXP { get; set; } = 0;
        public int Level => (int)Math.Sqrt(TotalXP / 100);
        public List<string> Badges { get; set; } = new();
        public DateTime LastSessionDate { get; set; } = DateTime.MinValue;
        public int Streak { get; set; } = 0;
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

        public async Task SaveStateAsync()
        {
            await _localStorage.SetItemAsync(StorageKey, State);
        }

        public async Task ResetGamification()
        {
            State = new GamificationState();
            await _localStorage.RemoveItemAsync(StorageKey);
            Console.WriteLine("🎯 Gamification reset");
        }

        //streak 
        public void IncrementStreak()
        {
            State.Streak++;
            Console.WriteLine($"🔥 Streak augmenté : {State.Streak} jours");
        }
        public void ResetStreak()
        {
            State.Streak = 0;
            Console.WriteLine("💤 Streak réinitialisé");
        }
        public void UpdateStreak()
        {
            var today = DateTime.UtcNow.Date;

            // Premier lancement ou jamais enregistré
            if (State.LastSessionDate == DateTime.MinValue)
            {
                State.Streak = 1;
                State.LastSessionDate = today;
                Console.WriteLine($"🔥 Nouveau streak démarré : {State.Streak} jour");
                return;
            }

            var daysSinceLastSession = (today - State.LastSessionDate).Days;

            if (daysSinceLastSession == 1)
            {
                State.Streak++;
                Console.WriteLine($"🔥 Streak continué : {State.Streak} jours");
            }
            else if (daysSinceLastSession >= 2)
            {
                State.Streak = 1;
                Console.WriteLine("💤 Streak remis à zéro");
            }
            else
            {
                // connecté le même jour, pas de changement
                Console.WriteLine("📅 Même jour, streak inchangé");
            }

            State.LastSessionDate = today;
        }

    }
}
