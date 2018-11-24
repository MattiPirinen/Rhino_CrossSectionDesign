using System;
using System.Collections.Generic;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;

namespace CrossSectionDesign.Abstract_classes
{
    public abstract class Material
    {

        public Beam Bm { get; set; }




        public abstract Material DeepCopy();

        public virtual double E { get; set; }

        public abstract string StrengthClass { get; set; } //Name of the material

        public abstract double Stress(double strain, LimitState ls);

        public abstract double TempStress(double strain, double temperature);

        public abstract Tuple<double,double> FailureStrains { get;}

        public abstract double Density(double temperature);
        public abstract double HeatConductivity(double temperature);
        public abstract double SpecificHeat(double temperature);
        public abstract double CalcTemp(double HeatQuantity);
    }
}
