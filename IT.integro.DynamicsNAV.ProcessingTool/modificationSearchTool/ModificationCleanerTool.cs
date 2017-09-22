using IT.integro.DynamicsNAV.ProcessingTool.parserClass;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;
using System.IO;
using System.Text;

namespace IT.integro.DynamicsNAV.ProcessingTool.modificationSearchTool
{
    public class ModificationCleanerTool
    {
        public static bool CleanChangeCode()
        {
            foreach (ChangeClass chg in ChangeClassRepository.changeRepository)
            {
                StringReader reader = new StringReader(chg.Contents);
                StringBuilder builder = new StringBuilder();
                StringWriter writer = new StringWriter(builder);
                string line;
                bool isFirstLine = true;
                int firstLineIndent = 0;

                while (null != (line = reader.ReadLine()))
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        firstLineIndent = line.Length - line.TrimStart(' ').Length;
                    }
                    if (line.Length > firstLineIndent)
                    {
                        line = line.Substring(firstLineIndent);
                    }
                    writer.WriteLine(line);
                }

                chg.Contents = builder.ToString();

                writer.Close();
                builder = new StringBuilder();
                writer = new StringWriter(builder);
            }
            return true;
        }
    }
}
