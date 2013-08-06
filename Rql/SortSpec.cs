using System;
using System.Collections.Generic;
using System.Linq;

namespace Rql
{
    public enum SortSpecSortOrder
    {
        Ascending,
        Descending
    }

    public class SortSpecField
    {
        public string Name { get; private set; }
        public SortSpecSortOrder Order { get; private set; }

        public SortSpecField(string name, SortSpecSortOrder order)
        {
            this.Name = name;
            this.Order = order;
        }
    }

    public class SortSpec
    {
        public SortSpecField[] Fields { get; set; }

        public SortSpec(IEnumerable<SortSpecField> fields)
        {
            this.Fields = fields.ToArray();
        }
    }
}

