using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Rql
{
    public abstract class RqlExpressionVisitor
    {
        protected RqlExpressionVisitor()
        {
        }

        protected virtual RqlExpression Visit(RqlExpression node)
        {
            if (node == null)
                return node;

            switch (node.ExpressionType)
            {
            case RqlExpressionType.Constant:
                return this.VisitConstant((RqlConstantExpression)node);
            case RqlExpressionType.Identifier:
                return this.VisitIdentifier((RqlIdentifierExpression)node);
            case RqlExpressionType.FunctionCall:
                return this.VisitFunctionCall((RqlFunctionCallExpression)node);
            case RqlExpressionType.Tuple:
                return this.VisitTuple((RqlTupleExpression)node);
            default:
                throw new NotImplementedException();
            }
        }

        protected virtual RqlExpression VisitFunctionCall(RqlFunctionCallExpression node)
        {
            for (int i = 0, n = node.Arguments.Count; i < n; i++)
            {
                this.Visit(node.Arguments[i]);
            }

            return node;
        }
        
        protected virtual RqlExpression VisitConstant(RqlConstantExpression node)
        {
            return node;
        }

        protected virtual RqlExpression VisitTuple(RqlTupleExpression node)
        {
            return node;
        }

        protected virtual RqlExpression VisitIdentifier(RqlIdentifierExpression node)
        {
            return node;
        }
    }
}

