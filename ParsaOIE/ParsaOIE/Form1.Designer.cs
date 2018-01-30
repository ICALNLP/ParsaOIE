namespace ParsaOIE
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnExtractOieRelations = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(529, 256);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // btnExtractOieRelations
            // 
            this.btnExtractOieRelations.Location = new System.Drawing.Point(205, 279);
            this.btnExtractOieRelations.Name = "btnExtractOieRelations";
            this.btnExtractOieRelations.Size = new System.Drawing.Size(131, 23);
            this.btnExtractOieRelations.TabIndex = 1;
            this.btnExtractOieRelations.Text = "استخراج اطلاعات آزاد";
            this.btnExtractOieRelations.UseVisualStyleBackColor = true;
            this.btnExtractOieRelations.Click += new System.EventHandler(this.btnExtractOieRelations_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 314);
            this.Controls.Add(this.btnExtractOieRelations);
            this.Controls.Add(this.richTextBox1);
            this.Name = "Form1";
            this.Text = "استخراج آزاد اطلاعات پارسا";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnExtractOieRelations;
    }
}

