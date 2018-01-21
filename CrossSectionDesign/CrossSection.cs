using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.VisualStyles;
using Rhino;
using Rhino.Geometry;



namespace CrossSectionDesign
{
    class CrossSection
    {

        

        public CrossSection(ConcreteMaterial concreteMtMaterial)
        {
            ConcreteMaterial = concreteMtMaterial;
            _number = _sNumber;
            _sNumber += 1;

        }

        public List<GeometryLarge> GeometryLarges { get; set; } = new List<GeometryLarge>();

        private double _sectionHeigth;
        private ConcreteMaterial _concreteMaterial;
        public ConcreteMaterial ConcreteMaterial
        {
            get { return _concreteMaterial; }
            set { _concreteMaterial = value; }
        }

        public List<IBrepGeometry> GeometryList { get; private set; } = new List<IBrepGeometry>();

        public BoundingBox CrossSectionbb { get; private set; } = BoundingBox.Empty;

        private void Updatebb(BoundingBox bb)
        {
            CrossSectionbb = BoundingBox.Union(CrossSectionbb,bb);

            _sectionHeigth = CrossSectionbb.Max.Y - CrossSectionbb.Min.Y;
        }

        private void Updatebb(IEnumerable<BoundingBox> bbList)
        {
            foreach (BoundingBox bb in bbList)
            {
                CrossSectionbb = BoundingBox.Union(CrossSectionbb, bb);
            }

            _sectionHeigth = CrossSectionbb.Max.Y - CrossSectionbb.Min.Y;

        }

        private static int _sNumber;
        private int _number;
        public List<Tuple<double, double>> Strength = new List<Tuple<double, double>>();

        public void AddGeometry(IEnumerable<IBrepGeometry> geometrys)
        {
            GeometryList.AddRange(geometrys);
        }

        public void AddGeometry(IBrepGeometry geometry)
        {
            GeometryList.Add(geometry);
        }

        private Point3d Centroid()
        {
            List<double> areas = new List<double>();
            Point3d sumPoint = Point3d.Origin;

            foreach (IBrepGeometry geom in GeometryList)
            {
                areas.Add(geom.AreaMassProp.Area);
                sumPoint += geom.AreaMassProp.Area*geom.AreaMassProp.Centroid;
            }
            var value = sumPoint / areas.Sum();

            return sumPoint / areas.Sum();

        }

        private void CutWithReinforcement()
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;


            List<Reinforcement> reinfList = new List<Reinforcement>();
            List<GeometrySegment> segList = new List<GeometrySegment>();
            foreach (IBrepGeometry brepGeometry in GeometryList)
            {
                
                if (brepGeometry.GetType() == typeof(Reinforcement))
                {
                    reinfList.Add((Reinforcement) brepGeometry);
                }
                else
                {
                    segList.Add((GeometrySegment) brepGeometry);
                }
            }
            GeometryList = new List<IBrepGeometry>();
            Brep[] reinfBrepList = reinfList.Select(o => o.BrepGeometry).ToArray();

            foreach (GeometryLarge seg in GeometryLarges)
            {
                //if (seg.Material.GetType() == typeof(SteelMaterial)) continue;
                if (seg.Material.GetType() == typeof(ConcreteMaterial) || seg.Material.GetType() == typeof(SteelMaterial))
                {
                    Brep[] tempList = Brep.CreateBooleanDifference(new[] { seg.BaseBrep }, reinfBrepList, doc.ModelAbsoluteTolerance);

                    Brep cuttedBrep;
                    if (tempList == null || tempList.Length == 0)
                        cuttedBrep = seg.BaseBrep;
                    else
                        cuttedBrep =  tempList[0];

                    List <Brep> slicedBrep = CurveAndBrepManipulation.CutBrep(cuttedBrep, Plane.WorldXY, Axis.XAxis);

                    //TODO update the correct material addition to here
                    foreach (Brep brep in slicedBrep)
                    {
                        GeometryList.Add(new GeometrySegment(brep,seg.Material));
                    }
                    
                }
                else
                    RhinoApp.WriteLine($"Cutting of material is not implemented for {seg.Material.GetType()}");
            }
            GeometryList.AddRange(reinfList);

        }

        private double LinearInterplate(double firstVal, double lastVal, double firstLoc, double lastLoc, double loc)
        {
            double totDist = lastLoc - firstLoc;
            double totVal = lastVal - firstVal;
            double fmDist = loc - firstLoc;
            return firstVal + fmDist / totDist * totVal;
        }

        private Tuple<double, double> CalculateLoading(double strainAtMin, double strainAtMax, Axis axis)
        {

            List<double> forces = new List<double>();
            List<double> momentDueToForces = new List<double>(); //help values needed to calculate the moment
            
            
            //get minimum and maximum values in that axis
            if (axis == Axis.XAxis)
            {
                double valueAtMin = CrossSectionbb.Min.Y;
                double valueAtMax = CrossSectionbb.Max.Y;

                foreach (IBrepGeometry segment in GeometryList)
                {
                    double strain = LinearInterplate(strainAtMin, strainAtMax, valueAtMin, valueAtMax,
                        segment.AreaMassProp.Centroid.Y);
                    double stress = segment.Material.Stress(strain);
                    forces.Add(stress * segment.AreaMassProp.Area*Math.Pow(10,-6));
                    momentDueToForces.Add(forces[forces.Count-1]*(segment.AreaMassProp.Centroid.Y-CrossSectionbb.Min.Y)*Math.Pow(10,-3));
                    
                }

                double force = forces.Sum();
                double tempTerm = force * (Centroid().Y - CrossSectionbb.Min.Y)*Math.Pow(10,-3);

                double mr = momentDueToForces.Sum()- tempTerm;
                return Tuple.Create(force, mr);
            }
            else
            {
                double valueAtMin = CrossSectionbb.Min.X;
                double valueAtMax = CrossSectionbb.Max.X;

                foreach (IBrepGeometry segment in GeometryList)
                {
                    
                        double strain = LinearInterplate(strainAtMin, strainAtMax, valueAtMin, valueAtMax,
                            segment.AreaMassProp.Centroid.X);
                        double stress = segment.Material.Stress(strain);
                        forces.Add(stress * segment.AreaMassProp.Area * Math.Pow(10, -6));
                        momentDueToForces.Add(forces[forces.Count - 1] * (segment.AreaMassProp.Centroid.X - CrossSectionbb.Min.X) * Math.Pow(10, -3));
                    
                }

                double force = forces.Sum();
                double tempTerm = force * (Centroid().X - CrossSectionbb.Min.X)*Math.Pow(10,-3);

                double mr = momentDueToForces.Sum()- tempTerm;

                return Tuple.Create(force, mr);
            }

        }

        private void saveStresses(double strainAtMin, double strainAtMax, Axis axis)
        {

            List<double> forces = new List<double>();
            List<double> momentDueToForces = new List<double>(); //help values needed to calculate the moment


            //get minimum and maximum values in that axis
            if (axis == Axis.XAxis)
            {
                double valueAtMin = CrossSectionbb.Min.Y;
                double valueAtMax = CrossSectionbb.Max.Y;

                foreach (IBrepGeometry segment in GeometryList)
                {
                    double strain = LinearInterplate(strainAtMin, strainAtMax, valueAtMin, valueAtMax,
                        segment.AreaMassProp.Centroid.Y);
                    segment.Stress = segment.Material.Stress(strain);
                }

            }
            else
            {
                double valueAtMin = CrossSectionbb.Min.X;
                double valueAtMax = CrossSectionbb.Max.X;

                foreach (IBrepGeometry segment in GeometryList)
                {

                    double strain = LinearInterplate(strainAtMin, strainAtMax, valueAtMin, valueAtMax,
                        segment.AreaMassProp.Centroid.X);
                    segment.Stress = segment.Material.Stress(strain);
                }
            }
        }



        //Get all bounding boxes
        private List<BoundingBox> getAllBBs()
        {
            List<BoundingBox> bbList = new List<BoundingBox>();
            foreach (GeometryLarge geometryLarge in GeometryLarges)
            {
                bbList.Add(geometryLarge.BaseBrep.GetBoundingBox(true));
            }

            return bbList;
        }

        //Calculates min and max strain from neutral axis position and from heigth of the section
        private Tuple<double, double> CalcMinAndMax(double na)
        {

            double minStr;
            double maxStr;
            if (na < _sectionHeigth)
            {
                minStr = _concreteMaterial.Epscu1;
                maxStr = (_sectionHeigth - na) / na * minStr;
            }

            else
            {
                double helpVal = (1 - _concreteMaterial.Epsc2 / _concreteMaterial.Epscu1) * _sectionHeigth;
                minStr = _concreteMaterial.Epsc2 * na / (na - helpVal);
                maxStr = (na- _sectionHeigth) / na * minStr;
            }

            return Tuple.Create(minStr, maxStr);
        }

        public void CalculateStrengthCurve(Axis axis)
        {
            
            Strength.Clear();

            //Calculate crossSectionBB
            Updatebb(getAllBBs());



            //Divide the geometry larges into smaller segments for numerical integration
            CutWithReinforcement();

            const double steps = 20;

            const double maxStrain = 0.01; //Allowed strain of the reinforcement

            //Tension failure
            List<double> minStrainList = new List<double>();
            for (int i = 0; i < steps; i++)
            {
                minStrainList.Add(maxStrain - (i / steps * (maxStrain - _concreteMaterial.Epscu1)));
            }

            foreach (double minStrain in minStrainList)
            {
                Strength.Add(CalculateLoading(minStrain,maxStrain,Axis.XAxis));
            }

            //Until all in compression
            List<double> maxStrainList = new List<double>();

            for (int i = 0; i < steps; i++)
            {
                maxStrainList.Add(maxStrain - i / steps * maxStrain);
            }

            foreach (double maxStr in maxStrainList)
            {
                Strength.Add(CalculateLoading(_concreteMaterial.Epscu1, maxStr,Axis.XAxis));
            }

            //Untill even compression
            double na = _sectionHeigth;

            while (na < _sectionHeigth*5)
            {
                Tuple<double,double> strains = CalcMinAndMax(na);
                Strength.Add(CalculateLoading(strains.Item1, strains.Item2,Axis.XAxis));
                na += _sectionHeigth * 0.25;
            }
            Strength.Add(CalculateLoading(_concreteMaterial.Epsc2, _concreteMaterial.Epsc2, Axis.XAxis));

        }

        public void CalculateStrains(double force, double moment,Axis axis)
        {
            //ColorRGB c = Utils.HSL2RGB(0.5, 0.5, 0.5);
            //Calculate crossSectionBB
            Updatebb(getAllBBs());

            //Divide the geometry larges into smaller segments for numerical integration
            CutWithReinforcement();


            //Initial test
            Tuple<double, double> temp = CalculateLoading(_concreteMaterial.Epsc2, _concreteMaterial.Epsc2, axis);
            
            if (temp.Item1 > force)
            {
                RhinoApp.WriteLine("Cross section cannot bear that large load.");
                return;
            }
            temp = CalculateLoading(0.01, 0.01, axis);
            if (temp.Item1 < force)
            {
                RhinoApp.WriteLine("Cross section cannot bear that large load.");
                return;
            }

            int i = 0;
            int tressHould = 200;
            double testForce = 0;
            double testMoment = 0;
            double strainBot = 0;
            double strainTop = 0;
            double HigherLimitTop = 0.01;
            double HigherLimitBot = 0.01;
            double LowerLimitTop = _concreteMaterial.Epsc2;
            double LowerLimitBot = _concreteMaterial.Epsc2;


            //Iteration
            while (Math.Abs((testForce - force) / force) > 0.001 && Math.Abs((testMoment - moment) / moment) > 0.001 && i < tressHould)
            {



                while (Math.Abs((testForce - force) / force) > 0.001 && i < tressHould)
                {

                    if (testForce < force)
                    {
                        LowerLimitTop = strainTop;
                        LowerLimitBot = strainBot;
                        strainTop += (HigherLimitTop - strainTop) / 2;
                        strainBot += (HigherLimitBot - strainBot) / 2;


                    }   
                    else
                    {
                        HigherLimitTop = strainTop;
                        HigherLimitBot = strainBot;
                        strainTop += (LowerLimitTop-strainTop) / 2;
                        strainBot += (LowerLimitBot-strainBot) / 2;
                    }
                    i += 1;
                    temp = CalculateLoading(strainTop, strainBot, axis);
                    testForce = temp.Item1;



                    RhinoApp.WriteLine($"Force: {testForce}, Moment: {testMoment}");
                }

                

                HigherLimitTop = -_concreteMaterial.Epsc2;
                HigherLimitBot = -_concreteMaterial.Epsc2;
                LowerLimitTop = _concreteMaterial.Epsc2;
                LowerLimitBot = _concreteMaterial.Epsc2;




                while (Math.Abs((testMoment - moment) / moment) > 0.01 && i < tressHould)
                {


                    if (testMoment < moment)
                    {
                        HigherLimitTop = strainTop;
                        LowerLimitBot = strainBot;
                        strainTop += (LowerLimitTop-strainTop) / 2;
                        strainBot += (HigherLimitBot - strainBot) / 2;
                    }
                    else
                    {
                        LowerLimitTop = strainTop;
                        HigherLimitBot = strainBot;
                        strainTop += ( HigherLimitTop - strainTop) / 2;
                        strainBot += (LowerLimitBot-strainBot) / 2;
                    }
                    i += 1;

                    temp = CalculateLoading(strainTop, strainBot, axis);
                    testMoment = temp.Item2;


                    RhinoApp.WriteLine($"Force: {testForce}, Moment: {testMoment}");
                }



            }
            if (i<tressHould)
                saveStresses(strainTop,strainBot,axis);

            RhinoApp.WriteLine(i < tressHould ? "Success" : "Failure");
        }


    }
}
