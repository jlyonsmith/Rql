using System;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public static class ObjectIdExtensions
    {
        public static RqlId ToRqlId(this ObjectId objectId)
        {
            return new RqlId("$" + objectId.ToString());
        }
    }
}

