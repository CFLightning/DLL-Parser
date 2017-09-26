using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AuxRepo = IT.integro.DynamicsNAV.ProcessingTool.repositories.AuxiliaryRepository;

namespace IT.integro.DynamicsNAV.ProcessingTool.changeDetection
{
    class TagDetection
    {
        static Regex[] tagPatterns;
        static List<Regex[]> tagPairPattern;
        static string regMod;
        static string regModNoSlash;
        static string regModNAV;
        static string regITPrefix = @"(IT\/)?";

        static TagDetection()
        {
            tagPatterns = new Regex[3];
            tagPairPattern = new List<Regex[]>();
            regMod = @"(?<mod>[A-Z0-9\/._-]+)";
            regModNoSlash = @"(?<mod>[A-Z0-9._-]+)";
            regModNAV = @"(?<mod>NAV[A-Z0-9\/._-]+)";
            regITPrefix = @"(IT\/)?";
            DefinePatterns();
        }

        enum Marks { BEGIN, END, OTHER };

        static private Regex[] DefinePatterns()
        {
            string lineFrontComment = @"^\s*\/\/\s*";                        // BEGIN,AND
            string lineBackComment = @" *[^\s\/{2}]+.*\/\/ *";             // OTHER

            //string regDate = @"(\d{2,4}.\d{2}.\d{2,4})?";
            //string regPrefix = @"((\w)*\/)?";
            //string regDigitalSuffix = @"(\/\d*)?";
            string regEnd = @".?$";

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

        static public void SetHighAccuracy()
        {
            string lineBackComment = @" *[^\s/{2}]+.*// *";
            List<string> otherPatternParts = new List<string>();
            otherPatternParts.Add(regITPrefix + regMod + @" *$");
            otherPatternParts.Add(regITPrefix + regMod + @" *(?i)/S/E$");
            string otherPattern = "(" + lineBackComment + otherPatternParts[0] + ")";
            for (int i = 1; i < otherPatternParts.Count; i++)
                otherPattern += "|(" + lineBackComment + otherPatternParts[i] + ")";
            Regex rgxOther = new Regex(otherPattern);
            tagPatterns[(int)Marks.OTHER] = rgxOther;
        }

        static public Regex GetFittingEndPattern(string beginTagLine)
        {
            string mod = GetTagedModyfication(beginTagLine);
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
            if (tagPatterns[(int)Marks.BEGIN].IsMatch(text) || tagPatterns[(int)Marks.END].IsMatch(text) || tagPatterns[(int)Marks.OTHER].IsMatch(text))
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

            AuxRepo.foundTagList = AuxRepo.foundTagList.Concat(tagList).ToList();
            AuxRepo.modInTagList = AuxRepo.modInTagList.Concat(tagModList).ToList();

            return modList;
        }

        static public bool ContainsRestrictedWords(string tag)
        {
            List<string> RestrictedWords = new List<string>
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
                "WITH"
            };
            foreach(string word in RestrictedWords)
            {
                if (tag == word)
                    return true;
            }
            return false;
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

        static public string GetModificationString(string path)
        {
            int blockSize = 1000000;
            int blockCount = File.ReadLines(path).Count() / blockSize;
            List<string> mods = new List<string>();
            string[] lines;

            for (int i = 0; i < blockCount; i++)
            {
                lines = File.ReadLines(path).Skip(blockSize * i).Take(blockSize).ToArray();
                mods = mods.Union(FindModsInTags(FindTagsAndGenerateList(lines), true)).ToList();
            }
            lines = File.ReadAllLines(path).Skip(blockCount * blockSize).ToArray();
            mods = mods.Union(FindModsInTags(FindTagsAndGenerateList(lines), true)).ToList();

            AuxRepo.DeleteFiles();
            AuxRepo.SaveToFiles();
            
            return string.Join(",", mods.ToArray());
        }

        static string obj;
        static private List<string> FindTagsAndGenerateList(string[] codeLines)
        {
            char[] separator = new char[] { ' ' };
            if (obj == null)
                obj = codeLines[0];//.Split(separator, 4)[3];
            List<string> tagList = new List<string>();

            foreach (var line in codeLines)
            {
                if (line.Contains(@"//"))
                {
                    if (CheckIfTagInLine(line))
                    {
                        tagList.Add(line.TrimStart(' '));
                        string mod = GetTagedModyfication(line);
                        if (!ContainsRestrictedWords(mod))
                        {
                            if (!AuxRepo.modList.Contains(mod))
                            {
                                AuxRepo.modList.Add(mod);
                                AuxRepo.modContentList.Add(obj);
                                continue;
                            }
                            if (!AuxRepo.modContentList[AuxRepo.modList.IndexOf(mod)].Contains(obj))
                                AuxRepo.modContentList[AuxRepo.modList.IndexOf(mod)] += (System.Environment.NewLine + obj);

                            if (!AuxRepo.objList.Contains(obj))
                            {
                                AuxRepo.objList.Add(obj);
                                AuxRepo.objContentList.Add(mod);
                                continue;
                            }
                            if (!AuxRepo.objContentList[AuxRepo.objList.IndexOf(obj)].Contains(mod))
                                AuxRepo.objContentList[AuxRepo.objList.IndexOf(obj)] += (System.Environment.NewLine + mod);
                        }
                    }
                    else
                        AuxRepo.abandonedList.Add("\t" + line.TrimStart(' '));
                }
                else if (line.StartsWith("OBJECT "))
                {
                    AuxRepo.abandonedList.Add(line);
                    obj = line;//.Split(separator, 4)[3];
                }
            }
            
            return tagList;
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
            if(fieldDescription == fieldDescription.ToUpper())
            {
                return fieldDescription.Split(',').ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
