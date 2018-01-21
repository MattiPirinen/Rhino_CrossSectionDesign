using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;

namespace CrossSectionDesign
{
    [System.Runtime.InteropServices.Guid("a1fbffbe-a487-4174-9956-8a58229242d5")]
    public class lockLayer : Command
    {
        static lockLayer _instance;
        public lockLayer()
        {
            _instance = this;
        }

        ///<summary>The only instance of the lockLayer command.</summary>
        public static lockLayer Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "lockLayer"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            //doc.Layers[0].IsLocked = !doc.Layers[0].IsLocked;

            Layer layer = doc.Layers[0];
            layer.IsLocked = !layer.IsLocked;
            layer.CommitChanges();
            return Result.Success;
        }
    }
}
