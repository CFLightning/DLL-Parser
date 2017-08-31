using IT.integro.DynamicsNAV.ProcessingTool;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessFile.RunProcessing("MF100", @"C:\ExportedObjectsNAVcust.txt" , @"C:\mapping.csv", "C:");
            //ProcessFile.RunProcessing(@"MF100.2", @"C:\FILES\ITWS_Test_01.08.2017.txt", @"C:\FILES\mapping.csv", @"C:\FILES\Output");
        }
    }
}
