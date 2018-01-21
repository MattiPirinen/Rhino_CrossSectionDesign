using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.PlugIns;

namespace CrossSectionDesign
{
    class PlugInOpen:Rhino.PlugIns.PlugIn
    {
        public PlugInOpen()
        {
            Instance = this;
            RhinoApp.Idle += OnIdle; //subscribe
        }

        private static void OnIdle(object sender, EventArgs e)
        {
            RhinoApp.Idle -= OnIdle; // unsubscribe
            RhinoApp.RunScript("_OpenMainPanel", false);
        }

        ///<summary>Gets the only instance of the plug-in.</summary>
        public static PlugInOpen Instance
        {
            get;
            private set;
        }

        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
    }
}
