using FitnessTracker.V1.Models.Gamification;
using FitnessTracker.V1.Services.Data;
using Supabase;

namespace FitnessTracker.V1.Services
{
    public class GamificationDbService
    {
        private readonly Client _supabase;
        private readonly AuthService _auth;
        private readonly SupabaseService2 _supabaseService;
        public GamificationDbService(Client supabase, AuthService auth, SupabaseService2 supabaseService)
        {
            _supabase = supabase;
            _auth = auth;
            _supabaseService = supabaseService;
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

            await UpsertGamificationAsync(gamification);
            Console.WriteLine("✅ Gamification créée et sauvegardée.");
            return gamification;
        }

        //public async Task UpsertGamificationAsync(GamificationDbModel gamification)
        //{
        //    try
        //    {
        //        await _supabase.From<GamificationDbModel>().Upsert(gamification);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ Erreur Upsert : {ex.Message}");
        //    }
        //}
        public async Task UpsertGamificationAsync(GamificationDbModel gamification)
        {
            var token = _supabase.Auth.CurrentSession?.AccessToken;
            var user = _supabase.Auth.CurrentUser;

            Console.WriteLine($"🔐 Token = {token}");
            Console.WriteLine($"👤 CurrentUser ID = {user?.Id}");

            if (string.IsNullOrEmpty(token) || user == null)
            {
                Console.WriteLine("❌ Impossible d'upsert → utilisateur non authentifié ou token manquant.");
                return;
            }

            try
            {
                await _supabase.From<GamificationDbModel>()
                    .Upsert(gamification); // Sans options
                Console.WriteLine("✅ Gamification upsert réussie.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erreur pendant l'upsert : " + ex.Message);
            }
        }


    }
}
