using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Enumerates;
using Rhino;
using Material = CrossSectionDesign.Abstract_classes.Material;


namespace CrossSectionDesign.Classes_and_structures
{
    public class SteelMaterial : Material
    {
        public static readonly Dictionary<string,double> MaterialYield = new Dictionary<string, double>()
        {
            {"B500B",500*Math.Pow(10,6) },
            {"S235",235*Math.Pow(10,6) },
            {"S355",355*Math.Pow(10,6) },
            {"S450",450*Math.Pow(10,6) },
            {"S550",550*Math.Pow(10,6) },
            {"Custom", 355*Math.Pow(10,6) }
        };
        public SteelType SteelType { get; private set; }

        public SteelMaterial(string strengthClass, double fyk, SteelType st, Beam bm)
        {
            SteelType = st;
            Bm = bm;

            if (strengthClass == "Custom")
            {
                StrengthClass = strengthClass;
                _fyk = fyk;
            }

            else if (MaterialYield.ContainsKey(strengthClass))
            {
                StrengthClass = strengthClass;
                _fyk = MaterialYield[strengthClass];
            }
            else
            {
                RhinoApp.WriteLine("Incorrect material name was chosen. B500B is used instead.");
                StrengthClass = "B500B";
                Fyk = MaterialYield["B500B"];
            }
        }
        public SteelMaterial(string strengthClass, SteelType st, Beam bm)
        {
            SteelType = st;
            Bm = bm;
            if (MaterialYield.ContainsKey(strengthClass))
            {
                StrengthClass = strengthClass;
                _fyk = MaterialYield[strengthClass];
            }
            else
            {
                RhinoApp.WriteLine("Incorrect material name was chosen. B500B is used instead.");
                StrengthClass = "B500B";
                _fyk = MaterialYield["B500B"];
            }
        }

        private double _fyk;
        //Yield strength
        public double Fyk
        {
            get => _fyk;
            set
            {
                if (StrengthClass == "Custom")
                {
                    _fyk = value;
                }
                else
                    MessageBox.Show("Strength Class can only be set for Custom material type");
            }
        }
        public double Fyd
        {
            get => _fyk / (SteelType == SteelType.Reinforcement ? Bm.Gammar : Bm.Gammas);
        }
        public override double E { get; set; } = 200 * Math.Pow(10, 9);

        public override Tuple<double, double> FailureStrains => Tuple.Create(-0.01,0.01);

        private string _strengthClass;
        public override string StrengthClass            {
            get { return _strengthClass;}
            set
            {
                if (!MaterialYield.ContainsKey(value))
                {
                    RhinoApp.WriteLine("Not a valid StrengthClass");
                    return;
                }
                else
                {
                    _fyk = MaterialYield[value];
                    _strengthClass = value;
                    
                }
            } }

        public override double Stress(double strain, LimitState ls)
        {

            double fy = ls == LimitState.Ultimate ? Fyd : Fyk;

            if (strain < -fy / E)
            {
                return -fy - strain;
            }
            else if (strain <= fy / E)
            {
                return (strain * E);
            }
            else
            {
                return fy + strain;
            }
        }

        public override Material DeepCopy()
        {
            return (SteelMaterial)MemberwiseClone();
        }
    }
}