using System;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public static class RqlIdExtensions
    {
        public static ObjectId ToObjectId(this RqlId rqlId)
        {
            return new ObjectId(rqlId.ToByteArray());
        }

        public static ObjectId ToObjectId(this RqlId? rqlId)
        {
            if (!rqlId.HasValue)
                return new ObjectId();
            else
                return new ObjectId(rqlId.Value.ToByteArray());
        }
    }
}

