﻿using IT.integro.DynamicsNAV.ProcessingTool.fileSplitter;
using IT.integro.DynamicsNAV.ProcessingTool.indentationChecker;
using IT.integro.DynamicsNAV.ProcessingTool.modificationSearchTool;
using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;
using IT.integro.DynamicsNAV.ProcessingTool.saveTool;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace IT.integro.DynamicsNAV.ProcessingTool
{
    public class ProcessFile
    {
        public static string RunProcessing(string expectedModifications, string inputFilePath, string mappingFilePath, bool highAccuracy, string documentationModifications) //highAccuracy = true -> find more tags, even bad ones
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);
            SaveTool.SetFullPermission(ref directory);

            string currProcess;
            CommonProgressBar progressBar = new CommonProgressBar("Full Processing", 10);
            progressBar.Show();

            List<string> expModifications = PrepareExpProcessing(expectedModifications);
            List<string> docModifications = PrepareDocProcessing(documentationModifications);

            currProcess = "Splitting file";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            FileSplitter.SplitFile(inputFilePath);

            currProcess = "Checking indentations";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            IndentationChecker.CheckIndentations();

            currProcess = "Searching for changes";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            if (!ModificationSearchTool.FindAndSaveChanges(expModifications))
                return "ERROR404";

            currProcess = "Cleaning code of changes";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            ModificationCleanerTool.CleanChangeCode();

            currProcess = "Updating documentation trigger";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            DocumentationTrigger.UpdateDocumentationTrigger(docModifications);

            currProcess = "Saving objects to files";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            SaveTool.SaveObjectsToFiles(outputPath);

            currProcess = "Saving changes to files";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            SaveTool.SaveChangesToFiles(outputPath, expModifications);

            currProcess = "Saving documentation file";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath, expModifications), expModifications, mappingFilePath);

            currProcess = "Saving objects of modification";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            SaveTool.SaveObjectModificationFiles(outputPath, expModifications);

            WatchStep();
            progressBar.Close();

            ChangeClassRepository.changeRepository.Clear();
            ObjectClassRepository.objectRepository.Clear();

            return outputPath;
        }

        public static string RunPreview(string expectedModifications, string inputFilePath, bool highAccuracy)
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);
            SaveTool.SetFullPermission(ref directory);

            string currProcess;
            CommonProgressBar progressBar = new CommonProgressBar("Preview Processing", 5);
            progressBar.Show();

            List<string> expModifications = PrepareExpProcessing(expectedModifications);

            SaveTool.SaveChangesToFiles(outputPath, expModifications);
            SaveTool.SaveObjectModificationFiles(outputPath, expModifications);
            
            currProcess = "Splitting file";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            FileSplitter.SplitFile(inputFilePath);

            reduceObjects(expModifications);

            currProcess = "Searching for changes";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            if (!ModificationSearchTool.FindAndSaveChanges(expModifications))
                return "ERROR404";

            currProcess = "Cleaning code of changes";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            ModificationCleanerTool.CleanChangeCode();

            currProcess = "Saving changes to files";
            WatchStep(currProcess);
            progressBar.PerformStep(currProcess);
            SaveTool.SaveChangesToFiles(outputPath, expModifications);

            WatchStep();
            progressBar.Close();

            ChangeClassRepository.changeRepository.Clear();
            ObjectClassRepository.objectRepository.Clear();
            return outputPath;
        }

        public static bool RunMergeProcess(string mergeString, string inputFilePath, string outputFilePath)
        {
            merge.MergeProgress mergeProcess = new merge.MergeProgress(mergeString, inputFilePath, outputFilePath);
            mergeProcess.ShowDialog();
            if (mergeProcess.DialogResult == System.Windows.Forms.DialogResult.OK)
                return true;
            else
                return false;
        }

        private static List<string> reduceObjects(List<string> expectedModifications)
        {
            List<string> objectsToSearch = new List<string>();

            foreach (var mod in expectedModifications)
            {
                string[] allText = TagRepository.GetModObjectList(mod).ToArray();
                objectsToSearch = objectsToSearch.Union(allText).ToList();
            }
            char[] separator = new char[] { ' ' };
            for (int i = 0; i < objectsToSearch.Count; i++)
            {
                objectsToSearch[i] = objectsToSearch[i].Split(separator, 4)[3].Replace(" ", string.Empty);
            }

            ObjectClassRepository.objectRepository = ObjectClassRepository.objectRepository.Where(o => objectsToSearch.Contains(o.Name)).ToList();
            return objectsToSearch;
        }
        
        public static List<string> PrepareExpProcessing(string expectedModifications)
        {
            if (expectedModifications == "")
                return new List<string>();
            return expectedModifications.Split(',').ToList();
        }

        public static List<string> PrepareDocProcessing(string documentationModifications)
        {
            if (documentationModifications == "")
                return new List<string>();
            return documentationModifications.Split(',').ToList();
        }

        public static string PassAllModificationTags(string inputPath, bool highAccuracy)
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            //return TagDetection.GetModificationString(inputPath);
            PassAllModificationProgress progress = new PassAllModificationProgress(inputPath);
            progress.ShowDialog();
            return progress.ReturnModificationString();
        }

        static System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        public static void WatchStep(string comment = "")
        {
            if (watch.IsRunning)
            {
                watch.Stop();
                Console.WriteLine(watch.Elapsed.TotalSeconds.ToString());
            }
            watch.Restart();
            Console.WriteLine(comment + ":");
        }
    }
}
