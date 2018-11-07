using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Static_classes;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrossSectionDesign
{
    public partial class ChartForm : Form
    {
        public ChartForm()
        {
            InitializeComponent();
            FormClosing += MyForm_FormClosing;
        }

        private void MyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void SetChartValues(ColLoadCase clc)
        {
            if (clc.NMCurve != null && clc.NMCurve.Count != 0)
            {
                ResultChart.Series["Strength"].Points.Clear();
                foreach (Point3d pt in clc.NMCurve)
                {
                    ResultChart.Series["Strength"].Points.AddXY(pt.Z * 0.001, pt.X * 0.001);
                }
                ChartManipulationTools.SetAxisIntervalAndMax(ResultChart, clc.NMCurve, Moment.Mz);

                if (ResultChart.Series.IndexOf("Loading") == -1)
                {
                    ResultChart.Series.Add("Loading");
                    ResultChart.Series["Loading"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    ResultChart.Series["Loading"].Color = Color.BlueViolet;
                    ResultChart.Series["Loading"].BorderWidth = 2;
                }
                ResultChart.Series["Loading"].Points.Clear();
                clc.LoadCurve.ForEach(o => ResultChart.Series["Loading"].Points.AddXY(o.Z * 0.001, o.X * 0.001));

                if (ResultChart.Series.IndexOf(clc.Name) == -1)
                {
                    ChartManipulationTools.CreateNewPointChart(clc.Name, ResultChart);
                    ResultChart.Series[clc.Name].Points.AddXY(clc.M_EdComb * 0.001,
                        clc.N_Ed * 0.001);
                }
                else
                {
                    int index = ResultChart.Series.IndexOf(clc.Name);
                    ResultChart.Series[index].Points.Clear();
                    ResultChart.Series[index].Points.AddXY(clc.M_EdComb * 0.001,
                        clc.N_Ed * 0.001);
                }
            }
        }
        public void ClearChartValues()
        {
            int i = 0;
            while (i < ResultChart.Series.Count)
            {
                if (ResultChart.Series[i].Name == "Strength")
                {
                    ResultChart.Series[i].Points.Clear();
                    i++;
                }
                else
                    ResultChart.Series.Remove(ResultChart.Series[i]);
            }
        }

    }



}
