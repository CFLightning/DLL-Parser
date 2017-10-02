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
using System.Security.Principal;
using System.Security.AccessControl;

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
            SaveTool.SetFullPermission(ref directory);

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

        public static bool MergeTags(string inputFilePath, string mergeString)
        {
            // MODfrom|>|MODto|#|MODfrom|>|MODto|#|...

            List<string> mergeList = mergeString.Split(new string[] { "|#|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            MergeTool mTool = new MergeTool(inputFilePath);
            foreach (var mergeItem in mergeList)
            {
                Merge merge;
                merge.fromMod = mergeItem.Split(new string[] { "|>|" }, StringSplitOptions.RemoveEmptyEntries)[0];
                merge.toMod = mergeItem.Split(new string[] { "|>|" }, StringSplitOptions.RemoveEmptyEntries)[1];
                mTool.FindTagsToMerge(merge);
                if (!mTool.Merge())
                {
                    Console.WriteLine("MERGE ERROR");
                    return false;
                }
            }
            MergeTool.SaveFile();
            return true;
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
            return TagDetection.GetModificationString(inputPath);
        }
    }
}
