using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Interfaces;
using CrossSectionDesign.Static_classes;
using Rhino;
using Rhino.Geometry;

namespace CrossSectionDesign.Classes_and_structures
{
    public class MeshSegment : ICalcGeometry
    {
        public Point3d Centroid { get; set; }

        public double Area { get; set; }
        private GeometryLarge _baseGeometry;
        private Mesh _resultMesh;

        public Material Material { get { return _baseGeometry.Material; } set { } }

        public int FaceNumber { get; set; }
        public Dictionary<LoadCase, double> Stresses { get; set; } = new Dictionary<LoadCase, double>();
        public Mesh ResultMesh { get => _resultMesh; set => _resultMesh = value; }
        public Mesh GeometryMesh { get; set; }

        public MeshSegment(Point3d c, double a, GeometryLarge gl, Mesh resultMesh)
        {
            Centroid = c;
            Area = a;
            GeometryMesh = resultMesh;
            _baseGeometry = gl;
        }

        public ICalcGeometry DeepCopy()
        {
            throw new NotImplementedException();
        }

        public void ModifyMesh(double distance)
        {
            _resultMesh =  MeshManipulationTools.CreateExtrudedMesh(GeometryMesh, Vector3d.ZAxis, distance);
        }
    }
}
