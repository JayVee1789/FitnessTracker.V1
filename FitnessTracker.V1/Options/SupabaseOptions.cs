namespace FitnessTracker.V1.Options
{
    public class SupabaseOptions
    {
        public string Url { get; init; } = string.Empty;      // https://<id>.supabase.co
        public string AnonKey { get; init; } = string.Empty;  // clé ‘anon’ (NE PAS versionner)

        public TablesConfig Tables { get; init; } = new();

        public class TablesConfig
        {
            public string Entries { get; init; } = "entries";
            public string Programmes { get; init; } = "programmes";
            public string ProgrammesManuels { get; init; } = "programmes_manuels";
        }
    }
}
