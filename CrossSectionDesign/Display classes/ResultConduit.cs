using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Interfaces;
using Rhino.Display;
using Rhino.Geometry;

namespace CrossSectionDesign.Display_classes
{
    public class ResultConduit: Rhino.Display.DisplayConduit
    {
        public bool ForceMaxAndMin { get; set; } = false;


        public ResultConduit()
        {
        }



        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);
            BoundingBox box = CreateBoundingBox();
            e.IncludeBoundingBox(box);
        }

        
        private BoundingBox CreateBoundingBox()
        {

            BoundingBox bb = new BoundingBox();
            if (ProjectPlugIn.Instance.CurrentBeam != null &&
                ProjectPlugIn.Instance.CurrentBeam.CrossSec != null)
            {
                Beam b = ProjectPlugIn.Instance.CurrentBeam;
                CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;
                List<ICalcGeometry> calcGeometries = new List<ICalcGeometry>();
                calcGeometries.AddRange(cs.GetReinforcements());
                List<GeometryLarge> glList = cs.GetGeometryLarges();
                glList.ForEach(gl => calcGeometries.AddRange(gl.CalcMesh.MeshSegments));

                foreach (ICalcGeometry item in calcGeometries)
                {
                    if (item.ResultMesh != null)
                        bb.Union(item.ResultMesh.GetBoundingBox(false));
                }
            }
            
            return bb;
        }
        
        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);
            

            if (ProjectPlugIn.Instance.CurrentBeam != null)
            {
                Beam b = ProjectPlugIn.Instance.CurrentBeam;
                if (b.CurrentLoadCase == null) return;
                CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;
                List<ICalcGeometry> calcGeometries = new List<ICalcGeometry>();
                calcGeometries.AddRange(cs.GetReinforcements());
                List<GeometryLarge> glList = cs.GetGeometryLarges();
                glList.ForEach(gl => calcGeometries.AddRange(gl.CalcMesh.MeshSegments));

                Tuple<double, double> minAndMax = cs.MinAndMaxStress ?? Tuple.Create(0.0, 0.0);

                double minValue = minAndMax.Item1;
                double maxValue = minAndMax.Item2;

                foreach (ICalcGeometry icalcG in calcGeometries)
                {
                    if (b.CrossSec.MaterialResultShown == Enumerates.MaterialType.Concrete &&
                        icalcG.Material.GetType() == typeof(ConcreteMaterial) ||
                        b.CrossSec.MaterialResultShown == Enumerates.MaterialType.Steel &&
                        icalcG.Material.GetType() == typeof(SteelMaterial))
                    {
                        ColorRGB color;
                        Mesh m = icalcG.ResultMesh;
                        if (m == null) continue;
                        double value = 0.7 - 0.7 * (icalcG.Stresses[b.CurrentLoadCase] - minValue) / (maxValue - minValue);
                        if (value < 0 || value > 0.7)
                            color = Utils.HSL2RGB(1, 1, 1);
                        else
                            color = Utils.HSL2RGB(value, 1, 0.5);
                        
                        m.VertexColors.Clear();
                        for (int k = 0; k < m.Vertices.Count; k++)
                        {
                            m.VertexColors.Add(color);
                        }
                        e.Display.DrawMeshFalseColors(m);

                    }


                }

            }


        }
        /*
        private void calcMaxAndMin()
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;
            
            foreach (Tuple<Brep, double> i in Breps)
            {
                maxValue = Math.Max(maxValue, i.Item2);
                minValue = Math.Min(minValue, i.Item2);
            }

            _maxValue = maxValue;
            _minValue = minValue;

        }

    */
    }

}
