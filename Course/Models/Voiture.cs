using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Course.Models
{
    public class Voiture
    {
        public string Nom { get; set; } = "";
        public double CapaciteAcceleration { get; set; }
        public double VitesseMax { get; set; }
        public string Description { get; set; } = "";
        public string ImagePath { get; set; } = "";

        public override string ToString()
        {
            return string.Format("{0}  -  {1:F0} km/h", Nom, VitesseMax);
        }

        public static List<Voiture> Charger(string chemin)
        {
            var voitures = new List<Voiture>();
            var ci = CultureInfo.InvariantCulture;

            foreach (var brut in File.ReadAllLines(chemin))
            {
                var ligne = brut.Trim();
                if (ligne.Length == 0 || ligne.StartsWith("#")) continue;

                var parts = ligne.Split('|');
                if (parts.Length < 3)
                    throw new FormatException("Ligne invalide (au moins 3 champs requis) : " + ligne);

                voitures.Add(new Voiture
                {
                    Nom = parts[0].Trim(),
                    CapaciteAcceleration = double.Parse(parts[1].Trim(), ci),
                    VitesseMax = double.Parse(parts[2].Trim(), ci),
                    Description = parts.Length > 3 ? parts[3].Trim() : "",
                    ImagePath = parts.Length > 4 ? parts[4].Trim() : ""
                });
            }
            return voitures;
        }
    }
}
