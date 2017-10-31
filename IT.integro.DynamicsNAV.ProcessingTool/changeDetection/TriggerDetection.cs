using System;
using System.Collections.Generic;
using System.Linq;

namespace IT.integro.DynamicsNAV.ProcessingTool.changeDetection
{

    class TriggerDetection
    {
        private static List<String> triggers;
        private static List<String> memberTriggers;
        private static List<String> uniqueTriggers;

        static TriggerDetection()
        {
            uniqueTriggers = new List<string>
            {
                // Codeunit
                "OnBeforeTestRun",
                "OnAfterTestRun",
                // Form
                "OnInit",
                "OnOpenForm",
                "OnQueryCloseForm",
                "OnCloseForm",
                "OnActivateForm",
                "OnDeactivateForm",
                "OnFindRecord",
                "OnNextRecord",
                "OnAfterGetRecord",
                "OnAfterGetCurrRecord",
                "OnBeforePutRecord",
                "OnNewRecord",
                "OnInsertRecord",
                "OnModifyRecord",
                "OnDeleteRecord",
                "OnTimer",
                "OnCreateHyperlink",
                "OnHyperlink",
                // DataPort
                "OnAfterExportRecord",
                "OnAfterFormatField",
                "OnAfterImportRecord",
                "OnBeforeEvaluateField",
                "OnBeforeExportRecord",
                "OnBeforeImportRecord",
                "OnInitDataPort",
                "OnPostDataPort",
                "OnPreDataPort",
                // Report
                "OnInitReport",
                "OnPreReport",
                "OnPostReport",
                "OnCreateHyperlink",
                "OnHyperlink",
                // Data Item
                "OnPreDataItem",
                "OnAfterGetRecord",
                "OnPostDataItem",
                // Section
                "OnPreSection",
                "OnPostSection",
                // Table
                "OnInsert",
                "OnModify",
                "OnDelete",
                "OnRename",
                // XMLPort
                "OnAfterAssignField",
                "OnAfterAssignVariable",
                "OnAfterGetField",
                "OnAfterGetRecord",
                "OnAfterInitRecord",
                "OnAfterInsertRecord",
                "OnBeforeInsertRecord",
                "OnBeforePassField",
                "OnBeforePassVariable",
                "OnInitXMLport",
                "OnPreXMLport",
                "OnPostXMLport",
                "OnPreXMLItem",
                // Page
                "OnInit",
                "OnOpenPage",
                "OnClosePage",
                "OnFindRecord",
                "OnNextRecord",
                "OnAfterGetRecord",
                "OnNewRecord",
                "OnInsertRecord",
                "OnModifyRecord",
                "OnDeleteRecord",
                "OnQueryClosePage",
                // Codeunit
                "OnBeforeTestRun",
                "OnAfterTestRun"
            };

            memberTriggers = new List<string>
            {
                // Form Control
                "OnActivate",
                "OnDeactivate",
                "OnFormat",
                "OnBeforeInput",
                "OnInputChange",
                "OnAfterInput",
                "OnPush",
                "OnValidate",
                "OnAfterValidate",
                "OnLookup",
                "OnDrillDown",
                "OnAssistEdit",
                "OnActivate",
                "OnDeactivate",
                "OnFormat",
                "OnBeforeInput",
                "OnInputChange",
                // Table (Fields)
                "OnValidate",
                "OnLookup",
                // Page (Fields)
                "OnValidate",
                "OnLookup",
                "OnDrillDown",
                "OnAssistEdit",
                "OnControlAddin",
                // Page (Action)
                "OnAction"
            };

            triggers = uniqueTriggers.Union(memberTriggers).ToList();
            triggers.Add("PROCEDURE");
        }

        static public bool DetectIfAnyTriggerInLine(string line, bool memberTrigger = false)
        {
            if (!memberTrigger)
            {
                foreach (var trigger in triggers)
                {
                    if (DetectIfSpecifiedTriggerInLine(trigger, line))
                        return true;
                }
            }
            else
            {
                foreach (var trigger in memberTriggers)
                {
                    if (DetectIfSpecifiedTriggerInLine(trigger, line))
                        return true;
                }
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
