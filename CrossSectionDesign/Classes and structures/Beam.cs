using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class Beam:Countable
    {
        public double Gammas { get; set; }
        public double Gammac { get; set; }
        public double Gammar { get; set; }
        public double Acc { get; set; }
        public bool HasResults { get; set; }

        public void ClearResults()
        {
            HasResults = false;
        }

        public Beam(string name, double gammas, double gammac, double gammar, double acc)
        {
            Name = name;
            Gammas = gammas;
            Gammac = gammac;
            Gammar = gammar;    
            Acc = acc;
            ClimateCond = new ClimateCondition(40, 28, 36500, this);
    }

        public List<LoadCase> LoadCases = new List<LoadCase>();
        public LoadCase CurrentLoadCase { get; set; }
        public string Name { get; set; }
        public CrossSection CrossSec { get; set; }
        public ClimateCondition ClimateCond { get; set; }
    }
}
