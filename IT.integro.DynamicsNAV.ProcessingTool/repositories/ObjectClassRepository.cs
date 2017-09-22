using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using System.Collections.Generic;

namespace IT.integro.DynamicsNAV.ProcessingTool.repositories
{
    class ObjectClassRepository
    {
        public static List<ObjectClass> objectRepository = new List<ObjectClass>();

        public static void AppendObject(ObjectClass newObject)
        {
            objectRepository.Add(newObject);
        }
    }
}
