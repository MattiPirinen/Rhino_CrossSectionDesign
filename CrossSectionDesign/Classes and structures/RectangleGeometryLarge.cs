using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using Rhino.FileIO;
using Rhino.Geometry;

namespace CrossSectionDesign.Classes_and_structures
{

    [Guid("440C5FD0-AABB-4A2C-B517-7B475FF5B713")]
    public class RectangleGeometryLarge:GeometryLarge
    {
        private double _height;
        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                if (Height != 0)
                {
                    CreateOutline();
                    CreateBaseBrep();
                }
            }
        }
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                if (Width != 0)
                {
                    CreateOutline();
                    CreateBaseBrep();
                }
                
            }
        }
        public Rectangle3d Outline { get; private set; }

        

        public RectangleGeometryLarge()
        { }

        public RectangleGeometryLarge(Point3d centroid, double height, double width)
        {
            Centroid = centroid;
            Height = height;
            Width = width;
            CreateBaseBrep();
        }

        private void CreateOutline()
        {
            Point3d pointA = Centroid - new Vector3d(Width / 2, -Height / 2, 0);
            Point3d pointB = Centroid + new Vector3d(Width / 2, -Height / 2, 0);
            Outline = new Rectangle3d(Plane.WorldXY, pointA, pointB);
        }

        private void CreateBaseBrep()
        {
            BaseBrep = Brep.CreatePlanarBreps(new Curve[] { Outline.ToNurbsCurve() })[0];
        }

        protected override bool Read(BinaryArchiveReader archive)
        {
            Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
            Height = dict.GetDouble("Height");
            Width = dict.GetDouble("Width");
            Centroid = dict.GetPoint3d("Centroid");

            return base.Read(archive);
        }

        protected override bool Write(BinaryArchiveWriter archive)
        {

            var dict = new Rhino.Collections.ArchivableDictionary(20181023, "Values");
            dict.Set("Height", _height);
            dict.Set("Width,", _width);
            dict.Set("Centroid", Centroid);
            return base.Write(archive);
        }
    }
}
