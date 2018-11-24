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
        private double _temperature = 20;
        private double _heatQuantity = 0;
        public CalcMesh CalculationMesh { get; private set; }
        public Material Material { get { return _baseGeometry.Material; } set { } }
        public List<FaceConnection> FaceNeighbors { get; private set; } = new List<FaceConnection>();
        public List<BoarderEdge> BoarderNeighbors { get; private set; } = new List<BoarderEdge>();
        public int FaceNumber { get; set; }
        public Dictionary<LoadCase, double> Stresses { get; set; } = new Dictionary<LoadCase, double>();
        public Mesh ResultMesh { get => _resultMesh; set => _resultMesh = value; }
        public Vector3d HeatFlow { get; set; }

        public Mesh GeometryMesh { get; set; }
        public Transform UnitTransform { get; private set; }
        public double Temperature { get => _temperature; set { _temperature = value; } }
        public double HeatQuantity { get => _heatQuantity;
            set {
                _heatQuantity = value;
                Temperature = Material.CalcTemp(value / Area);
            }
        } 
        public MeshSegment(Point3d c, double a, GeometryLarge gl, Mesh geometryMesh, CalcMesh calculationMesh, int faceNumber)
        {
            FaceNumber = faceNumber;
            
            Centroid = c;
            Area = a;
            GeometryMesh = geometryMesh;
            _baseGeometry = gl;
            CalculationMesh = calculationMesh;
        }

        public ICalcGeometry DeepCopy()
        {
            throw new NotImplementedException();
        }

        public void ModifyMesh(double distance)
        {
            _resultMesh = MeshManipulationTools.CreateExtrudedMesh(GeometryMesh, Vector3d.ZAxis, distance);
        }
        public Mesh GetModelScaleResultMesh()
        {
            Mesh m = ResultMesh.DuplicateMesh();
            m.Transform(_baseGeometry.InverseUnitTransform);
            return m;
        }

    }
}
