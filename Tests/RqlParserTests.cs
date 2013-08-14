using System;
using NUnit.Framework;
using Rql;
using System.Collections.Generic;

namespace Rql.Tests
{
    public class TestExpressionVisitor : RqlExpressionVisitor
    {
        private string s;
        private RqlExpression exp;

        public TestExpressionVisitor(RqlExpression exp)
        {
            this.exp = exp;
        }

        public override string ToString()
        {
            if (s == null)
            {
                s = String.Empty;
                Visit(exp);
            }

            return s;
        }

        protected override RqlExpression VisitFunctionCall(RqlFunctionCallExpression node)
        {
            s += node.Name;
            s += "(";
            
            for (int i = 0, n = node.Arguments.Count; i < n; i++)
            {
                this.Visit(node.Arguments[i]);

                if (i < node.Arguments.Count - 1)
                    s += ",";
            }

            s += ")";

            return node;
        }

        protected override RqlExpression VisitConstant(RqlConstantExpression node)
        {
            s += FormatSimpleConstant(node.Type, node.Value);

            return node;
        }

        protected override RqlExpression VisitTuple(RqlTupleExpression node)
        {
            var list = node.Constants;

            s += "(";

            for (int i = 0; i < list.Count; i++)
            {
                VisitConstant(list[i]);
                s += (i < list.Count - 1 ? "," : "");
            }

            s += ")";

            return node;
        }

        private string FormatSimpleConstant(Type type, object data)
        {
            if (type == typeof(string))
                return "'" + data.ToString() + "'";
            else if (type == typeof(bool))
                return data.ToString().ToLower();
            else
                return data.ToString();
        }

        protected override RqlExpression VisitIdentifier(RqlIdentifierExpression node)
        {
            s += node.Name;

            return node;
        }
    }

    [TestFixture()]
    public class RqlParserTests
    {
        [Test]
        public void TestSimpleExpression()
        { 
                                   //0000000000111111111122222222223333333333444444444455555555556666666666
                                   //0123456789012345678901234567890123456789012345678901234567890123456789
            string expressionText = "and(eq(field1,10),gte(field2,0),eq(field3,true),in(field4,(1,2,3,4)))";
            RqlExpression exp = new RqlParser().Parse(expressionText);

            Assert.IsInstanceOf<RqlExpression>(exp);
            Assert.AreEqual(expressionText, new TestExpressionVisitor(exp).ToString());
        }
    }
}

