namespace IT.integro.DynamicsNAV.ProcessingTool.merge
{
    partial class MergeProgress
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
            this.progressBarPreprocess = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerPreprocess = new System.ComponentModel.BackgroundWorker();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBarPreprocess
            // 
            this.progressBarPreprocess.Location = new System.Drawing.Point(12, 25);
            this.progressBarPreprocess.Name = "progressBarPreprocess";
            this.progressBarPreprocess.Size = new System.Drawing.Size(487, 23);
            this.progressBarPreprocess.TabIndex = 0;
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // backgroundWorkerPreprocess
            // 
            this.backgroundWorkerPreprocess.WorkerReportsProgress = true;
            this.backgroundWorkerPreprocess.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerPreprocess_DoWork);
            this.backgroundWorkerPreprocess.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerPreprocess_ProgressChanged);
            this.backgroundWorkerPreprocess.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerPreprocess_RunWorkerCompleted);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 67);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(487, 23);
            this.progressBar.TabIndex = 1;
            // 
            // MergeProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 112);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.progressBarPreprocess);
            this.Name = "MergeProgress";
            this.Text = "MergeProgress";
            this.Load += new System.EventHandler(this.MergeProgress_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarPreprocess;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.ComponentModel.BackgroundWorker backgroundWorkerPreprocess;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}