using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Rql;
using MongoDB.Bson;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Rql.MongoDB
{
    public class RqlMongoNamespace : IRqlNamespace
    {
        protected List<RqlMongoCollectionInfo> CollectionInfos { get; set; }

        public RqlMongoNamespace(params Type[] markerTypes) : this(markerTypes.Select(t => t.Assembly).ToArray())
        {
        }

        public RqlMongoNamespace(params Assembly[] assemblies) : base()
        {
            var types = new List<Type>();

            foreach (var assembly in assemblies)
            {
                types.AddRange(assembly.GetTypes().AsEnumerable().Where(t => typeof(IRqlCollection).IsAssignableFrom(t)));
            }

            this.CollectionInfos = new List<RqlMongoCollectionInfo>(types.Count());

            foreach (var type in types)
            {
                var fieldInfos = new List<RqlMongoFieldInfo>();

                object[] attrs = type.GetCustomAttributes(typeof(RqlNameAttribute), true);
                RqlNameAttribute attr = attrs.Length > 0 ? (RqlNameAttribute)attrs[0] : null;

                string rqlName;

                if (attr == null)
                    rqlName = type.Name.ToLower();
                else
                    rqlName = attr.Name.ToLower();

                this.CollectionInfos.Add(new RqlMongoCollectionInfo(this, type.Name, rqlName));
            }
        }

        public string[] GetRqlNames()
        {
            return this.CollectionInfos.Select(c => c.RqlName).ToArray();
        }

        public IRqlCollectionInfo GetCollectionInfoByName(string name)
        {
            return CollectionInfos.Find(c => c.Name == name);
        }

        public IRqlCollectionInfo GetCollectionInfoByRqlName(string rqlName)
        {
            return CollectionInfos.Find(c => c.RqlName == rqlName);
        }
    }
}

