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

    public delegate List<ObjectId> IdsQueryCompilerDelegate(RqlCollectionInfo collectionInfo, RqlExpression expression);

    public class RqlToMongoQueryCompiler : RqlExpressionVisitor
    {
        private static Regex indexRegex;
        private static Regex hashRegex;
        private StringBuilder sb;
        private RqlExpression exp;
        private RqlCollectionInfo collectionInfo;
        private Stack<RqlDataType> lhsFieldTypeStack;
        private IdsQueryCompilerDelegate idsQueryCompiler;
        
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

        public IMongoQuery Compile(RqlCollectionInfo collectionInfo, string rql, IdsQueryCompilerDelegate idsQueryCompiler)
        {
            return Compile(collectionInfo, new RqlParser().Parse(rql), idsQueryCompiler);
        }

        public IMongoQuery Compile(RqlCollectionInfo collectionInfo, RqlExpression expression, IdsQueryCompilerDelegate idsQueryCompiler)
        {
            if (collectionInfo == null)
                throw new ArgumentNullException();

            this.exp = expression;
            this.collectionInfo = collectionInfo;
            this.idsQueryCompiler = idsQueryCompiler;
            this.lhsFieldTypeStack = new Stack<RqlDataType>();
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

        private RqlFieldInfo GetFieldInfo(string name)
        {
            // Replace any indexes in the name with #'s for the look-up to work
            name = IndexRegex.Replace(name, ".#");

            return collectionInfo.GetFieldInfo(name);
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

            RqlFieldInfo fieldInfo = GetFieldInfo(identifier.Name);

            sb.Append("{");
            VisitIdentifier(identifier);
            lhsFieldTypeStack.Push(fieldInfo.RqlType);
            VisitConstant(constant);
            lhsFieldTypeStack.Pop();
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

            RqlFieldInfo fieldInfo = GetFieldInfo(identifier.Name);

            sb.Append("{");
            VisitIdentifier(identifier);
            sb.Append("{$");
            sb.Append(node.Name);
            sb.Append(": ");
            lhsFieldTypeStack.Push(fieldInfo.RqlType);
            VisitConstant(constant);
            lhsFieldTypeStack.Pop();
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

            RqlFieldInfo fieldInfo = GetFieldInfo(identifier.Name);

            VisitIdentifier(identifier);
            sb.Append("{$");
            sb.Append(node.Name);
            sb.Append(": ");
            lhsFieldTypeStack.Push(fieldInfo.SubRqlType);
            Visit(node.Arguments[1]);
            lhsFieldTypeStack.Pop();
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

        private RqlExpression VisitOperatorWhere(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 2)
                ThrowError(node, "{0} takes exactly two arguments", node.Name);
            
            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(identifier, "First argument must be a field identifier");

            RqlCollectionInfo idsCollectionInfo = this.collectionInfo.RqlNamespace.GetCollectionInfo(identifier.Name);

            if (idsCollectionInfo == null)
                ThrowError(identifier, "First argument is not a valid resource identifier");

            RqlExpression expression = node.Arguments[1];
            IEnumerable<ObjectId> ids = new ObjectId[0];

            if (idsQueryCompiler != null)
                ids = idsQueryCompiler(idsCollectionInfo, expression);

            VisitTuple(RqlExpression.Tuple(
                RqlToken.LeftParen(node.Token.Offset), 
                ids.Select<ObjectId, RqlConstantExpression>(id => RqlExpression.Constant(RqlToken.Constant(node.Token.Offset, id))).ToList()));

            return node;
        }

        private RqlExpression VisitOperatorLike(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 2)
                ThrowError(node, "{0} takes exactly two arguments", node.Name);
            
            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "First argument must be an identifier");
            
            RqlFieldInfo fieldInfo = GetFieldInfo(identifier.Name);

            if (fieldInfo.RqlType != RqlDataType.String)
                ThrowError(node.Arguments[0], "Field must be a string type");

            RqlConstantExpression constant = node.Arguments[1] as RqlConstantExpression;

            if (constant == null || constant.Value.GetType() != typeof(string))
                ThrowError(node, "Second argument must be a string constant");

            sb.Append("{");
            VisitIdentifier(identifier);
            sb.AppendFormat(@"/\b{0}/i", Regex.Escape((string)((RqlConstantExpression)node.Arguments[1]).Value));
            sb.Append("}");
            return node;
        }

        private RqlExpression VisitOperatorExists(RqlFunctionCallExpression node)
        {
            if (node.Arguments.Count != 1)
                ThrowError(node, "{0} takes exactly one argument", node.Name);

            RqlIdentifierExpression identifier = node.Arguments[0] as RqlIdentifierExpression;

            if (identifier == null)
                ThrowError(node.Arguments[0], "Argument must be an identifier");

            sb.Append("{");
            VisitIdentifier(identifier);
            sb.Append("{$");
            sb.Append(node.Name);
            sb.Append(": true }");
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

            RqlFieldInfo fieldInfo = GetFieldInfo(identifier.Name);

            if (fieldInfo.RqlType != RqlDataType.Tuple)
                ThrowError(identifier, "First argument must be a tuple");

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
                case "where":
                case "ids":
                    return VisitOperatorWhere(node);
                case "like":
                    return VisitOperatorLike(node);
                case "exists":
                    return VisitOperatorExists(node);
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
            RqlDataType lhsFieldType = (lhsFieldTypeStack.Count > 0 ? lhsFieldTypeStack.Peek() : RqlDataType.None);

            if (nodeType == typeof(string))
            {
                if (lhsFieldType == RqlDataType.Id)
                {
                    RqlId id;

                    if (RqlId.TryParse((string)nodeValue, out id))
                    {
                        nodeValue = new ObjectId(id.ToString("n"));
                        nodeType = nodeValue.GetType();
                    }
                    else
                        ThrowError(node, "{0} is not a valid resource id", nodeValue);
                }
                else if (lhsFieldType == RqlDataType.DateTime)
                {
                    RqlDateTime dateTime;

                    if (RqlDateTime.TryParse((string)nodeValue, out dateTime))
                    {
                        nodeValue = (DateTime)dateTime;
                        nodeType = nodeValue.GetType();
                    }
                    else
                        ThrowError(node, "{0} is not a valid date/time", nodeValue);
                }
            }
            else if (nodeType == typeof(RqlId))
            {
                nodeValue = new ObjectId(((RqlId)nodeValue).ToString("n"));
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
            RqlFieldInfo fieldInfo = GetFieldInfo(node.Name);

            if (fieldInfo == null)
                ThrowError(node, "Field '{0}' not found in '{1}' collection", node.Name, collectionInfo.Name);

            string fieldName = fieldInfo.Name;

            // Pull out any indexes from the identifier
            var indexMatches = IndexRegex.Matches(node.Name);

            // Put them back in the canonical field name
            int i = 0;

            fieldName = HashRegex.Replace(fieldName, m => indexMatches[i++].Groups[0].Value);

            sb.Append("\"");
            sb.Append(fieldName);
            sb.Append("\": ");

            return node;
        }
    }
}

