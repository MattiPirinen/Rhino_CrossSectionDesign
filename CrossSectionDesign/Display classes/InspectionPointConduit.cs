using System;
using System.Collections.Generic;
using System.Drawing;
using CrossSectionDesign.Classes_and_structures;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;

namespace CrossSectionDesign.Display_classes
{
    public class InspectionPointConduit : Rhino.Display.DisplayConduit
    {





        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
           // base.CalculateBoundingBox(e);
           // BoundingBox box = CreateBoundingBox();
           // e.IncludeBoundingBox(box);
    }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);
            List<InspectionPoint> insps = ProjectPlugIn.Instance.CurrentBeam.CrossSec.GetInspectionPoints();
            List<Point3d> pts = new List<Point3d>();
            insps.ForEach(o => pts.Add(o.GetModelUnitPoint()));

            BoundingBox bb = ProjectPlugIn.Instance.CurrentBeam.CrossSec.GetBoundingBox(Plane.WorldXY);
            bb.Transform(ProjectPlugIn.Instance.CurrentBeam.CrossSec.InverseUnitTransform);
            double size = bb.Diagonal.Length;


            for (int i = 0; i < pts.Count; i++)
            {
                e.Display.DrawPoint(pts[i], PointStyle.X, 3, Color.Black);
                e.Display.Draw3dText(new Text3d(insps[i].Id.ToString(),
                    new Plane(pts[i] + new Point3d(size / 50, 0, 0), Vector3d.ZAxis), size / 50), Color.Black);
            }
            

        }



        /*
        private BoundingBox CreateBoundingBox()
        {

            var box = new BoundingBox(new Point3d[] {new Point3d(0,0,0)});
            if (DisplayBrepConcrete != null)
            {
                foreach (Brep brep in DisplayBrepConcrete)
                {
                    box = BoundingBox.Union(box, brep.GetBoundingBox(false));
                }
            }

            if (DisplayBrepSteel != null)
            {
                foreach (Brep brep in DisplayBrepSteel)
                {
                    box = BoundingBox.Union(box, brep.GetBoundingBox(false));
                }
            }



            return box;
        }
        */
    }
}