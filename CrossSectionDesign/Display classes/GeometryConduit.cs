using CrossSectionDesign.Classes_and_structures;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Display_classes
{
    public class GeometryConduit : Rhino.Display.DisplayConduit
    {
        enum Material
        {
            Steel = 0,
            Concrete = 1,
            Selected = 2

        }

        private readonly Dictionary<Material, Color> _colors = new Dictionary<Material, Color>()
        {
            {Material.Concrete,Color.Gray},
            {Material.Steel,Color.Black }
        };

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
             base.CalculateBoundingBox(e);
             Brep[] b = ProjectPlugIn.Instance.beamBreps.Select(o => o.Item2).ToArray();
            foreach (Brep bb in b)
            {
                e.IncludeBoundingBox(bb.GetBoundingBox(false));
            }
             
        }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);

            Brep[] b= ProjectPlugIn.Instance.beamBreps.Select(o => o.Item2).ToArray();

            foreach (Brep brep in b)
            {
                e.Display.DrawBrepShaded(brep, new DisplayMaterial(_colors[Material.Concrete]));
            }
        }

    }
}
