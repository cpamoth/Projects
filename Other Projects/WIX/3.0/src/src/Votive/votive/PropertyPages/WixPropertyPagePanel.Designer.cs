namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    partial class WixPropertyPagePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WixPropertyPagePanel));
            this.banner = new Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBanner();
            this.SuspendLayout();
            // 
            // banner
            // 
            this.banner.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.banner, "banner");
            this.banner.ForeColor = System.Drawing.Color.White;
            this.banner.MinimumSize = new System.Drawing.Size(121, 90);
            this.banner.Name = "banner";
            // 
            // WixPropertyPagePanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.banner);
            this.Name = "WixPropertyPagePanel";
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Banner
        /// </summary>
        protected Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls.WixBanner banner;


    }
}
