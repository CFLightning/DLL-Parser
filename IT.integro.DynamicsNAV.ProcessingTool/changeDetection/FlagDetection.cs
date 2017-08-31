using System.Text.RegularExpressions;

namespace IT.integro.DynamicsNAV.ProcessingTool.changeDetection
{
    class FlagDetection
    {
        static Regex rgxField;

        static FlagDetection()
        {
            rgxField = new Regex(@"^\s*{(?<FieldNumber>[^;]*);[^;]*;(?<FieldName>[^;]*)\s*;([^;]*;)?(.*=)?.*$");
        }

        static public bool DetectIfNextFieldFlag(string codeLine)
        {
            return rgxField.IsMatch(codeLine);
        }

        static public string GetNextFieldName(string codeLine)
        {
            Match match = rgxField.Match(codeLine);
            return match.Groups["FieldName"].Value.Trim(' ');
        }

        static public string GetNextFieldNumber(string codeLine)
        {
            Match match = rgxField.Match(codeLine);
            return match.Groups["FieldNumber"].Value.Trim(' ');
        }

        static public bool DetectIfFieldsStartFlag(string codeLine)
        {
            if (codeLine == "  FIELDS")
                return true;
            else
                return false;
        }

        static public bool DetectIfFieldsEndFlag(string codeLine)
        {
            if (codeLine == "  KEYS")
                return true;
            else
                return false;
        }

        static public bool DetectIfActionStartFlag(string codeLine)
        {
            if (codeLine == "    ActionList=ACTIONS")
                return true;
            else
                return false;
        }

        static public bool DetectIfActionEndFlag(string codeLine)
        {
            if (codeLine == "  CONTROLS")
                return true;
            else
                return false;
        }

        static public bool DetectIfControlStartFlag(string codeLine)
        {
            if (codeLine == "  CONTROLS")
                return true;
            else
                return false;
        }

        static public bool DetectIfControlEndFlag(string codeLine)
        {
            if (codeLine == "  CODE")
                return true;
            else
                return false;
        }

        static public string GetSourceExpr(string codeLine)
        {
            string SourceExpr = codeLine.Substring((codeLine.IndexOf("SourceExpr=") + "SourceExpr=".Length));
            SourceExpr = SourceExpr.Remove(SourceExpr.Length - 1).Trim(' ');
            return SourceExpr;
        }

        static public string GetDescription(string codeLine)
        {
            string fieldDescription = codeLine.Substring((codeLine.IndexOf("Description=") + "Description=".Length));
            fieldDescription = fieldDescription.Remove(fieldDescription.Length - 1).Trim(' ');
            return fieldDescription;
        }
    }
}
