namespace FitnessTracker.V1.Services
{
    public static class ObjectifService
    {

        public static double GetPourcentage1RM(int repetitions)
        {
            if (repetitions < 1 || repetitions > 20)
                throw new ArgumentOutOfRangeException(nameof(repetitions), "Le nombre de répétitions doit être entre 1 et 20.");

            // Table classique de pourcentage par rapport au 1RM
            Dictionary<int, double> tablePourcentages = new Dictionary<int, double>()
        {
            { 1, 100 },
            { 2, 97 },
            { 3, 94 },
            { 4, 92 },
            { 5, 89 },
            { 6, 86 },
            { 7, 83 },
            { 8, 81 },
            { 9, 78 },
            { 10, 75 },
            { 11, 73 },
            { 12, 71 },
            { 13, 70 },
            { 14, 68 },
            { 15, 67 },
            { 16, 65 },
            { 17, 64 },
            { 18, 63 },
            { 19, 61 },
            { 20, 60 }
        };

            return tablePourcentages[repetitions];
        }
        public static double EpleyPercentageWithRIR(int reps, int rir)
        {
            rir = 2;
            if (reps < 1 || rir < 0) throw new ArgumentOutOfRangeException();
            return Math.Round(100 / (1 + 0.0333 * (reps + rir - 1)));
        }

        public static double DefinirObjectif(double poidsEnregistrer)
        {
            return poidsEnregistrer * 1.025;
        }

    }
}
