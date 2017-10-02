using System.Collections.Generic;
using System.Linq;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using IT.integro.DynamicsNAV.ProcessingTool.changeDetection;

namespace IT.integro.DynamicsNAV.ProcessingTool.fileSplitter
{
    public struct Merge
    {
        public string fromMod;
        public string toMod;
    }

    public class MergeTool
    {
        Merge merge;
        List<TagRepository.Tags> mergeTagList;
        static string[] fileLines;
        static string inputFilePath;

        public MergeTool(string path)
        {
            inputFilePath = path;
            fileLines = System.IO.File.ReadAllLines(inputFilePath);
        }

        public void FindTagsToMerge(Merge mergeParameter)
        {
            merge = mergeParameter;
            mergeTagList = TagRepository.fullTagList.Where(w => w.mod == mergeParameter.fromMod).ToList();
        }

        public bool Merge()
        {
            foreach (var tag in mergeTagList)
            {
                fileLines[tag.inLine - 1] = fileLines[tag.inLine - 1].Replace(merge.fromMod, merge.toMod);
                if (TagDetection.GetTagedModification(fileLines[tag.inLine - 1]) != merge.toMod)
                    return false;
            }
            return true;
        }

        static public void SaveFile()
        {
            string directoryPath = System.IO.Path.GetDirectoryName(inputFilePath) + "\\";
            string outputFileName = System.IO.Path.GetFileNameWithoutExtension(inputFilePath) + "(Merged).txt";
            string outputFilePath = directoryPath + outputFileName;
            System.IO.File.WriteAllLines(outputFilePath, fileLines);
        }
    }
}
