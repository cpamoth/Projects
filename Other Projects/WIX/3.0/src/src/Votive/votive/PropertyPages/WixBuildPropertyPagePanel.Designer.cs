namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    partial class WixBuildPropertyPagePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WixBuildPropertyPagePanel));
            System.Windows.Forms.Label outputNameLabel;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupLabel outputTypeGroupLabel;
            this.outputNameTextBox = new System.Windows.Forms.TextBox();
            this.outputTypeLibraryRadioButton = new System.Windows.Forms.RadioButton();
            this.outputTypeLibraryPictureBox = new System.Windows.Forms.PictureBox();
            this.outputTypeModuleRadioButton = new System.Windows.Forms.RadioButton();
            this.outputTypeModulePictureBox = new System.Windows.Forms.PictureBox();
            this.outputTypePackageRadioButton = new System.Windows.Forms.RadioButton();
            this.outputTypePackagePictureBox = new System.Windows.Forms.PictureBox();
            this.outputTypeDescriptionLabel = new System.Windows.Forms.Label();
            outputNameLabel = new System.Windows.Forms.Label();
            outputTypeGroupLabel = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupLabel();
            ((System.ComponentModel.ISupportInitialize)(this.outputTypeLibraryPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputTypeModulePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputTypePackagePictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // banner
            // 
            resources.ApplyResources(this.banner, "banner");
            // 
            // outputNameLabel
            // 
            resources.ApplyResources(outputNameLabel, "outputNameLabel");
            outputNameLabel.Name = "outputNameLabel";
            // 
            // outputTypeGroupLabel
            // 
            resources.ApplyResources(outputTypeGroupLabel, "outputTypeGroupLabel");
            outputTypeGroupLabel.Name = "outputTypeGroupLabel";
            // 
            // outputNameTextBox
            // 
            resources.ApplyResources(this.outputNameTextBox, "outputNameTextBox");
            this.outputNameTextBox.Name = "outputNameTextBox";
            this.outputNameTextBox.TextChanged += new System.EventHandler(this.outputNameTextBox_TextChanged);
            // 
            // outputTypeLibraryRadioButton
            // 
            resources.ApplyResources(this.outputTypeLibraryRadioButton, "outputTypeLibraryRadioButton");
            this.outputTypeLibraryRadioButton.Name = "outputTypeLibraryRadioButton";
            this.outputTypeLibraryRadioButton.UseVisualStyleBackColor = true;
            // 
            // outputTypeLibraryPictureBox
            // 
            resources.ApplyResources(this.outputTypeLibraryPictureBox, "outputTypeLibraryPictureBox");
            this.outputTypeLibraryPictureBox.Name = "outputTypeLibraryPictureBox";
            this.outputTypeLibraryPictureBox.TabStop = false;
            // 
            // outputTypeModuleRadioButton
            // 
            resources.ApplyResources(this.outputTypeModuleRadioButton, "outputTypeModuleRadioButton");
            this.outputTypeModuleRadioButton.Name = "outputTypeModuleRadioButton";
            this.outputTypeModuleRadioButton.UseVisualStyleBackColor = true;
            // 
            // outputTypeModulePictureBox
            // 
            resources.ApplyResources(this.outputTypeModulePictureBox, "outputTypeModulePictureBox");
            this.outputTypeModulePictureBox.Name = "outputTypeModulePictureBox";
            this.outputTypeModulePictureBox.TabStop = false;
            // 
            // outputTypePackageRadioButton
            // 
            resources.ApplyResources(this.outputTypePackageRadioButton, "outputTypePackageRadioButton");
            this.outputTypePackageRadioButton.Name = "outputTypePackageRadioButton";
            this.outputTypePackageRadioButton.UseVisualStyleBackColor = true;
            // 
            // outputTypePackagePictureBox
            // 
            resources.ApplyResources(this.outputTypePackagePictureBox, "outputTypePackagePictureBox");
            this.outputTypePackagePictureBox.Name = "outputTypePackagePictureBox";
            this.outputTypePackagePictureBox.TabStop = false;
            // 
            // outputTypeDescriptionLabel
            // 
            resources.ApplyResources(this.outputTypeDescriptionLabel, "outputTypeDescriptionLabel");
            this.outputTypeDescriptionLabel.Name = "outputTypeDescriptionLabel";
            // 
            // WixBuildPropertyPagePanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.outputTypeLibraryPictureBox);
            this.Controls.Add(this.outputTypeModulePictureBox);
            this.Controls.Add(this.outputTypeLibraryRadioButton);
            this.Controls.Add(this.outputTypeModuleRadioButton);
            this.Controls.Add(this.outputTypePackagePictureBox);
            this.Controls.Add(outputTypeGroupLabel);
            this.Controls.Add(this.outputTypeDescriptionLabel);
            this.Controls.Add(this.outputTypePackageRadioButton);
            this.Controls.Add(outputNameLabel);
            this.Controls.Add(this.outputNameTextBox);
            this.Name = "WixBuildPropertyPagePanel";
            this.Controls.SetChildIndex(this.outputNameTextBox, 0);
            this.Controls.SetChildIndex(outputNameLabel, 0);
            this.Controls.SetChildIndex(this.outputTypePackageRadioButton, 0);
            this.Controls.SetChildIndex(this.outputTypeDescriptionLabel, 0);
            this.Controls.SetChildIndex(outputTypeGroupLabel, 0);
            this.Controls.SetChildIndex(this.outputTypePackagePictureBox, 0);
            this.Controls.SetChildIndex(this.outputTypeModuleRadioButton, 0);
            this.Controls.SetChildIndex(this.outputTypeLibraryRadioButton, 0);
            this.Controls.SetChildIndex(this.outputTypeModulePictureBox, 0);
            this.Controls.SetChildIndex(this.outputTypeLibraryPictureBox, 0);
            this.Controls.SetChildIndex(this.banner, 0);
            ((System.ComponentModel.ISupportInitialize)(this.outputTypeLibraryPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputTypeModulePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputTypePackagePictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox outputNameTextBox;
        private System.Windows.Forms.RadioButton outputTypeLibraryRadioButton;
        private System.Windows.Forms.PictureBox outputTypeLibraryPictureBox;
        private System.Windows.Forms.RadioButton outputTypeModuleRadioButton;
        private System.Windows.Forms.PictureBox outputTypeModulePictureBox;
        private System.Windows.Forms.RadioButton outputTypePackageRadioButton;
        private System.Windows.Forms.PictureBox outputTypePackagePictureBox;
        private System.Windows.Forms.Label outputTypeDescriptionLabel;
    }
}
