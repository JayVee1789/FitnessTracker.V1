namespace FitnessTracker.V1.Models
{
    public class PoidsEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Exercice { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double Poids { get; set; }
    }
}
