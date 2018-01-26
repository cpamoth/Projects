namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    partial class WixCompilerPropertyPagePanel
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
            System.Windows.Forms.Label defineConstantsExampleLabel;
            System.Windows.Forms.Label defineConstantsLabel;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox generalGroupBox;
            System.Windows.Forms.Label outputPathLabel;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox suppressWarningsGroupBox;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox errorsAndWarningsGroupBox;
            this.outputPathTextBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox();
            this.defineDebugCheckBox = new System.Windows.Forms.CheckBox();
            this.defineConstantsTextBox = new System.Windows.Forms.TextBox();
            this.suppressWarningsSpecificRadioButton = new System.Windows.Forms.RadioButton();
            this.suppressWarningsNoneRadioButton = new System.Windows.Forms.RadioButton();
            this.specificWarningsTextBox = new System.Windows.Forms.TextBox();
            this.pedanticCheckBox = new System.Windows.Forms.CheckBox();
            this.warningsAsErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.advancedButton = new System.Windows.Forms.Button();
            defineConstantsExampleLabel = new System.Windows.Forms.Label();
            defineConstantsLabel = new System.Windows.Forms.Label();
            generalGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            outputPathLabel = new System.Windows.Forms.Label();
            suppressWarningsGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            errorsAndWarningsGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            generalGroupBox.SuspendLayout();
            suppressWarningsGroupBox.SuspendLayout();
            errorsAndWarningsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // banner
            // 
            this.banner.Size = new System.Drawing.Size(528, 90);
            this.banner.Text = "Candle Settings";
            // 
            // defineConstantsExampleLabel
            // 
            defineConstantsExampleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            defineConstantsExampleLabel.AutoSize = true;
            defineConstantsExampleLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            defineConstantsExampleLabel.Location = new System.Drawing.Point(166, 77);
            defineConstantsExampleLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            defineConstantsExampleLabel.Name = "defineConstantsExampleLabel";
            defineConstantsExampleLabel.Size = new System.Drawing.Size(241, 13);
            defineConstantsExampleLabel.TabIndex = 4;
            defineConstantsExampleLabel.Text = "Example: Name1=Value1;Name2;Name3=Value3";
            // 
            // defineConstantsLabel
            // 
            defineConstantsLabel.AutoSize = true;
            defineConstantsLabel.Location = new System.Drawing.Point(24, 56);
            defineConstantsLabel.Name = "defineConstantsLabel";
            defineConstantsLabel.Size = new System.Drawing.Size(92, 13);
            defineConstantsLabel.TabIndex = 2;
            defineConstantsLabel.Text = "Define &constants:";
            // 
            // generalGroupBox
            // 
            generalGroupBox.Controls.Add(outputPathLabel);
            generalGroupBox.Controls.Add(this.outputPathTextBox);
            generalGroupBox.Controls.Add(this.defineDebugCheckBox);
            generalGroupBox.Controls.Add(defineConstantsExampleLabel);
            generalGroupBox.Controls.Add(defineConstantsLabel);
            generalGroupBox.Controls.Add(this.defineConstantsTextBox);
            generalGroupBox.Location = new System.Drawing.Point(0, 58);
            generalGroupBox.Name = "generalGroupBox";
            generalGroupBox.Size = new System.Drawing.Size(519, 137);
            generalGroupBox.TabIndex = 0;
            generalGroupBox.Text = "General";
            // 
            // outputPathLabel
            // 
            outputPathLabel.AutoSize = true;
            outputPathLabel.Location = new System.Drawing.Point(24, 102);
            outputPathLabel.Name = "outputPathLabel";
            outputPathLabel.Size = new System.Drawing.Size(70, 13);
            outputPathLabel.TabIndex = 5;
            outputPathLabel.Text = "&Output path:";
            // 
            // outputPathTextBox
            // 
            this.outputPathTextBox.DialogDescription = "Compiler output path:";
            this.outputPathTextBox.Location = new System.Drawing.Point(169, 99);
            this.outputPathTextBox.Name = "outputPathTextBox";
            this.outputPathTextBox.Size = new System.Drawing.Size(350, 22);
            this.outputPathTextBox.TabIndex = 6;
            // 
            // defineDebugCheckBox
            // 
            this.defineDebugCheckBox.AutoSize = true;
            this.defineDebugCheckBox.Location = new System.Drawing.Point(27, 27);
            this.defineDebugCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.defineDebugCheckBox.Name = "defineDebugCheckBox";
            this.defineDebugCheckBox.Size = new System.Drawing.Size(136, 17);
            this.defineDebugCheckBox.TabIndex = 1;
            this.defineDebugCheckBox.Text = "Define &Debug constant";
            this.defineDebugCheckBox.UseVisualStyleBackColor = true;
            // 
            // defineConstantsTextBox
            // 
            this.defineConstantsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.defineConstantsTextBox.Location = new System.Drawing.Point(169, 53);
            this.defineConstantsTextBox.Name = "defineConstantsTextBox";
            this.defineConstantsTextBox.Size = new System.Drawing.Size(350, 21);
            this.defineConstantsTextBox.TabIndex = 3;
            // 
            // suppressWarningsGroupBox
            // 
            suppressWarningsGroupBox.Controls.Add(this.suppressWarningsSpecificRadioButton);
            suppressWarningsGroupBox.Controls.Add(this.suppressWarningsNoneRadioButton);
            suppressWarningsGroupBox.Controls.Add(this.specificWarningsTextBox);
            suppressWarningsGroupBox.Location = new System.Drawing.Point(0, 304);
            suppressWarningsGroupBox.Name = "suppressWarningsGroupBox";
            suppressWarningsGroupBox.Size = new System.Drawing.Size(519, 87);
            suppressWarningsGroupBox.TabIndex = 10;
            suppressWarningsGroupBox.Text = "Suppress Warnings";
            // 
            // suppressWarningsSpecificRadioButton
            // 
            this.suppressWarningsSpecificRadioButton.AutoSize = true;
            this.suppressWarningsSpecificRadioButton.Location = new System.Drawing.Point(27, 53);
            this.suppressWarningsSpecificRadioButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.suppressWarningsSpecificRadioButton.Name = "suppressWarningsSpecificRadioButton";
            this.suppressWarningsSpecificRadioButton.Size = new System.Drawing.Size(111, 17);
            this.suppressWarningsSpecificRadioButton.TabIndex = 12;
            this.suppressWarningsSpecificRadioButton.TabStop = true;
            this.suppressWarningsSpecificRadioButton.Text = "&Specific warnings:";
            this.suppressWarningsSpecificRadioButton.UseVisualStyleBackColor = true;
            // 
            // suppressWarningsNoneRadioButton
            // 
            this.suppressWarningsNoneRadioButton.AutoSize = true;
            this.suppressWarningsNoneRadioButton.Checked = true;
            this.suppressWarningsNoneRadioButton.Location = new System.Drawing.Point(27, 27);
            this.suppressWarningsNoneRadioButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.suppressWarningsNoneRadioButton.Name = "suppressWarningsNoneRadioButton";
            this.suppressWarningsNoneRadioButton.Size = new System.Drawing.Size(50, 17);
            this.suppressWarningsNoneRadioButton.TabIndex = 11;
            this.suppressWarningsNoneRadioButton.TabStop = true;
            this.suppressWarningsNoneRadioButton.Text = "&None";
            this.suppressWarningsNoneRadioButton.UseVisualStyleBackColor = true;
            // 
            // specificWarningsTextBox
            // 
            this.specificWarningsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.specificWarningsTextBox.Enabled = false;
            this.specificWarningsTextBox.Location = new System.Drawing.Point(169, 52);
            this.specificWarningsTextBox.Name = "specificWarningsTextBox";
            this.specificWarningsTextBox.Size = new System.Drawing.Size(350, 21);
            this.specificWarningsTextBox.TabIndex = 13;
            // 
            // errorsAndWarningsGroupBox
            // 
            errorsAndWarningsGroupBox.Controls.Add(this.pedanticCheckBox);
            errorsAndWarningsGroupBox.Controls.Add(this.warningsAsErrorsCheckBox);
            errorsAndWarningsGroupBox.Location = new System.Drawing.Point(0, 210);
            errorsAndWarningsGroupBox.Name = "errorsAndWarningsGroupBox";
            errorsAndWarningsGroupBox.Size = new System.Drawing.Size(519, 79);
            errorsAndWarningsGroupBox.TabIndex = 7;
            errorsAndWarningsGroupBox.Text = "Errors and Warnings";
            // 
            // pedanticCheckBox
            // 
            this.pedanticCheckBox.AutoSize = true;
            this.pedanticCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pedanticCheckBox.Location = new System.Drawing.Point(27, 50);
            this.pedanticCheckBox.Name = "pedanticCheckBox";
            this.pedanticCheckBox.Size = new System.Drawing.Size(146, 17);
            this.pedanticCheckBox.TabIndex = 9;
            this.pedanticCheckBox.Text = "Show &pedantic messages";
            this.pedanticCheckBox.UseVisualStyleBackColor = true;
            // 
            // warningsAsErrorsCheckBox
            // 
            this.warningsAsErrorsCheckBox.AutoSize = true;
            this.warningsAsErrorsCheckBox.Location = new System.Drawing.Point(27, 27);
            this.warningsAsErrorsCheckBox.Name = "warningsAsErrorsCheckBox";
            this.warningsAsErrorsCheckBox.Size = new System.Drawing.Size(144, 17);
            this.warningsAsErrorsCheckBox.TabIndex = 8;
            this.warningsAsErrorsCheckBox.Text = "&Treat warnings as errors";
            this.warningsAsErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // advancedButton
            // 
            this.advancedButton.AutoSize = true;
            this.advancedButton.Location = new System.Drawing.Point(442, 411);
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(77, 23);
            this.advancedButton.TabIndex = 15;
            this.advancedButton.Text = "Ad&vanced...";
            this.advancedButton.UseVisualStyleBackColor = true;
            this.advancedButton.Click += new System.EventHandler(this.advancedButton_Click);
            // 
            // WixCompilerPropertyPagePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(errorsAndWarningsGroupBox);
            this.Controls.Add(suppressWarningsGroupBox);
            this.Controls.Add(generalGroupBox);
            this.Controls.Add(this.advancedButton);
            this.Name = "WixCompilerPropertyPagePanel";
            this.Size = new System.Drawing.Size(528, 448);
            this.Controls.SetChildIndex(this.advancedButton, 0);
            this.Controls.SetChildIndex(this.banner, 0);
            this.Controls.SetChildIndex(generalGroupBox, 0);
            this.Controls.SetChildIndex(suppressWarningsGroupBox, 0);
            this.Controls.SetChildIndex(errorsAndWarningsGroupBox, 0);
            generalGroupBox.ResumeLayout(false);
            generalGroupBox.PerformLayout();
            suppressWarningsGroupBox.ResumeLayout(false);
            suppressWarningsGroupBox.PerformLayout();
            errorsAndWarningsGroupBox.ResumeLayout(false);
            errorsAndWarningsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox warningsAsErrorsCheckBox;
        private System.Windows.Forms.CheckBox pedanticCheckBox;
        private System.Windows.Forms.TextBox specificWarningsTextBox;
        private System.Windows.Forms.RadioButton suppressWarningsNoneRadioButton;
        private System.Windows.Forms.RadioButton suppressWarningsSpecificRadioButton;
        private System.Windows.Forms.TextBox defineConstantsTextBox;
        private System.Windows.Forms.CheckBox defineDebugCheckBox;
        private System.Windows.Forms.Button advancedButton;
        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox outputPathTextBox;

    }
}
