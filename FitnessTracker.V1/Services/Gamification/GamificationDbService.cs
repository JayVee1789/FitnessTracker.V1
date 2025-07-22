using FitnessTracker.V1.Models.Gamification;
using Supabase;

namespace FitnessTracker.V1.Services
{
    public class GamificationDbService
    {
        private readonly Client _supabase;
        private readonly AuthService _auth;

        public GamificationDbService(Client supabase, AuthService auth)
        {
            _supabase = supabase;
            _auth = auth;
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
            var gamification = await GetGamificationAsync();
            if (gamification != null)
                return gamification;

            Console.WriteLine("➕ Création de la gamification pour l'utilisateur...");

            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("❌ Pas d'utilisateur connecté pour création gamification.");

            gamification = new GamificationDbModel
            {
                UserId = Guid.Parse(userId),
                TotalSeances = 0,
                TotalXP = 0,
                TotalTrainingTimeMinutes = 0,
                StreakDays = 0,
                LastSessionDate = DateTime.UtcNow
            };
            Console.WriteLine($"👤 UserID utilisé pour Upsert : {userId}");

            await UpsertGamificationAsync(gamification);
            Console.WriteLine("✅ Gamification créée et sauvegardée.");
            return gamification;
        }

        public async Task UpsertGamificationAsync(GamificationDbModel gamification)
        {
            try
            {
                await _supabase.From<GamificationDbModel>().Upsert(gamification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur Upsert : {ex.Message}");
            }
        }
    }
}
