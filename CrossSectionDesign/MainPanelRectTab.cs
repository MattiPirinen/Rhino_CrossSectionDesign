using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CrossSectionDesign.Classes_and_structures;
using CrossSectionDesign.Enumerates;
using CrossSectionDesign.Properties;
using CrossSectionDesign.Static_classes;
using Dlubal.RFEM5;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using RMA.OpenNURBS;
using Column = CrossSectionDesign.Classes_and_structures.Column;
using Loadcase = CrossSectionDesign.Classes_and_structures.LoadCase;

namespace CrossSectionDesign
{
    public partial class MainPanel
    {
        //Updates textbox values from the cross section
        private void UpdatePredefinedFromCrossSection()
        {
            if (_projectPlugIn.CurrentBeam is Column)
            {
                Column c = (Column)_projectPlugIn.CurrentBeam;
                textBoxColumnLength.Text = (c.Length*Math.Pow(10,3)).ToString();
                textBoxKz.Text = c.Kz.ToString();
                textBoxKy.Text = c.Ky.ToString();
                textBoxStartTime.Text = c.ClimateCond.T0.ToString();
                textBoxEndTime.Text = c.ClimateCond.T.ToString();
                comboBoxRH.SelectedItem = c.ClimateCond.RH.ToString();
                checkBoxNominalCurvature1.Checked = c.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature1];
                checkBoxNominalStiffness.Checked = c.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness2];


            }

            if (_projectPlugIn.CurrentBeam.CrossSec is PredefinedCrossSection)
            {
                PredefinedCrossSection sec = (PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec;

                comboBoxConcreteS.SelectedIndex = (sec.ConcreteMaterial != null) ? comboBoxConcreteS.FindStringExact(sec.ConcreteMaterial.StrengthClass) :
                    comboBoxConcreteS.FindStringExact("C30/37");
                textBoxConcreteC.Text = sec.ConcreteCover.ToString();
                comboBoxSteelS.SelectedIndex = (sec.SteelMaterial != null) ? comboBoxSteelS.FindStringExact(sec.SteelMaterial.StrengthClass) :
                    comboBoxSteelS.FindStringExact("S355");
                comboBoxReinfS.SelectedIndex = (sec.ReinfMaterial != null) ? comboBoxReinfS.FindStringExact(sec.ReinfMaterial.StrengthClass) :
                    comboBoxReinfS.FindStringExact("B500B");
                textBoxConcreteStrength.ReadOnly = sec.ConcreteMaterial.StrengthClass == "Custom" ? true : false;
                textBoxSteelStrength.ReadOnly = sec.SteelMaterial.StrengthClass == "Custom" ? true : false;
                textBoxReinfStrength.ReadOnly = sec.ReinfMaterial.StrengthClass == "Custom" ? true : false;

                textBoxCSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammac.ToString();
                textBoxSSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammas.ToString();
                textBoxRSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammar.ToString();
                textBoxConcreteStrength.Text = (sec.ConcreteMaterial.Fck * Math.Pow(10,-6)).ToString();
                textBoxSteelStrength.Text = (sec.SteelMaterial.Fyk * Math.Pow(10, -6)).ToString();
                textBoxReinfStrength.Text = (sec.ReinfMaterial.Fyk * Math.Pow(10, -6)).ToString();
                textBoxRotation.Text = sec.Rotation.ToString();

                textBoxAcc.Text = _projectPlugIn.CurrentBeam.Acc.ToString();
                textBoxMainD.Text = (sec.MainD*Math.Pow(10,3)).ToString();
                checkBoxRC_OnOff.Checked = sec.HasSteelShell;
                textBoxRC_SteelThickness.Text = (sec.SteelThickness*Math.Pow(10,3)).ToString();
                textBoxStirrupD.Text = (sec.StirrupD*Math.Pow(10,3)).ToString();

                

                if (_projectPlugIn.CurrentBeam.CrossSec is RectangleCrossSection)
                {
                    radioButtonRect.Checked = true;
                    RectangleCrossSection secR = (RectangleCrossSection)_projectPlugIn.CurrentBeam.CrossSec;
                    textBoxWidth.Text = secR.ConcreteWidth.ToString();
                    textBoxHeigth.Text = secR.ConcreteHeight.ToString();
                    textBoxAmountH.Text = secR.NoReinfH.ToString();
                    textBoxAmountW.Text = secR.NoReinfW.ToString();
                }
                else if (_projectPlugIn.CurrentBeam.CrossSec is CircleCrossSection)
                {
                    radioButtonCircle.Checked = true;
                    CircleCrossSection secC = (CircleCrossSection)_projectPlugIn.CurrentBeam.CrossSec;
                    textBoxHeigth.Text = secC.ConcreteDiameter.ToString();
                    textBoxAmountH.Text = secC.NoReinf.ToString();
                    
                }
                if (_projectPlugIn.CurrentBeam.LoadCases.Count != 0)
                {
                    textBox_MSfactor.Text = ((ColLoadCase)_projectPlugIn.CurrentBeam.LoadCases[0]).Ratio.ToString();
                    textBox_C.Text = ((ColLoadCase)_projectPlugIn.CurrentBeam.LoadCases[0]).Ccurve.ToString();
                }
                else
                {
                    textBox_MSfactor.Text = "0.68";
                    textBox_C.Text = "9.9";
                }
                



                UpdateResults();
            }


            

        }

        // Updates the values of current rectangular cross section
        private bool UpdateCrossSectionValues()
        {
            bool success = false;

            if (radioButtonRect.Checked)
                success = UpdateRectValues();
            else
                success = UpdateCircleValues();
            CalcReinfRatio();
            

            return success;
        }

        private void CalcReinfRatio()
        {
            //Calculate reinforcement ratio
            if (_projectPlugIn.CurrentBeam.CrossSec != null)
            {
                double ratio = _projectPlugIn.CurrentBeam.CrossSec.ReinfRatio * 100;

                labelReinfRatio.Text = $"{ ratio:F1}";
                if (ratio > 0.2 && ratio < 6)
                    pictureBoxReinfRatio.Image = Resources.OK;
                else
                    pictureBoxReinfRatio.Image = Resources.Warning;
            }
        }

        private bool UpdateCircleValues()
        {
            
            RhinoDoc doc = ProjectPlugIn.Instance.ActiveDoc;

            if (int.TryParse(textBoxAmountH.Text, out var amount) &&
                int.TryParse(textBoxConcreteC.Text, out var concreteC) &&
                double.TryParse(textBoxStirrupD.Text, out var stirrupD) &&
                double.TryParse(textBoxMainD.Text, out var mainD) &&
                double.TryParse(textBoxHeigth.Text, out var diameter) &&
                int.TryParse(textBoxRotation.Text, out var rotation) &&
                double.TryParse(textBoxColumnLength.Text, out var cLength) &&
                double.TryParse(textBoxKz.Text, out var kz) &&
                double.TryParse(textBoxKy.Text, out var ky) &&
                int.TryParse(textBoxStartTime.Text, out var t0) &&
                int.TryParse(textBoxEndTime.Text, out var t) &&
                int.Parse(textBoxAmountH.Text) > 1 &&
                int.Parse(textBoxAmountW.Text) > 1)
            {

                int workLife = int.Parse(comboBoxDesWorkingLife.SelectedItem.ToString().Split(' ')[0]);
                int rh = int.Parse(comboBoxRH.SelectedItem.ToString().TrimEnd('%'));


                //Check if steel shell is selected to be on. If it is on and no thickness is given. do not continue.
                double steelThickness = 0;
                PredefinedCrossSection pc = (PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec;
                if (pc.HasSteelShell)
                {
                    if (!double.TryParse(textBoxRC_SteelThickness.Text, out steelThickness))
                        return false;
                }
                _projectPlugIn.CurrentBeam.CrossSec.ClearGeometryLarges();
                _projectPlugIn.CurrentBeam.CrossSec.ClearReinf();

                _projectPlugIn.CurrentBeam.CrossSec = new CircleCrossSection(_projectPlugIn.CurrentBeam.CrossSec.Name, _projectPlugIn.CurrentBeam, _projectPlugIn.CurrentBeam.CrossSec.AddingCentroid);
                _projectPlugIn.Beams[_projectPlugIn.SelectedBeamIndex] = _projectPlugIn.CurrentBeam;

                Column c = (Column)_projectPlugIn.CurrentBeam;
                c.Length = cLength * Math.Pow(10, -3);
                c.Ky = ky;
                c.Kz = kz;

                CircleCrossSection cc = (CircleCrossSection)_projectPlugIn.CurrentBeam.CrossSec;
                cc.ConcreteDiameter = diameter;
                cc.NoReinf = amount;
                cc.StirrupD = stirrupD * Math.Pow(10,-3);
                cc.MainD = mainD * Math.Pow(10, -3);
                cc.HasSteelShell = checkBoxRC_OnOff.Checked;
                cc.ConcreteCover = concreteC * Math.Pow(10, -3);
                cc.HasSteelShell = checkBoxRC_OnOff.Checked;
                cc.SteelThickness = steelThickness * Math.Pow(10, -3);
                if (comboBoxConcreteS.Text == "Custom" && double.TryParse(textBoxConcreteStrength.Text, out var cStrength))
                    cc.ConcreteMaterial = new ConcreteMaterial(comboBoxConcreteS.Text,
                        -cStrength * Math.Pow(10, 6), c);
                else
                    cc.ConcreteMaterial = new ConcreteMaterial(comboBoxConcreteS.Text, c);

                if (comboBoxSteelS.Text == "Custom" && double.TryParse(textBoxSteelStrength.Text, out var sStrength))
                    cc.SteelMaterial = new SteelMaterial(comboBoxSteelS.Text, sStrength * Math.Pow(10, 6)
                            ,SteelType.StructuralSteel, c);
                else
                    cc.SteelMaterial = new SteelMaterial(comboBoxSteelS.Text, SteelType.StructuralSteel, c);
                if (comboBoxReinfS.Text == "Custom" && double.TryParse(textBoxReinfStrength.Text,out var rStrength))
                    cc.ReinfMaterial = new SteelMaterial(comboBoxReinfS.Text, double.Parse(textBoxReinfStrength.Text) * Math.Pow(10, 6),
                        SteelType.Reinforcement, c);
                else
                    cc.ReinfMaterial = new SteelMaterial(comboBoxReinfS.Text, SteelType.Reinforcement, c);
                
                cc.Rotation = rotation;




                Curve concreteOutLine = new Circle(
                    Plane.WorldXY, _projectPlugIn.CurrentBeam.CrossSec.AddingCentroid,
                    diameter / 2).ToNurbsCurve();

                GeometryLarge larg = new GeometryLarge(MaterialType.Concrete, comboBoxConcreteS.Text,
                    Brep.CreatePlanarBreps(new[] { concreteOutLine })[0], cc);


                //Creates concrete geometry and bakes it into the current doc
                ObjectAttributes attr = new ObjectAttributes();
                attr.UserData.Add(larg);
                CreateGeometryLarge.GetLayerIndex(MaterialType.Concrete, ref attr);
                attr.SetUserString("Name", Enum.GetName(typeof(MaterialType), MaterialType.Concrete));
                attr.SetUserString("infType", "GeometryLarge");
                Guid guid = ProjectPlugIn.Instance.ActiveDoc.Objects.AddBrep(larg.GetModelUnitBrep());
                ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
                _projectPlugIn.CurrentBeam.CrossSec.GeometryLargeIds.Add(larg.Id);


                //Creates steel shell
                if (cc.HasSteelShell)
                {
                    Curve line1 = concreteOutLine.ToNurbsCurve();
                    Curve line2 = concreteOutLine.ToNurbsCurve().Offset(
                        new Plane(larg.Centroid, new Vector3d(0, 0, 1)),
                        steelThickness,
                        doc.ModelAbsoluteTolerance,
                        CurveOffsetCornerStyle.Sharp)[0];

                    Brep brep = CreateGeometryLarge.CreateHollowBrep(line1, line2);
                    GeometryLarge gl = new GeometryLarge(MaterialType.Steel, comboBoxSteelS.SelectedItem.ToString(), brep,cc);

                    //Creates steel geometry and bakes it into the current doc
                    attr = new ObjectAttributes();
                    attr.UserData.Add(gl);
                    CreateGeometryLarge.GetLayerIndex(MaterialType.Steel, ref attr);
                    attr.SetUserString("Name", Enum.GetName(typeof(MaterialType), MaterialType.Steel));
                    attr.SetUserString("infType", "GeometryLarge");
                    guid = ProjectPlugIn.Instance.ActiveDoc.Objects.AddBrep(gl.GetModelUnitBrep());
                    ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
                    _projectPlugIn.CurrentBeam.CrossSec.GeometryLargeIds.Add(gl.Id);

                }

                //Creates points for the reinforcement
                Curve tempCurve = concreteOutLine.Offset(
                    new Plane(larg.Centroid, new Vector3d(0, 0, -1)),
                    double.Parse(textBoxConcreteC.Text) + double.Parse(textBoxStirrupD.Text) +
                    double.Parse(textBoxMainD.Text) / 2,
                    doc.ModelAbsoluteTolerance,
                    CurveOffsetCornerStyle.None)[0];




                tempCurve.Rotate((double)rotation / 360 * 2 * Math.PI, Vector3d.ZAxis, cc.AddingCentroid);

                List<Point3d> points = new List<Point3d>();
                if (!CheckMinimumDistanceRequirement(amount, tempCurve.GetLength()))
                {
                    amount = 4;
                    pictureBoxAmountH.Image = Resources.Warning;
                }
                else
                    pictureBoxAmountH.Image = null;
                
                tempCurve.DivideByCount(amount, true, out var tempPoint);


                //Checks for layer index for the reinforcement
                int layerIndex = CreateReinforcement.GetOrCreateLayer(doc, "Reinforcement", Color.Black);
                if (layerIndex == 999) return false;

                //Creates reinforcement instances and bakes the points and saves the instances into the points
                foreach (Point3d point in tempPoint)
                {
                    guid = doc.Objects.AddPoint(point);
                    Reinforcement reinf = new Reinforcement(cc)
                    {
                        Material = new SteelMaterial(comboBoxReinfS.SelectedItem.ToString(),SteelType.Reinforcement,c),
                        Centroid = point,
                        Diameter = int.Parse(textBoxMainD.Text)
                    };
                    attr = new ObjectAttributes();
                    attr.UserData.Add(reinf);
                    attr.SetUserString("Name", "Reinforcement");
                    attr.SetUserString("infType", "Reinforcement");
                    ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
                    _projectPlugIn.CurrentBeam.CrossSec.ReinforementIds.Add(reinf.Id);

                }
                //Update climate condition
                ClimateCondition ccd = new ClimateCondition(rh, t0, t, _projectPlugIn.CurrentBeam);
                c.ClimateCond = ccd;

                c.LoadCases.ForEach(o => c.CrossSec.GeometryChanged += ((ColLoadCase)o).GeometryUpdated);
                c.LoadCases.ForEach(o => ((ColLoadCase)o).UpdateResults());
                return true;
            }
            else return false;
        }

        private void UpdateOnlyResults()
        {
            if (int.TryParse(textBoxAmountH.Text, out var amountH) &&
            int.TryParse(textBoxAmountW.Text, out var amountW) &&
            int.TryParse(textBoxConcreteC.Text, out var concreteC) &&
            int.TryParse(textBoxStirrupD.Text, out var stirrupD) &&
            int.TryParse(textBoxMainD.Text, out var mainD) &&
            int.TryParse(textBoxHeigth.Text, out var concreteHeigth) &&
            int.TryParse(textBoxWidth.Text, out var concreteWidth) &&
            int.TryParse(textBoxRotation.Text, out var rotation) &&
            int.TryParse(textBoxStartTime.Text, out var t0) &&
            int.TryParse(textBoxEndTime.Text, out var t) &&
            double.TryParse(textBoxColumnLength.Text, out var cLength) &&
            double.TryParse(textBoxKz.Text, out var kz) &&
            double.TryParse(textBoxKy.Text, out var ky) &&
            amountH > 1 &&
            amountW > 1)
            {


                SetStrengthChartCurve(chartRectMz, Moment.Mz,
                   _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(Plane.WorldXY,LimitState.Ultimate))
                   ;
                SetStrengthChartCurve(chartRectMy, Moment.My,
                    _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(new Plane(Point3d.Origin, Vector3d.YAxis, -Vector3d.XAxis), LimitState.Ultimate));
                UpdateAllLoadCases();
                ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
            }

        
         }

        private bool UpdateRectValues()
        {
            RhinoDoc doc = ProjectPlugIn.Instance.ActiveDoc;

            if (int.TryParse(textBoxAmountH.Text, out var amountH) &&
                int.TryParse(textBoxAmountW.Text, out var amountW) &&
                int.TryParse(textBoxConcreteC.Text, out var concreteC) &&
                int.TryParse(textBoxStirrupD.Text, out var stirrupD) &&
                int.TryParse(textBoxMainD.Text, out var mainD) &&
                int.TryParse(textBoxHeigth.Text, out var concreteHeigth) &&
                int.TryParse(textBoxWidth.Text, out var concreteWidth) &&
                int.TryParse(textBoxRotation.Text, out var rotation) &&
                int.TryParse(textBoxStartTime.Text, out var t0) &&
                int.TryParse(textBoxEndTime.Text, out var t) &&
                double.TryParse(textBoxColumnLength.Text, out var cLength) &&
                double.TryParse(textBoxKz.Text, out var kz) &&
                double.TryParse(textBoxKy.Text, out var ky) &&
                amountH > 1 &&
                amountW > 1)
            {
                double steelThickness = 0;
                int workLife = int.Parse(comboBoxDesWorkingLife.SelectedItem.ToString().Split(' ')[0]);
                int rh = int.Parse(comboBoxRH.SelectedItem.ToString().TrimEnd('%'));

                PredefinedCrossSection pc = (PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec;
                //Check if steel shell is selected to be on. If it is on and no thickness is given. do not continue.
                if (pc.HasSteelShell)
                {
                    
                    if (!double.TryParse(textBoxRC_SteelThickness.Text, out steelThickness))
                        return false;
                }

                _projectPlugIn.CurrentBeam.CrossSec.ClearGeometryLarges();
                _projectPlugIn.CurrentBeam.CrossSec.ClearReinf();


                _projectPlugIn.Beams[_projectPlugIn.SelectedBeamIndex] = _projectPlugIn.CurrentBeam;
                Column c = (Column)_projectPlugIn.CurrentBeam;
                c.Length = cLength * Math.Pow(10, -3);
                c.Ky = ky;
                c.Kz = kz;


                _projectPlugIn.CurrentBeam.CrossSec = new RectangleCrossSection(_projectPlugIn.CurrentBeam.CrossSec.Name, c, _projectPlugIn.CurrentBeam.CrossSec.AddingCentroid);
                


                RectangleCrossSection rc = (RectangleCrossSection)_projectPlugIn.CurrentBeam.CrossSec;
                rc.ConcreteHeight = concreteHeigth * Math.Pow(10,-3);
                rc.ConcreteWidth = concreteWidth * Math.Pow(10, -3);
                rc.NoReinfH = amountH;
                rc.NoReinfW = amountW;
                rc.HasSteelShell = checkBoxRC_OnOff.Checked;
                rc.StirrupD = stirrupD * Math.Pow(10,-3);
                rc.MainD = mainD * Math.Pow(10, -3);
                rc.ConcreteCover = concreteC * Math.Pow(10, -3);
                rc.HasSteelShell= checkBoxRC_OnOff.Checked;
                rc.SteelThickness = steelThickness * Math.Pow(10, -3);
                if (comboBoxConcreteS.Text == "Custom" && double.TryParse(textBoxConcreteStrength.Text, out var cStrength))
                    rc.ConcreteMaterial = new ConcreteMaterial(comboBoxConcreteS.Text,
                        -cStrength * Math.Pow(10, 6), c);
                else
                    rc.ConcreteMaterial = new ConcreteMaterial(comboBoxConcreteS.Text, c);

                if (comboBoxSteelS.Text == "Custom" && double.TryParse(textBoxSteelStrength.Text, out var sStrength))
                    rc.SteelMaterial = new SteelMaterial(comboBoxSteelS.Text, sStrength * Math.Pow(10, 6)
                            , SteelType.StructuralSteel, c);
                else
                    rc.SteelMaterial = new SteelMaterial(comboBoxSteelS.Text, SteelType.StructuralSteel, c);
                if (comboBoxReinfS.Text == "Custom" && double.TryParse(textBoxReinfStrength.Text, out var rStrength))
                    rc.ReinfMaterial = new SteelMaterial(comboBoxReinfS.Text, double.Parse(textBoxReinfStrength.Text) * Math.Pow(10, 6),
                        SteelType.Reinforcement, c);
                else
                   rc.ReinfMaterial = new SteelMaterial(comboBoxReinfS.Text, SteelType.Reinforcement, c);

                Curve outLine = CreateGeometryLarge.CreateOutline(rc.AddingCentroid, concreteWidth, concreteHeigth,rotation)
                    .ToNurbsCurve();


                Brep concreteBrep = Brep.CreatePlanarBreps(new[] { outLine })[0];

                GeometryLarge rg = new GeometryLarge(MaterialType.Concrete,
                    comboBoxConcreteS.Text, concreteBrep, rc);
                rg.BaseCurves.Add(outLine);


                //Creates concrete geometry and bakes it into the current doc
                ObjectAttributes attr = new ObjectAttributes();
                attr.UserData.Add(rg);
                CreateGeometryLarge.GetLayerIndex(MaterialType.Concrete, ref attr);
                attr.SetUserString("Name", Enum.GetName(typeof(MaterialType), MaterialType.Concrete));
                attr.SetUserString("infType", "GeometryLarge");
                Guid guid = ProjectPlugIn.Instance.ActiveDoc.Objects.AddBrep(rg.GetModelUnitBrep());
                ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
                rc.GeometryLargeIds.Add(rg.Id);

                //Creates steel shell
                if (rc.HasSteelShell)
                {
                    Curve line1 = rg.BaseCurves[0].ToNurbsCurve();
                    Curve line2 = rg.BaseCurves[0].ToNurbsCurve().Offset(
                        new Plane(rg.Centroid, new Vector3d(0, 0, 1)),
                        double.Parse(textBoxRC_SteelThickness.Text),
                        doc.ModelAbsoluteTolerance,
                        CurveOffsetCornerStyle.Sharp)[0];

                    Brep brep = CreateGeometryLarge.CreateHollowBrep(line1, line2);
                    GeometryLarge gl = new GeometryLarge(MaterialType.Steel, comboBoxSteelS.SelectedItem.ToString(), brep,rc);

                    //Creates steel geometry and bakes it into the current doc
                    attr = new ObjectAttributes();
                    attr.UserData.Add(gl);
                    CreateGeometryLarge.GetLayerIndex(MaterialType.Steel, ref attr);
                    attr.SetUserString("Name", Enum.GetName(typeof(MaterialType), MaterialType.Steel));
                    attr.SetUserString("infType", "GeometryLarge");
                    guid = ProjectPlugIn.Instance.ActiveDoc.Objects.AddBrep(gl.GetModelUnitBrep());
                    ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
                    rc.GeometryLargeIds.Add(gl.Id);

                }

                //Creates points for the reinforcement
                Curve tempCurve = rg.BaseCurves[0].ToNurbsCurve().Offset(
                    new Plane(rg.Centroid, new Vector3d(0, 0, -1)),
                    concreteC + stirrupD + mainD / 2,
                    doc.ModelAbsoluteTolerance,
                    CurveOffsetCornerStyle.None)[0];
                tempCurve.TryGetPolyline(out var pl);
                List<NurbsCurve> curves = pl.GetSegments().Select(o => o.ToNurbsCurve()).ToList();
                List<Curve> topAndBottom = new List<Curve>();
                List<Curve> leftAndRight = new List<Curve>();

                Plane localPl = Plane.WorldXY;
                localPl.Rotate(rotation*Math.PI*2/360, Vector3d.ZAxis);

                foreach (Curve curve in curves)
                {
                    Vector3d difference = curve.PointAtEnd - curve.PointAtStart;
                    difference.Transform(Transform.PlaneToPlane(localPl, Plane.WorldXY));

                    if (Math.Abs(difference.X) < doc.ModelAbsoluteTolerance)
                        leftAndRight.Add(curve);
                    else
                        topAndBottom.Add(curve);
                }

                List<Point3d> points = new List<Point3d>();
                if (!CheckMinimumDistanceRequirement(amountW, topAndBottom[0].GetLength()))
                {
                    amountW = 2;
                    pictureBoxAmountW.Image = Resources.Warning;
                }
                else
                    pictureBoxAmountW.Image = null;

                {
                    foreach (var curve in topAndBottom)
                    {
                        curve.DivideByCount(amountW - 1, true, out var tempPoint);
                        points.AddRange(tempPoint);
                    }
                }

                if (!CheckMinimumDistanceRequirement(amountH, leftAndRight[0].GetLength()))
                {
                    amountH = 2;
                    pictureBoxAmountH.Image = Resources.Warning;
                }
                else
                    pictureBoxAmountH.Image = null;


                foreach (var curve in leftAndRight)
                {
                    if (amountH > 2)
                    {
                        curve.DivideByCount(amountH - 1, false, out var tempPoint);
                        points.AddRange(tempPoint);
                    }
                }


                //Checks for layer index for the reinforcement
                int layerIndex = CreateReinforcement.GetOrCreateLayer(doc, "Reinforcement", Color.Black);
                if (layerIndex == 999) return false;

                //Creates reinforcement instances and bakes the points and saves the instances into the points
                foreach (Point3d point in points)
                {
                    guid = doc.Objects.AddPoint(point);
                    Reinforcement reinf = new Reinforcement(rc)
                    {
                        Material = new SteelMaterial(comboBoxReinfS.SelectedItem.ToString(), SteelType.Reinforcement, c),
                        Centroid = point,
                        Diameter = mainD * Math.Pow(10, -3)
                    };
                    attr = new ObjectAttributes();
                    attr.UserData.Add(reinf);
                    attr.SetUserString("Name", "Reinforcement");
                    attr.SetUserString("infType", "Reinforcement");
                    ProjectPlugIn.Instance.ActiveDoc.Objects.ModifyAttributes(guid, attr, true);
                    _projectPlugIn.CurrentBeam.CrossSec.ReinforementIds.Add(reinf.Id);
                }


                //Update climate condition
                ClimateCondition cc = new ClimateCondition(rh, t0, t, _projectPlugIn.CurrentBeam);
                c.ClimateCond = cc;

                c.LoadCases.ForEach(o => c.CrossSec.GeometryChanged += ((ColLoadCase)o).GeometryUpdated);
                c.LoadCases.ForEach(o => ((ColLoadCase)o).UpdateResults());

                return true;
            }
            else return false;
        }

        private bool CheckMinimumDistanceRequirement(int number, double distance)
        {
            //EC-2-8.2 Minimum distance between bars
            double k1 = 1; //National annex factor
            double k2 = 3; //national annex factor
            double dg = double.Parse(comboBoxMaxAggregateSize.SelectedItem.ToString()); // maxAggregate size
            
            PredefinedCrossSection temp = (PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec;

            double mainD = temp.MainD;

            double[] tempAmin = { k1 * mainD, dg + k2, 20 };
            double amin = tempAmin.Max();
            double dist = distance / (number - 1);
            return amin < dist;
        }

        private void CreateDefaultCircleCrossSection(string name)
        {
            textBoxAmountH.Text = "2";
            textBoxAmountW.Text = "2";
            textBoxConcreteC.Text = "35";
            textBoxStirrupD.Text = "10";
            textBoxMainD.Text = "6";
            textBoxHeigth.Text = "298";
            textBoxWidth.Text = "400";
            textBoxRotation.Text = "0";
            textBoxStartTime.Text = "28";
            textBoxEndTime.Text = "36000";
            textBoxColumnLength.Text = "5000";
            textBoxKz.Text = "1";
            textBoxKy.Text = "1";
            textBoxRC_SteelThickness.Text = "10.3";
            

            if (int.TryParse(textBoxStartTime.Text, out var t0) &&
                int.TryParse(textBoxEndTime.Text, out var t) &&
                double.TryParse(textBoxColumnLength.Text, out var cLength) &&
                double.TryParse(textBoxKz.Text, out var kz) &&
                double.TryParse(textBoxKy.Text, out var ky))
            {
                int workLife = int.Parse(comboBoxDesWorkingLife.SelectedItem.ToString().Split(' ')[0]);
                int rh = int.Parse(comboBoxRH.SelectedItem.ToString().TrimEnd('%'));

                comboBoxConcreteS.SelectedItem = "C35/45";


                Column col = new Column(name,1.0,1.5,1.15,0.85)
                {
                    Length = cLength * Math.Pow(10, -3),
                    Ky = ky,
                    Kz = kz
                };
                CircleCrossSection cs = new CircleCrossSection(name,col)
                {
                    ConcreteMaterial = new ConcreteMaterial(comboBoxConcreteS.SelectedItem.ToString(),col)
                };
                col.CrossSec = cs;

                _projectPlugIn.Beams.Add(col);
                _projectPlugIn.SelectedBeamIndex = _projectPlugIn.Beams.Count - 1;
                radioButtonCircle.Checked = true;
                comboBoxSteelS.Enabled = true;
                UpdateCrossSectionValues();

                textBoxConcreteStrength.Text = (-cs.ConcreteMaterial.Fck * Math.Pow(10, -6)).ToString();
                textBoxSteelStrength.Text = (cs.SteelMaterial.Fyk * Math.Pow(10, 6)).ToString();
                textBoxReinfStrength.Text = (cs.ReinfMaterial.Fyk * Math.Pow(10, 6)).ToString();
                textBoxCSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammac.ToString();
                textBoxSSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammas.ToString();
                textBoxRSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammar.ToString();
                textBoxAcc.Text = _projectPlugIn.CurrentBeam.Acc.ToString();




                ClimateCondition cc = new ClimateCondition(rh, t0, t,_projectPlugIn.CurrentBeam);
                col.ClimateCond = cc;

            }
            /*
            dataGridViewLoads.Rows[0].Cells[1].Value = -2000;
            dataGridViewLoads.Rows[0].Cells[2].Value = 150;
            dataGridViewLoads.Rows[0].Cells[3].Value = 150;
            dataGridViewLoads.Rows[0].Cells[4].Value = 150;
            dataGridViewLoads.Rows[0].Cells[5].Value = 150;
            */
            UpdateResults();
            comboBoxSteelS.SelectedItem = "S550";
            radioButtonCircle.Checked = true;
            checkBoxRC_OnOff.Checked = true;
            checkBoxRC_OnOff_CheckStateChanged(checkBoxRC_OnOff, EventArgs.Empty);
            radioButtonRect_CheckedChanged(radioButtonRect, EventArgs.Empty);
            //dataGridViewLoads_CellEndEdit(dataGridViewLoads, DataGridViewCellEventArgs.Empty as DataGridViewCellEventArgs);
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
            ZoomToCurrentBeam();
        }

        private void CreateDefaultRectCrossSection(string name)
        {
            textBoxAmountH.Text = "4";
            textBoxAmountW.Text = "4";
            textBoxConcreteC.Text = "35";
            textBoxStirrupD.Text = "10";
            textBoxMainD.Text = "25";
            textBoxHeigth.Text = "800";
            textBoxWidth.Text = "800";
            textBoxRotation.Text = "0";
            textBoxStartTime.Text = "28";
            textBoxEndTime.Text = "36000";
            textBoxColumnLength.Text = "5000";
            textBoxKz.Text = "1";
            textBoxKy.Text = "1";
            

            if (int.TryParse(textBoxStartTime.Text, out var t0) &&
                int.TryParse(textBoxEndTime.Text, out var t) &&
                double.TryParse(textBoxColumnLength.Text, out var cLength) &&
                double.TryParse(textBoxKz.Text, out var kz) &&
                double.TryParse(textBoxKy.Text, out var ky))
            {
                int workLife = int.Parse(comboBoxDesWorkingLife.SelectedItem.ToString().Split(' ')[0]);
                int rh = int.Parse(comboBoxRH.SelectedItem.ToString().TrimEnd('%'));

                Column col = new Column(name, 1.0, 1.5, 1.15, 0.85)
                {
                    Length = cLength * Math.Pow(10, -3),
                    Ky = ky,
                    Kz = kz
                };

                RectangleCrossSection cs = new RectangleCrossSection(name, col)
                {
                    ConcreteMaterial = new ConcreteMaterial(comboBoxConcreteS.SelectedItem.ToString(),col)
                };

                col.CrossSec = cs;

                _projectPlugIn.CurrentBeam = col;
                _projectPlugIn.Beams.Add(col);
                _projectPlugIn.SelectedBeamIndex = _projectPlugIn.Beams.Count - 1;
                radioButtonRect.Checked = true;
                comboBoxSteelS.Enabled = false;
                UpdateCrossSectionValues();


                textBoxConcreteStrength.Text = (-cs.ConcreteMaterial.Fck * Math.Pow(10, -6)).ToString();
                //textBoxSteelStrength.Text = (cs.SteelMaterial.Fyk * Math.Pow(10, 6)).ToString();
                textBoxReinfStrength.Text = (SteelMaterial.MaterialYield[comboBoxReinfS.Text] * Math.Pow(10, -6)).ToString();
                textBoxCSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammac.ToString();
                textBoxSSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammas.ToString();
                textBoxRSafetyFactor.Text = _projectPlugIn.CurrentBeam.Gammar.ToString();
                textBoxAcc.Text = _projectPlugIn.CurrentBeam.Acc.ToString();




                ClimateCondition cc = new ClimateCondition(rh, t0, t,
                      _projectPlugIn.CurrentBeam);
                col.ClimateCond = cc;

            }

            dataGridViewLoads.Rows[0].Cells[1].Value = -2000;
            dataGridViewLoads.Rows[0].Cells[2].Value = 150;
            dataGridViewLoads.Rows[0].Cells[3].Value = 150;
            dataGridViewLoads.Rows[0].Cells[4].Value = 150;
            dataGridViewLoads.Rows[0].Cells[5].Value = 150;

            UpdateResults();
            //dataGridViewLoads_CellEndEdit(dataGridViewLoads, DataGridViewCellEventArgs.Empty as DataGridViewCellEventArgs);
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
            ZoomToCurrentBeam();

        }

        private void ZoomToCurrentBeam()
        {
            RhinoView rw = ProjectPlugIn.Instance.ActiveDoc.Views.Find("Cross section view", false) ??
                           ProjectPlugIn.Instance.ActiveDoc.Views.Add("Cross section view", DefinedViewportProjection.Top, Bounds, false);
            rw.Maximized = true;
            ProjectPlugIn.Instance.ActiveDoc.Views.ActiveView = rw;
            BoundingBox bb = _projectPlugIn.CurrentBeam.CrossSec.GetBoundingBox(Plane.WorldXY);
            bb.Transform(Transform.Scale(_projectPlugIn.CurrentBeam.CrossSec.AddingCentroid, 1.0 /
                _projectPlugIn.Unitfactor));
            if (bb.Diagonal.Length != 0) {
                Transform transform = Transform.Scale(bb.Center, 1.5);
                bb.Transform(transform);
                ProjectPlugIn.Instance.ActiveDoc.Views.ActiveView.ActiveViewport.ZoomBoundingBox(bb);
            }
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private void buttonRectCalculate_Click(object sender, EventArgs e)
        {
            UpdateCrossSectionValues();
            SetStrengthChartCurve(chartRectMz, Moment.Mz,
                   _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(Plane.WorldXY, LimitState.Ultimate));
            SetStrengthChartCurve(chartRectMy, Moment.My,
                _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(new Plane(Point3d.Origin, Vector3d.YAxis, -Vector3d.XAxis), LimitState.Ultimate));
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private void UpdateResults()
        {
            if (UpdateCrossSectionValues())
            {
                SetStrengthChartCurve(chartRectMz, Moment.Mz,
                   _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(Plane.WorldXY, LimitState.Ultimate));
                SetStrengthChartCurve(chartRectMy, Moment.My,
                    _projectPlugIn.CurrentBeam.CrossSec.CalculateStrengthCurve(new Plane(Point3d.Origin, Vector3d.YAxis, -Vector3d.XAxis), LimitState.Ultimate));
            }
            UpdateAllLoadCases();
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private void checkBoxRC_OnOff_CheckStateChanged(object sender, EventArgs e)
        {

            if ((PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec != null)
            {
                var a = (PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec;
                a.HasSteelShell = checkBoxRC_OnOff.Checked;
                textBoxRC_SteelThickness.Enabled = checkBoxRC_OnOff.Checked;
                comboBoxSteelS.Enabled = checkBoxRC_OnOff.Checked;
                UpdateResults();
            }

        }

        private void updateResultsEvent(object sender, EventArgs e)
        {
            UpdateResults();
        }
        /*
        private void updateAccordingToStrengthTextBoxes(object sender, EventArgs e)
        {
            if (_projectPlugIn.CurrentBeam == null)
                return;
            List<GeometryLarge> gl = _projectPlugIn.CurrentBeam.CrossSec.GetGeometryLarges();
            List<Reinforcement> rl = _projectPlugIn.CurrentBeam.CrossSec.GetReinforcements();

            if (comboBoxConcreteS.SelectedItem.ToString() == "Custom")
                
            {
                _projectPlugIn.CurrentBeam.CrossSec.ConcreteMaterial =
                    new ConcreteMaterial("Custom", double.Parse(textBoxConcreteStrength.Text) * Math.Pow(10, 6), _projectPlugIn.CurrentBeam);
                foreach (GeometryLarge ge in gl)
                {
                    if (ge.Material.GetType() == typeof(ConcreteMaterial))
                        ge.Material = new ConcreteMaterial("Custom", -double.Parse(textBoxConcreteStrength.Text) * Math.Pow(10, 6), ge.OwnerBeam);

                }
            }

            if (comboBoxReinfS.SelectedItem.ToString() == "Custom")
            {
                foreach (Reinforcement reinforcement in rl)
                {
                    reinforcement.Material = new SteelMaterial("Custom", double.Parse(textBoxReinfStrength.Text) * Math.Pow(10, 6),SteelType.Reinforcement,reinforcement.OwnerBeam);
                }
            }
            if (comboBoxSteelS.SelectedItem.ToString() == "Custom")
                textBoxSteelStrength.ReadOnly = false;
            {
                foreach (GeometryLarge ge in gl)
                {
                    if (ge.Material.GetType() == typeof(SteelMaterial))
                        ge.Material = new SteelMaterial("Custom", double.Parse(textBoxSteelStrength.Text) * Math.Pow(10, 6), SteelType.StructuralSteel,ge.OwnerBeam);

                }
            }
        }
        */

        private void updatePartialSafetyFactors(object sender, EventArgs e)
        {
            if (double.TryParse(textBoxCSafetyFactor.Text, out var cSafetyFactor) &&
                double.TryParse(textBoxSSafetyFactor.Text, out var sSafetyFactor) &&
                double.TryParse(textBoxRSafetyFactor.Text, out var rSafetyFactor) &&
                double.TryParse(textBoxAcc.Text, out var acc))
            {
                _projectPlugIn.CurrentBeam.Gammac = cSafetyFactor;
                _projectPlugIn.CurrentBeam.Gammas = sSafetyFactor;
                _projectPlugIn.CurrentBeam.Gammar = rSafetyFactor;
                _projectPlugIn.CurrentBeam.Acc = acc;
                UpdateOnlyResults();
            }

               

            
        }

        private void updateCustomMaterialStrengths(object sender, EventArgs e)
        {

            PredefinedCrossSection pc = (PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec;

            if (comboBoxConcreteS.SelectedItem.ToString() == "Custom")

                

                if (double.TryParse(textBoxConcreteStrength.Text, out var strength))
                {
                    pc.ConcreteMaterial.Fck = -strength * Math.Pow(10, 6);

                    List<GeometryLarge> l = pc.GetGeometryLarges();
                    foreach (GeometryLarge large in l)
                    {
                        if (large.Material.GetType() == typeof(ConcreteMaterial))
                        {
                            ConcreteMaterial cm = (ConcreteMaterial)large.Material;
                            cm.Fck = -strength * Math.Pow(10, 6);
                        }
                    }
                }
                else
                    MessageBox.Show("Invalid concrete strength value");
            if (comboBoxReinfS.SelectedItem.ToString() == "Custom")
            {

                
                if (double.TryParse(textBoxReinfStrength.Text, out var strength))
                {
                    pc.ReinfMaterial.Fyk = strength * Math.Pow(10, 6);
                    List<Reinforcement> lr = _projectPlugIn.CurrentBeam.CrossSec.GetReinforcements();
                    if (lr.Count != 0)
                    {
                        SteelMaterial r = (SteelMaterial)lr[0].Material;
                        r.Fyk = strength * Math.Pow(10, 6);
                    }
                }
                else
                    MessageBox.Show("Invalid reinforcement strength value");
            }
            if (comboBoxSteelS.SelectedItem.ToString() == "Custom")
                if (comboBoxSteelS.Enabled)
                {
                    
                    if (double.TryParse(textBoxSteelStrength.Text, out var strength))
                    {
                        pc.SteelMaterial.Fyk = strength * Math.Pow(10, 6);


                        List<GeometryLarge> l = _projectPlugIn.CurrentBeam.CrossSec.GetGeometryLarges();
                        foreach (GeometryLarge large in l)
                        {
                            if (large.Material.GetType() == typeof(SteelMaterial))
                            {
                                SteelMaterial sm = (SteelMaterial)large.Material;
                                sm.Fyk = strength * Math.Pow(10, 6);
                            }
                        }
                    }
                    else
                        MessageBox.Show("Invalid steel strength value");
                }



            UpdateOnlyResults();


        }

        private void updateMaterialStrengthsEvent(object sender, EventArgs e)
        {



            if (comboBoxConcreteS.SelectedItem.ToString() == "Custom")
                textBoxConcreteStrength.ReadOnly = false;
            else
                textBoxConcreteStrength.ReadOnly = true;
            if (comboBoxReinfS.SelectedItem.ToString() == "Custom")
                textBoxReinfStrength.ReadOnly = false;
            else
                textBoxReinfStrength.ReadOnly = true;
            if (comboBoxSteelS.SelectedItem.ToString() == "Custom")
                textBoxSteelStrength.ReadOnly = false;
            else
                textBoxSteelStrength.ReadOnly = true;

            PredefinedCrossSection pd = (PredefinedCrossSection)_projectPlugIn.CurrentBeam.CrossSec;


            textBoxConcreteStrength.Leave -= updateCustomMaterialStrengths;
            textBoxSteelStrength.Leave -= updateCustomMaterialStrengths;
            textBoxReinfStrength.Leave -= updateCustomMaterialStrengths;

            textBoxConcreteStrength.Text = (-pd.ConcreteMaterial.Fck * Math.Pow(10, -6)).ToString();
            if (pd.HasSteelShell)
                textBoxSteelStrength.Text = (pd.SteelMaterial.Fyk * Math.Pow(10, -6)).ToString();
            textBoxReinfStrength.Text = (pd.ReinfMaterial.Fyk * Math.Pow(10, -6)).ToString();

            textBoxConcreteStrength.Leave += updateCustomMaterialStrengths;
            textBoxSteelStrength.Leave += updateCustomMaterialStrengths;
            textBoxReinfStrength.Leave += updateCustomMaterialStrengths;

            UpdateOnlyResults();

        }

        private void UpdateLoadCasesEvent(object sender, EventArgs e)
        {
            UpdateAllLoadCases();
        }

        private void radioButtonRect_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRect.Checked)
            {
                
                labelHeigthOrDiameter.Text = "Height =";
                labelAmountHeigth.Text = "Amount Height =";
                labelWidth.Visible = true;
                textBoxWidth.Visible = true;
                labelWidthUnit.Visible = true;
                labelAmountWidth.Visible = true;
                textBoxAmountW.Visible = true;
                labelAmountWidthUnit.Visible = true;
                comboBoxSteelS.Enabled = true;
            }
            else
            {
                labelHeigthOrDiameter.Text = "Diameter =";
                labelAmountHeigth.Text = "Amount =";
                labelWidth.Visible = false;
                textBoxWidth.Visible = false;
                labelWidthUnit.Visible = false;
                labelAmountWidth.Visible = false;
                textBoxAmountW.Visible = false;
                labelAmountWidthUnit.Visible = false;
                comboBoxSteelS.Enabled = false;
                
            }
            if (UpdateCrossSectionValues())
                UpdateResults();
            ProjectPlugIn.Instance.ActiveDoc.Views.Redraw();
        }

        private void dataGridViewLoad_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            dataGridViewLoads.Rows[dataGridViewLoads.Rows.Count - 1].Cells[0].Value =
                dataGridViewLoads.Rows.Count;
        }

        private void UpdateAllLoadCases()
        {
            for (int i = 0; i < dataGridViewLoads.RowCount; i++)
            {
                if (checkThatAllValuesAreGiven(i) && _projectPlugIn.CurrentBeam.CrossSec != null)
                {
                    ColLoadCase clc;
                    if ( _projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (i + 1).ToString()) != null &&
                        _projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (i + 1).ToString()).GetType() == typeof(ColLoadCase))
                    {
                        clc = (ColLoadCase)_projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (i + 1).ToString());
                        clc.N_Ed = double.Parse(dataGridViewLoads.Rows[i].Cells[1].Value.ToString()) * 1000;
                        clc.M_EzTop = double.Parse(dataGridViewLoads.Rows[i].Cells[2].Value.ToString()) * 1000;
                        clc.M_EzBottom = double.Parse(dataGridViewLoads.Rows[i].Cells[3].Value.ToString()) * 1000;
                        clc.M_EyTop = double.Parse(dataGridViewLoads.Rows[i].Cells[4].Value.ToString()) * 1000;
                        clc.M_EyBottom = double.Parse(dataGridViewLoads.Rows[i].Cells[5].Value.ToString()) * 1000;
                        clc.Ratio = double.Parse(textBox_MSfactor.Text);
                        clc.Ccurve = double.Parse(textBox_C.Text);
                        clc.CalcUtilization();

                    }
                    else
                    {
                        clc = new ColLoadCase(
                            double.Parse(dataGridViewLoads.Rows[i].Cells[1].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[i].Cells[2].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[i].Cells[3].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[i].Cells[4].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[i].Cells[5].Value.ToString()) * 1000,
                            _projectPlugIn.CurrentBeam as Column,
                            double.Parse(textBox_MSfactor.Text),
                            (i + 1).ToString(),
                            double.Parse(textBox_C.Text),LimitState.Ultimate);
                    }
                    updateDataGridViewResults(clc, i);
                }
            }
            AddLoadCasesToTheCharts();
        }


        private void updateDataGridViewResults(ColLoadCase clc, int rowIndex)
        {
            if (clc.Col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature1])
            {
                dataGridViewLoads.Rows[rowIndex].Cells[6].Value = Math.Round(clc.M_Edz_NomCurv.M_Ed * Math.Pow(10, -3), 1);
                dataGridViewLoads.Rows[rowIndex].Cells[7].Value = Math.Round(clc.M_Edy_NomCurv.M_Ed * Math.Pow(10, -3), 1);
                dataGridViewLoads.Rows[rowIndex].Cells[8].Value = clc.ZorY == true ? "Z-direction" : "Y-direction";
                dataGridViewLoads.Rows[rowIndex].Cells[9].Value = Math.Round(clc.Utilization[ColumnCalculationMethod.NominalCurvature1], 2);
            }
            else
            {
                dataGridViewLoads.Rows[rowIndex].Cells[6].Value = "";
                dataGridViewLoads.Rows[rowIndex].Cells[7].Value = "";
                dataGridViewLoads.Rows[rowIndex].Cells[8].Value = "";
                dataGridViewLoads.Rows[rowIndex].Cells[9].Value = "";
            }
            if (clc.Col.ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness2])
                dataGridViewLoads.Rows[rowIndex].Cells[10].Value = Math.Round(clc.Utilization[ColumnCalculationMethod.NominalStiffness2], 2);
            else
                dataGridViewLoads.Rows[rowIndex].Cells[10].Value = "";
        }



        private bool GetSelectedLoadCase(out ColLoadCase colLoadcase)
        {
            if (dataGridViewLoads.SelectedCells.Count != 0)
            {
                if (_projectPlugIn.CurrentBeam.CrossSec != null)
                {
                    int rowIndex = dataGridViewLoads.SelectedCells[0].RowIndex;
                    if (_projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (rowIndex + 1).ToString()) != null)
                    {
                        Loadcase lcModify = _projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (rowIndex + 1).ToString());
                        if (lcModify.GetType() == typeof(ColLoadCase))
                        {
                            colLoadcase = (ColLoadCase)lcModify;
                            return true;
                        }
                    }
                }
            }
            colLoadcase = null;
            return false;
            
        }


        private void dataGridViewLoads_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (checkThatAllValuesAreGiven(e.RowIndex) && _projectPlugIn.CurrentBeam.CrossSec != null)
            {
                ColLoadCase clc;
                if (_projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (e.RowIndex + 1).ToString()) != null &&
                    _projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (e.RowIndex + 1).ToString()).GetType() == typeof(ColLoadCase))
                {
                    clc = (ColLoadCase)_projectPlugIn.CurrentBeam.LoadCases.
                        Find(o => o.Name == dataGridViewLoads.Rows[e.RowIndex].Cells[0].Value.ToString());
                    switch (e.ColumnIndex)
                    {
                        case 1: clc.N_Ed = double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[1].Value.ToString()) * 1000; break;
                        case 2: clc.M_EzTop = double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[2].Value.ToString()) * 1000; break;
                        case 3: clc.M_EzBottom = double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[3].Value.ToString()) * 1000; break;
                        case 4: clc.M_EyTop = double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[4].Value.ToString()) * 1000; break;
                        case 5: clc.M_EyBottom = double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[5].Value.ToString()) * 1000; break;
                        default: break;
                    }
                    clc.UpdateResults();
                }
                else
                {
                    clc = new ColLoadCase(
                            double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[1].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[2].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[3].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[4].Value.ToString()) * 1000,
                            double.Parse(dataGridViewLoads.Rows[e.RowIndex].Cells[5].Value.ToString()) * 1000,
                            _projectPlugIn.CurrentBeam as Column,
                            double.Parse(textBox_MSfactor.Text),
                            (e.RowIndex + 1).ToString(),
                            double.Parse(textBox_C.Text),LimitState.Ultimate);

                }
                updateDataGridViewResults(clc, e.RowIndex);
            }

            //Add values to chart
            ColLoadCase cCase = _projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == (e.RowIndex + 1).ToString()) as ColLoadCase;
            if (cCase != null)
            {
                AddLoadCasesToTheCharts();
            }
            
        }

        private void AddLoadCasesToTheCharts()
        {
            //Remove old values
            int i = 0;
            while (i < chartRectMz.Series.Count)
            {
                if (int.TryParse(chartRectMz.Series[i].Name, out var temp))
                    chartRectMz.Series.Remove(chartRectMz.Series[i]);
                else
                    i++;
            }

            i = 0;
            while (i < chartRectMy.Series.Count)
            {
                if (int.TryParse(chartRectMy.Series[i].Name, out var temp))
                    chartRectMy.Series.Remove(chartRectMy.Series[i]);
                else
                    i++;
            }


            foreach (DataGridViewRow row in dataGridViewLoads.Rows)
            {

                string name = row.Cells[0].Value.ToString();

                ColLoadCase cCase = (ColLoadCase)_projectPlugIn.CurrentBeam.LoadCases.Find(o => o.Name == name);

                //Add new values
                if (cCase != null)
                {
                    if (((Column)_projectPlugIn.CurrentBeam).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature1])
                    {
                        if (row.Cells.Count != 0 && row.Cells[6].Value != null && row.Cells[6].Value.ToString() != "")
                        {
                            ChartManipulationTools.CreateNewPointChart(row.Cells[0].Value.ToString(), chartRectMz);
                            chartRectMz.Series[name].Points.AddXY(cCase.M_Edz_NomCurv.M_Ed * 0.001,
                                cCase.N_Ed * 0.001);

                            ChartManipulationTools.CreateNewPointChart(name, chartRectMy);
                            chartRectMy.Series[name].Points.AddXY(cCase.M_Edy_NomCurv.M_Ed * 0.001,
                                cCase.N_Ed * 0.001);
                        }
                    }

                    if (((Column)_projectPlugIn.CurrentBeam).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness2])
                    {
                        if (_projectPlugIn.ChForm == null)
                            _projectPlugIn.ChForm = new ChartForm();

                        _projectPlugIn.ChForm.SetChartValues(cCase);

                    }
                }

            }

        }

        private void dataGridViewLoads_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_projectPlugIn.CurrentBeam.CrossSec != null && _projectPlugIn.ChForm != null &&
                GetSelectedLoadCase(out var clc))
                _projectPlugIn.ChForm.SetChartValues(clc);
        }
        
        private bool checkThatAllValuesAreGiven(int i)
        {
            DataGridViewCellCollection cc = dataGridViewLoads.Rows[i].Cells;
            int k = 1;
            while (k < 6)
            {
                
                if (dataGridViewLoads.Rows[i].Cells[k].Value == null ||
                    !double.TryParse(dataGridViewLoads.Rows[i].Cells[k].Value.ToString(),out var s))
                {
                    return false;
                }
                k += 1;
            }
            if (textBox_C.Text == null ||
                    !double.TryParse(textBox_C.Text, out var u) || !double.TryParse(textBox_MSfactor.Text, out var r)
                    || textBox_MSfactor.Text == null)
                return false;


                return true;
        }

        private void dataGridViewLoads_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            dataGridViewLoads.Rows[dataGridViewLoads.Rows.Count - 1].Cells[0].Value =
                dataGridViewLoads.Rows.Count;
        }

        private void checkBoxNominalStiffness_CheckedChanged(object sender, EventArgs e)
        {
            if (_projectPlugIn.CurrentBeam != null && _projectPlugIn.CurrentBeam.GetType() == typeof(Column))
                ((Column)_projectPlugIn.CurrentBeam).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalStiffness2] =
                    checkBoxNominalStiffness.Checked;
            if (!checkBoxNominalStiffness.Checked)
            {
                if (_projectPlugIn.ChForm != null)
                {
                    _projectPlugIn.ChForm.ClearChartValues();
                }
            }
            UpdateAllLoadCases();
        }

        private void checkBoxNominalCurvature1_CheckedChanged(object sender, EventArgs e)
        {
            if (_projectPlugIn.CurrentBeam != null && _projectPlugIn.CurrentBeam.GetType() == typeof(Column))
                ((Column)_projectPlugIn.CurrentBeam).ColumnCalcSettings.ColumnCalMethod[ColumnCalculationMethod.NominalCurvature1] =
                    checkBoxNominalCurvature1.Checked;

            UpdateAllLoadCases();
        }

        private void OnlyInts_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void Only_Ints_commas_dashes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '-' && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void buttonRFEMImport_Click(object sender, EventArgs e)
        {
            if (textBoxLoadCaseNumbers.Text != "" && textBoxMemberNumber.Text != "")
            {
                List<int> loadcases = CreateLoadCaseList(textBoxLoadCaseNumbers.Text);
                int memberNumber = int.Parse(textBoxMemberNumber.Text);
                LoadingType loadingType = radioButtonLoadCase.Checked ? LoadingType.LoadCaseType : LoadingType.LoadCombinationType;

                List<Tuple<int,Vector3d,Vector3d>> results = ColumnCalculations.GetMemberIternalForces(loadcases, loadingType, memberNumber);
                DataGridViewRow r =(DataGridViewRow) dataGridViewLoads.Rows[0].Clone();
                if (results.Count != 0) dataGridViewLoads.Rows.Clear();
                int i = 0;
                foreach (Tuple<int,Vector3d,Vector3d> item in results)
                {
                    r = (DataGridViewRow) r.Clone();
                    double NormalForce = Math.Min(item.Item2.X, item.Item3.X);
                    dataGridViewLoads.Rows.Add(r);
                    dataGridViewLoads.Rows[i].Cells[0].Value = item.Item1;
                    dataGridViewLoads.Rows[i].Cells[1].Value = Math.Round(NormalForce* Math.Pow(10, -3),1);
                    dataGridViewLoads.Rows[i].Cells[2].Value = Math.Round(item.Item3.Z * Math.Pow(10, -3),1);
                    dataGridViewLoads.Rows[i].Cells[3].Value = Math.Round(item.Item2.Z * Math.Pow(10, -3),1);
                    dataGridViewLoads.Rows[i].Cells[4].Value = Math.Round(item.Item3.Y * Math.Pow(10, -3),1);
                    dataGridViewLoads.Rows[i].Cells[5].Value = Math.Round(item.Item2.Y * Math.Pow(10, -3),1);
                    i++;
                }
                UpdateAllLoadCases();
            }

            
        }

        private List<int> CreateLoadCaseList(string text)
        {
            HashSet<int> numbers = new HashSet<int>();

            string[] st = text.Split(',');
            foreach (string number in st)
            {
                string[] str = number.Split('-');
                if (str.Length == 1)
                    numbers.Add(int.Parse(str[0]));
                else
                {
                    List<int> e = Enumerable.Range(int.Parse(str[0]), int.Parse(str[1]) - int.Parse(str[0]) + 1).ToList();
                    e.ForEach(o => numbers.Add(o));
                }
                
            }
            return numbers.ToList();
        }
    }




}
