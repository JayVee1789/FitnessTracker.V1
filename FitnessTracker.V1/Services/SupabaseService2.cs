// FitnessTracker.V1/Services/SupabaseService.cs
using Blazored.LocalStorage;
using FitnessTracker.V1.Models;
using FitnessTracker.V1.Options;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Services;

/// <summary>
/// Accès / synchronisation Supabase : entrées de poids & programmes.
/// Toutes les URLs et la clé anon proviennent de SupabaseOptions (appsettings / secrets).
/// </summary>
public class SupabaseService2
{
    // --- champs ----------------------------------------------------
    private readonly HttpClient _http;
    private readonly Supabase.Client _supabase;
    private readonly ILocalStorageService _localStorage;
    private readonly SupabaseOptions _options;

    private readonly string _entriesUrl;
    private readonly string _programmesUrl;
    private readonly string _programmesManuelsUrl;

    // --- constructeur ----------------------------------------------
    public SupabaseService2(
        HttpClient http,
        Supabase.Client supabase,
        ILocalStorageService localStorage,
        IOptions<SupabaseOptions> optionsAccessor)
    {
        _http = http;
        _supabase = supabase;
        _localStorage = localStorage;
        _options = optionsAccessor.Value;

        // • Ajoute la clé anon dans chaque requête REST.
        if (!_http.DefaultRequestHeaders.Contains("apikey"))
            _http.DefaultRequestHeaders.Add("apikey", _options.AnonKey);

        // • Pré-construit les URLs REST (évite les concat’ répétées).
        var restBase = $"{_options.Url}/rest/v1";
        _entriesUrl = $"{restBase}/{_options.Tables.Entries}";
        _programmesUrl = $"{restBase}/{_options.Tables.Programmes}";
        _programmesManuelsUrl = $"{restBase}/{_options.Tables.ProgrammesManuels}";
    }

    // ----------------------------------------------------------------
    // 🔐 AUTH – Ajoute / retire le header Bearer dynamiquement
    // ----------------------------------------------------------------
    public void RefreshAuthHeaders()
    {
        var token = _supabase.Auth.CurrentSession?.AccessToken;

        if (!string.IsNullOrEmpty(token))
        {
            if (_http.DefaultRequestHeaders.Contains("Authorization"))
                _http.DefaultRequestHeaders.Remove("Authorization");

            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
        else
        {
            Console.WriteLine("❌ Aucun token trouvé. L'utilisateur n'est pas connecté ou la session a expiré.");
        }
    }

    public void ClearAuthHeaders()
    {
        if (_http.DefaultRequestHeaders.Contains("Authorization"))
            _http.DefaultRequestHeaders.Remove("Authorization");
    }

    public string? GetCurrentUserId() => _supabase.Auth.CurrentUser?.Id;

    // ----------------------------------------------------------------
    #region ENTRY (POIDS)
    // ----------------------------------------------------------------
    public async Task<List<PoidsEntry>> GetEntriesAsync()
    {
        var userId = _supabase.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("❌ Aucun utilisateur connecté → liste vide.");
            return new();
        }

        try
        {
            var result = await _supabase
                .From<PoidsEntry>()
                .Where(x => x.UserId == userId)
                .Get();

            Console.WriteLine($"✅ {result.Models.Count} entrées récupérées pour {userId}");
            return result.Models;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Erreur récupération Supabase : " + ex.Message);
            return new();
        }
    }

    public async Task AddEntryAsync(PoidsEntry entry)
    {
        var result = await _supabase
            .From<PoidsEntry>()
            .Insert(entry);

        Console.WriteLine(result.Model is null
            ? "❌ Erreur d'insertion Supabase."
            : "✅ Insertion Supabase réussie.");
    }

    public async Task DeleteEntriesNotInAsync(List<PoidsEntry> localEntries)
    {
        var remote = await GetEntriesAsync();
        var toDelete = remote.Where(r =>
            !localEntries.Any(l => l.Exercice == r.Exercice &&
                                   l.Date.Date == r.Date.Date))
                             .ToList();

        var tasks = toDelete.Select(entry =>
        {
            var url = $"{_entriesUrl}?exercice=eq.{Uri.EscapeDataString(entry.Exercice)}&date=eq.{entry.Date:yyyy-MM-dd}";
            return _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        });

        await Task.WhenAll(tasks);
    }

    public async Task RemoveByExerciceAndDateAsync(string exercice, DateTime date)
    {
        var url = $"{_entriesUrl}?exercice=eq.{Uri.EscapeDataString(exercice)}&date=eq.{date:yyyy-MM-dd}";
        await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
    }

    public async Task DeleteByExerciceAndDateAsync(string exercice, DateTime date)
    {
        var url = $"{_entriesUrl}?exercice=eq.{Uri.EscapeDataString(exercice)}&date=eq.{date:yyyy-MM-dd}";
        var res = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        res.EnsureSuccessStatusCode();
    }
    #endregion

    // ----------------------------------------------------------------
    #region PROGRAMMES UTILISATEUR
    // ----------------------------------------------------------------
    public async Task<List<ProgrammeModel>> GetProgrammesAsync()
    {
        RefreshAuthHeaders();
        var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmesUrl);
        foreach (var p in result ?? new()) p.Source = "auto";
        return result ?? new();
    }

    public async Task<List<ProgrammeModel>> GetProgrammesManuelsAsync()
    {
        RefreshAuthHeaders();
        var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmesManuelsUrl);
        foreach (var p in result ?? new()) p.Source = "manuel";
        return result ?? new();
    }

    public async Task<List<ProgrammeModel>> GetAllProgrammesAsync()
        => (await Task.WhenAll(GetProgrammesAsync(), GetProgrammesManuelsAsync()))
           .SelectMany(x => x)
           .ToList();

    public async Task<bool> AddProgrammeAsync(ProgrammeModel p)
    {
        var userId = _supabase.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("❌ Aucun utilisateur connecté");
            return false;
        }

        p.UserId = userId;

        RefreshAuthHeaders();
        Console.WriteLine("Payload JSON →\n" +
            JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));

        var response = await _http.PostAsJsonAsync(_programmesUrl, new[] { p });
        Console.WriteLine("Code retour : " + response.StatusCode);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Ajoute ou met à jour un programme (manuel ou auto) en un seul appel.
    /// </summary>
    public async Task<bool> SaveProgrammeAsync(ProgrammeModel p, bool isManual)
    {
        var userId = _supabase.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("❌ Aucun utilisateur connecté.");
            return false;
        }

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

        var response = await _http.PostAsJsonAsync(url, new[] { payload });

        Console.WriteLine("➡️ JSON envoyé :\n" +
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"➡️ Vers URL : {url}");
        Console.WriteLine("➡️ Code retour : " + response.StatusCode);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProgrammeUnifiedAsync(Guid id, string source)
    {
        var url = source == "manuel"
            ? $"{_programmesManuelsUrl}?id=eq.{id}"
            : $"{_programmesUrl}?id=eq.{id}";

        var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        Console.WriteLine($"[DELETE {source}] {url} → {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProgrammeUnifiedAsync(ProgrammeModel programme)
    {
        RefreshAuthHeaders();

        var url = programme.Source == "manuel"
            ? $"{_programmesManuelsUrl}?id=eq.{programme.Id}"
            : $"{_programmesUrl}?id=eq.{programme.Id}";

        var payload = new Dictionary<string, object>
        {
            ["nom"] = programme.Nom,
            ["date_debut"] = programme.DateDebut.ToString("yyyy-MM-dd"),
            ["contenu"] = programme.Contenu
        };

        var content = new StringContent(JsonSerializer.Serialize(payload),
                                          System.Text.Encoding.UTF8,
                                          "application/json");

        var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url) { Content = content });

        Console.WriteLine($"[PATCH programme] {url} → {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    // --- helpers de conversion (local ↔ remote) --------------------
    private ProgrammeModelLocal ConvertToLocal(ProgrammeModel remote) => new()
    {
        Id = remote.Id,
        Nom = remote.Nom,
        DateDebut = remote.DateDebut,
        Contenu = remote.Contenu,
        Source = remote.Source
    };

    private ProgrammeModel ConvertToRemote(ProgrammeModelLocal local, string userId) => new()
    {
        Id = local.Id,
        Nom = local.Nom,
        DateDebut = local.DateDebut,
        Contenu = local.Contenu,
        Source = local.Source,
        UserId = userId
    };
    #endregion
}
