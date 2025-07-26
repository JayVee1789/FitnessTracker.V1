using FitnessTracker.V1.Models;
using FitnessTracker.V1.Services.Data;

namespace FitnessTracker.V1.Services
{
    public class ReposJournalDbService 
    {

        private readonly Supabase.Client _supabase;

        public ReposJournalDbService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }
        public async Task SaveReposAsync(ReposModel model)
        {
            var response = await _supabase
                .From<ReposModel>()
                .Upsert(model);

            Console.WriteLine($"💾 Activité repos sauvegardée : {response.Model?.Activite}");
        }


    }
}
