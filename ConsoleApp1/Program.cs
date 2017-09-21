using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(ProcessFile.RunProcessing("ELSE", @"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt", @""));
            //string allMods = ProcessFile.PassAllModificationTags(@"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt");
            //Console.WriteLine(ProcessFile.RunPreview("ELSE", @"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt"));
            
            //Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
