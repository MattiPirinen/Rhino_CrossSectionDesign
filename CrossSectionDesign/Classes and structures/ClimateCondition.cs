using CrossSectionDesign.Enumerates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class ClimateCondition
    {
        private int _rh;
        private int _t0;
        private int _t;
        public Beam HostBeam { get; private set; }
        public double H0 { get; private set;}
        public double Alpha1{ get; private set; }
        public double Alpha2 { get; private set; }
        public double Alpha3 { get; private set; }
        public double BetaFcm { get; private set; }
        public double BetaT0 { get; private set; }
        public double BetaH { get; private set; }
        public double PhiRH { get; private set; }
        public double Phi0 { get; private set; }
        public double BetaC { get; private set; }
        public double CreepCoefficient { get; private set; }




        public int RH { get => _rh; set {
                _rh = value;
                CalcCreepCoefficient();
            }
        }
        public int T0 { get => _t0; set
            {
                _t0 = value;
                CalcCreepCoefficient();
            }
        }
        public int T { get => _t; set
            {
                _t = value;
                CalcCreepCoefficient();
            }
        }
        public double Circumference() { if (HostBeam.CrossSec != null)
                return HostBeam.CrossSec.GetConcreteCircumference();
            else return 0; }

        public ClimateCondition DeepCopy()
        {
            return (ClimateCondition)MemberwiseClone();
        }

        public double Area() {
            if (HostBeam.CrossSec != null)
                return HostBeam.CrossSec.A_Concrete;
            else return 0;
        }


        private void CalcCreepCoefficient()
        {
            if (HostBeam.CrossSec != null)
            {
                ConcreteMaterial ConcreteMaterial = HostBeam.CrossSec.ConcreteMaterial;

                H0 = 2 * Area() / Circumference()* Math.Pow(10, 3); //For some reason EC wants this in mm
                Alpha1 = Math.Pow(-35 * Math.Pow(10, 6) / ConcreteMaterial.Fcm, 0.7);
                Alpha2 = Math.Pow(-35 * Math.Pow(10, 6) / ConcreteMaterial.Fcm, 0.2);
                Alpha3 = Math.Pow(-35 * Math.Pow(10, 6) / ConcreteMaterial.Fcm, 0.5);
                BetaFcm = 16.8 / Math.Pow(-ConcreteMaterial.Fcm * Math.Pow(10, -6), 0.5); //the calc units needs to be MPa
                BetaT0 = 1 / (0.1 + Math.Pow(_t0, 0.2));
                BetaH = (ConcreteMaterial.Fcm >= -35 * Math.Pow(10, 6)) ? Math.Min(1.5 * (1 + Math.Pow(0.012 * _rh, 17)) * H0 + 250, 1500) :
                    Math.Min(1.5 * (1 + Math.Pow(0.012 * _rh, 17)) * H0 + 250 * Alpha3, 1500 * Alpha3);
                PhiRH = (ConcreteMaterial.Fcm >= -35 * Math.Pow(10, 6)) ? 1 + (1 - _rh / 100.0) / (0.1 * Math.Pow(H0, 1.0 / 3.0)) :
                    (1 + (1 - _rh / 100.0) / (0.1 * Math.Pow(H0, 1.0 / 3.0)) * Alpha1) * Alpha2;
                Phi0 = PhiRH * BetaFcm * BetaT0;
                BetaC = Math.Pow((T - T0) / (BetaH + T - T0), 0.3);
                CreepCoefficient = Phi0 * BetaC;
            }

            

            
        }


        public ClimateCondition(int rh, int t0, int t, Beam hostBeam)
        {
            HostBeam = hostBeam;
            RH = rh;
            T0 = t0;
            T = t;
            CalcCreepCoefficient();
        }
    }
}
