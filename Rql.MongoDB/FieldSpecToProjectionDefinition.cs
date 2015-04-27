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
    public class FieldSpecToProjectionDefinition
    {
        public FieldSpecToProjectionDefinition()
        {
        }

        public ProjectionDefinition<T, T> Compile<T>(string fieldSpec)
        {
            if (String.IsNullOrEmpty(fieldSpec))
                return new BsonDocumentProjectionDefinition<T, T>(new BsonDocument());

            return Compile<T>(new FieldSpecParser().Parse(fieldSpec));
        }

        public ProjectionDefinition<T, T> Compile<T>(FieldSpec fieldSpec)
        {
            var sb = new StringBuilder();

            sb.Append("{ ");

            for (int i = 0; i < fieldSpec.Fields.Length; i++)
            {
                var field = fieldSpec.Fields[i];
                string name = MongoNameFixer.Field(field.Name);

                if (String.CompareOrdinal(name, "textScore") == 0)
                {
                    sb.AppendFormat("{0} : {{ $meta : \"textScore\" }}", name);
                }
                else if (field.Presence == FieldSpecPresence.Included)
                {
                    sb.AppendFormat("{0} : 1", name);
                }
                else
                {
                    sb.AppendFormat("{0} : 0", name);
                }

                if (i < fieldSpec.Fields.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(" }");

            return new BsonDocumentProjectionDefinition<T, T>(BsonSerializer.Deserialize<BsonDocument>(sb.ToString()));
        }
    }
}

