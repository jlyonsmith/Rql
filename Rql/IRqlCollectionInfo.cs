using System;

namespace Rql
{
    public interface IRqlCollectionInfo
    {
        string Name { get; }
        string RqlName { get; }
        IRqlNamespace RqlNamespace { get; }
        string[] GetRqlNames();
        IRqlFieldInfo GetFieldInfoByName(string name);
        IRqlFieldInfo GetFieldInfoByRqlName(string rqlName);
    }
}

