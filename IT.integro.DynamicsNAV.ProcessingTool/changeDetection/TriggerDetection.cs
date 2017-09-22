using System;
using System.Collections.Generic;
using System.Linq;

namespace IT.integro.DynamicsNAV.ProcessingTool.changeDetection
{

    class TriggerDetection
    {
        private static List<String> triggers;

        static TriggerDetection()
        {
            triggers = new List<string>
            {
                "OnInit",
                "OnRun",
                "OnOpenPage",
                "OnClosePage",
                "OnFindRecord",
                "OnNextRecord",
                "OnAfterGetRecord",
                "OnNewRecord",
                "OnInsert",
                "OnModify",
                "OnDelete",
                "OnInsertRecord",
                "OnModifyRecord",
                "OnDeleteRecord",
                "OnQueryClosePage",
                "OnAfterGetCurrRecord",
                "OnPreDataItem",
                "OnPostDataItem",
                "OnValidate",
                "OnLookup",
                "OnDrillDown",
                "OnAssistEdit",
                "OnControlAddin",
                "OnAction",
                "PROCEDURE"
            };
        }

        static public bool DetectIfAnyTriggerInLine(string line)
        {
            foreach (var trigger in triggers)
            {
                if (DetectIfSpecifiedTriggerInLine(trigger, line))
                    return true;
            }
            return false;
        }

        static public bool DetectIfSpecifiedTriggerInLine(string trigger, string line)
        {
            line = line.Trim(' ');
            if (line == trigger + "=VAR" || line == trigger + "=BEGIN" || DetectifProcedureTriggerInLine(trigger, line))
            {
                return true;
            }
            return false;
        }

        static private bool DetectifProcedureTriggerInLine(string trigger, string line)
        {
            if (line.Contains("PROCEDURE ") && trigger == "PROCEDURE")
                return true;
            else
                return false;
        }

        static public string GetTriggerName(string triggerLine)
        {
            foreach (var trigger in triggers)
            {
                if (DetectIfSpecifiedTriggerInLine(trigger, triggerLine))
                {
                    if (trigger == "PROCEDURE")
                    {
                        return GetProcedureName(triggerLine);
                    }
                    return trigger;
                }
            }
            return "";
        }

        static private string GetProcedureName(string triggerLine)
        {
            string[] split = triggerLine.Split(new string[] { " ", "@" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Count(); i++)
            {
                if (split[i] == "PROCEDURE")
                {
                    return split[i] + " " + split[i + 1];
                }
            }
            return "";
        }

    }

}
