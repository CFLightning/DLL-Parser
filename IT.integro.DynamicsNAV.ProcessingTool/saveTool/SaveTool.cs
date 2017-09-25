using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

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

        public static bool SaveChangesToFiles(string path, List<string> modifications)
        {
            string modPath = path + "Modifications";
            DirectoryInfo directory = Directory.CreateDirectory(modPath);

            // Directory full permissions for everyone
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var account = (NTAccount)sid.Translate(typeof(NTAccount));
            DirectorySecurity dSecurity = directory.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            directory.SetAccessControl(dSecurity);

            List<ChangeClass> changes = ChangeClassRepository.changeRepository.FindAll(x => modifications.Contains(x.ChangelogCode));

            foreach (ChangeClass chg in changes)
            {
                if (File.Exists(modPath + @"\Modification " + CleanFileName(chg.ChangelogCode) + " list.txt"))
                    File.Delete(modPath + @"\Modification " + CleanFileName(chg.ChangelogCode) + " list.txt");

                string detailPath = modPath + @"\Details\" + CleanFileName(chg.ChangelogCode);
                DirectoryInfo directoryDetail = Directory.CreateDirectory(detailPath);
                directoryDetail.SetAccessControl(dSecurity);

                if (File.Exists(detailPath + @"\" + chg.SourceObject + "#" + chg.Location + @".txt"))
                    File.Delete(detailPath + @"\" + chg.SourceObject + "#" + chg.Location + @".txt");

            }
            
            foreach (ChangeClass modChange in changes)
            {
                if (modChange.ChangeType == "Code")
                {
                    File.AppendAllText(modPath + @"\Modification " + CleanFileName(modChange.ChangelogCode) + " list.txt", "Source object: " + modChange.SourceObject + Environment.NewLine);
                    File.AppendAllText(modPath + @"\Modification " + CleanFileName(modChange.ChangelogCode) + " list.txt", "Change location: " + modChange.Location + Environment.NewLine + Environment.NewLine);
                    File.AppendAllText(modPath + @"\Modification " + CleanFileName(modChange.ChangelogCode) + " list.txt", modChange.Contents);
                    File.AppendAllText(modPath + @"\Modification " + CleanFileName(modChange.ChangelogCode) + " list.txt", Environment.NewLine + "----------------------------------------------------------------------------------------------------" + Environment.NewLine);

                    string detailPath = modPath + @"\Details\" + CleanFileName(modChange.ChangelogCode);

                    File.AppendAllText(detailPath + @"\" + CleanFileName(modChange.SourceObject) + "#" + CleanFileName(modChange.Location) + @".txt", modChange.Contents);
                    File.AppendAllText(detailPath + @"\" + CleanFileName(modChange.SourceObject) + "#" + CleanFileName(modChange.Location) + @".txt", Environment.NewLine + "----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
            return true;
        }

        private static string CleanFileName(string fileName)
        {
            string filepath = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            return filepath;
        }

        public static bool SaveObjectModificationFiles(string path, List<string> modifications)
        {
            string objModPath = path + "Modification Objects";
            DirectoryInfo directory = Directory.CreateDirectory(objModPath);

            List<ChangeClass> changes = ChangeClassRepository.changeRepository.FindAll(x => modifications.Contains(x.ChangelogCode));

            foreach (ChangeClass chg in changes)
            {
                {
                    if (File.Exists(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + " .txt"))
                        File.Delete(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + " .txt");
                }
            }

            //foreach (string modification in modifications)
            foreach (ChangeClass chg in changes)
            {
                foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
                {
                    if(obj.changelog.GroupBy(x => x.ChangelogCode).Select(grp => grp.First()).ToList().Contains(chg))
                    {
                        File.AppendAllText(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + " .txt", obj.Contents);
                    }
                }
            }
            return true;
        }

        public static bool SaveDocumentationToFile(string path, string documentation, List<string> expectedModifications, string mappingPath)
        {
            Dictionary<string, string> mappingDictionary = new Dictionary<string, string>();
            if (File.Exists(mappingPath))
            {
                var dictionaryLines = File.ReadLines(mappingPath);
                mappingDictionary = dictionaryLines.Select(line => line.Split(';')).ToDictionary(data => data[0], data => data[1]);
            }

            string docPath = path + "Documentation";
            DirectoryInfo directory = Directory.CreateDirectory(docPath);
            File.WriteAllText(docPath + @"\Documentation.txt", documentation);

            docPath = docPath + @"\Modification Documentation";
            directory = Directory.CreateDirectory(docPath);

            foreach (string modification in expectedModifications)
            {
                {
                    if (File.Exists(docPath + @"\" + modification + " documentation file.txt"))
                        File.Delete(docPath + @"\" + modification + " documentation file.txt");
                }
            }
       
            foreach (string modification in expectedModifications)
            {
                string line;
                int lineAmount = 1;
                StringReader reader = new StringReader(documentation);
                while (null != (line = reader.ReadLine()))
                {
                    if (line.Contains("#" + modification + "#") || (mappingDictionary.ContainsKey(modification) && line.Contains("#" + mappingDictionary[modification] + "#")))
                    {
                        File.AppendAllText(docPath + @"\" + CleanFileName(modification) + " documentation file.txt", lineAmount + line.Substring(line.IndexOf("<next>")) + Environment.NewLine);
                        lineAmount++;
                    }
                }
                reader.Close();
            }
            return true;
        }
    }
}