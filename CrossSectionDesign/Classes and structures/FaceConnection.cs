using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class FaceConnection
    {
        public double BoarderLength { get; private set; }
        public double Distance1 { get; private set; } //Distance from the centroid of face1 to the boarder between faces
        public double Distance2 { get; private set; } //Distance from the boarder between faces to the centroid of face2  
        public Tuple<int,int> ConnectedFaces { get; private set; }
        public Vector3d FlowDirection { get; private set; }

        public FaceConnection(double boarderLength, double distance1, double distance2, Tuple<int,int> connectedFaces, 
            Vector3d flowDirection)
        {
            BoarderLength = boarderLength;
            Distance1 = distance1;
            Distance2 = distance2;
            ConnectedFaces = connectedFaces;
            FlowDirection = flowDirection;
        }

    }
}
