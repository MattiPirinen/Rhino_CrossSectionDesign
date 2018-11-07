namespace CrossSectionDesign
{
    partial class RFEMAnalysisForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewValues = new System.Windows.Forms.DataGridView();
            this.No = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Length = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Kz = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Ky = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Circle_Diam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Circle_Steel_Amount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rect_Height = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rect_Width = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Amount_Heigth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Amount_Width = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Utilization = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button_Import = new System.Windows.Forms.Button();
            this.textBox_Comment = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_ClimateCond = new System.Windows.Forms.Button();
            this.buttonChooseColumns = new System.Windows.Forms.Button();
            this.buttonCalculate = new System.Windows.Forms.Button();
            this.groupBox_Display = new System.Windows.Forms.GroupBox();
            this.radioButton_Results = new System.Windows.Forms.RadioButton();
            this.radioButton_geometry = new System.Windows.Forms.RadioButton();
            this.groupBox_ResultScale = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_Min = new System.Windows.Forms.TextBox();
            this.textBox_Max = new System.Windows.Forms.TextBox();
            this.groupBox_CalcType = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton_CrossSectionDesign = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewValues)).BeginInit();
            this.groupBox_Display.SuspendLayout();
            this.groupBox_ResultScale.SuspendLayout();
            this.groupBox_CalcType.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewValues
            // 
            this.dataGridViewValues.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewValues.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.No,
            this.Length,
            this.Kz,
            this.Ky,
            this.Type,
            this.Circle_Diam,
            this.Circle_Steel_Amount,
            this.Rect_Height,
            this.Rect_Width,
            this.Amount_Heigth,
            this.Amount_Width,
            this.Utilization});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewValues.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewValues.Location = new System.Drawing.Point(12, 106);
            this.dataGridViewValues.Name = "dataGridViewValues";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewValues.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewValues.RowTemplate.Height = 24;
            this.dataGridViewValues.Size = new System.Drawing.Size(864, 442);
            this.dataGridViewValues.TabIndex = 0;
            // 
            // No
            // 
            this.No.HeaderText = "No";
            this.No.Name = "No";
            this.No.Width = 50;
            // 
            // Length
            // 
            this.Length.HeaderText = "Length";
            this.Length.Name = "Length";
            this.Length.Width = 70;
            // 
            // Kz
            // 
            this.Kz.HeaderText = "Kz";
            this.Kz.Name = "Kz";
            this.Kz.Width = 70;
            // 
            // Ky
            // 
            this.Ky.HeaderText = "Ky";
            this.Ky.Name = "Ky";
            this.Ky.Width = 70;
            // 
            // Type
            // 
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            this.Type.Width = 70;
            // 
            // Circle_Diam
            // 
            this.Circle_Diam.HeaderText = "Circle Diameter";
            this.Circle_Diam.Name = "Circle_Diam";
            this.Circle_Diam.Width = 70;
            // 
            // Circle_Steel_Amount
            // 
            this.Circle_Steel_Amount.HeaderText = "Steel Amount";
            this.Circle_Steel_Amount.Name = "Circle_Steel_Amount";
            this.Circle_Steel_Amount.Width = 70;
            // 
            // Rect_Height
            // 
            this.Rect_Height.HeaderText = "Rect Height";
            this.Rect_Height.Name = "Rect_Height";
            this.Rect_Height.Width = 70;
            // 
            // Rect_Width
            // 
            this.Rect_Width.HeaderText = "Rect Width";
            this.Rect_Width.Name = "Rect_Width";
            this.Rect_Width.Width = 70;
            // 
            // Amount_Heigth
            // 
            this.Amount_Heigth.HeaderText = "Amount Heigth";
            this.Amount_Heigth.Name = "Amount_Heigth";
            this.Amount_Heigth.Width = 70;
            // 
            // Amount_Width
            // 
            this.Amount_Width.HeaderText = "Amount Width";
            this.Amount_Width.Name = "Amount_Width";
            this.Amount_Width.Width = 70;
            // 
            // Utilization
            // 
            this.Utilization.HeaderText = "Utilization";
            this.Utilization.Name = "Utilization";
            this.Utilization.Width = 70;
            // 
            // button_Import
            // 
            this.button_Import.Location = new System.Drawing.Point(882, 160);
            this.button_Import.Name = "button_Import";
            this.button_Import.Size = new System.Drawing.Size(93, 57);
            this.button_Import.TabIndex = 1;
            this.button_Import.Text = "Import from RFEM";
            this.button_Import.UseVisualStyleBackColor = true;
            this.button_Import.Click += new System.EventHandler(this.button_Import_Click);
            // 
            // textBox_Comment
            // 
            this.textBox_Comment.Location = new System.Drawing.Point(882, 132);
            this.textBox_Comment.Name = "textBox_Comment";
            this.textBox_Comment.Size = new System.Drawing.Size(100, 22);
            this.textBox_Comment.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(882, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "RFEM comment";
            // 
            // button_ClimateCond
            // 
            this.button_ClimateCond.Location = new System.Drawing.Point(882, 224);
            this.button_ClimateCond.Name = "button_ClimateCond";
            this.button_ClimateCond.Size = new System.Drawing.Size(93, 48);
            this.button_ClimateCond.TabIndex = 4;
            this.button_ClimateCond.Text = "Climate Conditions";
            this.button_ClimateCond.UseVisualStyleBackColor = true;
            this.button_ClimateCond.Click += new System.EventHandler(this.button_ClimateCond_Click);
            // 
            // buttonChooseColumns
            // 
            this.buttonChooseColumns.Location = new System.Drawing.Point(882, 278);
            this.buttonChooseColumns.Name = "buttonChooseColumns";
            this.buttonChooseColumns.Size = new System.Drawing.Size(93, 48);
            this.buttonChooseColumns.TabIndex = 5;
            this.buttonChooseColumns.Text = "Choose Columns";
            this.buttonChooseColumns.UseVisualStyleBackColor = true;
            this.buttonChooseColumns.Click += new System.EventHandler(this.buttonChooseColumns_Click);
            // 
            // buttonCalculate
            // 
            this.buttonCalculate.Location = new System.Drawing.Point(885, 467);
            this.buttonCalculate.Name = "buttonCalculate";
            this.buttonCalculate.Size = new System.Drawing.Size(93, 48);
            this.buttonCalculate.TabIndex = 6;
            this.buttonCalculate.Text = "Calculate";
            this.buttonCalculate.UseVisualStyleBackColor = true;
            this.buttonCalculate.Click += new System.EventHandler(this.buttonCalculate_Click);
            // 
            // groupBox_Display
            // 
            this.groupBox_Display.Controls.Add(this.radioButton_Results);
            this.groupBox_Display.Controls.Add(this.radioButton_geometry);
            this.groupBox_Display.Location = new System.Drawing.Point(882, 332);
            this.groupBox_Display.Name = "groupBox_Display";
            this.groupBox_Display.Size = new System.Drawing.Size(104, 76);
            this.groupBox_Display.TabIndex = 8;
            this.groupBox_Display.TabStop = false;
            this.groupBox_Display.Text = "Display";
            // 
            // radioButton_Results
            // 
            this.radioButton_Results.AutoSize = true;
            this.radioButton_Results.Location = new System.Drawing.Point(6, 48);
            this.radioButton_Results.Name = "radioButton_Results";
            this.radioButton_Results.Size = new System.Drawing.Size(76, 21);
            this.radioButton_Results.TabIndex = 9;
            this.radioButton_Results.TabStop = true;
            this.radioButton_Results.Text = "Results";
            this.radioButton_Results.UseVisualStyleBackColor = true;
            // 
            // radioButton_geometry
            // 
            this.radioButton_geometry.AutoSize = true;
            this.radioButton_geometry.Location = new System.Drawing.Point(6, 21);
            this.radioButton_geometry.Name = "radioButton_geometry";
            this.radioButton_geometry.Size = new System.Drawing.Size(91, 21);
            this.radioButton_geometry.TabIndex = 8;
            this.radioButton_geometry.TabStop = true;
            this.radioButton_geometry.Text = "Geometry";
            this.radioButton_geometry.UseVisualStyleBackColor = true;
            this.radioButton_geometry.CheckedChanged += new System.EventHandler(this.radioButton_geometry_CheckedChanged);
            // 
            // groupBox_ResultScale
            // 
            this.groupBox_ResultScale.Controls.Add(this.label3);
            this.groupBox_ResultScale.Controls.Add(this.label2);
            this.groupBox_ResultScale.Controls.Add(this.textBox_Min);
            this.groupBox_ResultScale.Controls.Add(this.textBox_Max);
            this.groupBox_ResultScale.Enabled = false;
            this.groupBox_ResultScale.Location = new System.Drawing.Point(12, 562);
            this.groupBox_ResultScale.Name = "groupBox_ResultScale";
            this.groupBox_ResultScale.Size = new System.Drawing.Size(154, 88);
            this.groupBox_ResultScale.TabIndex = 14;
            this.groupBox_ResultScale.TabStop = false;
            this.groupBox_ResultScale.Text = "Result Scale";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 17);
            this.label3.TabIndex = 16;
            this.label3.Text = "Min";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 17);
            this.label2.TabIndex = 15;
            this.label2.Text = "Max";
            // 
            // textBox_Min
            // 
            this.textBox_Min.Location = new System.Drawing.Point(44, 21);
            this.textBox_Min.Name = "textBox_Min";
            this.textBox_Min.Size = new System.Drawing.Size(100, 22);
            this.textBox_Min.TabIndex = 14;
            // 
            // textBox_Max
            // 
            this.textBox_Max.Location = new System.Drawing.Point(44, 49);
            this.textBox_Max.Name = "textBox_Max";
            this.textBox_Max.Size = new System.Drawing.Size(100, 22);
            this.textBox_Max.TabIndex = 13;
            // 
            // groupBox_CalcType
            // 
            this.groupBox_CalcType.Controls.Add(this.radioButton2);
            this.groupBox_CalcType.Controls.Add(this.radioButton_CrossSectionDesign);
            this.groupBox_CalcType.Location = new System.Drawing.Point(12, 12);
            this.groupBox_CalcType.Name = "groupBox_CalcType";
            this.groupBox_CalcType.Size = new System.Drawing.Size(200, 88);
            this.groupBox_CalcType.TabIndex = 15;
            this.groupBox_CalcType.TabStop = false;
            this.groupBox_CalcType.Text = "Calculation Type";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Enabled = false;
            this.radioButton2.Location = new System.Drawing.Point(6, 52);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(124, 21);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Column Design";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton_CrossSectionDesign
            // 
            this.radioButton_CrossSectionDesign.AutoSize = true;
            this.radioButton_CrossSectionDesign.Location = new System.Drawing.Point(6, 25);
            this.radioButton_CrossSectionDesign.Name = "radioButton_CrossSectionDesign";
            this.radioButton_CrossSectionDesign.Size = new System.Drawing.Size(160, 21);
            this.radioButton_CrossSectionDesign.TabIndex = 0;
            this.radioButton_CrossSectionDesign.TabStop = true;
            this.radioButton_CrossSectionDesign.Text = "Cross section design";
            this.radioButton_CrossSectionDesign.UseVisualStyleBackColor = true;
            this.radioButton_CrossSectionDesign.CheckedChanged += new System.EventHandler(this.radioButton_CrossSectionDesign_CheckedChanged);
            // 
            // RFEMAnalysisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1077, 676);
            this.Controls.Add(this.groupBox_CalcType);
            this.Controls.Add(this.groupBox_ResultScale);
            this.Controls.Add(this.groupBox_Display);
            this.Controls.Add(this.buttonCalculate);
            this.Controls.Add(this.buttonChooseColumns);
            this.Controls.Add(this.button_ClimateCond);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_Comment);
            this.Controls.Add(this.button_Import);
            this.Controls.Add(this.dataGridViewValues);
            this.Name = "RFEMAnalysisForm";
            this.Text = "RFEMAnalysisForm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewValues)).EndInit();
            this.groupBox_Display.ResumeLayout(false);
            this.groupBox_Display.PerformLayout();
            this.groupBox_ResultScale.ResumeLayout(false);
            this.groupBox_ResultScale.PerformLayout();
            this.groupBox_CalcType.ResumeLayout(false);
            this.groupBox_CalcType.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewValues;
        private System.Windows.Forms.Button button_Import;
        private System.Windows.Forms.TextBox textBox_Comment;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_ClimateCond;
        private System.Windows.Forms.DataGridViewTextBoxColumn No;
        private System.Windows.Forms.DataGridViewTextBoxColumn Length;
        private System.Windows.Forms.DataGridViewTextBoxColumn Kz;
        private System.Windows.Forms.DataGridViewTextBoxColumn Ky;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Circle_Diam;
        private System.Windows.Forms.DataGridViewTextBoxColumn Circle_Steel_Amount;
        private System.Windows.Forms.DataGridViewTextBoxColumn Rect_Height;
        private System.Windows.Forms.DataGridViewTextBoxColumn Rect_Width;
        private System.Windows.Forms.DataGridViewTextBoxColumn Amount_Heigth;
        private System.Windows.Forms.DataGridViewTextBoxColumn Amount_Width;
        private System.Windows.Forms.DataGridViewTextBoxColumn Utilization;
        private System.Windows.Forms.Button buttonChooseColumns;
        private System.Windows.Forms.Button buttonCalculate;
        private System.Windows.Forms.GroupBox groupBox_Display;
        private System.Windows.Forms.RadioButton radioButton_Results;
        private System.Windows.Forms.RadioButton radioButton_geometry;
        private System.Windows.Forms.GroupBox groupBox_ResultScale;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_Min;
        private System.Windows.Forms.TextBox textBox_Max;
        private System.Windows.Forms.GroupBox groupBox_CalcType;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton_CrossSectionDesign;
    }
}