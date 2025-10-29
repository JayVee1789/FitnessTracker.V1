

//using FitnessTracker.V1.Components;
//using FitnessTracker.V1.Helper;
//using FitnessTracker.V1.Mapping;
//using FitnessTracker.V1.Models;
//using FitnessTracker.V1.Models.FitnessTracker.V1.Models;
//using FitnessTracker.V1.Models.Gamification;
//using FitnessTracker.V1.Services;
//using FitnessTracker.V1.Services.Data;
//using Microsoft.AspNetCore.Components;
//using Microsoft.JSInterop;
//using System.Net.Http.Json;
//using System.Text.Json;
//using static FitnessTracker.V1.Models.Model;




//    public partial class ViewSeance : ComponentBase
//    {
//        [Inject] public SupabaseService2 SupabaseService { get; set; } = default!;
//        [Inject] public PoidsService PoidsService { get; set; } = default!;
//        [Inject] public IJSRuntime JS { get; set; } = default!;
//        [Inject] public NavigationManager Nav { get; set; } = default!;
//        [Inject] public AuthService AuthService { get; set; } = default!;
//        [Inject] public HttpClient Http { get; set; } = default!;
//        [Inject] public ViewSessionHelper viewSessionHelper { get; set; } = default!;
//        [Inject] public Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; } = default!;
//        [Inject] public GamificationDbService GamificationDbService { get; set; } = default!;
//        [Inject] public ProfileService ProfileService { get; set; } = default!;
//        [Inject] public AppState appState { get; set; } = default!;
//        [Inject] public Supabase.Client Supabase { get; set; } = default!;
//        [Inject] public ReposJournalDbService ReposJournalDbService { get; set; } = default!;

//    private List<ProgrammeModel> Programmes = new();
//    private WorkoutPlan? SelectedPlan;
//    private Guid SelectedProgrammeId;
//    private DateTime SelectedProgrammeDate = DateTime.Today;
//    private int SelectedSemaineIndex = 0;
//    private int SelectedJourIndex = 1;
//    private List<string> SeancesCompletes = new();
//    private string SemaineLabel => $"Semaine {SelectedSemaineIndex + 1}";
//    private string JourLabel => $"Jour {SelectedJourIndex}";
//    private GamificationDbModel? Gamification;
//    private Chronometre? chronoRef;
//    ReposModel? ActiviteRepos = null;
//    WorkoutDay? jour = null;
//    // protected override async Task OnInitializedAsync()
//    // {
//    //     var data = await LocalStorage.GetItemAsync<string>("seancesCompletes");
//    //     if (!string.IsNullOrEmpty(data))
//    //     {
//    //         SeancesCompletes = data.Split(',').ToList();
//    //         Console.WriteLine($"✅ Séances complètes récupérées : {SeancesCompletes.Count}");
//    //     }
//    //     var programmeId = await LocalStorage.GetItemAsync<Guid>("selectedProgrammeId");
//    //     if (programmeId != Guid.Empty)
//    //     {
//    //         var restored = await SupabaseService.LoadSessionAsync();
//    //         Console.WriteLine($"🟢 Session restaurée : {restored}");
//    //         var userId = SupabaseService.GetCurrentUserId();
//    //         Console.WriteLine($"🟣 UserID récupéré : {userId}");

//    //         Programmes = await SupabaseService.GetAllProgrammesForCurrentUserAsync();
//    //         var programme = Programmes.FirstOrDefault(p => p.Id == programmeId);
//    //         if (programme != null)
//    //         {
//    //             SelectedProgrammeId = programmeId;
//    //             SelectedProgrammeDate = programme.DateDebut;
//    //             SelectedPlan = JsonSerializer.Deserialize<WorkoutPlan>(programme.Contenu);
//    //             await ChargerSeance();
//    //         }
//    //     }
//    // }


//    // protected override async Task OnInitializedAsync()
//    // {

//    //     var restored = await SupabaseService.LoadSessionAsync();
//    //     Console.WriteLine($"🟢 Session restaurée : {restored}");
//    //     var isLoggedIn = await SupabaseService.EnsureLoggedInAsync(Nav);
//    //     if (!isLoggedIn)
//    //         return; // 🔒 Stop ici si pas connecté
//    //     Gamification = await GamificationDbService.GetGamificationAsync();

//    //     var data = await LocalStorage.GetItemAsync<string>("seancesCompletes");
//    //     SeancesCompletes = !string.IsNullOrEmpty(data)
//    //         ? data.Split(',').ToList()
//    //         : new List<string>();
//    //     Console.WriteLine($"✅ Séances complètes récupérées : {SeancesCompletes.Count}");

//    //     // 🔄 Charger programme sélectionné
//    //     var programmeId = await LocalStorage.GetItemAsync<Guid>("selectedProgrammeId");
//    //     if (programmeId == Guid.Empty)
//    //     {
//    //         Console.WriteLine("ℹ️ Aucun programme sélectionné.");
//    //         return;
//    //     }


//    //     var userGuid = SupabaseService.GetCurrentUserIdAsGuid();
//    //     if (userGuid == null)
//    //     {
//    //         Console.WriteLine("❌ Aucun utilisateur connecté → userId null");
//    //         userId = null;
//    //     }
//    //     else
//    //     {
//    //         userId = userGuid.Value.ToString();
//    //         Console.WriteLine($"✅ User ID : {userId}");
//    //     }

//    //     Programmes = await SupabaseService.GetAllProgrammesForCurrentUserAsync();
//    //     var programme = Programmes.FirstOrDefault(p => p.Id == programmeId);

//    //     if (programme is null)
//    //     {
//    //         Console.WriteLine("❌ Programme introuvable.");
//    //         return;
//    //     }

//    //     SelectedProgrammeId = programmeId;
//    //     SelectedProgrammeDate = programme.DateDebut;
//    //     SelectedPlan = JsonSerializer.Deserialize<WorkoutPlan>(programme.Contenu);
//    //     Console.WriteLine($"✅ Programme chargé : {programme.Nom}");

//    //     await ChargerSeance();
//    // }


//    private List<ExerciceJour> Exercises = new();

//    // private async Task ChargerSeance()
//    // {
//    //     Exercises.Clear();
//    //     ActiviteRepos = null;
//    //     if (SelectedPlan is null || SelectedSemaineIndex >= SelectedPlan.Weeks.Count)
//    //         return;

//    //     var week = SelectedPlan.Weeks[SelectedSemaineIndex];
//    //     jour = week.Days.FirstOrDefault(d => d.DayIndex == SelectedJourIndex);

//    //     if (jour == null || jour.IsRest)
//    //     {
//    //         ActiviteRepos = new ReposModel
//    //             {
//    //                 Activite = "rien",
//    //                 DureeMinutes = 0
//    //             };
//    //     }
//    //     else
//    //     {
//    //         var date = DateTime.Today;
//    //         var localPoids = await PoidsService.GetAllLocalPoidsAsync();

//    //         foreach (var ex in jour.Exercises)
//    //         {

//    //             var local = localPoids.FirstOrDefault(p =>
//    //                 p.Exercice == ex.ExerciseName && p.Date.Date == date.Date);

//    //             double? poids = local?.Poids;

//    //             var mapped = ExerciseMapper.MapToExerciceJour(ex, appState.AfficherEnLb, poids);
//    //             mapped.ObjectifAtteint = local?.ObjectifAtteint ?? false;
//    //             mapped.IsLb = appState.AfficherEnLb;
//    //             Exercises.Add(mapped);
//    //         }
//    //     }

//    //     StateHasChanged();
//    // }


//     private async Task OnProgrammeSelected(ChangeEventArgs e)
//     {
//         Exercises.Clear();
//         var value = e.Value?.ToString();
//         if (Guid.TryParse(value, out var id))
//         {
//             var selectedProgramme = Programmes.FirstOrDefault(p => p.Id == id);
//             if (string.IsNullOrWhiteSpace(selectedProgramme.Contenu))
//             {
//                 Console.WriteLine("❌ Contenu vide, on ne doit PAS lancer Generate ici !");
//                 return;
//             }
//             if (selectedProgramme is not null)
//             {
//                 SelectedProgrammeId = id;
//                 SelectedProgrammeSource = selectedProgramme.Source;
//                 var options = new JsonSerializerOptions
//                     {
//                         PropertyNameCaseInsensitive = true
//                     };

//                 SelectedPlan = JsonSerializer.Deserialize<WorkoutPlan>(selectedProgramme.Contenu, options);
//                 Console.WriteLine("🎯 Premier exo jour 1 : " + SelectedPlan?.Weeks[0].Days[0].Exercises[0].ExerciseName);
//                 Console.WriteLine("🔍 Type réel : " + SelectedPlan?.Weeks[0].Days[0].Exercises[0].GetType());


//                 SelectedProgrammeDate = selectedProgramme.DateDebut;

//                 // SelectedSemaineIndex = 0;
//                 // SelectedJourIndex = 1;
//                 // TrouverProchaineSeance();

//                 await ChargerSeance();
//             }
//         }
//     }

//    private void SetSemaine(int semaine)
//    {
//        SelectedSemaineIndex = semaine;
//    }

//    private void SetJour(int jour)
//    {
//        SelectedJourIndex = jour;
//    }

//    private bool SemaineComplete(int semaineIndex) => Enumerable.Range(1, 7).All(j => SeancesCompletes.Contains($"{SelectedProgrammeId}:{semaineIndex}:{j}"));
//    private bool JourComplete(int jourIndex) => SeancesCompletes.Contains($"{SelectedProgrammeId}:{SelectedSemaineIndex}:{jourIndex}");

//    private async Task OnSessionTerminee()
//    {
//        chronoRef?.StopChronoFromParent(); // stoppe au cas où
//        var userGuid = SupabaseService.GetCurrentUserIdAsGuid();

//        if (userGuid == null)
//        {
//            Console.WriteLine("❌ Utilisateur non connecté ou ID invalide.");
//            return;
//        }
//        var tempsTotal = chronoRef?.ElapsedTime ?? TimeSpan.Zero;
//        int minutes = (int)Math.Round(tempsTotal.TotalMinutes);

//        await SaveEverything();
//        if (ActiviteRepos is not null && !string.IsNullOrWhiteSpace(ActiviteRepos.Activite))
//        {
//            var reposModel = new ReposModel
//            {
//                Id = Guid.NewGuid(),
//                UserId = userGuid.Value,
//                Date = DateTime.Today,
//                Activite = ActiviteRepos.Activite,
//                DureeMinutes = ActiviteRepos.DureeMinutes
//            };
//            try
//            {
//                await ReposJournalDbService.SaveReposAsync(reposModel);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//            }

//        }
//        if (Gamification is null)
//        {
//            Console.WriteLine("⚠️ Gamification null ➡️ rechargement...");
//            Gamification = await GamificationDbService.GetOrCreateGamificationAsync();
//            if (Gamification is null) return;
//        }

//        Gamification.TotalSeances++;
//        Gamification.TotalXP += 100;
//        Gamification.StreakDays++;
//        Gamification.LastSessionDate = DateTime.UtcNow;
//        Gamification.TotalTrainingTimeMinutes += minutes;
//        Console.WriteLine($"👤 UserID utilisé pour Upsert : {userId}");

//        await GamificationDbService.UpdateGamificationAsync(Gamification);
//        Gamification = await GamificationDbService.GetOrCreateGamificationAsync();

//        Console.WriteLine($"✅ Session sauvegardée - durée : {minutes} min");
//        StateHasChanged();
//    }




//    private string toastMessage = "";
//    private string toastClass = "";
//    private async Task SaveEverything()
//    {
//        AppliquerModificationsDansSelectedPlan(); // ✅ important

//        if (HasPerformanceChanged())
//        {
//            Console.WriteLine("🔁 Des performances ont changé");
//            await SauvegarderSeance();          // ← enregistre Poids + programme
//            foreach (var ex in Exercises) ex.AcceptChanges();
//            toastMessage = "✅ Données enregistrées";
//        }
//        else
//        {
//            Console.WriteLine("📄 Aucune modif → on sauvegarde juste le plan");
//            await SauvegarderProgrammeModifié(); // ← programme seul
//            toastMessage = "ℹ️ Aucun changement à enregistrer";

//        }
//        // MarquerSeanceCommeComplete();
//        // await JS.InvokeVoidAsync("showToast", "✅ Session enregistrées");
//        toastClass = "show";
//        MarquerSeanceCommeComplete();
//        StateHasChanged();

//    }




//    private string? userId;
//    private void MarquerAtteint(ExerciceJour ex)
//    {
//        //si le poids en livre
//        //convertir en kg


//        // Conversion en kg si saisi en lb
//        double poidsSaisiKg = ex.IsLb ? ex.PoidsAffiche * 0.453592 : ex.PoidsAffiche;

//        // Calcul de l’objectif à partir du dernier poids connu (PoidsUtilisé est la référence)
//        if (string.IsNullOrEmpty(userId))
//        {
//            Console.WriteLine("❌ userId null dans MarquerAtteint, action annulée");
//            return;
//        }
//        var guidUserId = Guid.Parse(userId);

//        ex.ObjectifAtteint = poidsSaisiKg >= ex.Objectif;
//        var local = new PoidsEntryLocal
//        {
//            Id = Guid.NewGuid(),
//            Exercice = ex.ExerciseName,
//            Date = DateTime.Today,
//            Poids = poidsSaisiKg,
//            UserId = guidUserId,
//            EnLb = ex.IsLb,
//            ObjectifAtteint = ex.ObjectifAtteint
//        };
//        PoidsService.AddOrUpdateLocal(local);
//        _ = CheckFireworks(); // 🔥 déclenche sans bloquer
//    }
//    private async Task CheckFireworks()
//    {
//        if (ExercicesTotales() > 0 && ExercicesCompletes() == ExercicesTotales())
//        {
//            await JS.InvokeVoidAsync("launchFireworks");
//        }
//    }
//    private int ExercicesCompletes() =>
//   Exercises.Count(e => e.ObjectifAtteint && e.ExerciseName != "Repos");

//    private int ExercicesTotales() =>
//        Exercises.Count(e => e.ExerciseName != "Repos");

//    private void AppliquerModificationsDansSelectedPlan()
//    {
//        if (SelectedPlan is null) return;

//        var semaine = SelectedPlan.Weeks[SelectedSemaineIndex];
//        var jour = semaine.Days.FirstOrDefault(d => d.DayIndex == SelectedJourIndex);
//        if (jour is null) return;

//        // ⚠️ On écrase le contenu avec la version modifiée (sauf Repos)
//        jour.Exercises = Exercises
//            .Where(e => e.ExerciseName != "Repos")
//            .Select(e => new ExerciseSession
//            {
//                ExerciseName = e.ExerciseName,
//                Series = e.Series,
//                Repetitions = e.Repetitions,
//                RestTimeSeconds = e.RestTimeSeconds,
//                Pourcentage1RM = e.Pourcentage1RM,
//                IsSuperset = e.IsSuperset,
//            })
//            .ToList();
//    }
//    private bool HasPerformanceChanged() => Exercises.Any(e => e.IsDirty);

//    private async Task SauvegarderSeance()
//    {
//        foreach (var ex in Exercises)
//        {
//            Console.WriteLine($"🔍 {ex.ExerciseName} | IsDirty = {ex.IsDirty} | Poids = {ex.PoidsUtilisé}");

//            if (ex.ExerciseName == "Repos") continue;
//            double poidsKg = 0;
//            // ⚠️ Utiliser IsLb (pas AfficherEnLb)
//            if (ex.IsLb)
//            {
//                poidsKg = ex.PoidsAffiche * 0.453592;
//            }
//            else
//            {
//                poidsKg = ex.PoidsAffiche;
//            }
//            // var poidsKg = ex.IsLb ? ex.PoidsUtilisé * 0.453592 : ex.PoidsUtilisé;

//            var entry = new PoidsEntry
//            {
//                Exercice = ex.ExerciseName,
//                Date = DateTime.Today,
//                Poids = poidsKg,
//                UserId = Guid.Parse(userId),

//                EnLb = ex.IsLb
//            };

//            var local = new PoidsEntryLocal
//            {
//                Exercice = ex.ExerciseName,
//                Date = DateTime.Today,
//                Poids = poidsKg,
//                UserId = Guid.Parse(userId),
//                EnLb = ex.IsLb
//            };

//            try
//            {
//                // 🔁 méthode unifiée pour gérer doublons + synchro
//                await PoidsService.SaveEntryUnifiedAsync(entry, local);
//                Console.WriteLine($"✅ Entrée sauvegardée : {entry.Exercice} - {entry.Poids} kg");

//            }
//            catch (Exception exerer)
//            {
//                Console.WriteLine("❌ Erreur sauvegarde : " + exerer.Message);
//            }
//        }
//    }
//    private async Task SauvegarderProgrammeModifié()
//    {
//        if (SelectedPlan is null || SelectedProgrammeId == Guid.Empty)
//            return;

//        try
//        {


//            var programme = Programmes.FirstOrDefault(p => p.Id == SelectedProgrammeId);

//            Console.WriteLine("🧠 Sauvegarde BD de : " + programme.Id);
//            Console.WriteLine("📦 Contenu JSON : " + programme.Contenu?.Substring(0, 200));

//            if (programme is not null)
//            {
//                programme.Contenu = JsonSerializer.Serialize(SelectedPlan);
//                Console.WriteLine("✅ Programme mis à jour (contenu):");
//                Console.WriteLine(programme.Contenu.Substring(0, 300));
//                var success = await SupabaseService.UpdateProgrammeUnifiedAsync(programme);

//                if (success)
//                {
//                    Console.WriteLine("✅ Programme mis à jour avec succès.");
//                }
//                else
//                {
//                    Console.WriteLine("❌ Erreur lors de la mise à jour du programme.");
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Exception durant la mise à jour du programme : {ex.Message}");
//        }
//    }

//    private async Task MarquerSeanceCommeComplete()
//    {
//        if (SelectedProgrammeId == Guid.Empty)
//            return;

//        string key = $"{SelectedProgrammeId}:{SelectedSemaineIndex}:{SelectedJourIndex}";

//        if (!SeancesCompletes.Contains(key))
//        {
//            SeancesCompletes.Add(key);
//            await LocalStorage.SetItemAsync("seancesCompletes", string.Join(",", SeancesCompletes));
//            Console.WriteLine($"✅ Séance marquée comme complétée : {key}");
//        }
//    }

//    private ExerciceJour? ExerciceCible = null;
//    List<ExerciseDefinition> allExercises = new();
//    private List<string> Suggestions = new();
//    private async Task ProposerRemplacement(ExerciceJour ex)
//    {
//        ExerciceCible = ex;
//        var version = DateTime.Now.Ticks;
//        allExercises = await Http.GetFromJsonAsync<List<ExerciseDefinition>>($"data/ExercicesListeLocal.json?v={version}") ?? new();
//        var original = allExercises.FirstOrDefault(e => e.Name == ex.ExerciseName);
//        if (original == null)
//        {
//            Console.WriteLine("⚠️ Exercice source introuvable dans la base.");
//            Suggestions = new(); return;
//        }

//        string baseCategory = original.Category?.Split('/').FirstOrDefault()?.Trim() ?? "";

//        Suggestions = allExercises
//            .Where(e =>
//                e.Name != original.Name &&
//                e.Origin == original.Origin &&
//                e.Description == original.Description &&
//                e.Category.StartsWith(baseCategory, StringComparison.OrdinalIgnoreCase))
//            .OrderBy(_ => Guid.NewGuid())
//            .Select(e => e.Name)
//            .Take(2)
//            .ToList();

//        Console.WriteLine($"🔁 Suggestions trouvées pour {original.Name} : {string.Join(", ", Suggestions)}");
//    }

//    private void SetPoidsEtSale(ChangeEventArgs e, ExerciceJour ex)
//    {
//        if (double.TryParse(e.Value?.ToString(), out double val))
//        {
//            ex.PoidsAffiche = val; // ça déclenche le setter ⇒ isDirty
//        }
//    }

//    private async Task OnSemaineChanged(ChangeEventArgs e)
//    {
//        SelectedSemaineIndex = int.Parse(e.Value?.ToString() ?? "0");
//        await ChargerSeance();
//    }

//    private async Task OnJourChanged(ChangeEventArgs e)
//    {
//        SelectedJourIndex = int.Parse(e.Value?.ToString() ?? "1");
//        await ChargerSeance();
//    }



//    private bool ExercisesJourComplete(int jourIndex)
//    {
//        string key = $"{SelectedProgrammeId}:{SelectedSemaineIndex}:{jourIndex}";
//        return SeancesCompletes.Contains(key);
//    }

//    private bool ExercisesSemaineComplete(int semaineIndex, int _ = 0)
//    {
//        return Enumerable.Range(1, 7).All(j =>
//            SeancesCompletes.Contains($"{SelectedProgrammeId}:{semaineIndex}:{j}"));
//    }
//    private string SelectedProgrammeSource = "auto";
//    private async Task ConfirmerSuppression()
//    {
//        var modal = await JS.InvokeAsync<IJSObjectReference>("bootstrap.Modal.getInstance", "#confirmModal");
//        await modal.InvokeVoidAsync("hide");

//        if (SelectedProgrammeId == Guid.Empty)
//            return;

//        bool supabaseOk = false;
//        bool localOk = false;

//        // ✅ Supprimer côté Supabase si ce n'est pas un programme purement local
//        if (SelectedProgrammeSource is "auto" or "manuel")
//        {
//            supabaseOk = await SupabaseService.DeleteProgrammeUnifiedAsync(SelectedProgrammeId, SelectedProgrammeSource);
//            Console.WriteLine($"Supabase suppression status : {supabaseOk}");
//        }

//        // ✅ Supprimer côté localStorage dans tous les cas
//        var key = $"programme_{SelectedProgrammeId}";
//        var keys = await LocalStorage.KeysAsync();
//        if (keys.Contains(key))
//        {
//            await LocalStorage.RemoveItemAsync(key);
//            Console.WriteLine($"Programme supprimé du localStorage : {key}");
//            localOk = true;
//        }

//        if (supabaseOk || localOk)
//        {
//            Console.WriteLine("✅ Programme supprimé avec succès");

//            // Reset affichage
//            SelectedPlan = null;
//            SelectedProgrammeId = Guid.Empty;
//            SelectedProgrammeSource = "auto";
//            Exercises.Clear();

//            // 🔄 Rechargement propre
//            var supabaseProgrammes = await SupabaseService.GetAllProgrammesForCurrentUserAsync();
//            var locaux = await ChargerProgrammesLocauxAutoAsync();

//            Programmes = supabaseProgrammes.Concat(locaux).DistinctBy(p => p.Id).ToList();

//            StateHasChanged();
//        }
//        else
//        {
//            Console.WriteLine("❌ Échec suppression : aucune source supprimée");
//        }
//    }
//    private async Task<List<ProgrammeModel>> ChargerProgrammesLocauxAutoAsync()
//    {
//        var localProgrammes = new List<ProgrammeModel>();

//        var keys = await LocalStorage.KeysAsync();
//        var autoKeys = keys.Where(k => k.StartsWith("programme_"));

//        foreach (var key in autoKeys)
//        {
//            try
//            {
//                var local = await LocalStorage.GetItemAsync<ProgrammeModelLocal>(key);
//                if (local != null)
//                {
//                    localProgrammes.Add(new ProgrammeModel
//                    {
//                        Id = local.Id,
//                        Nom = local.Nom,
//                        DateDebut = local.DateDebut,
//                        Contenu = local.Contenu,
//                        Source = local.Source,
//                        UserId = SupabaseService.GetCurrentUserId() ?? ""
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Lecture locale échouée pour {key} : {ex.Message}");
//            }
//        }

//        return localProgrammes;
//    }
//    private async Task ConfirmerRemplacement(string nouveau)
//    {
//        if (ExerciceCible is null || SelectedPlan is null) return;

//        // Met à jour l'exercice affiché
//        ExerciceCible.ExerciseName = nouveau;

//        // Met à jour l'exercice dans le plan enregistré
//        var semaine = SelectedPlan.Weeks.ElementAtOrDefault(SelectedSemaineIndex);
//        var jour = semaine?.Days.FirstOrDefault(d => d.DayIndex == SelectedJourIndex);

//        if (jour is not null)
//        {
//            // Supprimer l'exercice existant
//            jour.Exercises.RemoveAll(e => e.ExerciseName == ExerciceCible.ExerciseName);

//            // Ajouter la nouvelle version (à mapper)
//            jour.Exercises.Add(ExerciseMapper.MapToSession(ExerciceCible));
//        }

//        await SauvegarderProgrammeModifié();
//        ExerciceCible = null;
//        StateHasChanged();
//    }

//    private async Task RemplacerExercice(string nouveau)
//    {
//        if (ExerciceCible is null)
//            return;

//        ExerciceCible.ExerciseName = nouveau;
//        ExerciceCible = null;

//        // await JS.InvokeVoidAsync("hideModal", "modalRemplacement");
//        // 👉 Pas de await → compatible Hot Reload
//        await JS.InvokeVoidAsync("hideModal", "modalRemplacement").AsTask();
//        StateHasChanged();
//    }
//}
