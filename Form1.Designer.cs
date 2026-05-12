namespace CurveAnalysis
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.formsPlot1 = new ScottPlot.FormsPlot();
            this.panelTop = new System.Windows.Forms.Panel();
            this.labelFileName = new System.Windows.Forms.Label();
            this.btnLoadCsv = new System.Windows.Forms.Button();
            this.numericK = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericK)).BeginInit();
            this.SuspendLayout();
            // 
            // formsPlot1
            // 
            this.formsPlot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot1.Location = new System.Drawing.Point(0, 40);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(1049, 550);
            this.formsPlot1.TabIndex = 0;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.LightGray;
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.labelFileName);
            this.panelTop.Controls.Add(this.btnLoadCsv);
            this.panelTop.Controls.Add(this.numericK);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1049, 40);
            this.panelTop.TabIndex = 1;
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(480, 14);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(65, 12);
            this.labelFileName.TabIndex = 0;
            this.labelFileName.Text = "未加载文件";
            // 
            // btnLoadCsv
            // 
            this.btnLoadCsv.Location = new System.Drawing.Point(901, 8);
            this.btnLoadCsv.Name = "btnLoadCsv";
            this.btnLoadCsv.Size = new System.Drawing.Size(100, 24);
            this.btnLoadCsv.TabIndex = 0;
            this.btnLoadCsv.Text = "加载 CSV";
            this.btnLoadCsv.Click += new System.EventHandler(this.btnLoadCsv_Click);
            // 
            // numericK
            // 
            this.numericK.Location = new System.Drawing.Point(74, 10);
            this.numericK.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.numericK.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericK.Name = "numericK";
            this.numericK.Size = new System.Drawing.Size(60, 21);
            this.numericK.TabIndex = 1;
            this.numericK.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericK.ValueChanged += new System.EventHandler(this.numericK_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "斜率参数K:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1049, 590);
            this.Controls.Add(this.formsPlot1);
            this.Controls.Add(this.panelTop);
            this.Name = "Form1";
            this.Text = "曲线分析";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericK)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ScottPlot.FormsPlot formsPlot1;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnLoadCsv;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.NumericUpDown numericK;
        private System.Windows.Forms.Label label1;
    }
}

