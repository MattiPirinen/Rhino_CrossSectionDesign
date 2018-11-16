using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Interfaces;
using CrossSectionDesign.Static_classes;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Material = CrossSectionDesign.Abstract_classes.Material;

namespace CrossSectionDesign.Classes_and_structures
{

    [System.Runtime.InteropServices.Guid("13129E11-AF5B-48AA-BCEB-C89705C643DF")]
    public class Reinforcement : CountableUserData, ICalcGeometry
    {
        //Private variables
        private double _diameter;
        private Mesh _resultMesh;
        private Point3d _centroid;

        public double Stress { get; set; }

        //Properties
        public int CroSecId { get; set; }
        public Point3d Centroid { get => _centroid; set { value.Transform(UnitTransform); _centroid = value; } }
        public Material Material { get; set; }
        public double Area { get; private set; }
        public CrossSection OwnerCrossSection { get; set; }


        public double Diameter
        {
            get => _diameter;
            set
            {
                _diameter = value;
                OutLine = new Circle(Centroid, _diameter / 2).ToNurbsCurve();
                Area = Math.PI * Math.Pow(_diameter, 2) / 4;
                BrepGeometry = Brep.CreatePlanarBreps(OutLine)[0];
                CreateMesh();
            }
        }

        private void CreateMesh()
        {
            MeshingParameters mp = new MeshingParameters
            {
                SimplePlanes = false,
                //GridMaxCount = 4,
                //GridMinCount = 4,
                MinimumEdgeLength = Diameter  / 4,
                MaximumEdgeLength = Diameter  / 3
            };


            GeometryMesh = Mesh.CreateFromBrep(BrepGeometry, mp)[0];
        }

        public Curve OutLine { get; set; }

        public Reinforcement(CrossSection ownerCrossSection)
        {
            UnitTransform = Transform.Scale(ownerCrossSection.AddingCentroid, ProjectPlugIn.Instance.Unitfactor);
            InverseUnitTransform = Transform.Scale(ownerCrossSection.AddingCentroid, 1/ProjectPlugIn.Instance.Unitfactor);

            OwnerCrossSection = ownerCrossSection;
        }

        public Reinforcement()
        {

        }

        public Brep GetModelUnitBrep()
        {
            Brep b = BrepGeometry.DuplicateBrep();
            b.Transform(InverseUnitTransform);
            return b;
        }



        public ICalcGeometry DeepCopy()
        {
            Reinforcement rf = new Reinforcement(OwnerCrossSection)
            {
                Diameter = Diameter,
                Centroid = Centroid,
                Material = Material,
                CroSecId = CroSecId,
                Stress = Stress,
                OutLine = OutLine

            };
            return rf;
        }

        public Brep BrepGeometry { get; set; }

        public override string Description => "Geometry representing reinforcing steel";


        // This class information will be written to the .3dm file
        public override bool ShouldWrite => true;

        public Dictionary<LoadCase, double> Stresses { get; set; } = new Dictionary<LoadCase, double>();
        public Mesh ResultMesh { get => _resultMesh; set => _resultMesh = value; }
        public Mesh GeometryMesh { get; set; }

        protected override void OnDuplicate(Rhino.DocObjects.Custom.UserData source)
        {
            if (source is Reinforcement src)
            {
                _centroid = src.Centroid;
                Diameter = src._diameter;
            }
        }

        protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
            UnitTransform = (Transform)dict["UnitTransform"];
            InverseUnitTransform = (Transform)dict["InverseUnitTransform"];
            _centroid = (Point3d)dict["Point"];
            Diameter = (double)dict["Diameter"];
            Id = (int)dict["Id"];
            //TODO add a coorect material assignment

            Material = new SteelMaterial((string)dict["MaterialName"], SteelType.Reinforcement, null);
            return true;
        }

        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            var dict = new Rhino.Collections.ArchivableDictionary(20171031, "Values");
            dict.Set("UnitTransform", UnitTransform);
            dict.Set("InverseUnitTransform", InverseUnitTransform);
            dict.Set("Point", Centroid);
            dict.Set("Diameter", _diameter);
            dict.Set("Id", Id);
            if (Material is SteelMaterial)
                dict.Set("MaterialName", Material.StrengthClass);
            else
            {
                dict.Set("MaterialName", "B500B");
                RhinoApp.WriteLine("No material was assigned to reinforcement.");
            }


            archive.WriteDictionary(dict);

            return true;
        }

        public void ModifyMesh(double distance)
        {
            _resultMesh = MeshManipulationTools.CreateExtrudedMesh(GeometryMesh, Vector3d.ZAxis, distance);
        }

        public Mesh GetModelScaleResultMesh()
        {
            Mesh m = ResultMesh.DuplicateMesh();
            m.Transform(InverseUnitTransform);
            return m;

        }
    }
}
