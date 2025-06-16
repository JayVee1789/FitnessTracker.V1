using System.Text.Json.Serialization;

public class ProgrammeModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nom")]
    public string Nom { get; set; } = "";

    [JsonPropertyName("contenu")]
    public string Contenu { get; set; } = "";

    [JsonPropertyName("date_debut")]
    public DateTime DateDebut { get; set; }
}
