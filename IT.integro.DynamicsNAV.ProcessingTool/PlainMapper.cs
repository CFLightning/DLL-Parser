using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IT.integro.DynamicsNAV.ProcessingTool
{
    public static class PlainMapper
    {
        private static Dictionary<string, string> mappingDictionary = new Dictionary<string, string>();

        private static void InitDictionary(string path)
        {
            if (File.Exists(path))
            {
                var dictionaryLines = File.ReadLines(path);
                mappingDictionary = dictionaryLines.Select(line => line.Split(';')).ToDictionary(data => data[0], data => data[1]);
            }
            else
            {
                mappingDictionary = new Dictionary<string, string>();
            }
        }

        public static string MapDocumentationToFile(string textToMap, string mappingPath)
        {
            InitDictionary(mappingPath);

            string workText = textToMap.Replace(@"%$%", "\n");

            Regex lineChecker = new Regex(".*#.*#.*");
            Regex blockChecker = new Regex(".*#.*#$");
            string documentation = "";
            int lineAmount = 1;

            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);
            StringReader reader = new StringReader(workText);
            string line;
            
            while (null != (line = reader.ReadLine()))
            {
                string[] lineDecompose = line.Split(new string[] { "<next>" }, StringSplitOptions.None);

                if(mappingDictionary.ContainsKey(lineDecompose[3].Trim('#')))
                {
                    writer.WriteLine("{0}<next>{1}<next>{2}<next>{3}<next>{4}", lineAmount, lineDecompose[1], lineDecompose[2], "#" + mappingDictionary[lineDecompose[3].Trim('#')] + "#", lineDecompose[4]);
                }
                else
                {
                    writer.WriteLine("{0}<next>{1}<next>{2}<next>{3}<next>{4}", lineAmount, lineDecompose[1], lineDecompose[2], lineDecompose[3], lineDecompose[4]);
                }
                lineAmount++;
            }
            documentation = builder.ToString();
            return documentation;
        }
    }
}