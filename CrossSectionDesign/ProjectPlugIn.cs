using RMA.UI;
using System;
using Rhino;
using Rhino.PlugIns;

namespace CrossSectionDesign
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class ProjectPlugIn : Rhino.PlugIns.PlugIn

    {

        /// <summary>
        /// Is called when the plug-in is being loaded.
        /// </summary>
        protected override Rhino.PlugIns.LoadReturnCode OnLoad(ref string errorMessage)
        {
            System.Type panelType = typeof(MainPanel);
            Rhino.UI.Panels.RegisterPanel(this, panelType, "Cross Section Design", System.Drawing.SystemIcons.WinLogo);
            return Rhino.PlugIns.LoadReturnCode.Success;
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        public ProjectPlugIn()
        {
            Instance = this;
            RhinoApp.Idle += OnIdle; // subcribe;
        }

        ///<summary>
        ///Gets the only instance of the TestProjectPlugIn plug-in.
        ///</summary>
        public static ProjectPlugIn Instance
        {
            get; private set;
        }

        private static void OnIdle(object sender, EventArgs e)
        {
            RhinoApp.Idle -= OnIdle; // Unsubscribe
            RhinoApp.RunScript("_OpenMainPanel", false);
        }


        /// <summary>
        /// The tabbed dockbar user control
        /// </summary>
        public MainPanel UserControl
        {
            get;
            set;
        }

        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;



    }
}