using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.DocObjects;


namespace CrossSectionDesign
{
    class SteelMaterial : Material
    {
        private Dictionary<string,double> _materialYield = new Dictionary<string, double>()
        {
            {"B500B",500*Math.Pow(10,6) },
            {"S235",235*Math.Pow(10,6) },
            {"S355",355*Math.Pow(10,6) }
        };


        public SteelMaterial(string name)
        {
            if (_materialYield.ContainsKey(name))
            {
                Name = name;
                Fyk = _materialYield[name];
            }
            else
            {
                RhinoApp.WriteLine("Incorrect material name was chosen. B500B is used instead.");
                Name = "B500B";
                Fyk = _materialYield["B500B"];
            }
        }

        private double _fyk;
        //Yield strength
        public double Fyk
        {
            get { return _fyk;}
            set
            {
                _fyk = value;
                if (Gammas == 0) Fyd = Fyk;
                else Fyd = Fyk / Gammas;

            }
        }

        public double Gammas { get; set; } = 1.15; //Partial safety factor
        public double Fyd { get; private set; }
        public override double E { get; set; } = 200 * Math.Pow(10, 9);

        public override double Stress(double strain)
        {
            if (strain < -Fyd / E)
            {
                return -Fyd;
            }
            else if (strain <= Fyd / E)
            {
                return (strain * E);
            }
            else
            {
                return Fyd;
            }
        }

    }
}