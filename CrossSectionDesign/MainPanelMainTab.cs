using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrossSectionDesign.Classes_and_structures;
using Rhino;
using CrossSectionDesign.Enumerates;


namespace CrossSectionDesign
{

    // Code related to the Main menu tab in the Main Panel
    public partial class MainPanel
    {

        //Changes the current cross section
        private void listBoxCrossSecs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxCrossSecs.SelectedIndex != -1)
            {
                _projectPlugIn.CurrentBeam = _projectPlugIn.Beams[listBoxCrossSecs.SelectedIndex];
                _projectPlugIn.SelectedBeamIndex = listBoxCrossSecs.SelectedIndex;
                if ( _projectPlugIn.CurrentBeam.CrossSec is PredefinedCrossSection)
                {
                    //Update dataGridviewRows
                    DataGridViewRow r = (DataGridViewRow)dataGridViewLoads.Rows[0].Clone();
                    dataGridViewLoads.Rows.Clear();
                    int i = 0;
                    foreach (LoadCase lc in _projectPlugIn.CurrentBeam.LoadCases)
                    {
                        if (lc.GetType() == typeof(ColLoadCase))
                        {
                            ColLoadCase clc = (ColLoadCase)lc;
                            r = (DataGridViewRow)r.Clone();
                            double NormalForce = 
                            dataGridViewLoads.Rows.Add(r);
                            dataGridViewLoads.Rows[i].Cells[0].Value = clc.Name;
                            dataGridViewLoads.Rows[i].Cells[1].Value = clc.N_Ed * Math.Pow(10, -3);
                            dataGridViewLoads.Rows[i].Cells[2].Value = clc.M_EzTop * Math.Pow(10, -3);
                            dataGridViewLoads.Rows[i].Cells[3].Value = clc.M_EzBottom * Math.Pow(10, -3);
                            dataGridViewLoads.Rows[i].Cells[4].Value = clc.M_EyTop * Math.Pow(10, -3);
                            dataGridViewLoads.Rows[i].Cells[5].Value = clc.M_EyBottom * Math.Pow(10, -3);
                            i++;
                        }
                    }
                    UpdatePredefinedFromCrossSection();
                    EnablePredefinedCrossSection();
                }
                else
                {
                    UpdateValuesGenericCrossSection();
                    EnableGenericCrossSection();
                }
                    

                
                ZoomToCurrentBeam();
            }


        }

        private void UpdateValuesGenericCrossSection()
        {
            ShowStressResults(false);


            dataGridView_GeometryLarge.Rows.Clear();
            dataGridView_Reinforcement.Rows.Clear();

            List<GeometryLarge> gl = _projectPlugIn.CurrentBeam.CrossSec.GetGeometryLarges();
            gl.ForEach(temp => dataGridView_GeometryLarge.Rows.Add(temp.Id, temp.Material.GetType()
                == typeof(SteelMaterial) ? "Steel" : "Concrete"));
            
            List<Reinforcement> rfList = _projectPlugIn.CurrentBeam.CrossSec.GetReinforcements();
            rfList.ForEach(reinforcement => dataGridView_Reinforcement.Rows.Add(reinforcement.Id,
                "Reinforcement", reinforcement.Diameter*Math.Pow(10,3)));

            switch (_projectPlugIn.CurrentBeam.CrossSec.MaterialResultShown)
            {
                case MaterialType.Concrete:
                    radioButtonConcrete.Checked = true;
                    break;
                case MaterialType.Steel:
                    radioButtonSteel.Checked = true;
                    break;
                default:
                    break;
            }

            radioButtonULS.Checked = true;

            chartFreeMz.Series["Strength"].Points.Clear();
            if (_projectPlugIn.CurrentBeam.ClimateCond != null)
            {
                textBoxFreeT.Text = _projectPlugIn.CurrentBeam.ClimateCond.T.ToString();
                textBoxFreeT0.Text = _projectPlugIn.CurrentBeam.ClimateCond.T0.ToString();
                comboBoxFreeRH.SelectedItem = _projectPlugIn.CurrentBeam.ClimateCond.RH.ToString() + "%";
            }
            else
            {
                textBoxFreeT.Text = "36500";
                textBoxFreeT0.Text = "28";
                comboBoxFreeRH.SelectedItem = "40%";
            }
            checkBoxShowCrackWidth.Checked = _projectPlugIn.CrackWidthConduit.Enabled;
            checkBoxShowStresses.Checked = _projectPlugIn.ResultConduit.Enabled;
            radioButtonSteel.Checked = true;


        }

        private void SetDefaultGenericCrossSectionValues()
        {
            ShowStressResults(false);
            radioButtonULS.Checked = true;
            switch (_projectPlugIn.CurrentBeam.CrossSec.MaterialResultShown)
            {
                case MaterialType.Concrete:
                    radioButtonConcrete.Checked = true;
                    break;
                case MaterialType.Steel:
                    radioButtonSteel.Checked = true;
                    break;
                default:
                    break;
            }
            
            textBoxMz.Text = "-100";
            textBoxForce.Text = "0";
            textBoxMy.Text = "0";
            textBoxFreeT.Text = "36500";
            textBoxFreeT0.Text = "28";
        }

        //Creates a new generic cross section
        private void buttonNewGenerCross_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text != "")
            {

                radioButtonSteel.Checked = true;

                EnableGenericCrossSection();
                string name = textBoxName.Text;
                listBoxCrossSecs.Items.Add(name);

                Beam beam = new Beam(name, 1.0, 1.5, 1.15, 0.85)
                {
                };
                beam.CrossSec = new CrossSection(name, beam)
                {
                    ConcreteMaterial = new ConcreteMaterial(comboBoxMaterialGeom.SelectedItem.ToString(),beam)
                };

                _projectPlugIn.Beams.Add(beam);
                _projectPlugIn.SelectedBeamIndex = _projectPlugIn.Beams.Count - 1;
                _projectPlugIn.CurrentBeam = beam;
                tabControlMain.SelectedTab = tabPageCrossSection;

                dataGridView_GeometryLarge.Rows.Clear();
                dataGridView_Reinforcement.Rows.Clear();

                chartFreeMz.Series["Strength"].Points.Clear();


                SetDefaultGenericCrossSectionValues();
                ZoomToCurrentBeam();
            }
            else
            {
                MessageBox.Show("No name was given", "Error", MessageBoxButtons.OK);
            }

        }


        //Creates a new rect cross section
        private void buttonNewRectCroSec_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text != "")
            {
                EnablePredefinedCrossSection();
                string name = textBoxName.Text;
                listBoxCrossSecs.Items.Add(name);
                if (_projectPlugIn.ChooseColForm != null)
                {
                    listBoxCrossSecs.Items.Add(name);
                }
                dataGridViewLoads.Rows.Clear();
                dataGridViewLoads.Rows[0].Cells[0].Value = "1";
                textBox_C.Text = "9.9";
                textBox_MSfactor.Text = "0.68";
                
                CreateDefaultRectCrossSection(name);
                checkBoxNominalCurvature1.Checked = true;
                tabControlMain.SelectedTab = tabPageRecCross;
            }
            else
            {
                MessageBox.Show("No name was given.", "Error", MessageBoxButtons.OK);
            }

        }

        
    }
}
