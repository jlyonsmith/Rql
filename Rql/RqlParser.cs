using System;
using System.Collections.Generic;

namespace Rql
{
    public class RqlParseException : Exception
    {
        public int ErrorOffset { get; private set; }
        
        public RqlParseException(RqlToken token)
        {
            this.ErrorOffset = token.Offset;
        }
        
        public RqlParseException(RqlToken token, string message) : base(message)
        {
            this.ErrorOffset = token.Offset;
        }
    }

    public class RqlParser
    {
        private RqlTokenizer tokenizer;

        public RqlParser()
        {
        }

        public RqlExpression Parse(string input)
        {
            tokenizer = new RqlTokenizer(input);

            RqlToken token = tokenizer.Next();

            if (!token.IsIdentifier)
                throw new RqlParseException(token);

            return ParseExpression(token);
        }

        private RqlExpression ParseExpression(RqlToken functionToken)
        {
            RqlToken token = tokenizer.Next();

            if (!token.IsLeftParen)
                throw new RqlParseException(token);

            var arguments = new List<RqlExpression>();

            while (true)
            {
                token = tokenizer.Next();

                RqlExpression argument = null;

                if (token.IsIdentifier)
                {
                    if (tokenizer.PeekNext().IsLeftParen)
                        argument = ParseExpression(token);
                    else
                        argument = RqlExpression.Identifier(token);
                }
                else if (token.IsConstant)
                {
                    argument = RqlExpression.Constant(token);
                }
                else if (token.IsLeftParen)
                {
                    argument = ParseTuple(token);
                }
                else
                    throw new RqlParseException(token);

                arguments.Add(argument);

                token = tokenizer.Next();

                if (token.IsComma)
                    continue;
                else if (token.IsRightParen)
                    break;
                else
                    throw new RqlParseException(token);
            }

            return RqlExpression.FunctionCall(functionToken, arguments);
        }

        private RqlConstantExpression ParseTuple(RqlToken leftParenToken)
        {
            var tuple = new List<object>();
            Type itemType = null;

            while (true)
            {
                RqlToken token = tokenizer.Next();

                if (token.IsConstant)
                {
                    if (itemType == null)
                        itemType = token.Data.GetType();
                    else if (token.Data.GetType() != itemType)
                        throw new RqlParseException(token, "Tuple items must all be of the same type");

                    tuple.Add(token.Data);
                }
                else
                    throw new RqlParseException(token);

                token = tokenizer.Next();

                if (token.IsComma)
                    continue;
                else if (token.IsRightParen)
                    break;
                else
                    throw new RqlParseException(token);
            }

            return RqlConstantExpression.Constant(leftParenToken, tuple.ToArray());
        }
    }
}

