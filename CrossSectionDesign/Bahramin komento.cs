using System;
using Rhino;
using Rhino.Commands;

namespace TestProject
{
    [System.Runtime.InteropServices.Guid("a97fc514-95b6-47b9-b837-c926a6c92b2f")]
    public class Bahramin_komento : Command
    {
        static Bahramin_komento _instance;
        public Bahramin_komento()
        {
            _instance = this;
        }

        ///<summary>The only instance of the Bahramin_komento command.</summary>
        public static Bahramin_komento Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "Bahramin_komento"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            return Result.Success;
        }
    }
}
