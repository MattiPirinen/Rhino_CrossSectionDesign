using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace CrossSectionDesign
{
    [System.Runtime.InteropServices.Guid("0b0c3a6e-7efb-47c9-b2e3-7d92788a9f74")]
    public partial class MainPanel : UserControl
    {
        private readonly object[] _concreteStrengthClasses = {
            "C20/25",
            "C25/30",
            "C30/37",
            "C35/45",
            "C40/50",
            "C45/55",
            "C50/60",
            "C55/65",
            "C60/70",
            "C70/80",
            "C80/90",
            "C90/100",
        };

        private readonly object[] _steelDiameters =
        {
            "6",
            "8",
            "10",
            "12",
            "16",
            "20",
            "25",
            "32"
        };

        private readonly object[] _reinfStrengthClasses =
        {
            "B500B"
        };

        private readonly object[] _steelStrengthClasses =
        {
            "S235",
            "S355"
        };

        private DivisionCondiot _divisionDoncuit;
        private BackGroundConduit _backGroundConduit;
        private ResultConduit _resultConduit;
        private CrossSection _currentCrossSection;

        
        public MainPanel()
        {


            InitializeComponent();
            //TODO add a correct material assignment

            InitializeComponentValues();
            _currentCrossSection = new CrossSection(new ConcreteMaterial("C30/37"));
            _divisionDoncuit = new DivisionCondiot() {Enabled = true};
            _backGroundConduit = new BackGroundConduit() {Enabled = true};
            _resultConduit = new ResultConduit() {Enabled = false};

            RhinoDoc.ActiveDoc.Views.Redraw();
            // Set the user control property on our plug-in
            ProjectPlugIn.Instance.UserControl = this;

            // Create a visible changed event handler
            this.VisibleChanged += new EventHandler(UserControl1_VisibleChanged);

            // Create a dispose event handler
            this.Disposed += new EventHandler(UserControl1_Disposed);


        }

        private void InitializeComponentValues()
        {
            comboBoxMaterialGeom.Items.AddRange(_concreteStrengthClasses);
            comboBoxMaterialGeom.SelectedItem = "C30/37";

            comboBoxMaterialReinf.Items.AddRange(_reinfStrengthClasses);
            comboBoxMaterialReinf.SelectedItem = "B500B";

            foreach (var item in Enum.GetValues(typeof(MaterialType)))
            {
                comboBoxMaterialType.Items.Add(item);
            }

            comboBoxMaterialType.SelectedIndex = 0;

            comboBoxDiam.Items.AddRange(_steelDiameters);
            comboBoxDiam.SelectedItem = "16";

            textBoxMoment.Text = "100";
            textBoxForce.Text = "-1000";

        }


        void UserControl1_VisibleChanged(object sender, EventArgs e)
        {

        }


        void UserControl1_Disposed(object sender, EventArgs e)
        {
            // Clear the user control property on our plug-in
            ProjectPlugIn.Instance.UserControl = null;
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

        private void AddSteelToDisplay()
        {
            
            RhinoObject[] objects1 = RhinoDoc.ActiveDoc.Objects.FindByUserString("Name", "Reinforcement", true);
            RhinoObject[] objects2 = RhinoDoc.ActiveDoc.Objects.FindByUserString("Name", "Steel", true);
            RhinoObject[] objects = new RhinoObject[objects1.Length + objects2.Length];
            Array.Copy(objects1,objects,objects1.Length);
            Array.Copy(objects2,0,objects,objects1.Length,objects2.Length);

            _backGroundConduit.DisplayBrepSteel.Clear();
            foreach (RhinoObject o in objects)
            {
                Rhino.DocObjects.Custom.UserDataList list = o.Attributes.UserData;

                if (list.Find(typeof(Reinforcement)) is Reinforcement reinf)
                    _backGroundConduit.DisplayBrepSteel.Add(reinf.BrepGeometry);
                

                if (list.Find(typeof(GeometryLarge)) is GeometryLarge larg)
                    _backGroundConduit.DisplayBrepSteel.Add(larg.BaseBrep);

            }


            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        private void AddConcreteToDisplay()
        {
            RhinoObject[] objects = RhinoDoc.ActiveDoc.Objects.FindByUserString("Name", "Concrete", true);

            _backGroundConduit.DisplayBrepConcrete.Clear();
            foreach (RhinoObject o in objects)
            {
                Rhino.DocObjects.Custom.UserDataList list = o.Attributes.UserData;

                GeometryLarge geoSeg = list.Find(typeof(GeometryLarge)) as GeometryLarge;

                if (geoSeg != null)
                {
                    _backGroundConduit.DisplayBrepConcrete.Add(geoSeg.BaseBrep);
                }
            }

            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        private void buttonAddConcreteGeometry_Click(object sender, EventArgs e)
        {
            _currentCrossSection.GeometryLarges.Add(CreateGeometryLarge.CreateGeometry(
                (MaterialType) Enum.Parse(typeof(MaterialType), comboBoxMaterialType.SelectedItem.ToString()),
                comboBoxMaterialGeom.SelectedItem.ToString()));
            MaterialType type =
                (MaterialType) Enum.Parse(typeof(MaterialType), comboBoxMaterialType.SelectedItem.ToString());
            if (type == MaterialType.Concrete)
                AddConcreteToDisplay();
            else
                AddSteelToDisplay();
        }

        private void buttonAddReinf_Click_1(object sender, EventArgs e)
        {

            _currentCrossSection.AddGeometry(CreateReinforcement.CreateReinforcements(
                comboBoxMaterialReinf.SelectedItem.ToString(),
                int.Parse(comboBoxDiam.SelectedItem.ToString())));
            AddSteelToDisplay();
        }

        private void buttonCalculate_Click_1(object sender, EventArgs e)
        {
            _currentCrossSection.CalculateStrengthCurve(Axis.XAxis);
            tabControlMain.SelectedTab = tabPageResults;
            chartResults.Series["Strength"].Points.Clear();
            foreach (Tuple<double, double> point in _currentCrossSection.Strength)
            {
                chartResults.Series["Strength"].Points.AddXY(point.Item2*Math.Pow(10,-3), point.Item1 * Math.Pow(10, -3));
            }

            SetAxisIntervalAndMax();

        }
        // This method sets x and y axis interval and max values according to the strength curve max values
        private void SetAxisIntervalAndMax()
        {
            //X-axis min max and interval
            double maxValue = _currentCrossSection.Strength.Max(x => x.Item2)*Math.Pow(10,-3);
            double minValue = _currentCrossSection.Strength.Min(x => x.Item2) * Math.Pow(10, -3);
            Tuple<double, double,double> minMaxInterval = CreateInterval(maxValue,minValue);
            chartResults.ChartAreas[0].AxisX.Minimum = minMaxInterval.Item1;
            chartResults.ChartAreas[0].AxisX.Maximum = minMaxInterval.Item2;
            chartResults.ChartAreas[0].AxisX.MajorGrid.Interval = minMaxInterval.Item3 ;
            chartResults.ChartAreas[0].AxisX.MinorGrid.Interval =  minMaxInterval.Item3/5;
            chartResults.ChartAreas[0].AxisX.LabelStyle.Interval = minMaxInterval.Item3;
            chartResults.ChartAreas[0].AxisX.MajorTickMark.Interval = minMaxInterval.Item3;


            //Y-axis min, max and interval 
            maxValue = _currentCrossSection.Strength.Max(x => x.Item1)*Math.Pow(10,-3);
            minValue = _currentCrossSection.Strength.Min(x => x.Item1)*Math.Pow(10,-3);
            minMaxInterval = CreateInterval(maxValue,minValue);
            chartResults.ChartAreas[0].AxisY.Minimum = minMaxInterval.Item1;
            chartResults.ChartAreas[0].AxisY.Maximum = minMaxInterval.Item2;
            chartResults.ChartAreas[0].AxisY.MajorGrid.Interval = minMaxInterval.Item3;
            chartResults.ChartAreas[0].AxisY.MinorGrid.Interval = minMaxInterval.Item3 / 5 ;
            chartResults.ChartAreas[0].AxisY.LabelStyle.Interval = minMaxInterval.Item3 ;
            chartResults.ChartAreas[0].AxisY.MajorTickMark.Interval = minMaxInterval.Item3;
        }

        private void checkBoxShowDivision_CheckedChanged_1(object sender, EventArgs e)
        {
            RhinoObject[] objects = RhinoDoc.ActiveDoc.Objects.FindByUserString("Name", "Concrete", true);

            _divisionDoncuit.DisplayBreps.Clear();
            foreach (RhinoObject o in objects)
            {
                Rhino.DocObjects.Custom.UserDataList list = o.Attributes.UserData;

                GeometryLarge geom = list.Find(typeof(GeometryLarge)) as GeometryLarge;

                if (geom != null)
                {
                    _divisionDoncuit.DisplayBreps.Add(geom.BaseBrep);
                }
            }


            switch (checkBoxShowDivision.CheckState)
            {
                case CheckState.Checked:
                    _divisionDoncuit.Enabled = true;
                    _backGroundConduit.Enabled = false;
                    break;
                case CheckState.Unchecked:
                    _divisionDoncuit.Enabled = false;
                    _backGroundConduit.Enabled = true;
                    break;
                case CheckState.Indeterminate:
                    break;
                default:
                    break;
            }
            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        //This method creates an interval for chart plotting. It returns axis min value max value and interval 
        private Tuple<double, double,double> CreateInterval(double maxValue,double minValue)
        {

            char firstNumMin = '0';
            char secondNumMin = '0';

            if (minValue > 0 || Math.Abs(minValue / maxValue) <Math.Pow(10,-5))
                minValue = 0;
            else
            {
                firstNumMin = minValue.ToString()[1];
                secondNumMin = minValue.ToString().Length > 1 ? minValue.ToString()[2] : '0';
            }

            char firstNumMax = maxValue.ToString()[0];
            char secondNumMax = maxValue.ToString()[1];
            double numbMax = Math.Floor(Math.Log10(maxValue));
            double numbMin = Math.Floor(Math.Log10(Math.Abs(minValue)));
            double numb = Math.Floor(Math.Log10(maxValue-minValue));
            
            char firstNum = (maxValue - minValue).ToString()[0];

            double axisMax = 0;
            double axisMin = 0;
            double interval = 0;

            if (firstNum == '9' || firstNum == '8' || firstNum == '7')
            {
                axisMax = (char.GetNumericValue(firstNumMax) + 1)*Math.Pow(10, numbMax);
                if (minValue == 0) axisMin = 0;
                else axisMin = -(char.GetNumericValue(firstNumMin) +1)*Math.Pow(10, numbMin);
                interval = 2*Math.Pow(10, numb);
            }

            else if (firstNum == '6' || firstNum == '5' || firstNum == '4')
            {
                
                axisMax = (char.GetNumericValue(firstNumMax) + 1) * Math.Pow(10, numbMax);
                    
                if (minValue == 0) axisMin = 0;
                else axisMin = -(char.GetNumericValue(firstNumMin) + 1) * Math.Pow(10, numbMin);
                interval = Math.Pow(10, numb);
            }
            else
            {
                
                double firstTwoMax = Math.Floor(double.Parse(char.ToString(firstNumMax) + char.ToString(secondNumMax)) / 5) * 5;
                if (numbMax < numb)
                    axisMax = (firstTwoMax + 50) * Math.Pow(10, numbMax - 1);
                else
                    axisMax = (firstTwoMax + 5) * Math.Pow(10, numbMax - 1);


                double firstTwoMin = Math.Floor(double.Parse(char.ToString(firstNumMin) + char.ToString(secondNumMin)) / 5) * 5;

                if (minValue == 0) axisMin = 0;



                else
                {
                    if (numbMin < numb)
                        axisMin = -(firstTwoMin + 50) * Math.Pow(10, numbMin - 1);
                    else
                    {
                        axisMin = -(firstTwoMin + 5) * Math.Pow(10, numbMin - 1);
                    }
                }
                
                interval = 0.5 * Math.Pow(10, numb);
            }

            /*
            if (interval > 10)
            {
                interval = Math.Round(interval, 0);
                axisMax = Math.Round(axisMax, 0);
                axisMin = Math.Round(axisMin, 0);
            }
            else
            {
                interval = Math.Round(interval,Convert.ToInt32(Math.Abs(Math.Floor(Math.Log10(interval))))+1);
                axisMax = Math.Round(maxValue, Convert.ToInt32(Math.Abs(Math.Floor(Math.Log10(interval)))) + 1);
                axisMin = Math.Round(axisMin, Convert.ToInt32(Math.Abs(Math.Floor(Math.Log10(interval)))) + 1);
            }
            */
            return Tuple.Create(axisMin, axisMax, interval);
        }

        private void tabPageMain_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxMaterialType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Enum.Parse(typeof(MaterialType),comboBoxMaterialType.SelectedItem.ToString()))
            {
                case MaterialType.Concrete:
                    comboBoxMaterialGeom.Items.Clear();
                    comboBoxMaterialGeom.Items.AddRange(_concreteStrengthClasses);
                    comboBoxMaterialGeom.SelectedItem = "C30/37";
                    break;
                case MaterialType.Steel:
                    comboBoxMaterialGeom.Items.Clear();
                    comboBoxMaterialGeom.Items.AddRange(_steelStrengthClasses);
                    comboBoxMaterialGeom.SelectedItem = "S355";
                    break;
                default:
                    break;
                    
            }
        }

        private void buttonCalc2_Click(object sender, EventArgs e)
        {
            _currentCrossSection.CalculateStrains(double.Parse(textBoxForce.Text)*1000, double.Parse(textBoxMoment.Text) * 1000,Axis.XAxis);
            _resultConduit.Breps = CreateResultDisplay();
            _resultConduit.Enabled = true;

        }

        private List<Tuple<Brep, double>> CreateResultDisplay()
        {
            Tuple<double, double> minAndMax = calcMaxAndMin();


            double maxLength = _currentCrossSection.CrossSectionbb.Diagonal.Length*0.7;
            List<Tuple<Brep,double>> tempList = new List<Tuple<Brep, double>>();
            foreach (IBrepGeometry brepGeometry in _currentCrossSection.GeometryList)
            {
                if (brepGeometry.GetType() == typeof(GeometrySegment))
                {
                    double height = brepGeometry.Stress / (minAndMax.Item2 - minAndMax.Item1) * maxLength;

                    tempList.Add(Tuple.Create(Brep.CreateFromOffsetFace(brepGeometry.BrepGeometry.Faces[0], height,
                        RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false, true), brepGeometry.Stress));
                }

            }

            return tempList;
        }


        private Tuple<double, double> calcMaxAndMin()
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;

            foreach (IBrepGeometry brepGeometry in _currentCrossSection.GeometryList)
            {
                if (brepGeometry.GetType() == typeof(GeometrySegment))
                {
                    maxValue = Math.Max(maxValue, brepGeometry.Stress);
                    minValue = Math.Min(minValue, brepGeometry.Stress);
                }

            }

            return Tuple.Create(minValue, maxValue);

        }


    }
}
