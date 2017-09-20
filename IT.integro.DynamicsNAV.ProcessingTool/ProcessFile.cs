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
        public static string RunProcessing(string expectedModifications, string inputFilePath, string mappingFilePath)
        {
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);

            List<string> expModifications = PrepareProcessing(expectedModifications);

            FileSplitter.SplitFile(inputFilePath);
            IndentationChecker.CheckIndentations();
            if (!ModificationSearchTool.FindAndSaveChanges(expModifications))
            {
                return "ERROR404";
            }
            ModificationCleanerTool.CleanChangeCode();
            DocumentationTrigger.UpdateDocumentationTrigger();
            SaveTool.SaveChangesToFiles(outputPath, expModifications);
            SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath, expModifications), expModifications, mappingFilePath);
            SaveTool.SaveObjectModificationFiles(outputPath, expModifications);

            ChangeClassRepository.changeRepository.Clear();
            ObjectClassRepository.objectRepository.Clear();

            return outputPath;
        }

        public static List<string> PrepareProcessing(string expectedModifications)
        {
            return expectedModifications.Split(',').ToList();
        }

        public static string PassAllModificationTags(string inputPath)
        {
            return TagDetection.GetModificationString(File.ReadAllText(inputPath));
        }
    }
}
