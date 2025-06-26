using Blazored.LocalStorage;
using FitnessTracker.V1.Models;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using static FitnessTracker.V1.Models.Model;

public class SupabaseService
{
    private readonly HttpClient _http;
    private readonly string _tableUrl;
    private readonly string _apiKey;
    private readonly string _programmeUrl;
    private readonly string _programmeManuelUrl;
    private readonly Supabase.Client _supabase;
    private readonly ILocalStorageService LocalStorage;


    public SupabaseService(HttpClient http, Supabase.Client supabase, ILocalStorageService localStorage)
    {

        _http = http;
        _supabase = supabase;
        _apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp2c2hhcGRsd3p6eXRwbXZnbWliIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMDg5NjEsImV4cCI6MjA2MzY4NDk2MX0.DKx7CvWsfo9b5V6-vShqHXU1eNrvYXYDP26uOtEghCc"; // À récupérer dans Supabase > Project Settings > API > anon key
        _tableUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/entries"; // Voir dans Supabase > API > REST
        _programmeUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/programmes";
        _programmeManuelUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/programmes_manuels";
        _http.DefaultRequestHeaders.Add("apikey", _apiKey);
        LocalStorage = localStorage;
    }
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

    //public async Task<List<PoidsEntry>> GetEntriesAsync()
    //{
    //  var response  = new List<PoidsEntry>();
    //    try
    //    {
    //        response = await _http.GetFromJsonAsync<List<PoidsEntry>>(_tableUrl);

    //    }catch(Exception ex)
    //    {
    //        Console.WriteLine(ex);
    //    }
    //    return response ?? new();

    //}
    public async Task<List<PoidsEntry>> GetEntriesAsync()
    {
        var userId = _supabase.Auth.CurrentUser?.Id;

        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("❌ Aucun utilisateur connecté → GetEntriesAsync retourne une liste vide.");
            return new();
        }

        try
        {
            var result = await _supabase
                .From<PoidsEntry>()
                .Where(x => x.UserId == userId)
                .Get();

            Console.WriteLine($"✅ {result.Models.Count} entrées récupérées pour l'utilisateur {userId}");
            return result.Models;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Erreur récupération Supabase : " + ex.Message);
            return new();
        }
    }

    #region ENTRY (POIDS)
    public async Task AddEntryAsync(PoidsEntry entry)
    {

        var result = await _supabase
       .From<PoidsEntry>()
       .Insert(entry);

        if (result.Model is null)
            Console.WriteLine("❌ Erreur d'insertion Supabase.");
        else
            Console.WriteLine("✅ Insertion Supabase réussie.");

    }
    public async Task DeleteEntriesNotInAsync(List<PoidsEntry> localEntries)
    {
        var remote = await GetEntriesAsync();

        var toDelete = remote.Where(r =>
            !localEntries.Any(l => l.Exercice == r.Exercice && l.Date.Date == r.Date.Date)).ToList();

        foreach (var entry in toDelete)
        {
            var url = $"{_tableUrl}?exercice=eq.{Uri.EscapeDataString(entry.Exercice)}&date=eq.{entry.Date:yyyy-MM-dd}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            await _http.SendAsync(request);
        }
    }
    #endregion

    #region EXERCICES
    public async Task RemoveByExerciceAndDateAsync(string exercice, DateTime date)
    {

        var url = $"{_tableUrl}?exercice=eq.{Uri.EscapeDataString(exercice)}&date=eq.{date:yyyy-MM-dd}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        await _http.SendAsync(request);

    }
    
    public async Task DeleteByExerciceAndDateAsync(string exercice, DateTime date)
    {
        var url = $"{_tableUrl}?exercice=eq.{Uri.EscapeDataString(exercice)}&date=eq.{date:yyyy-MM-dd}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
    #endregion

    #region PROGRAMMES UTILISATEUR

    public async Task<List<ProgrammeModel>> GetProgrammesAsync()
    {
        RefreshAuthHeaders();
        var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeUrl);
        foreach (var p in result ?? new()) p.Source = "auto";
        return result ?? new();
    }

    public async Task<List<ProgrammeModel>> GetProgrammesManuelsAsync()
    {
        RefreshAuthHeaders();
        var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeManuelUrl);
        foreach (var p in result ?? new()) p.Source = "manuel";
        return result ?? new();
    }

    public async Task<List<ProgrammeModel>> GetAllProgrammesAsync()
    {
        var auto = await GetProgrammesAsync();
        var manuels = await GetProgrammesManuelsAsync();
        return auto.Concat(manuels).ToList();
    }

    //public async Task<bool> SaveProgrammeUnifiedAsync(ProgrammeModel programme)
    //{
    //    // 🔐 Vérifie que l'utilisateur est connecté
    //    var userId = _supabase.Auth.CurrentUser?.Id;
    //    if (string.IsNullOrWhiteSpace(userId))
    //    {
    //        Console.WriteLine("❌ Aucun utilisateur connecté.");
    //        return false;
    //    }

    //    programme.UserId = userId;

    //    // 1️⃣ ⬇️ Enregistrement local (clé programmes_local)
    //    try
    //    {
    //        var localProgrammes = await LocalStorage.GetItemAsync<List<ProgrammeModel>>("programmes_local") ?? new();

    //        // supprime s'il existe déjà
    //        localProgrammes.RemoveAll(p => p.Id == programme.Id);

    //        // ajoute ou remplace
    //        localProgrammes.Add(programme);

    //        await LocalStorage.SetItemAsync("programmes_local", localProgrammes);

    //        Console.WriteLine("✅ Programme enregistré localement.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("❌ Erreur lors de l'enregistrement local : " + ex.Message);
    //        return false;
    //    }

    //    // 2️⃣ ☁️ Enregistrement Supabase
    //    try
    //    {
    //        RefreshAuthHeaders();

    //        var url = programme.Source == "manuel" ? _programmeManuelUrl : _programmeUrl;

    //        var payload = new
    //        {
    //            id = programme.Id == Guid.Empty ? Guid.NewGuid() : programme.Id,
    //            nom = programme.Nom,
    //            date_debut = programme.DateDebut.ToString("yyyy-MM-dd"),
    //            contenu = programme.Contenu,
    //            source = programme.Source,
    //            user_id = programme.UserId
    //        };

    //        var response = await _http.PostAsJsonAsync(url, new[] { payload });

    //        Console.WriteLine($"➡️ Envoi vers Supabase ({programme.Source}) → {response.StatusCode}");

    //        return response.IsSuccessStatusCode;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("❌ Erreur lors de la sauvegarde Supabase : " + ex.Message);
    //        return false;
    //    }
    //}



    public async Task<bool> AddProgrammeAsync(ProgrammeModel p)
    {
        // Vérification sécurisée
        var userId = _supabase.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("❌ Aucun utilisateur connecté");
            return false;
        }

        p.UserId = userId;
        Console.WriteLine("Payload JSON →");
        Console.WriteLine(JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));
        RefreshAuthHeaders();

        var response = await _http.PostAsJsonAsync(_programmeUrl, new[] { p });

        Console.WriteLine("Envoi JSON :\n" + JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Code retour : " + response.StatusCode);

        return response.IsSuccessStatusCode;
    }

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


    //public async Task<bool> SaveManualPlanAsync(string nom, WorkoutPlan plan)
    //{
    //    var userId = _supabase.Auth.CurrentUser?.Id;
    //    if (string.IsNullOrEmpty(userId))
    //    {
    //        Console.WriteLine("❌ Aucun utilisateur connecté");
    //        return false;
    //    }

    //    var p = new
    //    {
    //        id = Guid.NewGuid(),
    //        nom = nom,
    //        datedebut = DateTime.Today.ToString("yyyy-MM-dd"), // sécurise le format date
    //        contenu = JsonSerializer.Serialize(plan), // ✅ on sérialise en texte
    //        source = "manuel",
    //        user_id = userId
    //    };

    //    var response = await _http.PostAsJsonAsync(_programmeManuelUrl, new[] { p });

    //    Console.WriteLine("Envoi JSON vers Supabase (manuel) :\n" + JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));
    //    Console.WriteLine("Code retour : " + response.StatusCode);

    //    return response.IsSuccessStatusCode;
    //}

    public async Task<bool> DeleteProgrammeUnifiedAsync(Guid id, string source)
    {
        var url = source == "manuel"
            ? $"{_programmeManuelUrl}?id=eq.{id}"
            : $"{_programmeUrl}?id=eq.{id}";

        var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        Console.WriteLine($"[DELETE {source}] {url} → {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    //public async Task<bool> UpdateManualProgrammeAsync(Guid id, string nom, WorkoutPlan plan)
    //{
    //    var url = $"{_programmeManuelUrl}?id=eq.{id}";
    //    var payload = new Dictionary<string, object>
    //    {
    //        ["nom"] = nom,
    //        ["datedebut"] = DateTime.Today,
    //        ["contenu"] = JsonSerializer.Serialize(plan)
    //    };

    //    var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
    //    var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url) { Content = content });

    //    Console.WriteLine($"[UPDATE MANUEL] {url} → {response.StatusCode}");
    //    return response.IsSuccessStatusCode;
    ////}
    
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

        var url = isManual ? _programmeManuelUrl : _programmeUrl;

        // ✅ Injecte le bon token
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

        Console.WriteLine("➡️ JSON envoyé :\n" + JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"➡️ Vers URL : {url}");
        Console.WriteLine("➡️ Code retour : " + response.StatusCode);

        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> UpdateProgrammeUnifiedAsync(ProgrammeModel programme)
    {
        RefreshAuthHeaders();

        var url = programme.Source == "manuel"
            ? $"{_programmeManuelUrl}?id=eq.{programme.Id}"
            : $"{_programmeUrl}?id=eq.{programme.Id}";

        var payload = new Dictionary<string, object>
        {
            ["nom"] = programme.Nom,
            ["date_debut"] = programme.DateDebut.ToString("yyyy-MM-dd"),
            ["contenu"] = programme.Contenu
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url) { Content = content });

        Console.WriteLine($"[PATCH programme] {url} → {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    public string? GetCurrentUserId()
    {
        return _supabase.Auth.CurrentUser?.Id;
    }
    public void ClearAuthHeaders()
    {
        if (_http.DefaultRequestHeaders.Contains("Authorization"))
            _http.DefaultRequestHeaders.Remove("Authorization");
    }
}
