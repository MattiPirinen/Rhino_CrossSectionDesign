using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Display;
using Rhino.Geometry;
using System.Drawing;
using Rhino;
using System.Drawing.Drawing2D;
namespace CrossSectionDesign.Display_classes
{
    public class ColorScaleDisplay:DisplayConduit
    {
        private DisplayBitmap _bm;
        private string _label;

        public ColorScaleDisplay()
        {
            SetColorScale(-100,100,0,0.7, "temp");
        }

        public void SetColorScale(double minValue, double maxValue)
        {
            SetColorScale(minValue, maxValue, 0, 0.7, _label != null ? _label : "");
        }

        public void SetColorScale(double minValue, double maxValue, double minColor, double maxColor, string label)
        {
            _label = label;

            Bitmap bm = new System.Drawing.Bitmap(100, 290);
            Graphics g = Graphics.FromImage(bm);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            Rectangle r = new Rectangle(0,0,100,290);
            g.FillRectangle(new SolidBrush(Color.White), r);

            for (int i = 5; i < 260; i++)
            {
                double hue = (minColor + (i-5.0) * (maxColor - minColor)) / 255.0;

                Color c = new ColorHSL(hue, 1, 0.5);

                for (int j = 5; j < 25; j++)
                {
                    bm.SetPixel(j, i, c);
                    
                }
            }


            //minValue = minValue * 0.001;
            //maxValue = maxValue * 0.001;

            double range = maxValue-minValue;
            int numOfNumbers = 10;
            int step = 255 / (numOfNumbers-1);

            Font usedFont = new Font("Tahoma", 8,FontStyle.Bold);
            for (int i = 0; i < numOfNumbers; i++)
			{
                string value = Math.Round((maxValue-i/(numOfNumbers-1.0)*range),2).ToString();

			     g.DrawString(value,usedFont , Brushes.Black,30,1+i*step);
			}
            g.DrawString(_label, usedFont, Brushes.Black, 2, 270);
            _bm = new DisplayBitmap(bm);
        }

        protected override void DrawForeground(DrawEventArgs e)
        {

            base.DrawForeground(e);
            e.Display.DrawBitmap(_bm,10,25);
        }


    }
}
