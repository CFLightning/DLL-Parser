using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(ProcessFile.RunProcessing("048", @"C:\Users\Administrator\Documents\ObjAllTeutonia_p1.txt", @""));
            Console.ReadKey();
        }
    }
}
