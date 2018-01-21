using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace CrossSectionDesign
{
    class BackGroundConduit : Rhino.Display.DisplayConduit
    {

        enum Material
        {
            Steel = 0,
            Concrete = 1
        }

        public Line line = new Line(new Point3d(0, 0, 0), new Point3d(1, 0, 0));

        public List<Brep> DisplayBrepConcrete { get; set; } = new List<Brep>();
        public List<Brep> DisplayBrepSteel { get; set; } = new List<Brep>();
        private Dictionary<Material,Color> _colors = new Dictionary<Material, Color>()
        {
            { Material.Concrete,Color.Gray},
            {Material.Steel,Color.Black }
        };



        public BackGroundConduit()
        {


        }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);
            BoundingBox box = CreateBoundingBox();
            e.IncludeBoundingBox(box);
    }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);

            foreach (Brep brep in DisplayBrepConcrete)
            {
                e.Display.DrawBrepShaded(brep, new DisplayMaterial(_colors[Material.Concrete]));
                
            }

            foreach (Brep brep in DisplayBrepSteel)
            {
                e.Display.DrawBrepShaded(brep, new DisplayMaterial(_colors[Material.Steel]));
            }

        }


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
    }
}