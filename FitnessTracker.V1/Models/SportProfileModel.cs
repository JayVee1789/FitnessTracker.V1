using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("profils_sportifs")]
public class SportProfileModel : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    public string Id { get; set; } = "";

    [Column("age")]
    public int Age { get; set; }

    [Column("poids")]
    public int Poids { get; set; }

    [Column("taille")]
    public string Taille { get; set; } = "";

    [Column("sexe")]
    public string Sexe { get; set; }

    [Column("level")]
    public string Level { get; set; } = "";

    [Column("objective")]
    public string Objective { get; set; } = "";

    [Column("seances_per_week")]
    public int SeancesPerWeek { get; set; }

    [Column("program_duration_months")]
    public int ProgramDurationMonths { get; set; }

    [Column("wants_superset")]
    public bool WantsSuperset { get; set; }

    [Column("bodyweight_only")]
    public bool BodyweightOnly { get; set; }

    [Column("pathologie_muscle")]
    public string PathologieMuscle { get; set; } = "";

    [Column("priority_muscle")]
    public string PriorityMuscle { get; set; } = "";
}
