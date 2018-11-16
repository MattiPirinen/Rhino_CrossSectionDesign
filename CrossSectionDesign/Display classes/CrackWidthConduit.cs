using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Interfaces;
using Rhino.Display;
using Rhino.Geometry;

namespace CrossSectionDesign.Display_classes
{
    public class CrackWidthConduit: Rhino.Display.DisplayConduit
    {

        public CrackWidthConduit()
        {
        }



        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);
        }

        
        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);
            

            if (ProjectPlugIn.Instance.CurrentBeam != null)
            {
                

                Beam b = ProjectPlugIn.Instance.CurrentBeam;
                if (b.CurrentLoadCase == null) return;
                CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;
                Transform tr = Transform.Scale(cs.AddingCentroid, 1.0 / ProjectPlugIn.Instance.Unitfactor);

                if (b.CurrentLoadCase != null && b.CurrentLoadCase.GetType() == typeof(SimpleLoadCase))
                {
                    double crackwidth = ((SimpleLoadCase)b.CurrentLoadCase).CrackWidthCalc.CrackWidth;
                    Point3d location = new Point3d( ((SimpleLoadCase)b.CurrentLoadCase).CrackWidthCalc.CrackPoint);
                    location.Transform(tr);
                    BoundingBox bb = b.CrossSec.GetBoundingBox(Plane.WorldXY);
                    bb.Transform(tr);
                    double size = bb.Diagonal.Length;

                    e.Display.DrawCircle(new Circle(location, size / 60), Color.Red);
                    e.Display.Draw3dText(new Text3d(Math.Round(crackwidth*Math.Pow(10,3),3).ToString()+ " mm",
                        new Plane(location+ new Point3d(size/50,0,0),Vector3d.ZAxis), size / 50), System.Drawing.Color.Red);


                }
                    

            }
        }
    }
}
