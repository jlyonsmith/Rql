using System;
using System.Collections.Generic;
using System.Linq;

namespace Rql
{
    public abstract class RqlNamespace : IRqlNamespace
    {
        protected Dictionary<string, RqlCollectionInfo> CollectionInfos { get; set; }
        protected Dictionary<Type, string> CollectionTypes { get; set; }

        public RqlNamespace()
        {
        }

        public string[] GetCollectionNames()
        {
            return this.CollectionInfos.Keys.ToArray();
        }

        public RqlCollectionInfo GetCollectionInfo(Type type)
        {
            string collectionName;

            if (this.CollectionTypes.TryGetValue(type, out collectionName))
                return GetCollectionInfo(collectionName);
            else
                return null;
        }

        public RqlCollectionInfo GetCollectionInfo(string collectionName)
        {
            RqlCollectionInfo collectionInfo;

            if (this.CollectionInfos.TryGetValue(collectionName, out collectionInfo))
                return collectionInfo;
            else
                return null;
        }
    }
}

