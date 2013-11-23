using System;

namespace Rql
{
    public class RqlMongoFieldInfo : IRqlFieldInfo
    {
        public RqlMongoFieldInfo() { }

        public RqlMongoFieldInfo(string name, string rqlName)
        {
            this.Name = name;
            this.RqlName = rqlName;
            this.RqlType = RqlDataType.None;
            this.SubRqlType = RqlDataType.None;
        }

        public string Name { get; private set; }
        public string RqlName { get; private set; }
        public RqlDataType RqlType { get; private set; }
        public RqlDataType SubRqlType { get; private set; }
    }
}

