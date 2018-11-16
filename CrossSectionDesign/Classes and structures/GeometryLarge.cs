using System.Collections.Generic;
using System.Threading;
using CrossSectionDesign.Abstract_classes;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Interfaces;
using Rhino;
using Rhino.Geometry;

namespace CrossSectionDesign.Classes_and_structures
{
    [System.Runtime.InteropServices.Guid("D11BBCBB-37B5-4C31-BF70-97A498D12514")]
    public class GeometryLarge : CountableUserData
    {
        public CrossSection OwnerCrossSection { get; set; }
        private Brep _baseBrep;
        public int CrosecId { get; set; }
        public Point3d Centroid { get; set; }
        public List<Curve> BaseCurves { get; set; } = new List<Curve>();
        public Brep BaseBrep { get { return _baseBrep; } set {
                value.Transform(UnitTransform);
                AreaMassProp = AreaMassProperties.Compute(value);
                _baseBrep = value;
                CreateCalcMesh();

            } }
        public Material Material { get; set; }
        public CalcMesh CalcMesh { get; private set; }
        public AreaMassProperties AreaMassProp { get; private set; }


        private void CreateCalcMesh()
        {
            MeshingParameters mp = new MeshingParameters
            {
                SimplePlanes = false,
                GridMaxCount = 300,
                GridMinCount = 400,
            };

            Mesh[] temp = Mesh.CreateFromBrep(_baseBrep, mp);
            CalcMesh = new CalcMesh(temp[0], this);
            CalcMesh.Material = Material;
        }


        //Constructor
        public GeometryLarge()
        {
            Id = _idCounter;
            _idCounter++;
        }

        //Constructor
        public GeometryLarge(MaterialType materialType, string materialName, Brep baseBrep, CrossSection cs)
        {
            UnitTransform = Transform.Scale(cs.AddingCentroid, ProjectPlugIn.Instance.Unitfactor);
            if (materialType == MaterialType.Concrete)
                Material = new ConcreteMaterial(materialName, cs.HostBeam);
            else if (materialType == MaterialType.Steel)
                Material = new SteelMaterial(materialName,SteelType.StructuralSteel, cs.HostBeam);
            BaseBrep = baseBrep;
            OwnerCrossSection = cs;
            InverseUnitTransform = Transform.Scale(cs.AddingCentroid, 1.0/ProjectPlugIn.Instance.Unitfactor);
            Id = _idCounter;
            _idCounter++;

        }

        public Brep GetModelUnitBrep()
        {
            Brep b = BaseBrep.DuplicateBrep();
            b.Transform(InverseUnitTransform);
            return b;
        }





        public override string Description => "Segment with properties";


        // This class information will be written to the .3dm file
        public override bool ShouldWrite => true;


        protected override void OnDuplicate(Rhino.DocObjects.Custom.UserData source)
        {
            GeometryLarge src = source as GeometryLarge;
            if (src != null)
            {
                BaseBrep = src.BaseBrep;
            }
        }

        protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
            UnitTransform = (Transform)dict["UnitTransform"];
            InverseUnitTransform = (Transform)dict["InverseUnitTransform"];

            _baseBrep = (Brep) dict["baseBrep"];
            AreaMassProp = AreaMassProperties.Compute(_baseBrep);
            CreateCalcMesh();


            int noBaseCurves = dict.GetInteger("NoBaseCurves");
            
            for (int i = 0; i < noBaseCurves; i++)
            {
                BaseCurves.Add((Curve)dict["BaseCurve" + i]);
            }


            Id = (int) dict["Id"];
            if ((string)dict["Material"] == "Concrete")
                Material = new ConcreteMaterial((string) dict["MaterialStrength"], null);
            else if ((string)dict["Material"] == "Steel")
                Material = new SteelMaterial((string)dict["MaterialName"],SteelType.StructuralSteel, null);
            //TODO Add extra materials
            else
                Material = new ConcreteMaterial("C30/37", null);
                
            return true;
        }

        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            var dict = new Rhino.Collections.ArchivableDictionary(20181023, "Values");
            dict.Set("UnitTransform", UnitTransform);
            dict.Set("InverseUnitTransform", InverseUnitTransform);
            dict.Set("baseBrep", BaseBrep);
            dict.Set("Id", Id);
            if (OwnerCrossSection != null)
                dict.Set("CrossSectionId", OwnerCrossSection.Id);
            else
                dict.Set("BeamId", -1);
            dict.Set("NoBaseCurves", BaseCurves.Count);
            for (int i = 0; i < BaseCurves.Count; i++)
            {
                dict.Set("BaseCurve"+i, BaseCurves[i]);
            }
            
            if (Material.GetType() == typeof(ConcreteMaterial))
            {
                ConcreteMaterial material = Material as ConcreteMaterial;
                dict.Set("Material", "Concrete");
                dict.Set("MaterialStrength", material.StrengthClass);

            }
            else if (Material.GetType() == typeof(SteelMaterial))
            {
                SteelMaterial material = Material as SteelMaterial;
                
                dict.Set("Material", "Steel");
                dict.Set("MaterialName", material.StrengthClass);
            }
            
            archive.WriteDictionary(dict);

            return true;
        }

    }
}
