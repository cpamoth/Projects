namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Forms
{
    partial class WixCompilerAdvancedSettingsForm
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
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox errorsAndWarningsGroupBox;
            Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox includePathsGroupBox;
            System.Windows.Forms.Label folderLabel;
            this.verboseOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressSchemaValidationCheckBox = new System.Windows.Forms.CheckBox();
            this.showSourceTraceCheckBox = new System.Windows.Forms.CheckBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.folderBrowserTextBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.addFolderButton = new System.Windows.Forms.Button();
            this.foldersListBox = new System.Windows.Forms.ListBox();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            errorsAndWarningsGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            includePathsGroupBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixGroupBox();
            folderLabel = new System.Windows.Forms.Label();
            errorsAndWarningsGroupBox.SuspendLayout();
            includePathsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // errorsAndWarningsGroupBox
            // 
            errorsAndWarningsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            errorsAndWarningsGroupBox.Controls.Add(this.verboseOutputCheckBox);
            errorsAndWarningsGroupBox.Controls.Add(this.suppressSchemaValidationCheckBox);
            errorsAndWarningsGroupBox.Controls.Add(this.showSourceTraceCheckBox);
            errorsAndWarningsGroupBox.Location = new System.Drawing.Point(12, 12);
            errorsAndWarningsGroupBox.Margin = new System.Windows.Forms.Padding(3);
            errorsAndWarningsGroupBox.Name = "errorsAndWarningsGroupBox";
            errorsAndWarningsGroupBox.Size = new System.Drawing.Size(398, 100);
            errorsAndWarningsGroupBox.TabIndex = 0;
            errorsAndWarningsGroupBox.Text = "Errors and Warnings";
            // 
            // verboseOutputCheckBox
            // 
            this.verboseOutputCheckBox.AutoSize = true;
            this.verboseOutputCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.verboseOutputCheckBox.Location = new System.Drawing.Point(27, 73);
            this.verboseOutputCheckBox.Name = "verboseOutputCheckBox";
            this.verboseOutputCheckBox.Size = new System.Drawing.Size(100, 17);
            this.verboseOutputCheckBox.TabIndex = 3;
            this.verboseOutputCheckBox.Text = "&Verbose output";
            this.verboseOutputCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressSchemaValidationCheckBox
            // 
            this.suppressSchemaValidationCheckBox.AutoSize = true;
            this.suppressSchemaValidationCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.suppressSchemaValidationCheckBox.Location = new System.Drawing.Point(27, 27);
            this.suppressSchemaValidationCheckBox.Name = "suppressSchemaValidationCheckBox";
            this.suppressSchemaValidationCheckBox.Size = new System.Drawing.Size(328, 17);
            this.suppressSchemaValidationCheckBox.TabIndex = 1;
            this.suppressSchemaValidationCheckBox.Text = "Suppress sch&ema validation of documents (performance boost)";
            this.suppressSchemaValidationCheckBox.UseVisualStyleBackColor = true;
            // 
            // showSourceTraceCheckBox
            // 
            this.showSourceTraceCheckBox.AutoSize = true;
            this.showSourceTraceCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.showSourceTraceCheckBox.Location = new System.Drawing.Point(27, 50);
            this.showSourceTraceCheckBox.Name = "showSourceTraceCheckBox";
            this.showSourceTraceCheckBox.Size = new System.Drawing.Size(331, 17);
            this.showSourceTraceCheckBox.TabIndex = 2;
            this.showSourceTraceCheckBox.Text = "Show source &trace for errors, warnings, and verbose messages";
            this.showSourceTraceCheckBox.UseVisualStyleBackColor = true;
            // 
            // includePathsGroupBox
            // 
            includePathsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            includePathsGroupBox.Controls.Add(this.deleteButton);
            includePathsGroupBox.Controls.Add(this.folderBrowserTextBox);
            includePathsGroupBox.Controls.Add(this.moveDownButton);
            includePathsGroupBox.Controls.Add(this.updateButton);
            includePathsGroupBox.Controls.Add(this.addFolderButton);
            includePathsGroupBox.Controls.Add(this.foldersListBox);
            includePathsGroupBox.Controls.Add(this.moveUpButton);
            includePathsGroupBox.Controls.Add(folderLabel);
            includePathsGroupBox.Location = new System.Drawing.Point(12, 127);
            includePathsGroupBox.Name = "includePathsGroupBox";
            includePathsGroupBox.Size = new System.Drawing.Size(398, 225);
            includePathsGroupBox.TabIndex = 4;
            includePathsGroupBox.Text = "Include Paths";
            // 
            // deleteButton
            // 
            this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.deleteButton.Enabled = false;
            this.deleteButton.Image = global::Microsoft.Tools.WindowsInstallerXml.VisualStudio.WixStrings.Delete;
            this.deleteButton.Location = new System.Drawing.Point(369, 162);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(26, 26);
            this.deleteButton.TabIndex = 13;
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // folderBrowserTextBox
            // 
            this.folderBrowserTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.folderBrowserTextBox.DialogDescription = "Please choose a folder:";
            this.folderBrowserTextBox.Location = new System.Drawing.Point(27, 43);
            this.folderBrowserTextBox.Name = "folderBrowserTextBox";
            this.folderBrowserTextBox.Size = new System.Drawing.Size(371, 22);
            this.folderBrowserTextBox.TabIndex = 6;
            this.folderBrowserTextBox.TextChanged += new System.EventHandler(this.folderBrowserTextBox_TextChanged);
            // 
            // moveDownButton
            // 
            this.moveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Image = global::Microsoft.Tools.WindowsInstallerXml.VisualStudio.WixStrings.DownArrow;
            this.moveDownButton.Location = new System.Drawing.Point(369, 130);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(26, 26);
            this.moveDownButton.TabIndex = 12;
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // updateButton
            // 
            this.updateButton.AutoSize = true;
            this.updateButton.Enabled = false;
            this.updateButton.Location = new System.Drawing.Point(108, 69);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(75, 23);
            this.updateButton.TabIndex = 9;
            this.updateButton.Text = "&Update";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // addFolderButton
            // 
            this.addFolderButton.AutoSize = true;
            this.addFolderButton.Enabled = false;
            this.addFolderButton.Location = new System.Drawing.Point(27, 69);
            this.addFolderButton.Name = "addFolderButton";
            this.addFolderButton.Size = new System.Drawing.Size(75, 23);
            this.addFolderButton.TabIndex = 8;
            this.addFolderButton.Text = "&Add Folder";
            this.addFolderButton.UseVisualStyleBackColor = true;
            this.addFolderButton.Click += new System.EventHandler(this.addFolderButton_Click);
            // 
            // foldersListBox
            // 
            this.foldersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.foldersListBox.IntegralHeight = false;
            this.foldersListBox.Location = new System.Drawing.Point(27, 98);
            this.foldersListBox.Name = "foldersListBox";
            this.foldersListBox.Size = new System.Drawing.Size(336, 127);
            this.foldersListBox.TabIndex = 10;
            this.foldersListBox.SelectedIndexChanged += new System.EventHandler(this.foldersListBox_SelectedIndexChanged);
            // 
            // moveUpButton
            // 
            this.moveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveUpButton.Enabled = false;
            this.moveUpButton.Image = global::Microsoft.Tools.WindowsInstallerXml.VisualStudio.WixStrings.UpArrow;
            this.moveUpButton.Location = new System.Drawing.Point(369, 98);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(26, 26);
            this.moveUpButton.TabIndex = 11;
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // folderLabel
            // 
            folderLabel.AutoSize = true;
            folderLabel.Location = new System.Drawing.Point(24, 27);
            folderLabel.Name = "folderLabel";
            folderLabel.Size = new System.Drawing.Size(41, 13);
            folderLabel.TabIndex = 5;
            folderLabel.Text = "&Folder:";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(254, 360);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 14;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(335, 360);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 15;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // WixCompilerAdvancedSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 396);
            this.Controls.Add(errorsAndWarningsGroupBox);
            this.Controls.Add(includePathsGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(430, 430);
            this.Name = "WixCompilerAdvancedSettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Candle Advanced Settings";
            errorsAndWarningsGroupBox.ResumeLayout(false);
            errorsAndWarningsGroupBox.PerformLayout();
            includePathsGroupBox.ResumeLayout(false);
            includePathsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button addFolderButton;
        private System.Windows.Forms.ListBox foldersListBox;
        private System.Windows.Forms.CheckBox suppressSchemaValidationCheckBox;
        private System.Windows.Forms.CheckBox showSourceTraceCheckBox;
        private System.Windows.Forms.CheckBox verboseOutputCheckBox;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button moveDownButton;
        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox folderBrowserTextBox;
    }
}