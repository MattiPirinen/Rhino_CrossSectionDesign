using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dlubal.RFEM5;
using CrossSectionDesign.Static_classes;
using CrossSectionDesign.Classes_and_structures;
using Rhino.Geometry;

namespace CrossSectionDesign
{
    public partial class RFEMAnalysisForm : Form
    {
        public RFEMAnalysisForm()
        {
            ProjectPlugIn.Instance.RFEMForm = this;
            InitializeComponent();
            InitializeComponentEvents();

            FormClosing += MyForm_FormClosing;
            ProjectPlugIn.Instance.BackGroundConduit.Enabled = false;
            ProjectPlugIn.Instance.GeomConduit = new Display_classes.GeometryConduit
            {
                Enabled = true
            };

            ProjectPlugIn.Instance.ColumnResultConduit = new Display_classes.ResultConduit
            {
                Enabled = false
            };
            radioButton_geometry.Checked = true;
            radioButton_CrossSectionDesign.Checked = true;
        }


        private void LeaveControl(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                Focus();
        }

        private void InitializeComponentEvents()
        {
            textBox_Min.Leave += ChangeColorScale;
            textBox_Max.Leave += ChangeColorScale;

            textBox_Min.KeyDown += LeaveControl;
            textBox_Max.KeyDown += LeaveControl;
        }

        private void MyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }



        private void button_Import_Click(object sender, EventArgs e)
        {
            
            if (textBox_Comment.Text != null)
            {
                try
                {
                    Tuple<Member[], Dlubal.RFEM5.CrossSection[]> res = GetGeometry.GetMembers(textBox_Comment.Text);
                    Member[] members = res.Item1;
                    Dlubal.RFEM5.CrossSection[] crossSecs = res.Item2;
                    if (dataGridViewValues.Rows.Count == 1)
                        dataGridViewValues.Rows.Add();
                    for (int i = 0; i < members.Length; i++)
                    {
                        if (i > dataGridViewValues.Rows.Count - 1)
                            dataGridViewValues.Rows.Add();

                        dataGridViewValues.Rows[i].Cells["No"].Value = members[i].No;
                        //dataGridViewValues.Rows[i].Cells["Length"].Value = Math.Round(members[i].Length,3);
                        Dlubal.RFEM5.CrossSection c = Array.Find(crossSecs, o => o.No == members[i].StartCrossSectionNo);
                        if (c.TextID.Split(' ')[0] == "Rechteck")
                        {
                            double height = double.Parse(c.TextID.Split(' ')[1].Split('/')[1]);
                            double width = double.Parse(c.TextID.Split(' ')[1].Split('/')[0]);
                            dataGridViewValues.Rows[i].Cells["Type"].Value = "Rectangle";
                            dataGridViewValues.Rows[i].Cells["Rect_Height"].Value = height;
                            dataGridViewValues.Rows[i].Cells["Rect_Width"].Value = width;
                        }
                        else if (c.TextID.Split(' ')[0] == "Kreis")
                        {
                            double diam = double.Parse(c.TextID.Split(' ')[1]);
                            dataGridViewValues.Rows[i].Cells["Type"].Value = "Circle";
                            dataGridViewValues.Rows[i].Cells["Circle_Diam"].Value = diam.ToString();
                        }
                        else
                        {
                            MessageBox.Show($"Wrong type cross section. at member No. {members[i].No}. Cannot import", "Error", MessageBoxButtons.OK);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    //Cleans Garbage collector for releasing all COM interfaces and objects
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
            }
        }

        private void button_ClimateCond_Click(object sender, EventArgs e)
        {
            ProjectPlugIn.Instance.ClimateForm.Show();
        }



        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            //Tuple<int, int, int, int> cliValues = ProjectPlugIn.Instance.ClimateForm.ReturnValues();
            string name = ProjectPlugIn.Instance.ChooseColForm.GetChosenColumn();
            if (name == null)
                return;

            Classes_and_structures.Column col =(Classes_and_structures.Column) ProjectPlugIn.Instance.Beams.Find(o => o.Name == name);
            ProjectPlugIn.Instance.Beams.Clear();
            ProjectPlugIn.Instance.Beams.Add(col);

            List<int> memberNumbers = new List<int>();
            foreach (DataGridViewRow row in dataGridViewValues.Rows)
            {
                if (row.Cells["No"].Value != null && 
                    int.TryParse(row.Cells["No"].Value.ToString(),out var temp))
                    memberNumbers.Add(temp);
            }



            List<double> results = ColumnCalculations.GetUtilizations(col,memberNumbers);
            int i = 0;
            List<Tuple<Brep,double>> resultBreps = new List<Tuple<Brep, double>>();

            foreach (DataGridViewRow row in dataGridViewValues.Rows)
            {
                if (i < results.Count)
                {
                    row.Cells["Utilization"].Value = Math.Round(results[i], 2);
                    resultBreps.Add(Tuple.Create(
                        ProjectPlugIn.Instance.beamBreps.Find(o => o.Item1 == (int)row.Cells["No"].Value).Item2,
                        Math.Round(results[i], 2)));
                }
                i++;
            }
            ProjectPlugIn.Instance.GeomConduit.Enabled = false;
            ProjectPlugIn.Instance.ColumnResultConduit.Enabled = true;
            //double maxValue = ProjectPlugIn.Instance.ColumnResultConduit.MaxValue;
            //double minValue = ProjectPlugIn.Instance.ColumnResultConduit.MinValue;
            ProjectPlugIn.Instance.ColorScaleDisplay = new Display_classes.ColorScaleDisplay()
            {
                Enabled = true
            };
            //textBox_Min.Text = Math.Round(minValue, 3).ToString();
            //textBox_Max.Text = Math.Round(maxValue, 3).ToString();
            //ProjectPlugIn.Instance.ColorScaleDisplay.SetColorScale(minValue, maxValue, 0, 0.7, "Utilization");
            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        }

        private void buttonChooseColumns_Click(object sender, EventArgs e)
        {
            ProjectPlugIn.Instance.ChooseColForm.Show();
        }

        private void radioButton_geometry_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_geometry.Checked)
            {
                ProjectPlugIn.Instance.ColumnResultConduit.Enabled = false;
                ProjectPlugIn.Instance.GeomConduit.Enabled = true;
                groupBox_Display.Enabled = false;

            }
            else
            {
                ProjectPlugIn.Instance.ColumnResultConduit.Enabled = true;
                ProjectPlugIn.Instance.GeomConduit.Enabled = false;
                groupBox_Display.Enabled = true;
            }
        }

        private void ChangeColorScale(object sender, EventArgs e)
        {
            if (double.TryParse(textBox_Max.Text, out var max) &&
                double.TryParse(textBox_Min.Text, out var min))
            {
                //ProjectPlugIn.Instance.ColumnResultConduit.SetMaxAndMin(max, min);
                ProjectPlugIn.Instance.ColorScaleDisplay.SetColorScale(min, max, 0, 0.7, "Utilization");
                Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
            }
        }


        private void buttonSet_Click(object sender, EventArgs e)
        {


        }

        private void radioButton_CrossSectionDesign_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_CrossSectionDesign.Checked)
            {
                ChangeValuesToCrossSectionDesign();
            }
        }

        private void ChangeValuesToCrossSectionDesign()
        {
            dataGridViewValues.Columns.Clear();
            DataGridViewTextBoxCell template = new DataGridViewTextBoxCell();

            DataGridViewColumn column = new DataGridViewColumn(template)
            {
                Width = 70,
                Name = "No",
                HeaderText = "No"
            };
            dataGridViewValues.Columns.Add(column);




            column = new DataGridViewColumn(template)
            {
                Width = 70,
                Name = "Type",
                HeaderText = "Type"
            };
            dataGridViewValues.Columns.Add(column);

            column = new DataGridViewColumn(template)
            {
                Width = 70,
                Name = "Type",
                HeaderText = "Type"
            };
            dataGridViewValues.Columns.Add(column);

            column = new DataGridViewColumn(template)
            {
                Width = 70,
                Name = "Circle_Diam",
                HeaderText = "Circle Diameter"
            };
            dataGridViewValues.Columns.Add(column);

            column = new DataGridViewColumn(template)
            {
                Width = 70,
                Name = "Rect_Height",
                HeaderText = "Rect Height"
            };
            dataGridViewValues.Columns.Add(column);


            column = new DataGridViewColumn(template)
            {
                Width = 70,
                Name = "Rect_Width",
                HeaderText = "Rect Width"
            };
            dataGridViewValues.Columns.Add(column);


            column = new DataGridViewColumn(template)
            {
                Width = 70,
                Name = "Utilization",
                HeaderText = "Utilization"
            };
            dataGridViewValues.Columns.Add(column);
        }
    }
}
