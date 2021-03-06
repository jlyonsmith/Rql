﻿using System;
using MongoDB.Driver;

namespace Rql.MongoDB
{
    public static class MongoNameFixer
    {
        public static string Field(string field)
        {
            field = MongoUtils.ToCamelCase(field);

            if (field == "id")
                field = "_id";

            return field;
        }

        public static string Collection(string collection)
        {
            return MongoUtils.ToCamelCase(collection);
        }
    }
}

