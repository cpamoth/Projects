namespace Mail_Auditor
{
    partial class MainForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblLCMailOutResult = new System.Windows.Forms.Label();
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnFixTargets = new System.Windows.Forms.Button();
            this.lblMailResult = new System.Windows.Forms.Label();
            this.cboTargets = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Port Check Result:";
            // 
            // lblLCMailOutResult
            // 
            this.lblLCMailOutResult.AutoSize = true;
            this.lblLCMailOutResult.Location = new System.Drawing.Point(44, 31);
            this.lblLCMailOutResult.Name = "lblLCMailOutResult";
            this.lblLCMailOutResult.Size = new System.Drawing.Size(35, 13);
            this.lblLCMailOutResult.TabIndex = 1;
            this.lblLCMailOutResult.Text = "label2";
            // 
            // dgvFiles
            // 
            this.dgvFiles.AllowUserToAddRows = false;
            this.dgvFiles.AllowUserToDeleteRows = false;
            this.dgvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFiles.Location = new System.Drawing.Point(12, 87);
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFiles.Size = new System.Drawing.Size(527, 193);
            this.dgvFiles.TabIndex = 2;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.Location = new System.Drawing.Point(12, 286);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnFixTargets
            // 
            this.btnFixTargets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFixTargets.Location = new System.Drawing.Point(464, 286);
            this.btnFixTargets.Name = "btnFixTargets";
            this.btnFixTargets.Size = new System.Drawing.Size(75, 23);
            this.btnFixTargets.TabIndex = 5;
            this.btnFixTargets.Text = "Fix Targets";
            this.btnFixTargets.UseVisualStyleBackColor = true;
            this.btnFixTargets.Click += new System.EventHandler(this.btnFixTargets_Click);
            // 
            // lblMailResult
            // 
            this.lblMailResult.AutoSize = true;
            this.lblMailResult.Location = new System.Drawing.Point(44, 47);
            this.lblMailResult.Name = "lblMailResult";
            this.lblMailResult.Size = new System.Drawing.Size(35, 13);
            this.lblMailResult.TabIndex = 6;
            this.lblMailResult.Text = "label2";
            // 
            // cboTargets
            // 
            this.cboTargets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTargets.FormattingEnabled = true;
            this.cboTargets.Location = new System.Drawing.Point(262, 286);
            this.cboTargets.Name = "cboTargets";
            this.cboTargets.Size = new System.Drawing.Size(196, 21);
            this.cboTargets.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 291);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Target:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 321);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboTargets);
            this.Controls.Add(this.lblMailResult);
            this.Controls.Add(this.btnFixTargets);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.dgvFiles);
            this.Controls.Add(this.lblLCMailOutResult);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "Mail Auditor";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblLCMailOutResult;
        private System.Windows.Forms.DataGridView dgvFiles;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnFixTargets;
        private System.Windows.Forms.Label lblMailResult;
        private System.Windows.Forms.ComboBox cboTargets;
        private System.Windows.Forms.Label label2;
    }
}

