using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry.Intersect;
using Excel = Microsoft.Office.Interop.Excel;
using CrossSectionDesign.Static_classes;

namespace CrossSectionDesign.Classes_and_structures
{
    public class CrackWidthCalculation
    {
        public CrackWidthCalculation(SimpleLoadCase lc) { LoadCase = lc; }
        public double MaxStress { get; private set; }
        public double A_c_eff { get;private set; }
        public double A_s { get; private set; }
        public double roo_p_eff { get; private set; }
        public double CrackWidth { get; private set; }
        public double X { get; private set; }
        public double Diameter_Eq { get; private set; }
        public double H_c_eff { get; private set; }
        public double H { get; private set; }
        public double D { get; private set; }
        public const double k1 = 0.8;
        public const double k3 = 3.4;
        public const double k4 = 0.425;
        public const double kt = 0.4;
        public double f_ct_eff {get;private set;}
        public double StrainDiff { get; private set; }
        public double Alpha_e { get; private set; }
        public double s_r_max { get; private set; }
        public double W { get; private set; }
        public double k2 { get; private set; }
        public Point3d CrackPoint { get; private set; }

        private SimpleLoadCase _loadCase;
        public Beam HostBeam { get; private set; }
        public SimpleLoadCase LoadCase { get => _loadCase; private set { _loadCase = value; HostBeam = value.HostBeam; } }

        public void CalculateCrackWidth()
        {
            List<Reinforcement> reinfs = HostBeam.CrossSec.GetReinforcements();
            List<GeometryLarge> geomL = HostBeam.CrossSec.GetGeometryLarges();
            List<GeometryLarge> concreteGeoms = geomL.FindAll(geo => geo.Material.GetType() == typeof(ConcreteMaterial));
            if (concreteGeoms.Count != 1) {CrackWidth = 999; return; }
            Brep concrete = concreteGeoms[0].BaseBrep;
            double maxStress = double.MinValue;
            

            foreach (Reinforcement r in reinfs)
            {
                if (r.Stresses.ContainsKey(_loadCase))
                    maxStress = Math.Max(maxStress, r.Stresses[_loadCase]);
            }
            MaxStress = maxStress;
            if (maxStress < 0) { CrackWidth = 0; return; }

            //int index = HostBeam.LoadCases.FindIndex(lc => lc.IsDisplayed);
            //if (index == -1){ CrackWidth = 999; return; }

            Point3d p = _loadCase.NeutralAxis.Origin;
            Plane pl = _loadCase.NeutralAxis;

            //Plane localAxisPlane = new Plane(pl);
            //localAxisPlane.Transform(Transform.PlaneToPlane(Plane.WorldXY, pl));

            BoundingBox bb = HostBeam.CrossSec.GetBoundingBox(pl);
            if (LoadCase.strainAtMinAndMax.Item1 < LoadCase.strainAtMinAndMax.Item2)
                X = (Math.Abs(bb.Min.Y));
            else 
                X = (Math.Abs(bb.Max.Y));
            H = Math.Abs(bb.Max.Y - bb.Min.Y);

            // Calculate effective heigth
            CalculateEffectiveHeigth(reinfs,pl,bb);

            H_c_eff = Math.Min(2.5 * (H - D), Math.Min((H - X) / 3, H / 2));
            Brep[] breps;
            Plane cutPlane = new Plane(pl);
            double area = 0;

            if (LoadCase.strainAtMinAndMax.Item1 < LoadCase.strainAtMinAndMax.Item2)
            {
                cutPlane.Rotate(Math.PI / 2, cutPlane.XAxis);

                double distance = Math.Abs((bb.Max.Y)) - H_c_eff;

                Vector3d transV = Vector3d.YAxis * distance;
                transV.Transform(Transform.PlaneToPlane(Plane.WorldXY, pl));
                cutPlane.Translate(transV);
                breps = concrete.Trim(cutPlane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }
                
            else
            {
                cutPlane.Rotate(-Math.PI / 2, cutPlane.XAxis);

                double distance = Math.Abs((bb.Min.Y)) - H_c_eff;

                Vector3d transV = -Vector3d.YAxis * distance;
                transV.Transform(Transform.PlaneToPlane(Plane.WorldXY, pl));
                cutPlane.Translate(transV);
                breps = concrete.Trim(cutPlane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            }

            foreach (Brep brep in breps)
                area += brep.GetArea();
            A_c_eff = area;

            List<Reinforcement> tensReinf = new List<Reinforcement>();
            List<int> taken = new List<int>();
            int i = 0;
            int k = 0;
            while (k < breps.Length)
            {
                i = 0;
                while (i < reinfs.Count)
                {
                    if (new Vector3d(breps[k].ClosestPoint(reinfs[i].Centroid)- reinfs[i].Centroid).Length < 0.001 &&
                        !taken.Contains(i))
                    {
                        tensReinf.Add(reinfs[i]);
                        taken.Add(i);
                    }
                    i++;
                }
                k++;
            }
            double a_s = 0;
            tensReinf.ForEach(r => a_s += r.Area);
            A_s = a_s;
            roo_p_eff = A_s / A_c_eff;
            double e_s = reinfs[0].Material.E;

            List<Point3d> reinfCents = new List<Point3d>();
            tensReinf.ForEach(r => reinfCents.Add(r.Centroid));
            PointCloud pc = new PointCloud(reinfCents);

            double distanceSum = 0;

            for (int s = 0; s < pc.Count; s++)
            {
                Point3d point = pc[0].Location;
                pc.RemoveAt(0);
                distanceSum += new Vector3d(point - pc[pc.ClosestPoint(point)].Location).Length;
                pc.Add(point);
            }
            //Average distance between reinforcements
            W = distanceSum / pc.Count;


            //TODO Needs to add a question is the loading before or after 28days from the cast.
            f_ct_eff = ((ConcreteMaterial)concreteGeoms[0].Material).Fctm;

            Alpha_e = reinfs[0].Material.E / ((ConcreteMaterial)concreteGeoms[0].Material).E;

            StrainDiff = Math.Max((MaxStress - kt * f_ct_eff / roo_p_eff * (1 + Alpha_e * roo_p_eff)) / e_s, 0.6 * MaxStress / e_s);

            if (LoadCase.strainAtMinAndMax.Item1 < 0 || LoadCase.strainAtMinAndMax.Item2 < 0)
                k2 = 0.5;
            else
            {
                double maxStrain = Math.Max(LoadCase.strainAtMinAndMax.Item1, LoadCase.strainAtMinAndMax.Item2);
                double minStrain = Math.Min(LoadCase.strainAtMinAndMax.Item1, LoadCase.strainAtMinAndMax.Item2);
                k2 = (maxStrain + minStrain) / (2 * maxStrain);
            }


            //Calculate equivalent tension reinf diameter;
            double top = 0;
            double bottom = 0;
            tensReinf.ForEach(r => top += Math.Pow(r.Diameter, 2));
            tensReinf.ForEach(r => bottom += r.Diameter);
            Diameter_Eq = top / bottom;

            if (W < 5 * (HostBeam.CrossSec.ConcreteCover*Math.Pow(10,3) + Diameter_Eq*Math.Pow(10,3) / 2))
                s_r_max = k3 * HostBeam.CrossSec.ConcreteCover * Math.Pow(10, 3) + k1 * k2 * k4 * Diameter_Eq * Math.Pow(10, 3) / roo_p_eff;
            else
                s_r_max = 1.3 * H * X;

            CrackWidth = s_r_max * StrainDiff;

            CrackPoint = CalculateMaxCrackPoint(concrete, pl);

        }

        private Point3d CalculateMaxCrackPoint(Brep concrete, Plane pl)
        {
            BoundingBox bb = concrete.GetBoundingBox(pl);

            Line botLine;
            if (LoadCase.strainAtMinAndMax.Item1 > LoadCase.strainAtMinAndMax.Item2)
                botLine = new Line(new Point3d(bb.Min), new Point3d(bb.Max.X, bb.Min.Y, 0));
            else
                botLine = new Line(new Point3d(bb.Max), new Point3d(bb.Min.X, bb.Max.Y, 0));

            botLine.Transform(Transform.PlaneToPlane( Plane.WorldXY, pl));

            Intersection.CurveBrep(botLine.ToNurbsCurve(), concrete, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                out Curve[] overlapCurves, out Point3d[] overlapPoints);

            if (overlapPoints.Length != 0)
                return overlapPoints[0];
            else if (overlapCurves.Length != 0)
                return overlapCurves[0].PointAt(overlapCurves[0].Domain.Mid);
            else
                return Point3d.Origin;

        }

        private void CalculateEffectiveHeigth(List<Reinforcement> reinfs, Plane pl, BoundingBox bb)
        {
            List<Point3d> rfC = new List<Point3d>();
            reinfs.ForEach(r => rfC.Add(new Point3d(r.Centroid)));


            List<Point3d> pointList = new List<Point3d>();
            int i = 0;
            while (i < rfC.Count)
            {
                Point3d p = rfC[i];
                p.Transform(Transform.PlaneToPlane(pl, Plane.WorldXY));
                pointList.Add(p);
                i++;
            }
            rfC = pointList;

            List<Point3d> tensPoints = new List<Point3d>();

            foreach (Point3d point in rfC)
            {

                if ((point.Y > 0 && LoadCase.strainAtMinAndMax.Item1 < LoadCase.strainAtMinAndMax.Item2) ||
                    (point.Y < 0 && LoadCase.strainAtMinAndMax.Item1 > LoadCase.strainAtMinAndMax.Item2))
                    tensPoints.Add(point);
            }
           
            Point3d averagePoint = Point3d.Origin;
            tensPoints.ForEach(tp => averagePoint += tp);
            averagePoint = averagePoint / tensPoints.Count;
            if (LoadCase.strainAtMinAndMax.Item1 < LoadCase.strainAtMinAndMax.Item2)
                D = averagePoint.Y - bb.Min.Y;
            if (LoadCase.strainAtMinAndMax.Item1 > LoadCase.strainAtMinAndMax.Item2)
                D = bb.Max.Y - averagePoint.Y;
        }

        public void ExportToExcel(Excel.Worksheet ws, Excel.Range r)
        {
            if (r.Cells.Count == 0) return;
            Excel.Range rs = r.Cells[1, 1];
            int sRow = rs.Row;
            int sColumn = rs.Column;

            



            rs.Value = "Crack width calculation";
            ExcelGlobalSettings.Title1Font(rs);

            int dColumn = 0;
            int syColumn = 1;
            int vColumn = 2;
            int uColumn = 3;
            int rColumn = 4;


            int lcRow = sRow + 1;

            ws.Columns[sColumn + dColumn].ColumnWidth = 42;
            ws.Columns[sColumn + syColumn].ColumnWidth = 8;
            ws.Columns[sColumn + vColumn].ColumnWidth = 8;
            ws.Columns[sColumn + uColumn].ColumnWidth = 8;
            ws.Columns[sColumn + rColumn].ColumnWidth = 12;


            ws.Cells[lcRow, sColumn + dColumn].Value = "Load Case:";
            ws.Cells[lcRow, sColumn + vColumn].Value = LoadCase.Name;

            int titleRow = lcRow + 2;

            Excel.Range firstCell = ws.Cells[titleRow, sColumn + dColumn];
            Excel.Range lastCell = ws.Cells[titleRow+20, sColumn + uColumn];
            Excel.Range normalRange = ws.Range[firstCell, lastCell];
            ExcelGlobalSettings.NormalFont(normalRange);



            ws.Cells[titleRow, sColumn + dColumn].Value = "Description";
            ws.Cells[titleRow, sColumn + syColumn].Value = "Symbol";
            ws.Cells[titleRow, sColumn + vColumn].Value = "Value";
            ws.Cells[titleRow, sColumn + uColumn].Value = "Unit";
            ws.Cells[titleRow, sColumn + rColumn].Value = "Reference";
            ws.Range[ws.Cells[titleRow, sColumn + dColumn],
                ws.Cells[titleRow, sColumn + rColumn]].Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle 
                = Excel.XlLineStyle.xlContinuous;

            int heigthRow = titleRow + 1;
            ws.Cells[heigthRow, sColumn + dColumn].Value = "Cross section heigth:";
            ws.Cells[heigthRow, sColumn + syColumn].Value = "h";
            ws.Cells[heigthRow, sColumn + vColumn].Value = Math.Round(H,0);
            ws.Cells[heigthRow, sColumn + uColumn].Value = "mm";

            int effhRow = heigthRow + 1;
            ws.Cells[effhRow, sColumn + dColumn].Value = "Cross section effective heigth:";
            ws.Cells[effhRow, sColumn + syColumn].Value = "d";
            ws.Cells[effhRow, sColumn + vColumn].Value = Math.Round(D,0);
            ws.Cells[effhRow, sColumn + uColumn].Value = "mm";

            int xRow = effhRow + 1;
            ws.Cells[xRow, sColumn + dColumn].Value = "Compression zone heigth";
            ws.Cells[xRow, sColumn + syColumn].Value = "x";
            ws.Cells[xRow, sColumn + vColumn].Value = Math.Round(X,0);
            ws.Cells[xRow, sColumn + uColumn].Value = "mm";

            int hcRow = xRow + 1;
            ws.Cells[hcRow, sColumn + dColumn].Value = "Effective Concrete tension zone heigth";
            ws.Cells[hcRow, sColumn + syColumn].Value = "hc,eff";
            ws.Cells[hcRow, sColumn + syColumn].Characters[2, 5].Font.Subscript = true;
            ws.Cells[hcRow, sColumn + vColumn].Value = Math.Round(H_c_eff,0);
            ws.Cells[hcRow, sColumn + uColumn].Value = "mm";
            ws.Cells[hcRow, sColumn + rColumn].Value = "[1] (7.3.2(3))";

            int acRow = hcRow + 1;
            ws.Cells[acRow, sColumn + dColumn].Value = "Effective Concrete tension zone area";
            ws.Cells[acRow, sColumn + syColumn].Value = "Ac,eff";
            ws.Cells[acRow, sColumn + syColumn].Characters[2, 5].Font.Subscript = true;
            ws.Cells[acRow, sColumn + vColumn].Value = Math.Round(A_c_eff,0);
            ws.Cells[acRow, sColumn + uColumn].Value = "mm2";
            ws.Cells[acRow, sColumn + uColumn].Characters[3, 1].Font.Superscript = true;
            ws.Cells[acRow, sColumn + rColumn].Value = "[1] 7.3.2(3)";

            int asRow = acRow + 1;
            ws.Cells[asRow, sColumn + dColumn].Value = "Effective tension reinforcement area";
            ws.Cells[asRow, sColumn + syColumn].Value = "As";
            ws.Cells[asRow, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[asRow, sColumn + vColumn].Value = Math.Round(A_s,0);
            ws.Cells[asRow, sColumn + uColumn].Value = "mm2";
            ws.Cells[asRow, sColumn + uColumn].Characters[3, 1].Font.Superscript = true;
            

            int phiEfRow = asRow + 1;
            ws.Cells[phiEfRow, sColumn + dColumn].Value = "Equivalent reinforcement diameter";
            ws.Cells[phiEfRow, sColumn + syColumn].Value = "Feq";
            ws.Cells[phiEfRow, sColumn + syColumn].Characters[1, 1].Font.Name = "GreekS";
            ws.Cells[phiEfRow, sColumn + syColumn].Characters[2, 2].Font.Subscript = true;
            ws.Cells[phiEfRow, sColumn + vColumn].Value = Math.Round(Diameter_Eq,1);
            ws.Cells[phiEfRow, sColumn + uColumn].Value = "mm";
            ws.Cells[phiEfRow, sColumn + rColumn].Value = "[1] Eq. 7.12";


            int cRow = phiEfRow + 1;
            ws.Cells[cRow, sColumn + dColumn].Value = "Main reinforcement concrete cover";
            ws.Cells[cRow, sColumn + syColumn].Value = "c";
            ws.Cells[cRow, sColumn + vColumn].Value = Math.Round(HostBeam.CrossSec.ConcreteCover*Math.Pow(10, 3),0);
            ws.Cells[cRow, sColumn + uColumn].Value = "mm";


            int rooRow = cRow + 1;
            ws.Cells[rooRow, sColumn + dColumn].Value = "Reinforcement ratio in the tension zone";
            ws.Cells[rooRow, sColumn + syColumn].Value = "rp,eff";
            ws.Cells[rooRow, sColumn + syColumn].Characters[1, 1].Font.Name =  "GreekS";
            ws.Cells[rooRow, sColumn + syColumn].Characters[2, 5].Font.Subscript = true;
            ws.Cells[rooRow, sColumn + vColumn].Value = Math.Round(roo_p_eff,3);
            ws.Cells[rooRow, sColumn + uColumn].Value = "";
            ws.Cells[rooRow, sColumn + rColumn].Value = "[1] Eq. 7.10";


            int alpharow = rooRow + 1;
            ws.Cells[alpharow, sColumn + dColumn].Value = "Steel and concrete MOE ratio";
            ws.Cells[alpharow, sColumn + syColumn].Value = "ae";
            ws.Cells[alpharow, sColumn + syColumn].Characters[1, 1].Font.Name = "GreekS";
            ws.Cells[alpharow, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[alpharow, sColumn + vColumn].Value = Math.Round(Alpha_e,2);
            ws.Cells[alpharow, sColumn + uColumn].Value = "";
            ws.Cells[alpharow, sColumn + rColumn].Value = "[1] 7.3.4(2)";

            int ssRow = alpharow+ 1;
            ws.Cells[ssRow, sColumn + dColumn].Value = "Steel stress";
            ws.Cells[ssRow, sColumn + syColumn].Value = "ss";
            ws.Cells[ssRow, sColumn + syColumn].Characters[1, 1].Font.Name = "GreekS";
            ws.Cells[ssRow, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[ssRow, sColumn + vColumn].Value = Math.Round(MaxStress*Math.Pow(10,-6),1);
            ws.Cells[ssRow, sColumn + uColumn].Value = "MPa";

            int ktRow = ssRow+ 1;
            ws.Cells[ktRow, sColumn + dColumn].Value = "Factor depending on load duration";
            ws.Cells[ktRow, sColumn + syColumn].Value = "kt";
            ws.Cells[ktRow, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[ktRow, sColumn + vColumn].Value = kt;
            ws.Cells[ktRow, sColumn + uColumn].Value = "";
            ws.Cells[ktRow, sColumn + rColumn].Value = "[1] 7.3.4(2)";

            int k1Row = ktRow + 1;
            ws.Cells[k1Row, sColumn + dColumn].Value = "Factor taking into account bond properties";
            ws.Cells[k1Row, sColumn + syColumn].Value = "k1";
            ws.Cells[k1Row, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[k1Row, sColumn + vColumn].Value = k1;
            ws.Cells[k1Row, sColumn + uColumn].Value = "";
            ws.Cells[k1Row, sColumn + rColumn].Value = "[1] 7.3.4(3)";

            int k2Row = k1Row + 1;
            ws.Cells[k2Row, sColumn + dColumn].Value = "Factor taking into the distribution of strain";
            ws.Cells[k2Row, sColumn + syColumn].Value = "k2";
            ws.Cells[k2Row, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[k2Row, sColumn + vColumn].Value = k2;
            ws.Cells[k2Row, sColumn + uColumn].Value = "";
            ws.Cells[k2Row, sColumn + rColumn].Value = "[1] 7.3.4(3)";

            int k3Row = k2Row + 1;
            ws.Cells[k3Row, sColumn + dColumn].Value = "Factor";
            ws.Cells[k3Row, sColumn + syColumn].Value = "k3";
            ws.Cells[k3Row, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[k3Row, sColumn + vColumn].Value = k3;
            ws.Cells[k3Row, sColumn + uColumn].Value = "";
            ws.Cells[k3Row, sColumn + rColumn].Value = "[1] 7.3.4(3)";

            int k4Row = k3Row + 1;
            ws.Cells[k4Row, sColumn + dColumn].Value = "Factor";
            ws.Cells[k4Row, sColumn + syColumn].Value = "k4";
            ws.Cells[k4Row, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[k4Row, sColumn + vColumn].Value = k4;
            ws.Cells[k4Row, sColumn + uColumn].Value = "";
            ws.Cells[k4Row, sColumn + rColumn].Value = "[1] 7.3.4(3)";

            int srRow = k4Row + 1;
            ws.Cells[srRow, sColumn + dColumn].Value = "Crack spacing";
            ws.Cells[srRow, sColumn + syColumn].Value = "sr,max";
            ws.Cells[srRow, sColumn + syColumn].Characters[2, 5].Font.Subscript = true;
            ws.Cells[srRow, sColumn + vColumn].Value = s_r_max;
            ws.Cells[srRow, sColumn + uColumn].Value = "mm";
            ws.Cells[srRow, sColumn + rColumn].Value = "[1] Eq. 7.11";

            int sDiffRow = srRow+ 1;
            ws.Cells[sDiffRow, sColumn + dColumn].Value = "Reinforcement and concrete strain difference";
            ws.Cells[sDiffRow, sColumn + syColumn].Value = "esm - ecm";
            ws.Cells[sDiffRow, sColumn + syColumn].Characters[1, 1].Font.Name = "GreekS";
            ws.Cells[sDiffRow, sColumn + syColumn].Characters[7, 1].Font.Name = "GreekS";
            ws.Cells[sDiffRow, sColumn + syColumn].Characters[2, 2].Font.Subscript = true;
            ws.Cells[sDiffRow, sColumn + syColumn].Characters[8, 2].Font.Subscript = true;
            ws.Cells[sDiffRow, sColumn + vColumn].Value = StrainDiff;
            ws.Cells[sDiffRow, sColumn + uColumn].Value = "";
            ws.Cells[sDiffRow, sColumn + rColumn].Value = "[1] Eq. 7.9";

            int crackRow = sDiffRow + 1;
            ws.Cells[crackRow, sColumn + dColumn].Value = "Crack width";
            ws.Cells[crackRow, sColumn + syColumn].Value = "wk";
            ws.Cells[crackRow, sColumn + syColumn].Characters[2, 1].Font.Subscript = true;
            ws.Cells[crackRow, sColumn + vColumn].Value = CrackWidth;
            ws.Cells[crackRow, sColumn + uColumn].Value = "mm";
            ws.Cells[crackRow, sColumn + rColumn].Value = "[1] Eq. 7.8";





        }

    }
}
