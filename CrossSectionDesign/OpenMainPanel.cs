using System;
using Rhino;
using Rhino.Commands;

namespace CrossSectionDesign
{
    [System.Runtime.InteropServices.Guid("27ec48ec-b4fc-49c9-bd67-d1311d62113e")]
    [CommandStyle(Style.ScriptRunner)]
    public class OpenMainPanel : Command
    {
        public OpenMainPanel()
        {
            Instance = this;
            RhinoApp.Idle += OnIdle;
        }

        private void OnIdle(object sender, EventArgs e)
        {
            RhinoApp.Idle -= OnIdle;

        }

        ///<summary>The only instance of the OpenMainPanel command.</summary>
        public static OpenMainPanel Instance { get; private set; }

        public override string EnglishName => "OpenMainPanel";


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //Opens the main painel
            var type = typeof(MainPanel);
            Rhino.UI.Panels.OpenPanel(type.GUID);
            return Result.Success;
        }
    }
}
