using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IT.integro.DynamicsNAV.ProcessingTool.saveTool
{
    public class DocumentationExport
    {
        enum Types { TableData, Table, Form, Report, Dataport, Codeunit, XMLport, MenuSuite, Page };
        private static Dictionary<string, string> mappingDictionary = new Dictionary<string, string>();

        private static void InitDictionary(string path)
        {
            if(File.Exists(path))
            {
                var dictionaryLines = File.ReadLines(path);
                mappingDictionary = dictionaryLines.Select(line => line.Split(';')).ToDictionary(data => data[0], data => data[1]);
            }
            else
            {
                mappingDictionary = new Dictionary<string, string>();
            }
        }

        public static string GenerateDocumentationFile(string path, string mappingPath, List<string> expectedModifications)
        {
            Types result;
            int lineAmount = 1;
            InitDictionary(mappingPath);

            Regex lineChecker = new Regex(".*#.*#.*");
            Regex blockChecker = new Regex(".*#.*#$");
            string documentation = "";

            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);
            foreach (ObjectClass obj in ObjectClassRepository.objectRepository)
            {
                StringReader reader = new StringReader(obj.Contents);
                bool bracketFlag = false, beginFlag = false, writing = false, isOneLine = false;
                string line, tagLine = "", trimmer;

                if (Enum.TryParse(obj.Type, out result))
                {
                    while (null != (line = reader.ReadLine()))
                    {
                        if (line.StartsWith("    BEGIN"))
                        {
                            beginFlag = true;
                        }

                        if (line.StartsWith("      Automated Documentation"))
                        {
                            writing = true;
                            continue;
                        }

                        if (line.StartsWith("    {") && beginFlag)
                        {
                            bracketFlag = true;
                        }

                        if (line.StartsWith("    }") && bracketFlag)
                        {
                            bracketFlag = false;
                            writing = false;
                        }

                        if ((line.StartsWith("    END") && beginFlag))
                        {
                            beginFlag = false;
                        }

                        if (writing)
                        {
                            if (line.Length > 6)
                            {
                                line = line.Substring(6);

                                trimmer = line.TrimStart(' ');
                                trimmer = trimmer.TrimEnd(' ');
                                if (blockChecker.IsMatch(trimmer))
                                {
                                    trimmer = trimmer.Trim('#');
                                    if (mappingDictionary.ContainsKey(trimmer))
                                    {
                                        line = line.Replace(trimmer, mappingDictionary[trimmer]);
                                        trimmer = mappingDictionary[trimmer];
                                    }
                                    trimmer = "#" + trimmer + "#";
                                    tagLine = trimmer;

                                }
                                else if (lineChecker.IsMatch(trimmer))
                                {
                                    trimmer = trimmer.Substring(0, trimmer.LastIndexOf("#") + 1);
                                    tagLine = trimmer;
                                    isOneLine = true;
                                }
                                foreach (string expectedModification in expectedModifications)
                                {
                                    if ((mappingDictionary.ContainsKey(expectedModification) && (tagLine == "#" + mappingDictionary[expectedModification] + "#")) || tagLine == "#" + expectedModification + "#")
                                    {
                                        writer.WriteLine("{0}<next>{1}<next>{2}<next>{3}<next>{4}", lineAmount, (int)result, obj.Name, tagLine, line);
                                    }
                                   
                                    //if ((tagLine == "#" + mappingDictionary[expectedModification] + "#") || (tagLine == "#" + expectedModification + "#"))
                                    //{
                                    //    writer.WriteLine("{0}<next>{1}<next>{2}<next>{3}<next>{4}", lineAmount, (int)result, obj.Name, tagLine, line);
                                    //}
                                }
                                lineAmount++;
                                if (isOneLine)
                                {
                                    tagLine = "";
                                    isOneLine = false;
                                }
                            }
                        }
                    }
                }
            }
            documentation = builder.ToString();
            return documentation;
        }
    }
}
