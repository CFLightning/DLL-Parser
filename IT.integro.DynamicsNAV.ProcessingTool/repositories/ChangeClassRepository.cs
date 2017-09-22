using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using System.Collections.Generic;

namespace IT.integro.DynamicsNAV.ProcessingTool.repositories
{
    class ChangeClassRepository
    {
        public static List<ChangeClass> changeRepository = new List<ChangeClass>();

        public static void AppendChange(ChangeClass newChange)
        {
            changeRepository.Add(newChange);
        }
    }
}
