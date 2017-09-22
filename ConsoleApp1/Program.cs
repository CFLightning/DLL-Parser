using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string allMods = ProcessFile.PassAllModificationTags(@"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt");
            Console.WriteLine(ProcessFile.RunProcessing(allMods, @"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt", @""));

            //Console.WriteLine(ProcessFile.RunPreview(allMods, @"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt"));

            //Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
