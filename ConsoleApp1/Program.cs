﻿using IT.integro.DynamicsNAV.ProcessingTool;
using IT.integro.DynamicsNAV.ProcessingTool.merge;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //string inPath = @"C:\Users\Administrator\Documents\FARUTEX\LIVE_ALL_17_10_2017.txt";
            //string inPath = @"C:\Users\Administrator\Documents\TEUTONIA\ObjAllTeutonia_p1.txt";
            string inPath = @"c:\files\burdatest.txt";
            //string inPath = @"C:\Users\Administrator\AppData\Local\Temp\2\NAVCommentTool\Objects\Table 5 FinanceChargeTerms.txt";

                //      PassAllModifications
                //string allMods = ProcessFile.PassAllModificationTags(inPath, true);
            string allMods = ProcessFile.PassAllModificationTagsProcess(inPath, true);
            foreach (string mod in allMods.Split(','))
                Console.WriteLine(mod);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("###########################################################");
            Console.WriteLine(Environment.NewLine);

            //      Merge
            string mergeString = "026|>|926|#|031|>|931|#|006|>|906|#|ITBDP03|>|ABC|#|ITRKG100|>|ABC2|#|NAV2015PL/000|>|NAV2015";
            string outPath = @"C:\FILES\burda2.txt";
            ProcessFile.RunMergeProcess(mergeString, inPath, outPath);

            inPath = outPath;
            allMods = ProcessFile.PassAllModificationTagsProcess(inPath, true);
            foreach (string mod in allMods.Split(','))
                Console.WriteLine(mod);

            //      RunProcessing
            //Console.WriteLine(ProcessFile.RunProcessing(allMods, inPath, @"", true, allMods));

            //      RunPreview
            Console.WriteLine(ProcessFile.RunPreview(allMods, inPath, false));


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
