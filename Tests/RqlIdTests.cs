using System;
using NUnit.Framework;
using System.Collections.Generic;
using Rql;
using Rql.MongoDB;
using MongoDB.Bson;

namespace Rql.Tests
{
    [TestFixture]
    public class RqlIdTests
    {
        [Test]
        public void TestZero()
        {
            Assert.AreEqual("$0", RqlId.Zero.ToString());
        }

        [Test]
        public void TestFromString()
        {
            var datas = new string[]
            {
                "$0",
                "$1",
                "$123abcABC",
                "$ZZZZZZZZZ"
            };

            for (int i = 0; i < datas.Length; i++)
            {
                var id1 = new RqlId(datas[i]);
                var s = id1.ToString();
                var id2 = new RqlId(s);

                Assert.AreEqual(id1, id2, "Data value {0}", i);
            }
        }

        [Test]
        public void TestFromObjectIds()
        {
            var objIds = new ObjectId[]
            {
                ObjectId.Empty,
                new ObjectId(DateTime.MinValue, 0, 0, 0),
                new ObjectId(DateTime.MinValue, 100, 200, 65535),
                new ObjectId(DateTime.MaxValue, 0xffffff, short.MaxValue, 0xffffff),
            };

            var rqlIds = new RqlId[]
            {
                new RqlId("$0"),
                new RqlId("$24"),
                new RqlId("$1F2mgA9gNyZtkTIf6"),
                new RqlId("$1F2si9jk4p8vTbfmU")
            };

            for (int i = 0; i < objIds.Length; i++)
            {
                var rqlId = objIds[i].ToRqlId();
                var objId = rqlId.ToObjectId();
                var rqlId2 = objId.ToRqlId();

                Assert.AreEqual(objIds[i], objId, "ObjectId value {0}", i);
                Assert.AreEqual(rqlIds[i], rqlId2, "RqlId value {0}", i);
            }
        }

        [Test]
        public void TestFromRqlIds()
        {
            var rqlIds = new RqlId[]
            {
                new RqlId("$0"),
                new RqlId("$24"),
                new RqlId("$1F2mgA9gNyZtkTIf6"),
                new RqlId("$1Ad4Xro7A6yeAl77J")  // This one caused problems

            };

            var objIds = new ObjectId[]
            {
                ObjectId.Empty,
                new ObjectId(DateTime.MinValue, 0, 0, 0),
                new ObjectId(DateTime.MinValue, 100, 200, 65535),
                new ObjectId("53d5244dec98e866c0d800f4")
            };

            for (int i = 0; i < rqlIds.Length; i++)
            {
                var objId = rqlIds[i].ToObjectId();
                var rqlId = objId.ToRqlId();
                var objId2 = rqlId.ToObjectId();

                Assert.AreEqual(rqlIds[i], rqlId, "ObjectId value {0}", i);
                Assert.AreEqual(objIds[i], objId2, "RqlId value {0}", i);
            }
        }
    }
}

