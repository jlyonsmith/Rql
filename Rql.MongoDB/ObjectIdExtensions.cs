using System;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public static class ObjectIdExtensions
    {
        public static RqlId ToRqlId(this ObjectId objectId)
        {
            return new RqlId(objectId.ToByteArray());
        }

        public static RqlId ToRqlId(this ObjectId? objectId)
        {
            if (!objectId.HasValue)
                return new RqlId();
            else
                return new RqlId(objectId.Value.ToByteArray());
        }
    }
}
