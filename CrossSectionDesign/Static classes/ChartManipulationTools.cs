using CrossSectionDesign.Enumerates;
using MoreLinq;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace CrossSectionDesign.Static_classes
{
    public static class ChartManipulationTools
    {

        //This method creates an interval for chart plotting. It returns axis min value max value and interval 
        public static Tuple<double, double, double> CreateInterval(double maxValue, double minValue)
        {

            char firstNumMin = '0';
            char secondNumMin = '0';

            if (minValue > 0 || Math.Abs(minValue / maxValue) < Math.Pow(10, -5))
                minValue = 0;
            else
            {
                firstNumMin = minValue.ToString()[1];
                secondNumMin = minValue.ToString().Length > 1 ? minValue.ToString()[2] : '0';
            }

            char firstNumMax = maxValue.ToString()[0];
            char secondNumMax;
            if (maxValue.ToString().Length > 1)
                secondNumMax = maxValue.ToString()[1];
            else
                secondNumMax = '0';
            double numbMax = Math.Floor(Math.Log10(maxValue));
            double numbMin = Math.Floor(Math.Log10(Math.Abs(minValue)));
            double numb = Math.Floor(Math.Log10(maxValue - minValue));

            char firstNum = (maxValue - minValue).ToString()[0];

            double axisMax = 0;
            double axisMin = 0;
            double interval = 0;

            if (firstNum == '9' || firstNum == '8' || firstNum == '7')
            {
                axisMax = (char.GetNumericValue(firstNumMax) + 1) * Math.Pow(10, numbMax);
                if (minValue == 0) axisMin = 0;
                else axisMin = -(char.GetNumericValue(firstNumMin) + 1) * Math.Pow(10, numbMin);
                interval = 2 * Math.Pow(10, numb);
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
                    axisMax = (firstTwoMax + 10) * Math.Pow(10, numbMax - 1);
                else
                    axisMax = (firstTwoMax + 5) * Math.Pow(10, numbMax - 1);

                string test = char.ToString(firstNumMin) + char.ToString(secondNumMin);
                double test2 = double.Parse(test) / 5;
                double firstTwoMin = Math.Floor(double.Parse(char.ToString(firstNumMin) + char.ToString(secondNumMin)) / 5) * 5;

                if (minValue == 0) axisMin = 0;



                else
                {
                    if (numbMin < numb)
                        axisMin = -(firstTwoMin + 10) * Math.Pow(10, numbMin - 1);
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

        public static void SetAxisIntervalAndMax(Chart chart, Polyline values, Moment m)
        {
            //X-axis min max and interval
            double maxValue;
            double minValue;


            switch (m)
            {
                case Moment.Mz:
                    maxValue = values.Max(x => x.Z) * Math.Pow(10, -3);
                    minValue = values.Min(x => x.Z) * Math.Pow(10, -3);
                    break;
                case Moment.My:
                    maxValue = values.Max(x => x.Y) * Math.Pow(10, -3);
                    minValue = values.Min(x => x.Y) * Math.Pow(10, -3);
                    break;
                case Moment.MComb:
                    double maxValue1 = values.Max(x => x.Y) * Math.Pow(10, -3);
                    double minValue1 = values.Min(x => x.Y) * Math.Pow(10, -3);
                    double maxValue2 = values.Max(x => x.Z) * Math.Pow(10, -3);
                    double minValue2 = values.Min(x => x.Z) * Math.Pow(10, -3);
                    maxValue = Math.Max(maxValue1, maxValue2);
                    minValue = Math.Min(minValue1, minValue2);
                    break;
                default:
                    maxValue = 0;
                    minValue = 0;
                    break;
            }

            Tuple<double, double, double> minMaxInterval = ChartManipulationTools.CreateInterval(maxValue, minValue);
            //Tuple<double, double, double> minMaxInterval = CreateInterval(maxValue, 0);
            chart.ChartAreas[0].AxisX.Crossing = 0;
            chart.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart.ChartAreas[0].AxisX.Minimum = minMaxInterval.Item1;
            chart.ChartAreas[0].AxisX.Maximum = minMaxInterval.Item2;
            chart.ChartAreas[0].AxisX.MajorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisX.MajorGrid.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.MinorGrid.Interval = minMaxInterval.Item3 / 5;
            chart.ChartAreas[0].AxisX.MinorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % (minMaxInterval.Item3 / 5));
            chart.ChartAreas[0].AxisX.LabelStyle.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.LabelStyle.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);

            //Y-axis min, max and interval 
            maxValue = values.Max(x => x.X) * Math.Pow(10, -3);
            minValue = values.Min(x => x.X) * Math.Pow(10, -3);
            minMaxInterval = ChartManipulationTools.CreateInterval(maxValue, minValue);
            //minMaxInterval = CreateInterval(0, minValue);
            chart.ChartAreas[0].AxisY.Minimum = minMaxInterval.Item1;
            chart.ChartAreas[0].AxisY.Maximum = minMaxInterval.Item2;
            chart.ChartAreas[0].AxisY.MajorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisY.MajorGrid.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.MinorGrid.Interval = minMaxInterval.Item3 / 5;
            chart.ChartAreas[0].AxisY.MinorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % (minMaxInterval.Item3 / 5));
            chart.ChartAreas[0].AxisY.LabelStyle.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.LabelStyle.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisY.MajorTickMark.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
        }

        public static void SetAxisIntervalAndMax(Chart chart)
        {
            //X-axis min max and interval
            double maxValue = 100;
            double minValue = -100;


            foreach (Series series in chart.Series)
            {
                maxValue =Math.Max(maxValue, series.Points.MaxBy(p =>p.XValue).XValue);
                minValue = Math.Min(minValue, series.Points.MinBy(p => p.XValue).XValue);
            }

            Tuple<double, double, double> minMaxInterval = ChartManipulationTools.CreateInterval(maxValue, minValue);
            //Tuple<double, double, double> minMaxInterval = CreateInterval(maxValue, 0);
            chart.ChartAreas[0].AxisX.Crossing = 0;
            chart.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart.ChartAreas[0].AxisX.Minimum = minMaxInterval.Item1;
            chart.ChartAreas[0].AxisX.Maximum = minMaxInterval.Item2;
            chart.ChartAreas[0].AxisX.MajorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisX.MajorGrid.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.MinorGrid.Interval = minMaxInterval.Item3 / 5;
            chart.ChartAreas[0].AxisX.MinorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % (minMaxInterval.Item3 / 5));
            chart.ChartAreas[0].AxisX.LabelStyle.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.LabelStyle.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisX.MajorTickMark.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);

            //Y-axis min, max and interval 
            foreach (Series series in chart.Series)
            {
                maxValue =Math.Max(maxValue, series.Points.MaxBy(p => p.YValues[0]).YValues[0]);
                minValue = Math.Min(minValue, series.Points.MinBy(p => p.YValues[0]).YValues[0]);
            }
            minMaxInterval = ChartManipulationTools.CreateInterval(maxValue, minValue);
            //minMaxInterval = CreateInterval(0, minValue);
            chart.ChartAreas[0].AxisY.Minimum = minMaxInterval.Item1;
            chart.ChartAreas[0].AxisY.Maximum = minMaxInterval.Item2;
            chart.ChartAreas[0].AxisY.MajorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisY.MajorGrid.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.MinorGrid.Interval = minMaxInterval.Item3 / 5;
            chart.ChartAreas[0].AxisY.MinorGrid.IntervalOffset = Math.Abs(minMaxInterval.Item1 % (minMaxInterval.Item3 / 5));
            chart.ChartAreas[0].AxisY.LabelStyle.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.LabelStyle.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
            chart.ChartAreas[0].AxisY.MajorTickMark.Interval = minMaxInterval.Item3;
            chart.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = Math.Abs(minMaxInterval.Item1 % minMaxInterval.Item3);
        }


        public static void CreateNewPointChart(string name, Chart chart)
        {
            chart.Series.Add(name);
            chart.Series[name].ChartType = SeriesChartType.Point;
            chart.Series[name].MarkerStyle = MarkerStyle.Cross;
            chart.Series[name].MarkerSize = 10;
        }

    }
}
