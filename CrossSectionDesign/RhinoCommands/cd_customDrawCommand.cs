using System;
using CrossSectionDesign.Display_classes;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;

namespace CrossSectionDesign.RhinoCommands
{
    [System.Runtime.InteropServices.Guid("f7db2c8a-f294-4e60-9b41-029c3dda0087")]
    public class cd_customDrawCommand : Command
    {
        public cd_customDrawCommand()
        {
            Instance = this;
        }

        ///<summary>The only instance of the cd_customDrawCommand command.</summary>
        public static cd_customDrawCommand Instance { get; private set; }

        public override string EnglishName => "cd_customDrawCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            doc.Objects.AddPoint(new Point3d(0, 0, 0));


            Random rand = new Random();
            
            Line testLine = new Line(new Point3d( 0,0,0),new Point3d(rand.Next(100),rand.Next(100), 0));
            
            MyConduit cond = new MyConduit(testLine) {Enabled = true};
            doc.Views.Redraw();

            cond.Enabled = false;
            return Result.Success;
        }
    }
}
