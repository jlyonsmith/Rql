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
            var datas = new ObjectId[]
            {
                ObjectId.Empty,
                new ObjectId(DateTime.MinValue, 0, 0, 0),
                new ObjectId(DateTime.MinValue, 100, 200, 65535),
                new ObjectId(DateTime.MaxValue, 0xffffff, short.MaxValue, 0xffffff),
            };

            var strs = new string[]
            {
                "$0",
                "$24",
                "$1F2mgA9gNyZtkTIf6",
                "$1F2si9jk4p8vTbfmU",
            };

            for (int i = 0; i < datas.Length; i++)
            {
                var rqlId = datas[i].ToRqlId();
                var objId = rqlId.ToObjectId();
                var s = rqlId.ToString();

                Assert.AreEqual(datas[i], objId, "Data value {0}", i);
                Assert.AreEqual(strs[i], s, "String value {0}", i);
            }
        }
    }
}

