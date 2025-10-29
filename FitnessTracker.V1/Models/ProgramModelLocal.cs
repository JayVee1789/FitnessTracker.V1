using System.Text.Json.Serialization;

namespace FitnessTracker.V1.Models
{
    public class ProgramModelLocal
    {
         public Guid Id { get; set; }
    public string Nom { get; set; } = "";
    public DateTime DateDebut { get; set; }
    public string Contenu { get; set; } = "";
    public string Source { get; set; } = "auto";
    }
}
