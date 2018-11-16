using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace CrossSectionDesign.RhinoCommands
{
    
    [System.Runtime.InteropServices.Guid("9a44ed11-7555-493e-9d5e-8a33abcbbef1")]
    public class CreateReinforcement2 : Command
    {
        public CreateReinforcement2()
        {
            Instance = this;
        }

        ///<summary>The only instance of the Create_reinforcement command.</summary>
        public static CreateReinforcement2 Instance { get; private set; }

        public override string EnglishName => "cd_Create_reinforcement";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            const Rhino.DocObjects.ObjectType geometryFilter = Rhino.DocObjects.ObjectType.Point;
            Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
            go.SetCommandPrompt("Pick all the points that you want to change to reinforcement.");
            go.GeometryFilter = geometryFilter;
            go.GroupSelect = true;
            go.SubObjectSelect = false;
            go.EnableClearObjectsOnEntry(false);
            go.EnableUnselectObjectsOnExit(false);
            go.DeselectAllBeforePostSelect = false;

            bool bHavePreselectedObjects = false;
            
            for (;;)
            {
                
                Rhino.Input.GetResult res = go.GetMultiple(1, 0);
                /*
                if (res == Rhino.Input.GetResult.Option)
                {
                    go.EnablePreSelect(false, true);
                    continue;
                }
                else if (res != Rhino.Input.GetResult.Option)
                {
                    return Rhino.Commands.Result.Cancel;
                }
                */
                if (go.ObjectsWerePreselected)
                {
                    bHavePreselectedObjects = true;
                    go.EnablePreSelect(false, true);
                    continue;
                }
                break;

            }

            List<Point3d> points = new List<Point3d>(); 

            if (bHavePreselectedObjects)
            {
                for (int i = 0; i < go.ObjectCount; i++)
                {
                    RhinoObject rhinoObject = go.Object(i).Object();
                    if (null != rhinoObject)
                        
                        rhinoObject.Select(false);
                }
                doc.Views.Redraw();
            }

            int layerIndex = getLayerIndex(doc, "Reinforcement");
            doc.Layers[layerIndex].Color = Color.Black;
            if (layerIndex == 999) return Result.Failure;

            ObjRef[] objects = go.Objects();
            foreach (ObjRef obj in objects)
            {
                Point3d point = obj.Point().Location;

                //TODO add the functionality how to assign different steel materials.
                Reinforcement reinf = new Reinforcement(ProjectPlugIn.Instance.CurrentBeam.CrossSec)
                {
                    Material = new SteelMaterial("B500B",SteelType.Reinforcement, ProjectPlugIn.Instance.CurrentBeam), 
                    Centroid = point,
                    Diameter = 8
                };


                
                
                ObjectAttributes attr = new ObjectAttributes();
                attr.UserData.Add(reinf);
                attr.SetUserString("Name", "Reinforcement");
                attr.LayerIndex = layerIndex;

                //Unused code to create a hatch around the point
                /*
                doc.Objects.AddCurve(reinf.OutLine,attr);
                ObjectAttributes attrHatch = new ObjectAttributes();
                attrHatch.LayerIndex = layerIndex;
                doc.Objects.AddHatch(reinf.Hatch, attrHatch);
                */


                doc.Objects.ModifyAttributes(obj, attr, true);
            }
            


            return Result.Success;
        }
        private int getLayerIndex(RhinoDoc doc, string name)
        {
            
            List<Layer> layers = (from layer in doc.Layers
                                  where layer.Name == name
                                  select layer).ToList<Rhino.DocObjects.Layer>(); ;

            int index = 999;
            if (layers.Count == 0 || (layers.Count == 1 && layers[0].IsDeleted))
            {
                Color color = Color.Black;

                doc.Layers.Add("Reinforcement", color);
                index = doc.Layers.Find("Reinforcement", true);
                Layer layer = doc.Layers[index];
                //layer.IsLocked = true;
                layer.CommitChanges();
            }
            else if (layers.Count == 1)
            {
                index = layers[0].LayerIndex;
                //layers[0].IsLocked = true;
                layers[0].CommitChanges();
            }
            else { RhinoApp.WriteLine("More than one layer with name Reinforcement excists. Remove one of them.");}
            return index;
        }





    }

}
