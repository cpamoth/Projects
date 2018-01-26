namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    partial class WixLinkerPropertyPagePanel
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
            System.Windows.Forms.Label cultureExampleLabel;
            System.Windows.Forms.Label outputPathLabel;
            System.Windows.Forms.Label culturesLabel;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox suppressWarningsGroupBox;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox errorsAndWarningsGroupBox;
            this.outputPathTextBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox();
            this.wixVariablesTextBox = new System.Windows.Forms.TextBox();
            this.culturesTextBox = new System.Windows.Forms.TextBox();
            this.suppressWarningsSpecificRadioButton = new System.Windows.Forms.RadioButton();
            this.suppressWarningsNoneRadioButton = new System.Windows.Forms.RadioButton();
            this.specificWarningsTextBox = new System.Windows.Forms.TextBox();
            this.pedanticCheckBox = new System.Windows.Forms.CheckBox();
            this.warningsAsErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.advancedButton = new System.Windows.Forms.Button();
            defineConstantsExampleLabel = new System.Windows.Forms.Label();
            defineConstantsLabel = new System.Windows.Forms.Label();
            generalGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            cultureExampleLabel = new System.Windows.Forms.Label();
            outputPathLabel = new System.Windows.Forms.Label();
            culturesLabel = new System.Windows.Forms.Label();
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
            this.banner.Text = "Light Settings";
            // 
            // defineConstantsExampleLabel
            // 
            defineConstantsExampleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            defineConstantsExampleLabel.AutoSize = true;
            defineConstantsExampleLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            defineConstantsExampleLabel.Location = new System.Drawing.Point(166, 51);
            defineConstantsExampleLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            defineConstantsExampleLabel.Name = "defineConstantsExampleLabel";
            defineConstantsExampleLabel.Size = new System.Drawing.Size(241, 13);
            defineConstantsExampleLabel.TabIndex = 3;
            defineConstantsExampleLabel.Text = "Example: Name1=Value1;Name2;Name3=Value3";
            // 
            // defineConstantsLabel
            // 
            defineConstantsLabel.AutoSize = true;
            defineConstantsLabel.Location = new System.Drawing.Point(24, 30);
            defineConstantsLabel.Name = "defineConstantsLabel";
            defineConstantsLabel.Size = new System.Drawing.Size(109, 13);
            defineConstantsLabel.TabIndex = 1;
            defineConstantsLabel.Text = "Define WiX &variables:";
            // 
            // generalGroupBox
            // 
            generalGroupBox.Controls.Add(this.outputPathTextBox);
            generalGroupBox.Controls.Add(cultureExampleLabel);
            generalGroupBox.Controls.Add(this.wixVariablesTextBox);
            generalGroupBox.Controls.Add(outputPathLabel);
            generalGroupBox.Controls.Add(defineConstantsExampleLabel);
            generalGroupBox.Controls.Add(defineConstantsLabel);
            generalGroupBox.Controls.Add(culturesLabel);
            generalGroupBox.Controls.Add(this.culturesTextBox);
            generalGroupBox.Location = new System.Drawing.Point(0, 58);
            generalGroupBox.Name = "generalGroupBox";
            generalGroupBox.Size = new System.Drawing.Size(519, 150);
            generalGroupBox.TabIndex = 0;
            generalGroupBox.Text = "General";
            // 
            // outputPathTextBox
            // 
            this.outputPathTextBox.DialogDescription = "Linker output path:";
            this.outputPathTextBox.Location = new System.Drawing.Point(169, 73);
            this.outputPathTextBox.Name = "outputPathTextBox";
            this.outputPathTextBox.Size = new System.Drawing.Size(350, 22);
            this.outputPathTextBox.TabIndex = 5;
            // 
            // cultureExampleLabel
            // 
            cultureExampleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            cultureExampleLabel.AutoSize = true;
            cultureExampleLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cultureExampleLabel.Location = new System.Drawing.Point(166, 124);
            cultureExampleLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            cultureExampleLabel.Name = "cultureExampleLabel";
            cultureExampleLabel.Size = new System.Drawing.Size(111, 13);
            cultureExampleLabel.TabIndex = 8;
            cultureExampleLabel.Text = "Example: en-US;ja-JP";
            // 
            // wixVariablesTextBox
            // 
            this.wixVariablesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.wixVariablesTextBox.Location = new System.Drawing.Point(169, 27);
            this.wixVariablesTextBox.Name = "wixVariablesTextBox";
            this.wixVariablesTextBox.Size = new System.Drawing.Size(350, 21);
            this.wixVariablesTextBox.TabIndex = 2;
            // 
            // outputPathLabel
            // 
            outputPathLabel.AutoSize = true;
            outputPathLabel.Location = new System.Drawing.Point(24, 76);
            outputPathLabel.Name = "outputPathLabel";
            outputPathLabel.Size = new System.Drawing.Size(70, 13);
            outputPathLabel.TabIndex = 4;
            outputPathLabel.Text = "&Output path:";
            // 
            // culturesLabel
            // 
            culturesLabel.AutoSize = true;
            culturesLabel.Location = new System.Drawing.Point(24, 104);
            culturesLabel.Name = "culturesLabel";
            culturesLabel.Size = new System.Drawing.Size(51, 13);
            culturesLabel.TabIndex = 6;
            culturesLabel.Text = "Cultures:";
            // 
            // culturesTextBox
            // 
            this.culturesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.culturesTextBox.Location = new System.Drawing.Point(169, 100);
            this.culturesTextBox.Name = "culturesTextBox";
            this.culturesTextBox.Size = new System.Drawing.Size(350, 21);
            this.culturesTextBox.TabIndex = 7;
            // 
            // suppressWarningsGroupBox
            // 
            suppressWarningsGroupBox.Controls.Add(this.suppressWarningsSpecificRadioButton);
            suppressWarningsGroupBox.Controls.Add(this.suppressWarningsNoneRadioButton);
            suppressWarningsGroupBox.Controls.Add(this.specificWarningsTextBox);
            suppressWarningsGroupBox.Location = new System.Drawing.Point(0, 317);
            suppressWarningsGroupBox.Name = "suppressWarningsGroupBox";
            suppressWarningsGroupBox.Size = new System.Drawing.Size(519, 88);
            suppressWarningsGroupBox.TabIndex = 11;
            suppressWarningsGroupBox.Text = "Suppress Warnings";
            // 
            // suppressWarningsSpecificRadioButton
            // 
            this.suppressWarningsSpecificRadioButton.AutoSize = true;
            this.suppressWarningsSpecificRadioButton.Location = new System.Drawing.Point(27, 53);
            this.suppressWarningsSpecificRadioButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.suppressWarningsSpecificRadioButton.Name = "suppressWarningsSpecificRadioButton";
            this.suppressWarningsSpecificRadioButton.Size = new System.Drawing.Size(111, 17);
            this.suppressWarningsSpecificRadioButton.TabIndex = 13;
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
            this.suppressWarningsNoneRadioButton.TabIndex = 12;
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
            this.specificWarningsTextBox.TabIndex = 14;
            // 
            // errorsAndWarningsGroupBox
            // 
            errorsAndWarningsGroupBox.Controls.Add(this.pedanticCheckBox);
            errorsAndWarningsGroupBox.Controls.Add(this.warningsAsErrorsCheckBox);
            errorsAndWarningsGroupBox.Location = new System.Drawing.Point(0, 223);
            errorsAndWarningsGroupBox.Name = "errorsAndWarningsGroupBox";
            errorsAndWarningsGroupBox.Size = new System.Drawing.Size(519, 79);
            errorsAndWarningsGroupBox.TabIndex = 8;
            errorsAndWarningsGroupBox.Text = "Errors and Warnings";
            // 
            // pedanticCheckBox
            // 
            this.pedanticCheckBox.AutoSize = true;
            this.pedanticCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pedanticCheckBox.Location = new System.Drawing.Point(27, 50);
            this.pedanticCheckBox.Name = "pedanticCheckBox";
            this.pedanticCheckBox.Size = new System.Drawing.Size(146, 17);
            this.pedanticCheckBox.TabIndex = 10;
            this.pedanticCheckBox.Text = "Show &pedantic messages";
            this.pedanticCheckBox.UseVisualStyleBackColor = true;
            // 
            // warningsAsErrorsCheckBox
            // 
            this.warningsAsErrorsCheckBox.AutoSize = true;
            this.warningsAsErrorsCheckBox.Location = new System.Drawing.Point(27, 27);
            this.warningsAsErrorsCheckBox.Name = "warningsAsErrorsCheckBox";
            this.warningsAsErrorsCheckBox.Size = new System.Drawing.Size(144, 17);
            this.warningsAsErrorsCheckBox.TabIndex = 9;
            this.warningsAsErrorsCheckBox.Text = "&Treat warnings as errors";
            this.warningsAsErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // advancedButton
            // 
            this.advancedButton.AutoSize = true;
            this.advancedButton.Location = new System.Drawing.Point(442, 430);
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(77, 23);
            this.advancedButton.TabIndex = 16;
            this.advancedButton.Text = "Ad&vanced...";
            this.advancedButton.UseVisualStyleBackColor = true;
            this.advancedButton.Click += new System.EventHandler(this.advancedButton_Click);
            // 
            // WixLinkerPropertyPagePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(errorsAndWarningsGroupBox);
            this.Controls.Add(suppressWarningsGroupBox);
            this.Controls.Add(generalGroupBox);
            this.Controls.Add(this.advancedButton);
            this.Name = "WixLinkerPropertyPagePanel";
            this.Size = new System.Drawing.Size(528, 471);
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
        private System.Windows.Forms.TextBox wixVariablesTextBox;
        private System.Windows.Forms.Button advancedButton;
        private System.Windows.Forms.TextBox culturesTextBox;
        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox outputPathTextBox;

    }
}
