using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Geometry;


namespace CrossSectionDesign
{
    [System.Runtime.InteropServices.Guid("D11BBCBB-37B5-4C31-BF70-97A498D12514")]
    public class GeometryLarge : Rhino.DocObjects.Custom.UserData
    {

        public Curve BaseCurve
        {
            get { return _baseCurve;}
            set
            {
                _baseCurve = value;
                BaseBrep = Brep.CreatePlanarBreps(value)[0];
            }
        }
        public Brep BaseBrep { get; set; }
        public Material Material { get; private set; }

        //Constructor
        public  GeometryLarge() { }

        //Constructor
        public GeometryLarge(MaterialType materialType, string materialName, Brep baseBrep)
        {
            if (materialType == MaterialType.Concrete)
                Material = new ConcreteMaterial(materialName);
            else if (materialType == MaterialType.Steel)
                Material = new SteelMaterial(materialName);
            BaseBrep = baseBrep;
        }

        public List<IBrepGeometry> GeometrySegments { get; set; } = new List<IBrepGeometry>();

        private CrossSection _croSec;
        private Curve _baseCurve;

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
            int i = 0;
            while (dict.ContainsKey("geoSeg" + i))
            {
                Brep brep = (Brep) dict["gsBrep"+i];
                Material material;

                if (dict[$"gsMaterial{i}"] == "Concrete")
                {
                    material = new ConcreteMaterial((string) dict[$"gsMaterialStrength{i}"]);
                    GeometrySegments.Add(new GeometrySegment(brep, material));
                }
                else if (dict[$"gsMaterial{i}"] == "Steel")
                {
                    material = new SteelMaterial((string)dict[$"gsMaterialName{i}"]);
                    GeometrySegments.Add(new GeometrySegment(brep, material));
                }
                //TODO Add extra materials
                else
                {
                    material = new ConcreteMaterial("C30/37");
                    GeometrySegments.Add(new GeometrySegment(brep, material));
                }
                
                i++;
            }
            return true;
        }

        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            var dict = new Rhino.Collections.ArchivableDictionary(20171031, "Values");

            int i = 0;

            foreach (GeometrySegment geoSeg in GeometrySegments)
            {
                dict.Set("geoSeg" + i, i);
                dict.Set("gsBrep" + i, geoSeg.BrepGeometry);
                if (geoSeg.Material.GetType() == typeof(ConcreteMaterial))
                {
                    ConcreteMaterial material = geoSeg.Material as ConcreteMaterial;

                    dict.Set("gsMaterial" + i, "Concrete");
                    dict.Set("gsMaterialStrength" + i, material.StrenghtClass);

                }
                else if (geoSeg.Material.GetType() == typeof(SteelMaterial))
                {
                    SteelMaterial material = geoSeg.Material as SteelMaterial;
                    dict.Set("gsMaterial" + i, "Steel");
                    dict.Set("gsMaterialName" + i, material.Name);
                }
                
                i++;
            }
            
            archive.WriteDictionary(dict);

            return true;
        }

    }
}
