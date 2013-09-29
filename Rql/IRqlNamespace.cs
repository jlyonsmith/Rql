using System;

namespace Rql
{
    public interface IRqlNamespace
    {
        string[] GetCollectionNames();
        RqlCollectionInfo GetCollectionInfo(Type type);
        RqlCollectionInfo GetCollectionInfo(string collectionName);
    }
}

