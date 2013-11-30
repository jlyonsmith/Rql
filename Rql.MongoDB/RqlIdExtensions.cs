using System;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public static class RqlIdExtensions
    {
        public static ObjectId ToObjectId(this RqlId rqlId)
        {
            var s = rqlId.ToString("n");

            if (s.Length == 0)
                return new ObjectId();
            else
                return new ObjectId(s);
        }

        public static ObjectId ToObjectId(this RqlId? rqlId)
        {
            var s = rqlId.Value.ToString("n");

            if (s.Length == 0)
                return new ObjectId();
            else
                return new ObjectId(s);
        }
    }
}

