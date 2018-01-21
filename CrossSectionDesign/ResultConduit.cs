using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Display;
using Rhino.Geometry;

namespace CrossSectionDesign
{
    class ResultConduit: Rhino.Display.DisplayConduit
    {
        public List<Tuple<Brep,double>> Breps { get; set; }
        private double _maxValue;
        private double _minValue;


        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);
            BoundingBox box = CreateBoundingBox();
            e.IncludeBoundingBox(box);
            


        }

        private BoundingBox CreateBoundingBox()
        {
            var box = new BoundingBox(new Point3d[] { new Point3d(0, 0, 0) });
            if (Breps != null)
            {
                foreach (Tuple<Brep,double> tuple in Breps)
                {
                    box = BoundingBox.Union(box, tuple.Item1.GetBoundingBox(false));
                }
            }
            return box;
        }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);

            if (_maxValue == 0)
            {
                calcMaxAndMin();
            }


            foreach (Tuple<Brep, double> tuple in Breps)
            {
                double value = (tuple.Item2 - _minValue) / (_maxValue - _minValue);
                ColorRGB color = Utils.HSL2RGB(value, 0.5, 0.5);

                e.Display.DrawBrepShaded(tuple.Item1, new DisplayMaterial(color));

            }


        }

        private void calcMaxAndMin()
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;

            foreach (Tuple<Brep, double> i in Breps)
            {
                maxValue = Math.Max(maxValue, i.Item2);
                minValue = Math.Min(minValue, i.Item2);
            }

            _maxValue = maxValue;
            _minValue = minValue;

        }


    }

}
