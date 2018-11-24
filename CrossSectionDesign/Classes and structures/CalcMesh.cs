using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using CrossSectionDesign.Interfaces;
using CrossSectionDesign.Abstract_classes;
using System.Drawing;
using Numerics = MathNet.Numerics.LinearAlgebra;
using MoreLinq;
using Rhino.Geometry.Intersect;

namespace CrossSectionDesign.Classes_and_structures
{
    public class CalcMesh:Mesh
    {

        public List<InspectionPoint> InspectionPoints = new List<InspectionPoint>();
        public int MidIndice { get; set; }
        private Numerics.Matrix<double> _heatMatrix;
        public Numerics.Matrix<double> HeatMatrix { get => _heatMatrix; set { _heatMatrix = value; } }
        public List<double> FaceHeats { get; set; } = new List<double>();
        public List<Vector3d> FaceHeatDirections{ get; set; } = new List<Vector3d>();
        public List<MeshSegment> MeshSegments { get; set; } = new List<MeshSegment>();
        public Material Material { get; set; }
        public List<BoarderEdge> BoarderEdges { get;private set; } = new List<BoarderEdge>();
        public List<FaceConnection> FaceConnections { get; private set; } = new List<FaceConnection>();
        public Tuple<double, double> MinAndMaxTemp { get; set; } = Tuple.Create(0.0, 1200.0);
        public Mesh ResultMesh { get; set; } = new Mesh();
        public double HeatFlowFactor { get; set; } = 0;

        public CalcMesh(Mesh m, GeometryLarge gl)
        {


            double maxY = double.MinValue;
            Append(m);
            ResultMesh.Append(m);


            VertexColors.Clear();
            for (int s = 0; s < Vertices.Count; s++)
            {
                VertexColors.Add(Color.Beige);
            }


            int i = 0;
            foreach (MeshFace mf in Faces)
            {
                Mesh tempMesh = new Mesh();
                if (mf.IsTriangle)
                {
                    tempMesh.Vertices.Add(m.Vertices[mf.A]);
                    tempMesh.Vertices.Add(m.Vertices[mf.B]);
                    tempMesh.Vertices.Add(m.Vertices[mf.C]);
                    tempMesh.Faces.AddFace(new MeshFace(0, 1, 2));
                }
                else
                {
                    tempMesh.Vertices.Add(m.Vertices[mf.A]);
                    tempMesh.Vertices.Add(m.Vertices[mf.B]);
                    tempMesh.Vertices.Add(m.Vertices[mf.C]);
                    tempMesh.Vertices.Add(m.Vertices[mf.D]);
                    tempMesh.Faces.AddFace(new MeshFace(0, 1, 2,3));
                }
                Point3d c = ((Vertices[mf.A] + (Point3d)Vertices[mf.B]) + Vertices[mf.C]) / 3;
                Vector3d ac = (Vertices[mf.C] - Vertices[mf.A]);
                Vector3d ab = (Vertices[mf.B] - Vertices[mf.A]);
                double area = Vector3d.CrossProduct(ac, ab).Length / 2;

                //MeshSegments.Add(new MeshSegment(c, area,gl,ns.ToBrep()));
                if (!mf.IsTriangle)
                {
                    Point3d c2 = ((Vertices[mf.A] + (Point3d)Vertices[mf.C]) + Vertices[mf.D]) / 3;
                    Vector3d ad = (Vertices[mf.D] - Vertices[mf.A]);
                    ab = (Vertices[mf.C] - Vertices[mf.A]);
                    double area2 = Vector3d.CrossProduct(ad, ab).Length / 2;
                    c = (c * area + c2 * area2) / (area + area2);
                    area = area + area2;
                    
                }
                if (c.Y > maxY)
                    MidIndice = i;
                
                MeshSegments.Add(new MeshSegment(c, area, gl, tempMesh, this,i));
                i++;
            }
            for (int k = 0; k < MeshSegments.Count; k++)
            {
                FaceHeats.Add(0);
                FaceHeatDirections.Add(new Vector3d(0, 0, 0));
            }


            CalculateNeighbors();
            _heatMatrix = Numerics.Matrix<double>.Build.Dense(MeshSegments.Count + BoarderEdges.Count, 1, 0);


        }

        public CalcMesh(Mesh m, List<GeometryLarge> gls)
        {
            double maxY = double.MinValue;
            Append(m);
            ResultMesh.Append(m);
            VertexColors.Clear();
            for (int s = 0; s < Vertices.Count; s++)
            {
                VertexColors.Add(Color.Beige);
            }


            int i = 0;
            List<Brep> breps = new List<Brep>();
            gls.ForEach(gl => breps.Add(gl.BaseBrep));

            foreach (MeshFace mf in Faces)
            {
                Mesh tempMesh = new Mesh();
                if (mf.IsTriangle)
                {
                    tempMesh.Vertices.Add(m.Vertices[mf.A]);
                    tempMesh.Vertices.Add(m.Vertices[mf.B]);
                    tempMesh.Vertices.Add(m.Vertices[mf.C]);
                    tempMesh.Faces.AddFace(new MeshFace(0, 1, 2));
                }
                else
                {
                    tempMesh.Vertices.Add(m.Vertices[mf.A]);
                    tempMesh.Vertices.Add(m.Vertices[mf.B]);
                    tempMesh.Vertices.Add(m.Vertices[mf.C]);
                    tempMesh.Vertices.Add(m.Vertices[mf.D]);
                    tempMesh.Faces.AddFace(new MeshFace(0, 1, 2, 3));
                }
                Point3d c = ((Vertices[mf.A] + (Point3d)Vertices[mf.B]) + Vertices[mf.C]) / 3;
                Vector3d ac = (Vertices[mf.C] - Vertices[mf.A]);
                Vector3d ab = (Vertices[mf.B] - Vertices[mf.A]);
                double area = Vector3d.CrossProduct(ac, ab).Length / 2;

                //MeshSegments.Add(new MeshSegment(c, area,gl,ns.ToBrep()));
                if (!mf.IsTriangle)
                {
                    Point3d c2 = ((Vertices[mf.A] + (Point3d)Vertices[mf.C]) + Vertices[mf.D]) / 3;
                    Vector3d ad = (Vertices[mf.D] - Vertices[mf.A]);
                    ab = (Vertices[mf.C] - Vertices[mf.A]);
                    double area2 = Vector3d.CrossProduct(ad, ab).Length / 2;
                    c = (c * area + c2 * area2) / (area + area2);
                    area = area + area2;

                }
                if (c.Y > maxY)
                    MidIndice = i;

                int ii = 0;
                bool test = false;
                while (ii<breps.Count && !test)
                {
                    if (new Vector3d(breps[ii].ClosestPoint(c) - c).Length 
                        <= ProjectPlugIn.Instance.ActiveDoc.ModelAbsoluteTolerance)
                        test = true;
                    ii++;
                }

                MeshSegments.Add(new MeshSegment(c, area, gls[ii-1], tempMesh, this, i));
                i++;
            }
            for (int k = 0; k < MeshSegments.Count; k++)
            {
                FaceHeats.Add(0);
                FaceHeatDirections.Add(new Vector3d(0, 0, 0));
            }


            CalculateNeighbors();
            _heatMatrix = Numerics.Matrix<double>.Build.Dense(MeshSegments.Count + BoarderEdges.Count, 1, 0);
        }

        private void CalculateNeighbors()
        {
            for (int i = 0; i < TopologyEdges.Count; i++)
            {
                int[] faces = TopologyEdges.GetConnectedFaces(i);
                Rhino.IndexPair topoVertices = TopologyEdges.GetTopologyVertices(i);
                
                Line b = new Line(TopologyVertices[topoVertices.I], TopologyVertices[topoVertices.J]);
                double boarderLength = b.Length;


                if (faces.Length == 2)
                {
                    Line f = new Line(MeshSegments[faces[0]].Centroid, MeshSegments[faces[1]].Centroid);
                    Vector3d flowDirection = f.Direction;
                    flowDirection.Unitize();
                    Intersection.LineLine(f, b, out double a, out double temp);
                    double distance1 = new Vector3d(f.PointAt(a) - f.From).Length;
                    double distance2 = new Vector3d(f.PointAt(a) - f.To).Length;

                    FaceConnections.Add(new FaceConnection(boarderLength, distance1,distance2,Tuple.Create(faces[0],faces[1]),
                        flowDirection));
                }
                else if (faces.Length == 1)
                {
                    Line line = new Line(TopologyVertices[topoVertices.I], TopologyVertices[topoVertices.J]);


                    Point3d p = line.ClosestPoint(MeshSegments[faces[0]].Centroid, true);
                    Vector3d flowDirection = new Vector3d(MeshSegments[faces[0]].Centroid - p);
                    flowDirection.Unitize();

                    double distance = new Vector3d(MeshSegments[faces[0]].Centroid - p).Length;
                    BoarderEdges.Add(new BoarderEdge(distance, boarderLength, p, Tuple.Create(topoVertices.I,
                        topoVertices.J),faces[0], i,25, flowDirection));
                }
            }
        }

        //Calculate new temperatures by arranging the heat flow parameters to matrix... Very slow right now :(.. Maybe I should try to suck less.
        private void CalculateNewTemperatures(double timeStep, double currentTime)

        {
            
            // *************** Heat flow matrix calculation *********************
            int msCount = MeshSegments.Count;
            int HeatFlowObjects = MeshSegments.Count + BoarderEdges.Count;

            Numerics.Matrix<double> m = Numerics.Matrix<double>.Build.Sparse(HeatFlowObjects, HeatFlowObjects);
            foreach (FaceConnection f in FaceConnections)
            {
                double averageTemp = (MeshSegments[f.ConnectedFaces.Item1].Temperature +
                    MeshSegments[f.ConnectedFaces.Item2].Temperature) / 2;


                double heatFlow = (MeshSegments[f.ConnectedFaces.Item1].Temperature - MeshSegments[f.ConnectedFaces.Item2].Temperature) *
                                    (MeshSegments[f.ConnectedFaces.Item2].Material.HeatConductivity(averageTemp) / f.Distance1 +
                                     MeshSegments[f.ConnectedFaces.Item2].Material.HeatConductivity(averageTemp) / f.Distance2)
                                    * f.BoarderLength * timeStep;
                m[f.ConnectedFaces.Item1, f.ConnectedFaces.Item2] = -heatFlow;
                m[f.ConnectedFaces.Item2, f.ConnectedFaces.Item1] = heatFlow;
            }
            int i = 0;
            foreach (BoarderEdge b in BoarderEdges)
            {
                if (b.IsConductive)
                {
                    double averageTemp = (b.Temperature(currentTime) + MeshSegments[b.ConnectedFace].Temperature) / 2;

                    double heatFlow = (b.Temperature(currentTime) - MeshSegments[b.ConnectedFace].Temperature) /
                            b.Distance * b.BoarderLength * Material.HeatConductivity(averageTemp) * timeStep;
                    m[b.ConnectedFace, msCount + i] = heatFlow;
                }
                i++;
            }
            // *************** Temperature Change calculation *********************
            Numerics.Matrix<double> mult = Numerics.Matrix<double>.Build.Dense(HeatFlowObjects, 1, 1);
            

            _heatMatrix = m * mult + _heatMatrix;

            //Update heat quantitys
            for (int k = 0; k < MeshSegments.Count; k++)
            {
                MeshSegments[k].HeatQuantity = _heatMatrix[k, 0];
            }
            
        }

        public void CalculateNewTemperatures2(double timeStep, double currentTime)
        {
            // Stefan Boltzmann constant
            double sigma = 5.670367 * Math.Pow(10, -8);
            for (int s = 0; s < FaceHeatDirections.Count; s++)
            {
                FaceHeatDirections[s] = new Vector3d(0, 0, 0);
            }

            //Surface emissivity
            double epsilon_m = 0.8;

            foreach (FaceConnection f in FaceConnections)
            {
                double averageTemp = (MeshSegments[f.ConnectedFaces.Item1].Temperature +
                    MeshSegments[f.ConnectedFaces.Item2].Temperature) / 2;

                double resistivity = 1 /
                    (1 / (MeshSegments[f.ConnectedFaces.Item1].Material.HeatConductivity(averageTemp) / f.Distance1) +
                    1 / (MeshSegments[f.ConnectedFaces.Item2].Material.HeatConductivity(averageTemp) / f.Distance2));

                double heatFlow = (MeshSegments[f.ConnectedFaces.Item1].Temperature - MeshSegments[f.ConnectedFaces.Item2].Temperature) *
                    resistivity * f.BoarderLength * timeStep;
                FaceHeats[f.ConnectedFaces.Item1] += -heatFlow;
                FaceHeats[f.ConnectedFaces.Item2] += heatFlow;
                FaceHeatDirections[f.ConnectedFaces.Item1] += f.FlowDirection * heatFlow ;
                FaceHeatDirections[f.ConnectedFaces.Item2] += f.FlowDirection * heatFlow ;

            }
            int i = 0;
            foreach (BoarderEdge b in BoarderEdges)
            {
                if (b.IsConductive)
                {
                    
                    double averageTemp = (b.Temperature(currentTime) + MeshSegments[b.ConnectedFace].Temperature) / 2;
                    double heatFlowConvection = b.ConvCoef*b.BoarderLength*(b.Temperature(currentTime) - 
                        MeshSegments[b.ConnectedFace].Temperature)*timeStep;
                    double heatFlowRadiation = sigma* epsilon_m * (Math.Pow((b.Temperature(currentTime)+273),4) - 
                        Math.Pow(MeshSegments[b.ConnectedFace].Temperature+273,4))*b.BoarderLength*timeStep;
                    //double heatFlow = (b.Temperature - MeshSegments[b.ConnectedFace].Temperature) /
                    //        b.Distance * b.BoarderLength * Material.HeatConductivity(averageTemp) * timeStep;
                    FaceHeats[b.ConnectedFace] += heatFlowConvection+heatFlowRadiation;
                    FaceHeatDirections[b.ConnectedFace] += b.FlowDirection * (heatFlowConvection + heatFlowRadiation);

                }
                i++;
            }
            // *************** Temperature Change calculation *********************
            //Update heat quantitys
            for (int k = 0; k < MeshSegments.Count; k++)
            {
                MeshSegments[k].HeatQuantity = FaceHeats[k];
                MeshSegments[k].HeatFlow = FaceHeatDirections[k];
            }

        }


        public void CalculateTemperatures(double minValue, double maxValue)
        {
            
            double temp = 0;

            for (int i = 0; i < TopologyVertices.Count; i++)
            {

                List<BoarderEdge> bn = FindBoarderNeighbors(i);
                /*
                if (bn.Count == 2 && (bn[0].IsConductive || bn[1].IsConductive))
                {
                    if (bn[0].IsConductive && bn[1].IsConductive)
                    {
                        double l1 = new Vector3d(TopologyVertices[i] - bn[0].Centroid).Length;
                        double l2 = new Vector3d(TopologyVertices[i] - bn[1].Centroid).Length;
                        temp = (bn[0].Temperature(temp) * l1 + bn[1].Temperature* l2) / (l1 + l2);
                    }
                    else if (bn[0].IsConductive)
                        temp = bn[0].Temperature;
                    else
                        temp = bn[1].Temperature;
                }
                
                else
                */
                bool jee = false;
                int[] faces = TopologyVertices.ConnectedFaces(i);
                if (faces.Length == 0)
                    jee = true;
                double top = 0;
                double bottom = 0;
                foreach (int f in faces)
                {
                    double l = new Vector3d(TopologyVertices[i] - MeshSegments[f].Centroid).Length;
                    top += l * MeshSegments[f].Temperature;
                    bottom += l;
                }
                temp = top / bottom;

                int[] indices = TopologyVertices.MeshVertexIndices(i);

                if (indices.Length == 1 && indices[0] < VertexColors.Count)
                {
                    double value = 0.7 - 0.7 * (temp - minValue) / (maxValue - minValue);
                    if (value < 0 || value > 0.7)

                        VertexColors[indices[0]] = Utils.HSL2RGB(1, 1, 1);
                    else
                        VertexColors[indices[0]] = Utils.HSL2RGB(value, 1, 0.5);
                }
            }
        }





        public void CalculateVertexTemperatures(double calcTime)
        {
            
            double temp = 0;
            for (int i = 0; i < TopologyVertices.Count; i++)
            {
                /*
                List<BoarderEdge> bn = FindBoarderNeighbors(i);
                if (bn.Count == 2 && (bn[0].IsConductive || bn[1].IsConductive))
                {
                    if (bn[0].IsConductive && bn[1].IsConductive)
                    {
                        double l1 = new Vector3d(TopologyVertices[i] - bn[0].Centroid).Length;
                        double l2 = new Vector3d(TopologyVertices[i] - bn[1].Centroid).Length;
                        temp = (bn[0].Temperature * l1 + bn[1].Temperature * l2) / (l1 + l2);
                    }
                    else if (bn[0].IsConductive)
                        temp = bn[0].Temperature;
                    else
                        temp = bn[1].Temperature;
                }
                
                else
                {
                */
                int[] faces = TopologyVertices.ConnectedFaces(i);
                    double top = 0;
                    double bottom = 0;
                    foreach (int f in faces)
                    {
                        double l = new Vector3d(TopologyVertices[i] - MeshSegments[f].Centroid).Length;
                        top += l * MeshSegments[f].Temperature;
                        bottom += l;
                    }
                    temp = top / bottom;
                
                int[] indices = TopologyVertices.MeshVertexIndices(i);
                foreach (int index in indices)
                {
                    ResultMesh.Vertices[index] = new Point3f(ResultMesh.Vertices[index].X, ResultMesh.Vertices[index].Y,
                        (float)temp);

                    double value = 0.7 - 0.7 * (temp - MinAndMaxTemp.Item1) / (MinAndMaxTemp.Item2 - MinAndMaxTemp.Item1);
                    if (value < 0 || value > 0.7)
                        VertexColors[index] = Utils.HSL2RGB(1, 1, 1);
                    else
                        VertexColors[index] = Utils.HSL2RGB(value, 1, 0.5);
                }

            }

            foreach (InspectionPoint ip in InspectionPoints)
            {
                Line l = new Line(ip.Location+Vector3d.ZAxis*-10000, Vector3d.ZAxis * 20000);

                Point3d[] pts = Intersection.MeshLine(ResultMesh, l, out int[] temp1);
                if (pts.Length == 0)
                    continue;
                else
                    ip.Results.Add(new Point2d(calcTime, pts[0].Z));

            }

        }

        public void CalcMinAndMaxTemp()
        {
            /*
            MinAndMaxTemp =  Tuple.Create(BoarderEdges.MinBy(o => o.Temperature).Temperature, 
                BoarderEdges.MaxBy(o => o.Temperature).Temperature);
                */
            MinAndMaxTemp = Tuple.Create(0.0, 1200.0);
        }

        private List<BoarderEdge> FindBoarderNeighbors(int i)
        {
            int k = 0;
            List<BoarderEdge> bn = new List<BoarderEdge>();
            int counter = 0;
            while (counter < 2 && k < BoarderEdges.Count)
            {
                if (BoarderEdges[k].TopologyVertices.Item1 == i ||
                    BoarderEdges[k].TopologyVertices.Item2 == i)
                {
                    bn.Add(BoarderEdges[k]);
                    counter++;
                }
                k++;
            }
            return bn;
        }
    }
}
