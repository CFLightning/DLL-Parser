using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.integro.DynamicsNAV.ProcessingTool.repositories
{
    static class AuxiliaryRepository
    {
        static public List<string> abandonedList = new List<string>();
        static public List<string> foundTagList = new List<string>();
        static public List<string> modInTagList = new List<string>();
        static public List<string> modContentList = new List<string>();
        static public List<string> modList = new List<string>();
        static public List<string> objContentList = new List<string>();
        static public List<string> objList = new List<string>();

        static string pathAbandoned = Path.GetTempPath() + @"NAVCommentTool\Abandoned comments.txt";
        static string pathFoundTag = Path.GetTempPath() + @"NAVCommentTool\tagList.txt";
        static string pathModInTag = Path.GetTempPath() + @"NAVCommentTool\tagModList.txt";
        static public string pathModInObj = Path.GetTempPath() + @"NAVCommentTool\Modifications in Object\";
        static public string pathObjWithMod = Path.GetTempPath() + @"NAVCommentTool\Objects with Modification\";

        static public void SaveToFiles()
        {
            DirectoryInfo directory = Directory.CreateDirectory(pathObjWithMod);
            saveTool.SaveTool.SetFullPermission(ref directory);
            for (int i = 0; i < modList.Count; i++)
            {
                string modFileName = string.Join("_", modList[i].Split(Path.GetInvalidFileNameChars()));
                string modFilePath = pathObjWithMod + modFileName + ".txt";
                File.AppendAllText(modFilePath, modContentList[i]);
            }
            
            directory = Directory.CreateDirectory(pathModInObj);
            saveTool.SaveTool.SetFullPermission(ref directory);
            for (int i = 0; i < objList.Count; i++)
            {
                string objFileName = string.Join("_", objList[i].Split(Path.GetInvalidFileNameChars()));
                string objFilePath = pathModInObj + objFileName + ".txt";
                File.AppendAllText(objFilePath, objContentList[i]);
            }
            
            File.WriteAllLines(pathAbandoned, abandonedList);
            File.WriteAllLines(pathFoundTag, foundTagList);
            File.WriteAllLines(pathModInTag, modInTagList);
        }

        static public void DeleteFiles()
        {
            if (File.Exists(pathAbandoned))
                File.Delete(pathAbandoned);
            if (File.Exists(pathFoundTag))
                File.Delete(pathFoundTag);
            if (File.Exists(pathModInTag))
                File.Delete(pathModInTag);
            if (Directory.Exists(pathObjWithMod))
                Directory.Delete(pathObjWithMod, true);
            if (Directory.Exists(pathModInObj))
                Directory.Delete(pathModInObj, true);
        }

        static public List<string> GetModificationsInObject(ObjectClass obj)
        {
            string objectHeaderLine = obj.Contents.Substring(0, obj.Contents.IndexOf(Environment.NewLine));
            int idx = objList.FindIndex(x => x == objectHeaderLine);
            return objContentList[idx].Replace("\r", "").Split('\n').ToList();
        }
    }

    
}
