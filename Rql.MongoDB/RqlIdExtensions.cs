using System;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public static class RqlIdExtensions
    {
        public static ObjectId ToObjectId(this RqlId rqlId)
        {
            return new ObjectId(rqlId.ToString("n"));
        }

        public static ObjectId ToObjectId(this RqlId? rqlId)
        {
            return new ObjectId(rqlId.Value.ToString("n"));
        }
    }
}

