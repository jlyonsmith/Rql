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

        public SortSpecParserException(string message, Exception innerException) : base(message, innerException)
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
                RqlFunctionCallExpression funcExp = null;

                try
                {
                    funcExp = new RqlParser().Parse(part) as RqlFunctionCallExpression;
                }
                catch (RqlParseException e)
                {
                    throw new SortSpecParserException("Sort specification must be of the form field(...)", e);
                }

                if (funcExp.Arguments.Count != 1)
                    throw new SortSpecParserException("Sort specifications must have exactly one argument");

                RqlConstantExpression constExp = funcExp.Arguments[0] as RqlConstantExpression;

                if (constExp == null || !(constExp.Value is Int32) || ((int)constExp.Value != 1 && (int)constExp.Value != -1))
                    throw new SortSpecParserException("Sort specification value must be 1 or -1");

                fields.Add(new SortSpecField(funcExp.Name, (int)constExp.Value == 1 ? SortSpecSortOrder.Ascending : SortSpecSortOrder.Descending));
            }

            return new SortSpec(fields);
        }
    }
}

