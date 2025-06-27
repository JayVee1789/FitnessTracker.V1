namespace FitnessTracker.V1.Options
{
    public class SupabaseOptions
    {
        public string Url { get; set; } = default!;
        public string AnonKey { get; set; } = default!;

        public TablesSection Tables { get; set; } = new();

        public class TablesSection
        {
            public string Entries { get; set; } = "entries";
            public string Programmes { get; set; } = "programmes";
            public string ProgrammesManuels { get; set; } = "programmes_manuels";
        }
    }
}
