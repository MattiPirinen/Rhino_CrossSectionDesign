using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Material = CrossSectionDesign.Material;

namespace CrossSectionDesign
{

    [System.Runtime.InteropServices.Guid("13129E11-AF5B-48AA-BCEB-C89705C643DF")]
    public class Reinforcement : Rhino.DocObjects.Custom.UserData, IBrepGeometry
    {
        //Private variables
        private CrossSection _croSec;
        private int _diameter;

        public Material Material { get; set; }

        public double Stress { get; set; }

        //Properties
        public Point3d BasePoint { get; set; }

        public int Diameter {
            get
            {
                return _diameter;
            }
            set
            {
                if (!(value.GetType() == typeof(int)))
                {
                    Rhino.RhinoApp.WriteLine("Give the diameter as whole number.");
                    return;
                }
                _diameter = value;
                OutLine = new Circle(BasePoint, _diameter/2).ToNurbsCurve();
                AreaMassProp = AreaMassProperties.Compute(OutLine);
                BrepGeometry = Brep.CreatePlanarBreps(OutLine)[0];
            }
        }
        public AreaMassProperties AreaMassProp { get; private set; }
        public Curve OutLine { get; set; }



        public Brep BrepGeometry { get; set; }

        public override string Description => "Geometry representing reinforcing steel";


        // This class information will be written to the .3dm file
        public override bool ShouldWrite => true;

        protected override void OnDuplicate(Rhino.DocObjects.Custom.UserData source)
        {
            if (source is Reinforcement src)
            {
                BasePoint = src.BasePoint;
                _diameter = src._diameter;
                OutLine = src.OutLine;
            }
        }

 
        protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
            BasePoint = (Point3d)dict["Point"];
            OutLine = (Curve)dict["OutLine"];
            Diameter = (int)dict["Diameter"];

            double fyk = (double) dict["MaterialStrength"];
            //TODO add a coorect material assignment
            Material = new SteelMaterial((string)dict["MaterialName"]);

            return true;
        }

        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            var dict = new Rhino.Collections.ArchivableDictionary(20171031, "Values");
            dict.Set("Point", BasePoint);
            dict.Set("OutLine", OutLine);
            dict.Set("Diameter", _diameter);


            if (Material is SteelMaterial material) dict.Set("MaterialName", material.Name);
            else
            {
                dict.Set("MaterialName", "B500B");
                RhinoApp.WriteLine("No material was assigned to reinforcement.");
            }


            archive.WriteDictionary(dict);

            return true;
        }



    }
}
