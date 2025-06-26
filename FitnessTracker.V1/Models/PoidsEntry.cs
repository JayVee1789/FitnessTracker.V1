using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

[Table("entries")]
public class PoidsEntry : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("exercice")]
    [JsonPropertyName("exercice")]
    public string Exercice { get; set; } = string.Empty;

    [Column("date")]
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [Column("poids")]
    [JsonPropertyName("poids")]
    public double Poids { get; set; }

    [Column("user_id")]
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "";

    [Column("en_lb")]
    [JsonPropertyName("en_lb")]
    public bool EnLb { get; set; } = false;

}
