using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrossSectionDesign.Enumerates;
using Rhino.Geometry;
namespace CrossSectionDesign.Classes_and_structures
{
    public class LoadCase
    {
        public Plane NeutralAxis { get; set; } 
        public LimitState Ls { get; set; }
        public string Name { get; set; }
        public Polyline NMCurve { get; set; } = new Polyline();
        public Tuple<double,double> strainAtMinAndMax { get; set; }
        public Plane LoadPlane { get; set; }
        public Beam HostBeam { get; set; }
        public bool IsDisplayed { get; set; } = false;

        public void ClearResults()
        {
            NMCurve = new Polyline();
            LoadPlane = Plane.Unset;
        }

    }
}
