using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Interfaces;
using CrossSectionDesign.Static_classes;
using MoreLinq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Numerics = MathNet.Numerics.LinearAlgebra;

namespace CrossSectionDesign
{
    public partial class HeatFlowForm : Form
    {
        private ProjectPlugIn _projectPlugIn;
        private BackgroundWorker bw = new BackgroundWorker();
        private double _stepSize;
        private double _endTime;
        private List<InspectionPoint> ips = new List<InspectionPoint>();

        private void backgroundWorker_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarSimulation.Value = e.ProgressPercentage;
            _projectPlugIn.ActiveDoc.Views.Redraw();


            Beam b = ProjectPlugIn.Instance.CurrentBeam;
            CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;
            List<ICalcGeometry> calcGeometries = new List<ICalcGeometry>();

            if (_projectPlugIn.CurrentBeam.CrossSec.CalcMesh == null)
                return;
            CalcMesh cm = _projectPlugIn.CurrentBeam.CrossSec.CalcMesh;


            Tuple<bool, double> val = (Tuple<bool, double>)e.UserState;

            cm.CalculateVertexTemperatures(val.Item2);

            foreach (InspectionPoint ip in ips)
            {
                if (ip.Results.Count != 0)
                    chartTemp.Series[ip.Id.ToString()].Points.Add(new DataPoint(ip.Results[ip.Results.Count - 1].X, ip.Results[ip.Results.Count - 1].Y));
            }

            DataPoint dp = new DataPoint(val.Item2, BoarderEdge.StandardFireTemp(val.Item2));
            chartTemp.Series["Fire"].Points.Add(dp);
            /*
            if (val.Item1)
            {
                SetReinforcementTemperatures();
                Series s = CreateNewSeries(chartStrength, "R"+((int)val.Item2 / 60).ToString());
                Polyline pl = 
                    _projectPlugIn.CurrentBeam.CrossSec.CalculateFireStrengthCurve(Plane.WorldXY, LimitState.Ultimate);
                Moment m = Moment.Mz;
                foreach (Point3d point in pl)
                {
                    
                    s.Points.AddXY((m == Moment.Mz || m == Moment.MComb ? point.Z : point.Y) * Math.Pow(10, -3), point.X * Math.Pow(10, -3));
                    if (m == Moment.MComb)
                        s.Points.AddXY(point.Y * Math.Pow(10, -3), point.X * Math.Pow(10, -3));

                }
                ChartManipulationTools.SetAxisIntervalAndMax(chartStrength);
            }
            */
        }

        private void backGroundWorker_workCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBarSimulation.Value = 0;
        }

        private async void StartHeatFlowCalculation(object sender, DoWorkEventArgs e)
        {
            double stepSize = _stepSize;
            double endTime = _endTime;

            if (_projectPlugIn.CurrentBeam.CrossSec.CalcMesh == null)
                return;


            CalcMesh cm = _projectPlugIn.CurrentBeam.CrossSec.CalcMesh;
            cm.MeshSegments.ForEach(o => o.HeatQuantity = 0);
            for (int s = 0; s < cm.FaceHeats.Count; s++)
            {
                cm.FaceHeats[s] = 0;
            }
            int displayFrequency = 500;
            double progressStep = stepSize* displayFrequency;
            double totProgress = 0;
            int progress = 0;

            List<double> calcPoints = new List<double> { 0, 1800, 3600, 5400, 7200 };
            int u = 0;

            for (double i = 0; i < endTime; i += stepSize)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (i > totProgress + progressStep)
                {
                    progress = (int)(i*1.0 / endTime * 100);
                    totProgress += progressStep;

                    bw.ReportProgress(progress, Tuple.Create(false, i));
                }
                if (u < calcPoints.Count && i > calcPoints[u])
                {
                    progress = (int)(i * 1.0 / endTime * 100);
                    totProgress += progressStep;
                    bw.ReportProgress(progress, Tuple.Create(true, i));
                    u++;
                }

                cm.CalculateNewTemperatures2(stepSize, i);
            }
        }

        private void InitializeCalcValues(double stepSize)
        {
            chartStrength.Series.Clear();
            chartTemp.Series.Clear();
            CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;
            ips = cs.GetInspectionPoints();
            string name;
            foreach (InspectionPoint inspection in ips)
            {
                inspection.Results.Clear();
                name = inspection.Id.ToString();
                chartTemp.Series.Add(name);
                chartTemp.Series[name].ChartType = SeriesChartType.Line;
                chartTemp.Series[name].BorderWidth = 2;
            }

            name = "Fire";
            chartTemp.Series.Add(name);
            chartTemp.Series[name].ChartType = SeriesChartType.Line;
            chartTemp.Series[name].BorderWidth = 2;


            double heatFlowConvection =0.2* 25 * (500) * stepSize;
            double heatFlowRadiation =0.2* 5.670367 * Math.Pow(10, -8) * 0.8* (Math.Pow((800 + 273), 4) -
                Math.Pow(300+273, 4)) * stepSize;
            double heatFlow = heatFlowConvection + heatFlowRadiation;

            //cs.CalcMesh.CalculateNewTemperatures2(stepSize, 0);
            cs.CalcMesh.HeatFlowFactor = (cs.CalcMesh.GetBoundingBox(false).Diagonal.Length*1000 / 6)/ heatFlow;


        }

        private Series CreateNewSeries(Chart c, string name)
        {
            Series s = c.Series.Add(name);
            c.Series[name].ChartType = SeriesChartType.Line;
            c.Series[name].BorderWidth = 2;
            return s;
        }


        public HeatFlowForm()
        {
            _projectPlugIn = ProjectPlugIn.Instance;
            InitializeComponent();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += backgroundWorker_progressChanged;
            bw.RunWorkerCompleted += backGroundWorker_workCompleted;
            bw.DoWork += StartHeatFlowCalculation;

            InitializeComponentValues();

        }

        private void InitializeComponentValues()
        {
            textBoxStepSize.Text = "0.5";
            textBoxEndTime.Text = "7201";
            textBoxSurfaceTemp.Text = "200";
        }

        private void buttonAddConstraint_Click(object sender, EventArgs e)
        {
            /*
            if (!double.TryParse(textBoxSurfaceTemp.Text, out double surfTemp))
            {
                MessageBox.Show("Add the surface temperature first.");
                return;
            }
              */  

            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.SetCommandPrompt("Start of line");
            gp.Get();
            if (gp.CommandResult() != Rhino.Commands.Result.Success)
                return;

            Point3d pt_start = gp.Point();

            gp.SetCommandPrompt("End of line");
            gp.SetBasePoint(pt_start, false);
            
            gp.DrawLineFromPoint(pt_start, true);
            gp.Get();
            if (gp.CommandResult() != Rhino.Commands.Result.Success)
                return;

            Point3d pt_end = gp.Point();
            Vector3d v = pt_end - pt_start;
            if (v.IsTiny(Rhino.RhinoMath.ZeroTolerance))
                return;

            Line l = new Line(pt_start, pt_end);
            Transform inTr = _projectPlugIn.CurrentBeam.CrossSec.InverseUnitTransform;
            l.Transform(_projectPlugIn.CurrentBeam.CrossSec.UnitTransform);
            List<GeometryLarge> glList = _projectPlugIn.CurrentBeam.CrossSec.GetGeometryLarges();
            if (glList.Count != 1 || glList[0].Material.GetType() != typeof(ConcreteMaterial))
                return;

            foreach (BoarderEdge boarderEdge in glList[0].CalcMesh.BoarderEdges)
            {
                double distance = l.DistanceTo(boarderEdge.Centroid, true);
                if (distance < 0.001)
                {
                    boarderEdge.IsConductive = true;
                }
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBoxStepSize.Text, out double stepSize) &&
                double.TryParse(textBoxEndTime.Text, out double endTime))
            {

                _stepSize = stepSize;
                _endTime = endTime;
                _projectPlugIn.HeatFlowConduit.Enabled = true;

                //******** Set color scale **********
                

                _projectPlugIn.ColorScaleDisplay.Enabled = true;
                _projectPlugIn.ColorScaleDisplay.SetColorScale(0,
                    1200, 0, 0.7, "Temp [C]");

                //******** Start Calculation **********
                /*
                Mesh m = new Mesh();
                m.Append(_projectPlugIn.CurrentBeam.CrossSec.CalcMesh);
                m.Transform(_projectPlugIn.CurrentBeam.CrossSec.InverseUnitTransform);
                _projectPlugIn.ActiveDoc.Objects.AddMesh(m);
                */
                if (!bw.IsBusy)
                {
                    InitializeCalcValues(stepSize);
                    bw.RunWorkerAsync();
                }
                    
            }
            else
                MessageBox.Show("Define step size and end time First");
            
        }

        private void modifyResultMesh(GeometryLarge gl)
        {
            List<ICalcGeometry> geoms = new List<ICalcGeometry>();
            geoms.AddRange(gl.CalcMesh.MeshSegments);
            List<ICalcGeometry> breps = new List<ICalcGeometry>();
            double absMax = 100;

            double maxLength = _projectPlugIn.CurrentBeam.CrossSec.GetBoundingBox(Plane.WorldXY).Diagonal.Length * 0.5;
            List<Tuple<Brep, double>> tempList = new List<Tuple<Brep, double>>();



            foreach (ICalcGeometry geom in geoms)
            {
                //double height = geom.Stresses[lc] / absMax * maxLength;
                //geom.ModifyMesh(height);
                geom.ModifyMesh(1);

            }
        }

        private void buttonTestCond_Click(object sender, EventArgs e)
        {


            List<Tuple<double, double>> temps = TestCalculations.HeatConductionTest(0.005);
            if (chartTemp.Series.IndexOf("SimpleHeat") == -1)
                chartTemp.Series.Add("SimpleHeat");
            chartTemp.Series["SimpleHeat"].ChartType = SeriesChartType.Line;
            chartTemp.Series["SimpleHeat"].MarkerBorderWidth = 2;
            foreach (Tuple<double, double> item in temps)
            {
                chartTemp.Series["SimpleHeat"].Points.AddXY(item.Item1, item.Item2);
            }

            temps = TestCalculations.HeatConductionTest(0.01);
            if (chartTemp.Series.IndexOf("SimpleHeat1") == -1)
                chartTemp.Series.Add("SimpleHeat1");
            chartTemp.Series["SimpleHeat1"].ChartType = SeriesChartType.Line;
            chartTemp.Series["SimpleHeat1"].MarkerBorderWidth = 2;
            foreach (Tuple<double, double> item in temps)
            {
                chartTemp.Series["SimpleHeat1"].Points.AddXY(item.Item1, item.Item2);
            }

            temps = TestCalculations.HeatConductionTest(0.02);
            if (chartTemp.Series.IndexOf("SimpleHeat2") == -1)
                chartTemp.Series.Add("SimpleHeat2");
            chartTemp.Series["SimpleHeat2"].ChartType = SeriesChartType.Line;
            chartTemp.Series["SimpleHeat2"].MarkerBorderWidth = 2;
            Polyline pl = new Polyline();
            foreach (Tuple<double, double> item in temps)
            {
                pl.Add(new Point3d(item.Item2 * Math.Pow(10, 3), item.Item1 * Math.Pow(10, 3), 0));
                chartTemp.Series["SimpleHeat2"].Points.AddXY(item.Item1, item.Item2);
            }



            ChartManipulationTools.SetAxisIntervalAndMax(chartTemp, pl, Moment.My);

        }

        private void SetReinforcementTemperatures()
        {
            List<Reinforcement> rs = _projectPlugIn.CurrentBeam.CrossSec.GetReinforcements();
            List<GeometryLarge> gls = _projectPlugIn.CurrentBeam.CrossSec.GetGeometryLarges();
            if (gls.Count != 1)
                return;
            CalcMesh m = gls[0].CalcMesh;
            foreach (Reinforcement r in rs)
            {
                Line l = new Line(r.Centroid + Vector3d.ZAxis * -10000, Vector3d.ZAxis * 20000);

                Point3d[] pts = Intersection.MeshLine(m.ResultMesh, l, out int[] temp1);
                if (pts.Length != 1)
                    continue;
                else
                    r.Temperature =  pts[0].Z;
            }
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
            _projectPlugIn.HeatFlowConduit.Enabled = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void int_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verify that the pressed key isn't CTRL or any non-numeric digit
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != '\b'))
            {
                e.Handled = true;
            }

            // If you want, you can allow decimal (float) numbers
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        public void AddInspectionPoint(Point3d p)
        {
            if (_projectPlugIn.CurrentBeam.CrossSec.CalcMesh == null)
                return;

            CalcMesh cm = _projectPlugIn.CurrentBeam.CrossSec.CalcMesh;

            InspectionPoint ip = new InspectionPoint(p, _projectPlugIn.CurrentBeam.CrossSec.AddingCentroid,
                _projectPlugIn.CurrentBeam.CrossSec);
            cm.InspectionPoints.Add(ip);
            
            ObjectAttributes attr = new ObjectAttributes();

            attr.SetUserString("infType", "InspectionPoint");
            GetLayerIndex(ref attr);

            attr.UserData.Add(ip);
            Guid guid = ProjectPlugIn.Instance.ActiveDoc.Objects.AddPoint(p);
            ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
        }

        private void buttonAddInspectionPoint_Click(object sender, EventArgs e)
        {
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.SetCommandPrompt("Select location if Inspection Point");
            gp.Get();
            if (gp.CommandResult() != Rhino.Commands.Result.Success)
                return;
            Point3d location = gp.Point();
            CalcMesh cm = _projectPlugIn.CurrentBeam.CrossSec.CalcMesh;
            if (cm == null)
                return;

            InspectionPoint ip = new InspectionPoint(location, _projectPlugIn.CurrentBeam.CrossSec.AddingCentroid,
                _projectPlugIn.CurrentBeam.CrossSec);

            cm.InspectionPoints.Add(ip);

            ObjectAttributes attr = new ObjectAttributes();

            attr.SetUserString("infType", "InspectionPoint");
            GetLayerIndex(ref attr);

            attr.UserData.Add(ip);
            Guid guid = ProjectPlugIn.Instance.ActiveDoc.Objects.AddPoint(location);
            ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
        }

        public static void GetLayerIndex(ref ObjectAttributes attr)
        {
            RhinoDoc doc = ProjectPlugIn.Instance.ActiveDoc;

            List<Layer> layers = (from layer in doc.Layers
                                  where layer.Name == "InspectionPoints"
                                  select layer).ToList();

            if (layers.Count == 0 || (layers.Count == 1 && layers[0].IsDeleted))
            {
                attr.LayerIndex = createLayer(doc);
            }
            else if (layers.Count == 1)
            {
                attr.LayerIndex = layers[0].LayerIndex;
                layers[0].CommitChanges();
            }
            else
            {
                RhinoApp.WriteLine("More than one layer with name Concrete excists. Remove one of them.");
            }

        }

        private static int createLayer(RhinoDoc doc)
        {
            int index;
            Color color = Color.FromArgb(0, 0, 0);
            doc.Layers.Add("InspectionPoints", color);
            index = doc.Layers.Find("InspectionPoints", true);
            
            Layer layer = doc.Layers[index];
            layer.IsLocked = false;
            layer.CommitChanges();
            return index;
        }

        private void checkBoxShowIpNumbers_CheckedChanged(object sender, EventArgs e)
        {
            ProjectPlugIn.Instance.InspectionPointConduit.Enabled = checkBoxShowIpNumbers.Checked;
            _projectPlugIn.ActiveDoc.Views.Redraw();
        }
    }
}
