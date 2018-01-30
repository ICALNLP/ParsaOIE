namespace RahatCoreNlp.UI
{
    partial class DependencyGraphViewer
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.copySentenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cONNLFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dependencyTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.originalDependencyTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAndKeepPpDependencyTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapsedAndDropPpDependencyTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripFarsi = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripEnglish = new System.Windows.Forms.ToolStripMenuItem();
            this.breakLoopsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripBreakLoopsTrue = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripBreakLoopsFalse = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graphViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImageAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.menuStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(622, 357);
            this.panel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(177, 119);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copySentenceToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.dependencyTypeToolStripMenuItem,
            this.languageToolStripMenuItem,
            this.breakLoopsToolStripMenuItem,
            this.saveImageAsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(622, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // copySentenceToolStripMenuItem
            // 
            this.copySentenceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.textToolStripMenuItem,
            this.cONNLFormatToolStripMenuItem});
            this.copySentenceToolStripMenuItem.Name = "copySentenceToolStripMenuItem";
            this.copySentenceToolStripMenuItem.Size = new System.Drawing.Size(98, 20);
            this.copySentenceToolStripMenuItem.Text = "Copy Sentence";
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.textToolStripMenuItem.Text = "Text";
            this.textToolStripMenuItem.Click += new System.EventHandler(this.textToolStripMenuItem_Click);
            // 
            // cONNLFormatToolStripMenuItem
            // 
            this.cONNLFormatToolStripMenuItem.Name = "cONNLFormatToolStripMenuItem";
            this.cONNLFormatToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.cONNLFormatToolStripMenuItem.Text = "CONNL Format";
            this.cONNLFormatToolStripMenuItem.Click += new System.EventHandler(this.cONNLFormatToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.treeViewToolStripMenuItem,
            this.graphViewToolStripMenuItem,
            this.lineViewToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // dependencyTypeToolStripMenuItem
            // 
            this.dependencyTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.originalDependencyTypeToolStripMenuItem,
            this.collapseAndKeepPpDependencyTypeToolStripMenuItem,
            this.collapsedAndDropPpDependencyTypeToolStripMenuItem});
            this.dependencyTypeToolStripMenuItem.Name = "dependencyTypeToolStripMenuItem";
            this.dependencyTypeToolStripMenuItem.Size = new System.Drawing.Size(110, 20);
            this.dependencyTypeToolStripMenuItem.Text = "DependencyType";
            // 
            // originalDependencyTypeToolStripMenuItem
            // 
            this.originalDependencyTypeToolStripMenuItem.Name = "originalDependencyTypeToolStripMenuItem";
            this.originalDependencyTypeToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.originalDependencyTypeToolStripMenuItem.Text = "Original";
            this.originalDependencyTypeToolStripMenuItem.Click += new System.EventHandler(this.originalDependencyTypeToolStripMenuItem_Click);
            // 
            // collapseAndKeepPpDependencyTypeToolStripMenuItem
            // 
            this.collapseAndKeepPpDependencyTypeToolStripMenuItem.Name = "collapseAndKeepPpDependencyTypeToolStripMenuItem";
            this.collapseAndKeepPpDependencyTypeToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.collapseAndKeepPpDependencyTypeToolStripMenuItem.Text = "Collapse and Keep Pp";
            this.collapseAndKeepPpDependencyTypeToolStripMenuItem.Click += new System.EventHandler(this.collapseAndKeepPpDependencyTypeToolStripMenuItem_Click);
            // 
            // collapsedAndDropPpDependencyTypeToolStripMenuItem
            // 
            this.collapsedAndDropPpDependencyTypeToolStripMenuItem.Name = "collapsedAndDropPpDependencyTypeToolStripMenuItem";
            this.collapsedAndDropPpDependencyTypeToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.collapsedAndDropPpDependencyTypeToolStripMenuItem.Text = "Collapsed and Drop Pp";
            this.collapsedAndDropPpDependencyTypeToolStripMenuItem.Click += new System.EventHandler(this.collapsedAndDropPpDependencyTypeToolStripMenuItem_Click);
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripFarsi,
            this.ToolStripEnglish});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.languageToolStripMenuItem.Text = "Language";
            // 
            // ToolStripFarsi
            // 
            this.ToolStripFarsi.Name = "ToolStripFarsi";
            this.ToolStripFarsi.Size = new System.Drawing.Size(112, 22);
            this.ToolStripFarsi.Text = "فارسی";
            this.ToolStripFarsi.Click += new System.EventHandler(this.ToolStripFarsi_Click);
            // 
            // ToolStripEnglish
            // 
            this.ToolStripEnglish.Name = "ToolStripEnglish";
            this.ToolStripEnglish.Size = new System.Drawing.Size(112, 22);
            this.ToolStripEnglish.Text = "English";
            this.ToolStripEnglish.Click += new System.EventHandler(this.ToolStripEnglish_Click);
            // 
            // breakLoopsToolStripMenuItem
            // 
            this.breakLoopsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripBreakLoopsTrue,
            this.ToolStripBreakLoopsFalse});
            this.breakLoopsToolStripMenuItem.Name = "breakLoopsToolStripMenuItem";
            this.breakLoopsToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.breakLoopsToolStripMenuItem.Text = "Break Loops";
            // 
            // ToolStripBreakLoopsTrue
            // 
            this.ToolStripBreakLoopsTrue.Name = "ToolStripBreakLoopsTrue";
            this.ToolStripBreakLoopsTrue.Size = new System.Drawing.Size(100, 22);
            this.ToolStripBreakLoopsTrue.Text = "True";
            this.ToolStripBreakLoopsTrue.Click += new System.EventHandler(this.ToolStripBreakLoopsTrue_Click);
            // 
            // ToolStripBreakLoopsFalse
            // 
            this.ToolStripBreakLoopsFalse.Name = "ToolStripBreakLoopsFalse";
            this.ToolStripBreakLoopsFalse.Size = new System.Drawing.Size(100, 22);
            this.ToolStripBreakLoopsFalse.Text = "False";
            this.ToolStripBreakLoopsFalse.Click += new System.EventHandler(this.ToolStripBreakLoopsFalse_Click);
            // 
            // treeViewToolStripMenuItem
            // 
            this.treeViewToolStripMenuItem.Name = "treeViewToolStripMenuItem";
            this.treeViewToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.treeViewToolStripMenuItem.Text = "Tree View";
            this.treeViewToolStripMenuItem.Click += new System.EventHandler(this.btnTreeView_Click);
            // 
            // graphViewToolStripMenuItem
            // 
            this.graphViewToolStripMenuItem.Name = "graphViewToolStripMenuItem";
            this.graphViewToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.graphViewToolStripMenuItem.Text = "Graph View";
            this.graphViewToolStripMenuItem.Click += new System.EventHandler(this.graphViewToolStripMenuItem_Click);
            // 
            // lineViewToolStripMenuItem
            // 
            this.lineViewToolStripMenuItem.Name = "lineViewToolStripMenuItem";
            this.lineViewToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.lineViewToolStripMenuItem.Text = "Line View";
            this.lineViewToolStripMenuItem.Click += new System.EventHandler(this.lineViewToolStripMenuItem_Click);
            // 
            // saveImageAsToolStripMenuItem
            // 
            this.saveImageAsToolStripMenuItem.Name = "saveImageAsToolStripMenuItem";
            this.saveImageAsToolStripMenuItem.Size = new System.Drawing.Size(95, 20);
            this.saveImageAsToolStripMenuItem.Text = "Save Image As";
            this.saveImageAsToolStripMenuItem.Click += new System.EventHandler(this.saveImageAsToolStripMenuItem_Click);
            // 
            // DependencyGraphViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 357);
            this.Controls.Add(this.panel1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DependencyGraphViewer";
            this.Text = "DependencyGraphViewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copySentenceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cONNLFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dependencyTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem originalDependencyTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAndKeepPpDependencyTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapsedAndDropPpDependencyTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolStripFarsi;
        private System.Windows.Forms.ToolStripMenuItem ToolStripEnglish;
        private System.Windows.Forms.ToolStripMenuItem breakLoopsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolStripBreakLoopsTrue;
        private System.Windows.Forms.ToolStripMenuItem ToolStripBreakLoopsFalse;
        private System.Windows.Forms.ToolStripMenuItem treeViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem graphViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveImageAsToolStripMenuItem;
    }
}