using CrossSectionDesign.Classes_and_structures;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrossSectionDesign.Static_classes
{
    public static class MeshManipulationTools
    {
        public static Mesh[] CreateExtrudedMesh(Mesh m, Vector3d extrudeDirection, List<double> extrudeLengths)
        {
            if (extrudeLengths.Count != m.Faces.Count) { MessageBox.Show("Incorrect ExtrudeLengths!"); return null; }
            int i = 0;

            List<Mesh> meshList = new List<Mesh>();
            foreach (MeshFace mf in m.Faces)
            {
                Mesh newMesh = new Mesh();
                List<Point3d> points = new List<Point3d>();
                
                //Bottom face
                if (mf.IsTriangle)
                {
                    newMesh.Vertices.Add(m.Vertices[mf.A]);
                    newMesh.Vertices.Add(m.Vertices[mf.B]);
                    newMesh.Vertices.Add(m.Vertices[mf.C]);
                    newMesh.Vertices.Add(new Point3d(m.Vertices[mf.A]) + extrudeLengths[i] * extrudeDirection);
                    newMesh.Vertices.Add(new Point3d(m.Vertices[mf.B]) + extrudeLengths[i] * extrudeDirection);
                    newMesh.Vertices.Add(new Point3d(m.Vertices[mf.C]) + extrudeLengths[i] * extrudeDirection);

                    newMesh.Faces.AddFace(new MeshFace(0, 1, 2));
                    newMesh.Faces.AddFace(new MeshFace(0, 1, 4, 3));
                    newMesh.Faces.AddFace(new MeshFace(1, 2, 5, 4));
                    newMesh.Faces.AddFace(new MeshFace(2, 0, 3, 5));
                    newMesh.Faces.AddFace(new MeshFace(3, 4, 5));
                }
                else
                {
                    newMesh.Vertices.Add(m.Vertices[mf.A]);
                    newMesh.Vertices.Add(m.Vertices[mf.B]);
                    newMesh.Vertices.Add(m.Vertices[mf.C]);
                    newMesh.Vertices.Add(m.Vertices[mf.D]);
                    newMesh.Vertices.Add(new Point3d(m.Vertices[mf.A]) + extrudeLengths[i] * extrudeDirection);
                    newMesh.Vertices.Add(new Point3d(m.Vertices[mf.B]) + extrudeLengths[i] * extrudeDirection);
                    newMesh.Vertices.Add(new Point3d(m.Vertices[mf.C]) + extrudeLengths[i] * extrudeDirection);
                    newMesh.Vertices.Add(new Point3d(m.Vertices[mf.D]) + extrudeLengths[i] * extrudeDirection);

                    newMesh.Faces.AddFace(new MeshFace(0, 1, 2, 4));
                    newMesh.Faces.AddFace(new MeshFace(0, 1, 5, 4));
                    newMesh.Faces.AddFace(new MeshFace(1, 2, 6, 5));
                    newMesh.Faces.AddFace(new MeshFace(2, 3, 7, 6));
                    newMesh.Faces.AddFace(new MeshFace(3, 0, 4, 7));
                    newMesh.Faces.AddFace(new MeshFace(5, 6, 7, 8));
                }
                newMesh.Normals.ComputeNormals();
                newMesh.UnifyNormals();
                meshList.Add(newMesh);
            }
            return meshList.ToArray();


        
        }

        public static bool ModifyFaceMesh(ref Mesh m, Vector3d extrudeDirection, double extrudeLength)
        {
            MeshFace mf;
            if (m.Faces.Count == 0) { MessageBox.Show("Incorrect mesh face amount.!"); return false; }
            else if (m.Faces.Count != 1)
            {
                
                Point3f[] temp;
                if (m.Faces[0].IsQuad)
                    temp = m.Vertices.Take(4).ToArray();
                else
                    temp = m.Vertices.Take(3).ToArray();
                m.Vertices.Clear();
                m.Vertices.AddVertices(temp);

                mf = m.Faces[0];
                m.Faces.Clear();
                m.Faces.AddFace(new MeshFace(0,1,2,3));
            }


            mf = m.Faces[0];

            if (m.Faces[0].IsTriangle)
            {
                m.Vertices.Add(new Point3d(m.Vertices[mf.A]) + extrudeLength * extrudeDirection);
                m.Vertices.Add(new Point3d(m.Vertices[mf.B]) + extrudeLength * extrudeDirection);
                m.Vertices.Add(new Point3d(m.Vertices[mf.C]) + extrudeLength * extrudeDirection);

                m.Faces.AddFace(new MeshFace(0, 1, 4, 3));
                m.Faces.AddFace(new MeshFace(1, 2, 5, 4));
                m.Faces.AddFace(new MeshFace(2, 0, 3, 5));
                m.Faces.AddFace(new MeshFace(3, 4, 5));
            }
            else
            {
                m.Vertices.Add(new Point3d(m.Vertices[mf.A]) + extrudeLength * extrudeDirection);
                m.Vertices.Add(new Point3d(m.Vertices[mf.B]) + extrudeLength * extrudeDirection);
                m.Vertices.Add(new Point3d(m.Vertices[mf.C]) + extrudeLength * extrudeDirection);
                m.Vertices.Add(new Point3d(m.Vertices[mf.D]) + extrudeLength * extrudeDirection);

                m.Faces.AddFace(new MeshFace(0, 1, 5, 4));
                m.Faces.AddFace(new MeshFace(1, 2, 6, 5));
                m.Faces.AddFace(new MeshFace(2, 3, 7, 6));
                m.Faces.AddFace(new MeshFace(3, 0, 4, 7));
                m.Faces.AddFace(new MeshFace(4, 5, 6, 7));
            }
            m.UnifyNormals();
            m.Normals.ComputeNormals();
            return true;
        }

        public static Mesh CreateExtrudedMesh(Mesh m, Vector3d extrudeDirection, double extrudeLength)
        {

            Mesh newMesh = new Mesh();
            newMesh.CopyFrom(m);
            int faceCount = newMesh.Faces.Count;
            int verticeCount = newMesh.Vertices.Count;


            Polyline[] p = m.GetNakedEdges();
            if (p.Length != 1) return null;
            Polyline pl = p[0];
            List<int> indices = new List<int>();
            foreach (Point3d point in pl)
            {
                int i = 0;
                while (!isApproxEqual(m.Vertices[i], point, ProjectPlugIn.Instance.ActiveDoc.ModelAbsoluteTolerance))
                    i++;
                indices.Add(i);
            }
            for (int i = 0; i < verticeCount; i++)
            {
                newMesh.Vertices.Add(new Point3d(newMesh.Vertices[i]) + extrudeDirection * extrudeLength);
            }
            
            for (int i = 0; i < faceCount; i++)
            {
                if (newMesh.Faces[i].IsQuad)
                {
                    newMesh.Faces.AddFace(new MeshFace(
                        newMesh.Faces[i].A + verticeCount,
                        newMesh.Faces[i].B + verticeCount,
                        newMesh.Faces[i].C + verticeCount,
                        newMesh.Faces[i].D + verticeCount));
                }
                else
                {
                    newMesh.Faces.AddFace(new MeshFace(
                        newMesh.Faces[i].A + verticeCount,
                        newMesh.Faces[i].B + verticeCount,
                        newMesh.Faces[i].C + verticeCount));
                }
                
            }


            for (int i = 0; i < indices.Count-1; i++)
            {

                newMesh.Faces.AddFace(new MeshFace(
                    indices[i],
                    indices[i+1],
                    indices[i+1]+verticeCount,
                    indices[i]+ verticeCount));
            }
            newMesh.UnifyNormals();
            newMesh.Normals.ComputeNormals();

            return newMesh;
        }

        static private bool isApproxEqual(Point3d a, Point3d b, double tolerance)
        {
            return new Vector3d(a - b).Length < tolerance;
        }

    }
}
