using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TagRepo = IT.integro.DynamicsNAV.ProcessingTool.repositories.TagRepository;

namespace IT.integro.DynamicsNAV.ProcessingTool.changeDetection
{
    class TagDetection
    {
        static Regex[] tagPatterns;
        static List<Regex[]> tagPairPattern;
        static string regMod;
        static string regModNoSlash;
        static string regModNAV;
        static string regITPrefix;
        static string regEnd;
        static string lineFrontComment;
        static string lineBackComment;

        static TagDetection()
        {
            tagPatterns = new Regex[3];
            tagPairPattern = new List<Regex[]>();
            regMod = @"(?<mod>[A-Z0-9][A-Z0-9\/.]{2,})";
            regModNoSlash = @"(?<mod>[A-Z0-9._-]+)";
            regModNAV = @"(?<mod>NAV[A-Z0-9\/._-]+)";
            regITPrefix = @"(IT\/)?";
            regEnd = @" *$";
            lineFrontComment = @"^\s*\/{2}\s*";
            lineBackComment = @"^ *[^\s\/{2,}]+.*\/{2,} *";

            DefinePatterns();
        }

        enum Marks { BEGIN, END, OTHER };

        static private Regex[] DefinePatterns()
        {
            //string regDate = @"(\d{2,4}.\d{2}.\d{2,4})?";
            //string regPrefix = @"((\w)*\/)?";
            //string regDigitalSuffix = @"(\/\d*)?";

            List<Regex[]> PatternList = new List<Regex[]>();

            List<string> beginPatternParts = new List<string>();
            beginPatternParts.Add(@"<-+ *" + regITPrefix + regMod + regEnd);
            beginPatternParts.Add(regITPrefix + regMod + @" *(?i)((begin)|(start))" + regEnd);
            beginPatternParts.Add(regITPrefix + regMod + @" *(?i)(\/S|\/B)" + regEnd);
            beginPatternParts.Add(@"START\/" + regModNAV + regEnd);
            beginPatternParts.Add(@"START\/(\w*\/)*" + regModNoSlash + regEnd);

            List<string> endPatternParts = new List<string>();
            endPatternParts.Add(@"-+> *" + regITPrefix + regMod + regEnd);
            endPatternParts.Add(regITPrefix + regMod + @" *(?i)((end)|(stop))" + regEnd);
            endPatternParts.Add(regITPrefix + regMod + @" *(?i)/E" + regEnd);
            endPatternParts.Add(@"STOP ?\/" + regModNAV + regEnd);
            endPatternParts.Add(@"STOP ?\/(\w*\/)*" + regModNoSlash + regEnd);

            List<string> otherPatternParts = new List<string>();
            otherPatternParts.Add(regITPrefix + regMod + @" *(?i)\/S\/E$");

            Regex rgxBegin, rgxEnd, rgxOther;
            Regex[] rgxPair;

            if (beginPatternParts.Count == endPatternParts.Count)
                for (int i = 0; i < beginPatternParts.Count; i++)
                {
                    rgxBegin = new Regex(lineFrontComment + beginPatternParts[i]);
                    rgxEnd = new Regex(lineFrontComment + endPatternParts[i]);
                    rgxPair = new Regex[2];
                    rgxPair[(int)Marks.BEGIN] = rgxBegin;
                    rgxPair[(int)Marks.END] = rgxEnd;
                    tagPairPattern.Add(rgxPair);
                }

            string endPattern = "(" + lineFrontComment + endPatternParts[0] + ")";
            for (int i = 1; i < endPatternParts.Count; i++)
                endPattern += "|(" + lineFrontComment + endPatternParts[i] + ")";

            string beginPattern = "(" + lineFrontComment + beginPatternParts[0] + ")";
            for (int i = 1; i < beginPatternParts.Count; i++)
                beginPattern += "|(" + lineFrontComment + beginPatternParts[i] + ")";

            string otherPattern = "(" + lineBackComment + otherPatternParts[0] + ")";
            for (int i = 1; i < otherPatternParts.Count; i++)
                otherPattern += "|(" + lineBackComment + otherPatternParts[i] + ")"; // CHANGEEE

            rgxBegin = new Regex(beginPattern);
            rgxEnd = new Regex(endPattern);
            rgxOther = new Regex(otherPattern);

            tagPatterns[(int)Marks.BEGIN] = rgxBegin;
            tagPatterns[(int)Marks.END] = rgxEnd;
            tagPatterns[(int)Marks.OTHER] = rgxOther;

            return tagPatterns;
        }
        /// <summary>
        /// General regex definition
        /// </summary>
        static public void SetHighAccuracy()
        {
            List<string> beginPatternParts = new List<string>();
            beginPatternParts.Add(@"<-+ *" + regMod + regEnd);
            beginPatternParts.Add(regMod + @" *(?i)((begin)|(start))" + regEnd);
            beginPatternParts.Add(regMod + @" *(?i)(\/S|\/B)" + regEnd);
//            beginPatternParts.Add(@"START\/" + regModNAV + regEnd);
            beginPatternParts.Add(@"START\/" + regMod + regEnd);

            List<string> endPatternParts = new List<string>();
            endPatternParts.Add(@"-+> *" + regITPrefix + regMod + regEnd);
            endPatternParts.Add(regMod + @" *(?i)((end)|(stop))" + regEnd);
            endPatternParts.Add(regMod + @" *(?i)/E" + regEnd);
//            endPatternParts.Add(@"STOP ?\/" + regModNAV + regEnd);
            endPatternParts.Add(@"STOP ?\/" + regMod + regEnd);

            List<string> otherPatternParts = new List<string>();
            otherPatternParts.Add(regITPrefix + regMod + @" *(?i)/S/E" + regEnd);
            otherPatternParts.Add(regITPrefix + regMod + @" *" + regEnd);
            
            Regex rgxBegin, rgxEnd, rgxOther;
            Regex[] rgxPair;

            if (beginPatternParts.Count == endPatternParts.Count)
                for (int i = 0; i < beginPatternParts.Count; i++)
                {
                    rgxBegin = new Regex(lineFrontComment + beginPatternParts[i]);
                    rgxEnd = new Regex(lineFrontComment + endPatternParts[i]);
                    rgxPair = new Regex[2];
                    rgxPair[(int)Marks.BEGIN] = rgxBegin;
                    rgxPair[(int)Marks.END] = rgxEnd;
                    tagPairPattern.Add(rgxPair);
                }

            string endPattern = "(" + lineFrontComment + endPatternParts[0] + ")";
            for (int i = 1; i < endPatternParts.Count; i++)
                endPattern += "|(" + lineFrontComment + endPatternParts[i] + ")";

            string beginPattern = "(" + lineFrontComment + beginPatternParts[0] + ")";
            for (int i = 1; i < beginPatternParts.Count; i++)
                beginPattern += "|(" + lineFrontComment + beginPatternParts[i] + ")";

            string otherPattern = "(" + lineBackComment + otherPatternParts[0] + ")";
            for (int i = 1; i < otherPatternParts.Count; i++)
                otherPattern += "|(" + lineBackComment + otherPatternParts[i] + ")"; // CHANGEEE

            rgxBegin = new Regex(beginPattern);
            rgxEnd = new Regex(endPattern);
            rgxOther = new Regex(otherPattern);

            tagPatterns[(int)Marks.BEGIN] = rgxBegin;
            tagPatterns[(int)Marks.END] = rgxEnd;
            tagPatterns[(int)Marks.OTHER] = rgxOther;
        }

        static public Regex GetFittingEndPattern(string beginTagLine)
        {
            string mod = GetTagedModification(beginTagLine);
            foreach (var patternPair in tagPairPattern)
            {
                if (patternPair[(int)Marks.BEGIN].IsMatch(beginTagLine))
                {
                    string patternString = patternPair[(int)Marks.END].ToString();
                    if (patternPair[(int)Marks.BEGIN].ToString().Contains(regMod))
                        patternString = patternString.Replace(regMod, mod);
                    else if (patternPair[(int)Marks.BEGIN].ToString().Contains(regModNAV))
                        patternString = patternString.Replace(regModNAV, mod);
                    else if (patternPair[(int)Marks.BEGIN].ToString().Contains(regModNoSlash))
                        patternString = patternString.Replace(regModNoSlash, mod);
                    return new Regex(patternString);
                }
            }
            return new Regex(mod);
        }

        static public bool CheckIfTagInLine(string text)
        {
            if (tagPatterns[(int)Marks.BEGIN].IsMatch(text))
                return true;
            else if (tagPatterns[(int)Marks.END].IsMatch(text))
                return true;
            else if (tagPatterns[(int)Marks.OTHER].IsMatch(text))
                return true;
            else
                return false;
        }

        static public bool CheckIfBeginTagInLine(string text)
        {
            if (tagPatterns[(int)Marks.BEGIN].IsMatch(text))
                return true;
            else
                return false;
        }

        static public bool CheckIfTagsIsAlone(string tagLine)
        {
            if (tagPatterns[(int)Marks.OTHER].IsMatch(tagLine))
                return true;
            else
                return false;
        }

        static private List<string> FindTags(string[] codeLines)
        {
            List<string> tagList = new List<string>();
            foreach (var line in codeLines)
            {
                if (line.Contains(@"//") && line.TrimStart(' ').Length < 200)
                {
                    if (CheckIfTagInLine(line))
                    {
                        tagList.Add(line);
                    }
                }

            }
            return tagList;
        }

        static private List<string> FindModsInTags(List<string> tagList, bool saveTagList = false)
        {
            List<string> tagModList = new List<string>();
            Match match = null;

            foreach (var tag in tagList)
            {
                if (tagPatterns[(int)Marks.BEGIN].IsMatch(tag))
                {
                    match = tagPatterns[(int)Marks.BEGIN].Match(tag);
                }
                else if (tagPatterns[(int)Marks.END].IsMatch(tag))
                {
                    match = tagPatterns[(int)Marks.END].Match(tag);
                }
                else if (tagPatterns[(int)Marks.OTHER].IsMatch(tag))
                {
                    match = tagPatterns[(int)Marks.OTHER].Match(tag);
                }

                tagModList.Add(match.Groups["mod"].Value);
            }

            List<string> modList = new List<string>();
            foreach (var tag in tagModList)
            {
                if (!modList.Contains(tag) && !ContainsRestrictedWords(tag))
                {
                    modList.Add(tag);
                }
            }
            return modList;
        }

        static private List<string> FindModsInRepo()
        {
            List<string> tagModList = new List<string>();
            Match match = null;

            foreach (var tag in TagRepo.fullTagList.Where(t => t.mod != null).ToList())
            {
                if (tagPatterns[(int)Marks.BEGIN].IsMatch(tag.line))
                {
                    match = tagPatterns[(int)Marks.BEGIN].Match(tag.line);
                }
                else if (tagPatterns[(int)Marks.END].IsMatch(tag.line))
                {
                    match = tagPatterns[(int)Marks.END].Match(tag.line);
                }
                else if (tagPatterns[(int)Marks.OTHER].IsMatch(tag.line))
                {
                    match = tagPatterns[(int)Marks.OTHER].Match(tag.line);
                }
                TagRepo.Tags newTag = tag;
                newTag.mod = match.Groups["mod"].Value;
                int idx = TagRepo.fullTagList.FindIndex(t => t.inLine == tag.inLine);
                TagRepo.fullTagList[idx] = newTag;
                tagModList.Add(match.Groups["mod"].Value);
            }

            List<string> modList = new List<string>();
            foreach (var tag in tagModList)
            {
                if (!modList.Contains(tag) && !ContainsRestrictedWords(tag))
                {
                    modList.Add(tag);
                }
            }
            return modList;
        }

        static List<string> RestrictedWords = new List<string>
        {
            "ASSERTERROR",
            "BEGIN",
            "CASE",
            "DO",
            "DOWNTO",
            "ELSE",
            "END",
            "EXIT",
            "FOR",
            "IF",
            "OF",
            "REPEAT",
            "THEN",
            "TO",
            "UNTIL",
            "WHILE",
            "WITH",
            "OR"
        };

        static public bool ContainsRestrictedWords(string tag)
        {
            foreach(string word in RestrictedWords)
            {
                if (tag == word)
                    return true;
            }
            return false;
        }

        static public string GetTagedModification(string tagLine)
        {
            if (tagPatterns[(int)Marks.BEGIN].IsMatch(tagLine))
            {
                return tagPatterns[(int)Marks.BEGIN].Match(tagLine).Groups["mod"].Value;
            }
            else if (tagPatterns[(int)Marks.END].IsMatch(tagLine))
            {
                return tagPatterns[(int)Marks.END].Match(tagLine).Groups["mod"].Value;
            }
            else if (tagPatterns[(int)Marks.OTHER].IsMatch(tagLine))
            {
                return tagPatterns[(int)Marks.OTHER].Match(tagLine).Groups["mod"].Value;
            }
            return "";
        }

        static public List<string> GetModyficationList(string code)
        {
            string[] codeLines = code.Replace("\r", "").Split('\n');
            List<string> tags = FindModsInTags(FindTags(codeLines));
            List<string> descs = GetFieldDescriptionTagList(code);
            return tags.Union(descs).ToList();
        }

        static public string GetModificationString(string path)
        {
            TagRepo.ClearRepo(true);
            TagRepo.DeleteFiles();

            StreamReader inputfile = new StreamReader(path, Encoding.GetEncoding("ISO-8859-1"));
            
            string line;
            List<string> mods = new List<string>();
            List<string> tags = new List<string>();
            while ((line = inputfile.ReadLine()) != null)
            {
                string[] codeLine = line.Split('\n'); //Replace("\r", "").
                FindTagsToRepo(codeLine);
            }
            inputfile.Close();
            
            mods = TagRepo.GetAllModList().Distinct().ToList();
            TagRepo.SaveToFilesFull();
            return string.Join(",", mods.ToArray());
        }

        static public string GetModificationStringUsingRepo(string path)
        {
            TagRepo.ClearRepo(false);
            TagRepo.DeleteFiles();

            StreamReader inputfile = new StreamReader(path, Encoding.GetEncoding("ISO-8859-1"));

            string line;
            List<string> mods = new List<string>();
            List<string> tags = new List<string>();
            int lineNumber = 1;
            int tagNumber = 0;
            int tagCount = TagRepo.fullTagList.Count();

            while ((line = inputfile.ReadLine()) != null && tagNumber < TagRepo.fullTagList.Count())
            {
                if (TagRepo.fullTagList[tagNumber].inLine == lineNumber)
                {
                    TagRepo.Tags tag = TagRepo.fullTagList[tagNumber];
                    tag.line = line.TrimStart(' ');
                    if (CheckIfTagInLine(line))
                    {
                        if (!ContainsRestrictedWords(GetTagedModification(line)))
                        {
                            tag.mod = GetTagedModification(line);
                        }
                    }

                    TagRepo.fullTagList.Add(tag);
                    tagNumber++;
                }
                lineNumber++;
            }
            TagRepo.fullTagList.RemoveRange(0, tagCount);
            inputfile.Close();

            mods = TagRepo.GetAllModList().Distinct().ToList();
            TagRepo.SaveToFilesFull();
            return string.Join(",", mods.ToArray());
        }

        static public void FindTagsToRepo(string[] codeLines)
        {
            if (TagRepo.tagObject == string.Empty)
                TagRepo.tagObject = codeLines[0];
            
            foreach (var line in codeLines)
            {
                TagRepo.lineNo++;

                //  Ommit RDLDATA section of Report
                if (TagRepo.flagReport)
                {
                    if (!TagRepo.flagRDLDATA)
                    {
                        TagRepo.flagRDLDATA = FlagDetection.DetectIfTableRDLBegin(line);
                    }
                    else if (TagRepo.flagRDLDATA)
                    {
                        if (TagRepo.flagRDLDATA = !FlagDetection.DetectIfTableRDLEnd(line))
                        continue;
                    }
                }

                if (line.Contains(@"//"))
                {
                    TagRepo.Tags tag = new TagRepo.Tags();
                    tag.line = line.TrimStart(' ');
                    tag.inLine = TagRepo.lineNo;
                    tag.inObject = TagRepo.tagObject;
                    tag.isCodeOrField = true;

                    if (CheckIfTagInLine(line))
                    {
                        if (!ContainsRestrictedWords(GetTagedModification(line)))
                        {
                            tag.mod = GetTagedModification(line);
                        }
                    }
                    TagRepo.fullTagList.Add(tag);
                }
                else if (line.Contains("Description="))
                {
                    List<string> descMods = TagDetection.GetLineDescriptionTagList(line);
                    TagRepo.Tags tag = new TagRepo.Tags();
                    tag.line = line.TrimStart(' ');
                    tag.inLine = TagRepo.lineNo;
                    tag.inObject = TagRepo.tagObject;
                    tag.isCodeOrField = false;

                    if (descMods.Count() == 0)
                    {
                        TagRepo.fullTagList.Add(tag);
                    }
                    else
                    {
                        foreach (string item in descMods)
                        {
                            tag.mod = item;
                            TagRepo.fullTagList.Add(tag);
                        }
                    }
                }
                else if (line.StartsWith("OBJECT "))
                {
                    TagRepo.tagObject = line;
                    TagRepo.flagReport = TagRepo.CheckIfReport();
                }
            }
        }

        static public List<string> GetTagList(string code)
        {
            string[] codeLines = code.Replace("\r", "").Split('\n');
            return FindTags(codeLines);
        }

        static public List<string> GetFieldDescriptionTagList(string code)
        {
            string[] codeLines = code.Replace("\r", "").Split('\n');
            List<string> tagList = new List<string>();

            int i = 0;

            bool fieldFlag = false, actionFlag = false, controlFlag = false;
            while (i < codeLines.Length - 1)
            {
                if (!fieldFlag && FlagDetection.DetectIfFieldsStartFlag(codeLines[i]))
                    fieldFlag = true;
                else if (!actionFlag && FlagDetection.DetectIfActionStartFlag(codeLines[i]))
                    actionFlag = true;
                else if (!controlFlag && FlagDetection.DetectIfControlStartFlag(codeLines[i]))
                    controlFlag = true;

                if (fieldFlag && FlagDetection.DetectIfFieldsEndFlag(codeLines[i]))
                    fieldFlag = true;
                else if (actionFlag && FlagDetection.DetectIfActionEndFlag(codeLines[i]))
                    actionFlag = true;
                else if (controlFlag && FlagDetection.DetectIfControlEndFlag(codeLines[i]))
                    controlFlag = true;

                if (fieldFlag || actionFlag || controlFlag)
                {
                    if (codeLines[i].Contains("Description="))
                    {
                        tagList.AddRange(GetLineDescriptionTagList(codeLines[i]));
                    }
                }
                i++;
            }

            List<string> modList = new List<string>();
            foreach (var tag in tagList)
            {
                if (!modList.Contains(tag))
                {
                    modList.Add(tag);
                }
            }
            return tagList;
        }

        static char[] separator = new char[] { ' ' };
        static public List<string> GetLineDescriptionTagList(string codeLine)
        {
            string fieldDescription = FlagDetection.GetDescription(codeLine);
            List<string> descriptionMods = new List<string>();
            // if(fieldDescription == fieldDescription.ToUpper())
            foreach (string item in fieldDescription.Split(',').ToList())
            {
                string first = item.Trim(' ').Split(separator)[0];
                if (new Regex("^" + regMod + "$").IsMatch(first))
                {
                    descriptionMods.Add(first);
                }
            }

            return descriptionMods;
        }
    }
}
