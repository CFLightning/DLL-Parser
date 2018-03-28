using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;
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
                File.WriteAllText(path + @"\Objects\" + obj.Type + " " + obj.Number + " " + obj.Name + ".txt", obj.Contents, System.Text.Encoding.Default);
            }
            return true;
        }

        public static bool SaveChangesToFiles(string path, List<string> modifications)
        {
            List<ChangeClass> changes = ChangeClassRepository.changeRepository.FindAll(x => modifications.Contains(x.ChangelogCode));
            string modPath = path + "Modifications";
            string detailPath = modPath + @"\Details\";

            if (Directory.Exists(modPath))
                Directory.Delete(modPath, true);
            DirectoryInfo directory = Directory.CreateDirectory(modPath);
            SetFullPermission(ref directory);

            foreach (var mod in modifications)
            {
                File.Create(modPath + @"\Modification " + CleanFileName(mod) + " list.txt").Dispose();

                // Details
                DirectoryInfo directoryDetail = Directory.CreateDirectory(detailPath + CleanFileName(mod));
                SetFullPermission(ref directoryDetail);
            }

            string separatorLine = "----------------------------------------------------------------------------------------------------";
            foreach (ChangeClass modChange in changes)
            {
                string fileName = modPath + @"\Modification " + CleanFileName(modChange.ChangelogCode) + " list.txt";
                string detailFileName = detailPath + CleanFileName(modChange.ChangelogCode) + "//" + CleanFileName(modChange.SourceObject) + "#" + CleanFileName(modChange.Location) + @".txt";

                if (modChange.ChangeType == "Code" || modChange.ChangeType == "Field")
                {
                    File.AppendAllText(fileName, "Source object: " + modChange.SourceObject + Environment.NewLine, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, "Change location: " + modChange.Location + Environment.NewLine + Environment.NewLine, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, modChange.Contents, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, Environment.NewLine + separatorLine + Environment.NewLine, System.Text.Encoding.Default);

                    // Details
                    File.AppendAllText(detailFileName, modChange.Contents, System.Text.Encoding.Default);
                    File.AppendAllText(detailFileName, Environment.NewLine + separatorLine + Environment.NewLine, System.Text.Encoding.Default);
                }
                else if (modChange.ChangeType == "Column")
                {
                    File.AppendAllText(fileName, "Source object: " + modChange.SourceObject + Environment.NewLine, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, "Change location: " + modChange.Location + Environment.NewLine + Environment.NewLine, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, "New " + modChange.ChangeType + " : " + modChange.Contents, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, Environment.NewLine + separatorLine + Environment.NewLine, System.Text.Encoding.Default);
                }
                else if (modChange.ChangeType == "NewObj")
                {
                    File.AppendAllText(fileName, "Source object: " + modChange.SourceObject + Environment.NewLine, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, "Change location: " + modChange.Location + Environment.NewLine + Environment.NewLine, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, modChange.Contents, System.Text.Encoding.Default);
                    File.AppendAllText(fileName, Environment.NewLine + separatorLine + Environment.NewLine, System.Text.Encoding.Default);
                }
            }
            return true;
        }

        private static string CleanFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }

        public static bool SaveObjectModificationFiles(string path, List<string> modifications)
        {
            string objModPath = path + "Modification Objects";
            DirectoryInfo directory = Directory.CreateDirectory(objModPath);

            List<ChangeClass> changes = ChangeClassRepository.changeRepository.FindAll(x => modifications.Contains(x.ChangelogCode));

            foreach (ChangeClass chg in changes)
            {
                {
                    if (File.Exists(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + ".txt"))
                        File.Delete(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + ".txt");
                }
            }

            //foreach (string modification in modifications)
            foreach (ChangeClass chg in changes)
            {
                foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
                {
                    if (obj.changelog.GroupBy(x => x.ChangelogCode).Select(grp => grp.First()).ToList().Contains(chg))
                    {
                        File.AppendAllText(objModPath + @"\Objects modificated in " + CleanFileName(chg.ChangelogCode) + ".txt", obj.Contents, System.Text.Encoding.Default);
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
            File.WriteAllText(docPath + @"\Documentation.txt", documentation, System.Text.Encoding.Default);

            docPath = docPath + @"\Modification Documentation";
            directory = Directory.CreateDirectory(docPath);

            foreach (string modification in expectedModifications)
            {
                if (File.Exists(docPath + @"\" + CleanFileName(modification) + " documentation file.txt"))
                    File.Delete(docPath + @"\" + CleanFileName(modification) + " documentation file.txt");
                File.Create(docPath + @"\" + CleanFileName(modification) + " documentation file.txt").Dispose();
            }

            foreach (string modification in expectedModifications)
            {
                string line;
                int lineAmount = 1;
                StringReader reader = new StringReader(documentation);
                while (null != (line = reader.ReadLine()))
                {
                    if ((line.Contains("#" + modification + "#")) || (mappingDictionary.ContainsKey(modification) && line.Contains("#" + mappingDictionary[modification] + "#")))
                    {
                        if (mappingDictionary.ContainsKey(modification) && line.Contains("#" + mappingDictionary[modification] + "#"))
                        {
                            line = line.Replace("#" + mappingDictionary[modification] + "#", "#" + modification + "#");
                        }
                        File.AppendAllText(docPath + @"\" + CleanFileName(modification) + " documentation file.txt", lineAmount + line.Substring(line.IndexOf("<next>")) + Environment.NewLine, System.Text.Encoding.Default);
                        lineAmount++;
                    }
                }
                reader.Close();
            }
            return true;
        }

        public static void SetFullPermission(ref DirectoryInfo directory)
        {
            // Directory full permissions for everyone
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var account = (NTAccount)sid.Translate(typeof(NTAccount));
            DirectorySecurity dSecurity = directory.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            directory.SetAccessControl(dSecurity);
        }
    }
}