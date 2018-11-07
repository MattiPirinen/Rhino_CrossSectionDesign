using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrossSectionDesign.Enumerates;
using Rhino.Geometry;

namespace CrossSectionDesign.Classes_and_structures
{
    public class ColLoadCaseDirection
    {
        
        public const double Delta0 = 1.0 / 200.0;
        public const double m = 1;
        public const double alpha_m = 1;
        public const double gamma_CE = 1.2;
        public const double K_s = 1;
        public const double E_s = 200000000000;

        public double E_cd { get; private set; }

        public double M_02 { get; private set; }
        public double M_01 { get; private set; }
        public double M_0e { get; private set; }
        public double Alpha_h { get; private set; }
        public double Delta_i { get; private set; }
        public double E_i { get; private set; }
        public double M_i { get; private set; }
        public double N_bal { get; private set; }
        public double Phi_ef { get; private set; }
        public double B { get; private set; }
        public double A { get; private set; }
        public double R_m { get; private set; }
        public double N { get; private set; }
        public double Lambda_lim { get; private set; }
        public double N_u { get; private set; }
        public double Beta { get; private set; }
        public double D { get; private set; }
        public double Curvature0 { get; private set; }
        public double K_r { get; private set; }
        public double K_psi { get; private set; }
        public double Curvature { get; private set; }
        public double E2 { get; private set; }
        public double M_2 { get; private set; }
        public double E0 { get; private set; }
        public double C { get; private set; }
        private ColLoadCase _lc;
        public double M_Ed { get; set; }
        private Axis _direction;

        
        public double K_c { get; set; }
        public double K_1 { get; set; }
        public double K_2 { get; set; }
        public double I_c { get; set; }
        public double EI { get; set; }
        public double N_B { get; set; }
        public double Beta2 { get; set; }
        public double MultFactor { get; set; }
        public CalcMethod CalcMethod { get; private set; }

        public ColLoadCaseDirection(ColLoadCase lc, Axis direction, CalcMethod calcMethod)
        {
            _lc = lc;
            _direction = direction;
            CalcMethod = calcMethod;
        }

        public void CalcLoading()
        {
            //Equivalent Moment
            M_02 = Math.Max(Math.Abs(_direction == Axis.ZAxis ? _lc.M_EzBottom: _lc.M_EyBottom),
                Math.Abs(_direction == Axis.ZAxis ? _lc.M_EzTop : _lc.M_EyTop));
            M_01 = Math.Min(Math.Abs(_direction == Axis.ZAxis ? _lc.M_EzBottom : _lc.M_EyBottom),
                Math.Abs(_direction == Axis.ZAxis ? _lc.M_EzTop : _lc.M_EyTop));

            if ((_direction == Axis.ZAxis ? _lc.M_EzBottom : _lc.M_EyBottom) * 
                (_direction == Axis.ZAxis ? _lc.M_EzTop : _lc.M_EyTop) < 0)
                M_01 = -M_01;

            //Moment due to bow of the column (initial curvature)
            M_0e = Math.Max(0.6 * M_02 + 0.4 * M_01, 0.4 * M_02);
            Alpha_h = Math.Min(1, Math.Max(2 / 3, 2 / Math.Pow(_lc.Col.Length, 0.5)));
            Delta_i = Delta0 * alpha_m * Alpha_h;
            E_i = (((_lc.ZorY == true && _direction == Axis.ZAxis) || (_lc.ZorY == false && _direction == Axis.YAxis)) ? 1 : 0) 
                * _lc.Col.Length * Delta_i / 2;
            M_i = -_lc.N_Ed * E_i;

            //effective strain coefficient
            Phi_ef = _lc.Ratio * _lc.Col.ClimateCond.CreepCoefficient;

            double epsilon_yd;

            if (_lc.Col.CrossSec.GetReinforcements().Count == 0)
                epsilon_yd = 0.01;
            else
            {
                var steelMaterial = (SteelMaterial)_lc.Col.CrossSec.GetReinforcements()[0].Material;
                epsilon_yd = steelMaterial.Fyd / steelMaterial.E;
            }


            //calc of slenderness criteria
            B = Math.Pow(1 + 2 * _lc.Col.CrossSec.Omega, 0.5);
            A = 1 / (1 + 0.2 * Phi_ef);
            R_m = (M_01 + M_i) / (M_02 + M_i);
            C = 1.7 - R_m;
            N = -_lc.N_Ed / (_lc.Col.CrossSec.A_Concrete * -_lc.Col.CrossSec.ConcreteMaterial.Fcd);
            Lambda_lim = 20 * A * B * C / Math.Pow(N, 0.5);

            //Moment due to minunmum eccentricity of the force
            E0 = Math.Max(0.02, (_direction == Axis.ZAxis ? _lc.Col.CrossSec.Heigth(Plane.WorldXY) : _lc.Col.CrossSec.Width(Plane.WorldXY)) / 30);


            if (CalcMethod == CalcMethod.NominalCurvature)
            {


                //Second order moment by nominal curvature method (EC2 5.8.8.3)
                N_u = 1 + _lc.Col.CrossSec.Omega;

                N_bal = (_direction == Axis.ZAxis ? _lc.Col.CrossSec.CalculateBalanceFailureNormalForce(Plane.WorldXY, _lc.Ls)
                    : _lc.Col.CrossSec.CalculateBalanceFailureNormalForce(new Plane(Point3d.Origin,Vector3d.YAxis,-Vector3d.XAxis),_lc.Ls)) 
                    / (_lc.Col.CrossSec.A_Concrete* _lc.Col.CrossSec.ConcreteMaterial.Fcd);
                Beta = 0.35 + -_lc.Col.CrossSec.ConcreteMaterial.Fck / (200 * Math.Pow(10, 6)) -
                    (_direction == Axis.ZAxis ? _lc.Lambda_zz : _lc.Lambda_yy) / 150;
                D = (_direction == Axis.ZAxis ? _lc.Col.CrossSec.Heigth(Plane.WorldXY) : _lc.Col.CrossSec.Width(Plane.WorldXY)) / 2 +
                    (_direction == Axis.ZAxis ? _lc.Col.CrossSec.I_Reinf.Z : _lc.Col.CrossSec.I_Reinf.Z);
                Curvature0 = epsilon_yd / (0.45 * D * Math.Pow(10, 3));
                K_r = Math.Min(1, (N_u - N) / (N_u - N_bal));
                K_psi = Math.Max(1, 1 + Beta * Phi_ef);
                Curvature = K_psi * Curvature0 * K_r;
                E2 = Curvature * Math.Pow((_direction == Axis.ZAxis ? _lc.Col.L0_zz : _lc.Col.L0_zz) * Math.Pow(10, 3), 2) 
                    / _lc.Ccurve * Math.Pow(10, -3);
                M_2 = E2 * -_lc.N_Ed;



                //TODO: NEED TO FIX THIS TO BE SIVUSIIRTYVÄ AND SIVUSIIRTYMÄTÖN RAKENNE!!!
                //Total moment
                //if ()
                //    M_Ed = Math.Max(E0 * -_lc.N_Ed, M_02 + M_2 + M_i);
                //else
                M_Ed = Math.Max(E0 * -_lc.N_Ed, Math.Max(M_02 + M_2 + M_i, M_02 + M_i));
            }
            else
            {
                //Moment multiplication factor with NominalStiffness method (EC 5.8.7.2)
                E_cd = _lc.Col.CrossSec.ConcreteMaterial.E / gamma_CE;
                K_1 = Math.Sqrt(-_lc.Col.CrossSec.ConcreteMaterial.Fck / (20 * Math.Pow(10, 6)));
                K_2 = Math.Min(N * (_direction == Axis.ZAxis? _lc.Lambda_zz: _lc.Lambda_yy) / 170, 0.2);
                K_c = K_1 * K_2 / (1 + Phi_ef);
                I_c = _direction == Axis.ZAxis ? _lc.Col.CrossSec.I_Concrete.Z:
                    _lc.Col.CrossSec.I_Concrete.Y;

                EI = K_c * E_cd * I_c + K_s * E_s * 
                    (_direction == Axis.ZAxis ? _lc.Col.CrossSec.I_Reinf.Z: _lc.Col.CrossSec.I_Reinf.Y);
                N_B = Math.Pow(Math.PI, 2) * EI / Math.Pow(_direction == Axis.ZAxis ? _lc.Col.L0_zz: _lc.Col.L0_zz, 2);
                Beta2 = Math.Pow(Math.PI, 2) / _lc.Ccurve;
                MultFactor = 1 + Beta2 / (N_B / -_lc.N_Ed - 1);


                //TODO: NEED TO FIX THIS TO BE SIVUSIIRTYVÄ AND SIVUSIIRTYMÄTÖN RAKENNE!!!
                //Total moment
                //if ()
                //    M_Ed = Math.Max(E0 * -_lc.N_Ed, M_02 + M_2 + M_i);
                //else
                M_Ed = Math.Max(E0 * -_lc.N_Ed, Math.Max((M_02 + M_i)*MultFactor, M_02 + M_i));
            }

        }

        public ColLoadCaseDirection DeepCopy()
        {
            return (ColLoadCaseDirection)MemberwiseClone();
        }

    }
}
