using FitnessTracker.V1.Models;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

public class SupabaseService
{
    private readonly HttpClient _http;
    private readonly string _tableUrl;
    private readonly string _apiKey;
    private readonly string _programmeUrl; 
    public SupabaseService(HttpClient http)
    {

        _http = http;

        _apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp2c2hhcGRsd3p6eXRwbXZnbWliIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMDg5NjEsImV4cCI6MjA2MzY4NDk2MX0.DKx7CvWsfo9b5V6-vShqHXU1eNrvYXYDP26uOtEghCc"; // À récupérer dans Supabase > Project Settings > API > anon key
        _tableUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/entries"; // Voir dans Supabase > API > REST
        _programmeUrl = "https://zvshapdlwzzytpmvgmib.supabase.co/rest/v1/programmes";
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

    public async Task AddEntryAsync(PoidsEntry entry)
    {

        await RemoveByExerciceAndDateAsync(entry.Exercice, entry.Date);
        var response = await _http.PostAsJsonAsync(_tableUrl, new[] { entry });
        response.EnsureSuccessStatusCode();

    }

    public async Task RemoveByExerciceAndDateAsync(string exercice, DateTime date)
    {

        var url = $"{_tableUrl}?exercice=eq.{Uri.EscapeDataString(exercice)}&date=eq.{date:yyyy-MM-dd}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        await _http.SendAsync(request);

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
    public async Task DeleteByExerciceAndDateAsync(string exercice, DateTime date)
    {
        var url = $"{_tableUrl}?exercice=eq.{Uri.EscapeDataString(exercice)}&date=eq.{date:yyyy-MM-dd}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
    
     // table à créer dans Supabase
    public async Task<List<ProgrammeModel>> GetProgrammesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<ProgrammeModel>>(_programmeUrl);
        return result ?? new();
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
   
}
