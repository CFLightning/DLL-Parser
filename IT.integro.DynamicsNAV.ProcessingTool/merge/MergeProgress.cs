using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;

namespace IT.integro.DynamicsNAV.ProcessingTool.merge
{
    public partial class MergeProgress : Form
    {
        public struct Merge
        {
            public string fromMod;
            public string toMod;
        }

        string mergeString;
        string inputFilePath;
        string outputFilePath;
        List<Merge> mergePairList;
        List<TagRepository.Tags> tempMergeTagList;

        public MergeProgress(string mergeString, string inputFilePath, string outputFilePath)
        {
            InitializeComponent();
            this.mergeString = mergeString;
            this.inputFilePath = inputFilePath ;
            this.outputFilePath = outputFilePath;
            this.mergePairList = GetMergePairList(mergeString);
            this.tempMergeTagList = GetTagsToReplace(mergePairList);
        }

        private List<Merge> GetMergePairList(string mergeString)
        {
            List<string> mergeStringList = mergeString.Split(new string[] { "|#|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<Merge> mergePairList = new List<Merge>();

            foreach (var item in mergeStringList)
            {
                Merge tempMerge;
                tempMerge.fromMod = item.Split(new string[] { "|>|" }, StringSplitOptions.RemoveEmptyEntries)[0];
                tempMerge.toMod = item.Split(new string[] { "|>|" }, StringSplitOptions.RemoveEmptyEntries)[1];
                mergePairList.Add(tempMerge);
            }
            return mergePairList;
        }
        
        private List<TagRepository.Tags> GetTagsToReplace(List<Merge> mergePairList)
        {
            // Create list of all lines to edit ordered by line no
            List<TagRepository.Tags> tempMergeTagList = new List<TagRepository.Tags>();
            foreach (var item in mergePairList)
            {
                tempMergeTagList = tempMergeTagList.Union(TagRepository.fullTagList.Where(w => w.mod == item.fromMod)).OrderBy(o => o.inLine).ToList();
            }
            return tempMergeTagList;
        }

        //private void MergeAndSave(string inputFileName, string outputFileName, string mergeString)
        //{
        //    System.IO.StreamReader reader = new System.IO.StreamReader(inputFileName);
        //    System.IO.StreamWriter writer = new System.IO.StreamWriter(outputFileName);

        //    string line;
        //    int lineNumber = 1;
        //    int tagNumber = 0;

        //    // Edit lines from list
        //    while ((line = reader.ReadLine()) != null)
        //    {
        //        if (tagNumber < tempMergeTagList.Count() && tempMergeTagList[tagNumber].inLine == lineNumber)
        //        {
        //            Merge merge = mergePairList.Find(mp => mp.fromMod == tempMergeTagList[tagNumber].mod);
        //            line = line.Replace(merge.fromMod, merge.toMod);
        //            tagNumber++;
        //        }
        //        writer.WriteLine(line);
        //        lineNumber++;
        //    }

        //    reader.Close();
        //    writer.Close();
        //}

        private void MergeProgress_Load(object sender, EventArgs e)
        {
            progressBar1.Maximum = tempMergeTagList.Count();
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            //this.Text = e.ProgressPercentage.ToString();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(inputFilePath);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(outputFilePath);

            string line;
            int lineNumber = 1;
            int tagNumber = 0;

            // Edit lines from list
            while ((line = reader.ReadLine()) != null)
            {
                if (tagNumber < tempMergeTagList.Count() && tempMergeTagList[tagNumber].inLine == lineNumber)
                {
                    Merge merge = mergePairList.Find(mp => mp.fromMod == tempMergeTagList[tagNumber].mod);
                    line = line.Replace(merge.fromMod, merge.toMod);
                    tagNumber++;
                    backgroundWorker.ReportProgress(tagNumber);
                }
                writer.WriteLine(line);
                lineNumber++;

            }

            reader.Close();
            writer.Close();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }
    }
}
