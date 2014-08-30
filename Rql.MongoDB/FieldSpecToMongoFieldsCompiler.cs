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
    public class FieldSpecToMongoException : Exception
    {
        public FieldSpecToMongoException(string message) : base(message)
        {
        }
    }

    public class FieldSpecToMongoFieldsCompiler
    {
        public FieldSpecToMongoFieldsCompiler()
        {
        }

        public IMongoFields Compile(string fieldSpec)
        {
            if (String.IsNullOrEmpty(fieldSpec))
                return Fields.Null;

            return Compile(new FieldSpecParser().Parse(fieldSpec));
        }

        public IMongoFields Compile(FieldSpec fieldSpec)
        {
            var builder = new FieldsBuilder();

            foreach (var field in fieldSpec.Fields)
            {
                string name = MongoNameFixer.Field(field.Name);

                if (String.CompareOrdinal(name, "$textScore") == 0)
                {
                    builder.MetaTextScore(name);
                }
                else if (field.Presence == FieldSpecPresence.Included)
                {
                    builder.Include(name);
                }
                else
                {
                    builder.Exclude(name);
                }
            }

            return (IMongoFields)builder;
        }
    }
}

