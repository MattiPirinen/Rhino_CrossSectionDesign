using System;
using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Interfaces;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.PlugIns;
using Rhino.Geometry.Intersect;
using CrossSectionDesign.Static_classes;
using Rhino.DocObjects.Custom;
using Excel = Microsoft.Office.Interop.Excel;

namespace CrossSectionDesign
{
    [System.Runtime.InteropServices.Guid("0b0c3a6e-7efb-47c9-b2e3-7d92788a9f74")]
    public partial class MainPanel : Form
    {

        private string chartSeriesName = "Strength";
        private string chartSeriesName2 = "Strength2";
        private int chartSeriesNumb = 1;
        

        private void AddListeners()
        {
            
            textBoxHeigth.KeyDown += leaveControl;
            textBoxWidth.KeyDown += leaveControl;
            textBoxRotation.KeyDown += leaveControl;
            textBoxConcreteC.KeyDown += leaveControl;
            textBoxMainD.KeyDown += leaveControl;
            textBoxAmountH.KeyDown += leaveControl;
            textBoxAmountW.KeyDown += leaveControl;
            textBoxStirrupD.KeyDown += leaveControl;
            textBoxRC_SteelThickness.KeyDown += leaveControl;
            textBoxColumnLength.KeyDown += leaveControl;
            textBoxKy.KeyDown += leaveControl;
            textBoxKz.KeyDown += leaveControl;
            textBox_cs_max.KeyDown += leaveControl;
            textBox_cs_min.KeyDown += leaveControl;


            textBoxHeigth.Leave += updateResultsEvent;
            textBoxWidth.Leave += updateResultsEvent;
            textBoxRotation.Leave += updateResultsEvent;
            textBoxConcreteC.Leave += updateResultsEvent;
            textBoxMainD.Leave += updateResultsEvent;
            textBoxAmountH.Leave += updateResultsEvent;
            textBoxAmountW.Leave += updateResultsEvent;
            textBoxStirrupD.Leave += updateResultsEvent;
            textBoxRC_SteelThickness.Leave += updateResultsEvent;

            textBoxKz.Leave += updateResultsEvent;
            textBoxKy.Leave += updateResultsEvent;
            textBoxColumnLength.Leave += updateResultsEvent;
            textBox_cs_min.Leave += UpdateColorScale;
            textBox_cs_max.Leave += UpdateColorScale;

            //textBoxConcreteStrength.TextChanged += updateAccordingToStrengthTextBoxes;
            //textBoxSteelStrength.TextChanged += updateAccordingToStrengthTextBoxes;
            //textBoxReinfStrength.TextChanged += updateAccordingToStrengthTextBoxes;

            textBoxCSafetyFactor.Leave += updatePartialSafetyFactors;
            textBoxSSafetyFactor.Leave += updatePartialSafetyFactors;
            textBoxRSafetyFactor.Leave += updatePartialSafetyFactors;
            textBoxAcc.Leave += updatePartialSafetyFactors;

            textBoxConcreteStrength.Leave += updateCustomMaterialStrengths;
            textBoxSteelStrength.Leave += updateCustomMaterialStrengths;
            textBoxReinfStrength.Leave += updateCustomMaterialStrengths;

            comboBoxConcreteS.SelectedIndexChanged += updateResultsEvent;
            comboBoxConcreteS.SelectedIndexChanged += updateMaterialStrengthsEvent;
            comboBoxReinfS.SelectedIndexChanged += updateResultsEvent;
            comboBoxReinfS.SelectedIndexChanged += updateMaterialStrengthsEvent;
            comboBoxSteelS.SelectedIndexChanged += updateResultsEvent;
            comboBoxSteelS.SelectedIndexChanged += updateMaterialStrengthsEvent;
            comboBoxMaxAggregateSize.SelectedIndexChanged += updateResultsEvent;


            //Add restrictive listeners to textboxes to allow only surtain input
            textBoxLoadCaseNumbers.KeyPress += Only_Ints_commas_dashes_KeyPress;
            textBoxMemberNumber.KeyPress += OnlyInts_KeyPress;
        }

        private ProjectPlugIn _projectPlugIn;

        void UpdateColorScale(object sender, EventArgs e)
        {
            if (double.TryParse(textBox_cs_min.Text, out var min) &&
                double.TryParse(textBox_cs_max.Text, out var max))
            {
                _projectPlugIn.CurrentBeam.CrossSec.MinAndMaxStress = Tuple.Create(min, max);
            }
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();

        }

        public MainPanel()
        {
            _projectPlugIn = ProjectPlugIn.Instance;
            InitializeComponent();

            foreach (Beam beam in _projectPlugIn.Beams)
            {
                listBoxCrossSecs.Items.Add(beam.Name);
            }
            /*
            if (_projectPlugIn.CurrentBeam.CrossSec == null)
            {
                _projectPlugIn.CurrentBeam.CrossSec = new CrossSection(new ConcreteMaterial("C30/37"));
                _projectPlugIn.CrossSections.Add(_projectPlugIn.CurrentBeam.CrossSec);
            }
            */
            
            //TODO add a correct material assignment

            InitializeComponentValues();
            AddListeners();


            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
            // Set the user control property on our plug-in
            ProjectPlugIn.Instance.MainForm = this;

            // Create a visible changed event handler
            //this.VisibleChanged += new EventHandler(UserControl1_VisibleChanged);

            // Create a dispose event handler
            Disposed += new EventHandler(UserControl1_Disposed);


        }

        private void InitializeComponentValues()
        {

            tabPageCrossSection.Enabled = false;
            tabPageRecCross.Enabled = false;

            textBoxRC_SteelThickness.Enabled = checkBoxRC_OnOff.Checked;


            comboBoxMaxAggregateSize.Items.AddRange(ComboboxValues.AGGREGATE_SIZE);
            comboBoxMaxAggregateSize.SelectedItem = "32";

            comboBoxExposureClass.Items.AddRange(ComboboxValues.EXPOSURE_CLASSES);
            comboBoxExposureClass.SelectedItem = "XC3";

            comboBoxDesWorkingLife.Items.AddRange(ComboboxValues.DES_WORK_LIFE);
            comboBoxDesWorkingLife.SelectedItem = "50 years";

            comboBoxRH.Items.AddRange(ComboboxValues.RH);
            comboBoxRH.SelectedItem = "40%";

            comboBoxFreeRH.Items.AddRange(ComboboxValues.RH);
            comboBoxFreeRH.SelectedItem = "40%";

            comboBoxMaterialGeom.Items.AddRange(ComboboxValues.CONCRETE_STRENGTH_CLASSES);
            comboBoxMaterialGeom.SelectedItem = "C30/37";

            comboBoxMaterialReinf.Items.AddRange(ComboboxValues.REINF_STRENGTH_CLASSES);
            comboBoxMaterialReinf.SelectedItem = "B500B";

            comboBoxReinfS.Items.AddRange(ComboboxValues.REINF_STRENGTH_CLASSES);
            comboBoxReinfS.SelectedItem = "B500B";

            comboBoxConcreteS.Items.AddRange(ComboboxValues.CONCRETE_STRENGTH_CLASSES);
            comboBoxConcreteS.SelectedItem = "C30/37";

            comboBoxSteelS.Items.AddRange(ComboboxValues.STEEL_STRENGTH_CLASSES);
            comboBoxSteelS.SelectedItem = "S355";


            foreach (var item in Enum.GetValues(typeof(MaterialType)))
            {
                comboBoxMaterialType.Items.Add(item);
            }

            comboBoxMaterialType.SelectedIndex = 0;

            comboBoxDiam.Items.AddRange(ComboboxValues.STEEL_DIAMETER);
            comboBoxDiam.SelectedItem = "16";



        }

        void UserControl1_Disposed(object sender, EventArgs e)
        {
            // Clear the user control property on our plug-in
            ProjectPlugIn.Instance.MainForm = null;
        }

        /// <summary>
        /// Returns the ID of this panel.
        /// </summary>
        public static System.Guid PanelId
        {
            get
            {
                return typeof(MainPanel).GUID;
            }
        }

        private void EnablePredefinedCrossSection()
        {
            tabPageRecCross.Enabled = true;
            tabPageCrossSection.Enabled = false;
        }

        private void EnableGenericCrossSection()
        {
            tabPageRecCross.Enabled = false;
            tabPageCrossSection.Enabled = true;
        }

        private void buttonAddConcreteGeometry_Click(object sender, EventArgs e)
        {
            ShowStressResults(false);

            GeometryLarge temp = CreateGeometryLarge.CreateGeometry(
                (MaterialType)Enum.Parse(typeof(MaterialType), comboBoxMaterialType.SelectedItem.ToString()),
                comboBoxMaterialGeom.SelectedItem.ToString());
            //return if the geometry couldnt be created.
            if (temp == null)
                return;

            dataGridView_GeometryLarge.Rows.Add(temp.Id, temp.Material.GetType() == typeof(SteelMaterial)
                ? "Steel" : "Concrete");


            _projectPlugIn.CurrentBeam.CrossSec.GeometryLargeIds.Add(temp.Id);
            temp.CrosecId = _projectPlugIn.CurrentBeam.CrossSec.Id;
            MaterialType type =
                (MaterialType)Enum.Parse(typeof(MaterialType), comboBoxMaterialType.SelectedItem.ToString());
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();

            labelConcreteCover.Text = Math.Round(_projectPlugIn.CurrentBeam.CrossSec.ConcreteCover/
                _projectPlugIn.Unitfactor, 0).ToString();
        }

        private void buttonAddReinf_Click_1(object sender, EventArgs e)
        {
            ShowStressResults(false);

            Reinforcement[] temp = CreateReinforcement.CreateReinforcements(
                comboBoxMaterialReinf.SelectedItem.ToString(),
                double.Parse(comboBoxDiam.SelectedItem.ToString()) * Math.Pow(10, -3));

            foreach (Reinforcement reinforcement in temp)
            {
                reinforcement.CroSecId = _projectPlugIn.CurrentBeam.CrossSec.Id;
                _projectPlugIn.CurrentBeam.CrossSec.ReinforementIds.Add(reinforcement.Id);

                dataGridView_Reinforcement.Rows.Add(reinforcement.Id, "Reinforcement",reinforcement.Diameter*Math.Pow(10,3));
            }
            labelConcreteCover.Text = Math.Round(_projectPlugIn.CurrentBeam.CrossSec.ConcreteCover /
                _projectPlugIn.Unitfactor, 0).ToString();
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }


        //Sets strength values to the predefined cross section shape tab charts
        private void SetStrengthChartCurve(Chart chart, Moment m, Polyline strengthCurve)
        {

            
            chart.Series[chartSeriesName].Points.Clear();
            foreach (Point3d point in strengthCurve)
            {

                chart.Series[chartSeriesName].Points.AddXY((m == Moment.Mz || m == Moment.MComb ? point.Z : point.Y) * Math.Pow(10, -3), point.X * Math.Pow(10, -3));
                if (m == Moment.MComb)
                    chart.Series[chartSeriesName2].Points.AddXY(point.Y * Math.Pow(10, -3), point.X * Math.Pow(10, -3));

            }
            ChartManipulationTools.SetAxisIntervalAndMax(chart, strengthCurve, m);
        }

        private void buttonCalculate_Click_1(object sender, EventArgs e)
        {
            

            Polyline p = _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(Plane.WorldXY, GetLimitState());

            SetStrengthChartCurve(chartFreeMz, Moment.Mz,p);

        }


        /*
        // This method sets x and y axis interval and max values according to the strength curve max values
        private void SetAxisIntervalAndMax(Chart chart, List<Tuple<double, double>> values)
        {
            if (chartSeriesNumb > 1)
            {
                values.Clear();

                for (int i = 0; i < chart.Series["Strength"].Points.Count; i++)
                {
                    values.Add(Tuple.Create(chart.Series["Strength"].Points[i].YValues[0] * 1000, chart.Series["Strength"].Points[i].XValue * 1000));
                }
            }

            double maxValue = values.Max(x => x.Item2) * Math.Pow(10, -3);
            double minValue = values.Min(x => x.Item2) * Math.Pow(10, -3);
            Tuple<double, double, double> minMaxInterval = CreateInterval(maxValue, minValue);
            //Tuple<double, double, double> minMaxInterval = CreateInterval(maxValue, 0);
            chart.ChartAreas[0].AxisX.Crossing = 0;
            chart.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart.ChartAreas[0].AxisX.Minimum = minMaxInterval.Item1;
            chart.ChartAreas[0].AxisX.Maximum = minMaxInterval.Item2;
            chart.ChartAreas[0].AxisX.MajorGrid.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.MinorGrid.Interval = minMaxInterval.Item3 / 5;
            chart.ChartAreas[0].AxisX.LabelStyle.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = minMaxInterval.Item3;


            //Y-axis min, max and interval 
            maxValue = values.Max(x => x.Item1) * Math.Pow(10, -3);
            minValue = values.Min(x => x.Item1) * Math.Pow(10, -3);
            minMaxInterval = CreateInterval(maxValue, minValue);
            //minMaxInterval = CreateInterval(0, minValue);
            //minMaxInterval = CreateInterval(maxValue, 0);

            chart.ChartAreas[0].AxisY.Minimum = minMaxInterval.Item1;
            chart.ChartAreas[0].AxisY.Maximum = minMaxInterval.Item2;
            chart.ChartAreas[0].AxisY.MajorGrid.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.MinorGrid.Interval = minMaxInterval.Item3 / 5;
            chart.ChartAreas[0].AxisY.LabelStyle.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.MajorTickMark.Interval = minMaxInterval.Item3;
        }
        */





        //Adds the correct material values to the dropdown




        private void comboBoxMaterialType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Enum.Parse(typeof(MaterialType), comboBoxMaterialType.SelectedItem.ToString()))
            {
                case MaterialType.Concrete:
                    comboBoxMaterialGeom.Items.Clear();
                    comboBoxMaterialGeom.Items.AddRange(ComboboxValues.CONCRETE_STRENGTH_CLASSES);
                    comboBoxMaterialGeom.SelectedItem = "C30/37";
                    break;
                case MaterialType.Steel:
                    comboBoxMaterialGeom.Items.Clear();
                    comboBoxMaterialGeom.Items.AddRange(ComboboxValues.STEEL_STRENGTH_CLASSES);
                    comboBoxMaterialGeom.SelectedItem = "S355";
                    break;
                default:
                    break;

            }
        }

        private void UpdateResultDisplay()
        {
            LoadCase lc;
            try
            {
                int i = _projectPlugIn.CurrentBeam.LoadCases.FindIndex(o => o.Name == "SampleLoadCase");
                lc = _projectPlugIn.CurrentBeam.LoadCases[i];
                lc.IsDisplayed = true;

                if (radioButtonSteel.Checked)
                    _projectPlugIn.CurrentBeam.CrossSec.CreateResultDisplay(true, lc);
                else
                    _projectPlugIn.CurrentBeam.CrossSec.CreateResultDisplay(false, lc);
                //brepsAndStrains.ForEach(o => ProjectPlugIn.Instance.ActiveDoc.Objects.AddBrep(o.Item1));

                //Update scale
                textBox_cs_min.Enabled = true;
                textBox_cs_max.Enabled = true;
                textBox_cs_min.Text = (Math.Round(_projectPlugIn.CurrentBeam.CrossSec.MinAndMaxStress.Item1 * Math.Pow(10, -6), 0)).ToString();
                textBox_cs_max.Text = (Math.Round(_projectPlugIn.CurrentBeam.CrossSec.MinAndMaxStress.Item2 * Math.Pow(10, -6), 0)).ToString();

                ShowStressResults(true);
                checkBoxShowCrackWidth.Checked = true;

            }
            catch
            {
                //MessageBox.Show("No Loadcase could be found");
            };
        }

        private LimitState GetLimitState()
        {
            
            if (radioButtonULS.Checked)
                return LimitState.Ultimate;
            else if (radioButtonSLS_CH.Checked)
                return LimitState.Service_CH;
            else if (radioButtonSLS_QP.Checked)
                return LimitState.Service_QP;
            else
                return LimitState.Ultimate;
        }

        private void CalculateStresses()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

            LimitState ls = GetLimitState();

            bool r;
            try
            {

                LoadCase lc;
                if (_projectPlugIn.CurrentBeam.LoadCases.TrueForAll(o => o.Name != "SampleLoadCase"))
                {
                    lc = new SimpleLoadCase(double.Parse(textBoxForce.Text) * 1000, double.Parse(textBoxMz.Text) * 1000
                    , double.Parse(textBoxMy.Text) * 1000, _projectPlugIn.CurrentBeam, "SampleLoadCase", ls);
                    _projectPlugIn.CurrentBeam.LoadCases.Add(lc);
                    
                }
                else
                {
                    int i = _projectPlugIn.CurrentBeam.LoadCases.FindIndex(o => o.Name == "SampleLoadCase");
                    lc = _projectPlugIn.CurrentBeam.LoadCases[i];
                    lc.Ls = ls;
                }

                r = _projectPlugIn.CurrentBeam.CrossSec.CalculateStresses(double.Parse(textBoxForce.Text) * 1000,
                double.Parse(textBoxMz.Text) * 1000, double.Parse(textBoxMy.Text) * 1000, "SampleLoadCase", ls);

                if (r)
                {
                    
                    if (radioButtonSteel.Checked)
                       _projectPlugIn.CurrentBeam.CrossSec.CreateResultDisplay(true, lc);
                    else
                       _projectPlugIn.CurrentBeam.CrossSec.CreateResultDisplay(false, lc);
                    //brepsAndStrains.ForEach(o => ProjectPlugIn.Instance.ActiveDoc.Objects.AddBrep(o.Item1));

                    textBox_cs_min.Enabled = true;
                    textBox_cs_max.Enabled = true;
                    textBox_cs_min.Text = (Math.Round(_projectPlugIn.CurrentBeam.CrossSec.MinAndMaxStress.Item1 * Math.Pow(10, -6), 0)).ToString();
                    textBox_cs_max.Text = (Math.Round(_projectPlugIn.CurrentBeam.CrossSec.MinAndMaxStress.Item2 * Math.Pow(10, -6), 0)).ToString();

                    //Turns on local axis
                    
                    lc.IsDisplayed = true;
                    checkBoxShowStresses.Checked = true;
                    checkBoxShowCrackWidth.Checked = true;
                    ShowStressResults(true);


                }

                ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                RhinoApp.WriteLine("Time elapsed:" + elapsedMs.ToString() + "ms");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //}

            


        }

        //Calculates stresses in the cross section for given loading
        private void buttonCalc2_Click(object sender, EventArgs e)
        {
            if (UpdateFreeFormClimateConditions())
                CalculateStresses();
            else
                MessageBox.Show("Not all Condition factors are given.");
            
        }

        private void radiobuttonSteel_checkChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateResultDisplay();
                _projectPlugIn.CurrentBeam.CrossSec.MaterialResultShown = radioButtonSteel.Checked ?
                    MaterialType.Steel : MaterialType.Concrete;
                ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
            }
            catch
            {

            }

        }

        private void leaveControl(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                tabControlMain.Focus();

        }

        private void _Click(object sender, EventArgs e)
        {

        }

        private void button_OpenRFEM_Click(object sender, EventArgs e)
        {
            if (_projectPlugIn.RFEMForm == null)
            {
                _projectPlugIn.RFEMForm = new RFEMAnalysisForm();

            }
            if (_projectPlugIn.ClimateForm == null)
            {
                _projectPlugIn.ClimateForm = new ClimateConditionsForm();
            }
            if (_projectPlugIn.ChooseColForm == null)
            {
                _projectPlugIn.ChooseColForm = new ChooseColumnsForm();
            }

            _projectPlugIn.RFEMForm.Show();
        }

        private void dataGridView_Geometry_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridView d = (DataGridView)sender;

            if (e.KeyData == Keys.Delete)
            {
                int row = d.SelectedCells[0].RowIndex;
                string material = (string)d.Rows[row].Cells[1].Value;
                int no = (int)d.Rows[row].Cells[0].Value;
                if (material == "Reinforcement")
                {
                    RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
                    foreach (RhinoObject rhinoObject in objs)
                    {
                        Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                        Reinforcement tempReinf = list.Find(typeof(Reinforcement)) as Reinforcement;
                        if (tempReinf.Id == no)
                        {
                            list.Remove(tempReinf);
                            rhinoObject.Attributes.SetUserString("infType", null);
                            _projectPlugIn.CurrentBeam.CrossSec.ReinforementIds.Remove(no);

                            d.Rows.Remove(d.Rows[row]);
                            rhinoObject.Attributes.LayerIndex = 0;
                            rhinoObject.CommitChanges();
                        }
                        
                    }
                }
                else
                {
                    RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);
                    foreach (RhinoObject rhinoObject in objs)
                    {
                        

                        Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                        GeometryLarge tempGeom = list.Find(typeof(GeometryLarge)) as GeometryLarge;
                        if (tempGeom.Id == no)
                        {
                            Layer l = ProjectPlugIn.Instance.ActiveDoc.Layers[rhinoObject.Attributes.LayerIndex];

                            l.IsLocked = false;
                            l.CommitChanges();
                            ProjectPlugIn.Instance.ActiveDoc.Objects.Delete(rhinoObject, true);
                            l.IsLocked = true;
                            l.CommitChanges();


                            list.Remove(tempGeom);
                            _projectPlugIn.CurrentBeam.CrossSec.GeometryLargeIds.Remove(no);
                            d.Rows.Remove(d.Rows[row]);
                        }
                    }
                }



            }

            labelConcreteCover.Text = Math.Round(_projectPlugIn.CurrentBeam.CrossSec.ConcreteCover / 
                _projectPlugIn.Unitfactor, 0).ToString();
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private void OnlyHandleDigits(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!Char.IsDigit(ch) && ch != 8)
            {
                e.Handled = true;
            }
        }

        private void buttonAddSeries_Click(object sender, EventArgs e)
        {
            chartSeriesName = "Strength" + chartSeriesNumb.ToString();
            chartRectMz.Series.Add(chartSeriesName);
            chartRectMz.Series[chartSeriesName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

            chartRectMy.Series.Add(chartSeriesName);
            chartRectMy.Series[chartSeriesName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

            chartSeriesNumb += 1;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonCalcMN_Click(object sender, EventArgs e)
        {
            try
            {
                LoadCase lc;
                if (_projectPlugIn.CurrentBeam.LoadCases.TrueForAll(o => o.Name != "SampleLoadCase"))
                {
                    MessageBox.Show("The load case has not been calculated yet. Please press: calculate stresses button first.");
                }
                else
                {
                    int i = _projectPlugIn.CurrentBeam.LoadCases.FindIndex(o => o.Name == "SampleLoadCase");
                    lc = _projectPlugIn.CurrentBeam.LoadCases[i];

                    Plane calcPlane = lc.LoadPlane;
                    Polyline p = _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(calcPlane, GetLimitState());
                    SetStrengthChartCurve(chartFreeMz, Moment.Mz, p);
                }



            }
            catch
            {
                MessageBox.Show("One of the force or moment values was not given in right format.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TestCalculations.TestCalculation1();
        }

        private void dataGridView_Reinforcement_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView_Geometry_CellEnter(object sender, DataGridViewCellEventArgs e)
        {

            DataGridView d = (DataGridView)sender;
            if (d.SelectedCells.Count == 0)
                return;
            int row = d.SelectedCells[0].RowIndex;
            string material = (string)d.Rows[row].Cells[1].Value;
            int no = (int)d.Rows[row].Cells[0].Value;
            if (material == "Reinforcement")
            {
                RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
                foreach (RhinoObject rhinoObject in objs)
                {
                    Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                    Reinforcement tempReinf = list.Find(typeof(Reinforcement)) as Reinforcement;
                    if (tempReinf.Id == no)
                    {
                        tempReinf.Selected = true;
                        ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
                        return;
                    }
                }
            }
            else
            {
                RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);
                foreach (RhinoObject rhinoObject in objs)
                {
                    Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                    GeometryLarge tempGeom = list.Find(typeof(GeometryLarge)) as GeometryLarge;
                    if (tempGeom.Id == no)
                    {
                        tempGeom.Selected = true;
                        ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
                        return;
                    }
                }
            }
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private void dataGridView_Geometry_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            
            DataGridView d = (DataGridView)sender;
            if (d.SelectedCells.Count == 0)
                return;
            int row = d.SelectedCells[0].RowIndex;
            string material = (string)d.Rows[row].Cells[1].Value;
            int no = (int)d.Rows[row].Cells[0].Value;
            if (material == "Reinforcement")
            {
                RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
                foreach (RhinoObject rhinoObject in objs)
                {
                    Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                    Reinforcement tempReinf = list.Find(typeof(Reinforcement)) as Reinforcement;
                    if (tempReinf.Id == no)
                    {
                        tempReinf.Selected = false;
                        ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
                        return;
                    }
                }
            }
            else
            {
                RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);
                foreach (RhinoObject rhinoObject in objs)
                {
                    Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                    GeometryLarge tempGeom = list.Find(typeof(GeometryLarge)) as GeometryLarge;
                    if (tempGeom.Id == no)
                    {
                        tempGeom.Selected = false;
                        ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
                        return;
                    }
                }
            }
            
        }

        private void dataGridView_Geometry_Leave(object sender, EventArgs e)
        {
            DataGridView d = (DataGridView)sender;
            d.ClearSelection();
        }

        private void dataGridView_Reinforcement_Update_diameter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView d = (DataGridView)sender;
            if (e.ColumnIndex == 2)
            {
                Reinforcement r = _projectPlugIn.CurrentBeam.CrossSec.GetOneReinforcement((int)d.Rows[e.RowIndex].Cells[0].Value);
                if (r != null)

                    
                    r.Diameter = int.Parse(d.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString())*Math.Pow(10,-3);
            }
            
        }

        private void dataGridViewInt_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView d = (DataGridView)sender;
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
            if (d.CurrentCell.ColumnIndex == 2) //Desired Column
            {
                if (e.Control is TextBox tb)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                }
            }
        }

        private void Column1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        

        private void buttonShowNominalStiffnessChart_Click(object sender, EventArgs e)
        {
            if (_projectPlugIn.ChForm == null)
            {
                _projectPlugIn.ChForm = new ChartForm();
            }
            _projectPlugIn.ChForm.Show();
        }

        public void ChangeToStartView()
        {
            tabControlMain.SelectedTab = tabPageMain;
            listBoxCrossSecs.Items.Clear();
            foreach (Beam beam in _projectPlugIn.Beams)
            {
                listBoxCrossSecs.Items.Add(beam.Name);
            }
        }

        private void checkBoxShowResults_CheckedChanged(object sender, EventArgs e)
        {
            ShowStressResults(checkBoxShowStresses.Checked);
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private void ShowStressResults(bool show)
        {
            _projectPlugIn.ResultConduit.Enabled = show;
            _projectPlugIn.LocalAxisConduit.Enabled = show;
            _projectPlugIn.ColorScaleDisplay.Enabled = show;
        }

        private void checkBoxShowCrackWidth_CheckedChanged(object sender, EventArgs e)
        {
            _projectPlugIn.CrackWidthConduit.Enabled = checkBoxShowCrackWidth.Checked;
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private bool UpdateFreeFormClimateConditions()
        {
            if (int.TryParse(textBoxFreeT.Text, out int t) &&
                int.TryParse(textBoxFreeT0.Text, out int t0))
            {
                int rh = int.Parse(comboBoxFreeRH.SelectedItem.ToString().TrimEnd('%'));
                _projectPlugIn.CurrentBeam.ClimateCond = new ClimateCondition(rh, t0, t,  _projectPlugIn.CurrentBeam);

                labelCreepCoefficient.Text = Math.Round(_projectPlugIn.CurrentBeam.ClimateCond.CreepCoefficient, 2).ToString();
                return true;
            }
            return false;
        }

        private void FreeFormClimateConditionVariableChanged(object sender, EventArgs e)
        {
            UpdateFreeFormClimateConditions();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                ProjectPlugIn.Instance.ActiveDoc.Objects.Delete(rhinoObject,true);
            }

            objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                ProjectPlugIn.Instance.ActiveDoc.Objects.Delete(rhinoObject, true);
            }

        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            //Connect to excel
            Excel.Application oXL;
            Excel.Workbook oWB;
            Excel.Worksheet oSheet;
            try
            {
                //Start Excel and get Application object.
                try
                {
                    oXL = (Excel.Application)
                        System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                }
                catch
                {
                    oXL = new Excel.Application();
                }

                LimitState ls = LimitState.Ultimate;


                oXL.Visible = true;
                //Get a new workbook.
                oWB = oXL.Workbooks.Add();
                oXL.ScreenUpdating = false;
                oSheet = (Excel.Worksheet)oWB.ActiveSheet;
                oSheet.Name = "Cross section results";
                if (_projectPlugIn.CurrentBeam.CurrentLoadCase.GetType() == typeof(SimpleLoadCase))
                    ((SimpleLoadCase)_projectPlugIn.CurrentBeam.CurrentLoadCase).CrackWidthCalc.ExportToExcel(oSheet ,oSheet.Range["A1"]);
                oXL.ScreenUpdating = true;
            }

            catch (Exception theException)
            {
                string errorMessage;
                errorMessage = "Error: ";
                errorMessage = string.Concat(errorMessage, theException.Message);
                errorMessage = string.Concat(errorMessage, " Line: ");
                errorMessage = string.Concat(errorMessage, theException.Source);
                MessageBox.Show(errorMessage, "Error");

            }
            
        }

        private void buttonOpenHeatFlow_Click(object sender, EventArgs e)
        {
            if (_projectPlugIn.HeatFlowForm == null)
                _projectPlugIn.HeatFlowForm = new HeatFlowForm();
            _projectPlugIn.HeatFlowForm.Show();
        }
    }

}
