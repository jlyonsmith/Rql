using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Collections.ObjectModel;
using Rql;

namespace Rql.MongoDB
{
    // TODO: Need unit tests for this class
    // TODO: Should only be able to sort on fields that have indexes?
    // TODO: Change this so it doesn't do reflection all the time by using a global sort spec converter cache 

    public class SortSpecToMongoException : Exception
    {
        public SortSpecToMongoException(string message) : base(message)
        {
        }
    }

    public class SortSpecToMongoSortByCompiler
    {
        private HashSet<string> fieldNames;

        public SortSpecToMongoSortByCompiler()
        {
        }

        public IMongoSortBy Compile(Type collectionType, SortSpec sortSpec)
        {
            PropertyInfo[] propInfos = collectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            fieldNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var propInfo in propInfos)
                fieldNames.Add(propInfo.Name);

            var builder = new SortByBuilder();

            foreach (var field in sortSpec.Fields)
            {
                if (String.CompareOrdinal(field.Name, "$natural") != 0 && !fieldNames.Contains(field.Name))
                    throw new SortSpecToMongoException(String.Format("Field {0} does not exist", field.Name));

                if (field.Order == SortSpecSortOrder.Ascending)
                    builder.Ascending(MongoUtils.ToCamelCase(field.Name));
                else if (field.Order == SortSpecSortOrder.Descending)
                    builder.Descending(MongoUtils.ToCamelCase(field.Name));
            }

            return (IMongoSortBy)builder;
        }
    }
}

