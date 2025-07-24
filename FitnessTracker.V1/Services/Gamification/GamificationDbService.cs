using FitnessTracker.V1.Models.Gamification;
using FitnessTracker.V1.Services.Data;
using Microsoft.Extensions.Options;
using Supabase;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using FTOptions = FitnessTracker.V1.Options.SupabaseOptions;

namespace FitnessTracker.V1.Services
{
    public class GamificationDbService
    {
        private readonly Client _supabase;
        private readonly AuthService _auth;
        private readonly SupabaseService2 _supabaseService;
        private readonly HttpClient _http;
        private readonly FTOptions _options;


        public GamificationDbService(
            Client supabase,
            AuthService auth,
            SupabaseService2 supabaseService,
            HttpClient http,
            IOptions<FTOptions> options)
        {
            _supabase = supabase;
            _auth = auth;
            _supabaseService = supabaseService;
            _http = http;
            _options = options.Value; // 👈 stocké ici
        }


        public async Task<GamificationDbModel?> GetGamificationAsync()
        {
            try
            {
                var userId = _supabase.Auth.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("⚠️ Pas d'utilisateur connecté, gamification non chargée.");
                    return null;
                }

                var userGuid = Guid.Parse(userId);
                var res = await _supabase.From<GamificationDbModel>()
                    .Where(x => x.UserId == userGuid)
                    .Get();

                var gamification = res.Models.FirstOrDefault();

                if (gamification == null)
                    Console.WriteLine("⚠️ Gamification non trouvée pour l'utilisateur.");
                else
                    Console.WriteLine("✅ Gamification chargée depuis Supabase.");

                return gamification;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur GetGamificationAsync : {ex.Message}");
                return null;
            }
        }

        public async Task<GamificationDbModel> GetOrCreateGamificationAsync()
        {
            // ✅ Récupérer userId via le service
            var userId = _supabaseService.GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(userId) || userId == Guid.Empty.ToString())
            {
                // 🔁 Essayer de restaurer la session si nécessaire
                var restored = await _supabaseService.LoadSessionAsync();
                Console.WriteLine($"🔁 Tentative de restauration session : {restored}");

                userId = _supabaseService.GetCurrentUserId();
                if (string.IsNullOrWhiteSpace(userId) || userId == Guid.Empty.ToString())
                    throw new InvalidOperationException("❌ Pas d'utilisateur connecté pour création gamification.");
            }

            var gamification = await GetGamificationAsync();

            if (gamification != null)
                return gamification;

            Console.WriteLine("➕ Création de la gamification pour l'utilisateur...");
            Console.WriteLine($"👤 UserID utilisé pour Upsert : {userId}");

            gamification = new GamificationDbModel
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userId),
                TotalSeances = 0,
                TotalXP = 0,
                TotalTrainingTimeMinutes = 0,
                StreakDays = 0,
                LastSessionDate = DateTime.UtcNow
            };

            await UpdateGamificationAsync(gamification);
            Console.WriteLine("✅ Gamification créée et sauvegardée.");
            return gamification;
        }


        
        public async Task<bool> UpdateGamificationAsync(GamificationDbModel gamification)
        {
            if (gamification == null || gamification.Id == Guid.Empty)
                return false;

            // 🔐 Authentification
            var token = _supabase.Auth.CurrentSession?.AccessToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("❌ Aucun token disponible.");
                return false;
            }

            // ✅ Headers obligatoires
            _http.DefaultRequestHeaders.Remove("apikey");
            _http.DefaultRequestHeaders.Remove("Authorization");
            _http.DefaultRequestHeaders.Add("apikey", _options.AnonKey);
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var url = $"{_options.Url}/rest/v1/gamification?id=eq.{gamification.Id}";

            var patch = new Dictionary<string, object>
            {
                ["badges"] = gamification.Badges,
                ["last_session_date"] = gamification.LastSessionDate,
                ["total_xp"] = gamification.TotalXP,
                ["streak_days"] = gamification.StreakDays,
                ["total_training_time_minutes"] = gamification.TotalTrainingTimeMinutes,
                ["total_calories_burned"] = gamification.TotalCaloriesBurned,
                ["best_lift_record"] = gamification.BestLiftRecord,
                ["best_walking_distance"] = gamification.BestWalkingDistance
            };

            var content = new StringContent(
                JsonSerializer.Serialize(patch),
                System.Text.Encoding.UTF8,
                "application/json");

            var res = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = content
            });

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ PATCH Supabase échoué : " + await res.Content.ReadAsStringAsync());
            }

            return res.IsSuccessStatusCode;
        }

    }
}
