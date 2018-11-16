using System;
using System.Drawing;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;

namespace CrossSectionDesign.Static_classes
{
    static class CreateReinforcement
    {
        //Creates reinforcement objects and saves them into the selected point as userData
        public static Reinforcement[] CreateReinforcements(string mName,double diam)
        {
            RhinoDoc doc = ProjectPlugIn.Instance.ActiveDoc;

            //Allow user to pick multiple objects
            const Rhino.DocObjects.ObjectType geometryFilter = Rhino.DocObjects.ObjectType.Point;
            Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject()
            {
                GeometryFilter = geometryFilter,
                GroupSelect = true,
                SubObjectSelect = false,
                DeselectAllBeforePostSelect = false
            };
            go.SetCommandPrompt("Pick all the points that you want to change to reinforcement.");
            go.EnableClearObjectsOnEntry(false);
            go.EnableUnselectObjectsOnExit(false);

            bool bHavePreselectedObjects = false;
            for (;;)
            {
                Rhino.Input.GetResult res = go.GetMultiple(1, 0);
                if (go.ObjectsWerePreselected)
                {
                    bHavePreselectedObjects = true;
                    go.EnablePreSelect(false, true);
                    continue;
                }
                break;
            }

            //Unselects the preselected objects
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

            
            int layerIndex = GetOrCreateLayer(doc, "Reinforcement",Color.Black);
            if (layerIndex == 999) return new Reinforcement[] {};


            Reinforcement[] reinfList = { };
            //Create reinforcement objects for each selected points
            ObjRef[] objects = go.Objects();
            if (objects != null)
                reinfList = CreateReinfObjs(doc, objects, layerIndex,mName,diam);



            return reinfList;

        }

        private static Reinforcement[] CreateReinfObjs(RhinoDoc doc, ObjRef[] objects, int layerIndex, string mName,
            double diam)
        {

            List<Reinforcement> reinfList = new List<Reinforcement>();
            foreach (ObjRef obj in objects)
            {
                if (obj.Point() == null) continue; //Checks that object is a point
                Point3d point = obj.Point().Location;
                Reinforcement reinf = new Reinforcement(ProjectPlugIn.Instance.CurrentBeam.CrossSec)
                {
                    Material = new SteelMaterial(mName,SteelType.Reinforcement, ProjectPlugIn.Instance.CurrentBeam),
                    Centroid = point,
                    Diameter = diam
                };
                reinfList.Add(reinf);
                ObjectAttributes attr = new ObjectAttributes();
                attr.UserData.Add(reinf);
                attr.SetUserString("Name", "Reinforcement");
                attr.SetUserString("infType", "Reinforcement");
                attr.LayerIndex = layerIndex;
                doc.Objects.ModifyAttributes(obj, attr, true);
            }

            return reinfList.ToArray();
        }


        //Gets layer index by name or if the layer does not excist, deletes the layer.
        public static int GetOrCreateLayer(RhinoDoc doc, string name,Color color)
        {

            List<Layer> layers = (from layer in doc.Layers
                where layer.Name == name
                select layer).ToList<Rhino.DocObjects.Layer>();
            ;

            int index = 999;
            if (layers.Count == 0 || (layers.Count == 1 && layers[0].IsDeleted))
            {
                doc.Layers.Add("Reinforcement", color);
                index = doc.Layers.Find("Reinforcement", true);
                Layer layer = doc.Layers[index];

                layer.IsLocked = true;
                layer.CommitChanges();
            }
            else if (layers.Count == 1)
            {
                index = layers[0].LayerIndex;
                layers[0].IsLocked = true;
                layers[0].CommitChanges();
            }
            else
            {
                RhinoApp.WriteLine("More than one layer with name Reinforcement excists. Remove one of them.");
            }

            return index;

        }
    }
}
