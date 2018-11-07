using CrossSectionDesign.Static_classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrossSectionDesign
{
    public partial class ClimateConditionsForm : Form
    {
        public ClimateConditionsForm()
        {
            InitializeComponent();
            InitializeComponentValues();
            FormClosing += MyForm_FormClosing;
        }
        private void MyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public Tuple<int,int,int,int> ReturnValues()
        {
            string rh = comboBoxRH.SelectedItem.ToString();
            rh = (string)rh.Take(rh.Length - 1);
            if (!int.TryParse(textBoxStartTime.Text, out var starttime))
            {
                MessageBox.Show("Loading start time in Climateconditions was written wrong. 28d is used instead");
                starttime = 28;
            }
            if (!int.TryParse(textBoxEndTime.Text, out var endtime))
            {
                MessageBox.Show("Loading end time in Climateconditions was written wrong. 365000d is used instead");
                endtime = 28;
            }


            return Tuple.Create(int.Parse(comboBoxDesWorkingLife.SelectedItem.ToString().Split(' ')[0]),
                int.Parse(rh),starttime,endtime);
        }


        private void InitializeComponentValues()
        {
            comboBoxDesWorkingLife.Items.AddRange(ComboboxValues.DES_WORK_LIFE);
            comboBoxDesWorkingLife.SelectedItem = "50 years";

            comboBoxRH.Items.AddRange(ComboboxValues.RH);
            comboBoxRH.SelectedItem = "40%";

            comboBoxExposureClass.Items.AddRange(ComboboxValues.EXPOSURE_CLASSES);
            comboBoxExposureClass.SelectedItem = "XC3";

            textBoxStartTime.Text = "28";
            textBoxEndTime.Text = "36500";
        }
    }
}
