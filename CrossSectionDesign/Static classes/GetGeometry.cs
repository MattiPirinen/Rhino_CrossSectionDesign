using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dlubal.RFEM5;
using Rhino.Geometry;
using Rhino;
using RFEM3;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rhino.DocObjects;
using CrossSectionDesign.Classes_and_structures;

namespace CrossSectionDesign.Static_classes
{
    class GetGeometry:RFEMConnection
    {

        //Gets all the members in the model and bakes them as breps to the rhino.
        public static Tuple<Member[], Dlubal.RFEM5.CrossSection[]> GetMembers(string comment)
        {
            OpenConnection();
            try
            {
                IModelData rData = RModel.GetModelData();

                Dlubal.RFEM5.Line[] lines = rData.GetLines();
                Dlubal.RFEM5.CrossSection[] crossSecs = rData.GetCrossSections();
                Member[] members = rData.GetMembers();
                members = members.Where(o => o.Comment == comment).ToArray();
                List<Member> mList = new List<Member>();

                Dictionary<int, Brep> rCrossSecs = new Dictionary<int, Brep>();
                foreach (Dlubal.RFEM5.CrossSection crossSec in crossSecs)
                {
                    rCrossSecs.Add(crossSec.No, GetCrscDBShape(crossSec.TextID));
                }

                foreach (Member member in members)
                {
                    Dlubal.RFEM5.Line line = rData.GetLine(member.LineNo, ItemAt.AtNo).GetData();
                    Rhino.Geometry.Line rhLine = lineRfemToRhino(line, rData);
                    Vector3d direction = new Vector3d(rhLine.To - rhLine.From);
                    Plane plane = new Plane(rhLine.From, direction);
                    Brep tempCross = (Brep)rCrossSecs[member.StartCrossSectionNo].Duplicate();
                    Transform tr = Transform.PlaneToPlane(Plane.WorldXY, plane);
                    tempCross.Transform(tr);

                    Brep extruded = tempCross.Faces[0].CreateExtrusion(rhLine.ToNurbsCurve(), true);
                    ProjectPlugIn.Instance.beamBreps.Add(Tuple.Create(member.No, extruded));

                }

                foreach (Member m in members)
                {
                    Dlubal.RFEM5.CrossSection c = Array.Find(crossSecs, o => o.No == m.StartCrossSectionNo);
                    if (c.TextID.Split(' ')[0] == "Rechteck" || c.TextID.Split(' ')[0] == "Kreis")
                    {
                        mList.Add(m);
                    }
                }
                CloseConnection();
                return Tuple.Create(mList.ToArray(), crossSecs);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Cleans Garbage collector for releasing all COM interfaces and objects
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                CloseConnection();
            }


            return Tuple.Create<Member[], Dlubal.RFEM5.CrossSection[]>(null, null);
            

        }

        private static Rhino.Geometry.Line lineRfemToRhino(Dlubal.RFEM5.Line line,IModelData rData)
        {
            Point3D point1 = line.ControlPoints[0];
            Point3D point2 = line.ControlPoints[line.ControlPoints.Length-1];


            Rhino.Geometry.Line rhLine = new Rhino.Geometry.Line(new Point3d(point1.X,point1.Y,point1.Z),
                new Point3d(point2.X,point2.Y,point2.Z));
            return rhLine;
        }



        private static Brep GetCrscDBShape(string name)
        {
            Structure IStr = null;
            List<Curve> rCurves = new List<Curve>();
            try
            {
                /*
                IStr = (Structure)Marshal.GetActiveObject("RFEM3.Structure");
               // IStr.rfGetApplication().rfLockLicence();
                IrfStructuralData IData = IStr.rfGetStructuralData();
                IrfDatabaseCrSc db = IData.rfGetDatabaseCrSc();
                
                IrfCrossSectionDB2 crsc = (IrfCrossSectionDB2) db.rfGetCrossSection(name);

                List<Curve> rTempCurves = new List<Curve>();
                

                CURVE_2D[] curves = (CURVE_2D[]) crsc.rfGetShape();

                foreach (CURVE_2D curve in curves)
	            {
                    IPOINT_2D[] points = curve.arrPoints as IPOINT_2D[];
                    if (curve.type == CURVE_TYPE.CT_LINE)
                    {
                        rTempCurves.Add(new Rhino.Geometry.Line(new Point3d(points[0].x,points[0].y,0),
                            new Point3d(points[1].x,points[1].y,0)).ToNurbsCurve());
                    }
                    if (curve.type == CURVE_TYPE.CT_ARC)
                        rTempCurves.Add(new Arc(new Point3d(points[0].x, points[0].y, 0),
                            new Point3d(points[1].x, points[1].y, 0),
                            new Point3d(points[2].x, points[2].y, 0)).ToNurbsCurve());
                    if (curve.type == CURVE_TYPE.CT_CIRCLE){
                        double radius = (new Vector3d(points[0].x, points[0].y, 0) -
                            new Vector3d(points[1].x, points[1].y, 0)).Length;
                        rTempCurves.Add(new Circle(new Point3d(points[0].x, points[0].y, 0), 
                            radius).ToNurbsCurve());
                    }

                }
                rCurves.AddRange(Curve.JoinCurves(rTempCurves));
                */

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //Release COM object
                if (IStr != null)
                {
                    IStr.rfGetApplication().rfUnlockLicence();
                    IStr = null;
                }
                //Cleans Garbage collector for releasing all COM interfaces and objects
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }

            rCurves.Add(new Circle(Plane.WorldXY, 155).ToNurbsCurve());
            Brep cutter;
            Brep shape;
            if (rCurves.Count == 1)
            {
                shape = Brep.CreatePlanarBreps(rCurves[0])[0];
            }
                
            else if (rCurves.Count > 1)
            {
                if (!rCurves.TrueForAll(o => o.IsClosed))
                    return null;
                List<double> areas = new List<double>();
                foreach (Curve curve in rCurves)
                {
                    AreaMassProperties am = AreaMassProperties.Compute(curve);
                    areas.Add(am.Area);
                }
                double maxArea = areas.Max();
                int index = areas.IndexOf(maxArea);
                Curve outline = rCurves[index];
                shape = Brep.CreatePlanarBreps(outline)[0];
                for (int i = 0; i < rCurves.Count; i++)
                {
                    if (i != index)
                    {
                        cutter = Brep.CreatePlanarBreps(rCurves[i])[0];

                        shape = Brep.CreateBooleanDifference(shape, cutter, 0.001)[0];
                    }
                }
            }
            else
            {
                shape = new Brep();
            }

            Transform tr = Transform.Scale(new Point3d(0, 0, 0), 0.001);
            shape.Transform(tr);
            return shape;
        }
        
    }
}
