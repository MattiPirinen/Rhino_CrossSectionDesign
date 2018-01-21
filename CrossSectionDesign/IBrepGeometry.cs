using Rhino.Geometry;

namespace CrossSectionDesign
{
    public interface IBrepGeometry
    {
        Brep BrepGeometry { get; set; }
        AreaMassProperties AreaMassProp { get;}
        Material Material { get; set; }
        double Stress { get; set; }

    }
}