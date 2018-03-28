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
using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;
using System.Text.RegularExpressions;

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
            mergePairList = GetMergePairList(mergeString);
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

        private void MergeProgress_Load(object sender, EventArgs e)
        {
            progressBarPreprocess.Maximum = mergePairList.Count();
            backgroundWorkerPreprocess.RunWorkerAsync();
            timer1.Start();
            labelProcess.Text = "Preprocess";
        }

        private void backgroundWorkerPreprocess_DoWork(object sender, DoWorkEventArgs e)
        {
            // Create list of all lines to edit ordered by line no
            tempMergeTagList = new List<TagRepository.Tags>();
            for (int iMerge = 0; iMerge < mergePairList.Count(); iMerge++)
            {
                tempMergeTagList = tempMergeTagList.Union(TagRepository.fullTagList.Where(w => w.mod == mergePairList[iMerge].fromMod)).OrderBy(o => o.inLine).ToList();
                backgroundWorkerPreprocess.ReportProgress(iMerge);
            }
        }

        private void backgroundWorkerPreprocess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarPreprocess.Value = e.ProgressPercentage + 1;
        }

        private void backgroundWorkerPreprocess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Maximum = tempMergeTagList.Count();
            backgroundWorker.RunWorkerAsync();
            labelProcess.Text = "Merging";
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Encoding fileEncoding = GetFileEncoding(inputFilePath);
            System.IO.StreamReader reader = new System.IO.StreamReader(inputFilePath, fileEncoding);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(outputFilePath, false, fileEncoding);

            string line;
            int lineNumber = 0;
            int tagNumber = 0;


            int begins = 0;
            bool currLineBlank = false;
            bool prevLineBlank = false;
            bool flagDocumentation = false;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                // FIND DOCUMENTATION TRIGGERS
                prevLineBlank = currLineBlank;
                currLineBlank = Regex.IsMatch(line, "^$");
                if (!flagDocumentation)
                {
                    if (Regex.IsMatch(line, "^    BEGIN$"))
                    {
                        if (prevLineBlank && begins == 0)
                        {
                            flagDocumentation = true;
                            writer.WriteLine(line);
                            continue;
                        }
                        begins++;
                    }
                    else if (Regex.IsMatch(line, "^    END;$"))
                    {
                        begins--;
                    }
                }
                else // IN DOCUMENTATION TRIGGER
                {
                    if (Regex.IsMatch(line, "^    END.$"))
                    {
                        flagDocumentation = false;
                        writer.WriteLine(line);
                        continue;
                    }
                    foreach (var mergePair in mergePairList.Where(mp=>line.Contains(mp.fromMod)))
                    {
                        line = line.Replace(mergePair.fromMod, mergePair.toMod);
                    }
                    writer.WriteLine(line);
                    continue;
                }

                // EDITS LINES FROM LIST IN REPOSITORY OF TAGS
                if (tagNumber < tempMergeTagList.Count() && tempMergeTagList[tagNumber].inLine == lineNumber)
                {
                    if(line.StartsWith("    Version List="))
                    {
                        if(TagDetection.CheckIfTagInVersionList(line, tempMergeTagList[tagNumber].mod))
                        {
                            Merge merge = mergePairList.Find(mp => mp.fromMod == tempMergeTagList[tagNumber].mod);
                            //line = line.Replace(merge.fromMod, merge.toMod);
                            line = TagDetection.ReplaceTagInVersionList(line, merge.fromMod, merge.toMod);
                            tagNumber++;
                        }
                    }
                    else if (tempMergeTagList[tagNumber].isCodeOrField == true)  //  CODE
                    {                                                 
                        Merge merge = mergePairList.Find(mp => mp.fromMod == tempMergeTagList[tagNumber].mod);
                        line = line.Replace(merge.fromMod, merge.toMod);      // Replace May be problem if tag and part of code in line is the same
                        tagNumber++;
                    }
                    else    // FIELD DESCRIPTION
                    {
                        do
                        {
                            List<string> descMods = TagDetection.GetLineDescriptionTagList(line);
                            Merge merge = mergePairList.Find(mp => mp.fromMod == tempMergeTagList[tagNumber].mod);
                            string newDescription = string.Empty;
                            for (int i = 0; i < descMods.Count; i++)
                            {
                                if (descMods[i] == tempMergeTagList[tagNumber].mod)
                                {
                                    newDescription += descMods[i].Replace(merge.fromMod, merge.toMod) + ", ";
                                }
                                else
                                {
                                    newDescription += descMods[i] + ", ";
                                }
                            }
                            newDescription = newDescription.Substring(0, newDescription.Length - 2);
                            line = line.Replace(FlagDetection.GetDescription(line), newDescription);
                            
                        } while (tagNumber+1 < tempMergeTagList.Count() && tempMergeTagList[tagNumber].inLine == tempMergeTagList[++tagNumber].inLine);
                    }
                    backgroundWorker.ReportProgress(tagNumber);
                }
                writer.WriteLine(line);
            }

            reader.Close();
            writer.Close();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
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

        /// <summary>
        /// Detects the byte order mark of a file and returns
        /// an appropriate encoding for the file.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(string srcFile)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];
            System.IO.FileStream file = new System.IO.FileStream(srcFile, System.IO.FileMode.Open);
            file.Read(buffer, 0, 5);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;
            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;

            return enc;

        }
    }
}
