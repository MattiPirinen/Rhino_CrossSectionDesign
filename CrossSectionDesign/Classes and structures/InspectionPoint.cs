using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    [System.Runtime.InteropServices.Guid("DE3BF359-A023-4658-81B6-45BC10496162")]

    public class InspectionPoint:UserData
    {
        private static int _id = 1;

        public override string Description => "Point to inspect the heat flow results";
        public Point3d Location { get; set; }
        public List<Point2d> Results { get; private set; } = new List<Point2d>();
        public Transform UnitTransform { get; set; }
        public Transform InverseUnitTransform { get; set; }
        public CrossSection OwnerCrossSection { get; set; }
        public int Id { get; private set; }

        public InspectionPoint()
        {
            Id = _id;
            _id++;
        }

        public InspectionPoint(Point3d location, Point3d transformCenter, CrossSection ownerCrossSection)
        {
            Id = _id;
            _id++;

            OwnerCrossSection = ownerCrossSection;
            UnitTransform = Transform.Scale(transformCenter, ProjectPlugIn.Instance.Unitfactor);
            InverseUnitTransform = Transform.Scale(transformCenter, 1 / ProjectPlugIn.Instance.Unitfactor);
            location.Transform(UnitTransform);
            Location = location;
        }

        public Point3d GetModelUnitPoint()
        {
            Point3d p = new Point3d(Location);
            p.Transform(InverseUnitTransform);
            return p;
        }
    }
}
