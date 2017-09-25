using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //string allMods = ProcessFile.PassAllModificationTags(@"C:\Users\Administrator\Documents\ObjAllTeutonia_All.txt", true);
            string path = @"C:\Users\Administrator\Documents\TEUTONIA\ObjAllTeutonia_p1.txt";
            string allMods = ProcessFile.PassAllModificationTags(path, true);
            Console.WriteLine(ProcessFile.RunProcessing(allMods, path, @"", true, allMods));

            foreach(string mod in allMods.Split(','))
            {
                Console.WriteLine(mod + Environment.NewLine);
            }
           
            //Console.WriteLine(ProcessFile.RunProcessing(allMods, @"C:\Users\Administrator\Documents\ObjAllTeutonia_All.txt", @"", true));

            //Console.WriteLine(ProcessFile.RunPreview(allMods, @"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt", true));

            //Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
