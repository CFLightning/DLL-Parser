using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;
using System.Collections.Generic;
using System.Text;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace IT.integro.DynamicsNAV.ProcessingTool.modificationSearchTool
{
    public class ModificationSearchTool
    {
        private static List<string> tags;
        private static List<bool> checklist;

        static void InitTags(ObjectClass obj)
        {
            tags = TagDetection.GetModyficationList(obj.Contents);
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
                //foreach(string expectedModification in expectedModifications)
                //{
                //    if (!(tags.Contains(expectedModification)))
                //    {
                //        return false;
                //    }
                //}

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
                    bool fieldFlag = false, actionFlag = false, controlFlag = false, openControlFlag = false;
                    string fieldName = "", sourceExpr = "", description = "", fieldContent = "";

                    while (null != (line = reader.ReadLine()))
                    {
                        if (TriggerDetection.DetectIfAnyTriggerInLine(line))
                            trigger = TriggerDetection.GetTriggerName(line);

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
                                    change = new ChangeClass(modtag, line, "Code", (fieldFlag ? (fieldName + " - ") : "") + trigger, obj.Type + " " + obj.Number + " " + obj.Name);
                                    ChangeClassRepository.AppendChange(change);
                                    obj.Changelog.Add(change);
                                }
                                else if (TagDetection.GetTagedModyfication(line) == modtag && TagDetection.CheckIfBeginTagInLine(line))
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
                                if (line.Contains("Description=") && TagDetection.GetDescriptionTagList(line).Contains(modtag) && !(line.Contains("Version List=")))
                                {
                                    change = new ChangeClass(modtag, fieldContent, "Field", fieldName, obj.Type + " " + obj.Number + " " + obj.Name);
                                    ChangeClassRepository.AppendChange(change);
                                    obj.Changelog.Add(change);
                                }
                            }
                            else if (obj.Type == "Page")
                            {
                                if (actionFlag)
                                {
                                    if (line.Contains(modtag) && line.Contains("Description="))
                                    {
                                        description = FlagDetection.GetDescription(line);
                                        change = new ChangeClass(modtag, "", "Action", "", "");
                                        ChangeClassRepository.AppendChange(change);
                                        obj.Changelog.Add(change);
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
                                    }
                                }
                            }
                        }
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
    }
}
