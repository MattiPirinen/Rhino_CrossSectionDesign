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
    public class HeatFlowConduit: Rhino.Display.DisplayConduit
    {
        public bool ForceMaxAndMin { get; set; } = false;


        public HeatFlowConduit()
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
                        bb.Union(item.GetModelScaleResultMesh().GetBoundingBox(false));
                }
            }
            
            return bb;
        }
        
        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);
            

            if (ProjectPlugIn.Instance.CurrentBeam != null)
            {

                double minValue = 0;
                double maxValue = 100;

                Beam b = ProjectPlugIn.Instance.CurrentBeam;
                CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;
                List<ICalcGeometry> calcGeometries = new List<ICalcGeometry>();

                List<GeometryLarge> glList = cs.GetGeometryLarges();
                if (glList.Count != 1)
                    return;

                glList[0].CalcMesh.CalculateVertexColors(0.0,101.0);
                Mesh calcMesh = new Mesh();
                calcMesh.CopyFrom(glList[0].CalcMesh);

                calcMesh.Transform(ProjectPlugIn.Instance.CurrentBeam.CrossSec.InverseUnitTransform);
                e.Display.DrawMeshFalseColors(calcMesh);
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
