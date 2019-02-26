using System.Collections.Generic;
using System.Linq;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;
using System.IO;
using System;

namespace IT.integro.DynamicsNAV.ProcessingTool.fileSplitter
{
    public struct Merge
    {
        public string fromMod;
        public string toMod;
    }

    public class MergeTool
    {
        Merge merge;
        List<TagRepository.Tags> mergeTagList;
        static string[] fileLines;
        static string inputFilePath;

        public MergeTool(string path)
        {
            inputFilePath = path;
            fileLines = System.IO.File.ReadAllLines(inputFilePath);
        }

        //public MergeTool(string path, string mergeString)
        //{
        //    inputFilePath = path;
        //    fileLines = System.IO.File.ReadAllLines(inputFilePath);
        //    mergePairList = GetMergePairList(mergeString);
        //}

        static public List<Merge> GetMergePairList(string mergeString)
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

        public void FindTagsToMerge(Merge mergeParameter)
        {
            merge = mergeParameter;
            mergeTagList = TagRepository.fullTagList.Where(w => w.mod == mergeParameter.fromMod).ToList();
        }

        public bool Merge()
        {
            foreach (var tag in mergeTagList)
            {
                if (!ReplaceTag(tag))
                    return false;
            }
            return true;
        }

        private bool ReplaceTag(TagRepository.Tags tag)
        {
            string oldLine = fileLines[tag.inLine - 1];
            fileLines[tag.inLine - 1] = fileLines[tag.inLine - 1].Replace(merge.fromMod, merge.toMod);
            if (TagDetection.GetTagedModification(fileLines[tag.inLine - 1]) != merge.toMod)
            {
                string text = "Line:\t" + oldLine.Trim(' ') + "\nTag:\t" + TagDetection.GetTagedModification(oldLine) + "\n\nReplaced by\nLine:\t" + fileLines[tag.inLine - 1].Trim(' ') + "\nTag:\t" + TagDetection.GetTagedModification(fileLines[tag.inLine - 1]) + "\n\nObject:\t" + tag.inObject;
                System.Windows.Forms.MessageBox.Show(text, "Possibly error during the merge");
                //return false;
            }
            return true;
        }

        static private string SetOutputPath(string outputFileName)
        {
            string directoryPath = System.IO.Path.GetDirectoryName(inputFilePath) + "\\";
            if (outputFileName == "")
                outputFileName = System.IO.Path.GetFileNameWithoutExtension(inputFilePath) + " (Merged).txt";
            if (!outputFileName.EndsWith(".txt"))
                outputFileName += ".txt";
            return directoryPath + outputFileName;
            
        }

        static public void MergeAndSave(string inputFileName, string outputFileName, string mergeString)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(inputFileName);
            //string outputFilePath = SetOutputPath(outputFileName);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(outputFileName);
            
            List<Merge> mergePairList = GetMergePairList(mergeString);
            string line;
            int lineNumber = 1;
            int tagNumber = 0;

            // Create list of all lines to edit ordered by line no
            List<TagRepository.Tags> tempMergeTagList = new List<TagRepository.Tags>();
            foreach (var item in mergePairList)
            {
                tempMergeTagList = tempMergeTagList.Union(TagRepository.fullTagList.Where(w => w.mod == item.fromMod)).OrderBy(o => o.inLine).ToList();
            }

            // Edit lines from list
            while ((line = reader.ReadLine()) != null)
            {
                if (tagNumber < tempMergeTagList.Count() && tempMergeTagList[tagNumber].inLine == lineNumber)
                {
                    Merge merge = mergePairList.Find(mp => mp.fromMod == tempMergeTagList[tagNumber].mod);
                    line = line.Replace(merge.fromMod, merge.toMod);
                    tagNumber++;
                }
                writer.WriteLine(line);
                lineNumber++;
            }
            
            reader.Close();
            writer.Close();
        }

        public static void Fun(System.ComponentModel.BackgroundWorker backgroundWorker)
        {
            for (int i = 1; i <= 100; i++)
            {
                // Wait 100 milliseconds.
                System.Threading.Thread.Sleep(100);
                // Report progress.
                backgroundWorker.ReportProgress(i);
            }
        }

    }
}
