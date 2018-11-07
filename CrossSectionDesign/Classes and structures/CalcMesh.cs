using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using CrossSectionDesign.Interfaces;
using CrossSectionDesign.Abstract_classes;

namespace CrossSectionDesign.Classes_and_structures
{
    public class CalcMesh:Mesh
    {
        public List<MeshSegment> MeshSegments { get; set; } = new List<MeshSegment>();
        public Material Material { get; set; }
        public CalcMesh(Mesh m, GeometryLarge gl)
        {
            Append(m);
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
                MeshSegments.Add(new MeshSegment(c, area, gl, tempMesh));
                i++;
            }
        }
        
    }
}
