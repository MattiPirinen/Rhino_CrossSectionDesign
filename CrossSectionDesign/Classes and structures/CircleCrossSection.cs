using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class CircleCrossSection : PredefinedCrossSection
    {
        public int NoReinf { get; set; }
        public double ConcreteDiameter { get; set; }

        public CircleCrossSection(string name, Beam beam):base(name,beam)
        { }


    }
}
