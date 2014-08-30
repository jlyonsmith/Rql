using System;
using System.Collections.Generic;
using System.Linq;

namespace Rql
{
    public enum FieldSpecPresence
    {
        Included,
        Excluded
    }

    public class FieldSpecField
    {
        public string Name { get; private set; }
        public FieldSpecPresence Presence { get; private set; }

        public FieldSpecField(string name, FieldSpecPresence presence)
        {
            this.Name = name;
            this.Presence = presence;
        }
    }

    public class FieldSpec
    {
        public FieldSpecField[] Fields { get; set; }

        public FieldSpec(IEnumerable<FieldSpecField> fields)
        {
            this.Fields = fields.ToArray();
        }
    }
}

