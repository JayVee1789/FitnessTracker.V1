namespace FitnessTracker.V1.Models
{
    public class AppState
    {
        // ✅ Propriété publique : true = lb, false = kg
        public bool AfficherEnLb { get; private set; } = false;

        // ✅ Événement pour notifier les composants
        public event Action? OnChange;

        // ✅ Méthode pour changer l’unité et notifier
        public void SetUnite(bool enLb)
        {
            if (AfficherEnLb != enLb)
            {
                AfficherEnLb = enLb;
                OnChange?.Invoke();
            }
        }

        // ✅ Toggle (facultatif, utile pour bouton)
        public void ToggleUnite()
        {
            AfficherEnLb = !AfficherEnLb;
            OnChange?.Invoke();
        }
    }


}
