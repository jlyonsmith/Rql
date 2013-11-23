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
        public SortSpecToMongoSortByCompiler()
        {
        }

        public IMongoSortBy Compile(IRqlCollectionInfo collectionInfo, SortSpec sortSpec)
        {
            var builder = new SortByBuilder();

            foreach (var field in sortSpec.Fields)
            {
                var fieldName = field.Name;

                if (String.CompareOrdinal(field.Name, "$natural") != 0)
                {
                    var fieldInfo = collectionInfo.GetFieldInfoByRqlName(field.Name);

                    if (fieldInfo == null)
                        throw new SortSpecToMongoException(String.Format("Field {0} does not exist", field.Name));

                    fieldName = fieldInfo.Name;
                }

                if (field.Order == SortSpecSortOrder.Ascending)
                    builder.Ascending(MongoUtils.ToCamelCase(fieldName));
                else if (field.Order == SortSpecSortOrder.Descending)
                    builder.Descending(MongoUtils.ToCamelCase(fieldName));
            }

            return (IMongoSortBy)builder;
        }
    }
}

