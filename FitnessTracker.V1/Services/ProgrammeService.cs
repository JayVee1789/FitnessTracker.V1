using Blazored.LocalStorage;
using FitnessTracker.V1.Models;
using System.Text.Json;
using static FitnessTracker.V1.Models.Model;
using static System.Net.WebRequestMethods;
using System.Net.Http.Json;

namespace FitnessTracker.V1.Services;

public class ProgrammeService
{
    private readonly SupabaseService _supabase;         // ↩︎ déjà fourni
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public ProgrammeService(SupabaseService supabase,
                            ILocalStorageService localStorage, HttpClient http) 
    {
        _supabase = supabase;
        _localStorage = localStorage;
        _http = http;
    }

    /* 1️⃣  Programme actif : le plus récent (date_debut la + récente) */
    public async Task<ProgrammeModel?> GetCurrentAsync()
    {
        var list = await _supabase.GetAllProgrammesAsync();             // ➜ fusion auto/manuel 
        return list.OrderByDescending(p => p.DateDebut).FirstOrDefault();
    }

    /* 2️⃣  Exercices de référence (JSON mis en cache local) */
    public async Task<List<Model.ExerciseDefinition>> GetExerciseDefinitionsAsync()
    {
        const string cacheKey = "exercise_defs";
       
        // 1) essaie le cache
        var defs = await _localStorage.GetItemAsync<List<Model.ExerciseDefinition>>(cacheKey);
        if (defs is not null && defs.Any()) return defs;

        // 2) sinon, télécharge depuis Supabase (ou API externe)
        //var json = await _supabase.DownloadExerciseJsonAsync();        // crée cette méthode si besoin
        //defs = JsonSerializer.Deserialize<List<Model.ExerciseDefinition>>(json) ?? new();
        try
        {
            // si tu as placé ExercicesListeLocal.json dans wwwroot/data/
            defs = await _http.GetFromJsonAsync<List<ExerciseDefinition>>("data/ExercicesListeLocal.json")
                   ?? new List<ExerciseDefinition>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erreur chargement exercise_defs.json : {ex.Message}");
            defs = new List<ExerciseDefinition>();
        }

        await _localStorage.SetItemAsync(cacheKey, defs);
        return defs;
    }
}

