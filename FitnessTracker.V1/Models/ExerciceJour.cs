using FitnessTracker.V1.Pages;

namespace FitnessTracker.V1.Models
{
    public class ExerciceJour
    {
        public string ExerciseName { get; set; } = "";
        public int Series { get; set; }
        public int Repetitions { get; set; }
        public int RestTimeSeconds { get; set; }
        public double Pourcentage1RM { get; set; }
        public double PoidsUtilisé { get; set; }
        public double Objectif { get; set; }
        public bool ObjectifAtteint { get; set; } = false;
        public bool IsLb { get; set; } = false;
        public bool RefreshFlag { get; set; } = false;
        public bool IsDirty { get; private set; } = false;
        public bool IsSuperset { get; set; } = false;
        public double PoidsAffiche
        {
            get => ViewSession.AfficherEnLb
            ? Math.Round(PoidsUtilisé / 0.453592, 1)
            : Math.Round(PoidsUtilisé, 1);
            set
            {
                double newKg = ViewSession.AfficherEnLb
                    ? Math.Round(value * 0.453592, 2)
                    : value;

                if (!newKg.Equals(PoidsUtilisé))          // ← comparaison
                {
                    PoidsUtilisé = newKg;
                    IsDirty = true;                       // ← on marque sale
                }
            }
        }

        /* ─── méthode pour (dé)marquer l’objectif ─── */
        public void ToggleObjectif()
        {
            ObjectifAtteint = !ObjectifAtteint;
            IsDirty = true;
        }

        /* ─── réinitialisation après sauvegarde ─── */
        public void AcceptChanges() => IsDirty = false;



    }
}
