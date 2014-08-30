using System;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public static class RqlIdExtensions
    {
        public static ObjectId ToObjectId(this RqlId rqlId)
        {
            var bytes = rqlId.ToByteArray();
            byte[] tmp;

            if (bytes.Length > 12)
            {
                tmp = new byte[12];
                Array.Copy(bytes, tmp, 12);
            }
            else if (bytes.Length < 12)
            {
                tmp = new byte[12];
                Array.Copy(bytes, tmp, bytes.Length);
                for (int i = bytes.Length; i < 12; i++)
                    tmp[i] = 0;
            }
            else
                tmp = bytes;

            return new ObjectId(tmp);
        }

        public static ObjectId ToObjectId(this RqlId? rqlId)
        {
            if (!rqlId.HasValue)
                return new ObjectId();
            else
                return rqlId.Value.ToObjectId();
        }
    }
}

