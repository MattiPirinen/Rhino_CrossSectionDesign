using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class Column: Beam
    {
        public Column(string name, double gammas, double gammac, double gammar, double acc):base(name,gammas,gammac,gammar,acc)
        {
        }
        //NOT ready get. Need to implementing the deepcopying of cross sections
        /*
        public Column DeepCopy()
        {
            Column other =(Column) MemberwiseClone();
            other.ClimateCond = ClimateCond.DeepCopy();
            other.LoadCases.Clear();
            foreach (LoadCase lc in LoadCases)
            {
                ColLoadCase clc = (ColLoadCase)lc;
                other.LoadCases.Add(clc.DeepCopy(this));
            }
            return other;
        }
        */

        /// <summary>
        /// Creates a shallow copy of the instance without the loadcases and with new name
        /// </summary>
        public Column ShallowCopy(string name)
        {
            Column other = (Column)MemberwiseClone();
            other.LoadCases = new List<LoadCase>();
            other.Name = name;
            return other;
        }

        public ColumnCalculationSettings ColumnCalcSettings { get; } = new ColumnCalculationSettings();

        private double _ky;
        private double _kz;            

        public double Length { get; set; }
        public double Ky { get { return _ky; } set { L0_yy = Length * value; _ky = value; }  }
        public double Kz { get { return _kz; } set { L0_zz = Length * value; _kz = value; } }

        public double L0_yy { get; private set; }
        public double L0_zz { get; private set; }


        
    }
}
