using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Rql;
using Rql.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Driver.Builders;
using System.Collections.Generic;
using System.Reflection;

namespace Rql.MongoDB.Tests
{
    [RqlName("datas")] // Yes, I know this is not grammatically correct...
    class Data : IRqlCollection
    {
        public ObjectId Id { get; set; }
        public bool Deleted { get; set; }
        public string Field { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public double TheValueOfPi { get; set; }
        public DateTime When { get; set; }
        public bool Logical { get; set; }
        public ObjectId Reference { get; set; }
        public List<ObjectId> References { get; set; }
        public OtherData Other { get; set; }
        public List<OtherData> Others { get; set; }
        public List<List<int>> Matrix { get; set; }
        public string Unsettable { get { return ""; } }
    }

    class OtherData : IRqlDocument
    {
        public ObjectId Id { get; set; }
        public int A { get; set; }
        public List<double> Numbers { get; set; }
    }

    [TestFixture()]
    public class RqlToMongoQueryCompilerTests
    {
        private IRqlNamespace RqlNamespace { get; set; }

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            this.RqlNamespace = new RqlMongoNamespace(Assembly.GetExecutingAssembly());
        }

        [Test()]
        public void TestComparisonOps()
        {
            var pairs = new[] 
            {
                new { Rql = "eq(field,'abc')", Mongo = "{ \"field\" : \"abc\" }" },
                new { Rql = "eq(logical,true)", Mongo = "{ \"logical\" : true }" },
                new { Rql = "ne(field,'abc')", Mongo = "{ \"field\" : { \"$ne\" : \"abc\" } }" },
                new { Rql = "gt(field,'abc')", Mongo = "{ \"field\" : { \"$gt\" : \"abc\" } }" },
                new { Rql = "gte(field,'abc')", Mongo = "{ \"field\" : { \"$gte\" : \"abc\" } }" },
                new { Rql = "lt(field,'abc')", Mongo = "{ \"field\" : { \"$lt\" : \"abc\" } }" },
                new { Rql = "lte(field,'abc')", Mongo = "{ \"field\" : { \"$lte\" : \"abc\" } }" },
                new { Rql = "in(field,('abc','def'))", Mongo = "{ \"field\" : { \"$in\" : [\"abc\", \"def\"] } }" },
                new { Rql = "nin(field,('abc','def'))", Mongo = "{ \"field\" : { \"$nin\" : [\"abc\", \"def\"] } }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                string mongo = new RqlToMongoQueryCompiler().Compile(
                    this.RqlNamespace.GetCollectionInfo(typeof(Data)), pair.Rql, null).ToString();

                Assert.AreEqual(pair.Mongo, mongo, String.Format("Iteration {0}", i));
            }
        }
        
        [Test()]
        public void TestIdentifiers()
        {
            var pairs = new[] 
            {
                new { Rql = "eq(thevalueofpi,3.14)", Mongo = "{ \"theValueOfPi\" : 3.14 }" },
                new { Rql = "eq(references.0,$51d1e6baec98e811b7ee9d20)", Mongo = "{ \"references.0\" : ObjectId(\"51d1e6baec98e811b7ee9d20\") }" },
                new { Rql = "eq(other.a,10)", Mongo = "{ \"other.a\" : 10 }" },
                new { Rql = "eq(others.3,20)", Mongo = "{ \"others.3\" : 20 }" },
                new { Rql = "eq(others.a,10)", Mongo = "{ \"others.a\" : 10 }" },
                new { Rql = "eq(matrix.1.2,10)", Mongo = "{ \"matrix.1.2\" : 10 }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                string mongo = new RqlToMongoQueryCompiler().Compile(
                    this.RqlNamespace.GetCollectionInfo(typeof(Data)), pair.Rql, null).ToString();

                Assert.AreEqual(pair.Mongo, mongo, String.Format("Iteration {0}", i));
            }
        }

        [Test()]
        public void TestLogicalOps()
        {
            var pairs = new[] 
            {
                new { Rql = "and(eq(a,1),ne(b,2))", Mongo = "{ \"$and\" : [{ \"a\" : 1 }, { \"b\" : { \"$ne\" : 2 } }] }" },
                new { Rql = "or(eq(a,1),gt(b,0))", Mongo = "{ \"$or\" : [{ \"a\" : 1 }, { \"b\" : { \"$gt\" : 0 } }] }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                string mongo = new RqlToMongoQueryCompiler().Compile(
                    this.RqlNamespace.GetCollectionInfo(typeof(Data)), pair.Rql, null).ToString();

                Assert.AreEqual(pair.Mongo, mongo, String.Format("Iteration {0}", i));
            }
        }

        [Test()]
        public void TestWhere()
        {
            string rql = "in(reference,ids(datas,eq(a,10)))";
            string expectedMongo = "{ \"reference\" : { \"$in\" : [ObjectId(\"51d1e6baec98e811b7ee9d20\"), ObjectId(\"51d1e6baec98e811b7ee9d25\")] } }";
            string mongo = new RqlToMongoQueryCompiler().Compile(
                this.RqlNamespace.GetCollectionInfo(typeof(Data)), rql, 
                (name, exp) =>
                {
                    return new List<ObjectId>()
                    {
                        new ObjectId("51d1e6baec98e811b7ee9d20"),
                        new ObjectId("51d1e6baec98e811b7ee9d25")
                    };
                }).ToString();

            Assert.AreEqual(expectedMongo, mongo);
        }

        [Test()]
        public void TestIdsAtRoot()
        {
            string rql = "ids(datas,eq(a,10))";
            Assert.That(() => new RqlToMongoQueryCompiler().Compile(
                this.RqlNamespace.GetCollectionInfo(typeof(Data)), rql, null), Throws.Exception);
        }

        [Test()]
        public void TestTypeCoercion()
        {
            // TODO: Test error paths too
            var pairs = new[] 
            {
                new { Rql = "eq(field,null)", Mongo = "{ \"field\" : null }" },
                new { Rql = "eq(logical,true)", Mongo = "{ \"logical\" : true }" },
                new { Rql = "eq(logical,false)", Mongo = "{ \"logical\" : false }" },
                new { Rql = "eq(a,10)", Mongo = "{ \"a\" : 10 }" },
                new { Rql = "eq(a,10.01)", Mongo = "{ \"a\" : 10.01 }" },
                new { Rql = "eq(id,'51d1e6baec98e811b7ee9d20')", Mongo = "{ \"_id\" : ObjectId(\"51d1e6baec98e811b7ee9d20\") }" },
                new { Rql = "eq(id,$51d1e6baec98e811b7ee9d20)", Mongo = "{ \"_id\" : ObjectId(\"51d1e6baec98e811b7ee9d20\") }" },
                new { Rql = "eq(when,'2013-06-24T15:00:00Z')", Mongo = "{ \"when\" : ISODate(\"2013-06-24T15:00:00Z\") }" },
                new { Rql = "eq(when,@2013-06-24T15:00:00Z)", Mongo = "{ \"when\" : ISODate(\"2013-06-24T15:00:00Z\") }" },
                new { Rql = "in(a,(1,2,3))", Mongo = "{ \"a\" : { \"$in\" : [1, 2, 3] } }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                string mongo = new RqlToMongoQueryCompiler().Compile(
                    this.RqlNamespace.GetCollectionInfo(typeof(Data)), pair.Rql, null).ToString();

                Assert.AreEqual(pair.Mongo, mongo, String.Format("Iteration {0}", i));
            }
        }
    }
}

