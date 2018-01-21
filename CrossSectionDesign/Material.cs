namespace CrossSectionDesign
{
    public abstract class Material
    {
        

        public virtual double E { get; set; }

        public string Name { get; set; } //Name of the material

        public abstract double Stress(double strain);
    }
}
