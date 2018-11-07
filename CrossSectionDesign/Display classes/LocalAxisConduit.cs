using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using CrossSectionDesign.Classes_and_structures;
namespace CrossSectionDesign.Display_classes
{
    public class LocalAxisConduit:DisplayConduit
    {
        private ProjectPlugIn _projectPlugIn;

        public LocalAxisConduit(ProjectPlugIn projectPlugin)
        {
            _projectPlugIn = projectPlugin;
        }

        /*
        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);
            e.IncludeBoundingBox();
        }
        */
        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PreDrawObjects(e);

            var xColor = Rhino.ApplicationSettings.AppearanceSettings.GridXAxisLineColor;
            var yColor = Rhino.ApplicationSettings.AppearanceSettings.GridYAxisLineColor;
            var zColor = Rhino.ApplicationSettings.AppearanceSettings.GridZAxisLineColor;

            e.Display.EnableDepthWriting(false);
            e.Display.EnableDepthTesting(false);

            if (_projectPlugIn.CurrentBeam != null)
            {
                
                Beam beam = _projectPlugIn.CurrentBeam;

                Point3d c = beam.CrossSec.Centroid();
                BoundingBox bb = beam.CrossSec.GetBoundingBox(Plane.WorldXY);
                double length = bb.Diagonal.Length;

                //Create neutral axis
                int index = beam.LoadCases.FindIndex(o => o.IsDisplayed);
                if (index != -1)
                {
                    SimpleLoadCase slc = (SimpleLoadCase)beam.LoadCases[index];
                    Point3d p = slc.NeutralAxis.Origin;
                    Plane pl = slc.NeutralAxis;
                    Point3d startPt = p - pl.XAxis * length / 2;
                    double distance = new Vector3d((p + pl.XAxis * length / 2) - (p - pl.XAxis * length / 2)).Length;
                    int totNum = 20;

                    List<Line> ll = new List<Line>();
                    for (int i = 0; i < totNum; i = i + 2)
                    {
                        ll.Add(new Line(startPt + pl.XAxis * distance * (i) / totNum, startPt + pl.XAxis * distance * (i + 1) / totNum));
                    }

                    ll.ToArray();

                    Plane textPlane = new Plane(pl);
                    textPlane.Translate(new Vector3d(pl.XAxis * length / 1.9));
                    e.Display.DrawArrow(new Line(c, pl.ZAxis * length / 12), xColor);
                    e.Display.DrawArrow(new Line(c, pl.YAxis * length / 12), yColor);
                    e.Display.DrawArrow(new Line(c, pl.XAxis * length / 12), zColor);
                    e.Display.DrawLines(ll, System.Drawing.Color.Aquamarine);
                    e.Display.Draw3dText(new Text3d("N.A", textPlane, length / 30), System.Drawing.Color.Aquamarine);
                }


            }

            e.Display.EnableDepthWriting(false);
            e.Display.EnableDepthTesting(false);
        }

    }
}
