namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    partial class WixBuildEventsPropertyPagePanel
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
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox preBuildGroupBox;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox postBuildGroupBox;
            System.Windows.Forms.Label runLabel;
            this.preBuildEditor = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBuildEventEditor();
            this.postBuildEditor = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBuildEventEditor();
            this.runPostBuildComboBox = new System.Windows.Forms.ComboBox();
            preBuildGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            postBuildGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            runLabel = new System.Windows.Forms.Label();
            preBuildGroupBox.SuspendLayout();
            postBuildGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // banner
            // 
            this.banner.Size = new System.Drawing.Size(528, 90);
            this.banner.Text = "Build Events";
            // 
            // preBuildGroupBox
            // 
            preBuildGroupBox.Controls.Add(this.preBuildEditor);
            preBuildGroupBox.Location = new System.Drawing.Point(0, 58);
            preBuildGroupBox.Name = "preBuildGroupBox";
            preBuildGroupBox.Size = new System.Drawing.Size(519, 152);
            preBuildGroupBox.TabIndex = 0;
            preBuildGroupBox.Text = "P&re-build Event Command Line";
            // 
            // preBuildEditor
            // 
            this.preBuildEditor.ButtonText = "Ed&it Pre-build...";
            this.preBuildEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.preBuildEditor.EditorFormText = "Edit Pre-build Event Command Line";
            this.preBuildEditor.Location = new System.Drawing.Point(24, 24);
            this.preBuildEditor.Name = "preBuildEditor";
            this.preBuildEditor.Size = new System.Drawing.Size(495, 128);
            this.preBuildEditor.TabIndex = 0;
            // 
            // postBuildGroupBox
            // 
            postBuildGroupBox.Controls.Add(this.postBuildEditor);
            postBuildGroupBox.Controls.Add(this.runPostBuildComboBox);
            postBuildGroupBox.Controls.Add(runLabel);
            postBuildGroupBox.Location = new System.Drawing.Point(0, 225);
            postBuildGroupBox.Name = "postBuildGroupBox";
            postBuildGroupBox.Size = new System.Drawing.Size(519, 197);
            postBuildGroupBox.TabIndex = 1;
            postBuildGroupBox.Text = "P&ost-build Event Command Line";
            // 
            // postBuildEditor
            // 
            this.postBuildEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.postBuildEditor.ButtonText = "Edit Post-b&uild...";
            this.postBuildEditor.EditorFormText = "Edit Post-build Event Command Line";
            this.postBuildEditor.Location = new System.Drawing.Point(24, 24);
            this.postBuildEditor.Name = "postBuildEditor";
            this.postBuildEditor.Size = new System.Drawing.Size(495, 128);
            this.postBuildEditor.TabIndex = 1;
            // 
            // runLabel
            // 
            runLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            runLabel.AutoSize = true;
            runLabel.Location = new System.Drawing.Point(21, 155);
            runLabel.Name = "runLabel";
            runLabel.Size = new System.Drawing.Size(130, 13);
            runLabel.TabIndex = 2;
            runLabel.Text = "Ru&n the post-build event:";
            // 
            // runPostBuildComboBox
            // 
            this.runPostBuildComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.runPostBuildComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.runPostBuildComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.runPostBuildComboBox.FormattingEnabled = true;
            this.runPostBuildComboBox.Items.AddRange(new object[] {
            "Always",
            "On successful build",
            "When the build updates the project output"});
            this.runPostBuildComboBox.Location = new System.Drawing.Point(24, 171);
            this.runPostBuildComboBox.Name = "runPostBuildComboBox";
            this.runPostBuildComboBox.Size = new System.Drawing.Size(495, 21);
            this.runPostBuildComboBox.TabIndex = 3;
            // 
            // WixBuildEventsPropertyPagePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(postBuildGroupBox);
            this.Controls.Add(preBuildGroupBox);
            this.Name = "WixBuildEventsPropertyPagePanel";
            this.Size = new System.Drawing.Size(528, 435);
            this.Controls.SetChildIndex(this.banner, 0);
            this.Controls.SetChildIndex(preBuildGroupBox, 0);
            this.Controls.SetChildIndex(postBuildGroupBox, 0);
            preBuildGroupBox.ResumeLayout(false);
            postBuildGroupBox.ResumeLayout(false);
            postBuildGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBuildEventEditor preBuildEditor;
        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBuildEventEditor postBuildEditor;
        private System.Windows.Forms.ComboBox runPostBuildComboBox;


    }
}
