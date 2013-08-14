using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rql
{
    public enum RqlExpressionType
    {
        Identifier,
        Constant,
        Tuple,
        FunctionCall
    }

    public class RqlExpression
    {
        public RqlExpression(RqlExpressionType expressionType)
        {
            this.ExpressionType = expressionType;
        }

        public RqlExpressionType ExpressionType { get; private set; }
        public RqlToken Token { get; protected set; }

        public static RqlIdentifierExpression Identifier(RqlToken token)
        {
            return new RqlIdentifierExpression(token);
        }

        public static RqlConstantExpression Constant(RqlToken token)
        {
            return new RqlConstantExpression(token);
        }

        public static RqlTupleExpression Tuple(RqlToken token, IList<RqlConstantExpression> constants)
        {
            return new RqlTupleExpression(token, constants);
        }

        public static RqlFunctionCallExpression FunctionCall(RqlToken token, IList<RqlExpression> arguments)
        {
            return new RqlFunctionCallExpression(token, arguments);
        }
    }

    public class RqlIdentifierExpression : RqlExpression
    {
        public RqlIdentifierExpression(RqlToken token) : base(RqlExpressionType.Identifier)
        {
            if (!token.IsIdentifier)
                throw new RqlParseException(token);

            this.Token = token;
        }

        public string Name { get { return (string)Token.Data; } }
    }

    public class RqlTupleExpression : RqlExpression
    {
        public RqlTupleExpression(RqlToken token) : this(token, new RqlConstantExpression[0])
        {
        }

        public RqlTupleExpression(RqlToken token, IList<RqlConstantExpression> constants) : base(RqlExpressionType.Tuple)
        {
            if (!token.IsLeftParen)
                throw new RqlParseException(token);

            this.Token = token;
            this.Constants = new ReadOnlyCollection<RqlConstantExpression>(constants);
        }

        public ReadOnlyCollection<RqlConstantExpression> Constants { get; private set; }
    }

    public class RqlConstantExpression : RqlExpression
    {
        public RqlConstantExpression(RqlToken token) : base(RqlExpressionType.Constant)
        {
            if (!token.IsConstant)
                throw new RqlParseException(token);

            this.Token = token;
        }

        public Type Type { get { return Value == null ? null : Value.GetType(); } }
        public object Value { get { return Token.Data; } }
    }

    public class RqlFunctionCallExpression : RqlExpression
    {
        public RqlFunctionCallExpression(RqlToken token, IList<RqlExpression> arguments) 
            : base(RqlExpressionType.FunctionCall)
        {
            if (!token.IsIdentifier)
                throw new RqlParseException(token);

            this.Token = token;
            this.Arguments = new ReadOnlyCollection<RqlExpression>(arguments);
        }

        public string Name { get { return (string)Token.Data; } }
        public ReadOnlyCollection<RqlExpression> Arguments { get; private set; }
    }
}

