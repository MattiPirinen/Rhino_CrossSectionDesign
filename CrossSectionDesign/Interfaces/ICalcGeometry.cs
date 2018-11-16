using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Classes_and_structures;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace CrossSectionDesign.Interfaces
{
    public interface ICalcGeometry
    {
        Point3d Centroid { get; }
        double Area{ get;}
        Mesh GeometryMesh { get; set; }
        Mesh ResultMesh { get; set; }
        Material Material { get; set; }
        Dictionary<LoadCase, double> Stresses { get; set; }
        ICalcGeometry DeepCopy();
        Mesh GetModelScaleResultMesh();
        void ModifyMesh(double distance);
    }
}