namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    partial class WixLibrarianPropertyPagePanel
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
            System.Windows.Forms.Label outputPathLabel;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox generalGroupBox;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox suppressWarningsGroupBox;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox errorsAndWarningsGroupBox;
            this.outputPathTextBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox();
            this.bindFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressWarningsSpecificRadioButton = new System.Windows.Forms.RadioButton();
            this.suppressWarningsNoneRadioButton = new System.Windows.Forms.RadioButton();
            this.specificWarningsTextBox = new System.Windows.Forms.TextBox();
            this.suppressIntermediateFileVersionMismatchCheckBox = new System.Windows.Forms.CheckBox();
            this.verboseOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressSchemaValidationCheckBox = new System.Windows.Forms.CheckBox();
            this.warningsAsErrorsCheckBox = new System.Windows.Forms.CheckBox();
            outputPathLabel = new System.Windows.Forms.Label();
            generalGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
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
            this.banner.Text = "Lit Settings";
            // 
            // outputPathLabel
            // 
            outputPathLabel.AutoSize = true;
            outputPathLabel.Location = new System.Drawing.Point(24, 30);
            outputPathLabel.Name = "outputPathLabel";
            outputPathLabel.Size = new System.Drawing.Size(70, 13);
            outputPathLabel.TabIndex = 1;
            outputPathLabel.Text = "&Output path:";
            // 
            // generalGroupBox
            // 
            generalGroupBox.Controls.Add(this.outputPathTextBox);
            generalGroupBox.Controls.Add(outputPathLabel);
            generalGroupBox.Controls.Add(this.bindFilesCheckBox);
            generalGroupBox.Location = new System.Drawing.Point(0, 58);
            generalGroupBox.Name = "generalGroupBox";
            generalGroupBox.Size = new System.Drawing.Size(519, 98);
            generalGroupBox.TabIndex = 0;
            generalGroupBox.Text = "General";
            // 
            // outputPathTextBox
            // 
            this.outputPathTextBox.DialogDescription = "Compiler output path:";
            this.outputPathTextBox.Location = new System.Drawing.Point(169, 27);
            this.outputPathTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.outputPathTextBox.Name = "outputPathTextBox";
            this.outputPathTextBox.Size = new System.Drawing.Size(350, 22);
            this.outputPathTextBox.TabIndex = 2;
            // 
            // bindFilesCheckBox
            // 
            this.bindFilesCheckBox.AutoSize = true;
            this.bindFilesCheckBox.Location = new System.Drawing.Point(27, 64);
            this.bindFilesCheckBox.Name = "bindFilesCheckBox";
            this.bindFilesCheckBox.Size = new System.Drawing.Size(158, 17);
            this.bindFilesCheckBox.TabIndex = 3;
            this.bindFilesCheckBox.Text = "&Bind files into the library file";
            this.bindFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressWarningsGroupBox
            // 
            suppressWarningsGroupBox.Controls.Add(this.suppressWarningsSpecificRadioButton);
            suppressWarningsGroupBox.Controls.Add(this.suppressWarningsNoneRadioButton);
            suppressWarningsGroupBox.Controls.Add(this.specificWarningsTextBox);
            suppressWarningsGroupBox.Location = new System.Drawing.Point(0, 315);
            suppressWarningsGroupBox.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
            suppressWarningsGroupBox.Name = "suppressWarningsGroupBox";
            suppressWarningsGroupBox.Size = new System.Drawing.Size(519, 89);
            suppressWarningsGroupBox.TabIndex = 9;
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
            errorsAndWarningsGroupBox.Controls.Add(this.suppressIntermediateFileVersionMismatchCheckBox);
            errorsAndWarningsGroupBox.Controls.Add(this.verboseOutputCheckBox);
            errorsAndWarningsGroupBox.Controls.Add(this.suppressSchemaValidationCheckBox);
            errorsAndWarningsGroupBox.Controls.Add(this.warningsAsErrorsCheckBox);
            errorsAndWarningsGroupBox.Location = new System.Drawing.Point(0, 171);
            errorsAndWarningsGroupBox.Name = "errorsAndWarningsGroupBox";
            errorsAndWarningsGroupBox.Size = new System.Drawing.Size(519, 129);
            errorsAndWarningsGroupBox.TabIndex = 4;
            errorsAndWarningsGroupBox.Text = "Errors and Warnings";
            // 
            // suppressIntermediateFileVersionMismatchCheckBox
            // 
            this.suppressIntermediateFileVersionMismatchCheckBox.AutoSize = true;
            this.suppressIntermediateFileVersionMismatchCheckBox.Location = new System.Drawing.Point(27, 73);
            this.suppressIntermediateFileVersionMismatchCheckBox.Name = "suppressIntermediateFileVersionMismatchCheckBox";
            this.suppressIntermediateFileVersionMismatchCheckBox.Size = new System.Drawing.Size(279, 17);
            this.suppressIntermediateFileVersionMismatchCheckBox.TabIndex = 7;
            this.suppressIntermediateFileVersionMismatchCheckBox.Text = "Suppress &intermediate file version mismatch checking";
            this.suppressIntermediateFileVersionMismatchCheckBox.UseVisualStyleBackColor = true;
            // 
            // verboseOutputCheckBox
            // 
            this.verboseOutputCheckBox.AutoSize = true;
            this.verboseOutputCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.verboseOutputCheckBox.Location = new System.Drawing.Point(27, 96);
            this.verboseOutputCheckBox.Name = "verboseOutputCheckBox";
            this.verboseOutputCheckBox.Size = new System.Drawing.Size(100, 17);
            this.verboseOutputCheckBox.TabIndex = 8;
            this.verboseOutputCheckBox.Text = "&Verbose output";
            this.verboseOutputCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressSchemaValidationCheckBox
            // 
            this.suppressSchemaValidationCheckBox.AutoSize = true;
            this.suppressSchemaValidationCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.suppressSchemaValidationCheckBox.Location = new System.Drawing.Point(27, 50);
            this.suppressSchemaValidationCheckBox.Name = "suppressSchemaValidationCheckBox";
            this.suppressSchemaValidationCheckBox.Size = new System.Drawing.Size(328, 17);
            this.suppressSchemaValidationCheckBox.TabIndex = 6;
            this.suppressSchemaValidationCheckBox.Text = "Suppress sch&ema validation of documents (performance boost)";
            this.suppressSchemaValidationCheckBox.UseVisualStyleBackColor = true;
            // 
            // warningsAsErrorsCheckBox
            // 
            this.warningsAsErrorsCheckBox.AutoSize = true;
            this.warningsAsErrorsCheckBox.Location = new System.Drawing.Point(27, 27);
            this.warningsAsErrorsCheckBox.Name = "warningsAsErrorsCheckBox";
            this.warningsAsErrorsCheckBox.Size = new System.Drawing.Size(144, 17);
            this.warningsAsErrorsCheckBox.TabIndex = 5;
            this.warningsAsErrorsCheckBox.Text = "&Treat warnings as errors";
            this.warningsAsErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // WixLibrarianPropertyPagePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(errorsAndWarningsGroupBox);
            this.Controls.Add(suppressWarningsGroupBox);
            this.Controls.Add(generalGroupBox);
            this.Name = "WixLibrarianPropertyPagePanel";
            this.Size = new System.Drawing.Size(528, 421);
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

        }

        #endregion

        private System.Windows.Forms.CheckBox warningsAsErrorsCheckBox;
        private System.Windows.Forms.TextBox specificWarningsTextBox;
        private System.Windows.Forms.RadioButton suppressWarningsNoneRadioButton;
        private System.Windows.Forms.RadioButton suppressWarningsSpecificRadioButton;
        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox outputPathTextBox;
        private System.Windows.Forms.CheckBox bindFilesCheckBox;
        private System.Windows.Forms.CheckBox verboseOutputCheckBox;
        private System.Windows.Forms.CheckBox suppressSchemaValidationCheckBox;
        private System.Windows.Forms.CheckBox suppressIntermediateFileVersionMismatchCheckBox;

    }
}
