using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Rql;
using Rql.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Rql.MongoDB.Tests
{
    [TestFixture()]
    public class RqlToMongoQueryCompilerTests
    {
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
                var filter = new RqlToMongoFilterDefinition().Compile(pair.Rql);
                var doc = filter as BsonDocumentFilterDefinition<BsonDocument>;

                Assert.NotNull(doc);
                Assert.AreEqual(pair.Mongo, doc.Document.ToString(), String.Format("Iteration {0}", i));
            }
        }
        
        [Test()]
        public void TestIdentifiers()
        {
            var pairs = new[] 
            {
                new { Rql = "eq(theValueOfPi,3.14)", Mongo = "{ \"theValueOfPi\" : 3.14 }" },
                new { Rql = "eq(references.0,$d7X2HQlexQbSmn0B)", Mongo = "{ \"references.0\" : ObjectId(\"51d1e6baec98e811b7ee9d20\") }" },
                new { Rql = "eq(other.a,10)", Mongo = "{ \"other.a\" : 10 }" },
                new { Rql = "eq(others.3,20)", Mongo = "{ \"others.3\" : 20 }" },
                new { Rql = "eq(others.a,10)", Mongo = "{ \"others.a\" : 10 }" },
                new { Rql = "eq(matrix.1.2,10)", Mongo = "{ \"matrix.1.2\" : 10 }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                var filter = new RqlToMongoFilterDefinition().Compile(pair.Rql);
                var doc = filter as BsonDocumentFilterDefinition<BsonDocument>;

                Assert.NotNull(doc);
                Assert.AreEqual(pair.Mongo, doc.Document.ToString(), String.Format("Iteration {0}", i));
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
                var filter = new RqlToMongoFilterDefinition().Compile(pair.Rql);
                var doc = filter as BsonDocumentFilterDefinition<BsonDocument>;

                Assert.NotNull(doc);
                Assert.AreEqual(pair.Mongo, doc.Document.ToString(), String.Format("Iteration {0}", i));
            }
        }

        [Test()]
        public void TestAllTypes()
        {
            var pairs = new[] 
            {
                new { Rql = "eq(field,null)", Mongo = "{ \"field\" : null }" },
                new { Rql = "eq(logical,true)", Mongo = "{ \"logical\" : true }" },
                new { Rql = "eq(logical,false)", Mongo = "{ \"logical\" : false }" },
                new { Rql = "eq(a,10)", Mongo = "{ \"a\" : 10 }" },
                new { Rql = "eq(a,10.01)", Mongo = "{ \"a\" : 10.01 }" },
                new { Rql = "eq(id,$d7X2HQlexQbSmn0B)", Mongo = "{ \"_id\" : ObjectId(\"51d1e6baec98e811b7ee9d20\") }" },
                new { Rql = "eq(when,@2013-06-24T15:00:00Z)", Mongo = "{ \"when\" : ISODate(\"2013-06-24T15:00:00Z\") }" },
                new { Rql = "in(a,(1,2,3))", Mongo = "{ \"a\" : { \"$in\" : [1, 2, 3] } }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                var filter = new RqlToMongoFilterDefinition().Compile(pair.Rql);
                var doc = filter as BsonDocumentFilterDefinition<BsonDocument>;

                Assert.NotNull(doc);
                Assert.AreEqual(pair.Mongo, doc.Document.ToString(), String.Format("Iteration {0}", i));
            }
        }
    }
}

