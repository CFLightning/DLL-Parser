using IT.integro.DynamicsNAV.ProcessingTool;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(ProcessFile.RunProcessing("MF100sdfsd", @"C:\ExportedObjectsNAVcust.txt", @"C:\mapping.csv"));
            //ProcessFile.RunProcessing(@"MF100.2", @"C:\FILES\ITWS_Test_01.08.2017.txt", @"C:\FILES\mapping.csv", @"C:\FILES\Output");
            Console.ReadKey();
        }
    }
}
