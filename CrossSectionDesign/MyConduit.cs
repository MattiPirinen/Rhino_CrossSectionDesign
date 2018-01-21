using System.Drawing;
using Rhino.Display;
using Rhino.Geometry;

namespace CrossSectionDesign
{
    class MyConduit : Rhino.Display.DisplayConduit
    {
        public Line MyLine  { get; set; }
        public MyConduit(Line testingLine)
        {
            MyLine = testingLine;
        }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);
            e.IncludeBoundingBox(MyLine.BoundingBox);
        }

        protected override void PreDrawObject(DrawObjectEventArgs e)
        {
            base.PreDrawObject(e);
            e.Display.DrawLine(MyLine, Color.Red);
        }
    }
}