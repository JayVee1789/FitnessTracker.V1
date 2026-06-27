using Blazored.LocalStorage;
using Supabase;
namespace FitnessTracker.V1.Services
{
    public class AuthService
    {
        public record AuthOperationResult(bool Success, string Message);

        private const string SessionKey = "supabase_session";
        private const string AccessTokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";

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
            var result = await SignInWithMessageAsync(email, password);
            return result.Success;
        }

        public async Task<AuthOperationResult> SignInWithMessageAsync(string email, string password)
        {
            try
            {
                var session = await _supabase.Auth.SignIn(email, password);
                if (session?.User is not null)
                {
                    await _localStorage.SetItemAsync(AccessTokenKey, session.AccessToken);
                    await _localStorage.SetItemAsync(RefreshTokenKey, session.RefreshToken);
                    return new(true, "Connexion réussie.");
                }

                return new(false, "Échec de la connexion. Vérifie tes identifiants.");
            }
            catch (Exception ex) when (IsNetworkError(ex))
            {
                return new(false, "Impossible de joindre Supabase. Vérifie l'URL du projet Supabase et ta connexion réseau.");
            }
            catch (Exception ex)
            {
                return new(false, $"Erreur de connexion : {ex.Message}");
            }
        }

        public async Task<AuthOperationResult> SignUpAsync(string email, string password)
        {
            try
            {
                var session = await _supabase.Auth.SignUp(email, password);
                if (session?.User is null)
                    return new(false, "Échec de l'inscription. Veuillez réessayer.");

                if (!string.IsNullOrWhiteSpace(session.AccessToken))
                    await _localStorage.SetItemAsync(AccessTokenKey, session.AccessToken);

                if (!string.IsNullOrWhiteSpace(session.RefreshToken))
                    await _localStorage.SetItemAsync(RefreshTokenKey, session.RefreshToken);

                if (!string.IsNullOrWhiteSpace(session.AccessToken))
                {
                    var profile = new SupabaseUserProfile
                    {
                        Id = session.User.Id,
                        Email = email,
                        Role = "user"
                    };

                    await _supabase.From<SupabaseUserProfile>().Insert(profile);
                }

                return new(true, "Compte créé avec succès.");
            }
            catch (Exception ex) when (IsNetworkError(ex))
            {
                return new(false, "Impossible de joindre Supabase. Vérifie l'URL du projet Supabase et ta connexion réseau.");
            }
            catch (Exception ex)
            {
                return new(false, $"Erreur d'inscription : {ex.Message}");
            }
        }

        public async Task SignOutAsync()
        {
            await _supabase.Auth.SignOut();
            await _localStorage.RemoveItemAsync(SessionKey);
            await _localStorage.RemoveItemAsync(AccessTokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
            _http.DefaultRequestHeaders.Remove("Authorization");

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

        private static bool IsNetworkError(Exception ex)
        {
            return ex is HttpRequestException
                || ex.Message.Contains("ERR_NAME_NOT_RESOLVED", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("Hôte inconnu", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("host", StringComparison.OrdinalIgnoreCase);
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
