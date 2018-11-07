using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace CrossSectionDesign.Display_classes
{
    public class DivisionConduit : Rhino.Display.DisplayConduit
    {
        public List<Brep> DisplayBreps { get; set; }
        private Color[] _colors = new Color[] {Color.Blue, Color.Aquamarine};


        public DivisionConduit(List<Brep> displayBreps)
        {
            DisplayBreps = DisplayBreps;
        }

        public DivisionConduit()
        {
            DisplayBreps = new List<Brep>();

        }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);
            BoundingBox box = CreateBoundingBox();
            e.IncludeBoundingBox(box);
        }


        protected override void PostDrawObjects(DrawEventArgs e)
        {
            List<bool> colorList = new List<bool>();
            bool switcher = false;
            base.PostDrawObjects(e);
            if (DisplayBreps == null) return;
            for (int i = 0; i < DisplayBreps.Count; i++)
            {
                if (i > 0)
                {
                    for (int j = 1; j < Math.Min(5,i+1); j++)
                    {
                        Curve[] tempCurve;
                        Point3d[] intPoints;
                        Intersection.BrepBrep(DisplayBreps[i], DisplayBreps[i - j], 0.001, out tempCurve, out intPoints);
                        if (tempCurve.Length != 0)
                        {
                            switcher = !colorList[i-j];
                            j = 100;
                        }
                       

                    }

                }
                e.Display.Draw2dText(i.ToString(), Color.Black, AreaMassProperties.Compute(DisplayBreps[i]).Centroid+ new Point3d(0,0,1), true);

                RhinoApp.WriteLine(_colors[switcher ? 1 : 0].ToString());
                e.Display.DrawBrepShaded(DisplayBreps[i], new DisplayMaterial(_colors[switcher ? 1 : 0]));
                colorList.Add(switcher);

                
            }

        }


        private BoundingBox CreateBoundingBox()
        {
            var box = new BoundingBox(new Point3d[] {new Point3d(0,0,0)});
            if (DisplayBreps != null)
            {
                foreach (Brep brep in DisplayBreps)
                {
                    box = BoundingBox.Union(box, brep.GetBoundingBox(false));
                }
            }
            return box;
        }
    }
}