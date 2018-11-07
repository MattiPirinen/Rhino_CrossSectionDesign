using System.Collections.Generic;
using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Interfaces;
using Rhino.Geometry;

namespace CrossSectionDesign.Classes_and_structures
{
    public class GeometrySegment: ICalcGeometry
    {
        public GeometrySegment(Brep brep, Material material)
        {
            BrepGeometry = brep;
            Material = material;
        }

        private Brep _brepSegment;
        public Brep BrepGeometry
        {
            get { return _brepSegment; }
            set
            {
                _brepSegment = value;
                AreaMassProp = AreaMassProperties.Compute(_brepSegment);
            }
        }
        public AreaMassProperties AreaMassProp { get; private set; }
        public Material Material { get; set; }
        public double Stress { get; set; }

        public Point3d Centroid => throw new System.NotImplementedException();

        public double Area => throw new System.NotImplementedException();

        public Dictionary<LoadCase, double> Stresses { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Mesh ResultMesh { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Mesh GeometryMesh { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public ICalcGeometry DeepCopy()
        {
            GeometrySegment gs = new GeometrySegment(BrepGeometry, Material)
            {
                Stress = Stress
            };

            return gs;
        }

        public void ModifyMesh(double distance)
        {
            throw new System.NotImplementedException();
        }
    }
}