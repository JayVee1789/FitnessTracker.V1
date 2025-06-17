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
    public SupabaseService(HttpClient http)
    {

        _http = http;

        _apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp2c2hhcGRsd3p6eXRwbXZnbWliIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMDg5NjEsImV4cCI6MjA2MzY4NDk2MX0.DKx7CvWsfo9b5V6-vShqHXU1eNrvYXYDP26uOtEghCc"; // À récupérer dans Supabase > Project Settings > API > anon key
        _tableUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/entries"; // Voir dans Supabase > API > REST
        _programmeUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/programmes";
        _programmeManuelUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/programmes_manuels";
        _http.DefaultRequestHeaders.Add("apikey", _apiKey);
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

    }

    public async Task<List<PoidsEntry>> GetEntriesAsync()
    {
      var response  = new List<PoidsEntry>();
        try
        {
            response = await _http.GetFromJsonAsync<List<PoidsEntry>>(_tableUrl);
           
        }catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
        return response ?? new();

    }

    #region ENTRY (POIDS)
    public async Task AddEntryAsync(PoidsEntry entry)
    {

        await RemoveByExerciceAndDateAsync(entry.Exercice, entry.Date);
        var response = await _http.PostAsJsonAsync(_tableUrl, new[] { entry });
        response.EnsureSuccessStatusCode();

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

    #region PROGRAMME
    // Récupère les programmes générés automatiquement
    public async Task<List<ProgrammeModel>> GetProgrammesAsync()
    {
        //var programmesAuto = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeUrl);

        //var programmesManuels = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeManuelUrl); 



        //var tous = (programmesAuto ?? new()).Concat(programmesManuels ?? new()).ToList();

        //var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeUrl);
        //return tous ?? new();
        var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeUrl);
        foreach (var p in result ?? new()) p.Source = "auto";
        return result ?? new();
    }
    // Récupère les programmes manuels
    public async Task<List<ProgrammeModel>> GetProgrammesManuelsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeManuelUrl);
        foreach (var p in result ?? new()) p.Source = "manuel";
        return result ?? new();
    }
    
    // Combine les deux sources
    public async Task<List<ProgrammeModel>> GetAllProgrammesAsync()
    {
        var auto = await GetProgrammesAsync();
        var manuels = await GetProgrammesManuelsAsync();
        return auto.Concat(manuels).ToList();
    }
    // Suppression unifiée selon la source
    public async Task<bool> DeleteProgrammeUnifiedAsync(Guid id, string source)
    {
        var url = source == "manuel"
            ? $"{_programmeManuelUrl}?id=eq.{id}"
            : $"{_programmeUrl}?id=eq.{id}";

        var response = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
        Console.WriteLine($"[DELETE {source}] {url} → {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    // Enregistrement manuel dans la table programmes_manuels PROGRAMME
    public async Task<bool> SaveManualPlanAsync(string nom, WorkoutPlan plan)
    {
        var p = new Dictionary<string, object>
        {
            ["id"] = Guid.NewGuid(),
            ["nom"] = nom,
            ["datedebut"] = DateTime.Today,
            ["contenu"] = JsonSerializer.Serialize(plan)
        };

        var response = await _http.PostAsJsonAsync(_programmeManuelUrl, new[] { p });

        var debugJson = JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("Envoi JSON vers Supabase (manuel) :\n" + debugJson);
        Console.WriteLine("Code retour : " + response.StatusCode);

        return response.IsSuccessStatusCode;
    }
    public async Task<bool> AddProgrammeAsync(ProgrammeModel p)
    {
        var response = await _http.PostAsJsonAsync(_programmeUrl, new[] { p });

        var debugJson = JsonSerializer.Serialize(p);
        Console.WriteLine("Envoi JSON vers Supabase : " + debugJson);
        Console.WriteLine("Code retour : " + response.StatusCode);

        return response.IsSuccessStatusCode;
    }
    public async Task<bool> DeleteProgrammeAsync(Guid id)
    {
        var url = $"{_programmeUrl}?id=eq.{id}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        var response = await _http.SendAsync(request);

        Console.WriteLine($"[SUPABASE DELETE] {url} → {response.StatusCode}");

        return response.IsSuccessStatusCode;
    }
    public async Task<bool> UpdateManualProgrammeAsync(Guid id, string nom, WorkoutPlan plan)
    {
        var url = $"{_programmeManuelUrl}?id=eq.{id}";
        var payload = new Dictionary<string, object>
        {
            ["nom"] = nom,
            ["datedebut"] = DateTime.Today,
            ["contenu"] = JsonSerializer.Serialize(plan)
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = content
        };

        var response = await _http.SendAsync(request);
        Console.WriteLine($"[UPDATE MANUEL] {url} → {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    #endregion



}
