using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Rql;
using MongoDB.Bson;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Rql.MongoDB
{
    public class RqlMongoNamespace : RqlNamespace
    {
        public RqlMongoNamespace(Type markerType) : this(markerType.Assembly)
        {
        }

        public RqlMongoNamespace(params Assembly[] assemblies) : base()
        {
            var types = new List<Type>();

            foreach (var assembly in assemblies)
            {
                types.AddRange(assembly.GetTypes().AsEnumerable().Where(t => typeof(IRqlCollection).IsAssignableFrom(t)));
            }

            this.CollectionInfos = new Dictionary<string, RqlCollectionInfo>(types.Count(), StringComparer.InvariantCultureIgnoreCase);
            this.CollectionTypes = new Dictionary<Type, string>();

            foreach (var type in types)
            {
                var fieldInfos = new Dictionary<string, RqlFieldInfo>(StringComparer.InvariantCultureIgnoreCase);

                AddProperties(fieldInfos, "", "", type);

                object[] attrs = type.GetCustomAttributes(typeof(RqlNameAttribute), true);
                RqlNameAttribute attr = attrs.Length > 0 ? (RqlNameAttribute)attrs[0] : null;

                string collectionName;

                if (attr == null)
                    collectionName = type.Name.ToLower();
                else
                    collectionName = attr.Name.ToLower();

                this.CollectionInfos.Add(collectionName, new RqlCollectionInfo(this, type.Name, fieldInfos));
                this.CollectionTypes.Add(type, collectionName);
            }
        }

        private static RqlDataType ClrTypeToRqlType(Type type)
        {
            RqlDataType rqlType;

            if (type == typeof(bool) || type == typeof(bool?))
                rqlType = RqlDataType.Boolean;
            else if (type == typeof(int) || type == typeof(int?))
                rqlType = RqlDataType.Integer;
            else if (type == typeof(double) || type == typeof(double?))
                rqlType = RqlDataType.Double;
            else if (type == typeof(string))
                rqlType = RqlDataType.String;
            else if (type == typeof(ObjectId))
                rqlType = RqlDataType.Id;
            else if (type == typeof(DateTime) || type == typeof(DateTime?) ||
                     type == typeof(TimeSpan) || type == typeof(TimeSpan?))
                rqlType = RqlDataType.DateTime;
            else if (type.IsArray || 
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                rqlType = RqlDataType.Tuple;
            else if (type.IsClass && typeof(IRqlDocument).IsAssignableFrom(type))
                rqlType = RqlDataType.Document;
            else
                throw new NotSupportedException(String.Format("CLR type '{0}' is not supported", type.FullName));

            return rqlType;
        }
                
        private static void ClrSubTypeToSubRqlType(Type type, RqlDataType rqlType, out Type subType, out RqlDataType subRqlType)
        {
            if (rqlType == RqlDataType.Tuple)
            {
                if (type.IsArray)
                    subType = type.GetElementType();
                else
                    subType = type.GetGenericArguments()[0];

                subRqlType = ClrTypeToRqlType(subType);
            }
            else
            {
                subType = null;
                subRqlType = RqlDataType.None;
            }
        }

		private static List<PropertyInfo> GetProperties(Type type)
		{
			PropertyInfo[] propInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			List<PropertyInfo> actualPropInfos = new List<PropertyInfo>();

			foreach (var propInfo in propInfos)
			{
				if (!propInfo.CanRead)
					continue;

				actualPropInfos.Add(propInfo);
			}

			return actualPropInfos;
		}

        private static void AddProperties(Dictionary<string, RqlFieldInfo> fieldInfos, string rqlBaseName, string dbBaseName, Type collectionType)
        {
            var propInfos = GetProperties(collectionType);

            foreach (var propInfo in propInfos)
            {
                Type propType = propInfo.PropertyType;

                string fieldName = ToCamelCase(propInfo.Name);
                string rqlFieldName = rqlBaseName + fieldName;
                string dbFieldName;

                if (fieldName == "id")
                    dbFieldName = dbBaseName + "_id";
                else
                    dbFieldName = dbBaseName + fieldName;

                RqlDataType rqlType = ClrTypeToRqlType(propType);
                Type subPropType;
                RqlDataType subRqlType;

                ClrSubTypeToSubRqlType(propType, rqlType, out subPropType, out subRqlType);

                fieldInfos.Add(rqlFieldName, new RqlFieldInfo(dbFieldName, rqlType, subRqlType));

                if (rqlType == RqlDataType.Document)
                {
                    AddProperties(fieldInfos, rqlFieldName + ".", dbFieldName + ".", propType);
                }
                else
                {
                    // TODO: Should be limited to 3 or 4 levels deep?
                    while (rqlType == RqlDataType.Tuple)
                    {
                        if (subRqlType == RqlDataType.Document)
                        {
                            AddProperties(fieldInfos, rqlFieldName + ".", dbFieldName + ".", subPropType);
                        }

                        propType = subPropType;
                        rqlType = subRqlType;

                        ClrSubTypeToSubRqlType(propType, rqlType, out subPropType, out subRqlType);

                        rqlFieldName += ".#";
                        dbFieldName += ".#";

                        fieldInfos.Add(rqlFieldName, new RqlFieldInfo(dbFieldName, rqlType, subRqlType));
                    }
                }
            }
        }
        
        private static string ToCamelCase(string value)
        {
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }
    }
}

