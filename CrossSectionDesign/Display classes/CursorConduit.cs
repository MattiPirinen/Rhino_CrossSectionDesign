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
    public class CursorConduit : Rhino.Display.DisplayConduit
    {
        public string text { get; set; } = "";


        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
           // base.CalculateBoundingBox(e);
           // BoundingBox box = CreateBoundingBox();
           // e.IncludeBoundingBox(box);
    }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);

            Point2d pt = Rhino.UI.MouseCursor.Location;
            e.Display.Draw2dText(text, Color.Blue, pt, false);
        }


        protected override void DrawForeground(DrawEventArgs e)
        {
            base.DrawForeground(e);
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