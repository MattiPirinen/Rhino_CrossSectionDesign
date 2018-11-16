using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CrossSectionDesign.Display_classes;
using CrossSectionDesign.Enumerates;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.PlugIns;


namespace CrossSectionDesign.Classes_and_structures
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

        public int SelectedBeamIndex { get; set; }
        public LocalAxisConduit LocalAxisConduit { get; set; }
        public BackGroundConduit BackGroundConduit { get; set; }
        public ResultConduit ResultConduit { get; set; }
        public ColorScaleDisplay ColorScaleDisplay { get; set; }
        public GeometryConduit GeomConduit { get; set; }
        public ResultConduit ColumnResultConduit { get; set; }
        public CrackWidthConduit CrackWidthConduit { get; set; }
        public HeatFlowConduit HeatFlowConduit { get; set; }
        public RhinoDoc ActiveDoc { get; private set; }
        public CursorConduit CursorConduit { get; set; }
        public InspectionPointConduit InspectionPointConduit { get; set; }


        public double Unitfactor { get; set; }

        public Beam CurrentBeam { get; set; }
        public List<Beam> Beams { get; set; } = new List<Beam>();

        private Dictionary<UnitSystem,double> unitfactors = new Dictionary<UnitSystem, double>()
        {
            {UnitSystem.Meters, 1.0 },
            {UnitSystem.Millimeters, 0.001 },
            {UnitSystem.Centimeters, 0.01 }
        };



        public Point3d NextCentroid()
        {
            return new Point3d(10000 * (Beams.Count+0), 0, 0);
        }

        /// <summary>
        /// Is called when the plug-in is being loaded.
        /// </summary>
        protected override Rhino.PlugIns.LoadReturnCode OnLoad(ref string errorMessage)
        {
            //System.Type panelType = typeof(MainPanel);
            //Rhino.UI.Panels.RegisterPanel(this, panelType, "Cross Section Design", System.Drawing.SystemIcons.WinLogo);
            return Rhino.PlugIns.LoadReturnCode.Success;
            
        }

        //This function is overrided so that the plugin will save userdata
        protected override bool ShouldCallWriteDocument(FileWriteOptions options)
        {
            return true;
        }

        protected override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options)
        {
            ArchivableDictionary dict = new ArchivableDictionary();
            int i = 1;
            dict.Set("BeamCount", Beams.Count);
            dict.Set("CountableUserData", CountableUserData.getCounter());
            foreach (Beam beam in Beams)
            {
                if (beam.GetType() == typeof(Column))
                {
                    Column col = beam as Column;
                    dict.Set("BeamType" + i, "Column");
                    dict.Set("ky" + i, col.Ky);
                    dict.Set("kz" + i, col.Kz);
                    dict.Set("ColLength" + i, col.Length);

                    dict.Set("CliConcreteStrenghtClass" + i, beam.CrossSec.ConcreteMaterial.StrengthClass ?? null);
                    dict.Set("ColumnCalculationSettings1" + i, 
                        col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature1]);
                    dict.Set("ColumnCalculationSettings2" + i,
                        col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature2]);
                    dict.Set("ColumnCalculationSettings3" + i,
                        col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness1]);
                    dict.Set("ColumnCalculationSettings4" + i,
                        col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness2]);
                }
                else
                {
                    dict.Set("BeamType" + i, "Other");
                }


                dict.Set("Rh" + i, beam.ClimateCond.RH);
                dict.Set("T0" + i, beam.ClimateCond.T0);
                dict.Set("T" + i, beam.ClimateCond.T);


                dict.Set("BeamName" + i, beam.Name);


                dict.Set("Gammac" + i, beam.Gammac);
                dict.Set("Gammas" + i, beam.Gammas);
                dict.Set("Gammar" + i, beam.Gammar);
                dict.Set("Acc" + i, beam.Acc);

                dict.Set("CrossSecName" + i, beam.CrossSec.Name);
                dict.Set("ConcreteStrenghtClass" + i, beam.CrossSec.ConcreteMaterial.StrengthClass ?? null);
                dict.Set("BeamId" + i, beam.CrossSec.Id);
                dict.Set("geomLarges" + i, beam.CrossSec.GeometryLargeIds);
                dict.Set("reinf" + i, beam.CrossSec.ReinforementIds);


                if (beam.CrossSec.GetType() == typeof(RectangleCrossSection))
                {
                    RectangleCrossSection crossSectionTemp = beam.CrossSec as RectangleCrossSection;
                    dict.Set("CrossSectionType" + i, "Rect");
                    dict.Set("NoReinfH" + i, crossSectionTemp.NoReinfH);
                    dict.Set("NoReinfW" + i, crossSectionTemp.NoReinfW);
                    dict.Set("ConcreteCover" + i, crossSectionTemp.ConcreteCover);
                    dict.Set("ConcreteWidth" + i, crossSectionTemp.ConcreteWidth);
                    dict.Set("ConcreteHeight" + i, crossSectionTemp.ConcreteHeight);
                    dict.Set("HasSteelShell" + i, crossSectionTemp.HasSteelShell);
                    dict.Set("SteelThickness" + i, crossSectionTemp.SteelThickness);
                    dict.Set("MainDiameter" + i, crossSectionTemp.MainD);
                    dict.Set("StirrupDiameter" + i, crossSectionTemp.StirrupD);
                    dict.Set("SteelMaterialName" + i, crossSectionTemp.SteelMaterial.StrengthClass ?? null);
                    dict.Set("ReinforcementMaterialName" + i, crossSectionTemp.ReinfMaterial.StrengthClass ?? null);
                    dict.Set("Rotation" + i, crossSectionTemp.Rotation);
                }
                else if (beam.CrossSec.GetType() == typeof(CrossSection))
                    dict.Set("CrossSectionType" + i, "Basic");
                else if (beam.CrossSec.GetType() == typeof(CircleCrossSection))
                {
                    CircleCrossSection crossSectionTemp = beam.CrossSec as CircleCrossSection;
                    dict.Set("CrossSectionType" + i, "Rect");
                    dict.Set("NoReinf" + i, crossSectionTemp.NoReinf);
                    dict.Set("ConcreteCover" + i, crossSectionTemp.ConcreteCover);
                    dict.Set("ConcreteDiameter" + i, crossSectionTemp.ConcreteDiameter);
                    dict.Set("HasSteelShell" + i, crossSectionTemp.HasSteelShell);
                    dict.Set("SteelThickness" + i, crossSectionTemp.SteelThickness);
                    dict.Set("MainDiameter" + i, crossSectionTemp.MainD);
                    dict.Set("StirrupDiameter" + i, crossSectionTemp.StirrupD);
                    dict.Set("SteelMaterialName" + i, crossSectionTemp.SteelMaterial.StrengthClass ?? null);
                    dict.Set("ReinforcementMaterialName" + i, crossSectionTemp.ReinfMaterial.StrengthClass ?? null);
                }

                int k = 1;
                dict.Set("NumberOfLoadCases" + i, beam.LoadCases.Count);

                foreach (LoadCase loadCase in beam.LoadCases)
                {
                    if (loadCase.GetType() == typeof(ColLoadCase))
                    {
                        ColLoadCase clc = (ColLoadCase)loadCase;
                        dict.Set("LoadCaseType" + i + "s" + k, "ColLoadCase");
                        dict.Set("N_Ed"+i+"s"+k, clc.N_Ed);
                        dict.Set("M_EzTop" + i + "s" + k, clc.M_EzTop);
                        dict.Set("M_EzBottom" + i + "s" + k, clc.M_EzBottom);
                        dict.Set("M_EyTop" + i + "s" + k, clc.M_EyTop);
                        dict.Set("M_EyBottom" + i + "s" + k, clc.M_EyBottom);
                        dict.Set("Ratio" + i + "s" + k, clc.Ratio);
                        dict.Set("CCurve" + i + "s" + k, clc.Ccurve);
                    }
                    else if (loadCase.GetType() == typeof(SimpleLoadCase))
                    {
                        SimpleLoadCase slc = (SimpleLoadCase)loadCase;
                        dict.Set("LoadCaseType" + i + "s" + k, "SimpleLoadCase");
                        dict.Set("N_Ed" + i + "s" + k, slc.N_Ed);
                        dict.Set("M_Edy" + i + "s" + k, slc.M_Edy);
                        dict.Set("M_Edz" + i + "s" + k, slc.M_Edz);
                    }
                    dict.Set("LoadCaseName" + i + "s" + k, loadCase.Name);
                    switch (loadCase.Ls)
                    {
                        case LimitState.Ultimate:
                            dict.Set("LimitState" + i + "s" + k, 0); break;
                        case LimitState.Service_CH:
                            dict.Set("LimitState" + i + "s" + k, 1); break;
                        case LimitState.Service_FR:
                            dict.Set("LimitState" + i + "s" + k, 2); break;
                        case LimitState.Service_QP:
                            dict.Set("LimitState" + i + "s" + k, 3); break;
                        default:
                            break;
                    }
                    
                    k++;
                }
                i++;
            }
            if (CurrentBeam != null)
                dict.Set("currentBeamId", CurrentBeam.Id);
            else
                dict.Set("currentBeamId", -1);

            archive.WriteDictionary(dict);
        }




        protected override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options)
        {
            Beams.Clear();
            CurrentBeam = null;
            ActiveDoc = RhinoDoc.ActiveDoc;
            if (unitfactors.ContainsKey(ActiveDoc.ModelUnitSystem))
            {
                Unitfactor = unitfactors[ActiveDoc.ModelUnitSystem];
                ActiveDoc.ModelAbsoluteTolerance = 0.0001;
            }
            else
            {
                MessageBox.Show("Cross section design tool does not support the chosen" +
                    "unit system. Unit system will be changed to millimeters.");
                ActiveDoc.ModelUnitSystem = UnitSystem.Millimeters;
                ActiveDoc.ModelAbsoluteTolerance = 0.0001;
                Unitfactor = unitfactors[ActiveDoc.ModelUnitSystem];
            }
                

            try
            {
                ArchivableDictionary dict = archive.ReadDictionary();

                int i = 1;
                int count = 0;
                if (dict["BeamCount"] != null)
                    count = (int)dict["BeamCount"];

                dict.Set("CountUserData", CountableUserData.getCounter());
                while (i < count + 1)
                {


                    Beam bTemp;
                    string beamName = (string)dict["BeamName" + i];

                    if ((string)dict["BeamType" + i] == "Column")
                    {
                        bTemp = new Column(beamName, dict.GetDouble("Gammas" + i),
                            dict.GetDouble("Gammac" + i),
                            dict.GetDouble("Gammar" + i),
                            dict.GetDouble("Acc" + i))
                        {
                            Length = (double)dict["ColLength" + i],
                            Ky = (double)dict["ky" + i],
                            Kz = (double)dict["kz" + i],
                        };
                        ((Column)bTemp).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature1] =
                            dict.GetBool("ColumnCalculationSettings1" + i);
                        ((Column)bTemp).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature2] =
                            dict.GetBool("ColumnCalculationSettings2" + i);
                        ((Column)bTemp).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness1] =
                            dict.GetBool("ColumnCalculationSettings3" + i);
                        ((Column)bTemp).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness2] =
                            dict.GetBool("ColumnCalculationSettings4" + i);
                    }
                    else
                    {
                        bTemp = new Beam(beamName, dict.GetDouble("Gammas" + i),
                            dict.GetDouble("Gammac" + i),
                            dict.GetDouble("Gammar" + i),
                            dict.GetDouble("Acc" + i))
                        {
                        };
                    }

                    bTemp.ClimateCond = new ClimateCondition(
                        (int)dict["Rh" + i],
                        (int)dict["T0" + i],
                        (int)dict["T" + i],
                        bTemp);

                    string crossSecName = (string)dict["CrossSecName" + i];
                    CrossSection cTemp;
                    if ((string)dict["CrossSectionType" + i] == "Basic")
                    {
                        cTemp =
                            new CrossSection(crossSecName, bTemp)
                            {
                            };
                    }

                    else if (dict.GetString("CrossSectionType" + i) == "Rect")
                    {
                        cTemp =
                            new RectangleCrossSection(crossSecName, bTemp)
                            {
                                NoReinfH = (int)dict["NoReinfH" + i],
                                NoReinfW = (int)dict["NoReinfW" + i],
                                ConcreteCover = (int)dict["ConcreteCover" + i],
                                ConcreteWidth = dict.GetInteger("ConcreteWidth" + i),
                                ConcreteHeight = dict.GetInteger("ConcreteHeight" + i),
                                HasSteelShell = dict.GetBool("HasSteelShell" + i),
                                SteelThickness = dict.GetDouble("SteelThickness" + i),
                                MainD = dict.GetInteger("MainDiameter" + i),
                                StirrupD = dict.GetInteger("StirrupDiameter" + i),
                                SteelMaterial = new SteelMaterial(dict.GetString("SteelMaterialName" + i) ?? "S355", SteelType.StructuralSteel, bTemp),
                                ReinfMaterial = new SteelMaterial(dict.GetString("ReinforcementMaterialName" + i ?? "B500B"), SteelType.Reinforcement, bTemp),
                                Rotation = dict.GetInteger("Rotation" + i),
                            };

                    }
                    else
                    {
                        cTemp =
                            new CircleCrossSection(crossSecName, bTemp)
                            {
                                NoReinf = (int)dict["NoReinf"],
                                ConcreteCover = (int)dict["ConcreteCover" + i],
                                ConcreteDiameter = dict.GetInteger("ConcreteDiameter" + i),
                                HasSteelShell = dict.GetBool("HasSteelShell" + i),
                                SteelThickness = dict.GetDouble("SteelThickness" + i),
                                MainD = dict.GetInteger("MainDiameter" + i),
                                StirrupD = dict.GetInteger("StirrupDiameter" + i),
                                SteelMaterial = new SteelMaterial(dict.GetString("SteelMaterialName" + i) ?? "S355", SteelType.StructuralSteel, bTemp),
                                ReinfMaterial = new SteelMaterial(dict.GetString("ReinforcementMaterialName" + i) ?? "B500B", SteelType.Reinforcement, bTemp),
                            };

                    }


                    //Sets a link between reinforcements and the beam
                    List<int> reinforcements = ((int[])dict["reinf" + i]).ToList();

                    List<Reinforcement> temp = GetReinforcements(reinforcements);
                    temp.ForEach(o => o.Material.Bm = bTemp);
                    temp.ForEach(o => o.OwnerCrossSection = cTemp);


                    //Sets a link between geometry larges and the beam
                    List<int> geometryLarges = ((int[])dict["geomLarges" + i]).ToList();

                    List<GeometryLarge> gls = GetGeometryLarges(geometryLarges);
                    gls.ForEach(o => o.Material.Bm = bTemp);
                    gls.ForEach(o => o.OwnerCrossSection = cTemp);

                    cTemp.ConcreteMaterial = new ConcreteMaterial((string)dict["ConcreteStrenghtClass" + i], bTemp);
                    bTemp.CrossSec = cTemp;
                    geometryLarges.ForEach(id => cTemp.GeometryLargeIds.Add(id));
                    reinforcements.ForEach(id => cTemp.ReinforementIds.Add(id));
                    bTemp.Id = dict.GetInteger("BeamId" + i);
                    Beams.Add(bTemp);




                    //Set loadCases
                    int lc_n = dict.GetInteger("NumberOfLoadCases" + i);
                    int k = 1;
                    while (k <= lc_n)
                    {
                        string name = dict.GetString("LoadCaseName" + i + "s" + k);
                        int limitStatenumb = dict.GetInteger("LimitState" + i + "s" + k);

                        LimitState ls;
                        switch (limitStatenumb)
                        {
                            case 0:
                                ls = LimitState.Ultimate; break;
                            case 1:
                                ls = LimitState.Service_CH; break;
                            case 2:
                                ls = LimitState.Service_FR; break;
                            case 3:
                                ls = LimitState.Service_QP; break;
                            default:
                                ls = LimitState.Ultimate; break;
                        };

                        if (dict.GetString("LoadCaseType" + i + "s" + k) == "ColLoadCase")
                        {
                            ColLoadCase clc = new ColLoadCase(
                                dict.GetDouble("N_Ed" + i + "s" + k),
                                dict.GetDouble("M_EzTop" + i + "s" + k),
                                dict.GetDouble("M_EzBottom" + i + "s" + k),
                                dict.GetDouble("M_EyTop" + i + "s" + k),
                                dict.GetDouble("M_EyBottom" + i + "s" + k),
                                (Column)bTemp,
                                dict.GetDouble("Ratio" + i + "s" + k),
                                name,
                                dict.GetDouble("CCurve" + i + "s" + k),
                                ls);
                        }
                        else if (dict.GetString("LoadCaseType" + i + "s" + k) == "SimpleLoadCase")
                        {
                            SimpleLoadCase slc = new SimpleLoadCase(
                                dict.GetDouble("N_Ed" + i + "s" + k),
                                dict.GetDouble("M_Edz" + i + "s" + k),
                                dict.GetDouble("M_Edy" + i + "s" + k),
                                bTemp,
                                name,
                                ls);
                            bTemp.LoadCases.Add(slc);
                        }
                        k++;
                    }


                    i++;

                }
                Countable.SetCounter(i - 2);
                if (Beams.Count != 0)
                {
                    int currentID = (int)dict["currentBeamId"];
                    if (currentID != -1)
                        CurrentBeam = Beams.FirstOrDefault(beam => beam.Id == currentID);

                }

                CountableUserData.setCounter((int)dict["CountableUserData"]);

                if (MainForm != null)
                {
                    MainForm.ChangeToStartView();
                }

                //If there is previous display results, clear them
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                base.ReadDocument(doc, archive, options);
            }
            
        }

        private List<GeometryLarge> GetGeometryLarges(List<int> GeometryLargeIds)
        {
            List<GeometryLarge> geometryLarges = new List<GeometryLarge>();
            RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;

                GeometryLarge temp = list.Find(typeof(GeometryLarge)) as GeometryLarge ?? list.Find(typeof(RectangleGeometryLarge)) as GeometryLarge;

                if (temp == null)
                {
                    temp = list.Find(typeof(RectangleGeometryLarge)) as GeometryLarge;
                }
                if (GeometryLargeIds.IndexOf(temp.Id) != -1)
                    geometryLarges.Add(temp);
            }

            return geometryLarges;
        }

        public List<Reinforcement> GetReinforcements(List<int> ReinforementIds)
        {
            List<Reinforcement> temp = new List<Reinforcement>();
            RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                Reinforcement tempReinf = list.Find(typeof(Reinforcement)) as Reinforcement;
                if (ReinforementIds.IndexOf(tempReinf.Id) != -1)
                    temp.Add(tempReinf);
            }
            return temp;
        }

        private void onActiveDocumentChanged(object sender, DocumentEventArgs e)
        {
            ActiveDoc = RhinoDoc.ActiveDoc;

            if (unitfactors.ContainsKey(ActiveDoc.ModelUnitSystem))
            {
                Unitfactor = unitfactors[ActiveDoc.ModelUnitSystem];
                ActiveDoc.ModelAbsoluteTolerance = 0.0001;
            }
            else
            {
                MessageBox.Show("Cross section design tool does not support the chosen" +
                    "unit system. Unit system will be changed to millimeters.");
                ActiveDoc.ModelUnitSystem = UnitSystem.Millimeters;
                ActiveDoc.ModelAbsoluteTolerance = 0.0001;
                Unitfactor = unitfactors[ActiveDoc.ModelUnitSystem];
            }
        }

        private void onMouseMoved(object sender, GetPointMouseEventArgs e)
        {
            
            CursorConduit.text =e.Point.ToString();
            ActiveDoc.Views.Redraw();
        }


        private List<Brep> GetConcreteBreps()
        {
            List<Brep> brepList = new List<Brep>();
            if (CurrentBeam != null)
            {
                brepList.AddRange(CurrentBeam.CrossSec.GetGeometryLarges()
                    .Where(o => o.Material is ConcreteMaterial).Select(o => o.BaseBrep).ToList());

            }
            return brepList;
        }

        private List<Brep> GetSteelBreps()
        {
            List<Brep> brepList = new List<Brep>();
            if (CurrentBeam != null)
            {
                brepList.AddRange(CurrentBeam.CrossSec.GetReinforcements().Select(r => r.BrepGeometry).ToList());
            }
            return brepList;
        }
        private GetPoint gp;

        /// <summary>
        /// Public constructor
        /// </summary>
        public ProjectPlugIn()
        {
            RhinoDoc.EndOpenDocument += onActiveDocumentChanged;
            RhinoDoc.NewDocument += onActiveDocumentChanged;
            gp = new GetPoint();
            gp.MouseMove += onMouseMoved;


            Instance = this;
            BackGroundConduit = new BackGroundConduit() { Enabled = true };
            ResultConduit = new ResultConduit() { Enabled = true };
            LocalAxisConduit = new LocalAxisConduit(this) { Enabled = true };
            ColorScaleDisplay = new ColorScaleDisplay();
            CrackWidthConduit = new CrackWidthConduit() { Enabled = true };
            HeatFlowConduit = new HeatFlowConduit() { Enabled = true };
            CursorConduit = new CursorConduit() { Enabled = true };
            InspectionPointConduit = new InspectionPointConduit() { Enabled = false };
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
            MainPanel p = new MainPanel();
            p.Show();
        }


        /// <summary>
        /// Userform
        /// </summary>
        public MainPanel MainForm
        {
            get;
            set;
        }
        public HeatFlowForm HeatFlowForm { get; set; }
        public RFEMAnalysisForm RFEMForm { get; set; }
        public ClimateConditionsForm ClimateForm {get;set;}
        public ChooseColumnsForm ChooseColForm { get; set; }
        public ChartForm ChForm { get; set; }
        public List<Tuple<int,Brep>> beamBreps = new List<Tuple<int,Brep>>();
        
        public override PlugInLoadTime LoadTime { get { return PlugInLoadTime.AtStartup; } }



    }
}