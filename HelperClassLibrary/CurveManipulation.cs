using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace HelperClassLibrary
{
    public class CurveManipulation
    {

        public enum Axis
        {
            XAxis,
            YAxis
        }

        //This method cuts the inputcurve into segments
        public static List<Curve> cutCurve(Curve curve, Plane plane, List<Plane> cuttingPlanes, Axis axis)
        {
            Rhino.RhinoApp.WriteLine("jee");
            //Initializes the list where the cutted curves are added
            List<Curve> cutCurveList = new List<Curve>();
            //Transform curve to the local coordinates
            Transform localCoordinates = Transform.PlaneToPlane(plane, Plane.WorldXY);
            curve.Transform(localCoordinates);

            List<double> parameters = new List<double>();
            List<Curve> validCurves = new List<Curve>();
            List<Curve> invalidCurves = new List<Curve>();
            List<Curve> straightCurveList = new List<Curve>();
            Curve remainingCurve = curve;

            foreach (Plane cutPlane in cuttingPlanes)
            {
                parameters.Clear();
                validCurves.Clear();
                invalidCurves.Clear();
                straightCurveList.Clear();
                Rhino.Geometry.Intersect.CurveIntersections intersections = Rhino.Geometry.Intersect.Intersection.CurvePlane(remainingCurve, cutPlane, 0.001);

                if (intersections != null && intersections.Count % 2 == 0)
                {

                    foreach (Rhino.Geometry.Intersect.IntersectionEvent intEvent in intersections)
                    {
                        parameters.Add(intEvent.ParameterA);
                    }

                    Curve[] curveList = remainingCurve.Split(parameters);
                    foreach (Curve cuttedCurve in curveList)
                    {
                        if (compareCurve(cuttedCurve, axis, cutPlane)) validCurves.Add(cuttedCurve);
                        else invalidCurves.Add(cuttedCurve);
                    }



                    Point3d[] intPoints = sortPoints(parameters, remainingCurve, axis);

                    for (int i = 0; i < intPoints.Count(); i += 2)
                    {
                        Curve straightCurve = new Line(intPoints[i], intPoints[i + 1]).ToNurbsCurve();
                        straightCurveList.Add(straightCurve);
                        //Rhino.RhinoApp.WriteLine(intersections[i+1].ParameterA.ToString());
                        //Rhino.RhinoApp.WriteLine(straightCurve.ToString());

                        Curve[] joinedCurves = Curve.JoinCurves(validCurves.ToArray().Union(new Curve[] { straightCurve }));

                        foreach (Curve testCurve in joinedCurves)
                        {
                            if (testCurve.IsClosed)
                            {
                                cutCurveList.Add(testCurve);
                                break;
                            }

                        }
                    }


                    remainingCurve = Curve.JoinCurves(invalidCurves.Union(straightCurveList))[0];

                }

            }
            cutCurveList.Add(remainingCurve);

            return cutCurveList;
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


        //This method creates a bounding box to curve in a given plane and returns the min and max coordinates of that box in the plane coordinates.
        private static Tuple<Point3d, Point3d> getMinAndMax(Curve curve, Plane plane)
        {

            Transform vali = Transform.PlaneToPlane(Plane.WorldXY, plane);
            BoundingBox box = curve.GetBoundingBox(plane);
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
