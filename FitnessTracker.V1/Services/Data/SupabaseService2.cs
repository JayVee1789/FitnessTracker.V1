// FitnessTracker.V1/Services/SupabaseService2.cs
using Blazored.LocalStorage;
using FitnessTracker.V1.Models;
using FitnessTracker.V1.Models.Gamification;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Supabase;
using System.Net.Http.Json;
using System.Text.Json;
using FTOptions = FitnessTracker.V1.Options.SupabaseOptions;   // 👈 alias anti-conflit




namespace FitnessTracker.V1.Services.Data;

/// <summary>
/// Accès / synchronisation Supabase : entrées de poids & programmes.
/// Toutes les URLs et la clé anon proviennent de SupabaseOptions (appsettings / secrets).
/// </summary>
public class SupabaseService2
{
    // ─── champs ───────────────────────────────────────────────────
    private readonly HttpClient _http;
    private readonly Client _supabase;
    private readonly ILocalStorageService _localStorage;
    private readonly FTOptions _options;       // 👈 alias

    private readonly string _entriesUrl;
    private readonly string _programmesUrl;
    private readonly string _programmesManuelsUrl;
    private const string SessionKey = "supabase_session";
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    // ─── constructeur ─────────────────────────────────────────────
    public SupabaseService2(
        HttpClient http,
        Client supabase,
        ILocalStorageService localStorage,
        IOptions<FTOptions> optionsAccessor)             // 👈 alias
    {
        _http = http;
        _supabase = supabase;
        _localStorage = localStorage;
        _options = optionsAccessor.Value;

        // Ajoute la clé anon dans chaque requête REST
        if (!_http.DefaultRequestHeaders.Contains("apikey"))
            _http.DefaultRequestHeaders.Add("apikey", _options.AnonKey);

        // URLs REST pré-construites
        var restBase = $"{_options.Url}/rest/v1";
        _entriesUrl = $"{restBase}/{_options.Tables.Entries}";
        _programmesUrl = $"{restBase}/{_options.Tables.Programmes}";
        _programmesManuelsUrl = $"{restBase}/{_options.Tables.ProgrammesManuels}";
    }

    // ─── AUTH : Bearer dynamique ──────────────────────────────────
    public bool RefreshAuthHeaders()
    {
        var token = _supabase.Auth.CurrentSession?.AccessToken;
        _http.DefaultRequestHeaders.Remove("Authorization");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("❌ Aucun token (utilisateur non connecté ou session expirée).");
            return false;
        }

        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return true;
    }

    public void ClearAuthHeaders() =>
        _http.DefaultRequestHeaders.Remove("Authorization");

    public string? GetCurrentUserId() => _supabase.Auth.CurrentUser?.Id;

    public bool IsCurrentUserAuthenticated => !string.IsNullOrEmpty(GetCurrentUserId());

    // ─── ENTRY (POIDS) ────────────────────────────────────────────
    #region Entry
    public async Task<List<PoidsEntry>> GetEntriesAsync()
    {
        var userIdString = GetCurrentUserId();
        if (string.IsNullOrEmpty(userIdString)) return new();
        var userGuid = Guid.Parse(userIdString);
        try
        {
            var res = await _supabase
                .From<PoidsEntry>()
                .Where(e => e.UserId == userGuid)
                .Get();

            return res.Models;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetEntriesAsync : {ex.Message}");
            return new();
        }
    }

    public async Task AddEntryAsync(PoidsEntry entry)
    {
        var userId = GetCurrentUserIdAsGuid();
        if (userId is null)
            throw new InvalidOperationException("Utilisateur non connecté.");

        entry.UserId = userId.Value;
        await _supabase.From<PoidsEntry>().Insert(entry);
    }

    public async Task DeleteEntriesNotInAsync(List<PoidsEntry> local)
    {
        var userId = GetCurrentUserIdAsGuid();
        if (userId is null)
            return;

        RefreshAuthHeaders();
        var remote = await GetEntriesAsync();
        var toDelete = remote.Where(r =>
            !local.Any(l => l.Exercice == r.Exercice && l.Date.Date == r.Date.Date));

        var tasks = toDelete.Select(r =>
        {
            var url = $"{_entriesUrl}?user_id=eq.{userId}&exercice=eq.{Uri.EscapeDataString(r.Exercice)}&date=eq.{r.Date:yyyy-MM-dd}";
            return _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        });

        await Task.WhenAll(tasks);
    }

    public Task RemoveByExerciceAndDateAsync(string exo, DateTime date) =>
        DeleteByExerciceAndDateAsync(exo, date);

    public async Task DeleteByExerciceAndDateAsync(string exo, DateTime date)
    {
        var userId = GetCurrentUserIdAsGuid();
        if (userId is null)
            throw new InvalidOperationException("Utilisateur non connecté.");

        RefreshAuthHeaders();
        var url = $"{_entriesUrl}?user_id=eq.{userId}&exercice=eq.{Uri.EscapeDataString(exo)}&date=eq.{date:yyyy-MM-dd}";
        var res = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        res.EnsureSuccessStatusCode();
    }
    #endregion

    // ─── PROGRAMMES UTILISATEUR ───────────────────────────────────
    #region Programmes
    public async Task<List<ProgrammeModel>> GetProgrammesAsync()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return new();

        RefreshAuthHeaders();
        var list = await _http.GetFromJsonAsync<List<ProgrammeModel>>($"{_programmesUrl}?user_id=eq.{userId}") ?? new();
        list.ForEach(p => p.Source = "auto");
        return list;
    }

    public async Task<List<ProgrammeModel>> GetProgrammesManuelsAsync()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return new();

        RefreshAuthHeaders();
        var list = await _http.GetFromJsonAsync<List<ProgrammeModel>>($"{_programmesManuelsUrl}?user_id=eq.{userId}") ?? new();
        list.ForEach(p => p.Source = "manuel");
        return list;
    }

    public async Task<List<ProgrammeModel>> GetAllProgrammesAsync()
        => (await Task.WhenAll(GetProgrammesAsync(), GetProgrammesManuelsAsync()))
           .SelectMany(p => p)
           .ToList();

    /// <summary>Ancien nom conservé pour compatibilité.</summary>
    public Task<bool> AddProgrammeAsync(ProgrammeModel p) =>
        SaveProgrammeAsync(p, isManual: false);

    public async Task<bool> SaveProgrammeAsync(ProgrammeModel p, bool isManual)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return false;

        var programmeId = p.Id == Guid.Empty ? Guid.NewGuid() : p.Id;
        p.Id = programmeId;
        p.UserId = userId;
        p.Source = isManual ? "manuel" : "auto";
        var url = isManual ? _programmesManuelsUrl : _programmesUrl;

        var localModel = new ProgrammeModelLocal
        {
            Id = programmeId,
            Nom = p.Nom,
            DateDebut = p.DateDebut,
            Contenu = p.Contenu,
            Source = p.Source
        };
        await _localStorage.SetItemAsync($"programme_{programmeId}", localModel);

        RefreshAuthHeaders();

        var payload = new
        {
            id = programmeId,
            nom = p.Nom,
            date_debut = p.DateDebut.ToString("yyyy-MM-dd"),
            contenu = p.Contenu,
            source = p.Source,
            user_id = Guid.Parse(userId)
        };

        try
        {
            var resp = await _http.PostAsJsonAsync(url, new[] { payload });
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Programme conservé localement, synchro Supabase impossible : " + ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteProgrammeUnifiedAsync(Guid id, string source)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return false;

        RefreshAuthHeaders();
        var url = source == "manuel"
            ? $"{_programmesManuelsUrl}?id=eq.{id}&user_id=eq.{userId}"
            : $"{_programmesUrl}?id=eq.{id}&user_id=eq.{userId}";

        await _localStorage.RemoveItemAsync($"programme_{id}");

        var res = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProgrammeUnifiedAsync(ProgrammeModel programme)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return false;

        await _localStorage.SetItemAsync($"programme_{programme.Id}", new ProgrammeModelLocal
        {
            Id = programme.Id,
            Nom = programme.Nom,
            DateDebut = programme.DateDebut,
            Contenu = programme.Contenu,
            Source = programme.Source
        });

        RefreshAuthHeaders();

        var url = programme.Source == "manuel"
            ? $"{_programmesManuelsUrl}?id=eq.{programme.Id}&user_id=eq.{userId}"
            : $"{_programmesUrl}?id=eq.{programme.Id}&user_id=eq.{userId}";
        var patch = new Dictionary<string, object>
        {
            ["nom"] = programme.Nom,
            ["date_debut"] = programme.DateDebut.ToString("yyyy-MM-dd"),
            ["contenu"] = programme.Contenu
        };

        var content = new StringContent(JsonSerializer.Serialize(patch),
                                        System.Text.Encoding.UTF8,
                                        "application/json");

        var res = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url) { Content = content });
        return res.IsSuccessStatusCode;
    }
    public async Task<List<ProgrammeModel>> GetAllProgrammesForCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return new();

        RefreshAuthHeaders();

        var url1 = $"{_programmesUrl}?user_id=eq.{userId}";
        var url2 = $"{_programmesManuelsUrl}?user_id=eq.{userId}";

        var auto = await _http.GetFromJsonAsync<List<ProgrammeModel>>(url1) ?? new();
        auto.ForEach(p => p.Source = "auto");

        var manuels = await _http.GetFromJsonAsync<List<ProgrammeModel>>(url2) ?? new();
        manuels.ForEach(p => p.Source = "manuel");

        return auto.Concat(manuels).ToList();
    }

    #endregion
    public async Task SaveSessionAsync()
    {
        var session = _supabase.Auth.CurrentSession;
        if (session is null)
        {
            await ClearStoredSessionAsync();
            return;
        }

        var json = JsonSerializer.Serialize(session);
        await _localStorage.SetItemAsync(SessionKey, json);
        await _localStorage.SetItemAsync(AccessTokenKey, session.AccessToken);
        await _localStorage.SetItemAsync(RefreshTokenKey, session.RefreshToken);
    }

    public async Task<bool> LoadSessionAsync()
    {
        var json = await _localStorage.GetItemAsync<string>(SessionKey);
        if (!string.IsNullOrEmpty(json))
        {
            Supabase.Gotrue.Session? session;
            try
            {
                session = JsonSerializer.Deserialize<Supabase.Gotrue.Session>(json);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Session locale corrompue : {ex.Message}");
                await ClearStoredSessionAsync();
                return false;
            }

            if (session != null)
            {
                try
                {
                    var access = await _localStorage.GetItemAsync<string>(AccessTokenKey);
                    var refresh = await _localStorage.GetItemAsync<string>(RefreshTokenKey);

                    if (string.IsNullOrEmpty(access) || string.IsNullOrEmpty(refresh))
                    {
                        await ClearStoredSessionAsync();
                        return false;
                    }

                    await _supabase.Auth.SetSession(access, refresh);
                    Console.WriteLine("🟢 Session restaurée avec succès");
                    return true;
                }
                catch (Supabase.Gotrue.Exceptions.GotrueException ex)
                {
                    Console.WriteLine($"❌ Session invalide : {ex.Message}");
                    await ClearStoredSessionAsync();
                    return false;
                }
            }
        }
        return false;
    }
    public async Task<bool> EnsureLoggedInAsync(NavigationManager nav)
    {
        var user = _supabase.Auth.CurrentUser;

        if (user != null)
            return true;

        var restored = await LoadSessionAsync();
        if (!restored || _supabase.Auth.CurrentUser == null)
        {
            Console.WriteLine("🔐 Redirection vers login : utilisateur non connecté.");
            nav.NavigateTo("/login"); // adapte ton chemin si besoin
            return false;
        }

        return true;
    }

    public string? GetUserId()
    {
        return _supabase.Auth.CurrentUser?.Id;
    }
    public Guid? GetCurrentUserIdAsGuid()
    {
        var idString = _supabase.Auth.CurrentUser?.Id;

        if (Guid.TryParse(idString, out var idGuid))
            return idGuid;
        return null;
    }

    private async Task ClearStoredSessionAsync()
    {
        await _localStorage.RemoveItemAsync(SessionKey);
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
    }


    public async Task<List<PoidsEntry>> GetPoidsEntriesFromSupabaseAsync()
{
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId))
    {
        Console.WriteLine("❌ Aucun utilisateur connecté ➡️ aucun poids récupéré.");
        return new();
    }

    try
    {
        var userGuid = Guid.Parse(userId);

        var result = await _supabase
            .From<PoidsEntry>()
            .Where(x => x.UserId == userGuid)
            .Get();

        var entries = result.Models.ToList();

        Console.WriteLine($"✅ {entries.Count} entrées poids récupérées depuis Supabase.");
        return entries;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur GetPoidsEntriesFromSupabaseAsync : {ex.Message}");
        return new();
    }


}
    public async Task<bool> UpdateGamificationAsync(GamificationDbModel gamification)
    {
        if (gamification == null || gamification.Id == Guid.Empty)
            return false;

        // 🔐 Ajoute les headers
        RefreshAuthHeaders();
        _http.DefaultRequestHeaders.Remove("apikey");
        _http.DefaultRequestHeaders.Add("apikey", _options.AnonKey);

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

        var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = content
        });

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("❌ Erreur PATCH Supabase : " + await response.Content.ReadAsStringAsync());
        }

        return response.IsSuccessStatusCode;
    }





}



