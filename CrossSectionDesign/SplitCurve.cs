using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;

namespace CrossSectionDesign
{
    [System.Runtime.InteropServices.Guid("a1a585c9-9c60-4972-9991-f1d2997e2eb8")]
    public class SplitCurve : Command
    {
        static SplitCurve _instance;
        public SplitCurve()
        {
            _instance = this;
        }

        ///<summary>The only instance of the MyCommand1 command.</summary>
        public static SplitCurve Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "cd_splitCurve"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            ObjRef sRef;
            Result r = RhinoGet.GetOneObject("Pick closed curve", false, ObjectType.Curve, out sRef);
            if (r != Result.Success || null == sRef) return r;

            RhinoObject obj = sRef.Object();
            obj.Attributes.SetUserString("Name", "Concrete");
            Curve selectedCurve = sRef.Curve();
            

            //if the curve is not closed do nothing
            if (!selectedCurve.IsClosed)
            {
                Rhino.RhinoApp.WriteLine("The curve was not closed!!");
                return Result.Success;
            }

            List<Brep> breps = new List<Brep>();
            List<Curve> cuttedCurves = CurveAndBrepManipulation.cutCurve(selectedCurve, Plane.WorldXY, Axis.XAxis);
            foreach (Curve curve in cuttedCurves)
            {
                breps.AddRange(Brep.CreatePlanarBreps(curve));
            }


            DrawAndSaveUserAttr(breps, doc,obj);
            return Result.Success;
        }

        //this method draws the curves and saves the useraddributes into them
        private void DrawAndSaveUserAttr(List<Brep> breps, RhinoDoc doc,RhinoObject obj)
        {
            //TODO not a fully correctly implemented geometry large initialization
            GeometryLarge seg = new GeometryLarge(MaterialType.Concrete,"C30/37",breps[0]);
            foreach (Brep brep in breps)
            {
                seg.GeometrySegments.Add(new GeometrySegment(brep,seg.Material));
            }
            ObjectAttributes attr = new ObjectAttributes();
            attr.SetUserString("Name", "Concrete");
            List<Layer> layers = (from layer in doc.Layers
                                  where layer.Name == "Concrete"
                                  select layer).ToList<Rhino.DocObjects.Layer>(); ;
            

            if (layers.Count == 0 || (layers.Count == 1 && layers[0].IsDeleted))
            {
                Color color = Color.FromArgb(170,170,170);
                
                doc.Layers.Add("Concrete", color);
                int index = doc.Layers.Find("Concrete", true);
                Layer layer = doc.Layers[index];
                //layer.IsLocked = true;
                attr.LayerIndex = index;
                layer.CommitChanges();
            }
            else if (layers.Count == 1)
            {
                attr.LayerIndex = layers[0].LayerIndex;
                //layers[0].IsLocked = true;
                layers[0].CommitChanges();
            }
            else { RhinoApp.WriteLine("More than one layer with name Concrete excists. Remove one of them.");return; }
            attr.UserData.Add(seg);

            doc.Objects.ModifyAttributes(obj, attr, true);




        }
    }
}
