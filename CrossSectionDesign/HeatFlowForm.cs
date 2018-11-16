using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Interfaces;
using CrossSectionDesign.Static_classes;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
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

namespace CrossSectionDesign
{
    public partial class HeatFlowForm : Form
    {
        private ProjectPlugIn _projectPlugIn;
        private BackgroundWorker bw = new BackgroundWorker();
        private int _stepSize;
        private int _endTime;
        private List<InspectionPoint> ips = new List<InspectionPoint>();


        private void backgroundWorker_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarSimulation.Value = e.ProgressPercentage;
            _projectPlugIn.ActiveDoc.Views.Redraw();
            
            foreach (InspectionPoint ip in ips)
            {
                if (ip.Results.Count != 0)
                    chartTemp.Series[ip.Id.ToString()].Points.Add(new DataPoint(ip.Results[ip.Results.Count - 1].X, ip.Results[ip.Results.Count - 1].Y));
            }
            
        }

        private void backGroundWorker_workCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBarSimulation.Value = 0;
        }


        private async void StartHeatFlowCalculation(object sender, DoWorkEventArgs e)
        {
            int stepSize = _stepSize;
            int endTime = _endTime;

            List<GeometryLarge> glList = _projectPlugIn.CurrentBeam.CrossSec.GetGeometryLarges();
            if (glList.Count != 1 || glList[0].Material.GetType() != typeof(ConcreteMaterial))
                return;
            glList[0].CalcMesh.MeshSegments.ForEach(o => o.Temperature = 0);

            int index = glList[0].CalcMesh.MidIndice;

            modifyResultMesh(glList[0]);
            List<double> temperatures = new List<double>();

            double progressStep = stepSize*500;
            double totProgress = 0;
            int progress = 0;
            for (int i = 0; i < endTime; i += stepSize)
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
                    bw.ReportProgress(progress);
                }

                temperatures.Clear();
                foreach (MeshSegment ms in glList[0].CalcMesh.MeshSegments)
                {
                    temperatures.Add(ms.CalculateNewTemperature(stepSize,i));
                }

                for (int k = 0; k < temperatures.Count; k++)
                {
                    glList[0].CalcMesh.MeshSegments[k].Temperature = temperatures[k];
                }
            }

        }

        private void InitializeCalcValues()
        {
            chartTemp.Series.Clear();
            CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;
            ips = cs.GetInspectionPoints();

            foreach (InspectionPoint inspection in ips)
            {
                inspection.Results.Clear();
                string name = inspection.Id.ToString();
                chartTemp.Series.Add(name);
                chartTemp.Series[name].ChartType = SeriesChartType.Line;
                chartTemp.Series[name].BorderWidth = 2;
            }



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
        }

        private void buttonAddConstraint_Click(object sender, EventArgs e)
        {
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

            foreach (BoarderNeighbor boarderNeighbor in glList[0].CalcMesh.boarderNeighbors)
            {
                double distance = l.DistanceTo(boarderNeighbor.Centroid, true);
                if (distance < 0.001)
                {
                    boarderNeighbor.Temperature = 100;
                    boarderNeighbor.IsConductive = true;
                }
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxStepSize.Text, out int stepSize) &&
                int.TryParse(textBoxEndTime.Text, out int endTime))
            {
                _stepSize = stepSize;
                _endTime = endTime;
                

                if (!bw.IsBusy)
                {
                    InitializeCalcValues();
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

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void int_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void buttonAddInspectionPoint_Click(object sender, EventArgs e)
        {
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.SetCommandPrompt("Select location if Inspection Point");
            gp.Get();
            if (gp.CommandResult() != Rhino.Commands.Result.Success)
                return;
            Point3d location = gp.Point();

            List<GeometryLarge> glList = _projectPlugIn.CurrentBeam.CrossSec.GetGeometryLarges();
            if (glList.Count != 1 || glList[0].Material.GetType() != typeof(ConcreteMaterial))
                return;

            CalcMesh cm = glList[0].CalcMesh;
            List<Point3d> centroids = new List<Point3d>();
            cm.MeshSegments.ForEach(o => centroids.Add(o.Centroid));

            
            Point3d calcSpacePoint = new Point3d(location);
            calcSpacePoint.Transform(_projectPlugIn.CurrentBeam.CrossSec.UnitTransform);
            PointCloud cl = new PointCloud(centroids);
            int index = cl.ClosestPoint(calcSpacePoint);

            InspectionPoint ip = new InspectionPoint(location, cm.MeshSegments[index], _projectPlugIn.CurrentBeam.CrossSec.AddingCentroid,
                _projectPlugIn.CurrentBeam.CrossSec);
            cm.MeshSegments[index].HasInspectionPoint = true;
            cm.MeshSegments[index].InspectionPoint = ip;

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
