using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class FaceNeighbor
    {
        public double BoarderLength { get; private set; }
        public double Distance { get; private set; }
        public int Number { get; private set; }

        public FaceNeighbor(double boarderLength, double distance, int number)
        {
            BoarderLength = boarderLength;
            Distance = distance;
            Number = number;
        }

    }
}
