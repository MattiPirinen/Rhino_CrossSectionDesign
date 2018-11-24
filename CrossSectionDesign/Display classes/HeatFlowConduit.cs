using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Interfaces;
using MoreLinq;
using Rhino.Display;
using Rhino.Geometry;

namespace CrossSectionDesign.Display_classes
{
    public class HeatFlowConduit : Rhino.Display.DisplayConduit
    {
        public bool ForceMaxAndMin { get; set; } = false;
        public Dictionary<HeatResultType,bool> ShownResults { get; set; } = new Dictionary<HeatResultType, bool> {
            {HeatResultType.HeatFlow, true },
            {HeatResultType.Temperature, true}
        };

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

                CrossSection cs = ProjectPlugIn.Instance.CurrentBeam.CrossSec;

                if (cs.CalcMesh == null)
                    return;

                Mesh calcMesh = new Mesh();
                calcMesh.CopyFrom(cs.CalcMesh);
                calcMesh.Transform(ProjectPlugIn.Instance.CurrentBeam.CrossSec.InverseUnitTransform);

                if (ShownResults[HeatResultType.HeatFlow])
                {

                    foreach (MeshSegment ms in cs.CalcMesh.MeshSegments)
                    {
                        Point3d p = new Point3d(ms.Centroid);
                        p.Transform(cs.InverseUnitTransform);
                        Vector3d flow = ms.HeatFlow * cs.CalcMesh.HeatFlowFactor;
                        Line l = new Line(p - flow / 2, p + flow / 2);
                        e.Display.DrawArrow(l, Color.Black);
                    }
                }



                if (ShownResults[HeatResultType.Temperature])
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
