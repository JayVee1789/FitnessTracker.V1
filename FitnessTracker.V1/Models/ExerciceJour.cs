using FitnessTracker.V1.Pages;
using FitnessTracker.V1.Models;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Components;

namespace FitnessTracker.V1.Models
{
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

            private bool lastAfficherEnLb;
            public double PoidsAffiche
            {
                get
                {
                    return IsLb
                        ? Math.Round(PoidsUtilisé / 0.453592, 1)
                        : Math.Round(PoidsUtilisé, 1);
                }
                set
                {
                    double newKg = IsLb
                        ? Math.Round(value * 0.453592, 2)
                        : value;

                    if (!newKg.Equals(PoidsUtilisé) || lastAfficherEnLb != IsLb)
                    {
                        PoidsUtilisé = newKg;
                        IsDirty = true;
                    }

                    lastAfficherEnLb = IsLb;
                }
            }
         
            //public double GetPoidsAffiche(AppState appState)
            //{
            //    return appState.AfficherEnLb
            //        ? Math.Round(PoidsUtilisé / 0.453592, 1)
            //        : Math.Round(PoidsUtilisé, 1);
            //}

            //public void SetPoidsAffiche(double value, AppState appState)
            //{
            //    double newKg = appState.AfficherEnLb
            //        ? Math.Round(value * 0.453592, 2)
            //        : value;

            //    if (!newKg.Equals(PoidsUtilisé) || lastAfficherEnLb != appState.AfficherEnLb)
            //    {
            //        PoidsUtilisé = newKg;
            //        IsDirty = true;
            //    }

            //    lastAfficherEnLb = appState.AfficherEnLb;
            //}

            public void ToggleObjectif()
            {
                ObjectifAtteint = !ObjectifAtteint;
                IsDirty = true;
            }

            public void AcceptChanges() => IsDirty = false;
        }
    }

}
