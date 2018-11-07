using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;

namespace CrossSectionDesign.Static_classes
{
    public static class CreateGeometryLarge
    {
        public static GeometryLarge CreateGeometry(MaterialType mType,string mName)

        {

            RhinoDoc doc = RhinoDoc.ActiveDoc;

            //Allow user to pick multiple objects
            const Rhino.DocObjects.ObjectType geometryFilter = Rhino.DocObjects.ObjectType.Curve;
            Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject()
            {
                GeometryFilter = geometryFilter,
                GroupSelect = true,
                SubObjectSelect = false,
                DeselectAllBeforePostSelect = false
            };
            go.SetCommandPrompt("Pick one or two closed curves.");
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

            ObjRef[] objs = go.Objects();
            if (objs == null)
            {
                RhinoApp.WriteLine("No curves were selected");
                return null;
            }

            List<Curve> selectedCurves = new List<Curve>();
            
            foreach (ObjRef objRef in objs)
            {
                if (objRef.Curve() == null)
                {
                    RhinoApp.WriteLine("One of the selected objects was invalid.");
                    continue;
                }
                else if (!objRef.Curve().IsClosed)
                {
                    RhinoApp.WriteLine("One of the selected curves was not closed.");
                    continue;
                }
                selectedCurves.Add(objRef.Curve());
            }

            GeometryLarge larg;
            //if the curve is not closed do nothing
            switch (selectedCurves.Count)
            {
                case 0:
                    Rhino.RhinoApp.WriteLine("No valid geometries was found.");
                    return null;
                case 1:

                    larg = DrawAndSaveUserAttr(Brep.CreatePlanarBreps(selectedCurves)[0],selectedCurves, doc, mType, mName);
                    break;
                case 2:
                    Brep brep = CreateHollowBrep(selectedCurves[0], selectedCurves[1]);
                    if (brep != null)
                        larg = DrawAndSaveUserAttr(brep,selectedCurves, doc, mType, mName);
                    else
                    {
                        RhinoApp.WriteLine("The curves were not inside one another.");
                        return null;
                    }
                    break;
                default:
                    return null;



            }
            /*
            foreach (ObjRef objRef in objs)
            {
                doc.Objects.Delete(objRef, false);
            }
            */
            return larg;

        }


        public static Rectangle3d CreateOutline(Point3d centroid,double width, double height,int rotation)
        {
            Point3d pointA = centroid - new Vector3d(width / 2, -height / 2, 0);
            Point3d pointB = centroid + new Vector3d(width / 2, -height / 2, 0);
            Rectangle3d rect = new Rectangle3d(Plane.WorldXY, pointA, pointB);
            double rotRadians = (double) rotation / 360 * 2 * Math.PI;
            Transform trans = Transform.Rotation(rotRadians, Vector3d.ZAxis, centroid);
            rect.Transform(trans);
            return rect;
        }

        public static Brep CreateHollowBrep(Curve curve1, Curve curve2)
        {
            Brep brep1 = Brep.CreatePlanarBreps(new[] { curve1 })[0];
            Brep brep2 = Brep.CreatePlanarBreps(new[] { curve2 })[0];
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            double area = Brep.CreateBooleanUnion(new[] { brep1, brep2 }, 0.001)[0].GetArea();
            double brep1Area = brep1.GetArea();
            double brep2Area = brep2.GetArea();
            if (area > 0.999 * brep1Area && area < 1.001 * brep1Area)
            {
                return Brep.CreateBooleanDifference(brep1, brep2, doc.ModelAbsoluteTolerance)[0];
                
            }

            else if (area > 0.999 * brep2Area && area < 1.001 * brep2Area)
            {
                return Brep.CreateBooleanDifference(brep1, brep2, doc.ModelAbsoluteTolerance)[0];
            }
            else
            {
                return null;
            }

        }



        private static int createLayer(RhinoDoc doc,MaterialType mType)
        {
            int index;
            if (mType == MaterialType.Concrete)
            {
                Color color = Color.FromArgb(190, 190, 190);
                doc.Layers.Add("Concrete", color);
                index = doc.Layers.Find("Concrete", true);
            }

            else
            {

                Color color = Color.FromArgb(0, 0, 0);
                doc.Layers.Add("Steel", color);
                index = doc.Layers.Find("Steel", true);
            }
            Layer layer = doc.Layers[index];
            layer.IsLocked = true;
            layer.CommitChanges();
            return index;
        }


        //This method saves the useraddributes into the outlineCurve
        private static GeometryLarge DrawAndSaveUserAttr(Brep brep,List<Curve> baseCurves, RhinoDoc doc,MaterialType mType,string mName)
        {
            ObjectAttributes attr = new ObjectAttributes();
            attr.SetUserString("Name", Enum.GetName(typeof(MaterialType), mType));
            
            attr.SetUserString("infType", "GeometryLarge");
            GetLayerIndex(mType,ref attr);

            GeometryLarge seg = new GeometryLarge(mType, mName, brep, ProjectPlugIn.Instance.CurrentBeam)
            { BaseCurves = baseCurves };
            attr.UserData.Add(seg);
            Guid guid = doc.Objects.AddBrep(brep);
            doc.Objects.ModifyAttributes(guid, attr, true);
            //doc.Objects.AddBrep(brep, attr);

            return seg;

        }

        public static void GetLayerIndex(MaterialType mType, ref ObjectAttributes attr)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            List<Layer> layers = (from layer in doc.Layers
                where layer.Name == Enum.GetName(typeof(MaterialType), mType)
                select layer).ToList<Rhino.DocObjects.Layer>();


            if (layers.Count == 0 || (layers.Count == 1 && layers[0].IsDeleted))
            {
                attr.LayerIndex = createLayer(doc, mType);
            }
            else if (layers.Count == 1)
            {
                attr.LayerIndex = layers[0].LayerIndex;
                //layers[0].IsLocked = true;
                layers[0].CommitChanges();
            }
            else
            {
                RhinoApp.WriteLine("More than one layer with name Concrete excists. Remove one of them.");
            }

        }
    }
}