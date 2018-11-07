using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class PredefinedCrossSection:CrossSection
    {
        public double SteelThickness { get; set; }
        public double MainD { get; set; }
        public double StirrupD { get; set; }
        public SteelMaterial SteelMaterial { get; set; }
        public SteelMaterial ReinfMaterial { get; set; }
        public int Rotation { get; set; }

        public double ConcreteCover { get; set; }


        public PredefinedCrossSection(string name, Beam beam): base(name,beam)
        {
        }

    }
}
