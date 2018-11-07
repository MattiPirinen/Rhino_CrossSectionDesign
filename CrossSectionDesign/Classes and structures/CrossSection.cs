using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Interfaces;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using CrossSectionDesign.Static_classes;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry.Intersect;

namespace CrossSectionDesign.Classes_and_structures
{
    public class CrossSection : Countable
    {
        public MaterialType MaterialResultShown { get; set; } = MaterialType.Steel;
        public Tuple<double, double> MinAndMaxStress
        {
            get => _minAndMaxStress;
            set
            {
                _minAndMaxStress = value;
                ProjectPlugIn.Instance.ColorScaleDisplay.SetColorScale(
                    value.Item1*Math.Pow(10,-6), value.Item2 * Math.Pow(10, -6), 0, 0.7, "Stress [MPA]");
            }
        }

        public bool HasSteelShell { get; set; }
        public CrossSection(string name, Beam b)//,ConcreteMaterial concreteMaterial)
        {
            AddingCentroid = ProjectPlugIn.Instance.NextCentroid();
            //ConcreteMaterial = concreteMaterial;
            Name = name;
            HostBeam = b;
            GeometryLargeIds.CollectionChanged += UpdateGeometricValuesEvent;
            ReinforementIds.CollectionChanged += UpdateGeometricValuesEvent;

            GeometryList = Tuple.Create(Plane.WorldXY, new List<ICalcGeometry>());
        }

        private void UpdateGeometricValuesEvent(object sender, EventArgs e)
        {
            //Clear previous result values
            HostBeam.ClearResults();
            ClearStresses();
            ProjectPlugIn.Instance.ResultConduit.Enabled = false;
            ProjectPlugIn.Instance.LocalAxisConduit.Enabled = false;
            ProjectPlugIn.Instance.ColorScaleDisplay.Enabled = false;
            //Calculate new geometric propertios
            CalcConcreteValues();
            CalcReinfValues();
            CalcOmega();
            CalcConcreteCover();

            if (GeometryLargeIds.Count != 0)
                OnGeometryChanged();
        }


        private void CalcConcreteCover()
        {
            List<Reinforcement> reinf = GetReinforcements();
            List<GeometryLarge> geomL = GetGeometryLarges();
            List<GeometryLarge> concreteGeoms = geomL.FindAll(geo => geo.Material.GetType() == typeof(ConcreteMaterial));
            if (concreteGeoms.Count != 1 || reinf.Count == 0) { ConcreteCover = 999; return; }
            double minDistance = double.MaxValue;

            foreach (Reinforcement reinforcement in reinf)
            {
                foreach (Curve c in concreteGeoms[0].BaseCurves)
                {
                    c.ClosestPoint(reinforcement.Centroid, out var t);
                    Point3d cp = c.PointAt(t);
                    double distance = new Vector3d(reinforcement.Centroid - cp).Length - reinforcement.Diameter / 2;
                    minDistance = Math.Min(minDistance, distance);
                }
            }
            ConcreteCover = minDistance*Math.Pow(10,-3);

        }

        public Vector3d I_Concrete { get; private set; }
        public Vector3d I_Reinf { get; private set; }
        public Vector3d i_Concrete { get; private set; }
        public Vector3d i_Reinf { get; private set; }
        public double A_Concrete { get; private set; }
        public double A_Reinf { get; set; }
        public double ReinfRatio { get { return A_Reinf / A_Concrete; } }
        public double Omega { get; set; }
        public double ConcreteCover { get; private set; }

        public double Heigth(Plane calcPlane)
        {
            BoundingBox bb = GetBoundingBox(calcPlane);
            return (bb.Max.Y - bb.Min.Y) * Math.Pow(10, -3);
        }
        public double Width(Plane calcPlane)
        {
            BoundingBox bb = GetBoundingBox(calcPlane);
            return (bb.Max.X - bb.Min.X) * Math.Pow(10, -3);
        }

        public string Name { get; private set; }
        public UserDataList GeometryLargeIds { get; set; } = new UserDataList();
        public UserDataList ReinforementIds { get; set; } = new UserDataList();
        public Beam HostBeam { get; set; }




        
        private ConcreteMaterial _concreteMaterial;
        private Tuple<double, double> _minAndMaxStress;

        public ConcreteMaterial ConcreteMaterial
        {
            get { return _concreteMaterial; }
            set { _concreteMaterial = value; }
        }


        public Point3d AddingCentroid { get; set; }
        public Tuple<Plane, List<ICalcGeometry>> GeometryList { get; private set; }
        public Point3d NeutralAxisCenter { get; set; }

        /* TODO need to implement the copying of geometry :(.
        public CrossSection DeepCopy()
        {
            CrossSection other = (CrossSection)MemberwiseClone();
            other.ConcreteMaterial =(ConcreteMaterial)_concreteMaterial.DeepCopy();
            other.AddingCentroid = ProjectPlugIn.Instance.NextCentroid();
        }
        */

        /* 
       public Curve GetStrengthCurve(Plane calcPlane) 
       {
           double angle = Vector3d.VectorAngle(Plane.WorldXY.YAxis, calcPlane.YAxis);
           Vector3d rotationAxis = StrengthSurface.Item3 - StrengthSurface.Item2;
           Plane cutPlane = new Plane(StrengthSurface.Item2, Vector3d.CrossProduct(Vector3d.ZAxis, rotationAxis), rotationAxis);
           cutPlane.Rotate(angle, Vector3d.YAxis);


           var watch = System.Diagnostics.Stopwatch.StartNew();
           Intersection.BrepPlane(StrengthSurface.Item1, cutPlane, 1,
               out var curves, out var points);
           watch.Stop();
           var elapsedMs = watch.ElapsedMilliseconds;
           RhinoApp.WriteLine("Time elapsed:" + elapsedMs.ToString() + "ms");


           Curve c = Curve.JoinCurves(curves)[0];
           c.TryGetPlane(out Plane pl, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
           if (pl.ZAxis.Z < 0) pl.Flip();

           angle = Vector3d.VectorAngle(pl.ZAxis, Vector3d.ZAxis, 
               new Plane(Point3d.Origin,Vector3d.CrossProduct(Vector3d.ZAxis,
               rotationAxis),Vector3d.ZAxis));


           c.Transform(Transform.Rotation(angle, rotationAxis,StrengthSurface.Item2));
           return c;
       }
       */

        protected virtual void OnGeometryChanged()
        {
            GeometryChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler GeometryChanged;

        public void AddGeometry(IEnumerable<ICalcGeometry> geometrys)
        {
            GeometryList.Item2.AddRange(geometrys);
        }

        public void AddGeometry(ICalcGeometry geometry)
        {
            GeometryList.Item2.Add(geometry);
        }

        public Point3d Centroid()
        {
            List<double> areas = new List<double>();
            Point3d sumPoint = Point3d.Origin;
            List<GeometryLarge> gl = GetGeometryLarges();

            foreach (GeometryLarge geom in gl)
            {
                AreaMassProperties amp = AreaMassProperties.Compute(geom.BaseBrep);
                areas.Add(amp.Area);
                sumPoint += amp.Area * amp.Centroid;
            }
            var value = sumPoint / areas.Sum();

            return sumPoint / areas.Sum();

        }

        public void CalcReinfValues()
        {
            Point3d c = Centroid();
            List<Reinforcement> reinfs = GetReinforcements();
            double Is_yy = 0;
            double Is_zz = 0;
            double Astot = 0;
            foreach (Reinforcement reinf in reinfs)
            {
                Point3d p = reinf.Centroid;
                double Area = reinf.Area * Math.Pow(10, -6);
                Astot += Area;
                double I = Math.Pow(reinf.Diameter, 4) * Math.PI / 64;
                Is_yy += Area * Math.Pow((c.Y - p.Y) * Math.Pow(10, -3), 2) + I;
                Is_zz += Area * Math.Pow((c.X - p.X) * Math.Pow(10, -3), 2) + I;
            }
            I_Reinf = new Vector3d(0, Is_yy, Is_zz);
            if (Astot == 0)
                i_Reinf = Vector3d.Zero;
            else
                i_Reinf = new Vector3d(Math.Sqrt(I_Reinf.X / Astot), Math.Sqrt(I_Reinf.Y / Astot), Math.Sqrt(I_Reinf.Z / Astot));
            A_Reinf = Astot;
        }

        public void CalcConcreteValues()
        {
            List<GeometryLarge> geomls = GetGeometryLarges();
            Point3d c = Centroid();
            double Actot = 0;
            double Ic_yy = 0;
            double Ic_zz = 0;
            foreach (GeometryLarge geomL in geomls)
            {
                if (geomL.Material.GetType() == typeof(ConcreteMaterial))
                {
                    Point3d p = geomL.Centroid;
                    double Area = geomL.AreaMassProp.Area * Math.Pow(10, -6);
                    Actot += Area;
                    double Iy = geomL.AreaMassProp.CentroidCoordinatesMomentsOfInertia.Y * Math.Pow(10, -12);
                    double Iz = geomL.AreaMassProp.CentroidCoordinatesMomentsOfInertia.X * Math.Pow(10, -12);
                    Ic_yy += Area * Math.Pow((c.Y - p.Y) * Math.Pow(10, -3), 2) + Iy;
                    Ic_zz += Area * Math.Pow((c.X - p.X) * Math.Pow(10, -3), 2) + Iz;
                }
            }
            I_Concrete = new Vector3d(0, Ic_yy, Ic_zz);
            if (Actot == 0)
                i_Concrete = Vector3d.Zero;
            else
                i_Concrete = new Vector3d(Math.Sqrt(I_Concrete.X / Actot),
                    Math.Sqrt(I_Concrete.Y / Actot), Math.Sqrt(I_Concrete.Z / Actot));
            A_Concrete = Actot;
        }

        //creates a list of tuples where the first value is a brep that shows the stress in the geometry segment
        //by the height of the brep and the second value is the stress value at the segment.
        public void CreateResultDisplay(bool steelOrConcrete, LoadCase lc)
        {

            List<ICalcGeometry> geoms = new List<ICalcGeometry>();
            List<Reinforcement> r = GetReinforcements();
            geoms.AddRange(r);
            List<GeometryLarge> gl = GetGeometryLarges();
            gl.ForEach(g => geoms.AddRange(g.CalcMesh.MeshSegments));

            MinAndMaxStress = CalcMaxAndMin(geoms, lc, steelOrConcrete);
            List<ICalcGeometry> breps = new List<ICalcGeometry>();
            double absMax = Math.Max(Math.Abs(MinAndMaxStress.Item1), Math.Abs(MinAndMaxStress.Item2));

            double maxLength = GetBoundingBox(Plane.WorldXY).Diagonal.Length * 0.5;
            List<Tuple<Brep, double>> tempList = new List<Tuple<Brep, double>>();



            foreach (ICalcGeometry geom in geoms)
            {
                if (steelOrConcrete && geom.Material.GetType() == typeof(SteelMaterial) ||
                    !steelOrConcrete && geom.Material.GetType() == typeof(ConcreteMaterial))
                {
                    double height = geom.Stresses[lc] / absMax * maxLength;
                    geom.ModifyMesh(height);
                }
            }
        }

        //Calculates the minumum and maximum stress in the cross section
        private Tuple<double, double> CalcMaxAndMin(List<ICalcGeometry> geoms, LoadCase lc, bool steelOrConcrete)
        {
            double maxValue = double.MinValue;
            double minValue = double.MaxValue;

            foreach (ICalcGeometry brepGeometry in geoms)
            {
                if ((steelOrConcrete && brepGeometry.Material.GetType() == typeof(SteelMaterial)) ||
                    (!steelOrConcrete && brepGeometry.Material.GetType() == typeof(ConcreteMaterial)))
                {
                    bool test = brepGeometry.Stresses.ContainsKey(lc);
                    maxValue = Math.Max(maxValue, brepGeometry.Stresses[lc]);
                    minValue = Math.Min(minValue, brepGeometry.Stresses[lc]);
                }
            }
            return Tuple.Create(minValue, maxValue);
        }
        /*
        public void CreateGeometryForCalc(Plane calcPlane)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            
            List<GeometrySegment> segList = new List<GeometrySegment>();
            List<GeometryLarge> geometryLarges = GetGeometryLarges();
            List<Reinforcement> reinfList = GetReinforcements();
            List<Brep> reinfBrepList = reinfList.Select(l => l.BrepGeometry).ToList();
            GeometryList = Tuple.Create(calcPlane, new List<ICalcGeometry>());

            foreach (GeometryLarge seg in geometryLarges)
            {
                //if (seg.Material.GetType() == typeof(SteelMaterial)) continue;
                if (seg.Material.GetType() == typeof(ConcreteMaterial) || seg.Material.GetType() == typeof(SteelMaterial))
                {
                    Brep[] tempList = Brep.CreateBooleanDifference(new[] { seg.BaseBrep }, reinfBrepList, doc.ModelAbsoluteTolerance);

                    Brep cuttedBrep;
                    if (tempList == null || tempList.Length == 0)
                        cuttedBrep = seg.BaseBrep.DuplicateBrep();
                    else
                        cuttedBrep =  tempList[0];

                    List <Brep> slicedBrep = CurveAndBrepManipulation.CutBrep(cuttedBrep, calcPlane);

                    //TODO update the correct material addition to here
                    foreach (Brep brep in slicedBrep)
                    {
                        GeometryList.Item2.Add(new GeometrySegment(brep,seg.Material));
                    }
                }
                else
                    RhinoApp.WriteLine(string.Format("Cutting of material is not implemented for {0}",seg.Material.GetType()));
            }
            GeometryList.Item2.AddRange(reinfList);

        }
        */
        public List<Reinforcement> GetReinforcements()
        {
            List<Reinforcement> temp = new List<Reinforcement>();
            RhinoObject[] objs = RhinoDoc.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                Reinforcement tempReinf = list.Find(typeof(Reinforcement)) as Reinforcement;
                if (ReinforementIds.IndexOf(tempReinf.Id) != -1)
                    temp.Add(tempReinf);


            }

            return temp;
        }

        public BoundingBox GetBoundingBox(Plane plane)
        {
            List<Brep> breps = GetGeometryLarges().Select(x => x.BaseBrep).ToList();

            BoundingBox bb;
            if (breps.Count != 0)
                bb = breps[0].GetBoundingBox(plane);
            else
                bb = new BoundingBox();
            foreach (Brep brep in breps)
            {
                bb.Union(brep.GetBoundingBox(plane));
            }

            return bb;
        }

        private void CalcOmega()
        {
            List<Reinforcement> reinf = GetReinforcements();

            if (A_Concrete == 0 || reinf.Count == 0)
                Omega = 0;
            else
            {
                SteelMaterial steelM = (SteelMaterial)reinf[0].Material;
                Omega = (A_Reinf * steelM.Fyd / (A_Concrete * -ConcreteMaterial.Fcd));
            }

        }

        //Gets concrete circumference
        public double GetConcreteCircumference()
        {
            List<GeometryLarge> largs = GetGeometryLarges().ToList();
            Brep totBrep = new Brep();
            foreach (GeometryLarge larg in largs)
            {
                if (larg.Material.GetType() == typeof(ConcreteMaterial))
                {
                    Brep[] brepsTemp = Brep.CreateBooleanUnion(new Brep[] { totBrep, larg.BaseBrep }, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    if (brepsTemp.Length == 1)
                        totBrep = brepsTemp[0];
                    else
                    {
                        RhinoApp.WriteLine("The breps didnt touch");
                        return double.MaxValue;
                    }
                }

            }

            Curve[] cs = totBrep.DuplicateNakedEdgeCurves(true, true);
            double length = 0;
            foreach (Curve c in cs)
            {
                length += c.GetLength();
            }
            return length * Math.Pow(10, -3);


        }

        private double LinearInterplate(double firstVal, double lastVal, double firstLoc, double lastLoc, double loc)
        {
            double totDist = lastLoc - firstLoc;
            double totVal = lastVal - firstVal;
            double fmDist = loc - firstLoc;
            return firstVal + fmDist / totDist * totVal;
        }

        public Reinforcement GetOneReinforcement(int id)
        {
            RhinoObject[] objs = RhinoDoc.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                Reinforcement tempReinf = list.Find(typeof(Reinforcement)) as Reinforcement;
                if (tempReinf.Id == id)
                {
                    return tempReinf;
                }
            }
            return null;
        }

        public List<GeometryLarge> GetGeometryLarges()
        {
            List<GeometryLarge> geometryLarges = new List<GeometryLarge>();
            RhinoObject[] objs = RhinoDoc.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;

                GeometryLarge temp = list.Find(typeof(GeometryLarge)) as GeometryLarge ?? list.Find(typeof(RectangleGeometryLarge)) as GeometryLarge;

                if (temp == null)
                {
                    temp = list.Find(typeof(RectangleGeometryLarge)) as GeometryLarge;
                }
                if (GeometryLargeIds.IndexOf(temp.Id) != -1)
                    geometryLarges.Add(temp);
            }

            return geometryLarges;
        }

        public void ClearGeometryLarges()
        {
            RhinoObject[] objs = RhinoDoc.ActiveDoc.Objects.FindByUserString("infType", "GeometryLarge", true);

            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                GeometryLarge temp = list.Find(typeof(GeometryLarge)) as GeometryLarge ?? list.Find(typeof(RectangleGeometryLarge)) as GeometryLarge;
                if (GeometryLargeIds.IndexOf(temp.Id) != -1)
                {
                    int layerIndex = rhinoObject.Attributes.LayerIndex;
                    Layer l = RhinoDoc.ActiveDoc.Layers[layerIndex];
                    l.IsLocked = false;
                    l.CommitChanges();
                    
                    RhinoDoc.ActiveDoc.Objects.Delete(rhinoObject, true);
                    l.IsLocked = true;
                    l.CommitChanges();
                }
                    
            }
            GeometryLargeIds.Clear();
        }

        public void ClearReinf()
        {
            RhinoObject[] objs = RhinoDoc.ActiveDoc.Objects.FindByUserString("infType", "Reinforcement", true);
            foreach (RhinoObject rhinoObject in objs)
            {
                Rhino.DocObjects.Custom.UserDataList list = rhinoObject.Attributes.UserData;
                Reinforcement rf = list.Find(typeof(Reinforcement)) as Reinforcement;
                if (ReinforementIds.IndexOf(rf.Id) != -1)
                    RhinoDoc.ActiveDoc.Objects.Delete(rhinoObject, true);
            }
            ReinforementIds.Clear();
        }

        public Point3d CalculateLoading(double strainAtMin, double strainAtMax, Plane calcPlane, LimitState ls)
        {
            BoundingBox crossSectionbb = GetBoundingBox(calcPlane);
            List<double> NList = new List<double>();
            List<double> MzList = new List<double>(); //help values needed to calculate the moment
            List<double> MyList = new List<double>();
            //get minimum and maximum values in that axis in local coordinate axis
            double MinY = crossSectionbb.Min.Y;
            double MaxY = crossSectionbb.Max.Y;

            List<ICalcGeometry> geoms = new List<ICalcGeometry>();
            List<Reinforcement> r = GetReinforcements();
            geoms.AddRange(r);
            List<GeometryLarge> gl = GetGeometryLarges();
            gl.ForEach(g => geoms.AddRange(g.CalcMesh.MeshSegments));

            foreach (ICalcGeometry segment in geoms)
            {
                Point3d cp = segment.Centroid;
                Point3d cpWorld = new Point3d(cp);
                cp.Transform(Transform.PlaneToPlane(calcPlane, Plane.WorldXY));
                double strain = LinearInterplate(strainAtMin, strainAtMax, MinY, MaxY,
                    cp.Y);
                double stress = segment.Material.Stress(strain, ls);
                NList.Add(stress * segment.Area * Math.Pow(10, -6));
                MzList.Add(NList[NList.Count - 1] * cpWorld.Y * Math.Pow(10, -3));
                MyList.Add(NList[NList.Count - 1] * cpWorld.X * Math.Pow(10, -3));
            }

            double N = NList.Sum();

            Point3d c = Centroid();
            double tempTerm = N * (c.Y) * Math.Pow(10, -3);
            double Mz = MzList.Sum() - tempTerm;

            tempTerm = N * (c.X) * Math.Pow(10, -3);
            double My = MyList.Sum() - tempTerm;

            return new Point3d(N, My, Mz);
        }

        private List<ICalcGeometry> GetAllGeometrys()
        {
            List<ICalcGeometry> geoms = new List<ICalcGeometry>();
            List<Reinforcement> r = GetReinforcements();
            geoms.AddRange(r);
            List<GeometryLarge> gl = GetGeometryLarges();
            gl.ForEach(g => geoms.AddRange(g.CalcMesh.MeshSegments));
            return geoms;
        }

        private void ClearStresses()
        {
            List<ICalcGeometry> geoms = GetAllGeometrys();
            geoms.ForEach(o => o.Stresses = new Dictionary<LoadCase, double>());
        }

        private void SaveStresses(double strainAtMin, double strainAtMax, Plane calcPlane, LoadCase lc)
        {

            BoundingBox crossSectionbb = GetBoundingBox(calcPlane);

            List<double> forces = new List<double>();
            List<double> momentDueToForces = new List<double>(); //help values needed to calculate the moment

            List<ICalcGeometry> geoms = GetAllGeometrys();

            double valueAtMin = crossSectionbb.Min.Y;
            double valueAtMax = crossSectionbb.Max.Y;

            double neutralAxisY = SolveRoot(strainAtMin, strainAtMax, valueAtMin, valueAtMax);
            Point3d c = Centroid();
            c.Transform(Transform.PlaneToPlane(calcPlane, Plane.WorldXY));

            Point3d neutralAxisCenter = new Point3d(c.X, neutralAxisY, 0);
            neutralAxisCenter.Transform(Transform.PlaneToPlane(Plane.WorldXY, calcPlane));
            Plane pl = new Plane(calcPlane);
            pl.Translate(neutralAxisCenter - pl.Origin);
            lc.NeutralAxis = pl;
            lc.strainAtMinAndMax = Tuple.Create(strainAtMin, strainAtMax);

            foreach (ICalcGeometry segment in geoms)
            {

                Point3d cp = segment.Centroid;
                cp.Transform(Transform.PlaneToPlane(calcPlane, Plane.WorldXY));

                double strain = LinearInterplate(strainAtMin, strainAtMax, valueAtMin, valueAtMax,
                    cp.Y);
                if (segment.Stresses.ContainsKey(lc))
                    segment.Stresses[lc] = segment.Material.Stress(strain,lc.Ls);
                else
                    segment.Stresses.Add(lc, segment.Material.Stress(strain, lc.Ls));
            }
        }
        //Solves a root of linear function with two input points.
        private double SolveRoot(double y1, double y2, double x1, double x2)
        {
            return (x1 - (x2 - x1) * y1 / (y2 - y1));

        }

        //Get all bounding boxes
        private List<BoundingBox> getAllBBs()
        {
            List<BoundingBox> bbList = new List<BoundingBox>();

            List<GeometryLarge> geometryLarges = GetGeometryLarges();
            foreach (GeometryLarge geometryLarge in geometryLarges)
            {
                bbList.Add(geometryLarge.BaseBrep.GetBoundingBox(true));
            }

            return bbList;
        }

        //Calculates min and max strain from neutral axis position and from heigth of the section
        private Tuple<double, double> CalcMinAndMax(double na, double sectionHeigth)
        {

            double minStr;
            double maxStr;
            if (na < sectionHeigth || HasSteelShell)
            {
                minStr = -0.01;
                maxStr = (na - sectionHeigth) / na * minStr;
            }

            else
            {
                double helpVal = (1 - _concreteMaterial.Epsc2 / _concreteMaterial.Epscu1) * sectionHeigth;
                minStr = _concreteMaterial.Epsc2 * na / (na - helpVal);
                maxStr = (na - sectionHeigth) / na * minStr;
            }

            return Tuple.Create(minStr, maxStr);
        }

        /*
        public void CalculateStrengthSurface()
        {
            List<Polyline> lines = new List< Polyline>();


            Point3d origin = Point3d.Unset;
            Point3d second = Point3d.Unset;
            Plane calcPlane = Plane.WorldXY;
            for (int i = 0; i < 12; i++)
            {
                if (i == 0)
                {
                    lines.Add(CalculateStrengthCurve(calcPlane, true, Tuple.Create(Point3d.Unset, Point3d.Unset)));
                    origin = lines[0][lines[0].Count - 1];
                    second = lines[0][0];
                }
                    
                else
                {
                    Polyline pl = CalculateStrengthCurve(calcPlane, false,Tuple.Create(origin,second));
                    pl.Insert(0, lines[0][0]);
                    pl.Add(origin);
                    lines.Add(pl);
                }
                calcPlane.Rotate(Math.PI*2 / 12, Vector3d.ZAxis);
            }
            lines.ForEach(l => RhinoDoc.ActiveDoc.Objects.AddPolyline(l));

            List<Curve> curves = new List<Curve>();
            lines.ForEach(l => curves.Add(l.ToNurbsCurve().Rebuild(100, 2, true)));
            curves.ForEach(l => RhinoDoc.ActiveDoc.Objects.AddCurve(l));
            Brep tempBrep =  Brep.CreateFromLoft(curves, Point3d.Unset, Point3d.Unset, LoftType.Normal, true)[0];
            RhinoDoc.ActiveDoc.Objects.AddBrep(tempBrep);
            StrengthSurface = Tuple.Create(tempBrep,lines[0][lines[0].Count - 1], lines[0][0]);
            
        }
        */

        public Polyline CalculateStrengthCurve(Plane calcPlane, LimitState ls)
        {
            BoundingBox bb = GetBoundingBox(calcPlane);
            double sectionHeigth = bb.Max.Y - bb.Min.Y;

            Point3d strengthPoint;
            //Divide the geometry larges into smaller segments for numerical integration
            //CreateGeometryForCalc(axis, calcPlane);


            Polyline strengthCurve = new Polyline();

            List<Reinforcement> reinf = GetReinforcements();

            const double steps = 20;

            const double maxStrain = 0.01; //Allowed strain of the reinforcement

            //Reinf yield strain
            double yieldStrain = 0.0022;
            if (reinf.Count != 0)
            {
                SteelMaterial sm = (SteelMaterial)reinf[0].Material;
                yieldStrain = sm.Fyd / sm.E;
            }

            strengthPoint = CalculateLoading(maxStrain, maxStrain, calcPlane, ls);
            strengthCurve.Add(strengthPoint);

            //Tension failure
            List<double> minStrainList = new List<double>();
            for (int i = 8; i < steps; i++)
            {
                minStrainList.Add(yieldStrain - (i / steps * (yieldStrain - _concreteMaterial.Epscu1)));
            }


            foreach (double minStrain in minStrainList)
            {
                strengthPoint = CalculateLoading(minStrain, maxStrain, calcPlane, ls);
                strengthCurve.Add(strengthPoint);
            }

            //Until all in compression
            List<double> maxStrainList = new List<double>();

            for (int i = 0; i < steps; i++)
            {
                maxStrainList.Add(maxStrain - i / steps * maxStrain);
            }

            foreach (double maxStr in maxStrainList)
            {
                if (HasSteelShell)
                {
                    strengthPoint = CalculateLoading(-maxStrain, maxStr, calcPlane, ls);
                    strengthCurve.Add(strengthPoint);
                }
                else
                {
                    strengthPoint = CalculateLoading(_concreteMaterial.Epscu1, maxStr, calcPlane, ls);
                    strengthCurve.Add(strengthPoint);
                }
            }

            //Until even compression
            double na = sectionHeigth;

            while (na < sectionHeigth * 5)
            {
                Tuple<double, double> strains = CalcMinAndMax(na, sectionHeigth);
                strengthPoint = CalculateLoading(strains.Item1, strains.Item2, calcPlane, ls);
                strengthCurve.Add(strengthPoint);
                na += sectionHeigth * 0.251;
            }



            //Pure compression (If the crossSection has steel shell we allow stains to go all the way to Epscu1 without failure
            //else only to Epsc2

            if (HasSteelShell)
            {
                strengthPoint = CalculateLoading(_concreteMaterial.Epscu1, _concreteMaterial.Epscu1, calcPlane, ls);
                strengthCurve.Add(strengthPoint);
            }

            else
            {
                strengthPoint = CalculateLoading(_concreteMaterial.Epsc2, _concreteMaterial.Epsc2, calcPlane, ls);
                strengthCurve.Add(strengthPoint);
            }

            //Second round

            //Until no more all in compression
            na = 5 * sectionHeigth;

            while (na > sectionHeigth)
            {
                Tuple<double, double> strains = CalcMinAndMax(na, sectionHeigth);
                strengthPoint = CalculateLoading(strains.Item2, strains.Item1, calcPlane, ls);
                strengthCurve.Add(strengthPoint);
                na -= sectionHeigth * 0.25;
            }


            //Until balanced failure
            maxStrainList = new List<double>();

            for (int i = 0; i < steps; i++)
            {
                maxStrainList.Add(
                    maxStrain * i / steps);
            }

            foreach (double maxStr in maxStrainList)
            {
                if (HasSteelShell)
                {
                    strengthPoint = CalculateLoading(maxStr, -maxStrain, calcPlane, ls);
                    strengthCurve.Add(strengthPoint);
                }
                else
                {
                    strengthPoint = CalculateLoading(maxStr, _concreteMaterial.Epscu1, calcPlane, ls);
                    strengthCurve.Add(strengthPoint);
                }
            }

            //Until even tension failure
            minStrainList = new List<double>();
            for (int i = 0; i < steps; i++)
            {
                minStrainList.Add(_concreteMaterial.Epscu1 + (i / steps * (yieldStrain - _concreteMaterial.Epscu1)));
            }

            foreach (double minStrain in minStrainList)
            {
                strengthPoint = CalculateLoading(maxStrain, minStrain, calcPlane, ls);
                strengthCurve.Add(strengthPoint);
            }

            strengthPoint = CalculateLoading(maxStrain, maxStrain, calcPlane, ls);
            strengthCurve.Add(strengthPoint);


            strengthCurve.DeleteShortSegments(100);

            return strengthCurve;
        }

        public double CalculateBalanceFailureNormalForce(Plane calcPlane,LimitState ls)
        {
            List<Reinforcement> reinf = GetReinforcements();
            BoundingBox bb = GetBoundingBox(calcPlane);
            double yieldStrain = 0.0022;
            if (reinf.Count != 0)
            {
                SteelMaterial sm = (SteelMaterial)reinf[0].Material;
                double distance = 55;
                double ayieldstrain = sm.Fyd / sm.E;
                yieldStrain = (ayieldstrain - ConcreteMaterial.Epscu1) / (bb.Max.Y - bb.Min.Y - distance) * 55 + ayieldstrain;
            }


            return CalculateLoading(_concreteMaterial.Epscu1, yieldStrain, calcPlane,ls).X;
        }

        public bool CalculateStresses(double n, double mz, double my, string loadCaseName, LimitState ls)
        {
            Tuple<bool, Vector<double>, int, int> result = broyden(n, mz, my, ls);
            if (result.Item1)
            {
                LoadCase lc;
                if (HostBeam.LoadCases.TrueForAll(o => o.Name != loadCaseName))
                {
                    lc = new SimpleLoadCase(n, mz, my, HostBeam, loadCaseName, ls);
                    HostBeam.LoadCases.Add(lc);
                }
                else
                {

                    int index = HostBeam.LoadCases.FindIndex(o => o.Name == loadCaseName);
                    lc = HostBeam.LoadCases[index];
                    if (lc.GetType() == typeof(SimpleLoadCase))
                    {
                        SimpleLoadCase slc = (SimpleLoadCase)lc;
                        slc.N_Ed = n;
                        slc.M_Edz = mz;
                        slc.M_Edy = my;
                    }
                }
                Plane pl = Plane.WorldXY;
                pl.Rotate(result.Item2[2], Vector3d.ZAxis);
                SaveStresses(result.Item2[0], result.Item2[1], pl, lc);
                lc.LoadPlane = pl;
                HostBeam.CurrentLoadCase = lc;
                ((SimpleLoadCase)lc).CrackWidthCalc.CalculateCrackWidth();
                return true;
            }

            return false;

        }

        public bool CalculateStresses(double n, double mz, double my, string loadCaseName, ref int iterations, ref int resets, LimitState ls)
        {
            Tuple<bool, Vector<double>, int, int> result = broyden(n, mz, my, ls);
            iterations = result.Item4;
            resets = result.Item3;
            if (result.Item1)
            {
                LoadCase lc;
                if (HostBeam.LoadCases.TrueForAll(o => o.Name != loadCaseName))
                {
                    lc = new SimpleLoadCase(n, mz, my, HostBeam, loadCaseName, ls);
                    HostBeam.LoadCases.Add(lc);
                }
                else
                {
                    int index = HostBeam.LoadCases.FindIndex(o => o.Name == loadCaseName);
                    lc = HostBeam.LoadCases[index];
                    SimpleLoadCase slc = (SimpleLoadCase)lc;
                    slc.N_Ed = n;
                    slc.M_Edz = mz;
                    slc.M_Edy = my;

                }
                Plane pl = Plane.WorldXY;
                pl.Rotate(result.Item2[2], Vector3d.ZAxis);
                SaveStresses(result.Item2[0], result.Item2[1], pl, lc);
                lc.LoadPlane = pl;


                return true;
            }

            return false;

        }

        public Tuple<bool, Vector<double>, int, int> broyden(double n, double mz, double my, LimitState ls)
        {
            Plane basePlane = Plane.WorldXY;

            List<Vector3d> resultList = new List<Vector3d>();

            Vector<double> fg = Vector<double>.Build.Dense(new double[] { 0, 0, 0 });


            int resets = 0;

            Vector<double> x2 = Vector<double>.Build.Dense(new double[] { -0.0005, -0.0001, 0.1 });
            Plane calcPlane = Plane.WorldXY;
            calcPlane.Rotate(x2[2], Vector3d.ZAxis);
            Point3d strengthPoint = CalculateLoading(x2[0], x2[1], calcPlane,ls);
            Vector<double> fx2 = Vector<double>.Build.Dense(new double[] {(strengthPoint.X-n)/((n != 0)? n : 1),
                (strengthPoint.Z-mz)/((mz != 0) ? mz : 1), (strengthPoint.Y-my)/((my != 0)? my : 1)});

            int i = 0;
            while ((Math.Abs(fx2[0]) > 0.001 || Math.Abs(fx2[1]) > 0.001 || Math.Abs(fx2[2]) > 0.001) && i < 100)
            {
                Matrix<double> Jnorm = DefineJakobianMatrix(fx2, x2, n, mz, my, ls);
                Matrix<double> J = Jnorm.Inverse();
                x2 = x2 + J * (fg - fx2);
                CheckStrains(ref x2, ref resets);

                calcPlane = Plane.WorldXY;
                calcPlane.Rotate(x2[2], Vector3d.ZAxis);
                strengthPoint = CalculateLoading(x2[0], x2[1], calcPlane, ls);
                fx2 = Vector<double>.Build.Dense(new double[] {(strengthPoint.X-n)/((n != 0)? n : 1),
                (strengthPoint.Z-mz)/((mz != 0) ? mz : 1), (strengthPoint.Y-my)/((my != 0)? my : 1)});
                i++;
            }

            if (i < 100)
            {

                RhinoApp.WriteLine("Success! Iterations needed:" + i);

                return Tuple.Create(true, x2, resets, i);
            }
            else
            {
                RhinoApp.WriteLine("Failure!");
                return Tuple.Create(false, x2, resets, i);
            }
        }

        private Matrix<double> DefineJakobianMatrix(Vector<double> fx2, Vector<double> x2, double n, double mz, double my, LimitState ls)
        {


            Vector<double> x11 = x2 - Vector<double>.Build.Dense(new double[] { 0.0001, 0, 0 });
            Vector<double> x12 = x2 - Vector<double>.Build.Dense(new double[] { 0, 0.0001, 0 });
            Vector<double> x13 = x2 + Vector<double>.Build.Dense(new double[] { 0, 0, 0.01 });


            Plane calcPlane = Plane.WorldXY;
            calcPlane.Rotate(x11[2], Vector3d.ZAxis);
            Point3d strengthPoint = CalculateLoading(x11[0], x11[1], calcPlane,ls);
            Vector<double> fx11 = Vector<double>.Build.Dense(new double[] {(strengthPoint.X-n)/((n != 0)? n : 1),
                (strengthPoint.Z-mz)/((mz != 0) ? mz : 1), (strengthPoint.Y-my)/((my != 0)? my : 1)});


            calcPlane = Plane.WorldXY;
            calcPlane.Rotate(x12[2], Vector3d.ZAxis);
            strengthPoint = CalculateLoading(x12[0], x12[1], calcPlane,ls);
            Vector<double> fx12 = Vector<double>.Build.Dense(new double[] {(strengthPoint.X-n)/((n != 0)? n : 1),
                (strengthPoint.Z-mz)/((mz != 0) ? mz : 1), (strengthPoint.Y-my)/((my != 0)? my : 1)});

            calcPlane = Plane.WorldXY;
            calcPlane.Rotate(x13[2], Vector3d.ZAxis);
            strengthPoint = CalculateLoading(x13[0], x13[1], calcPlane,ls);
            Vector<double> fx13 = Vector<double>.Build.Dense(new double[] {(strengthPoint.X-n)/((n != 0)? n : 1),
                (strengthPoint.Z-mz)/((mz != 0) ? mz : 1), (strengthPoint.Y-my)/((my != 0)? my : 1)});



            Matrix<double> Jnorm = Matrix<double>.Build.DenseOfArray(
                new double[,] { { Computederivate(fx11[0], fx2[0], x11[0], x2[0]),Computederivate(fx12[0], fx2[0], x12[1], x2[1]), Computederivate(fx13[0], fx2[0], x13[2], x2[2])},
                                { Computederivate(fx11[1], fx2[1], x11[0], x2[0]),Computederivate(fx12[1], fx2[1], x12[1], x2[1]),Computederivate(fx13[1], fx2[1], x13[2], x2[2])},
                                { Computederivate(fx11[2], fx2[2], x11[0], x2[0]),Computederivate(fx12[2], fx2[2], x12[1], x2[1]),Computederivate(fx13[2], fx2[2], x13[2], x2[2])},
                });
            return Jnorm;
        }

        private void CheckStrains(ref Vector<double> strains, ref int resets)
        {

            Random r = new Random();
            for (int i = 0; i < strains.Count - 1; i++)
            {
                if (strains[i] < -0.05 || strains[i] > 0.05)
                {
                    strains[0] = r.NextDouble() * -0.001;
                    strains[1] = r.NextDouble() * -0.001;
                    strains[2] = r.NextDouble() * 0.01;
                    resets += 1;
                }

            }
            //if (strains[2] > Math.PI * 2) strains[2] = Math.PI * 2;
            //if (strains[2] < 0) strains[2] = 0;


        }

        private double Computederivate(double fx1, double fx2, double x1, double x2)
        {
            return (fx2 - fx1) / (x2 - x1);
        }

    }
}
