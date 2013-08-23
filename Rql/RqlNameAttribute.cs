using System;

namespace Rql
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class RqlNameAttribute : Attribute
    {
        public RqlNameAttribute()
        {
        }

        public RqlNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}

