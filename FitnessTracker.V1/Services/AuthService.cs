using Blazored.LocalStorage;
using Supabase;
using static System.Net.WebRequestMethods;

namespace FitnessTracker.V1.Services
{
    public class AuthService
    {
        private readonly Client _supabase;
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public AuthService(Client supabase, ILocalStorageService localStorage, HttpClient http)
        {
            _supabase = supabase;
            _localStorage = localStorage;
            _http = http;
        }

        public bool IsAuthenticated => _supabase.Auth.CurrentUser is not null;

        public async Task<bool> SignInAsync(string email, string password)
        {
            var session = await _supabase.Auth.SignIn(email, password);
            if (session?.User is not null)
            {
                // 🔐 Sauvegarde des tokens
                await _localStorage.SetItemAsync("access_token", session.AccessToken);
                await _localStorage.SetItemAsync("refresh_token", session.RefreshToken);
                return true;
            }

            return false;
        }

        public async Task SignOutAsync()
        {
            await _supabase.Auth.SignOut();
            await _localStorage.RemoveItemAsync("supabase_session");
            _http.DefaultRequestHeaders.Remove("Authorization"); // optionnel mais propre

        }

        public async Task<string> GetCurrentUserRoleAsync()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user is null) return "anonymous";

            var profile = await _supabase
                .From<SupabaseUserProfile>()
                .Where(x => x.Id == user.Id)
                .Single();

            return profile?.Role ?? "user";
        }
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                await _supabase.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur envoi email reset : {ex.Message}");
                return false;
            }
        }


        public async Task<bool> UpdatePasswordAsync(string newPassword)
        {
            try
            {
                var user = _supabase.Auth.CurrentUser;
                if (user is null) return false;

                await _supabase.Auth.Update(new Supabase.Gotrue.UserAttributes
                {
                    Password = newPassword
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur MAJ mot de passe : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsAdminAsync() =>
            (await GetCurrentUserRoleAsync()) is "admin" or "coach";

        public async Task<bool> RestaurerSessionAsync()
        {
            try
            {
                var session = _supabase.Auth.CurrentSession;

                if (session?.AccessToken is not null)
                {
                    Console.WriteLine("✅ Session active détectée");
                    return true;
                }

                // tentative de refresh
                var refreshed = await _supabase.Auth.RefreshSession();

                if (refreshed?.AccessToken is not null)
                {
                    Console.WriteLine("✅ Session rafraîchie avec succès");
                    return true;
                }

                Console.WriteLine("❌ Session absente ou invalide");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur restauration session : {ex.Message}");
                return false;
            }
        }


    }
}
