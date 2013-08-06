using System;
using System.Collections.Generic;

namespace Rql
{
    // TODO: Need unit tests for this class

    public class SortSpecParserException : Exception
    {
        public SortSpecParserException(string message) : base(message)
        {
        }
    }

    public class SortSpecParser
    {
        private string input;

        public SortSpecParser()
        {
        }

        public SortSpec Parse(string sortSpec)
        {
            this.input = sortSpec;

            string[] parts = input.Split(',');
            var fields = new List<SortSpecField>();

            foreach (var part in parts)
            {
                RqlFunctionCallExpression funcExp = new RqlParser().Parse(part) as RqlFunctionCallExpression;

                if (funcExp == null || funcExp.Arguments.Count != 1)
                    throw new SortSpecParserException("Sort specifications must be of the form field(...)");

                RqlConstantExpression constExp = funcExp.Arguments[0] as RqlConstantExpression;

                if (constExp == null || !(constExp.Value is Int32))
                    throw new SortSpecParserException("Sort specification direction must be 1 or -1");

                fields.Add(new SortSpecField(funcExp.Name, (int)constExp.Value == 1 ? SortSpecSortOrder.Ascending : SortSpecSortOrder.Descending));
            }

            return new SortSpec(fields);
        }
    }
}

