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
        public static string RunProcessing(string expectedModifications, string inputFilePath, string mappingFilePath, bool highAccuracy, string documentationModifications) //highAccuracy = true -> find more tags, even bad ones
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
            DirectoryInfo directory = Directory.CreateDirectory(outputPath);

            List<string> expModifications = PrepareExpProcessing(expectedModifications);
            List<string> docModifications = PrepareDocProcessing(documentationModifications);

            FileSplitter.SplitFile(inputFilePath);
            IndentationChecker.CheckIndentations();
            
            if (!ModificationSearchTool.FindAndSaveChanges(expModifications))
            {
                return "ERROR404";
            }
            ModificationCleanerTool.CleanChangeCode();
            DocumentationTrigger.UpdateDocumentationTrigger(docModifications);
            //SaveTool.SaveObjectsToFiles(outputPath);
            SaveTool.SaveChangesToFiles(outputPath, expModifications);
            SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath, expModifications), expModifications, mappingFilePath);
            SaveTool.SaveObjectModificationFiles(outputPath, expModifications);

            ChangeClassRepository.changeRepository.Clear();
            ObjectClassRepository.objectRepository.Clear();

            return outputPath;
        }

    public static string RunPreview(string expectedModifications, string inputFilePath, bool highAccuracy)
    {
        if (highAccuracy) TagDetection.SetHighAccuracy();
        string outputPath = Path.GetTempPath() + @"NAVCommentTool\";
        DirectoryInfo directory = Directory.CreateDirectory(outputPath);

        List<string> expModifications = PrepareExpProcessing(expectedModifications);
        FileSplitter.SplitFile(inputFilePath);
        //IndentationChecker.CheckIndentations();
        reduceObjects(expModifications);
        if (!ModificationSearchTool.FindAndSaveChanges(expModifications))
            return "ERROR404";
        ModificationCleanerTool.CleanChangeCode();
        //DocumentationTrigger.UpdateDocumentationTrigger();
        SaveTool.SaveChangesToFiles(outputPath, expModifications);
        //SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath, expModifications), expModifications, mappingFilePath);
        SaveTool.SaveObjectModificationFiles(outputPath, expModifications);
        ChangeClassRepository.changeRepository.Clear();
        ObjectClassRepository.objectRepository.Clear();
        return outputPath;
    }

        private static List<string> reduceObjects(List<string> expectedModifications)
        {
            List<string> objectsToSearch = new List<string>();
            string modObjPath = Path.GetTempPath() + @"NAVCommentTool\Modification Objects List\";

            foreach (var mod in expectedModifications)
            {
                string modFileName = string.Join("_", mod.Split(Path.GetInvalidFileNameChars()));
                string modFilePath = modObjPath + modFileName + ".txt";
                //.Replace(" ", string.Empty)
                string allText = File.ReadAllText(modFilePath).Replace(" ", string.Empty);
                objectsToSearch = objectsToSearch.Union(allText.Split('\n')).ToList();
            }

            ObjectClassRepository.objectRepository = ObjectClassRepository.objectRepository.Where(o => objectsToSearch.Contains(o.Name)).ToList();
            return objectsToSearch;
        }

        public static List<string> PrepareExpProcessing(string expectedModifications)
        {
            return expectedModifications.Split(',').ToList();
        }

        public static List<string> PrepareDocProcessing(string documentationModifications)
        {
            return documentationModifications.Split(',').ToList();
        }

        public static string PassAllModificationTags(string inputPath, bool highAccuracy)
        {
            if (highAccuracy) TagDetection.SetHighAccuracy();
            return TagDetection.GetModificationString(File.ReadAllText(inputPath));
        }
    }
}
