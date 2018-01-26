namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Forms
{
    partial class WixBuildEventEditorForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Button cancelButton;
            System.Windows.Forms.Button okButton;
            System.Windows.Forms.ColumnHeader nameColumnHeader;
            System.Windows.Forms.ColumnHeader valueColumnHeader;
            this.contentTextBox = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBuildEventTextBox();
            this.macrosListView = new System.Windows.Forms.ListView();
            this.macrosButton = new System.Windows.Forms.Button();
            this.insertButton = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            cancelButton = new System.Windows.Forms.Button();
            okButton = new System.Windows.Forms.Button();
            nameColumnHeader = new System.Windows.Forms.ColumnHeader();
            valueColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            cancelButton.Location = new System.Drawing.Point(376, 382);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            okButton.Location = new System.Drawing.Point(295, 382);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.TabIndex = 3;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            // 
            // nameColumnHeader
            // 
            nameColumnHeader.Text = "Macro";
            nameColumnHeader.Width = 140;
            // 
            // valueColumnHeader
            // 
            valueColumnHeader.Text = "Value";
            valueColumnHeader.Width = 260;
            // 
            // contentTextBox
            // 
            this.contentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.contentTextBox.Location = new System.Drawing.Point(12, 12);
            this.contentTextBox.Name = "contentTextBox";
            this.contentTextBox.Size = new System.Drawing.Size(439, 180);
            this.contentTextBox.TabIndex = 0;
            // 
            // macrosListView
            // 
            this.macrosListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.macrosListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            nameColumnHeader,
            valueColumnHeader});
            this.macrosListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.macrosListView.Location = new System.Drawing.Point(12, 198);
            this.macrosListView.MultiSelect = false;
            this.macrosListView.Name = "macrosListView";
            this.macrosListView.Size = new System.Drawing.Size(439, 136);
            this.macrosListView.SmallImageList = this.imageList;
            this.macrosListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.macrosListView.TabIndex = 1;
            this.macrosListView.UseCompatibleStateImageBehavior = false;
            this.macrosListView.View = System.Windows.Forms.View.Details;
            this.macrosListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.macrosListView_MouseDoubleClick);
            this.macrosListView.SelectedIndexChanged += new System.EventHandler(this.macrosListView_SelectedIndexChanged);
            // 
            // macrosButton
            // 
            this.macrosButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.macrosButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.macrosButton.Location = new System.Drawing.Point(343, 340);
            this.macrosButton.Name = "macrosButton";
            this.macrosButton.Size = new System.Drawing.Size(108, 23);
            this.macrosButton.TabIndex = 4;
            this.macrosButton.Text = "Hide &Macros";
            this.macrosButton.UseVisualStyleBackColor = true;
            this.macrosButton.Click += new System.EventHandler(this.macrosButton_Click);
            // 
            // insertButton
            // 
            this.insertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.insertButton.Enabled = false;
            this.insertButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.insertButton.Location = new System.Drawing.Point(12, 340);
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(75, 23);
            this.insertButton.TabIndex = 5;
            this.insertButton.Text = "&Insert";
            this.insertButton.UseVisualStyleBackColor = true;
            this.insertButton.Click += new System.EventHandler(this.insertButton_Click);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // WixBuildEventEditorForm
            // 
            this.AcceptButton = okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = cancelButton;
            this.ClientSize = new System.Drawing.Size(463, 417);
            this.Controls.Add(this.insertButton);
            this.Controls.Add(this.macrosButton);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(this.macrosListView);
            this.Controls.Add(this.contentTextBox);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WixBuildEventEditorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WixBuildEventEditorForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBuildEventTextBox contentTextBox;
        private System.Windows.Forms.ListView macrosListView;
        private System.Windows.Forms.Button macrosButton;
        private System.Windows.Forms.Button insertButton;
        private System.Windows.Forms.ImageList imageList;
    }
}