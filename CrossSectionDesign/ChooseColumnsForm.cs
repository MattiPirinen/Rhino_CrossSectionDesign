using CrossSectionDesign.Classes_and_structures;
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
    public partial class ChooseColumnsForm : Form
    {
        public ChooseColumnsForm()
        {
            InitializeComponent();
            FormClosing += MyForm_FormClosing;
            InitializeComponentValues();
            VisibleChanged += updateListBox;
        }

        private void updateListBox(object sender, EventArgs e)
        {
            int index = listBoxColumns.SelectedIndex;
            listBoxColumns.Items.Clear();
            listBoxColumns.Items.AddRange(ProjectPlugIn.Instance.Beams.Select(o => o.Name).Cast<object>().ToArray());
            if (listBoxColumns.Items.Count > index)
                listBoxColumns.SelectedIndex = index;

        }

        private void InitializeComponentValues()
        {
            listBoxColumns.Items.AddRange(ProjectPlugIn.Instance.Beams.Select(o => o.Name).Cast<object>().ToArray());
        }

        private void MyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public string GetChosenColumn()
        {
            if (listBoxColumns.SelectedItem == null)
            {
                MessageBox.Show("No Column were chosen");
                return null;
            }
            else
            {
                return listBoxColumns.SelectedItem.ToString();
            }
            
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
