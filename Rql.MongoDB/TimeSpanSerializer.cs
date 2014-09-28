using System;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace Rql.MongoDB
{
    public class TimeSpanSerializer : BsonBaseSerializer
    {
        public TimeSpanSerializer()
        {
        }

        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(TimeSpan));

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
            case BsonType.Null:
                bsonReader.ReadNull();
                return null;
            case BsonType.Int32:
                return TimeSpan.FromMilliseconds((double)bsonReader.ReadInt32());
            case BsonType.Int64:
                return TimeSpan.FromMilliseconds((double)bsonReader.ReadInt64());
            case BsonType.Double:
                return TimeSpan.FromMilliseconds(bsonReader.ReadDouble());
            default:
                var message = string.Format("Cannot deserialize TimeSpan from BsonType {0}.", bsonType);
                throw new FileFormatException(message);
            }
        }

        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var bsonDouble = new BsonDouble(((TimeSpan)value).TotalMilliseconds);
            bsonWriter.WriteDouble(bsonDouble.Value);
        }
    }
}
