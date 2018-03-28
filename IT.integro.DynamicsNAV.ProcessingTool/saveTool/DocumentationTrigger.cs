using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;
using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IT.integro.DynamicsNAV.ProcessingTool.saveTool
{
    public class DocumentationTrigger
    {
        private static string UpdateVersionList(string codeLine, ObjectClass obj)
        {
            List<string> versionList = new List<string>();
            versionList = codeLine.Substring(codeLine.IndexOf('=') + 1).Split(',').ToList();
            versionList[versionList.Count - 1] = versionList.Last().Substring(0, versionList.Last().Length - 1);
            versionList = versionList.Union(TagDetection.GetModyficationList(obj.Contents, obj.Number)).ToList();

            string versionString = string.Join(",", versionList.ToArray());
            if (versionString.Length >= 248)
            {
                versionString = versionString.Remove(0, versionString.Length - (247));
                versionString = versionString.Remove(0, versionString.IndexOf(','));
                versionString = versionString.Insert(0, "~");
            }
            return "    Version List=" + versionString + ";";
        }

        public static bool UpdateDocumentationTrigger(List<string> docModifications)
        {
            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);
            string line;
            bool bracketFlag = false, beginFlag = false, writing = false, documentationPrompt = false, deleteFlag = false, addBrackets = false, bracketsExist = false;

            List<string> locationList = new List<string>();
            foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
            {
                List<string> ObjectModList = TagDetection.GetModyficationList(obj.Contents, obj.Number);
                StringReader reader = new StringReader(obj.Contents);
                bracketFlag = false; beginFlag = false; writing = false; documentationPrompt = false; deleteFlag = false; addBrackets = false; bracketsExist = false;

                while (null != (line = reader.ReadLine()) && line != "  PROPERTIES")
                {
                    if (line.Contains("Version List="))
                        line = UpdateVersionList(line, obj);
                    writer.WriteLine(line);
                }
                writer.WriteLine(line);

                // Ommit All sections before CODE
                while (null != (line = reader.ReadLine()))
                {
                    writer.WriteLine(line);
                    if (line == "  CODE")
                    {
                        break;
                    }
                }

                while (null != (line = reader.ReadLine())) 
                {
                    // Ommit detection if end of CODE section
                    if (line == "  RDLDATA")
                    {
                        writer.WriteLine(line);
                        while (null != (line = reader.ReadLine()))
                        {
                            writer.WriteLine(line);
                        }
                        break;
                    }

                    if (line.StartsWith("    BEGIN"))
                    {
                        beginFlag = true;
                    }

                    if (line.StartsWith("    {") && beginFlag)
                    {
                        bracketFlag = true;
                        bracketsExist = true;
                    }

                    if (line.StartsWith("      Automated Documentation"))
                    {
                        documentationPrompt = true;
                    }

                    if (line.StartsWith("    }") && bracketFlag)
                    {
                        bracketFlag = false;
                        deleteFlag = false;
                        writing = true;
                        bracketsExist = true;
                    }

                    if (line.StartsWith("    END") && beginFlag && bracketsExist)
                    {
                        beginFlag = false;
                    }

                    if (line.StartsWith("    END.") && beginFlag && !bracketsExist)
                    {
                        writing = true;
                        addBrackets = true;
                        deleteFlag = false;
                        beginFlag = false;
                    }

                    if (line.StartsWith("      #") && documentationPrompt)
                    {
                        deleteFlag = false;
                        foreach (string item in ObjectModList)
                        {
                            if (line.Contains(item)) deleteFlag = true;
                        }
                    }

                    if (writing)
                    {
                        if (addBrackets)
                        {
                            writer.WriteLine("    {");
                        }
                        if (!(documentationPrompt) && obj.Changelog.Count > 0 && !addBrackets)
                        {
                            writer.WriteLine(Environment.NewLine + "      Automated Documentation");
                        }
                        else if (!(documentationPrompt) && obj.Changelog.Count > 0 && addBrackets)
                        {
                            writer.WriteLine("      Automated Documentation");
                        }
                        foreach (string item in docModifications) //TagDetection.GetModyficationList(obj.Contents))
                        {
                            int actionCounter = 0;
                            if (ObjectModList.Contains(item))
                            {
                                writer.WriteLine("      #" + item + "#");
                            }
                            locationList = new List<string>();
                            foreach (ChangeClass change in obj.Changelog)
                            {
                                if (change.ChangelogCode == item && change.ChangeType != "Action" && !(locationList.Exists(loc => loc == change.Location)))
                                {

                                    if (change.ChangeType == "Field")
                                    {
                                        int fieldNum = Int32.Parse(change.SourceObject.Split(' ').ToArray().ElementAt(1));
                                        if (fieldNum <= 50000 || fieldNum >= 99999) 
                                        {
                                            locationList.Add(change.Location);
                                            writer.WriteLine("      - Changed " + change.ChangeType + ": " + change.Location);
                                        }
                                        else
                                        {
                                            locationList.Add(change.Location);
                                            writer.WriteLine("      - New " + change.ChangeType + ": " + change.Location);
                                        }
                                    }
                                    else if(change.ChangeType == "NewObj")
                                    {
                                        locationList.Add(change.Location);
                                        writer.WriteLine("      - New " + change.Contents + ": " + change.Location);
                                    }
                                    else
                                    {
                                        locationList.Add(change.Location);
                                        writer.WriteLine("      - New " + change.ChangeType + ": " + change.Location);
                                    }
                                }

                                if (change.ChangeType == "Action" && change.ChangelogCode == item) actionCounter++;
                            }

                            if (actionCounter == 1)
                            {
                                writer.WriteLine("      - New Action");
                            }
                            else if (actionCounter > 1)
                            {
                                writer.WriteLine("      - New Actions");
                            }
                        }
                        if (addBrackets) writer.WriteLine("    }");
                        addBrackets = false;
                        writing = false;
                    }
                    if (!(deleteFlag)) writer.WriteLine(line);
                }

                obj.Contents = builder.ToString();
                writer.Close();
                builder = new StringBuilder();
                writer = new StringWriter(builder);
            }
            return true;
        }
    }
}
