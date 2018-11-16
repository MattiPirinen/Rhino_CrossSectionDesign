using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class BoarderNeighbor
    {
        public double Temperature { get; set; }
        public double Distance { get; private set; }
        public bool IsConductive { get; set; } = false;
        public double BoarderLength { get; set; }
        public Point3d Centroid { get; set; } 
        public int EdgeIndex { get; set; }
        public Tuple<int,int> TopologyVertices { get; set; }
        public BoarderNeighbor (double distance, double boarderLength, Point3d centroid, int edgeIndex, Tuple<int,int> topologyVertices)
        {
            Distance = distance;
            BoarderLength = boarderLength;
            Centroid = centroid;
            EdgeIndex = edgeIndex;
            TopologyVertices = topologyVertices;
        }


    }
}
