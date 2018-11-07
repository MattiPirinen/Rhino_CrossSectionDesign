using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class RectangleCrossSection:PredefinedCrossSection
    {

        
        public int NoReinfH { get; set; }
        public int NoReinfW { get; set; }
        public double ConcreteWidth { get; set; }
        public double ConcreteHeight { get; set; }
        public RectangleCrossSection(string name, Beam beam) : base(name, beam)
        {
        }
    }
}
