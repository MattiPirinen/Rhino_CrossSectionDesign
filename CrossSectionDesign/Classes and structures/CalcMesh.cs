using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using CrossSectionDesign.Interfaces;
using CrossSectionDesign.Abstract_classes;
using System.Drawing;

namespace CrossSectionDesign.Classes_and_structures
{
    public class CalcMesh:Mesh
    {
        public int MidIndice { get; set; } 

        public List<MeshSegment> MeshSegments { get; set; } = new List<MeshSegment>();
        public Material Material { get; set; }
        public List<BoarderNeighbor> boarderNeighbors { get; set; } = new List<BoarderNeighbor>();
        public CalcMesh(Mesh m, GeometryLarge gl)
        {


            double maxY = double.MinValue;
            Append(m);

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
            foreach (MeshSegment meshSegment in MeshSegments)
            {
                meshSegment.CalculateNeighbors();
            }





        }
        public void CalculateVertexColors(double minValue, double maxValue)
        {
            double temp = 0;
            for (int i = 0; i < TopologyVertices.Count; i++)
            {

                List<BoarderNeighbor> bn = findBoarderNeighbors(i);
                if (bn.Count == 2 && (bn[0].IsConductive || bn[1].IsConductive))
                {
                    if (bn[0].IsConductive && bn[1].IsConductive)
                    {
                        double l1 = new Vector3d(TopologyVertices[i] - bn[0].Centroid).Length;
                        double l2 = new Vector3d(TopologyVertices[i] - bn[1].Centroid).Length;
                        temp = (bn[0].Temperature * l1 + bn[1].Temperature* l2) / (l1 + l2);
                    }
                    else if (bn[0].IsConductive)
                        temp = bn[0].Temperature;
                    else
                        temp = bn[1].Temperature;
                }
                else
                {
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
                }
                int[] indices = TopologyVertices.MeshVertexIndices(i);

                if  (indices.Length == 1 && indices[0] < VertexColors.Count)
                {
                    double value = 0.7 - 0.7 * (temp - minValue) / (maxValue - minValue);
                    if (value < 0 || value > 0.7)
                        
                        VertexColors[indices[0]] = Utils.HSL2RGB(1, 1, 1);
                    else
                        VertexColors[indices[0]] = Utils.HSL2RGB(value, 1, 0.5);
                }

            }



        }

        private List<BoarderNeighbor> findBoarderNeighbors(int i)
        {
            int k = 0;
            List<BoarderNeighbor> bn = new List<BoarderNeighbor>();
            int counter = 0;
            while (counter < 2 && k < boarderNeighbors.Count)
            {
                if (boarderNeighbors[k].TopologyVertices.Item1 == i ||
                    boarderNeighbors[k].TopologyVertices.Item2 == i)
                {
                    bn.Add(boarderNeighbors[k]);
                    counter++;
                }
                k++;
            }
            return bn;
        }
    }
}
