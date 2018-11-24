using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Enumerates;
using Rhino;
using MathNet.Numerics.Integration;

namespace CrossSectionDesign.Classes_and_structures
{
    public class ConcreteMaterial : Material
    {
        private List<Tuple<double, double>> fcTfactors = new List<Tuple<double, double>> {
        Tuple.Create(20.0, 1.0),
        Tuple.Create(100.0, 1.0),
        Tuple.Create(200.0, 0.95),
        Tuple.Create(300.0, 0.85),
        Tuple.Create(400.0, 0.75),
        Tuple.Create(500.0, 0.6),
        Tuple.Create(600.0, 0.45),
        Tuple.Create(700.0, 0.3),
        Tuple.Create(800.0, 0.15),
        Tuple.Create(900.0, 0.08),
        Tuple.Create(1000.0, 0.04),
        Tuple.Create(1100.0, 0.01),
        Tuple.Create(1200.0, 0.0)};

        private List<Tuple<double, double>> epsc1Tfactors = new List<Tuple<double, double>> {
        Tuple.Create(20.0, -0.0025),
        Tuple.Create(100.0, -0.004),
        Tuple.Create(200.0, -0.0055),
        Tuple.Create(300.0, -0.007),
        Tuple.Create(400.0, -0.01),
        Tuple.Create(500.0, -0.015),
        Tuple.Create(600.0, -0.025),
        Tuple.Create(700.0, -0.025),
        Tuple.Create(800.0, -0.025),
        Tuple.Create(900.0, -0.025),
        Tuple.Create(1000.0, -0.025),
        Tuple.Create(1100.0, -0.025),
        Tuple.Create(1200.0, -0.025)};

        private List<Tuple<double, double>> epscu1Tfactors = new List<Tuple<double, double>> {
        Tuple.Create(20.0, -0.02),
        Tuple.Create(100.0, -0.0225),
        Tuple.Create(200.0, -0.025),
        Tuple.Create(300.0, -0.0275),
        Tuple.Create(400.0, -0.03),
        Tuple.Create(500.0, -0.0325),
        Tuple.Create(600.0, -0.035),
        Tuple.Create(700.0, -0.0375),
        Tuple.Create(800.0, -0.04),
        Tuple.Create(900.0, -0.0425),
        Tuple.Create(1000.0, -0.045),
        Tuple.Create(1100.0, -0.0475),
        Tuple.Create(1200.0, -0.0475)};

        public ConcreteMaterial(string strengthClass, double fck, Beam bm)
        {

            Bm = bm;
            StrengthClass = strengthClass;
            Fck = fck;

            SetMaterialValues();
        }

        private double Integral(double temp)
        {
            double totIntegral = 0;
            if (temp < 100)
                return SpecificHeat(temp) * Density(temp) * temp;
            if (temp < 115)
            {
                totIntegral += 2.07 * Math.Pow(10, 8);

                totIntegral += SpecificHeat(110) * Density(110) * (temp - 100);
                return totIntegral;
            }
            if (temp < 200)
            {
                totIntegral += 2.557715 * Math.Pow(10, 8);
                totIntegral += 46 * (temp - 115) * (94 * Math.Pow(temp, 2) - 669165 * temp + 393090175) / 4335;
                return totIntegral;
            }
            if (temp < 400)
            {
                totIntegral += 4.96896217 * Math.Pow(10, 8);
                totIntegral += 23 * (temp - 200) * (7200 * temp - Math.Pow(temp, 2) + 37800000) / 400;
                return totIntegral;
            }
            if (temp < 1200)
            {
                //totIntegral += 8.89585016 * Math.Pow(10, 8);
                totIntegral += 9.6287622 * Math.Pow(10, 8);
                totIntegral += -253 * (temp - 400) * (7 * temp - 154800) / 16;
                return totIntegral;
            }
            totIntegral += 2.814845 * Math.Pow(10, 9);
            totIntegral += (temp - 1200) * SpecificHeat(temp) * Density(temp);
            return totIntegral;
;                

        }

        public ConcreteMaterial(string strengthClass, Beam bm)
        {
            Bm = bm;
            StrengthClass = strengthClass;
            SetMaterialValues();
        }

        //Specific heat for concrete in different temperatures when the moisture content is 1,5%
        public override double SpecificHeat(double temperature)
        {
            if (temperature < 100)
                return 900.0;
            else if (temperature < 115)
                return 1470.0;
            else if (temperature < 200)
                return 1470 - 470 * (temperature - 115) / (200 - 115);
            else if (temperature < 400)
                return 1000 + 100 * (temperature - 200) / (400 - 200);
            else
                return 1100;
        }

        public override double HeatConductivity(double temperature)
        {
            if (temperature < 20)
                return 1.333;
            else
                return 1.36 - 0.136 * (temperature / 100) + 0.0057 * Math.Pow(temperature / 100, 2);
        }

        public override double Density (double temperature)
        {
            double basedens = 2300;
            if (temperature < 115)
                return basedens;
            else if (temperature < 200)
                return basedens * (1 - 0.02 * (temperature - 115) / 85);
            else if (temperature < 400)
                return basedens * (0.98 - 0.03 * (temperature - 200) / 200);
            else if (temperature < 1200)
                return basedens * (0.95 - 0.07 * (temperature - 400) / 800);
            else return basedens * 0.88;
                    
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

        public double GetfcT(double temperature)
        {
            return LinearInterpolate(fcTfactors, temperature)*Fck;
        }

        public double GetEpsc1T(double temperature)
        {
            return LinearInterpolate(epsc1Tfactors, temperature);
        }

        public double GetEpscu1T(double temperature)
        {
            return LinearInterpolate(epscu1Tfactors, temperature);
        }

        private double LinearInterpolate(List<Tuple<double,double>> list, double val)
        {
            int i = 0;
            while (i < list.Count && list[i].Item1 < val)
                i++;
            if (i == 0)
                return list[0].Item2;
            else if (i < list.Count)
                return list[i - 1].Item2 - (list[i - 1].Item2 - list[i].Item2) * (val - list[i - 1].Item1) / (list[i].Item1 - list[i - 1].Item1);
            else
                return list[list.Count].Item2;
                

        }


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

        public override double TempStress(double strain, double temperature)
        {
            double epcs1T = GetEpsc1T(temperature);
            double fcT= GetfcT(temperature);
            if (strain > 0)
                return 0-strain;
            else if (strain > epcs1T)
                return 3 * strain * fcT / (epcs1T * (2 + Math.Pow(strain / epcs1T, 3)));
            else
            {
                double epcsu1T = GetEpscu1T(temperature);
                if (strain > epcsu1T)
                    return fcT *(1- (strain - epcs1T) / (epcsu1T - epcs1T));
                else
                    return 1 / strain;
            }
        }


        public override Material DeepCopy()
        {
            return (ConcreteMaterial)MemberwiseClone();
        }


        public override double CalcTemp(double HeatQuantity)
        {
            if (HeatQuantity < 2.07 * Math.Pow(10, 8))
                return HeatQuantity / (SpecificHeat(50) * Density(50));
            else if (HeatQuantity < 2.557715 * Math.Pow(10, 8))
                return (100 + (HeatQuantity - 2.07 * Math.Pow(10, 8)) / (SpecificHeat(101) * Density(101)));
            else
            {
                if (MathNet.Numerics.RootFinding.Brent.TryFindRoot(t => Integral(t) -
                    HeatQuantity, -50, 1800, 0.001, 100, out double temp))
                    return temp;
                else
                    return -100;
            }

        }
    }
}