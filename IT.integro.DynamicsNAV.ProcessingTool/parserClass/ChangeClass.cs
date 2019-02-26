namespace IT.integro.DynamicsNAV.ProcessingTool.parserClass
{
    class ChangeClass
    {
        private string changelogCode;
        private string contents;
        private string changeType;  // code, field, maybe function
        private string location;    //  trigger name or function name
        private string sourceObject;

        public ChangeClass()
        {
        }
        public ChangeClass(string code, string content, string type, string name, string sObject)
        {
            changelogCode = code;
            contents = content;
            changeType = type;
            location = name;
            sourceObject = sObject;
        }
        public string Contents { get { return contents; } set { contents = value; } }
        public string ChangelogCode { get { return changelogCode; } set { changelogCode = value; } }
        public string ChangeType { get { return changeType; } set { changeType = value; } }
        public string Location { get { return location; } set { location = value; } }
        public string SourceObject { get { return sourceObject; } set { sourceObject = value; } }
    }
}
