namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Forms
{
    partial class WixLinkerAdvancedSettingsForm
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
            System.Windows.Forms.Label cachePathLabel;
            System.Windows.Forms.TabPage advancedTabPage;
            System.Windows.Forms.TabPage cabinetsTabPage;
            System.Windows.Forms.TabPage suppressionsTabPage;
            this.verboseOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.allowIdenticalRowsCheckBox = new System.Windows.Forms.CheckBox();
            this.setMsiAssemblyNameFileVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.leaveTemporaryFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.cabinetThreadsUpDown = new System.Windows.Forms.NumericUpDown();
            this.reuseCabCheckBox = new System.Windows.Forms.CheckBox();
            this.cabinetCacheTextBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox();
            this.suppressIcesCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressValidationCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressIntermediateFileVersionMismatchCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressDefaultUISequenceCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressSchemaValidationCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressLayoutCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressFileAndHashInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressMsiAssemblyTableProcessingCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressDroppingUnrealTablesCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressDefaultAdvSequenceCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressDefaultAdminSequenceCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressAclResetCheckBox = new System.Windows.Forms.CheckBox();
            this.suppressAssembliesCheckBox = new System.Windows.Forms.CheckBox();
            this.iceExampleLabel = new System.Windows.Forms.Label();
            this.suppressIcesTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.cabinetThreadsCheckBox = new System.Windows.Forms.CheckBox();
            cachePathLabel = new System.Windows.Forms.Label();
            advancedTabPage = new System.Windows.Forms.TabPage();
            cabinetsTabPage = new System.Windows.Forms.TabPage();
            suppressionsTabPage = new System.Windows.Forms.TabPage();
            advancedTabPage.SuspendLayout();
            cabinetsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cabinetThreadsUpDown)).BeginInit();
            suppressionsTabPage.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // cachePathLabel
            // 
            cachePathLabel.AutoSize = true;
            cachePathLabel.Location = new System.Drawing.Point(9, 109);
            cachePathLabel.Name = "cachePathLabel";
            cachePathLabel.Size = new System.Drawing.Size(115, 13);
            cachePathLabel.TabIndex = 3;
            cachePathLabel.Text = "&Path to cabinet cache:";
            // 
            // advancedTabPage
            // 
            advancedTabPage.Controls.Add(this.verboseOutputCheckBox);
            advancedTabPage.Controls.Add(this.allowIdenticalRowsCheckBox);
            advancedTabPage.Controls.Add(this.setMsiAssemblyNameFileVersionCheckBox);
            advancedTabPage.Controls.Add(this.leaveTemporaryFilesCheckBox);
            advancedTabPage.Location = new System.Drawing.Point(4, 22);
            advancedTabPage.Name = "advancedTabPage";
            advancedTabPage.Padding = new System.Windows.Forms.Padding(6);
            advancedTabPage.Size = new System.Drawing.Size(414, 377);
            advancedTabPage.TabIndex = 0;
            advancedTabPage.Text = "Advanced";
            advancedTabPage.UseVisualStyleBackColor = true;
            // 
            // verboseOutputCheckBox
            // 
            this.verboseOutputCheckBox.AutoSize = true;
            this.verboseOutputCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.verboseOutputCheckBox.Location = new System.Drawing.Point(9, 9);
            this.verboseOutputCheckBox.Name = "verboseOutputCheckBox";
            this.verboseOutputCheckBox.Size = new System.Drawing.Size(100, 17);
            this.verboseOutputCheckBox.TabIndex = 0;
            this.verboseOutputCheckBox.Text = "&Verbose output";
            this.verboseOutputCheckBox.UseVisualStyleBackColor = true;
            // 
            // allowIdenticalRowsCheckBox
            // 
            this.allowIdenticalRowsCheckBox.AutoSize = true;
            this.allowIdenticalRowsCheckBox.Location = new System.Drawing.Point(9, 32);
            this.allowIdenticalRowsCheckBox.Name = "allowIdenticalRowsCheckBox";
            this.allowIdenticalRowsCheckBox.Size = new System.Drawing.Size(233, 17);
            this.allowIdenticalRowsCheckBox.TabIndex = 1;
            this.allowIdenticalRowsCheckBox.Text = "Allow &identical rows, but treat as a warning";
            this.allowIdenticalRowsCheckBox.UseVisualStyleBackColor = true;
            // 
            // setMsiAssemblyNameFileVersionCheckBox
            // 
            this.setMsiAssemblyNameFileVersionCheckBox.AutoSize = true;
            this.setMsiAssemblyNameFileVersionCheckBox.Location = new System.Drawing.Point(9, 55);
            this.setMsiAssemblyNameFileVersionCheckBox.Name = "setMsiAssemblyNameFileVersionCheckBox";
            this.setMsiAssemblyNameFileVersionCheckBox.Size = new System.Drawing.Size(366, 17);
            this.setMsiAssemblyNameFileVersionCheckBox.TabIndex = 2;
            this.setMsiAssemblyNameFileVersionCheckBox.Text = "Add a \'&fileVersion\' entry to the MsiAssemblyName table (rarely needed)";
            this.setMsiAssemblyNameFileVersionCheckBox.UseVisualStyleBackColor = true;
            // 
            // leaveTemporaryFilesCheckBox
            // 
            this.leaveTemporaryFilesCheckBox.AutoSize = true;
            this.leaveTemporaryFilesCheckBox.Location = new System.Drawing.Point(9, 78);
            this.leaveTemporaryFilesCheckBox.Name = "leaveTemporaryFilesCheckBox";
            this.leaveTemporaryFilesCheckBox.Size = new System.Drawing.Size(276, 17);
            this.leaveTemporaryFilesCheckBox.TabIndex = 3;
            this.leaveTemporaryFilesCheckBox.Text = "&Do not delete temporary files (useful for debugging)";
            this.leaveTemporaryFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // cabinetsTabPage
            // 
            cabinetsTabPage.Controls.Add(this.cabinetThreadsCheckBox);
            cabinetsTabPage.Controls.Add(this.cabinetThreadsUpDown);
            cabinetsTabPage.Controls.Add(cachePathLabel);
            cabinetsTabPage.Controls.Add(this.reuseCabCheckBox);
            cabinetsTabPage.Controls.Add(this.cabinetCacheTextBox);
            cabinetsTabPage.Location = new System.Drawing.Point(4, 22);
            cabinetsTabPage.Name = "cabinetsTabPage";
            cabinetsTabPage.Padding = new System.Windows.Forms.Padding(6, 12, 6, 6);
            cabinetsTabPage.Size = new System.Drawing.Size(414, 377);
            cabinetsTabPage.TabIndex = 1;
            cabinetsTabPage.Text = "Cabinets";
            cabinetsTabPage.UseVisualStyleBackColor = true;
            // 
            // cabinetThreadsUpDown
            // 
            this.cabinetThreadsUpDown.Enabled = false;
            this.cabinetThreadsUpDown.Location = new System.Drawing.Point(28, 38);
            this.cabinetThreadsUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cabinetThreadsUpDown.Name = "cabinetThreadsUpDown";
            this.cabinetThreadsUpDown.Size = new System.Drawing.Size(75, 21);
            this.cabinetThreadsUpDown.TabIndex = 1;
            this.cabinetThreadsUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // reuseCabCheckBox
            // 
            this.reuseCabCheckBox.AutoSize = true;
            this.reuseCabCheckBox.Location = new System.Drawing.Point(9, 80);
            this.reuseCabCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.reuseCabCheckBox.Name = "reuseCabCheckBox";
            this.reuseCabCheckBox.Size = new System.Drawing.Size(193, 17);
            this.reuseCabCheckBox.TabIndex = 2;
            this.reuseCabCheckBox.Text = "&Reuse cabinets from cabinet cache";
            this.reuseCabCheckBox.UseVisualStyleBackColor = true;
            // 
            // cabinetCacheTextBox
            // 
            this.cabinetCacheTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cabinetCacheTextBox.DialogDescription = "Path to cabinet cache:";
            this.cabinetCacheTextBox.Location = new System.Drawing.Point(9, 125);
            this.cabinetCacheTextBox.Name = "cabinetCacheTextBox";
            this.cabinetCacheTextBox.Size = new System.Drawing.Size(396, 22);
            this.cabinetCacheTextBox.TabIndex = 4;
            // 
            // suppressionsTabPage
            // 
            suppressionsTabPage.Controls.Add(this.suppressIcesCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressValidationCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressIntermediateFileVersionMismatchCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressDefaultUISequenceCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressSchemaValidationCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressLayoutCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressFileAndHashInfoCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressFilesCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressMsiAssemblyTableProcessingCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressDroppingUnrealTablesCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressDefaultAdvSequenceCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressDefaultAdminSequenceCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressAclResetCheckBox);
            suppressionsTabPage.Controls.Add(this.suppressAssembliesCheckBox);
            suppressionsTabPage.Controls.Add(this.iceExampleLabel);
            suppressionsTabPage.Controls.Add(this.suppressIcesTextBox);
            suppressionsTabPage.Location = new System.Drawing.Point(4, 22);
            suppressionsTabPage.Name = "suppressionsTabPage";
            suppressionsTabPage.Padding = new System.Windows.Forms.Padding(6);
            suppressionsTabPage.Size = new System.Drawing.Size(414, 377);
            suppressionsTabPage.TabIndex = 2;
            suppressionsTabPage.Text = "Suppressions";
            suppressionsTabPage.UseVisualStyleBackColor = true;
            // 
            // suppressIcesCheckBox
            // 
            this.suppressIcesCheckBox.AutoSize = true;
            this.suppressIcesCheckBox.Location = new System.Drawing.Point(9, 308);
            this.suppressIcesCheckBox.Name = "suppressIcesCheckBox";
            this.suppressIcesCheckBox.Size = new System.Drawing.Size(254, 17);
            this.suppressIcesCheckBox.TabIndex = 13;
            this.suppressIcesCheckBox.Text = "Suppress internal consistency evaluators (I&CE):";
            this.suppressIcesCheckBox.UseVisualStyleBackColor = true;
            this.suppressIcesCheckBox.CheckedChanged += new System.EventHandler(this.suppressIcesCheckBox_CheckedChanged);
            // 
            // suppressValidationCheckBox
            // 
            this.suppressValidationCheckBox.AutoSize = true;
            this.suppressValidationCheckBox.Location = new System.Drawing.Point(9, 285);
            this.suppressValidationCheckBox.Name = "suppressValidationCheckBox";
            this.suppressValidationCheckBox.Size = new System.Drawing.Size(166, 17);
            this.suppressValidationCheckBox.TabIndex = 12;
            this.suppressValidationCheckBox.Text = "Suppress &MSI/MSM validation";
            this.suppressValidationCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressIntermediateFileVersionMismatchCheckBox
            // 
            this.suppressIntermediateFileVersionMismatchCheckBox.AutoSize = true;
            this.suppressIntermediateFileVersionMismatchCheckBox.Location = new System.Drawing.Point(9, 262);
            this.suppressIntermediateFileVersionMismatchCheckBox.Name = "suppressIntermediateFileVersionMismatchCheckBox";
            this.suppressIntermediateFileVersionMismatchCheckBox.Size = new System.Drawing.Size(279, 17);
            this.suppressIntermediateFileVersionMismatchCheckBox.TabIndex = 11;
            this.suppressIntermediateFileVersionMismatchCheckBox.Text = "Suppress &intermediate file version mismatch checking";
            this.suppressIntermediateFileVersionMismatchCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressDefaultUISequenceCheckBox
            // 
            this.suppressDefaultUISequenceCheckBox.AutoSize = true;
            this.suppressDefaultUISequenceCheckBox.Location = new System.Drawing.Point(9, 239);
            this.suppressDefaultUISequenceCheckBox.Name = "suppressDefaultUISequenceCheckBox";
            this.suppressDefaultUISequenceCheckBox.Size = new System.Drawing.Size(207, 17);
            this.suppressDefaultUISequenceCheckBox.TabIndex = 10;
            this.suppressDefaultUISequenceCheckBox.Text = "Suppress default &UI sequence actions";
            this.suppressDefaultUISequenceCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressSchemaValidationCheckBox
            // 
            this.suppressSchemaValidationCheckBox.AutoSize = true;
            this.suppressSchemaValidationCheckBox.Location = new System.Drawing.Point(9, 216);
            this.suppressSchemaValidationCheckBox.Name = "suppressSchemaValidationCheckBox";
            this.suppressSchemaValidationCheckBox.Size = new System.Drawing.Size(328, 17);
            this.suppressSchemaValidationCheckBox.TabIndex = 9;
            this.suppressSchemaValidationCheckBox.Text = "Suppress &schema validation of documents (performance boost)";
            this.suppressSchemaValidationCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressLayoutCheckBox
            // 
            this.suppressLayoutCheckBox.AutoSize = true;
            this.suppressLayoutCheckBox.Location = new System.Drawing.Point(9, 193);
            this.suppressLayoutCheckBox.Name = "suppressLayoutCheckBox";
            this.suppressLayoutCheckBox.Size = new System.Drawing.Size(103, 17);
            this.suppressLayoutCheckBox.TabIndex = 8;
            this.suppressLayoutCheckBox.Text = "Suppress &layout";
            this.suppressLayoutCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressFileAndHashInfoCheckBox
            // 
            this.suppressFileAndHashInfoCheckBox.AutoSize = true;
            this.suppressFileAndHashInfoCheckBox.Location = new System.Drawing.Point(9, 170);
            this.suppressFileAndHashInfoCheckBox.Name = "suppressFileAndHashInfoCheckBox";
            this.suppressFileAndHashInfoCheckBox.Size = new System.Drawing.Size(314, 17);
            this.suppressFileAndHashInfoCheckBox.TabIndex = 7;
            this.suppressFileAndHashInfoCheckBox.Text = "Suppress file info (do not get &hash, version, language, etc.)";
            this.suppressFileAndHashInfoCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressFilesCheckBox
            // 
            this.suppressFilesCheckBox.AutoSize = true;
            this.suppressFilesCheckBox.Location = new System.Drawing.Point(9, 147);
            this.suppressFilesCheckBox.Name = "suppressFilesCheckBox";
            this.suppressFilesCheckBox.Size = new System.Drawing.Size(251, 17);
            this.suppressFilesCheckBox.TabIndex = 6;
            this.suppressFilesCheckBox.Text = "Suppress &files  (do not get any file information)";
            this.suppressFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressMsiAssemblyTableProcessingCheckBox
            // 
            this.suppressMsiAssemblyTableProcessingCheckBox.AutoSize = true;
            this.suppressMsiAssemblyTableProcessingCheckBox.Location = new System.Drawing.Point(9, 124);
            this.suppressMsiAssemblyTableProcessingCheckBox.Name = "suppressMsiAssemblyTableProcessingCheckBox";
            this.suppressMsiAssemblyTableProcessingCheckBox.Size = new System.Drawing.Size(269, 17);
            this.suppressMsiAssemblyTableProcessingCheckBox.TabIndex = 5;
            this.suppressMsiAssemblyTableProcessingCheckBox.Text = "Suppress &processing the data in MsiAssembly table";
            this.suppressMsiAssemblyTableProcessingCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressDroppingUnrealTablesCheckBox
            // 
            this.suppressDroppingUnrealTablesCheckBox.AutoSize = true;
            this.suppressDroppingUnrealTablesCheckBox.Location = new System.Drawing.Point(9, 101);
            this.suppressDroppingUnrealTablesCheckBox.Name = "suppressDroppingUnrealTablesCheckBox";
            this.suppressDroppingUnrealTablesCheckBox.Size = new System.Drawing.Size(278, 17);
            this.suppressDroppingUnrealTablesCheckBox.TabIndex = 4;
            this.suppressDroppingUnrealTablesCheckBox.Text = "Suppress &dropping unreal tables to the output image";
            this.suppressDroppingUnrealTablesCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressDefaultAdvSequenceCheckBox
            // 
            this.suppressDefaultAdvSequenceCheckBox.AutoSize = true;
            this.suppressDefaultAdvSequenceCheckBox.Location = new System.Drawing.Point(9, 78);
            this.suppressDefaultAdvSequenceCheckBox.Name = "suppressDefaultAdvSequenceCheckBox";
            this.suppressDefaultAdvSequenceCheckBox.Size = new System.Drawing.Size(214, 17);
            this.suppressDefaultAdvSequenceCheckBox.TabIndex = 3;
            this.suppressDefaultAdvSequenceCheckBox.Text = "Suppress default ad&v sequence actions";
            this.suppressDefaultAdvSequenceCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressDefaultAdminSequenceCheckBox
            // 
            this.suppressDefaultAdminSequenceCheckBox.AutoSize = true;
            this.suppressDefaultAdminSequenceCheckBox.Location = new System.Drawing.Point(9, 55);
            this.suppressDefaultAdminSequenceCheckBox.Name = "suppressDefaultAdminSequenceCheckBox";
            this.suppressDefaultAdminSequenceCheckBox.Size = new System.Drawing.Size(224, 17);
            this.suppressDefaultAdminSequenceCheckBox.TabIndex = 2;
            this.suppressDefaultAdminSequenceCheckBox.Text = "Suppress default admi&n sequence actions";
            this.suppressDefaultAdminSequenceCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressAclResetCheckBox
            // 
            this.suppressAclResetCheckBox.AutoSize = true;
            this.suppressAclResetCheckBox.Location = new System.Drawing.Point(9, 32);
            this.suppressAclResetCheckBox.Name = "suppressAclResetCheckBox";
            this.suppressAclResetCheckBox.Size = new System.Drawing.Size(387, 17);
            this.suppressAclResetCheckBox.TabIndex = 1;
            this.suppressAclResetCheckBox.Text = "Suppress &resetting ACLs (useful when laying out image to a network share)";
            this.suppressAclResetCheckBox.UseVisualStyleBackColor = true;
            // 
            // suppressAssembliesCheckBox
            // 
            this.suppressAssembliesCheckBox.AutoSize = true;
            this.suppressAssembliesCheckBox.Location = new System.Drawing.Point(9, 9);
            this.suppressAssembliesCheckBox.Name = "suppressAssembliesCheckBox";
            this.suppressAssembliesCheckBox.Size = new System.Drawing.Size(389, 17);
            this.suppressAssembliesCheckBox.TabIndex = 0;
            this.suppressAssembliesCheckBox.Text = "Suppress &assemblies (do not get assembly name information for assemblies)";
            this.suppressAssembliesCheckBox.UseVisualStyleBackColor = true;
            // 
            // iceExampleLabel
            // 
            this.iceExampleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.iceExampleLabel.AutoSize = true;
            this.iceExampleLabel.Enabled = false;
            this.iceExampleLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.iceExampleLabel.Location = new System.Drawing.Point(26, 355);
            this.iceExampleLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.iceExampleLabel.Name = "iceExampleLabel";
            this.iceExampleLabel.Size = new System.Drawing.Size(149, 13);
            this.iceExampleLabel.TabIndex = 14;
            this.iceExampleLabel.Text = "Example: ICE33;ICE34;ICE35";
            // 
            // suppressIcesTextBox
            // 
            this.suppressIcesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.suppressIcesTextBox.Enabled = false;
            this.suppressIcesTextBox.Location = new System.Drawing.Point(28, 331);
            this.suppressIcesTextBox.Name = "suppressIcesTextBox";
            this.suppressIcesTextBox.Size = new System.Drawing.Size(377, 21);
            this.suppressIcesTextBox.TabIndex = 14;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(278, 421);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 15;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(359, 421);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 16;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(advancedTabPage);
            this.tabControl.Controls.Add(cabinetsTabPage);
            this.tabControl.Controls.Add(suppressionsTabPage);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(422, 403);
            this.tabControl.TabIndex = 17;
            // 
            // cabinetThreadsCheckBox
            // 
            this.cabinetThreadsCheckBox.AutoSize = true;
            this.cabinetThreadsCheckBox.Location = new System.Drawing.Point(9, 15);
            this.cabinetThreadsCheckBox.Name = "cabinetThreadsCheckBox";
            this.cabinetThreadsCheckBox.Size = new System.Drawing.Size(304, 17);
            this.cabinetThreadsCheckBox.TabIndex = 0;
            this.cabinetThreadsCheckBox.Text = "&Specify number of threads to use when creating cabinets:";
            this.cabinetThreadsCheckBox.UseVisualStyleBackColor = true;
            this.cabinetThreadsCheckBox.CheckedChanged += new System.EventHandler(this.cabinetThreadsCheckBox_CheckedChanged);
            // 
            // WixLinkerAdvancedSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 457);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(430, 430);
            this.Name = "WixLinkerAdvancedSettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Light Advanced Settings";
            advancedTabPage.ResumeLayout(false);
            advancedTabPage.PerformLayout();
            cabinetsTabPage.ResumeLayout(false);
            cabinetsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cabinetThreadsUpDown)).EndInit();
            suppressionsTabPage.ResumeLayout(false);
            suppressionsTabPage.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox verboseOutputCheckBox;
        private System.Windows.Forms.CheckBox allowIdenticalRowsCheckBox;
        private System.Windows.Forms.TextBox suppressIcesTextBox;
        private System.Windows.Forms.CheckBox reuseCabCheckBox;
        private System.Windows.Forms.CheckBox leaveTemporaryFilesCheckBox;
        private System.Windows.Forms.CheckBox setMsiAssemblyNameFileVersionCheckBox;
        private System.Windows.Forms.NumericUpDown cabinetThreadsUpDown;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.FolderBrowserTextBox cabinetCacheTextBox;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.CheckBox suppressValidationCheckBox;
        private System.Windows.Forms.CheckBox suppressIntermediateFileVersionMismatchCheckBox;
        private System.Windows.Forms.CheckBox suppressDefaultUISequenceCheckBox;
        private System.Windows.Forms.CheckBox suppressSchemaValidationCheckBox;
        private System.Windows.Forms.CheckBox suppressLayoutCheckBox;
        private System.Windows.Forms.CheckBox suppressFileAndHashInfoCheckBox;
        private System.Windows.Forms.CheckBox suppressFilesCheckBox;
        private System.Windows.Forms.CheckBox suppressMsiAssemblyTableProcessingCheckBox;
        private System.Windows.Forms.CheckBox suppressDroppingUnrealTablesCheckBox;
        private System.Windows.Forms.CheckBox suppressDefaultAdvSequenceCheckBox;
        private System.Windows.Forms.CheckBox suppressDefaultAdminSequenceCheckBox;
        private System.Windows.Forms.CheckBox suppressAclResetCheckBox;
        private System.Windows.Forms.CheckBox suppressAssembliesCheckBox;
        private System.Windows.Forms.CheckBox suppressIcesCheckBox;
        private System.Windows.Forms.Label iceExampleLabel;
        private System.Windows.Forms.CheckBox cabinetThreadsCheckBox;
    }
}