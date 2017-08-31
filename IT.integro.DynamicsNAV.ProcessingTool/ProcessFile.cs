using IT.integro.DynamicsNAV.ProcessingTool.fileSplitter;
using IT.integro.DynamicsNAV.ProcessingTool.indentationChecker;
using IT.integro.DynamicsNAV.ProcessingTool.modificationSearchTool;
using IT.integro.DynamicsNAV.ProcessingTool.saveTool;
using System;
using System.IO;

namespace IT.integro.DynamicsNAV.ProcessingTool
{
    public class ProcessFile
    {
        public static string RunProcessing(string expectedModification, string inputFilePath, string mappingFilePath)
        {
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);

            FileSplitter.SplitFile(inputFilePath);
            IndentationChecker.CheckIndentations();
            //ModificationSearchTool.FindAndSaveChanges(expectedModification);
            if (!ModificationSearchTool.FindAndSaveChanges(expectedModification))
            {
                return "ERROR404";
            }
            ModificationCleanerTool.CleanChangeCode();
            DocumentationTrigger.UpdateDocumentationTrigger(expectedModification);
            //SaveTool.SaveObjectsToFiles(outputPath);
            SaveTool.SaveChangesToFiles(outputPath, expectedModification);
            SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath, expectedModification));
            SaveTool.SaveObjectModificationFiles(outputPath, expectedModification);

            return outputPath;
        }
    }
}
