using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.IO;
using System.Linq;

namespace IT.integro.DynamicsNAV.ProcessingTool.saveTool
{
    public class SaveTool
    {
        public static bool SaveObjectsToFiles(string path)
        {
            string objPath = path + "Objects";
            DirectoryInfo directory = Directory.CreateDirectory(objPath);
            foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
            {
                File.WriteAllText(path + @"\Objects\" + obj.Type + " " + obj.Number + " " + obj.Name + " .txt", obj.Contents);
            }
            return true;
        }

        public static bool SaveChangesToFiles(string path, string modification)
        {
            string modPath = path + "Modifications";
            DirectoryInfo directory = Directory.CreateDirectory(modPath);
            ChangeClass chg = ChangeClassRepository.changeRepository.Find(x => x.ChangelogCode == modification);
            {
                if (File.Exists(modPath + @"\Modification " + chg.ChangelogCode + " list.txt"))
                    File.Delete(modPath + @"\Modification " + chg.ChangelogCode + " list.txt");
            }

            foreach (ChangeClass modChange in ChangeClassRepository.changeRepository.Where(o => o.ChangelogCode == modification))
            {
                if (modChange.ChangeType == "Code")
                {
                    File.AppendAllText(modPath + @"\Modification " + modChange.ChangelogCode + " list.txt", "Source object: " + modChange.SourceObject + Environment.NewLine);
                    File.AppendAllText(modPath + @"\Modification " + modChange.ChangelogCode + " list.txt", "Change location: " + modChange.Location + Environment.NewLine + Environment.NewLine);
                    File.AppendAllText(modPath + @"\Modification " + modChange.ChangelogCode + " list.txt", modChange.Contents);
                    File.AppendAllText(modPath + @"\Modification " + modChange.ChangelogCode + " list.txt", Environment.NewLine + "----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
            return true;
        }

        private static string CleanFileName(string fileName)
        {
            string filepath = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            return filepath;
        }

        public static bool SaveObjectModificationFiles(string path, string modification)
        {
            string objModPath = path + "Modification Objects";
            DirectoryInfo directory = Directory.CreateDirectory(objModPath);

            ChangeClass chg = ChangeClassRepository.changeRepository.Find(x => x.ChangelogCode == modification);
            {
                if (File.Exists(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + " .txt"))
                    File.Delete(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + " .txt");
            }

            foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
            {
                {
                    File.AppendAllText(objModPath + @"\Objects modificated in " + CleanFileName(modification) + " .txt", obj.Contents);
                }
            }
            return true;
        }

        public static bool SaveDocumentationToFile(string path, string documentation)
        {
            string docPath = path + "Documentation";
            DirectoryInfo directory = Directory.CreateDirectory(docPath);
            File.WriteAllText(docPath + @"\Documentation.txt", documentation);
            return true;
        }
    }
}