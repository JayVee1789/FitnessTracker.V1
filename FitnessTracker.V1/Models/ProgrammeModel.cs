using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

public class ProgrammeModel : BaseModel
{
    [PrimaryKey("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Column("nom")]
    [JsonPropertyName("nom")]
    public string Nom { get; set; } = "";

    [Column("date_debut")]
    [JsonPropertyName("date_debut")]
    public DateTime DateDebut { get; set; }

    [Column("contenu")]
    [JsonPropertyName("contenu")]
    public string Contenu { get; set; } = "";

    [Column("user_id")]
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "";

    [Column("source")]
    [JsonPropertyName("source")]
    public string Source { get; set; } = "auto";


}
