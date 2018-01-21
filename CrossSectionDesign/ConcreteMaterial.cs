using System;
using System.Collections.Generic;
using System.Linq;
using CrossSectionDesign;
using Rhino;


namespace CrossSectionDesign
{
    class ConcreteMaterial : Material
    {



        public ConcreteMaterial(string strengthClass)
        {
            //Gammac = gammac;
            StrenghtClass = strengthClass;
            SetMaterialValues();
        }


        //COmpressive strength classes of cocnrete
        public static Dictionary<string, double> FckList = new Dictionary<string, double>()
        {
            {"C20/25", -20*Math.Pow(10,6)},
            {"C25/30", -25*Math.Pow(10,6)},
            {"C30/37", -30*Math.Pow(10,6)},
            {"C35/45", -35*Math.Pow(10,6)},
            {"C40/50", -40*Math.Pow(10,6)},
            {"C45/55", -45*Math.Pow(10,6)},
            {"C50/60", -50*Math.Pow(10,6)},
            {"C55/65", -55*Math.Pow(10,6)},
            {"C60/70", -60*Math.Pow(10,6)},
            {"C70/80", -70*Math.Pow(10,6)},
            {"C80/90", -80*Math.Pow(10,6)},
            {"C90/100", -90*Math.Pow(10,6)}
        };


        private void SetMaterialValues()
        {
            Fcm = Fck + 8 * Math.Pow(10, 6);
            E = 22 * Math.Pow(Fcm * Math.Pow(10, -6) / 10, 0.3);
            if (Fck <= 50 * Math.Pow(10, 6))
            {
                Epsc2 = -2 * Math.Pow(10,-3);
                Epsc3 = -1.75 * Math.Pow(10, -3);
                Epscu1 = -3.5 * Math.Pow(10, -3);
                Epscu2 = -3.5 * Math.Pow(10, -3);
                Epscu3 = -3.5 * Math.Pow(10, -3);
                N = 2;
            }
            else
            {
                Epsc2 = -(2+0.0085*Math.Pow(Fck*Math.Pow(10,-6)-50,0.53)) *Math.Pow(10, -3);
                Epsc3 = -(1.75 + 0.55 * Fck * (Math.Pow(10, -6) - 50)/40) * Math.Pow(10, -3);
                Epscu1 = -(2.8+27*Math.Pow((98-Fcm*Math.Pow(10,-6))/100,4)) * Math.Pow(10, -3);
                Epscu2 = -(2.6 + 35 * Math.Pow((90 - Fck * Math.Pow(10, -6)) / 100, 4)) * Math.Pow(10, -3);
                Epscu3 = -(2.6 + 35 * Math.Pow((90 - Fck * Math.Pow(10, -6)) / 100, 4)) * Math.Pow(10, -3); 
                N = 1.4 + 23.4 * Math.Pow(90 - Fck * Math.Pow(10, -6) / 100, 4);
            }

        }

        private string _strengthClass;
        public string StrenghtClass
        {
            get { return _strengthClass;}
            set
            {
                if (!FckList.ContainsKey(value))
                {
                    RhinoApp.WriteLine("Not a valid StrengthClass");
                    return;
                }

                Fck = FckList[value];
                _strengthClass = value;
                if (Gammac == 0) Fcd = Acc * FckList[value];
                else Fcd = Acc * FckList[value] / Gammac;

            } }
        //Compressive strength
        public double Fck { get; private set; }
        public double Fcm { get; private set; }
        public double Fctm { get; private set; }

        public double Epsc1 { get; private set; }
        public double Epsc2 { get; private set; }
        public double Epsc3 { get; private set; }
        public double Epscu1 { get; private set; }
        public double Epscu2 { get; private set; }
        public double Epscu3 { get; private set; }
        public double N { get; private set; }

        //Modification factor
        public const double Acc = 0.85;

        //Partial safety factor
        public double Gammac { get; set; } = 1.5;

        //Design compressive strength
        public double Fcd { get; private set; }

        //Calculates stress in the material from the strain given. Uses EC2 formula (XXX)
        //to calculate the stress
        public override double Stress(double strain)
        {
            if (strain > 0)
                return 0;
            else if (strain > Epsc2)
                return Fcd * Math.Pow(1 - (1 - strain / Epsc2),N);
            else if (strain > Epscu1)
                return Fcd;
            else
                return 0;
        }
    }
}