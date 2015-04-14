using System;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public class TimeSpanSerializer : SerializerBase<TimeSpan>
    {
        public TimeSpanSerializer()
        {
        }

        public override TimeSpan Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            IBsonReader reader = context.Reader;
            BsonType bsonType = reader.GetCurrentBsonType();

            switch (bsonType)
            {
            case BsonType.Null:
                reader.ReadNull();
                return TimeSpan.Zero;
            case BsonType.Int32:
                return TimeSpan.FromMilliseconds((double)reader.ReadInt32());
            case BsonType.Int64:
                return TimeSpan.FromMilliseconds((double)reader.ReadInt64());
            case BsonType.Double:
                return TimeSpan.FromMilliseconds(reader.ReadDouble());
            default:
                throw base.CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeSpan value)
        {
            IBsonWriter writer = context.Writer;
            var bsonDouble = new BsonDouble(((TimeSpan)value).TotalMilliseconds);

            writer.WriteDouble(bsonDouble.Value);
        }
    }
}
