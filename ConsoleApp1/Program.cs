﻿using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\Administrator\Documents\TEUTONIA\ObjAllTeutonia_p1.txt";
            string allMods = ProcessFile.PassAllModificationTags(path, true);
            foreach (string mod in allMods.Split(','))
                Console.WriteLine(mod);

            //Console.WriteLine(ProcessFile.RunProcessing(allMods, path, @"", true, allMods));
            Console.WriteLine(ProcessFile.RunPreview(allMods, path, false));


            Console.WriteLine(Environment.NewLine + "Finish.");
            Console.ReadKey();
        }
    }
}
