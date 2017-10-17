using IT.integro.DynamicsNAV.ProcessingTool;
using IT.integro.DynamicsNAV.ProcessingTool.merge;
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
            //foreach (string mod in allMods.Split(','))
            //    Console.WriteLine(mod);
            //Console.WriteLine(Environment.NewLine);

            //  Merge
            string mergeString = "001|>|MERGE001|#|192|>|MERGE192|#|010|>|MERGE010|#|NAV2015PL/000|>|MERGENAV2015PL/000|#|010|>|M010|#|022|>|M022|#|017|>|M017";
            string outPath = @"C:\FILES\out2.txt";
            WatchStep();
            WatchStep("Per change");
            ProcessFile.RunMergeProcess(mergeString, inPath, outPath);
            WatchStep("By line");
            inPath = outPath;
            allMods = ProcessFile.PassAllModificationTags(inPath, true);
            foreach (string mod in allMods.Split(','))
                Console.WriteLine(mod);

            //  RunProcessing
            //Console.WriteLine(ProcessFile.RunProcessing(allMods, inPath, @"", true, allMods));

            //  RunPreview
            // Console.WriteLine(ProcessFile.RunPreview(allMods, inPath, false));


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
