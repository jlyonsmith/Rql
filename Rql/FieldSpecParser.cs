using System;
using System.Collections.Generic;

namespace Rql
{
    public class FieldSpecParserException : Exception
    {
        public FieldSpecParserException(string message) : base(message)
        {
        }

        public FieldSpecParserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class FieldSpecParser
    {
        private string input;

        public FieldSpecParser()
        {
        }

        public FieldSpec Parse(string fieldSpec)
        {
            this.input = fieldSpec;

            string[] parts = input.Split(',');
            var fields = new List<FieldSpecField>();

            foreach (var part in parts)
            {
                RqlFunctionCallExpression funcExp = null;

                try
                {
                    funcExp = new RqlParser().Parse(part) as RqlFunctionCallExpression;
                }
                catch (RqlParseException e)
                {
                    throw new FieldSpecParserException("Field specification must be of the form field(...)", e);
                }

                if (funcExp.Arguments.Count != 1)
                    throw new FieldSpecParserException("Field specifications must have exactly one argument");

                RqlConstantExpression constExp = funcExp.Arguments[0] as RqlConstantExpression;

                if (constExp == null || !(constExp.Value is Int32))
                    throw new FieldSpecParserException("Field specification must be an integer");

                var value = (int)constExp.Value;

                if (value != 0 && value != 1)
                    throw new FieldSpecParserException("Field specification value must be 0 or 1");

                fields.Add(new FieldSpecField(funcExp.Name, value == 1 ? FieldSpecPresence.Included : FieldSpecPresence.Excluded));
            }

            return new FieldSpec(fields);
        }
    }
}

