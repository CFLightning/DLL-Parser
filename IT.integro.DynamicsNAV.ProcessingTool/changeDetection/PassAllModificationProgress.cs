using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IT.integro.DynamicsNAV.ProcessingTool.changeDetection
{
    public partial class PassAllModificationProgress : Form
    {
        string inputFilePath;
        string outputModificationString;
        int buffsize;

        public PassAllModificationProgress(string inputFilePath, int buffsize)
        {
            InitializeComponent();
            this.inputFilePath = inputFilePath;
            this.buffsize = buffsize;
        }

        private void PassAllModificationProgress_Load(object sender, EventArgs e)
        {
            //  progress bar ass bytes read
            FileInfo fileInfo = new FileInfo(inputFilePath);
            progressBar.Maximum = (int)fileInfo.Length;
            backgroundWorker.RunWorkerAsync();
            timer1.Start();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TagRepository.ClearRepo(true);
            TagRepository.DeleteFiles();

            StreamReader inputfile = new StreamReader(inputFilePath, Encoding.GetEncoding("ISO-8859-1"));
            
            List<string> mods = new List<string>();
            List<string> tags = new List<string>();

            string[] codeLine = new string[buffsize];
            int i = 0;
            int bytes = 0;

            while ((codeLine[i] = inputfile.ReadLine()) != null)
            {
                bytes += System.Text.ASCIIEncoding.ASCII.GetByteCount(codeLine[i]);
                if (++i == buffsize)
                {
                    TagDetection.FindTagsToRepo(codeLine);
                    backgroundWorker.ReportProgress(bytes);
                    i = 0;
                    bytes = 0;
                    Array.Clear(codeLine, 0, codeLine.Count());
                }
            }
            string[] lastCodeLine = codeLine.Where(cl => cl != null).ToArray();
            if (lastCodeLine.Count() != 0)
            {
                TagDetection.FindTagsToRepo(lastCodeLine);
                backgroundWorker.ReportProgress(bytes);
            }
            inputfile.Close();

            mods = TagRepository.GetAllModList().Distinct().ToList();

            TagRepository.SaveToFilesFull();

            outputModificationString = string.Join(",", mods.ToArray());
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar.Value + e.ProgressPercentage < progressBar.Maximum)
                progressBar.Value += e.ProgressPercentage;
            labelObject.Text = TagRepository.tagObject;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer1.Stop();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string ReturnModificationString()
        {
            return outputModificationString;
        }

        int minutes;
        int seconds;

        private void timer1_Tick(object sender, EventArgs e)
        {
            seconds++;
            if (seconds % 60 == 0)
            {
                seconds = 0;
                minutes++;
            }
            textBox1.Text = minutes.ToString().PadLeft(2, '0') + ":" + seconds.ToString().PadLeft(2, '0');
        }
    }
}
