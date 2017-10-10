using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string inPath = @"C:\Users\Administrator\Documents\TEUTONIA\ObjAllTeutonia_p1.txt";
            //string path = @"C:\FILES\Object.txt";

                //  PassAllModifications
            string allMods = ProcessFile.PassAllModificationTags(inPath, true);
            //foreach (string mod in allMods.Split(','))
            //    Console.WriteLine(mod);
            //Console.WriteLine(Environment.NewLine);

                //  Merge
            string mergeString = "IT|>|-TI|#|FX02|>|-FFXX|#|";
            string outName = "";
            ProcessFile.MergeTags(mergeString, inPath);
            inPath = @"C:\FILES\Object(Merged).txt";
            allMods = ProcessFile.PassAllModificationTags(inPath, true);
            foreach (string mod in allMods.Split(','))
                Console.WriteLine(mod);
                
                //  RunProcessing
            //Console.WriteLine(ProcessFile.RunProcessing(allMods, path, @"", true, allMods));

                //  RunPreview
            //Console.WriteLine(ProcessFile.RunPreview(allMods, path, false));


            Console.WriteLine(Environment.NewLine + "Finish.");
            Console.ReadKey();
        }
    }
}
