using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static bool SaveChangesToFiles(string path)
        {
            string modPath = path + "Modifications";
            DirectoryInfo directory = Directory.CreateDirectory(modPath);
            foreach (ChangeClass chg in ChangeClassRepository.changeRepository)
            {
                if (File.Exists(modPath + @"\Modification " + chg.ChangelogCode + " list.txt")) File.Delete(modPath + @"\Modification " + chg.ChangelogCode + " list.txt");
            }

            foreach (ChangeClass chg in ChangeClassRepository.changeRepository)
            {
                if (chg.ChangeType == "Code")
                {
                    File.AppendAllText(modPath + @"\Modification " + chg.ChangelogCode + " list.txt", "Source object: " + chg.SourceObject + Environment.NewLine);
                    File.AppendAllText(modPath + @"\Modification " + chg.ChangelogCode + " list.txt", "Change location: " + chg.Location + Environment.NewLine + Environment.NewLine);
                    File.AppendAllText(modPath + @"\Modification " + chg.ChangelogCode + " list.txt", chg.Contents);
                    File.AppendAllText(modPath + @"\Modification " + chg.ChangelogCode + " list.txt", Environment.NewLine + "----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
            return true;
        }

        private static string CleanFileName(string fileName)
        {
            string filepath = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            return filepath;
        }

        public static bool SaveObjectModificationFiles(string path)
        {
            string objModPath = path + "Modification Objects";
            DirectoryInfo directory = Directory.CreateDirectory(objModPath);

            foreach (ChangeClass chg in ChangeClassRepository.changeRepository)
            {
                if (File.Exists(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + " .txt")) File.Delete(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + " .txt");
            }

            foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
            {
                List<string> changeList = new List<string>();
                changeList = obj.Changelog.Select(o => o.ChangelogCode).Distinct().ToList();

                foreach (string change in changeList)
                {
                    File.AppendAllText(objModPath + @"\Objects modificated in " + CleanFileName(change) + " .txt", obj.Contents);
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
