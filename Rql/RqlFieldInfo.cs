using System;

namespace Rql
{
    public class RqlFieldInfo
    {
        public RqlFieldInfo() { }

        public RqlFieldInfo(string name, RqlDataType rqlType)
        {
            this.Name = name;
            this.RqlType = rqlType;
            this.SubRqlType = RqlDataType.None;
        }

        public RqlFieldInfo(string name, RqlDataType rqlType, RqlDataType subRqlType)
        {
            this.Name = name;
            this.RqlType = rqlType;
            this.SubRqlType = subRqlType;
        }

        public string Name { get; private set; }
        public RqlDataType RqlType { get; private set; }
        public RqlDataType SubRqlType { get; private set; }
    }
}

