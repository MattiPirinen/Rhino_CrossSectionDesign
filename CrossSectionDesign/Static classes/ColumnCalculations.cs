using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrossSectionDesign.Classes_and_structures;
using Dlubal.RFEM5;
using MoreLinq;
using Rhino.Geometry;
using CrossSectionDesign.Enumerates;
namespace CrossSectionDesign.Static_classes
{
    public class ColumnCalculations: RFEMConnection
    {
        public static List<double> GetUtilizations(Classes_and_structures.Column col, List<int> memberNumbs)
        {
            
            //Create new Columns for each of the imported column
            foreach (int no in memberNumbs)
            {
                ProjectPlugIn.Instance.Beams.Add(col.ShallowCopy(no.ToString()));
            }
            
            List<double> utilzTot = new List<double>();
            OpenConnection();
            try
            {
                Dictionary<int, Tuple<int, double>> utilz = new Dictionary<int, Tuple<int, double>>();
                foreach (int numb in memberNumbs)
                {
                    utilz.Add(numb, Tuple.Create(0, 0.0));
                }

                ICalculation calc = RModel.GetCalculation();
                IModelData data = RModel.GetModelData();
                ILoads loads = RModel.GetLoads();
                LoadCombination[] lc = loads.GetLoadCombinations();
                
                //int[] numbs = lc.Select(o => o.Loading.No).ToArray();
                //numbs = Array.FindAll(numbs, o => o > 100 || o < 200);
                List<int> numbs = new List<int>() { 149, 150,153,154,157,158,159,160,166 };
                int k = 0;
                foreach (int number in numbs)
                {
                    IResults res = calc.GetResultsInFeNodes(LoadingType.LoadCombinationType, number);

                    MemberForces[] mfs = res.GetMembersInternalForces(true);

                    ProjectPlugIn ppi = ProjectPlugIn.Instance;
                    SimpleLoadCase slc;
                    for (int i = 0; i < mfs.Length; i++)
                    {
                        if (memberNumbs.Contains(mfs[i].MemberNo))
                        {

                            slc = new SimpleLoadCase(mfs[i].Forces.X, mfs[i].Moments.Z, mfs[i].Moments.Y,
                                col, number.ToString(), LimitState.Ultimate);

                            if (slc.Utilization > utilz[mfs[i].MemberNo].Item2)
                                utilz[mfs[i].MemberNo] = Tuple.Create(i, slc.Utilization);
                        }
                    }

                    foreach (int key in utilz.Keys)
                    {

                        Classes_and_structures.Column tempCol = (Classes_and_structures.Column)ProjectPlugIn.Instance.Beams
                                .Find(o => o.Name == key.ToString());
                        tempCol.LoadCases.Add(new SimpleLoadCase(mfs[utilz[key].Item1].Forces.X, mfs[utilz[key].Item1].Moments.Z, mfs[utilz[key].Item1].Moments.Y,
                                tempCol, number.ToString(),LimitState.Ultimate));
                    }
                    mfs = null;
                    
                    k++;

                }

                foreach (int memberNo in memberNumbs)
                {
                    Classes_and_structures.Column tempCol = (Classes_and_structures.Column)ProjectPlugIn.Instance.Beams
                            .Find(o => o.Name == memberNo.ToString());

                    SimpleLoadCase[] temp = tempCol.LoadCases.Select(o => o as SimpleLoadCase).ToArray();
                    utilzTot.Add(temp.MaxBy(o => o.Utilization).Utilization);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Cleans Garbage collector for releasing all COM interfaces and objects
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
            finally
            {
                CloseConnection();
            }

            return utilzTot;
        }

        public static List<Tuple<int, Vector3d,Vector3d>> GetMemberIternalForces(List<int> loadCases, LoadingType loadingType, int memberNumber)
        {
            OpenConnection();

            List<Tuple<int, Vector3d, Vector3d>> results = new List<Tuple<int, Vector3d, Vector3d>>();

            try
            {
                ICalculation calc = RModel.GetCalculation();
                IModelData data = RModel.GetModelData();
                ILoads loads = RModel.GetLoads();
                LoadCombination[] lc = loads.GetLoadCombinations();
                foreach (int loadcase in loadCases)
                {
                    try
                    {
                        IResults res = calc.GetResultsInFeNodes(loadingType, loadcase);
                        MemberForces[] mf = res.GetMemberInternalForces(memberNumber, ItemAt.AtNo, true);
                        Vector3d startForces = new Vector3d(mf[0].Forces.X, mf[0].Moments.Y, mf[0].Moments.Z);
                        Vector3d endForces = new Vector3d(mf[mf.Length - 1].Forces.X, mf[mf.Length - 1].Moments.Y, mf[mf.Length - 1].Moments.Z);
                        results.Add(Tuple.Create(loadcase, startForces, endForces));
                    }
                    catch { continue; }

                }


                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Cleans Garbage collector for releasing all COM interfaces and objects
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
            finally
            {
                CloseConnection();
                
            }
            return results;
            

        }


    }
}
