using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IT.integro.DynamicsNAV.ProcessingTool.changeDetection
{
    class TagDetection
    {
        static Regex[] tagPatterns;
        static List<Regex[]> tagPairPattern;
        static string modNo;
        static string prefix;
        static string endSymbol;
        static string date;

        static TagDetection()
        {
            tagPatterns = new Regex[3];
            tagPairPattern = new List<Regex[]>();
            DefinePatternsNEW();
        }

        enum Marks { BEGIN, END, OTHER };

        static private Regex[] DefinePatternsNEW()
        {
            string lineFrontComment = @" *// *";                        // BEGIN,AND
            string lineBackComment = @" *[^\s/{2}]+.*// *";             // OTHER

            date = @"(\d{2,4}.\d{2}.\d{2,4})";
            modNo = @"(?<mod>([A-Z0-9\._-]+)|" + date + ")";
            prefix = @"(IT/)?";
            endSymbol = @".?" + date + @"?" + "($| )";

            List<Regex[]> PatternList = new List<Regex[]>();

            List<string> beginPatternParts = new List<string>();
            beginPatternParts.Add(@"<-+ *" + prefix + modNo + endSymbol);
            beginPatternParts.Add(prefix + modNo + @" *(?i)((begin)|(start))" + endSymbol);
            beginPatternParts.Add(prefix + modNo + @" *(?i)(/S|/B)" + endSymbol);

            List<string> endPatternParts = new List<string>();
            endPatternParts.Add(@"-+> *" + prefix + modNo + endSymbol);
            endPatternParts.Add(prefix + modNo + @" *(?i)((end)|(stop))" + endSymbol);
            endPatternParts.Add(prefix + modNo + @" *(?i)/E" + endSymbol);

            List<string> otherPatternParts = new List<string>();
            otherPatternParts.Add(prefix + modNo + @" *$");
            otherPatternParts.Add(prefix + modNo + @" *(?i)/S/E$");

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

        static public Regex GetFittingEndPattern(string beginTagLine)
        {
            foreach (var patternPair in tagPairPattern)
            {
                if (patternPair[(int)Marks.BEGIN].IsMatch(beginTagLine))
                {
                    string patternString = patternPair[(int)Marks.END].ToString();
                    string mod = GetTagedModyfication(beginTagLine);
                    patternString = patternString.Replace(modNo, mod);
                    return new Regex(patternString);
                }
            }
            return new Regex(@"");
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
                if (line.Contains(@"//"))
                {
                    if (CheckIfTagInLine(line))
                    {
                        tagList.Add(line);
                    }
                }
            }
            return tagList;
        }

        static private List<string> FindModsInTags(List<string> tagList)
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
                if (!modList.Contains(tag))
                {
                    modList.Add(tag);
                }
            }

            return modList;
        }


        static public string GetTagedModyfication(string tagLine)
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
            List<string> ret = FindModsInTags(FindTags(codeLines));
            return ret.Union(GetFieldDescriptionTagList(code)).ToList();
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
                        tagList.AddRange(GetDescriptionTagList(codeLines[i]));
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

        static public List<string> GetDescriptionTagList(string codeLine)
        {
            string fieldDescription = FlagDetection.GetDescription(codeLine);
            fieldDescription = fieldDescription.Replace("IT/", "");
            return fieldDescription.Split(',').ToList();
        }
    }
}
