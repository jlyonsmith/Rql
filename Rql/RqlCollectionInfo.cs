using System;
using System.Collections.Generic;
using System.Linq;

namespace Rql
{
    public class RqlCollectionInfo
    {
        private Dictionary<string, RqlFieldInfo> FieldInfos { get; set; }

        public RqlCollectionInfo(IRqlNamespace rqlNamespace, string name, Dictionary<string, RqlFieldInfo> fieldInfos)
        {
            this.Name = name;
            this.FieldInfos = fieldInfos;
            this.RqlNamespace = rqlNamespace;
        }

        public string Name { get; private set;}
        public IRqlNamespace RqlNamespace { get; private set; }

        public string[] GetFieldInfoNames()
        {
            return this.FieldInfos.Keys.ToArray();
        }

        public RqlFieldInfo GetFieldInfo(string name)
        {
            RqlFieldInfo fieldInfo;

            if (FieldInfos.TryGetValue(name, out fieldInfo))
                return fieldInfo;
            else
                return null;
        }
    }
}

