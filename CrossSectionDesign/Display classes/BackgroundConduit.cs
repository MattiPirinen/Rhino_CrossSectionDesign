using System;
using System.Collections.Generic;
using System.Drawing;
using CrossSectionDesign.Classes_and_structures;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;

namespace CrossSectionDesign.Display_classes
{
    public class BackGroundConduit : Rhino.Display.DisplayConduit
    {

        enum Material
        {
            Steel = 0,
            Concrete = 1
        }

        private readonly Dictionary<Material,Color> _colors = new Dictionary<Material, Color>()
        {
            {Material.Concrete,Color.Gray},
            {Material.Steel,Color.Black }
        };



        public Tuple<List<Brep>, List<Brep>> GetReinforcementBreps()
        {
            List<Brep> reinforcement = new List<Brep>();
            List<Brep> selected = new List<Brep>();
            RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                if (list.Find(typeof(Reinforcement)) != null)
                {
                    
                    Reinforcement tempValue = list.Find(typeof(Reinforcement)) as Reinforcement;
                    if (tempValue != null && tempValue.Selected)
                    {
                        selected.Add(tempValue.GetModelUnitBrep());
                    }
                        
                    else
                        reinforcement.Add(tempValue.GetModelUnitBrep());
                }
                     
            }

            return Tuple.Create(reinforcement,selected);
        }

        //This method will return all the breps in the geometry larges saved in the geometrys of the model. 
        // the return type is tuple where first value is the list of concrete breps and the second value
        // is a list of steel breps
        public Tuple<List<Brep>, List<Brep>, List<Brep>> GetGeometryLarges()
        {
            List<Brep> concreteBreps = new List<Brep>();
            List<Brep> steelBreps = new List<Brep>();
            List<Brep> selectedBreps = new List<Brep>();

            RhinoObject[] objs = ProjectPlugIn.Instance.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;

                if (!(list.Find(typeof(GeometryLarge)) is GeometryLarge temp))
                    temp = list.Find(typeof(RectangleGeometryLarge)) as GeometryLarge;

                if (temp != null && temp.Selected == true)
                    selectedBreps.Add(temp.GetModelUnitBrep());
                else if (temp != null && temp.Material.GetType() == typeof(ConcreteMaterial))
                    concreteBreps.Add(temp.GetModelUnitBrep());
                else if (temp != null && temp.Material.GetType() == typeof(SteelMaterial))
                    steelBreps.Add(temp.GetModelUnitBrep());
                
                    
                    
            }

            return new Tuple<List<Brep>, List<Brep>, List<Brep>>(concreteBreps,steelBreps,selectedBreps);
        }




        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
           // base.CalculateBoundingBox(e);
           // BoundingBox box = CreateBoundingBox();
           // e.IncludeBoundingBox(box);
    }

        protected override void PostDrawObjects(DrawEventArgs e)
        {
            base.PostDrawObjects(e);

            List<Brep> concreteBreps = new List<Brep>();
            List<Brep> steelBreps = new List<Brep>();
            List<Brep> selectedBreps = new List<Brep>();

            Tuple<List<Brep>, List<Brep>, List<Brep>> temp1 = GetGeometryLarges();
            concreteBreps.AddRange(temp1.Item1);
            steelBreps.AddRange(temp1.Item2);
            selectedBreps.AddRange(temp1.Item3);

            Tuple<List<Brep>, List<Brep>> temp2 = GetReinforcementBreps();
            steelBreps.AddRange(temp2.Item1);
            selectedBreps.AddRange(temp2.Item2);

            foreach (Brep brep in concreteBreps)
            {
                e.Display.DrawBrepShaded(brep, new DisplayMaterial(_colors[Material.Concrete]));
            }

            foreach (Brep brep in steelBreps)
            {
                e.Display.DrawBrepShaded(brep, new DisplayMaterial(_colors[Material.Steel]));
            }
            foreach (Brep brep in selectedBreps)
            {
                e.Display.DrawBrepShaded(brep, new DisplayMaterial(Color.Red));
            }

        }


        protected override void DrawForeground(DrawEventArgs e)
        {
            base.DrawForeground(e);
        }

        /*
        private BoundingBox CreateBoundingBox()
        {

            var box = new BoundingBox(new Point3d[] {new Point3d(0,0,0)});
            if (DisplayBrepConcrete != null)
            {
                foreach (Brep brep in DisplayBrepConcrete)
                {
                    box = BoundingBox.Union(box, brep.GetBoundingBox(false));
                }
            }

            if (DisplayBrepSteel != null)
            {
                foreach (Brep brep in DisplayBrepSteel)
                {
                    box = BoundingBox.Union(box, brep.GetBoundingBox(false));
                }
            }



            return box;
        }
        */
    }
}