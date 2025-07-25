﻿namespace FitnessTracker.V1.Models
{
    public class PoidsEntryLocal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Exercice { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double Poids { get; set; }
        public Guid UserId { get; set; } 
        public bool EnLb { get; set; } = false;
        public bool ObjectifAtteint { get; set; } = false;
    }
}
