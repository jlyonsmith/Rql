using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using Rql;
using System.Text.RegularExpressions;

namespace Rql.MongoDB
{
    public class RqlToMongoException : Exception
    {
        public RqlToMongoException(string message) : base(message)
        {
        }
    }

    public class RqlToMongoQueryCompiler : RqlExpressionVisitor
    {
        private static Regex indexRegex;
        private static Regex hashRegex;
        private StringBuilder sb;
        private RqlExpression exp;

        private static Regex IndexRegex 
        {
            get
            {
                if (indexRegex == null)
                {
                    indexRegex = new Regex(@"\.\d+", RegexOptions.Singleline | RegexOptions.Compiled);
                }

                return indexRegex;
            }
        }
        
        private static Regex HashRegex 
        {
            get
            {
                if (hashRegex == null)
                {
                    hashRegex = new Regex(@"\.#", RegexOptions.Singleline | RegexOptions.Compiled);
                }

                return hashRegex;
            }
        }

        public RqlToMongoQueryCompiler()
        {
        }

        public IMongoQuery Compile(string rql)
        {
            return Compile(new RqlParser().Parse(rql));
        }

        public IMongoQuery Compile(RqlExpression expression)
        {
            this.exp = expression;
            this.sb = new StringBuilder();

            Visit(exp);

            string s = sb.ToString();
            IMongoQuery query = Query.Null;

            if (!String.IsNullOrEmpty(s))
            {
                try
                {
                    query = new QueryDocument(BsonSerializer.Deserialize<BsonDocument>(s));
                }
                catch
                {
                    throw new RqlToMongoException("Invalid query specified");
                }
            }

            this.sb = null;

            return query;
        }

        private void ThrowError(RqlExpression node, string format, params object[] args)
        {
            string s = String.Format("Offset {0}: {1}", node.Token.Offset, format);

            throw new RqlToMongoException(String.Format(s, args));
        }

        private RqlExpression VisitOperatorEq(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 2)
                ThrowError(node, "Equality takes exactly two arguments");

            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "First argument must be a field identifier");

            RqlConstantExpression constant = node.Arguments[1] as RqlConstantExpression;

            if (constant == null)
                ThrowError(node, "Second argument must be a constant");

            sb.Append("{");
            VisitIdentifier(identifier);
            VisitConstant(constant);
            sb.Append("}");
            return node;
        }

        private RqlExpression VisitOperatorNeGtGteLtLte(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 2)
                ThrowError(node, String.Format("{0} takes exactly two arguments", node.Name));

            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "First argument must be a field identifier");

            RqlConstantExpression constant = node.Arguments[1] as RqlConstantExpression;

            if (constant == null)
                ThrowError(node, "Second argument must be a constant");

            sb.Append("{");
            VisitIdentifier(identifier);
            sb.Append("{$");
            sb.Append(node.Name);
            sb.Append(": ");
            VisitConstant(constant);
            sb.Append("}");
            sb.Append("}");
            return node;
        }

        private RqlExpression VisitOperatorInNinAll(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 2)
                ThrowError(node, String.Format("{0} needs two arguments", node.Name));
            
            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "First argument must be a field identifier");

            RqlTupleExpression tuple = node.Arguments[1] as RqlTupleExpression;
            RqlFunctionCallExpression funcCall = node.Arguments[1] as RqlFunctionCallExpression;

            // TODO: Need a way to find the return types of operators
            if (tuple == null && !(funcCall != null && (funcCall.Name == "where" || funcCall.Name == "ids")))
                ThrowError(node.Arguments[1], "Second argument must be a tuple or an expression returning a tuple");

            sb.Append("{");

            VisitIdentifier(identifier);
            sb.Append("{$");
            sb.Append(node.Name);
            sb.Append(": ");
            Visit(node.Arguments[1]);
            sb.Append("}");
            sb.Append("}");
            return node;
        }

        private RqlExpression VisitOperatorAndOr(RqlFunctionCallExpression node)
        {
            sb.Append("{");
            sb.Append("$");
            sb.Append(node.Name);
            sb.Append(": [");

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                RqlExpression argument = node.Arguments[i];

                if (argument.ExpressionType != RqlExpressionType.FunctionCall ||
                    (argument.ExpressionType == RqlExpressionType.Constant && argument.Token.Data.GetType() != typeof(bool)))
                    ThrowError(argument, "Argument must be boolean constant or expression");

                Visit(argument);

                if (i < node.Arguments.Count - 1)
                    sb.Append(",");
            }

            sb.Append("]");
            sb.Append("}");
            return node;
        }

        private RqlExpression VisitOperatorLikeLikei(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 2)
                ThrowError(node, "{0} takes exactly two arguments", node.Name);
            
            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "First argument must be an identifier");
            
            RqlConstantExpression constant = node.Arguments[1] as RqlConstantExpression;

            if (constant == null || constant.Value.GetType() != typeof(string))
                ThrowError(node, "Second argument must be a string constant");

            bool ignoreCase = (identifier.Name == "likei");

            sb.Append("{");
            VisitIdentifier(identifier);
            sb.AppendFormat(@"/\b{0}/{1}", Regex.Escape((string)((RqlConstantExpression)node.Arguments[1]).Value), ignoreCase ? "i" : "");
            sb.Append("}");
            return node;
        }

        private RqlExpression VisitOperatorExistsNexists(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 1)
                ThrowError(node, "{0} takes exactly one argument", node.Name);

            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "Argument must be an identifier");

            bool exists = (node.Name == "exists");

            sb.Append("{");
            VisitIdentifier(identifier);
            sb.Append("{$");
            sb.Append(node.Name);
            sb.AppendFormat(": {0} }}", exists ? "true" : "false");
            sb.Append("}");
            return node;
        }

        private RqlExpression VisitOperatorSize(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 2)
                ThrowError(node, "{0} takes exactly two arguments", node.Name);

            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "First argument must be an identifier");

            RqlConstantExpression constant = node.Arguments[1] as RqlConstantExpression;

            if (constant == null || constant.Value.GetType() != typeof(int))
                ThrowError(node.Arguments[1], "Second argument must be an integer constant");

            sb.Append("{");
            VisitIdentifier(identifier);
            sb.Append("{$");
            sb.Append(node.Name);
            sb.Append(": ");
            VisitConstant(constant);
            sb.Append("}");
            sb.Append("}");
            return node;
        }

        protected override RqlExpression VisitFunctionCall(RqlFunctionCallExpression node)
        {
            switch (node.Name)
            {
                case "eq":
                    return VisitOperatorEq(node);
                case "ne":
                case "gt":
                case "gte":
                case "lt":
                case "lte":
                    return VisitOperatorNeGtGteLtLte(node);
                case "in":
                case "nin":
                case "all":
                    return VisitOperatorInNinAll(node);
                case "and":
                case "or":
                    return VisitOperatorAndOr(node);
                case "like":
                case "likei":
                    return VisitOperatorLikeLikei(node);
                case "exists":
                case "nexists":
                    return VisitOperatorExistsNexists(node);
                case "size":
                    return VisitOperatorSize(node);
                default:
                    ThrowError(node, "{0} is not a supported operator", node.Name);
                    break;
            }

            return node;
        }

        protected override RqlExpression VisitConstant(RqlConstantExpression node)
        {
            object nodeValue = node.Value;
            Type nodeType = node.Type;

            if (nodeType == typeof(RqlId))
            {
                nodeValue = ((RqlId)nodeValue).ToObjectId();
                nodeType = nodeValue.GetType();
            }
            else if (nodeType == typeof(RqlDateTime))
            {
                nodeValue = (DateTime)(RqlDateTime)nodeValue;
                nodeType = nodeValue.GetType();
            }

            if (nodeValue == null)
            {
                sb.Append("null");
            }
            else if (nodeType == typeof(string))
            {
                sb.Append("\"" + nodeValue + "\"");
            }
            else if (nodeType == typeof(bool))
            {
                sb.Append(nodeValue.ToString().ToLower());
            }
            else if (nodeType == typeof(ObjectId))
            {
                sb.Append("ObjectId(\"");
                sb.Append(((ObjectId)nodeValue).ToString());
                sb.Append("\")");
            }
            else if (nodeType == typeof(DateTime))
            {
                sb.Append("ISODate(\"");
                sb.Append(((DateTime)nodeValue).ToString(RqlDateTime.FormatPattern));
                sb.Append("\")");
            }
            else if (nodeType == typeof(double))
            {
                sb.Append(nodeValue.ToString());
            }
            else if (nodeType == typeof(int))
            {
                sb.Append(nodeValue.ToString());
            }
            else
                throw new NotImplementedException();

            return node;
        }

        protected override RqlExpression VisitTuple(RqlTupleExpression node)
        {
            sb.Append("[");
            var list = node.Constants;
            for (int i = 0; i < list.Count; i++)
            {

                VisitConstant(list[i]);
                sb.Append(i < list.Count - 1 ? "," : "");
            }
            sb.Append("]");

            return node;
        }

        protected override RqlExpression VisitIdentifier(RqlIdentifierExpression node)
        {
            sb.Append("\"");
            sb.Append(MongoNameFixer.Field(node.Name));
            sb.Append("\": ");

            return node;
        }
    }
}

