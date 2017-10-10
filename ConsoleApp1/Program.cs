using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string inPath = @"C:\Users\Administrator\Documents\TEUTONIA\ObjAllTeutonia_p1.txt";
            //string inPath = @"C:\FILES\Object.txt";

                //  PassAllModifications
            string allMods = ProcessFile.PassAllModificationTags(inPath, true);
            foreach (string mod in allMods.Split(','))
                Console.WriteLine(mod);
            Console.WriteLine(Environment.NewLine);

            //  Merge
            string mergeString = "001|>|MERGE001|#|192|>|MERGE192|#|010|>|MERGE010|#|NAV2015PL/000|>|MERGENAV2015PL/000|#|";
            string outName = "";
            WatchStep();
            ProcessFile.MergeTagsPerChange(mergeString, inPath, outName);
            WatchStep("Per change");
            ProcessFile.MergeTagsLineByLine(mergeString, inPath, outName);
            WatchStep("By line");
            //inPath = @"C:\FILES\Object(Merged).txt";
            //allMods = ProcessFile.PassAllModificationTags(inPath, true);
            //foreach (string mod in allMods.Split(','))
            //    Console.WriteLine(mod);

            //  RunProcessing
            //Console.WriteLine(ProcessFile.RunProcessing(allMods, path, @"", true, allMods));

            //  RunPreview
            //Console.WriteLine(ProcessFile.RunPreview(allMods, path, false));


            Console.WriteLine(Environment.NewLine + "Finish.");
            Console.ReadKey();
        }

        static System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        public static void WatchStep(string comment = "")
        {
            if (watch.IsRunning)
            {
                watch.Stop();
                Console.WriteLine(watch.Elapsed.TotalSeconds.ToString() + "\t" + comment);
            }
            watch.Restart();
        }
    }
}
