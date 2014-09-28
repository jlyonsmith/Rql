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

    public class TestTimeSpan2
    {
        public TimeSpan Int32 { get; set; }
        public TimeSpan Int64 { get; set; }
        public TimeSpan Double { get; set; }
    }

    [TestFixture]
    public class TimeSpanSerializerTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            BsonSerializer.RegisterSerializer(typeof(TimeSpan), new Rql.MongoDB.TimeSpanSerializer());
        }

        [Test]
        public void TestRoundtrip()
        {
            var obj = new TestTimeSpan() 
            {
                Span = new TimeSpan(1, 1, 1, 1, 1),
                OptionalSpan = new TimeSpan(2, 2, 2, 2, 2)
            };

            var doc = obj.ToBsonDocument();

            Assert.AreEqual("{ \"Span\" : 90061001.0, \"OptionalSpan\" : 180122002.0 }", doc.ToString());

            var obj2 = BsonSerializer.Deserialize<TestTimeSpan>(doc);

            Assert.AreEqual(obj.Span, obj2.Span);
            Assert.AreEqual(obj.OptionalSpan, obj2.OptionalSpan);

            // TODO: How to unregister a serializer?
            // BsonSerializer.RegisterSerializer(typeof(TimeSpan), new OriginalTimeSpanSerializer());
        }

        [Test]
        public void TestNonDecimalDeserialize()
        {
            var doc = new BsonDocument();

            doc.Add("Int32", new BsonInt32(1000));
            doc.Add("Int64", new BsonInt64(2000));
            doc.Add("Double", new BsonDouble(3000.0));

            var obj = BsonSerializer.Deserialize<TestTimeSpan2>(doc);

            Assert.AreEqual(TimeSpan.FromMilliseconds(1000), obj.Int32);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2000), obj.Int64);
            Assert.AreEqual(TimeSpan.FromMilliseconds(3000), obj.Double);
        }
    }
}

