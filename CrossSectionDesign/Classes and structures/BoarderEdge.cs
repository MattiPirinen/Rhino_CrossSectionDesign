using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class BoarderEdge
    {

        public bool IsStandardFire { get; set; } = true;
        public double Distance { get; private set; }
        public bool IsConductive { get; set; } = false;
        public double BoarderLength { get; set; }
        public Point3d Centroid { get; set; }
        public Tuple<int, int> TopologyVertices { get; set; }
        public int ConnectedFace { get; set; }
        public int TopologyEdgeIndex { get; set; }
        public double ConvCoef { get; set; }
        public Vector3d FlowDirection { get; set; } 

        public static double StandardFireTemp(double time)
        {
            return 20 + 345 * Math.Log10(8 * (time / 60) + 1);
        }



        public double Temperature(double time)
        {
            if (IsStandardFire)
                return StandardFireTemp(time);
            else
                return 20.0;
        }

        public BoarderEdge(double distance, double boarderLength, Point3d centroid, Tuple<int, int> topologyVertices, int connectedFace,
            int topologyEdgeIndex, double convCoef, Vector3d flowDirection)
        {
            Distance = distance;
            BoarderLength = boarderLength;
            Centroid = centroid;
            TopologyVertices = topologyVertices;
            ConnectedFace = connectedFace;
            TopologyEdgeIndex = topologyEdgeIndex;
            ConvCoef = convCoef;
            FlowDirection = flowDirection;
        }







    }
}
