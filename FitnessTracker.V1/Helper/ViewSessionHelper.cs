namespace FitnessTracker.V1.Helper
{
    public class ViewSessionHelper
    {
        public static bool AfficherEnLb { get; set; } = false;


        private double ConvertToDisplayUnit(double poidsKg) =>
    AfficherEnLb ? Math.Round(poidsKg / 0.453592, 1) : poidsKg;
    }
}
