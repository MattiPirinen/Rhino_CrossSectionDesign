using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;

namespace CrossSectionDesign.Classes_and_structures
{
    public class SimpleLoadCase : LoadCase
    {
        public double N_Ed { get; set; }
        public double M_Edz { get; set; }
        public double M_Edy { get; set; }
        public double Utilization { get; set; }
        public CrackWidthCalculation CrackWidthCalc { get; set; }


        public SimpleLoadCase(double n_Ed, double m_Edz,double m_Edy, Beam b, string name, LimitState ls)
        {
            Ls = ls;

            HostBeam = b;

            N_Ed = n_Ed;
            M_Edz = Math.Abs(m_Edz);
            M_Edy = Math.Abs(m_Edy);

            Name = name;

            //CalcUtilization();
            CrackWidthCalc =  new CrackWidthCalculation(this);

        }




        public void CalcUtilization()
        {

            Vector3d direction = new Vector3d(M_Edz, M_Edy, 0);
            double M_EdComb = direction.Length;


            Curve pl_M = HostBeam.CrossSec.CalculateStrengthCurve(new Plane(Point3d.Origin, direction,
                Vector3d.CrossProduct(direction, Vector3d.ZAxis)),Ls).ToNurbsCurve();

            Vector3d v1 = new Vector3d(Point3d.Origin - new Point3d(M_EdComb, N_Ed, 0));
            Vector3d v2 = Vector3d.CrossProduct(v1, Vector3d.ZAxis);

            Vector3d loadVector = new Vector3d(M_EdComb, N_Ed,0); 
            Plane pl = new Plane(new Point3d(M_EdComb, N_Ed, 0), v2);

            Point3d MaxPoint = FindMaxM(pl_M,pl);

            Utilization =loadVector.Length/ new Vector3d(MaxPoint).Length;

        }

        private Point3d FindMaxM(Curve c, Plane p)
        {
            CurveIntersections ci = Intersection.CurvePlane(c, p, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (ci == null) return new Point3d(0.1,0.1,0);
            List<Point3d> points = new List<Point3d>();
            foreach (IntersectionEvent ie in ci)
            {
                points.Add(ie.PointA);
            }
            return points.Max();
        }

    }
}
