using FitnessTracker.V1.Models.FitnessTracker.V1.Models;
using FitnessTracker.V1.Services;
using static FitnessTracker.V1.Models.Model;

namespace FitnessTracker.V1.Mapping
{
    public static class ExerciseMapper
    {
        public static ExerciceJour MapToExerciceJour(ExerciseSession session, bool afficherEnLb, double? poids = null)
        {
            var poidsKg = poids ?? 0;
            return new ExerciceJour
            {
                ExerciseName = session.ExerciseName,
                Series = session.Series,
                Repetitions = session.Repetitions,
                RestTimeSeconds = session.RestTimeSeconds,
                //Pourcentage1RM = ObjectifService.GetPourcentage1RM(session.Repetitions),
                Pourcentage1RM = ObjectifService.EpleyPercentageWithRIR(session.Repetitions, 2),

                IsSuperset = session.IsSuperset,
                PoidsUtilisé = poidsKg,
                //Objectif = poidsKg * (1 + session.Pourcentage1RM / 100.0),
                //Objectif = ObjectifService.DefinirObjectif(poidsKg),
                Objectif = ObjectifService.DefinirObjectif2(poidsKg, ObjectifService.EpleyPercentageWithRIR(session.Repetitions, 2)),
                IsLb = afficherEnLb
                
            };
        }

        public static ExerciseSession MapToSession(ExerciceJour jour)
        {
            return new ExerciseSession
            {
                ExerciseName = jour.ExerciseName,
                Series = jour.Series,
                Repetitions = jour.Repetitions,
                RestTimeSeconds = jour.RestTimeSeconds,
                Pourcentage1RM = jour.Pourcentage1RM,
                IsSuperset = false, // si tu as besoin, tu peux le mapper aussi
                    ExerciseId = 0
            };
        }
    }
}
