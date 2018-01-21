using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace CrossSectionDesign
{
    public static class CurveAndBrepManipulation
    {

        public static List<Brep> CutBrep(Brep brep, Plane plane, Axis axis)
        {
            Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;

            //Finds the min and max range where the cuts are made
            Tuple<Point3d, Point3d> minAndMax = getMinAndMax(brep, plane);

            //Creates the cutting planes
            List<Plane> cuttingPlanes = getCuttingPlanes(minAndMax.Item1, minAndMax.Item2, plane, axis);

            //Initializes the list where the cutted curves are added
            List<Brep> cutBrepList = new List<Brep>();

            //Transform brep to the local coordinates
            Transform localCoordinates = Transform.PlaneToPlane(plane, Plane.WorldXY);
            brep.Transform(localCoordinates);

            Brep[] brepList = new[] {brep};

            foreach (Plane cuttingPlane in cuttingPlanes)
            {
                Plane cuttingPlane2 = new Plane(cuttingPlane);
                cuttingPlane2.Rotate(Math.PI, cuttingPlane.XAxis, cuttingPlane.Origin);

                List<Brep> tempBrepList = new List<Brep>();
                foreach (Brep brep1 in brepList)
                {
                    tempBrepList.AddRange(brep1.Trim(cuttingPlane2, doc.ModelAbsoluteTolerance));
                    cutBrepList.AddRange(brep1.Trim(cuttingPlane, doc.ModelAbsoluteTolerance));
                }
                brepList = tempBrepList.ToArray();
            }
            cutBrepList.AddRange(brepList);
            return cutBrepList;
        }

        //This method cuts the inputcurve into segments
        public static List<Curve> cutCurve(Curve curve, Plane plane, Axis axis)
        {

            //Finds the min and max range where the cuts are made
            Tuple<Point3d, Point3d> minAndMax = getMinAndMax(curve, plane);

            //Creates the cutting planes
            List<Plane> cuttingPlanes = getCuttingPlanes(minAndMax.Item1, minAndMax.Item2, plane,axis);

            //Initializes the list where the cutted curves are added
            List<Curve> cutCurveList = new List<Curve>();
            //Transform curve to the local coordinates
            Transform localCoordinates = Transform.PlaneToPlane(plane, Plane.WorldXY);
            curve.Transform(localCoordinates);

            
            List<double> parameters = new List<double>(); // Curve parameters that intersect with the cutting plane
            // Curves that come out of the splitting process as curves that are not part of that particular joining cycle
            List<Curve> validCurves = new List<Curve>();
            // Curves that come out of the splitting process as curves that are not part of that particular joining cycle
            List<Curve> invalidCurves = new List<Curve>(); 
            List<Curve> straightCurveList = new List<Curve>(); // curves that connect the splitted parts in the main geometry
            Curve[] remainingCurves = { curve }; //the curves that still need to be split
            List<Curve> remainingTemp = new List<Curve>();


            foreach (Plane cutPlane in cuttingPlanes)
            {
                 remainingTemp.Clear();
                foreach (Curve remainingCurve in remainingCurves)
                {
                    //Initialize the lists for this cycle
                    parameters.Clear();
                    validCurves.Clear();
                    invalidCurves.Clear();
                    straightCurveList.Clear();
                    Rhino.Geometry.Intersect.CurveIntersections intersections = Rhino.Geometry.Intersect.Intersection.CurvePlane(remainingCurve, cutPlane, 0.001);

                    if (intersections != null && intersections.Count % 2 == 0)
                    {
                        //gets parameters on the curve form the intersection events
                        foreach (Rhino.Geometry.Intersect.IntersectionEvent intEvent in intersections)
                        {
                            parameters.Add(intEvent.ParameterA);
                        }


                        //Checks if curve is higher or lower than the cutting plane and saves the curves in to different lists
                        Curve[] curveList = remainingCurve.Split(parameters);
                        foreach (Curve cuttedCurve in curveList)
                        {
                            if (compareCurve(cuttedCurve, axis, cutPlane)) validCurves.Add(cuttedCurve);
                            else invalidCurves.Add(cuttedCurve);
                        }


                        //sorts the intersection points from lowest to highest according to axis selected
                        Point3d[] intPoints = sortPoints(parameters, remainingCurve, axis);

                        //creates lines between the points
                        for (int i = 0; i < intPoints.Count(); i += 2)
                        {
                            Curve straightCurve = new Line(intPoints[i], intPoints[i + 1]).ToNurbsCurve();
                            straightCurveList.Add(straightCurve);
                        }

                        //Creates closed curves from the curves that are lower than the cutting plane
                        Curve[] joinedCurves = Curve.JoinCurves(validCurves.ToArray().Union(straightCurveList));
                        if (findClosedCurve(joinedCurves).Item1)
                            cutCurveList.AddRange(findClosedCurve(joinedCurves).Item2);

                        

                        //join the remaining curves to form a closed curve for later splitting
                         Curve[] joinedRemainingCurves = Curve.JoinCurves(invalidCurves.Union(straightCurveList));
                        if (findClosedCurve(joinedRemainingCurves).Item1)
                            remainingTemp.AddRange(findClosedCurve(joinedRemainingCurves).Item2);
                    }
                    
                    
                }
                // changes the list into array
                if (remainingTemp.Count != 0)
                    remainingCurves = remainingTemp.ToArray();

            }
            cutCurveList.AddRange(remainingCurves);

            return cutCurveList;
        }

        // This method finds a closed curve in curvelist and returns it. If no closed curves were found returns null
        private static Tuple<Boolean,List<Curve>> findClosedCurve(Curve[] curveList)
        {
            List<Curve> newCurveList = new List<Curve>();
            foreach (Curve testCurve in curveList) {if (testCurve.IsClosed) newCurveList.Add(testCurve);}

            if (newCurveList.Count != 0)
                return Tuple.Create(true, newCurveList);
            else
                return Tuple.Create(false, newCurveList);
        }

        //This method checks if the curve is below or above the cutting plane
        private static bool compareCurve(Curve curve, Axis axis, Plane plane)
        {
            switch (axis)
            {
                case (Axis.XAxis):
                    if (curve.PointAtNormalizedLength(0.5).Y < plane.Origin.Y) return true;
                    else return false;
                case (Axis.YAxis):
                    if (curve.PointAtNormalizedLength(0.5).X < plane.Origin.X) return true;
                    else return false;
                default:
                    return false;
            }


        }

        //this method sorts point either by x or y coordinate values
        private static Point3d[] sortPoints(List<double> parameters, Curve curve, Axis axis)
        {
            List<Point3d> pointList = new List<Point3d>();
            foreach (double parameter in parameters)
            {
                pointList.Add(curve.PointAt(parameter));
            }
            Point3d[] pointArray = pointList.ToArray();

            switch (axis)
            {
                case (Axis.XAxis):
                    Array.Sort(pointArray,
                      delegate (Point3d x, Point3d y) { return x.X.CompareTo(y.X); });
                    break;
                case (Axis.YAxis):
                    Array.Sort(pointArray,
                      delegate (Point3d x, Point3d y) { return x.Y.CompareTo(y.Y); });
                    break;
                default:
                    break;
            }

            return pointArray;

        }

        //This method creates a bounding box to a geometry in a given plane and returns the min and max coordinates of that box in the plane coordinates.
        private static Tuple<Point3d, Point3d> getMinAndMax(GeometryBase geometry, Plane plane)
        {

            Transform vali = Transform.PlaneToPlane(Plane.WorldXY, plane);
            BoundingBox box = geometry.GetBoundingBox(plane);
            Point3d minPoint = box.Min;
            Point3d maxPoint = box.Max;

            return Tuple.Create(minPoint, maxPoint);
        }

        //This method will create cutting planes for a geometry in local coordinate axis
        private static List<Plane> getCuttingPlanes(Point3d minP, Point3d maxP, Plane plane, Axis axis)
        {

            Tuple<Plane, Vector3d> returnValues = chooseAxis(axis, minP, maxP);
            Plane cuttingPlane = returnValues.Item1;
            Vector3d distance = returnValues.Item2;

            Transform planeTransform = Transform.PlaneToPlane(Plane.WorldXY, plane);
            cuttingPlane.Translate(new Vector3d(minP));

            double count = 20;
            Vector3d step = distance / count;
            int i = 0;
            List<Plane> planeList = new List<Plane>();
            while (i++ < count - 1)
            {
                Plane newPlane = new Plane(cuttingPlane);
                newPlane.Translate(step * i);
                //newPlane.Transform(planeTransform);
                planeList.Add(newPlane);
            }
            return planeList;
        }

        //returns a plane and range of the input geometry according to users choosing
        private static Tuple<Plane, Vector3d> chooseAxis(Axis axis, Point3d minP, Point3d maxP)
        {
            switch (axis)
            {
                case Axis.XAxis:
                    return Tuple.Create(Plane.WorldZX, new Vector3d(0, maxP.Y - minP.Y, 0));
                case Axis.YAxis:
                    return Tuple.Create(Plane.WorldYZ, new Vector3d(maxP.X - minP.X, 0, 0));
                default:
                    return Tuple.Create(Plane.WorldZX, new Vector3d(0, maxP.Y - minP.Y, 0));
            }

        }


    }
}
