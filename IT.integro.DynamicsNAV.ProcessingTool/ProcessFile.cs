using IT.integro.DynamicsNAV.ProcessingTool.fileSplitter;
using IT.integro.DynamicsNAV.ProcessingTool.indentationChecker;
using IT.integro.DynamicsNAV.ProcessingTool.modificationSearchTool;
using IT.integro.DynamicsNAV.ProcessingTool.saveTool;
using System;


namespace IT.integro.DynamicsNAV.ProcessingTool
{
    public class ProcessFile
    {
        public bool RunProcessing(string expectedModification, string inputFilePath, string mappingFilePath, string outputPath)
        {
            outputPath = outputPath + "\\";

            FileSplitter.SplitFile(inputFilePath);
            IndentationChecker.CheckIndentations();
            if (!ModificationSearchTool.FindAndSaveChanges(expectedModification))
            {
                Console.WriteLine("ERROR: Modofication {0} not found", expectedModification);
                Console.ReadLine();
                return false;
            }
            ModificationCleanerTool.CleanChangeCode();
            DocumentationTrigger.UpdateDocumentationTrigger();
            SaveTool.SaveObjectsToFiles(outputPath);
            SaveTool.SaveChangesToFiles(outputPath);
            SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath));
            SaveTool.SaveObjectModificationFiles(outputPath);

            return true;
        }
    }
}
