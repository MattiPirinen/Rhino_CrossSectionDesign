namespace CrossSectionDesign
{
    partial class MainPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.StripLine stripLine1 = new System.Windows.Forms.DataVisualization.Charting.StripLine();
            System.Windows.Forms.DataVisualization.Charting.StripLine stripLine2 = new System.Windows.Forms.DataVisualization.Charting.StripLine();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.comboBoxDiam = new System.Windows.Forms.ComboBox();
            this.comboBoxMaterialType = new System.Windows.Forms.ComboBox();
            this.comboBoxMaterialReinf = new System.Windows.Forms.ComboBox();
            this.comboBoxMaterialGeom = new System.Windows.Forms.ComboBox();
            this.buttonCalculate = new System.Windows.Forms.Button();
            this.checkBoxShowDivision = new System.Windows.Forms.CheckBox();
            this.buttonAddReinf = new System.Windows.Forms.Button();
            this.buttonAddGeometry = new System.Windows.Forms.Button();
            this.tabPageResults = new System.Windows.Forms.TabPage();
            this.chartResults = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.textBoxForce = new System.Windows.Forms.TextBox();
            this.textBoxMoment = new System.Windows.Forms.TextBox();
            this.buttonCalc2 = new System.Windows.Forms.Button();
            this.tabControlMain.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.tabPageResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartResults)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageMain);
            this.tabControlMain.Controls.Add(this.tabPageResults);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(502, 475);
            this.tabControlMain.TabIndex = 1;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.buttonCalc2);
            this.tabPageMain.Controls.Add(this.textBoxMoment);
            this.tabPageMain.Controls.Add(this.textBoxForce);
            this.tabPageMain.Controls.Add(this.comboBoxDiam);
            this.tabPageMain.Controls.Add(this.comboBoxMaterialType);
            this.tabPageMain.Controls.Add(this.comboBoxMaterialReinf);
            this.tabPageMain.Controls.Add(this.comboBoxMaterialGeom);
            this.tabPageMain.Controls.Add(this.buttonCalculate);
            this.tabPageMain.Controls.Add(this.checkBoxShowDivision);
            this.tabPageMain.Controls.Add(this.buttonAddReinf);
            this.tabPageMain.Controls.Add(this.buttonAddGeometry);
            this.tabPageMain.Location = new System.Drawing.Point(4, 25);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(494, 446);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Main Page";
            this.tabPageMain.UseVisualStyleBackColor = true;
            this.tabPageMain.Click += new System.EventHandler(this.tabPageMain_Click);
            // 
            // comboBoxDiam
            // 
            this.comboBoxDiam.FormattingEnabled = true;
            this.comboBoxDiam.Location = new System.Drawing.Point(287, 47);
            this.comboBoxDiam.Name = "comboBoxDiam";
            this.comboBoxDiam.Size = new System.Drawing.Size(93, 24);
            this.comboBoxDiam.TabIndex = 18;
            // 
            // comboBoxMaterialType
            // 
            this.comboBoxMaterialType.FormattingEnabled = true;
            this.comboBoxMaterialType.Location = new System.Drawing.Point(190, 10);
            this.comboBoxMaterialType.Name = "comboBoxMaterialType";
            this.comboBoxMaterialType.Size = new System.Drawing.Size(93, 24);
            this.comboBoxMaterialType.TabIndex = 17;
            this.comboBoxMaterialType.SelectedIndexChanged += new System.EventHandler(this.comboBoxMaterialType_SelectedIndexChanged);
            // 
            // comboBoxMaterialReinf
            // 
            this.comboBoxMaterialReinf.FormattingEnabled = true;
            this.comboBoxMaterialReinf.Location = new System.Drawing.Point(190, 46);
            this.comboBoxMaterialReinf.Name = "comboBoxMaterialReinf";
            this.comboBoxMaterialReinf.Size = new System.Drawing.Size(93, 24);
            this.comboBoxMaterialReinf.TabIndex = 16;
            // 
            // comboBoxMaterialGeom
            // 
            this.comboBoxMaterialGeom.FormattingEnabled = true;
            this.comboBoxMaterialGeom.Location = new System.Drawing.Point(287, 10);
            this.comboBoxMaterialGeom.Name = "comboBoxMaterialGeom";
            this.comboBoxMaterialGeom.Size = new System.Drawing.Size(93, 24);
            this.comboBoxMaterialGeom.TabIndex = 15;
            // 
            // buttonCalculate
            // 
            this.buttonCalculate.Location = new System.Drawing.Point(7, 105);
            this.buttonCalculate.Name = "buttonCalculate";
            this.buttonCalculate.Size = new System.Drawing.Size(176, 27);
            this.buttonCalculate.TabIndex = 14;
            this.buttonCalculate.Text = "Calculate";
            this.buttonCalculate.UseVisualStyleBackColor = true;
            this.buttonCalculate.Click += new System.EventHandler(this.buttonCalculate_Click_1);
            // 
            // checkBoxShowDivision
            // 
            this.checkBoxShowDivision.AutoSize = true;
            this.checkBoxShowDivision.Location = new System.Drawing.Point(7, 78);
            this.checkBoxShowDivision.Name = "checkBoxShowDivision";
            this.checkBoxShowDivision.Size = new System.Drawing.Size(117, 21);
            this.checkBoxShowDivision.TabIndex = 13;
            this.checkBoxShowDivision.Text = "Show Division";
            this.checkBoxShowDivision.UseVisualStyleBackColor = true;
            this.checkBoxShowDivision.CheckedChanged += new System.EventHandler(this.checkBoxShowDivision_CheckedChanged_1);
            // 
            // buttonAddReinf
            // 
            this.buttonAddReinf.Location = new System.Drawing.Point(7, 43);
            this.buttonAddReinf.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddReinf.Name = "buttonAddReinf";
            this.buttonAddReinf.Size = new System.Drawing.Size(176, 28);
            this.buttonAddReinf.TabIndex = 10;
            this.buttonAddReinf.Text = "Add Reinforcement";
            this.buttonAddReinf.UseVisualStyleBackColor = true;
            this.buttonAddReinf.Click += new System.EventHandler(this.buttonAddReinf_Click_1);
            // 
            // buttonAddGeometry
            // 
            this.buttonAddGeometry.Location = new System.Drawing.Point(7, 7);
            this.buttonAddGeometry.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddGeometry.Name = "buttonAddGeometry";
            this.buttonAddGeometry.Size = new System.Drawing.Size(176, 28);
            this.buttonAddGeometry.TabIndex = 8;
            this.buttonAddGeometry.Text = "Add Geometry";
            this.buttonAddGeometry.UseVisualStyleBackColor = true;
            this.buttonAddGeometry.Click += new System.EventHandler(this.buttonAddConcreteGeometry_Click);
            // 
            // tabPageResults
            // 
            this.tabPageResults.Controls.Add(this.chartResults);
            this.tabPageResults.Location = new System.Drawing.Point(4, 25);
            this.tabPageResults.Name = "tabPageResults";
            this.tabPageResults.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageResults.Size = new System.Drawing.Size(494, 446);
            this.tabPageResults.TabIndex = 1;
            this.tabPageResults.Text = "Results";
            this.tabPageResults.UseVisualStyleBackColor = true;
            // 
            // chartResults
            // 
            chartArea1.AxisX.ArrowStyle = System.Windows.Forms.DataVisualization.Charting.AxisArrowStyle.Lines;
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX.LabelStyle.Interval = 0D;
            chartArea1.AxisX.MinorGrid.Enabled = true;
            chartArea1.AxisX.MinorGrid.LineColor = System.Drawing.Color.Silver;
            stripLine1.BackColor = System.Drawing.Color.Red;
            stripLine1.BorderColor = System.Drawing.Color.Black;
            stripLine1.BorderWidth = 2;
            chartArea1.AxisX.StripLines.Add(stripLine1);
            chartArea1.AxisX.Title = "[kNm]";
            chartArea1.AxisX2.Crossing = 0D;
            chartArea1.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chartArea1.AxisX2.MinorGrid.Enabled = true;
            chartArea1.AxisX2.MinorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisX2.Title = "[kNm]";
            chartArea1.AxisY.ArrowStyle = System.Windows.Forms.DataVisualization.Charting.AxisArrowStyle.Lines;
            chartArea1.AxisY.Crossing = 0D;
            chartArea1.AxisY.LabelStyle.Interval = 0D;
            chartArea1.AxisY.MinorGrid.Enabled = true;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            stripLine2.BorderColor = System.Drawing.Color.Black;
            stripLine2.BorderWidth = 2;
            chartArea1.AxisY.StripLines.Add(stripLine2);
            chartArea1.AxisY.Title = "[kN]";
            chartArea1.Name = "ChartAreaResults";
            this.chartResults.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.chartResults.Legends.Add(legend1);
            this.chartResults.Location = new System.Drawing.Point(-1, 0);
            this.chartResults.Name = "chartResults";
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartAreaResults";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.Fuchsia;
            series1.Legend = "Legend1";
            series1.Name = "Strength";
            this.chartResults.Series.Add(series1);
            this.chartResults.Size = new System.Drawing.Size(499, 450);
            this.chartResults.TabIndex = 1;
            this.chartResults.Text = "chart1";
            // 
            // textBoxForce
            // 
            this.textBoxForce.Location = new System.Drawing.Point(5, 165);
            this.textBoxForce.Name = "textBoxForce";
            this.textBoxForce.Size = new System.Drawing.Size(100, 22);
            this.textBoxForce.TabIndex = 19;
            // 
            // textBoxMoment
            // 
            this.textBoxMoment.Location = new System.Drawing.Point(135, 165);
            this.textBoxMoment.Name = "textBoxMoment";
            this.textBoxMoment.Size = new System.Drawing.Size(100, 22);
            this.textBoxMoment.TabIndex = 20;
            // 
            // buttonCalc2
            // 
            this.buttonCalc2.Location = new System.Drawing.Point(7, 203);
            this.buttonCalc2.Name = "buttonCalc2";
            this.buttonCalc2.Size = new System.Drawing.Size(75, 23);
            this.buttonCalc2.TabIndex = 21;
            this.buttonCalc2.Text = "button1";
            this.buttonCalc2.UseVisualStyleBackColor = true;
            this.buttonCalc2.Click += new System.EventHandler(this.buttonCalc2_Click);
            // 
            // MainPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlMain);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainPanel";
            this.Size = new System.Drawing.Size(502, 475);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.tabPageMain.PerformLayout();
            this.tabPageResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartResults)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.Button buttonCalculate;
        private System.Windows.Forms.CheckBox checkBoxShowDivision;
        private System.Windows.Forms.Button buttonAddReinf;
        private System.Windows.Forms.Button buttonAddGeometry;
        private System.Windows.Forms.TabPage tabPageResults;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartResults;
        private System.Windows.Forms.ComboBox comboBoxMaterialReinf;
        private System.Windows.Forms.ComboBox comboBoxMaterialGeom;
        private System.Windows.Forms.ComboBox comboBoxMaterialType;
        private System.Windows.Forms.ComboBox comboBoxDiam;
        private System.Windows.Forms.TextBox textBoxMoment;
        private System.Windows.Forms.TextBox textBoxForce;
        private System.Windows.Forms.Button buttonCalc2;
    }
}
