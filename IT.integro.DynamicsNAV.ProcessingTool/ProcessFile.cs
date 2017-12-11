using IT.integro.DynamicsNAV.ProcessingTool.fileSplitter;
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
        public static string RunProcessingOld(string expectedModifications, string inputFilePath, string mappingFilePath, bool highAccuracy, string documentationModifications) //highAccuracy = true -> find more tags, even bad ones
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);
            SaveTool.SetFullPermission(ref directory);

            string currProcess;

            List<string> expModifications = PrepareExpProcessing(expectedModifications);
            List<string> docModifications = PrepareDocProcessing(documentationModifications);

            currProcess = "Splitting file";
            FileSplitter.SplitFile(inputFilePath);

            currProcess = "Checking indentations";
            IndentationChecker.CheckIndentations();

            currProcess = "Searching for changes";
            FileSplitter.ReduceObjects(expModifications);
            if (!ModificationSearchTool.FindAndSaveChanges(expModifications))
                return "ERROR404";

            currProcess = "Cleaning code of changes";
            ModificationCleanerTool.CleanChangeCode();

            currProcess = "Updating documentation trigger";
            DocumentationTrigger.UpdateDocumentationTrigger(docModifications);

            currProcess = "Saving objects to files";
            SaveTool.SaveObjectsToFiles(outputPath);

            currProcess = "Saving changes to files";
            SaveTool.SaveChangesToFiles(outputPath, expModifications);

            currProcess = "Saving documentation file";
            SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath, expModifications), expModifications, mappingFilePath);

            currProcess = "Saving objects of modification";
            SaveTool.SaveObjectModificationFiles(outputPath, expModifications);

            ChangeClassRepository.changeRepository.Clear();
            ObjectClassRepository.objectRepository.Clear();

            return outputPath;
        }

        public static string RunProcessing(string expectedModifications, string inputFilePath, string mappingFilePath, bool highAccuracy, string documentationModifications) //highAccuracy = true -> find more tags, even bad ones
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);
            SaveTool.SetFullPermission(ref directory);

            List<string> expModifications = PrepareExpProcessing(expectedModifications);
            List<string> docModifications = PrepareDocProcessing(documentationModifications);
            bool[] processesMap = { true, false, true, true, true, true, true, true, true, true };

            RunProcessingProgressBar progressBar = new RunProcessingProgressBar(processesMap, inputFilePath, outputPath, mappingFilePath, expModifications, docModifications);
            progressBar.ShowDialog();

            if (progressBar.DialogResult == System.Windows.Forms.DialogResult.OK)
                return outputPath;
            else
                return "Error";
        }

        public static string RunPreviewOld(string expectedModifications, string inputFilePath, bool highAccuracy)
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);
            SaveTool.SetFullPermission(ref directory);

            string currProcess;

            List<string> expModifications = PrepareExpProcessing(expectedModifications);

            SaveTool.SaveChangesToFiles(outputPath, expModifications);
            SaveTool.SaveObjectModificationFiles(outputPath, expModifications);
            
            currProcess = "Splitting file";
            FileSplitter.SplitFile(inputFilePath);

            currProcess = "Searching for changes";
            FileSplitter.ReduceObjects(expModifications);
            if (!ModificationSearchTool.FindAndSaveChanges(expModifications))
                return "ERROR404";

            currProcess = "Cleaning code of changes";
            ModificationCleanerTool.CleanChangeCode();

            currProcess = "Saving objects to files";
            SaveTool.SaveObjectsToFiles(outputPath);

            currProcess = "Saving changes to files";
            SaveTool.SaveChangesToFiles(outputPath, expModifications);
            
            ChangeClassRepository.changeRepository.Clear();
            ObjectClassRepository.objectRepository.Clear();
            return outputPath;
        }

        public static string RunPreview(string expectedModifications, string inputFilePath, bool highAccuracy) //highAccuracy = true -> find more tags, even bad ones
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);
            SaveTool.SetFullPermission(ref directory);

            List<string> expModifications = PrepareExpProcessing(expectedModifications);
            bool[] processesMap = { true, false, true, true, false, false, true, false, false, true };

            RunProcessingProgressBar progressBar = new RunProcessingProgressBar(processesMap, inputFilePath, outputPath, "", expModifications, new List<string>());
            progressBar.ShowDialog();

            if (progressBar.DialogResult == System.Windows.Forms.DialogResult.OK)
                return outputPath;
            else
                return "error";
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
      
        private static List<string> PrepareExpProcessing(string expectedModifications)
        {
            if (expectedModifications == "")
                return new List<string>();
            return expectedModifications.Split(',').ToList();
        }

        private static List<string> PrepareDocProcessing(string documentationModifications)
        {
            if (documentationModifications == "")
                return new List<string>();
            return documentationModifications.Split(',').ToList();
        }

        public static string PassAllModificationTagsProcess(string inputPath, bool highAccuracy)
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            PassAllModificationProgress progress = new PassAllModificationProgress(inputPath, 10000);
            progress.ShowDialog();
            return progress.ReturnModificationString();
        }

        //public static string PassAllModificationTags(string inputPath, bool highAccuracy)
        //{
        //    if (highAccuracy) TagDetection.SetHighAccuracy();
        //    return TagDetection.GetModificationString(inputPath);
        //}

        //public static string PassAllModificationTagsAfterMerge(string inputPath)
        //{
        //    return TagDetection.GetModificationStringUsingRepo(inputPath);
        //}

        public static bool CheckIfTagIsCorrect(string tag)
        {
            return TagDetection.tagNamePattern.IsMatch(tag) && !TagDetection.ContainsRestrictedWords(tag);
        }

        public static string GetRegexTagDefinition()
        {
            return TagDetection.tagNamePattern.ToString();
        }

        public static string GetTemporaryPath()
        {
            return Path.GetTempPath() + @"NAVCommentTool\";
        }

        public static string CleanFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
