using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Enumerates;
using Rhino;

namespace CrossSectionDesign.Classes_and_structures
{
    public class ConcreteMaterial : Material
    {
        public ConcreteMaterial(string strengthClass, double fck, Beam bm)
        {

            Bm = bm;
            StrengthClass = strengthClass;
            Fck = fck;

            SetMaterialValues();
        }



        public ConcreteMaterial(string strengthClass, Beam bm)
        {
            Bm = bm;
            StrengthClass = strengthClass;
            SetMaterialValues();
        }

        public override double HeatConductivity { get; set; } = 0.73;
        public override double SpecificHeat { get; set; } = 880;
        public override double Density { get; set; } = 2200;
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
            {"C90/100", -90*Math.Pow(10,6)},
            {"Custom", -30*Math.Pow(10,6)}
        };


        private void SetMaterialValues()
        {
            Fcm = Fck - 8 * Math.Pow(10, 6);
            E = 22 * Math.Pow(-Fcm * Math.Pow(10, -6) / 10, 0.3)*Math.Pow(10,9);
            if (Fck <= 50 * Math.Pow(10, 6))
            {
                Fctm = 0.3 * Math.Pow(-Fck*Math.Pow(10,-6), (2.0/3.0))*Math.Pow(10,6);
                Epsc2 = -2 * Math.Pow(10,-3);
                Epsc3 = -1.75 * Math.Pow(10, -3);
                Epscu1 = -3.5 * Math.Pow(10, -3);
                Epscu2 = -3.5 * Math.Pow(10, -3);
                Epscu3 = -3.5 * Math.Pow(10, -3);
                N = 2;
            }
            else
            {
                Fctm = 2.12 * Math.Log(1 + -Fcm*Math.Pow(10,-6) / 10)*Math.Pow(10,6);
                Epsc2 = -(2+0.0085*Math.Pow(Fck*Math.Pow(10,-6)-50,0.53)) *Math.Pow(10, -3);
                Epsc3 = -(1.75 + 0.55 * Fck * (Math.Pow(10, -6) - 50)/40) * Math.Pow(10, -3);
                Epscu1 = -(2.8+27*Math.Pow((98-Fcm*Math.Pow(10,-6))/100,4)) * Math.Pow(10, -3);
                Epscu2 = -(2.6 + 35 * Math.Pow((90 - Fck * Math.Pow(10, -6)) / 100, 4)) * Math.Pow(10, -3);
                Epscu3 = -(2.6 + 35 * Math.Pow((90 - Fck * Math.Pow(10, -6)) / 100, 4)) * Math.Pow(10, -3); 
                N = 1.4 + 23.4 * Math.Pow(90 - Fck * Math.Pow(10, -6) / 100, 4);
            }

        }

        private string _strengthClass;
        public override string StrengthClass
        {
            get { return _strengthClass;}
            set
            {
                if (!FckList.ContainsKey(value))
                {
                    RhinoApp.WriteLine("Not a valid StrengthClass");
                    return;
                }
                else
                {
                    _fck = FckList[value];
                    _strengthClass = value;
                    
                }
            } }
        //Compressive strength
        private double _fck;

        public double Fck { get => _fck;
            set
            {
                if (StrengthClass == "Custom")
                        _fck = value;
                else
                    MessageBox.Show("Strength Class can only be set for Custom material type");
            }
        }
        public double Fcm { get; private set; }
        public double Fctm { get; private set; }

        public double Epsc1 { get; private set; }
        public double Epsc2 { get; private set; }
        public double Epsc3 { get; private set; }
        public double Epscu1 { get; private set; }
        public double Epscu2 { get; private set; }
        public double Epscu3 { get; private set; }
        public double N { get; private set; }


        //Design compressive strength
        public double Fcd { get => Bm.Acc * _fck / Bm.Gammac; }

        public override Tuple<double, double> FailureStrains => Tuple.Create(-0.0035,0.0);

        //Calculates stress in the material from the strain given. Uses EC2 formula (XXX)
        //to calculate the stress
        public override double Stress(double strain, LimitState ls)
        {
            double factor;
            if (ls == LimitState.Service_QP)
                factor = 1 + Bm.ClimateCond.CreepCoefficient;
            else
                factor = 1;

            double fc;
            if (ls == LimitState.Ultimate)
                fc = Fcd;
            else
                fc = Fck;



            if (strain > 0)
                return 0-strain;
            else if (strain > Epsc2*factor)
                return fc *(1- Math.Pow(1 - (strain / (Epsc2* factor)),N));
            else if (strain >= Epscu1*factor)
                return fc + strain;
            else
                return fc + strain;
        }

        public override Material DeepCopy()
        {
            return (ConcreteMaterial)MemberwiseClone();
        }
    }
}