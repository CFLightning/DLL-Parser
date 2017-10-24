using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.integro.DynamicsNAV.ProcessingTool.repositories
{
    static class TagRepository
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
        static public bool flagReport;
        static public bool flagRDLDATA;

        static string pathFullTagList = Path.GetTempPath() + @"NAVCommentTool\Full tag list.txt";
        static string pathTagList = Path.GetTempPath() + @"NAVCommentTool\Tag list.txt";
        static string pathAbandoned = Path.GetTempPath() + @"NAVCommentTool\Abandoned comments.txt";
        static public string pathModInObj = Path.GetTempPath() + @"NAVCommentTool\Modifications in Object\";
        static public string pathObjWithMod = Path.GetTempPath() + @"NAVCommentTool\Objects with Modification\";

        static public List<string> GetModObjectList(string mod)
        {
            return TagRepository.fullTagList.Where(w => w.mod == mod).Select(t => t.inObject).Distinct().ToList();
        }

        static public List<string> GetAllObjectList()
        {
            return TagRepository.fullTagList.Where(r => r.mod != null).Select(t => t.inObject).Distinct().ToList();
        }

        static public List<string> GetObjectModList(string inObject)
        {
            return TagRepository.fullTagList.Where(w => w.inObject == inObject && w.mod != null).Select(t => t.mod).Distinct().ToList();
        }

        static public List<string> GetAllModList()
        {
            return TagRepository.fullTagList.Where(r => r.mod != null).Select(t => t.mod).Distinct().ToList();
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
            //string[] textTag = TagRepository.fullTagList.Where(r => r.mod != null).Select(t => t.comment + separator + t.mod).ToArray();
            //File.WriteAllLines(pathTagList, textTag);
                //  Abandoned Comments
            //string[] textAbandonedComments = TagRepository.fullTagList.Where(r => r.mod == null).Select(t => t.inObject + separator + t.comment).ToArray();
            //File.WriteAllLines(pathAbandoned, textAbandonedComments);
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

        static public void DeleteFiles()
        {
            if (File.Exists(pathAbandoned))
                File.Delete(pathAbandoned);
            if (Directory.Exists(pathObjWithMod))
                Directory.Delete(pathObjWithMod, true);
            if (Directory.Exists(pathModInObj))
                Directory.Delete(pathModInObj, true);
        }

        static public void ClearRepo(bool KillTags)
        {
            tagObject = string.Empty;
            lineNo = 0;
            flagReport = false;
            flagRDLDATA = false;
            if (KillTags)
                fullTagList.Clear();
        }

        static public bool CheckIfReport()
        {
            if (tagObject.StartsWith("OBJECT Report"))
                return true;
            return false;
        }
    }
}
