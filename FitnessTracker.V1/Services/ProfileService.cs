using static FitnessTracker.V1.Models.Model;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using FitnessTracker.V1.Models.Enumeration;
using Supabase.Gotrue;

public class ProfileService
{
    private readonly Supabase.Client _supabase;

    public ProfileService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<UserProfile> GetOrCreateUserProfileAsync()
    {
        var user = _supabase.Auth.CurrentUser;
        if (user is null)
            throw new InvalidOperationException("Aucun utilisateur connecté.");

        var result = await _supabase
            .From<SportProfileModel>()
            .Where(x => x.Id == user.Id)
            .Get();

        var record = result.Models.FirstOrDefault();

        if (record == null)
        {
            record = new SportProfileModel
            {
                Id = user.Id,
                Age = 28,
                Sexe = "Homme",              // 👈 NOUVEAU
                Level = "Intermediaire",
                Objective = "Hypertrophy",
                SeancesPerWeek = 3,
                ProgramDurationMonths = 1,
                WantsSuperset = false,
                BodyweightOnly = false,
                PathologieMuscle = "",
                PriorityMuscle = ""
            };
            Console.WriteLine($"👤 UserID utilisé pour Upsert : {user.Id}");

            // ✅ Upsert au lieu de Insert
            await _supabase.From<SportProfileModel>().Upsert(record);
        }

        return new UserProfile
        {
            Age = record.Age,
            Sexe = record.Sexe,
            Level = Enum.Parse<UserLevel>(record.Level),
            Objective = Enum.Parse<TrainingObjective>(record.Objective),
            SeancesPerWeek = record.SeancesPerWeek,
            ProgramDurationMonths = record.ProgramDurationMonths,
            WantsSuperset = record.WantsSuperset,
            BodyweightOnly = record.BodyweightOnly,
            PathologieMuscle = record.PathologieMuscle,
            PriorityMuscle = record.PriorityMuscle,
            Unite = record.Unite

        };
    }

}
