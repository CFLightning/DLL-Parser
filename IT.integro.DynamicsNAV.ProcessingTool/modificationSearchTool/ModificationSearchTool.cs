using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;
using System.Collections.Generic;
using System.Text;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;

namespace IT.integro.DynamicsNAV.ProcessingTool.modificationSearchTool
{
    public class ModificationSearchTool
    {
        private static List<string> tags;
        private static List<bool> checklist;

        static void InitTags(ObjectClass obj)
        {
            tags = TagDetection.GetModyficationList(obj.Contents, obj.Number);

        }

        static void InitErrorChecklist(List<string> expectedModifications)
        {
             checklist = Enumerable.Repeat(false, expectedModifications.Count()).ToList();
        }

        public static bool FindAndSaveChanges(List<string> expectedModifications)
        {
            InitErrorChecklist(expectedModifications);
            foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
            {
                InitTags(obj);

                for(int i = 0; i < expectedModifications.Count; i++)
                {
                    if (tags.Contains(expectedModifications[i]))
                    {
                        checklist[i] = true;
                    }
                }

                foreach (string modtag in tags)
                {
                    StringReader reader = new StringReader(obj.Contents);
                    StringBuilder builder = new StringBuilder();
                    StringWriter writer = new StringWriter(builder);
                    string line, currentFlag = null; //MAYBE SUBJECT TO CHANGES
                    Regex endFlag = new Regex("");
                    string nestedFlag = "";
                    ChangeClass change = new ChangeClass();
                    bool startFlag = false;
                    int nesting = 0;
                    string trigger = "";
                    bool fieldFlag = false, actionFlag = false, controlFlag = false, openControlFlag = false, rdlFlag = false, columnFlag = false, colBuildFlag = false, onlyVersionList = false;
                    string fieldName = "", sourceExpr = "", columnSourceExpr = "", description = "", fieldContent = "", columnName = "", columnContent = "";

                    while (null != (line = reader.ReadLine())) // VERSION LIST / tag 
                    {
                        if(line.StartsWith("    Version List="))
                        {
                            //string firstVer = line.Substring(line.IndexOf("Version List=") + 13); 
                            if(!line.StartsWith("    Version List=NAVW"))
                                if(TagDetection.CheckIfTagInVersionList(line, modtag))
                                    onlyVersionList = true;
                        }

                        if (TriggerDetection.DetectIfAnyTriggerInLine(line))
                        {
                            trigger = TriggerDetection.GetTriggerName(line);
                            if (TriggerDetection.DetectIfAnyTriggerInLine(line,true))
                            {
                                trigger += " (" + fieldName + ")";
                            }
                        }

                        // Flags
                        if (obj.Type == "Table")
                        {
                            if (fieldFlag == false && FlagDetection.DetectIfFieldsStartFlag(line))
                                fieldFlag = true;
                            else if (fieldFlag == true && FlagDetection.DetectIfFieldsEndFlag(line))
                                fieldFlag = false;
                            else if (fieldFlag == true && FlagDetection.DetectIfNextFieldFlag(line))
                            {
                                fieldName = FlagDetection.GetNextFieldName(line);
                                fieldContent = FlagDetection.GetNextFieldNumber(line);
                            }
                        }
                        else if (obj.Type == "Page")
                        {
                            if (actionFlag == false && FlagDetection.DetectIfActionStartFlag(line))
                                actionFlag = true;
                            else if (actionFlag == true && FlagDetection.DetectIfActionEndFlag(line))
                            {
                                actionFlag = false;
                                controlFlag = true;
                            }
                            else if (controlFlag == true && FlagDetection.DetectIfControlEndFlag(line))
                                controlFlag = false;
                        }
                        else if (obj.Type == "Report")   //  ommit table rdldata
                        {
                            if (!rdlFlag)
                            {
                                rdlFlag = FlagDetection.DetectIfTableRDLBegin(line);
                            }
                            else if (rdlFlag)
                            {
                                if (rdlFlag = !FlagDetection.DetectIfTableRDLEnd(line))
                                    continue;
                            }

                            if (columnFlag == false && FlagDetection.DetectIfDatasetStartFlag(line))
                                columnFlag = true;
                            else if (columnFlag == true && FlagDetection.DetectIfDatasetEndFlag(line))
                                columnFlag = false;
                            else if (columnFlag == true && FlagDetection.DetectIfNextFieldFlag(line))
                            {
                                columnName = FlagDetection.GetNextColumnName(line);
                                columnSourceExpr = FlagDetection.GetNextColumnExpr(line);
                                columnContent = FlagDetection.GetNextFieldNumber(line);
                            }
                        }

                        if (startFlag == true)
                        {
                            if (line.Contains(modtag) && endFlag.IsMatch(line)) 
                            {
                                if (nesting == 1) 
                                {
                                    startFlag = false;
                                    if (builder.ToString() != "")
                                    {
                                        change = new ChangeClass(currentFlag, builder.ToString(), "Code", (fieldFlag ? (fieldName + " - ") : "") + trigger, obj.Type + " " + obj.Number + " " + obj.Name);
                                        ChangeClassRepository.AppendChange(change);
                                        obj.Changelog.Add(change);
                                        onlyVersionList = false;
                                    }

                                    writer.Close();
                                    builder = new StringBuilder();
                                    writer = new StringWriter(builder);
                                }
                                nesting--; 
                            }
                            else if (line.EndsWith(nestedFlag))
                            {
                                nesting++; 
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }
                        else if (startFlag == false)
                        {
                            if (line.Contains(modtag) && !(line.StartsWith("Description=")) && !(line.Contains("Version List=")) && line.Contains(@"//"))
                            {
                                if (TagDetection.CheckIfTagsIsAlone(line))
                                {
                                    line = line.Substring(0, line.LastIndexOf(@"//"));
                                    change = new ChangeClass(modtag, line, "Code", (fieldFlag ? (fieldName + " - ") : "") + trigger, obj.Type + " " + obj.Number + " " + obj.Name);
                                    ChangeClassRepository.AppendChange(change);
                                    obj.Changelog.Add(change);
                                    onlyVersionList = false;
                                }
                                else if (TagDetection.GetTagedModification(line) == modtag && TagDetection.CheckIfBeginTagInLine(line))
                                {
                                    currentFlag = modtag;
                                    startFlag = true;
                                    nesting++; 
                                    endFlag = TagDetection.GetFittingEndPattern(line);
                                    nestedFlag = line.Trim(' ');
                                }
                            }
                            else if (obj.Type == "Table")
                            {
                                if (line.Contains("Description=") && TagDetection.GetLineDescriptionTagList(line).Contains(modtag) && !(line.Contains("Version List=")))
                                {
                                    int fieldNo = System.Int32.Parse(fieldContent);
                                    if (fieldNo < 50000 || fieldNo > 99999)
                                    {
                                        fieldContent = "Change in standard field: " + fieldNo + " " + fieldName;
                                    }
                                    else
                                    {
                                        fieldContent = "New field: " + fieldNo + " " + fieldName;
                                    }
                                    change = new ChangeClass(modtag, fieldContent, "Field", fieldName, obj.Type + " " + obj.Number + " " + obj.Name);
                                    ChangeClassRepository.AppendChange(change);
                                    obj.Changelog.Add(change);
                                    onlyVersionList = false;
                                }
                            }
                            else if (obj.Type == "Report")
                            {
                                if (line.Contains("Description=") && TagDetection.GetLineDescriptionTagList(line).Contains(modtag) && !(line.Contains("Version List=")))
                                {
                                    int columnNo = System.Int32.Parse(columnContent);
                                    change = new ChangeClass(modtag, "", "Column", columnName, obj.Header);
                                    colBuildFlag = true;
                                    onlyVersionList = false;
                                }
                                else if (columnFlag && line.Contains("SourceExpr=") && colBuildFlag)
                                {
                                    change.Contents += line.Trim(' ');
                                    change.Contents = change.Contents.Trim('}');
                                    change.Contents = change.Contents.Trim(' ');
                                    ChangeClassRepository.AppendChange(change);
                                    obj.Changelog.Add(change);
                                    colBuildFlag = false;
                                }
                            }
                            else if (obj.Type == "Page")
                            {
                                if (actionFlag)
                                {
                                    if (line.Contains(modtag) && line.Contains("Description="))
                                    {
                                        description = FlagDetection.GetDescription(line);
                                        change = new ChangeClass(modtag, "", "Action", "", obj.Header);
                                        ChangeClassRepository.AppendChange(change);
                                        obj.Changelog.Add(change);
                                        onlyVersionList = false;
                                    }
                                }
                                else if (controlFlag)
                                {
                                    if (line.Contains(modtag) && line.Contains("Description="))
                                    {
                                        description = FlagDetection.GetDescription(line);
                                        openControlFlag = true;
                                    }
                                    else if (openControlFlag && line.Contains("SourceExpr="))
                                    {
                                        sourceExpr = FlagDetection.GetSourceExpr(line);
                                        change = new ChangeClass(modtag, "", "Control", sourceExpr, "");
                                        ChangeClassRepository.AppendChange(change);
                                        obj.Changelog.Add(change);
                                        openControlFlag = false;
                                        onlyVersionList = false;
                                    }
                                }
                            }
                        }
                    }

                    if (onlyVersionList)
                    {
                        change = new ChangeClass(modtag, PrepareCodeunitHash(obj.Contents), "NewObj", "", obj.Header);
                        ChangeClassRepository.AppendChange(change);
                        obj.Changelog.Add(change);
                    }
                }
            }
            if (checklist.Contains(false))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        static public string PrepareCodeunitHash(string code)
        {
            //string[] codeLines = code.Replace("\r", "").Split('\n');
            string noProperties = code.Remove(code.IndexOf("OBJECT-PROPERTIES"), code.IndexOf("  PROPERTIES") - code.IndexOf("OBJECT-PROPERTIES"));

            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(code);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
