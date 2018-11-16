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
        private double _temperature = 0;
        public CalcMesh CalculationMesh { get; private set; }
        public Material Material { get { return _baseGeometry.Material; } set { } }
        public List<FaceNeighbor> FaceNeighbors { get; private set; } = new List<FaceNeighbor>();
        public List<BoarderNeighbor> BoarderNeighbors { get; private set; } = new List<BoarderNeighbor>();
        public int FaceNumber { get; set; }
        public Dictionary<LoadCase, double> Stresses { get; set; } = new Dictionary<LoadCase, double>();
        public Mesh ResultMesh { get => _resultMesh; set => _resultMesh = value; }

        public bool HasInspectionPoint { get; set; }
        public InspectionPoint InspectionPoint {get;set;}

        public Mesh GeometryMesh { get; set; }
        public Transform UnitTransform { get; private set; }
        public double Temperature { get => _temperature; set { _temperature = value; HeatQuantity = value * Material.SpecificHeat * Area * Material.Density; } }
        public double HeatQuantity { get; private set; }
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

        public void CalculateNeighbors()
        {
            List<int> neighbours = new List<int>();
            MeshFace thisFace = CalculationMesh.Faces[FaceNumber];
            
            FindNeighbour(thisFace, thisFace.A, thisFace.B, ref neighbours);
            FindNeighbour(thisFace, thisFace.B, thisFace.C, ref neighbours);
            if (thisFace.IsTriangle)
                FindNeighbour(thisFace, thisFace.C, thisFace.A, ref neighbours);
            else
            {
                FindNeighbour(thisFace, thisFace.C, thisFace.D, ref neighbours);
                FindNeighbour(thisFace, thisFace.D, thisFace.A, ref neighbours);
            }
        }

        public double CalculateNewTemperature(double timeStep, double currentTime)
        {
            double addedHeat = 0;

            foreach (FaceNeighbor faceNeighbor in FaceNeighbors)
            {
                addedHeat += (CalculationMesh.MeshSegments[faceNeighbor.Number].Temperature - Temperature) /
                    faceNeighbor.Distance * faceNeighbor.BoarderLength * Material.HeatConductivity * timeStep;
            }

            foreach (BoarderNeighbor bn in BoarderNeighbors)
            {
                if (bn.IsConductive)
                    addedHeat += (bn.Temperature - Temperature) /
                        bn.Distance * bn.BoarderLength * Material.HeatConductivity * timeStep;
            }
            double newTemp = (HeatQuantity + addedHeat) / (Material.SpecificHeat * Area * Material.Density);

            if (HasInspectionPoint)
                InspectionPoint.Results.Add(new Point2d(currentTime, newTemp));


            return newTemp;

            
        }

        private void FindNeighbour(MeshFace f, int a, int b, ref List<int> l)
        {
            int ta = CalculationMesh.TopologyVertices.TopologyVertexIndex(a);
            int tb = CalculationMesh.TopologyVertices.TopologyVertexIndex(b);
            int index = CalculationMesh.TopologyEdges.GetEdgeIndex(ta, tb);
            int[] connectedFaces = CalculationMesh.TopologyEdges.GetConnectedFaces(index);
            double boarderLength = new Vector3d(CalculationMesh.Vertices[a] - CalculationMesh.Vertices[b]).Length;
            if (connectedFaces.Length == 2)
            {
                int i;
                if (connectedFaces[0] != FaceNumber)
                    i = connectedFaces[0];
                else
                    i = connectedFaces[1];

                MeshSegment ms = CalculationMesh.MeshSegments.Find(s => s.FaceNumber == i);
                if (ms != null)
                {
                    double distance = new Vector3d(ms.Centroid - Centroid).Length;
                    FaceNeighbors.Add(new FaceNeighbor(boarderLength, distance, i));
                }
            }
            if (connectedFaces.Length == 1)
            {
                Line line = new Line(CalculationMesh.Vertices[a], CalculationMesh.Vertices[b]);
                Point3d p = line.ClosestPoint(Centroid, true);
                double distance = new Vector3d(Centroid - p).Length;
                BoarderNeighbor bo = new BoarderNeighbor(distance, boarderLength, p, index,Tuple.Create(ta,tb));
                BoarderNeighbors.Add(bo);
                CalculationMesh.boarderNeighbors.Add(bo);
            }

        }
    }
}
