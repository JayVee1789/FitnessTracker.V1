// FitnessTracker.V1/Services/SupabaseService2.cs
using Blazored.LocalStorage;
using FitnessTracker.V1.Models;
using Microsoft.Extensions.Options;
using Supabase;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using static FitnessTracker.V1.Models.Model;
using FTOptions = FitnessTracker.V1.Options.SupabaseOptions;   // 👈 alias anti-conflit

namespace FitnessTracker.V1.Services;

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
    public void RefreshAuthHeaders()
    {
        var token = _supabase.Auth.CurrentSession?.AccessToken;
        _http.DefaultRequestHeaders.Remove("Authorization");

        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        else
            Console.WriteLine("❌ Aucun token (utilisateur non connecté ou session expirée).");
    }

    public void ClearAuthHeaders() =>
        _http.DefaultRequestHeaders.Remove("Authorization");

    public string? GetCurrentUserId() => _supabase.Auth.CurrentUser?.Id;

    // ─── ENTRY (POIDS) ────────────────────────────────────────────
    #region Entry
    public async Task<List<PoidsEntry>> GetEntriesAsync()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return new();

        try
        {
            var res = await _supabase
                .From<PoidsEntry>()
                .Where(e => e.UserId == userId)
                .Get();

            return res.Models;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetEntriesAsync : {ex.Message}");
            return new();
        }
    }

    public async Task AddEntryAsync(PoidsEntry entry) =>
        await _supabase.From<PoidsEntry>().Insert(entry);

    public async Task DeleteEntriesNotInAsync(List<PoidsEntry> local)
    {
        var remote = await GetEntriesAsync();
        var toDelete = remote.Where(r =>
            !local.Any(l => l.Exercice == r.Exercice && l.Date.Date == r.Date.Date));

        var tasks = toDelete.Select(r =>
        {
            var url = $"{_entriesUrl}?exercice=eq.{Uri.EscapeDataString(r.Exercice)}&date=eq.{r.Date:yyyy-MM-dd}";
            return _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        });

        await Task.WhenAll(tasks);
    }

    public Task RemoveByExerciceAndDateAsync(string exo, DateTime date) =>
        DeleteByExerciceAndDateAsync(exo, date);

    public async Task DeleteByExerciceAndDateAsync(string exo, DateTime date)
    {
        var url = $"{_entriesUrl}?exercice=eq.{Uri.EscapeDataString(exo)}&date=eq.{date:yyyy-MM-dd}";
        var res = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        res.EnsureSuccessStatusCode();
    }
    #endregion

    // ─── PROGRAMMES UTILISATEUR ───────────────────────────────────
    #region Programmes
    public async Task<List<ProgrammeModel>> GetProgrammesAsync()
    {
        RefreshAuthHeaders();
        var list = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmesUrl) ?? new();
        list.ForEach(p => p.Source = "auto");
        return list;
    }

    public async Task<List<ProgrammeModel>> GetProgrammesManuelsAsync()
    {
        RefreshAuthHeaders();
        var list = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmesManuelsUrl) ?? new();
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

        p.UserId = userId;
        p.Source = isManual ? "manuel" : "auto";
        var url = isManual ? _programmesManuelsUrl : _programmesUrl;

        RefreshAuthHeaders();

        var payload = new
        {
            id = p.Id == Guid.Empty ? Guid.NewGuid() : p.Id,
            nom = p.Nom,
            date_debut = p.DateDebut.ToString("yyyy-MM-dd"),
            contenu = p.Contenu,
            source = p.Source,
            user_id = Guid.Parse(userId)
        };

        var resp = await _http.PostAsJsonAsync(url, new[] { payload });
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProgrammeUnifiedAsync(Guid id, string source)
    {
        var url = source == "manuel"
            ? $"{_programmesManuelsUrl}?id=eq.{id}"
            : $"{_programmesUrl}?id=eq.{id}";

        var res = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProgrammeUnifiedAsync(ProgrammeModel programme)
    {
        RefreshAuthHeaders();

        var url = programme.Source == "manuel"
            ? $"{_programmesManuelsUrl}?id=eq.{programme.Id}"
            : $"{_programmesUrl}?id=eq.{programme.Id}";

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
    #endregion
}
