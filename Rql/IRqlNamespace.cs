using System;

namespace Rql
{
    public interface IRqlNamespace
    {
        string[] GetRqlNames();
        IRqlCollectionInfo GetCollectionInfoByRqlName(string rqlName);
        IRqlCollectionInfo GetCollectionInfoByName(string name);
    }
}

