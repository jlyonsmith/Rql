using System;

namespace Rql
{
    public interface IRqlFieldInfo
    {
        string Name { get; }
        string RqlName { get; }
        RqlDataType RqlType { get; }
        RqlDataType SubRqlType { get; }
    }
}

