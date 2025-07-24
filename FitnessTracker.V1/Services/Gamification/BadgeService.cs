using FitnessTracker.V1.Models;
using FitnessTracker.V1.Models.FitnessTracker.V1.Models;
using FitnessTracker.V1.Models.Gamification;
using FitnessTracker.V1.Services.Data;
using Microsoft.JSInterop;

namespace FitnessTracker.V1.Services
{
    public static class BadgeService
    {
        private static IJSRuntime? _jsRuntime;
        private static SupabaseService2? _supabaseService;
        public static void Inject(IJSRuntime jsRuntime, SupabaseService2 supabase)
        {
            _jsRuntime = jsRuntime;
            _supabaseService = supabase;

        }

        public static async Task VerifierBadgesAsync(GamificationDbModel gamification)
        {
            //if (!gamification.Badges.Any(b => b.Id == "objectif-1") &&
            //    exercices.Count(e => e.ObjectifAtteint) >= 5)
            //{
            //    AjouterBadge(gamification, new BadgeModel
            //    {
            //        Id = "objectif-1",
            //        Title = "Départ en force",
            //        Description = "Atteins 5 objectifs d'exercices",
            //        Icon = "bi-star-fill"
            //    });
            //}
            bool anyUnlocked = false;
            if (!gamification.Badges.Any(b => b.Id == "walk-10km") &&
            gamification.BestWalkingDistance >= 10)
            {
                AjouterBadge(gamification, new BadgeModel
                {
                    Id = "walk-10km",
                    Title = "10km parcourus",
                    Description = "Une vraie machine à marcher",
                    Emoji = "🏃",
                    Icon = "bi-shoe-print"
                });
                anyUnlocked = true;
            }
            // 🔁 Ajoute ici d'autres règles (par exemple : total XP, streak, temps d'entraînement, etc.)
            if (!gamification.Badges.Any(b => b.Id == "streak-3") &&
            gamification.StreakDays >= 3)
            {
                AjouterBadge(gamification, new BadgeModel
                {
                    Id = "streak-3",
                    Title = "3 jours d'affilée",
                    Description = "Tu as enchaîné 3 jours d'entraînement",
                    Emoji = "🔥",
                    Icon = "bi-fire"
                });
                anyUnlocked = true;
            }
            if (!gamification.Badges.Any(b => b.Id == "training-60min") &&
            gamification.TotalTrainingTimeMinutes >= 60)
            {
                AjouterBadge(gamification, new BadgeModel
                {
                    Id = "training-60min",
                    Title = "1h au compteur",
                    Description = "Tu as atteint 60 minutes d'entraînement cumulées",
                    Emoji = "⏱️",
                    Icon = "bi-clock-history"
                });
                anyUnlocked = true;
            }
            if (!gamification.Badges.Any(b => b.Id == "record-lift") &&
             gamification.BestLiftRecord >= 100)
            {
                AjouterBadge(gamification, new BadgeModel
                {
                    Id = "record-lift",
                    Title = "Force brute",
                    Description = "Tu as battu les 100 kg",
                    Emoji = "🏅",
                    Icon = "bi-bar-chart-line-fill"
                });
                anyUnlocked = true;
            }
            if (!gamification.Badges.Any(b => b.Id == "first-Thousands-Calorie-Burnt") &&
                gamification.TotalCaloriesBurned >= 1000)
            {
                AjouterBadge(gamification, new BadgeModel
                {
                    Id = "first-Thousands-Calorie-Burnt",
                    Title = "1000 calories brulé",
                    Description = "Tu fonds à vue d'oeil",
                    Icon = "bi-shoe-print"
                });
                anyUnlocked = true;
            }

            // ✅ Sauvegarde automatique si au moins un badge a été ajouté
            if (anyUnlocked && _supabaseService is not null)
            {
                await _supabaseService.UpdateGamificationAsync(gamification);
            }
        }

        private static void AjouterBadge(GamificationDbModel gamification, BadgeModel badge)
        {
            badge.Obtained = true;
            badge.ObtainedAt = DateTime.Now;
            gamification.Badges.Add(badge);

            // 🎉 Feedback UI
            _jsRuntime?.InvokeVoidAsync("showToast", $"🏅 Badge débloqué : {badge.Title} !");
            _jsRuntime?.InvokeVoidAsync("launchFireworks");

        }
        public static void InjectServices(IJSRuntime js, SupabaseService2 supabase)
        {
            _jsRuntime = js;
            _supabaseService = supabase;
        }
    }
}
