using System;
using NUnit.Framework;
using System.Collections.Generic;
using Rql;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using OriginalTimeSpanSerializer = MongoDB.Bson.Serialization.Serializers.TimeSpanSerializer;

namespace Rql.Tests
{
    public class TestTimeSpan
    {
        public TimeSpan Span { get; set; }
        public TimeSpan? OptionalSpan { get; set; }
    }

    [TestFixture]
    public class TimeSpanSerializerTests
    {
        [Test]
        public void TestRoundtrip()
        {
            var obj = new TestTimeSpan() 
            {
                Span = new TimeSpan(1, 1, 1, 1, 1),
                OptionalSpan = new TimeSpan(2, 2, 2, 2, 2)
            };

            BsonSerializer.RegisterSerializer(typeof(TimeSpan), new Rql.MongoDB.TimeSpanSerializer());

            var doc = obj.ToBsonDocument();

            Assert.AreEqual("{ \"Span\" : 90061001.0, \"OptionalSpan\" : 180122002.0 }", doc.ToString());

            var obj2 = BsonSerializer.Deserialize<TestTimeSpan>(doc);

            Assert.AreEqual(obj.Span, obj2.Span);
            Assert.AreEqual(obj.OptionalSpan, obj2.OptionalSpan);

            // TODO: How to unregister a serializer?
            // BsonSerializer.RegisterSerializer(typeof(TimeSpan), new OriginalTimeSpanSerializer());
        }
    }
}

