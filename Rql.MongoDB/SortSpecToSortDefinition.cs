using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.ObjectModel;
using Rql;
using System.Text;
using MongoDB.Bson.Serialization;

namespace Rql.MongoDB
{
    public class SortSpecToSortDefinition
    {
        public SortSpecToSortDefinition()
        {
        }

        public SortDefinition<BsonDocument> Compile(string sortSpec)
        {
            if (String.IsNullOrEmpty(sortSpec))
                return new BsonDocumentSortDefinition<BsonDocument>(new BsonDocument());

            return Compile(new SortSpecParser().Parse(sortSpec));
        }

        public SortDefinition<BsonDocument> Compile(SortSpec sortSpec)
        {
            var sb = new StringBuilder();

            sb.Append("{ ");

            for (int i = 0; i < sortSpec.Fields.Length; i++)
            {
                var field = sortSpec.Fields[i];
                var name = MongoNameFixer.Field(field.Name);

                sb.AppendFormat("{0} : {1}", name, field.Order == SortSpecSortOrder.Ascending ? "1" : "-1");

                if (i < sortSpec.Fields.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(" }");

            return new BsonDocumentSortDefinition<BsonDocument>(BsonSerializer.Deserialize<BsonDocument>(sb.ToString()));
        }
    }
}

