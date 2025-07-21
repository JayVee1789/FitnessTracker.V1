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


        public async Task UpsertGamificationAsync(GamificationDbModel gamification)
        {
            try
            {
                await _supabase.From<GamificationDbModel>().Upsert(gamification);
            }
            catch(Exception ex)
            {
                Console.WriteLine(  ex);
            }
            
        }
    }
}
