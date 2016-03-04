namespace GetNumberFromTextBoxes
{
    partial class Form1
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
            this.tbFirstNumber = new System.Windows.Forms.TextBox();
            this.btnAnswer = new System.Windows.Forms.Button();
            this.tbSecondNumber = new System.Windows.Forms.TextBox();
            this.tbthirdNumber = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbFirstNumber
            // 
            this.tbFirstNumber.Location = new System.Drawing.Point(40, 35);
            this.tbFirstNumber.Name = "tbFirstNumber";
            this.tbFirstNumber.Size = new System.Drawing.Size(50, 22);
            this.tbFirstNumber.TabIndex = 0;
            this.tbFirstNumber.Text = "5656";
            // 
            // btnAnswer
            // 
            this.btnAnswer.Location = new System.Drawing.Point(86, 107);
            this.btnAnswer.Name = "btnAnswer";
            this.btnAnswer.Size = new System.Drawing.Size(75, 25);
            this.btnAnswer.TabIndex = 1;
            this.btnAnswer.Text = "Answer";
            this.btnAnswer.UseVisualStyleBackColor = true;
            this.btnAnswer.Click += new System.EventHandler(this.btnAnswer_Click);
            // 
            // tbSecondNumber
            // 
            this.tbSecondNumber.Location = new System.Drawing.Point(111, 35);
            this.tbSecondNumber.Name = "tbSecondNumber";
            this.tbSecondNumber.Size = new System.Drawing.Size(50, 22);
            this.tbSecondNumber.TabIndex = 2;
            this.tbSecondNumber.Text = "7";
            // 
            // tbthirdNumber
            // 
            this.tbthirdNumber.Location = new System.Drawing.Point(184, 34);
            this.tbthirdNumber.Name = "tbthirdNumber";
            this.tbthirdNumber.Size = new System.Drawing.Size(50, 22);
            this.tbthirdNumber.TabIndex = 3;
            this.tbthirdNumber.Text = "2156";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Controls.Add(this.tbthirdNumber);
            this.Controls.Add(this.tbSecondNumber);
            this.Controls.Add(this.btnAnswer);
            this.Controls.Add(this.tbFirstNumber);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbFirstNumber;
        private System.Windows.Forms.Button btnAnswer;
        private System.Windows.Forms.TextBox tbSecondNumber;
        private System.Windows.Forms.TextBox tbthirdNumber;
    }
}

