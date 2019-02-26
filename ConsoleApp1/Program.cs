using IT.integro.DynamicsNAV.ProcessingTool;
using IT.integro.DynamicsNAV.ProcessingTool.merge;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string doc = @"1<next>8<next>FADepreciationBooksSubform<next>#ITAA6.0#<next>#ITAA6.0#%$%2<next>8<next>FADepreciationBooksSubform<next>#ITAA6.0#<next>- New Actions%$%3<next>8<next>FADepreciationBooks-I<next>#ITAA6.0#<next>#ITAA6.0#%$%4<next>8<next>FADepreciationBooks-I<next>#ITAA6.0#<next>- New Actions";

            //string inPath = @"C:\Users\Administrator\Documents\FARUTEX\LIVE_ALL_17_10_2017.txt";
            //string inPath = @"C:\Users\Administrator\Documents\TEUTONIA\ObjAllTeutonia_p1.txt";
            string inPath = @"C:\Users\michal.dlugosz\Desktop\dttest.txt";
            //string inPath = @"C:\Users\Administrator\AppData\Local\Temp\2\NAVCommentTool\Objects\Table 5 FinanceChargeTerms.txt";

                //      PassAllModifications
                //string allMods = ProcessFile.PassAllModificationTags(inPath, true);
            string allMods = ProcessFile.PassAllModificationTagsProcess(inPath, true);
            Console.WriteLine(allMods);
            //foreach (string mod in allMods.Split(','))
            //    Console.WriteLine(mod);
            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("###########################################################");
            //Console.WriteLine(Environment.NewLine);

            //      Merge
            //string mergeString = "ITBDP|>|ABC";
            //string outPath = @"C:\FILES\burda2.txt";
            //ProcessFile.RunMergeProcess(mergeString, inPath, outPath);

            //inPath = outPath;
            //allMods = ProcessFile.PassAllModificationTagsProcess(inPath, true);
            //foreach (string mod in allMods.Split(','))
            //    Console.WriteLine(mod);

            //      RunProcessing
            Console.WriteLine(ProcessFile.RunProcessing(allMods, inPath, @"C:\mapping.csv", true, allMods));

            //      RunPreview
            //Console.WriteLine(ProcessFile.RunPreview(allMods, inPath, false));
            Console.Write(PlainMapper.MapDocumentationToFile(doc, @"C:\mapping.csv"));

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
