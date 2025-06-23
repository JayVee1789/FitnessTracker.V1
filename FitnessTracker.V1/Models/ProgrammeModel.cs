using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

public class ProgrammeModel : BaseModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nom")]
    public string Nom { get; set; } = "";

    [JsonPropertyName("date_debut")]
    public DateTime DateDebut { get; set; }

    [JsonPropertyName("contenu")]
    public string Contenu { get; set; } = "";

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("source")]
    public string Source { get; set; } = "auto";
}
