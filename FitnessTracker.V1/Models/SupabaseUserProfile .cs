using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("user_profiles")]
public class SupabaseUserProfile : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    public string Id { get; set; } = "";

    [Column("email")]
    public string Email { get; set; } = "";

    [Column("role")]
    public string Role { get; set; } = "user";
}
