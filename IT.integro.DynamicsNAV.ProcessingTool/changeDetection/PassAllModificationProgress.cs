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

        public PassAllModificationProgress(string inputFilePath)
        {
            InitializeComponent();
            this.inputFilePath = inputFilePath;
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
            TagRepository.ClearRepo();
            TagRepository.DeleteFiles();

            StreamReader inputfile = new StreamReader(inputFilePath, Encoding.GetEncoding("ISO-8859-1"));

            string line;
            List<string> mods = new List<string>();
            List<string> tags = new List<string>();

            int buffsize = 10000;
            string[] codeLine = new string[buffsize];
            int i = 0;
            int bytes = 0;
            while ((line = inputfile.ReadLine()) != null)
            {
                codeLine[i++] = line;
                bytes += System.Text.ASCIIEncoding.ASCII.GetByteCount(line);
                if (i == buffsize)
                {
                    TagDetection.FindTagsToRepo(codeLine);
                    backgroundWorker.ReportProgress(bytes);
                    i = 0;
                    bytes = 0;
                    Array.Clear(codeLine, 0, codeLine.Count());
                }
            }
            TagDetection.FindTagsToRepo(codeLine.Where(cl => cl != null).ToArray());
            backgroundWorker.ReportProgress(bytes);
            inputfile.Close();

            mods = TagRepository.GetAllModList().Distinct().ToList();

            TagRepository.SaveToFilesFull();

            outputModificationString = string.Join(",", mods.ToArray());
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar.Value + e.ProgressPercentage < progressBar.Maximum)
                progressBar.Value += e.ProgressPercentage;
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
                minutes++;
            textBox1.Text = minutes.ToString().PadLeft(2,'0') + ":" + seconds.ToString().PadLeft(2, '0');
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
