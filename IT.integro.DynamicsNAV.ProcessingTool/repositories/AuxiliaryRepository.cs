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
        public struct Tags
        {
            public string comment;
            public string mod;
            public string inObject;
            public int inLine;
        }

        static public List<Tags> fullTagList = new List<Tags>();
        static public string tagObject;
        static public int lineNo = 0;

        //static public List<string> abandonedList = new List<string>();
        //static public List<string> foundTagList = new List<string>();
        //static public List<string> modInTagList = new List<string>();
        //static public List<int> tagLineList = new List<int>();
        //static public List<string> modContentList = new List<string>();
        //static public List<string> modList = new List<string>();
        //static public List<string> objContentList = new List<string>();
        //static public List<string> objList = new List<string>();

        static string pathFullTagList = Path.GetTempPath() + @"NAVCommentTool\Full tag list.txt";
        static string pathTagList = Path.GetTempPath() + @"NAVCommentTool\Tag list.txt";
        static string pathAbandoned = Path.GetTempPath() + @"NAVCommentTool\Abandoned comments.txt";
        //static string pathFoundTag = Path.GetTempPath() + @"NAVCommentTool\tagList.txt";
        //static string pathModInTag = Path.GetTempPath() + @"NAVCommentTool\tagModList.txt";
        //static string pathTagLine = Path.GetTempPath() + @"NAVCommentTool\tagLineList.txt";
        static public string pathModInObj = Path.GetTempPath() + @"NAVCommentTool\Modifications in Object\";
        static public string pathObjWithMod = Path.GetTempPath() + @"NAVCommentTool\Objects with Modification\";

        static public List<string> GetTagModList()
        {
            return AuxiliaryRepository.fullTagList.Where(r => r.mod != null).Select(t => t.mod).ToList();
        }

        static public List<string> GetTagList()
        {
            return AuxiliaryRepository.fullTagList.Where(r => r.mod != null).Select(t => t.comment).ToList();
        }

        static public List<string> GetAbandonedCommentList()
        {
            return AuxiliaryRepository.fullTagList.Where(r => r.mod == null).Select(t => t.comment).ToList();
        }

        static public List<string> GetAllCommentList()
        {
            return AuxiliaryRepository.fullTagList.Select(t => t.comment).ToList();
        }

        static public List<string> GetModObjectList(string mod)
        {
            return AuxiliaryRepository.fullTagList.Where(w => w.mod == mod).Select(t => t.inObject).Distinct().ToList();
        }

        static public List<string> GetAllObjectList()
        {
            return AuxiliaryRepository.fullTagList.Where(r => r.mod != null).Select(t => t.inObject).Distinct().ToList();
        }

        static public List<string> GetObjectModList(string inObject)
        {
            return AuxiliaryRepository.fullTagList.Where(w => w.inObject == inObject && w.mod != null).Select(t => t.mod).Distinct().ToList();
        }

        static public List<string> GetAllModList()
        {
            return AuxiliaryRepository.fullTagList.Where(r => r.mod != null).Select(t => t.mod).Distinct().ToList();
        }

        static public void SaveToFilesFull()
        {
            DirectoryInfo directory = Directory.CreateDirectory(pathObjWithMod);
            saveTool.SaveTool.SetFullPermission(ref directory);
            directory = Directory.CreateDirectory(pathModInObj);
            saveTool.SaveTool.SetFullPermission(ref directory);

            string separator = "|#|";
            //  Full Repository
            string[] textFullTag = fullTagList.Select(s => s.inLine + separator + s.inObject + separator + s.comment + separator + s.mod).ToArray();
            File.WriteAllLines(pathFullTagList, textFullTag);
            //  Found Tags + Mods
            string[] textTag = AuxiliaryRepository.fullTagList.Where(r => r.mod != null).Select(t => t.comment + separator + t.mod).ToArray();
            File.WriteAllLines(pathTagList, textTag);
            //  Abandoned Comments
            string[] textAbandonedComments = AuxiliaryRepository.fullTagList.Where(r => r.mod == null).Select(t => t.inObject + separator + t.comment).ToArray();
            File.WriteAllLines(pathAbandoned, textAbandonedComments);
            //  Mod per Object
            foreach (var obj in GetAllObjectList())
            {
                string objFileName = string.Join("_", obj.Split(Path.GetInvalidFileNameChars()));
                string objFilePath = pathModInObj + objFileName + ".txt";
                string[] textObjectMod = GetObjectModList(obj).ToArray();
                File.AppendAllLines(objFilePath, textObjectMod);
            }
            // Object per Mod
            foreach (var mod in GetAllModList()) 
            {
                string modFileName = string.Join("_", mod.Split(Path.GetInvalidFileNameChars()));
                string modFilePath = pathObjWithMod + modFileName + ".txt";
                string[] textModObject = GetModObjectList(mod).ToArray();
                File.AppendAllLines(modFilePath, textModObject);
            }
        }

        //static public void SaveToFiles()
        //{
        //    DirectoryInfo directory = Directory.CreateDirectory(pathObjWithMod);
        //    saveTool.SaveTool.SetFullPermission(ref directory);
        //    for (int i = 0; i < modList.Count; i++)
        //    {
        //        string modFileName = string.Join("_", modList[i].Split(Path.GetInvalidFileNameChars()));
        //        string modFilePath = pathObjWithMod + modFileName + ".txt";
        //        File.AppendAllText(modFilePath, modContentList[i]);
        //    }
            
        //    directory = Directory.CreateDirectory(pathModInObj);
        //    saveTool.SaveTool.SetFullPermission(ref directory);
        //    for (int i = 0; i < objList.Count; i++)
        //    {
        //        string objFileName = string.Join("_", objList[i].Split(Path.GetInvalidFileNameChars()));
        //        string objFilePath = pathModInObj + objFileName + ".txt";
        //        File.AppendAllText(objFilePath, objContentList[i]);
        //    }
            
        //    File.WriteAllLines(pathAbandoned, abandonedList);
        //    File.WriteAllLines(pathFoundTag, foundTagList);
        //    File.WriteAllLines(pathModInTag, modInTagList);
        //    File.WriteAllLines(pathTagLine, tagLineList.ConvertAll<string>(delegate (int i)
        //    {
        //        return i.ToString();
        //    }));
        //}

        static public void DeleteFiles()
        {
            if (File.Exists(pathAbandoned))
                File.Delete(pathAbandoned);
            //if (File.Exists(pathFoundTag))
            //    File.Delete(pathFoundTag);
            //if (File.Exists(pathModInTag))
            //    File.Delete(pathModInTag);
            //if (File.Exists(pathTagLine))
            //    File.Delete(pathTagLine);
            if (Directory.Exists(pathObjWithMod))
                Directory.Delete(pathObjWithMod, true);
            if (Directory.Exists(pathModInObj))
                Directory.Delete(pathModInObj, true);
        }

        //static public List<string> GetModificationsInObject(ObjectClass obj)
        //{
        //    string objectHeaderLine = obj.Contents.Substring(0, obj.Contents.IndexOf(Environment.NewLine));
        //    int idx = objList.FindIndex(x => x == objectHeaderLine);
        //    return objContentList[idx].Replace("\r", "").Split('\n').ToList();
        //}

        static public void ClearRepo()
        {
            //abandonedList.Clear();
            //foundTagList.Clear();
            //modInTagList.Clear();
            //tagLineList.Clear();
            //modContentList.Clear();
            //modList.Clear();
            //objContentList.Clear();
            //objList.Clear();

            fullTagList.Clear();
            tagObject = string.Empty;
            lineNo = 0;
        }
    }
}
