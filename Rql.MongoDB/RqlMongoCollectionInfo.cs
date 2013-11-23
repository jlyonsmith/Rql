using System;
using System.Collections.Generic;
using System.Linq;

namespace Rql.MongoDB
{
    public class RqlMongoCollectionInfo : IRqlCollectionInfo
    {
        public RqlMongoCollectionInfo(IRqlNamespace rqlNamespace, string name, string rqlName)
        {
            this.Name = name;
            this.RqlName = rqlName;
            this.RqlNamespace = rqlNamespace;
        }

        public string Name { get; private set;}
        public string RqlName { get; private set;}
        public IRqlNamespace RqlNamespace { get; private set; }

        public string[] GetFieldInfoNames()
        {
            throw new NotSupportedException("Cannot get list of field names with MongoDB");
        }

        public virtual IRqlFieldInfo GetFieldInfoByRqlName(string rqlName)
        {
            string name = (rqlName == "id" ? "_id" : rqlName);

            return new RqlMongoFieldInfo(name, rqlName);
        }

        public virtual IRqlFieldInfo GetFieldInfoByName(string name)
        {
            string rqlName = (name == "_id" ? "id" : name);

            return new RqlMongoFieldInfo(name, rqlName);
        }

        private List<RqlMongoFieldInfo> FieldInfos { get; set; }

        public string[] GetRqlNames()
        {
            return this.FieldInfos.Select(f => f.RqlName).ToArray();
        }
    }
}

