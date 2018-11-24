using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Enumerates;
using MathNet.Numerics.Integration;
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
        public const double eps_syT = 0.02;
        public const double eps_stT = 0.15;
        public const double eps_suT = 0.2;
        public double eps_spT(double temp)
        {
            double fsp = LinearInterpolate(fspTFactor, temp)*Fyk;
            double Esp = LinearInterpolate(EsTfactor, temp) * E;
            return fsp/Esp;
        }




        //Hot rolled steel factor for max strength reduction in different temperatures
        private List<Tuple<double, double>> fsyTFactor = new List<Tuple<double, double>> {
        Tuple.Create(20.0, 1.0),
        Tuple.Create(100.0, 1.0),
        Tuple.Create(200.0, 1.0),
        Tuple.Create(300.0, 1.0),
        Tuple.Create(400.0, 1.0),
        Tuple.Create(500.0, 0.78),
        Tuple.Create(600.0, 0.47),
        Tuple.Create(700.0, 0.23),
        Tuple.Create(800.0, 0.11),
        Tuple.Create(900.0, 0.06),
        Tuple.Create(1000.0, 0.04),
        Tuple.Create(1100.0, 0.02),
        Tuple.Create(1200.0, 0.0)};

        //Hot rolled steel factor for linear strength reduction in different temperatures
        private List<Tuple<double, double>> fspTFactor = new List<Tuple<double, double>> {
        Tuple.Create(20.0, 1.0),
        Tuple.Create(100.0, 1.0),
        Tuple.Create(200.0, 0.81),
        Tuple.Create(300.0, 0.61),
        Tuple.Create(400.0, 0.42),
        Tuple.Create(500.0, 0.36),
        Tuple.Create(600.0, 0.18),
        Tuple.Create(700.0, 0.07),
        Tuple.Create(800.0, 0.05),
        Tuple.Create(900.0, 0.04),
        Tuple.Create(1000.0, 0.02),
        Tuple.Create(1100.0, 0.01),
        Tuple.Create(1200.0, 0.0)};

        //Hot rolled steel, facotr for modulus of elasticity reduction in different temperatures
        private List<Tuple<double, double>> EsTfactor = new List<Tuple<double, double>> {
        Tuple.Create(20.0, 1.0),
        Tuple.Create(100.0, 1.0),
        Tuple.Create(200.0, 0.9),
        Tuple.Create(300.0, 0.8),
        Tuple.Create(400.0, 0.7),
        Tuple.Create(500.0, 0.6),
        Tuple.Create(600.0, 0.31),
        Tuple.Create(700.0, 0.13),
        Tuple.Create(800.0, 0.09),
        Tuple.Create(900.0, 0.07),
        Tuple.Create(1000.0, 0.04),
        Tuple.Create(1100.0, 0.02),
        Tuple.Create(1200.0, 0.0)};

        private double LinearInterpolate(List<Tuple<double, double>> list, double val)
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

        public override double TempStress(double strain, double temperature)
        {
            double EsT = E * LinearInterpolate(EsTfactor, temperature);
            double eps_sp = eps_spT(temperature);



            if (Math.Abs(strain) < eps_sp)
                return EsT * strain;
            else
            {
                double fsyT = Fyk * LinearInterpolate(fsyTFactor, temperature);
                double fspT = Fyk * LinearInterpolate(fspTFactor, temperature);

                double c = Math.Pow((fsyT - fspT), 2) / ((eps_syT - eps_sp) * EsT - 2 * (fsyT - fspT));
                double a = Math.Sqrt((eps_syT - eps_sp) / (eps_syT - eps_sp + c / EsT));
                double b = Math.Sqrt(c * (eps_syT - eps_sp) / EsT + Math.Pow(c, 2));

                if (Math.Abs(strain) < eps_syT)
                    return (strain / Math.Abs(strain)) *
                        (fspT - c + b / a * Math.Sqrt(Math.Pow(a, 2) - Math.Pow(eps_syT - Math.Abs(strain), 2)));
                else if (Math.Abs(strain) < eps_stT)
                    return (strain / Math.Abs(strain)) * fsyT;
                else if (Math.Abs(strain) < eps_suT)
                    return (strain / Math.Abs(strain)) *
                        fsyT * (1 - (Math.Abs(strain) - eps_stT) / (eps_suT - eps_stT));
                else
                    return 0;
            }


        }


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

        public override double CalcTemp(double HeatQuantity)
        {
            if (MathNet.Numerics.RootFinding.Brent.TryFindRoot(t => Integral(t) -
                HeatQuantity, -20, 1400, 0.001, 100, out double temp))
                return temp;
            else
                return -100;
        }

        private double Integral(double temp)
        {
            return GaussLegendreRule.Integrate(T => SpecificHeat(T) * Density(T), 0, temp, 5);
        }


        public override double Density(double temperature)
        {
            return 7800;
        }

        public override double HeatConductivity(double temperature)
        {
            return 66;
        }

        public override double SpecificHeat(double temperature)
        {
            return 2000;
        }
    }
}