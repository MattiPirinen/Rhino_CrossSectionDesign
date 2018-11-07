using CrossSectionDesign.Enumerates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class ColumnCalculationSettings
    {
        public Dictionary<ColumnCalculationMethod, bool> ColumnCalMethod { get; set; } = new Dictionary<ColumnCalculationMethod, bool>() {
            {ColumnCalculationMethod.NominalCurvature1,true},
            {ColumnCalculationMethod.NominalCurvature2,false},
            {ColumnCalculationMethod.NominalStiffness1,false},
            {ColumnCalculationMethod.NominalStiffness2,false},
};

        public ColumnCalculationSettings() { }
    }
}
