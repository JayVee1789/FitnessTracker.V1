using Blazored.LocalStorage;
using FitnessTracker.V1.Models;
using System.Text.Json;

namespace FitnessTracker.V1.Services;

public class ProgrammeService
{
    private readonly SupabaseService _supabase;         // ↩︎ déjà fourni
    private readonly ILocalStorageService _localStorage;

    public ProgrammeService(SupabaseService supabase,
                            ILocalStorageService localStorage)
    {
        _supabase = supabase;
        _localStorage = localStorage;
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

        await _localStorage.SetItemAsync(cacheKey, defs);
        return defs;
    }
}

