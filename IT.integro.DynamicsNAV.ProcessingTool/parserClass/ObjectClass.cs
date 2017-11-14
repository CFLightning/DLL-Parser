using System.Collections.Generic;

namespace IT.integro.DynamicsNAV.ProcessingTool.parserClass
{
    class ObjectClass
    {
        private string header;
        private int number;
        private string type;
        private string name;
        private string contents;
        public List<ChangeClass> changelog;

        public ObjectClass()
        {
            header = "";
            number = 0;
            type = "";
            name = "";
            contents = "";
            changelog = new List<ChangeClass>();
        }

        public ObjectClass(string header, int number, string type, string name, string contents)
        {
            this.header = header;
            this.number = number;
            this.type = type;
            this.name = name;
            this.contents = contents;
            changelog = new List<ChangeClass>();
        }

        public string Header { get => header; set => header = value; }
        public int Number { get => number; set => number = value; }
        public string Type { get => type; set => type = value; }
        public string Name { get => name; set => name = value; }
        public string Contents { get => contents; set => contents = value; }
        internal List<ChangeClass> Changelog { get => changelog; set => changelog = value; }
    }
}
