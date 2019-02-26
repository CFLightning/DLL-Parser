using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IT.integro.DynamicsNAV.ProcessingTool.fileSplitter
{
    public class FileSplitter
    {
        public static bool SplitFile(string path)
        {
            StringBuilder builder = new StringBuilder();
            StringWriter sWriter = null;
            ObjectClass newObject = new ObjectClass();
            string name = "";
            try
            {
                using (StreamReader inputfile = new StreamReader(path, EncodingManager.nav))
                {
                    string line;
                    while ((line = inputfile.ReadLine()) != null)
                    {
                        if (sWriter == null || line.StartsWith("OBJECT ") || inputfile.EndOfStream)
                        {
                            if (sWriter != null)
                            {
                                sWriter.Close();
                                sWriter = null;
                            }
                            if (newObject.Name != "")
                            {
                                //if (inputfile.EndOfStream)
                                //{
                                //    builder.AppendLine("}");
                                //}
                                newObject.Contents = builder.ToString();
                                ObjectClassRepository.AppendObject(newObject);
                            }

                            builder.Clear();
                            sWriter = new StringWriter(builder);
                        }

                        if (line.StartsWith("OBJECT "))
                        {
                            name = "";
                            string[] parameters = line.Split(' ');
                            for (int i = 3; i <= parameters.Length - 1; i++)
                            {
                                name = string.Concat(name, parameters[i]);
                            }
                            name = name.Replace("/", "");
                            newObject = new ObjectClass(line, Int32.Parse(parameters[2]), parameters[1], name, "");
                        }
                        sWriter.WriteLine(line);
                    }
                }
            }
            finally
            {
                if (sWriter != null)
                    sWriter.Close();
            }
            return true;
        }

        public static List<string> ReduceObjects(List<string> expectedModifications)
        {
            List<string> objectsToSearch = new List<string>();

            foreach (var mod in expectedModifications)
            {
                string[] allText = TagRepository.GetModObjectList(mod).ToArray();
                objectsToSearch = objectsToSearch.Union(allText).ToList();
            }
            //char[] separator = new char[] { ' ' };

            //for (int i = 0; i < objectsToSearch.Count; i++)
            //{
            //    objectsToSearch[i] = objectsToSearch[i].Split(separator, 4)[1].Replace(" ", string.Empty);
            //}

            ObjectClassRepository.objectRepository = ObjectClassRepository.objectRepository.Where(o => objectsToSearch.Contains(o.Header)).ToList();
            return objectsToSearch;
        }
    }
}
