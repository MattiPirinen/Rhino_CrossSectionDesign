using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrossSectionDesign.Classes_and_structures;
using Rhino.Geometry;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Excel.Application;
using CrossSectionDesign.Enumerates;

namespace CrossSectionDesign.Static_classes
{
    public static class TestCalculations
    {


        public static void TestCalculation1()
        {
            ProjectPlugIn ppi = ProjectPlugIn.Instance;


            CrossSection cs = ppi.CurrentBeam.CrossSec;
            Random rand = new Random();
            LoadCase lc = new SimpleLoadCase(0, 0, 0, ppi.CurrentBeam, "sampleLoadCase", Enumerates.LimitState.Ultimate);


            //Connect to excel
            Application oXL;
            Excel._Workbook oWB;
            Excel._Worksheet oSheet;
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
                    oXL = new Application();
                }

                LimitState ls = LimitState.Ultimate;


                oXL.Visible = true;
                //Get a new workbook.
                oWB = oXL.Workbooks.Add();
                oXL.ScreenUpdating = false;
                oSheet = (Excel._Worksheet)oWB.ActiveSheet;


                oSheet.Cells[1, 1].Value = "n";
                oSheet.Cells[1, 2].Value = "my";
                oSheet.Cells[1, 3].Value = "mz";
                oSheet.Cells[1, 4].Value = "iterations";
                oSheet.Cells[1, 5].Value = "resets";


                int[] iterationBoxes = Enumerable.Repeat(0, 10).ToArray();
                int[] repeatBoxes = Enumerable.Repeat(0, 10).ToArray();
                int iterationSpace = 10;
                int repeatSpace = 1;
                int failures = 0;

                for (int i = 0; i < 1000; i++)
                {
                    int iterations = 0;
                    int resets = 0;
                    double topStrain = rand.NextDouble() * (0.01 + cs.ConcreteMaterial.Epscu1) + cs.ConcreteMaterial.Epscu1;
                    double bottomStrain;
                    if (topStrain < cs.ConcreteMaterial.Epsc2)
                        bottomStrain = rand.NextDouble() * (0.01);
                    else if (topStrain < 0)
                        bottomStrain = rand.NextDouble() * (0.01 + cs.ConcreteMaterial.Epsc2) + cs.ConcreteMaterial.Epsc2;
                    else bottomStrain = rand.NextDouble() * (0.01 + cs.ConcreteMaterial.Epscu1) + cs.ConcreteMaterial.Epscu1;
                    double angle = Math.PI * rand.NextDouble();
                    Plane pl = Plane.WorldXY;
                    pl.Rotate(angle, Vector3d.ZAxis);
                    Point3d loading = cs.CalculateLoading(bottomStrain, topStrain, pl, ls);
                    bool success = cs.CalculateStresses(loading.X, loading.Z, loading.Y, "sampleLoadCase", ref iterations, ref resets, ls);
                    if (!success) failures++;
                    oSheet.Cells[i + 2, 1].Value = loading.X;
                    oSheet.Cells[i + 2, 2].Value = loading.Y;
                    oSheet.Cells[i + 2, 3].Value = loading.Z;
                    oSheet.Cells[i + 2, 4].Value = iterations;
                    oSheet.Cells[i + 2, 5].Value = resets;
                    oSheet.Cells[i + 2, 6].value = success;
                    if (iterations / iterationSpace < 9)
                        iterationBoxes[iterations / iterationSpace]++;
                    else
                        iterationBoxes[9]++;
                    if (resets / repeatSpace < 9)
                        repeatBoxes[resets / repeatSpace]++;
                    else
                        repeatBoxes[9]++;

                }

                oSheet.Range[oSheet.Cells[5, 8], oSheet.Cells[14, 8]].NumberFormat = "@";

                oSheet.Cells[1, 8].Value = "failures";
                oSheet.Cells[1, 9].Value = failures;

                oSheet.Cells[3, 8].Value = "Iterations";
                oSheet.Cells[4, 8].Value = "Range";
                oSheet.Cells[4, 9].Value = "Count";

                for (int i = 0; i < iterationBoxes.Length; i++)
                {

                    oSheet.Cells[i + 5, 8] = $"{i * iterationSpace}-{(i + 1) * iterationSpace}";
                    oSheet.Cells[i + 5, 9] = iterationBoxes[i];
                }
                Excel.Range r = oSheet.Range[oSheet.Cells[3, 8], oSheet.Cells[14, 9]];
                //Creating chart for iterations

                Excel.ChartObjects xlCharts = (Excel.ChartObjects)oSheet.ChartObjects(Type.Missing);
                Excel.ChartObject myChart = xlCharts.Add(10, 80, 300, 250);
                Excel.Chart c = myChart.Chart;
                c.ChartType = Excel.XlChartType.xlColumnClustered;
                c.SetSourceData(r);



                oSheet.Range[oSheet.Cells[17, 8], oSheet.Cells[28, 8]].NumberFormat = "@";

                oSheet.Cells[17, 8].Value = "Resets";
                oSheet.Cells[18, 8].Value = "Range";
                oSheet.Cells[18, 9].Value = "Count";
                for (int i = 0; i < iterationBoxes.Length; i++)
                {
                    oSheet.Cells[i + 19, 8] = $"{ i * repeatSpace}-{(i + 1) * repeatSpace}";
                    oSheet.Cells[i + 19, 9] = repeatBoxes[i];
                }


                r = oSheet.Range[oSheet.Cells[17, 8], oSheet.Cells[28, 9]];
                myChart = xlCharts.Add(390, 80, 300, 250);
                c = myChart.Chart;
                c.ChartType = Excel.XlChartType.xlColumnClustered;
                c.SetSourceData(r);




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


        public static List<Tuple<double,double>> HeatConductionTest(double ldif_1)
        {
            double A = 1;
            double gamma = 880;
            double K = 0.73;
            double roo = 2200;
            double ldif = ldif_1;
            double ltot = 0.1;
            double Tini = 100;
            double t0 = 0;
            List<Slice> slices = new List<Slice>();
            double timeStep = 10;
            List<Tuple<double, double>> SamplePointValues = new List<Tuple<double, double>>();
            for (double i = 0; isApproxSmallerThan(i,ltot,0.00001); i+= ldif)
            {
                slices.Add(new Slice(i, i + ldif, 0));
            }
            SamplePointValues.Add(Tuple.Create(0.0, slices[slices.Count - 1].T));
            for (int i = 0; i < 10000; i++)
            {
                List<double> temps = new List<double>();
                for (int k = 0; k < slices.Count; k++)
                {
                    double heatDiff = 0;
                    if (k == 0)
                        heatDiff += (Tini-slices[k].T) * K * A / (ldif/2)* timeStep;
                    else
                        heatDiff += (slices[k -1].T - slices[k].T) * K * A / ldif* timeStep;
                    if (k != slices.Count-1)
                        heatDiff += (slices[k + 1].T - slices[k].T) * K * A / ldif*timeStep;

                    temps.Add(slices[k].T + heatDiff / (roo * A * gamma * ldif));
                }
                for (int k = 0; k < slices.Count; k++)
                {
                    slices[k].T = temps[k];
                }
                SamplePointValues.Add(Tuple.Create(timeStep*i, slices[slices.Count - 1].T));

            }
            return SamplePointValues;

        }

        private static bool isApproxSmallerThan(double i, double ltot, double v)
        {
            return (ltot - i) > v; 
        }

        private class Slice
        {
            public double StartPos;
            public double EndPos;
            public double T;
            
            public Slice(double startPos, double endPos, double t)
            {
                StartPos = startPos;
                EndPos = endPos;
                t = T;
            }

        }

    }
}
