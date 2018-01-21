using Rhino.Geometry;

namespace CrossSectionDesign
{
    public class GeometrySegment: IBrepGeometry
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
    }
}