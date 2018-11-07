using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using CrossSectionDesign.Enumerates;

namespace CrossSectionDesign.Classes_and_structures
{
    public class ColLoadCase:LoadCase
    {
        private bool _zorY;
        private double _ratio;
        private double _n_Ed;
        private double _m_EzTop;
        private double _m_EyTop;
        private double _m_EzBottom;
        private double _m_EyBottom;
        private double _ccurve;

        public bool ZorY { get { return _zorY; }
            private set {
                _zorY = value;
            } } // Is the moment due to dimensions error taken in Z-axis or Y-axis direction True = Z, False = Y
        public double Ratio { get { return _ratio; } set { _ratio = value; } }
        public double Lambda_yy { get; private set; }
        public double Lambda_zz { get; private set; }

        public double N_Ed { get { return _n_Ed; } set { _n_Ed = value; } }
        public double M_EzTop { get { return _m_EzTop; } set { _m_EzTop = value;} }
        public double M_EzBottom { get { return _m_EzBottom; } set { _m_EzBottom = value;  } }
        public double M_EyBottom { get { return _m_EyTop; } set { _m_EyTop = value;} }
        public double M_EyTop { get { return _m_EyBottom; } set { _m_EyBottom = value; } }
        public Dictionary<ColumnCalculationMethod, double> Utilization { get; private set; } = new Dictionary<ColumnCalculationMethod, double>()
        {
            {ColumnCalculationMethod.NominalCurvature1,999 },
            {ColumnCalculationMethod.NominalCurvature2,999 },
            {ColumnCalculationMethod.NominalStiffness1,999 },
            {ColumnCalculationMethod.NominalStiffness2,999 },
        };
        public double Ccurve { get { return _ccurve; } set { _ccurve = value; } }
        public ColLoadCaseDirection M_Edz_NomStiff { get; private set; }
        public ColLoadCaseDirection M_Edy_NomStiff { get; private set; }
        public ColLoadCaseDirection M_Edz_NomCurv { get; private set; }
        public ColLoadCaseDirection M_Edy_NomCurv { get; private set; }
        public Column Col { get; set; }
        public Polyline LoadCurve { get; private set; } = new Polyline();
        public double M_EdComb { get; private set; }

        public ColLoadCase(double n_Ed,double m_EzTop,double m_EzBottom,double m_EyTop,double m_EyBottom,
            Column col, double ratio, string name,double ccurve, LimitState ls)
        {
            Ls = ls;
            col.CrossSec.GeometryChanged += GeometryUpdated;
            _n_Ed = n_Ed;
            _m_EzTop = m_EzTop;
            _m_EzBottom = m_EzBottom;
            _m_EyTop = m_EyTop;
            _m_EyBottom = m_EyBottom;
            Col = col;
            Name = name;
            _ccurve = ccurve;
            Lambda_yy = Col.L0_yy / Col.CrossSec.i_Concrete.Y;
            Lambda_zz = Col.L0_zz / Col.CrossSec.i_Concrete.Z;
            _ratio = ratio;

            M_Edz_NomStiff = new ColLoadCaseDirection(this, Axis.ZAxis, CalcMethod.NominalStiffness);
            M_Edy_NomStiff = new ColLoadCaseDirection(this, Axis.YAxis, CalcMethod.NominalStiffness);
            M_Edz_NomCurv = new ColLoadCaseDirection(this, Axis.ZAxis, CalcMethod.NominalCurvature);
            M_Edy_NomCurv = new ColLoadCaseDirection(this, Axis.YAxis, CalcMethod.NominalCurvature);


            Col.LoadCases.Add(this);
            CalcUtilization();
        }

        public void CalcUtilization()
        {
            if (Col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature1]) CalcUtilizationNominalCurvature1();
            else ClearNominalCurvatureValues();
            //if (Col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature2]) 
            //if (Col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness1]) 
            if (Col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness2]) CalcUtilizationNominalStiffness2();
            else ClearNominalStiffness2();
        }

        private void ClearNominalCurvatureValues()
        {
            Utilization[ColumnCalculationMethod.NominalCurvature1] = 999;
        }

        private void ClearNominalStiffness2()
        {
            NMCurve = new Polyline();
            M_EdComb = 0;
            LoadCurve = new Polyline();
            Utilization[ColumnCalculationMethod.NominalStiffness2] = 999;
        }

        public void GeometryUpdated(object sender, EventArgs e)
        {
            Lambda_yy = Col.L0_yy / Col.CrossSec.i_Concrete.Y;
            Lambda_zz = Col.L0_zz / Col.CrossSec.i_Concrete.Z;
            UpdateResults();
        }

        public void UpdateResults()
        {
            if (M_Edz_NomStiff != null && M_Edy_NomStiff != null)
                CalcUtilization();
        }

        private void CalcUtilizationNominalCurvature1()
        {
            Curve pl_Mz_c = Col.CrossSec.CalculateStrengthCurve(Plane.WorldXY, Ls).ToNurbsCurve() ;


            BoundingBox bb = pl_Mz_c.GetBoundingBox(true);
            double N_Rdmin = bb.Min.X;
            double a;

            if (Col.CrossSec.GetType() == typeof(RectangleCrossSection))
            {
                if (N_Ed / N_Rdmin <= 0.1)
                    a = 1;
                else if (N_Ed / N_Rdmin > 0.1 && N_Ed / N_Rdmin <= 0.7)
                    a = 1 + 0.5 * ((N_Ed / N_Rdmin - 0.1) / 0.6);
                else
                    a = 2;
            }
            else a = 2;


            if (M_Edy_NomCurv != null && M_Edz_NomCurv != null)
            {
                Curve pl_My_c = Col.CrossSec.CalculateStrengthCurve(new Plane(Point3d.Origin,Vector3d.YAxis,-Vector3d.XAxis), Ls)
                    .ToNurbsCurve();
                double MaxMz = FindMaxM(pl_Mz_c, new Plane(new Point3d(N_Ed, 0, 0), new Vector3d(1, 0, 0)),Axis.ZAxis);
                double MaxMy = FindMaxM(pl_My_c, new Plane(new Point3d(N_Ed, 0, 0), new Vector3d(1, 0, 0)),Axis.YAxis);

                
                //Check is the bigger utilization when the eccentricity is in z, or y-direction and choose that
                ZorY = true;
                M_Edy_NomCurv.CalcLoading();
                M_Edz_NomCurv.CalcLoading();
                
                double a1 = Math.Pow(M_Edz_NomCurv.M_Ed / MaxMz, a);
                double a2 = Math.Pow(M_Edy_NomCurv.M_Ed / MaxMy, a);
                double UtilizationZ =  a1+ a2;

                ZorY = false;
                M_Edy_NomCurv.CalcLoading();
                M_Edz_NomCurv.CalcLoading();
                a1 = Math.Pow(M_Edz_NomCurv.M_Ed / MaxMz, a);
                a2 = Math.Pow(M_Edy_NomCurv.M_Ed / MaxMy, a);
                double UtilizationY = a1 + a2;
                if (UtilizationZ > UtilizationY)
                {
                    ZorY = true;
                    M_Edy_NomCurv.CalcLoading();
                    M_Edz_NomCurv.CalcLoading();
                    Utilization[ColumnCalculationMethod.NominalCurvature1] = UtilizationZ;
                }
                else
                {
                    Utilization[ColumnCalculationMethod.NominalCurvature1] = UtilizationY;
                }
            }
            else
                Utilization[ColumnCalculationMethod.NominalCurvature1] = 999;
        }

        private void CalcUtilizationNominalStiffness2()
        {

            Polyline pl_Mz = Col.CrossSec.CalculateStrengthCurve(Plane.WorldXY,Ls);

            BoundingBox bb = pl_Mz.BoundingBox;
            double N_Rdmin = bb.Min.X;

            if (M_Edy_NomStiff != null && M_Edz_NomStiff != null)
            {
                //Save original loads
                double N_temp = N_Ed;

                //Check is the bigger utilization when the eccentricity is in z, or y-direction and choose that
                ZorY = true;
                M_Edy_NomStiff.CalcLoading();
                M_Edz_NomStiff.CalcLoading();

                Vector3d direction = new Vector3d(M_Edz_NomStiff.M_Ed, M_Edy_NomStiff.M_Ed, 0);
                M_EdComb = direction.Length;
                bool success = Col.CrossSec.CalculateStresses(N_Ed, M_Edz_NomStiff.M_Ed, M_Edy_NomStiff.M_Ed, Name,Ls);
                if (!success)
                {
                    Utilization[ColumnCalculationMethod.NominalStiffness2] = 999;
                    return;
                }
                    

                //Plane calcPlane = new Plane(Point3d.Origin, direction,
                //    Vector3d.CrossProduct(direction, Vector3d.ZAxis));
                Plane calcPlane = LoadPlane;
                if (calcPlane.ZAxis.Z < 0) calcPlane.Flip();
                Plane testPlane = new Plane(calcPlane);
                testPlane.Transform(Transform.PlaneToPlane(Plane.WorldXY, Plane.WorldYZ));


                Polyline pl_temp = Col.CrossSec.CalculateStrengthCurve(calcPlane,Ls);
                pl_temp.Transform(Transform.PlaneToPlane(testPlane, Plane.WorldYZ));
                pl_temp.Transform(Transform.PlanarProjection(Plane.WorldZX));
                NMCurve = pl_temp;

                /*
                Curve pl_M = Col.CrossSec.CalculateStrengthCurve(calcPlane).ToNurbsCurve();

                pl_M.Transform(Transform.PlaneToPlane(testPlane, Plane.WorldYZ));
                pl_M.Transform(Transform.PlanarProjection(Plane.WorldZX));
                */
                Curve pl_M = pl_temp.ToNurbsCurve();
                BoundingBox bbox = pl_M.GetBoundingBox(false);
                double MinNormalForce = bbox.Min.X;

                double[] NormalForceRange = Enumerable.Range(0, 100).ToList().Select(o => (double)o).ToList().Select(o => o * MinNormalForce/100).ToArray();

                Polyline pl = new Polyline();

                CalcLoadPoint(NormalForceRange[0], ref pl);
                int i = 1;

                
                while (pl_M.Contains(pl[pl.Count - 1],Plane.WorldZX) ==PointContainment.Inside)
                {
                    CalcLoadPoint(NormalForceRange[i], ref pl);
                    i++;
                }

                //Save original values back
                _n_Ed = N_temp;
                M_Edy_NomStiff.CalcLoading();
                M_Edz_NomStiff.CalcLoading();

                Utilization[ColumnCalculationMethod.NominalStiffness2] = N_Ed / pl[pl.Count - 1].X;

                LoadCurve = pl;
            }
            else
                Utilization[ColumnCalculationMethod.NominalStiffness2] = 999;
        }

        private void CalcLoadPoint(double v, ref Polyline pl)
        {
            _n_Ed = v;
            M_Edy_NomStiff.CalcLoading();
            M_Edz_NomStiff.CalcLoading();
            double M_Ed = new Vector3d(M_Edz_NomStiff.M_Ed, M_Edy_NomStiff.M_Ed, 0).Length;
            pl.Add(new Point3d(N_Ed, 0, M_Ed));
        }

        private Point3d FindMaxMTemp(Curve c, Plane p)
        {
            CurveIntersections ci = Intersection.CurvePlane(c, p, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (ci == null) return new Point3d(0.1, 0.1, 0);
            List<Point3d> points = new List<Point3d>();
            foreach (IntersectionEvent ie in ci)
            {
                points.Add(ie.PointA);
            }
            return points.Max();
        }

        private double FindMaxM(Curve c, Plane p, Axis direction)
        {
            
            CurveIntersections ci = Intersection.CurvePlane(c,p, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (ci == null) return 0.1;
            List<Point3d> points = new List<Point3d>();
            foreach (IntersectionEvent ie in ci)
            {
                points.Add(ie.PointA);
            }
            return (direction == Axis.ZAxis?  points.Select(o => o.Z).Max() : points.Select(o => o.Y).Max());
        }

        public ColLoadCase DeepCopy(Column newCol)
        {
            ColLoadCase other = (ColLoadCase) MemberwiseClone();
            other.M_Edz_NomStiff = M_Edz_NomStiff.DeepCopy();
            other.M_Edy_NomStiff = M_Edy_NomStiff.DeepCopy();
            other.Col = newCol;
            return other;
        }


    }
}
