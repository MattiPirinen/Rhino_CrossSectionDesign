namespace CrossSectionDesign
{
    partial class HeatFlowForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.StripLine stripLine1 = new System.Windows.Forms.DataVisualization.Charting.StripLine();
            System.Windows.Forms.DataVisualization.Charting.StripLine stripLine2 = new System.Windows.Forms.DataVisualization.Charting.StripLine();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.chartTemp = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.buttonTestCond = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.button_AddConstraint = new System.Windows.Forms.Button();
            this.buttonAddInspectionPoint = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBarSimulation = new System.Windows.Forms.ProgressBar();
            this.textBoxStepSize = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxEndTime = new System.Windows.Forms.TextBox();
            this.checkBoxShowIpNumbers = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.chartTemp)).BeginInit();
            this.SuspendLayout();
            // 
            // chartTemp
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
            chartArea1.AxisX.Title = "[s]";
            chartArea1.AxisX2.Crossing = 0D;
            chartArea1.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chartArea1.AxisX2.MinorGrid.Enabled = true;
            chartArea1.AxisX2.MinorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisX2.Title = "[s]";
            chartArea1.AxisY.ArrowStyle = System.Windows.Forms.DataVisualization.Charting.AxisArrowStyle.Lines;
            chartArea1.AxisY.Crossing = 0D;
            chartArea1.AxisY.LabelStyle.Interval = 0D;
            chartArea1.AxisY.MinorGrid.Enabled = true;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            stripLine2.BorderColor = System.Drawing.Color.Black;
            stripLine2.BorderWidth = 2;
            chartArea1.AxisY.StripLines.Add(stripLine2);
            chartArea1.AxisY.Title = "[C]";
            chartArea1.Name = "ChartAreaResults";
            this.chartTemp.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.chartTemp.Legends.Add(legend1);
            this.chartTemp.Location = new System.Drawing.Point(8, -2);
            this.chartTemp.Name = "chartTemp";
            this.chartTemp.Size = new System.Drawing.Size(499, 450);
            this.chartTemp.TabIndex = 25;
            this.chartTemp.Text = "chartTemperature";
            // 
            // buttonTestCond
            // 
            this.buttonTestCond.Location = new System.Drawing.Point(199, 455);
            this.buttonTestCond.Name = "buttonTestCond";
            this.buttonTestCond.Size = new System.Drawing.Size(90, 67);
            this.buttonTestCond.TabIndex = 49;
            this.buttonTestCond.Text = "Test Cond";
            this.buttonTestCond.UseVisualStyleBackColor = true;
            this.buttonTestCond.Click += new System.EventHandler(this.buttonTestCond_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(8, 528);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(88, 53);
            this.buttonStart.TabIndex = 48;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // button_AddConstraint
            // 
            this.button_AddConstraint.Location = new System.Drawing.Point(8, 454);
            this.button_AddConstraint.Name = "button_AddConstraint";
            this.button_AddConstraint.Size = new System.Drawing.Size(88, 68);
            this.button_AddConstraint.TabIndex = 47;
            this.button_AddConstraint.Text = "Add Constraint";
            this.button_AddConstraint.UseVisualStyleBackColor = true;
            this.button_AddConstraint.Click += new System.EventHandler(this.buttonAddConstraint_Click);
            // 
            // buttonAddInspectionPoint
            // 
            this.buttonAddInspectionPoint.Location = new System.Drawing.Point(103, 455);
            this.buttonAddInspectionPoint.Name = "buttonAddInspectionPoint";
            this.buttonAddInspectionPoint.Size = new System.Drawing.Size(90, 67);
            this.buttonAddInspectionPoint.TabIndex = 50;
            this.buttonAddInspectionPoint.Text = "Add Inspection Point";
            this.buttonAddInspectionPoint.UseVisualStyleBackColor = true;
            this.buttonAddInspectionPoint.Click += new System.EventHandler(this.buttonAddInspectionPoint_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(102, 528);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 53);
            this.buttonCancel.TabIndex = 51;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // progressBarSimulation
            // 
            this.progressBarSimulation.Location = new System.Drawing.Point(199, 528);
            this.progressBarSimulation.Name = "progressBarSimulation";
            this.progressBarSimulation.Size = new System.Drawing.Size(305, 53);
            this.progressBarSimulation.TabIndex = 52;
            // 
            // textBoxStepSize
            // 
            this.textBoxStepSize.Location = new System.Drawing.Point(368, 454);
            this.textBoxStepSize.Name = "textBoxStepSize";
            this.textBoxStepSize.Size = new System.Drawing.Size(100, 22);
            this.textBoxStepSize.TabIndex = 53;
            this.textBoxStepSize.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBoxStepSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.int_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(296, 455);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 17);
            this.label1.TabIndex = 54;
            this.label1.Text = "Step size";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(474, 456);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 17);
            this.label2.TabIndex = 55;
            this.label2.Text = "s";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(474, 484);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 17);
            this.label3.TabIndex = 58;
            this.label3.Text = "s";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(296, 483);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 17);
            this.label4.TabIndex = 57;
            this.label4.Text = "End time";
            // 
            // textBoxEndTime
            // 
            this.textBoxEndTime.Location = new System.Drawing.Point(368, 482);
            this.textBoxEndTime.Name = "textBoxEndTime";
            this.textBoxEndTime.Size = new System.Drawing.Size(100, 22);
            this.textBoxEndTime.TabIndex = 56;
            this.textBoxEndTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.int_KeyPress);
            // 
            // checkBoxShowIpNumbers
            // 
            this.checkBoxShowIpNumbers.AutoSize = true;
            this.checkBoxShowIpNumbers.Location = new System.Drawing.Point(505, 456);
            this.checkBoxShowIpNumbers.Name = "checkBoxShowIpNumbers";
            this.checkBoxShowIpNumbers.Size = new System.Drawing.Size(162, 21);
            this.checkBoxShowIpNumbers.TabIndex = 59;
            this.checkBoxShowIpNumbers.Text = "Show IPoint numbers";
            this.checkBoxShowIpNumbers.UseVisualStyleBackColor = true;
            this.checkBoxShowIpNumbers.CheckedChanged += new System.EventHandler(this.checkBoxShowIpNumbers_CheckedChanged);
            // 
            // HeatFlowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 598);
            this.Controls.Add(this.checkBoxShowIpNumbers);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxEndTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxStepSize);
            this.Controls.Add(this.progressBarSimulation);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAddInspectionPoint);
            this.Controls.Add(this.buttonTestCond);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.button_AddConstraint);
            this.Controls.Add(this.chartTemp);
            this.Name = "HeatFlowForm";
            this.Text = "HeatFlowForm";
            ((System.ComponentModel.ISupportInitialize)(this.chartTemp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartTemp;
        private System.Windows.Forms.Button buttonTestCond;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button button_AddConstraint;
        private System.Windows.Forms.Button buttonAddInspectionPoint;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ProgressBar progressBarSimulation;
        private System.Windows.Forms.TextBox textBoxStepSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxEndTime;
        private System.Windows.Forms.CheckBox checkBoxShowIpNumbers;
    }
}