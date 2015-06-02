using NUnit.Framework;
using System;
using Rql;
using Rql.MongoDB;
using System.Reflection;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Rql.MongoDB.Tests
{
    [TestFixture]
    public class SortSpecToSortDefinitionTests
    {
        [Test()]
        public void TestSortBy()
        {
            var pairs = new[] 
            {
                new { SortBy = "", Mongo = "{ \"$natural\" : 1 }" },
                new { SortBy = "name(1),age(-1)", Mongo = "{ \"name\" : 1, \"age\" : -1 }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                var sort = new SortSpecToSortDefinition().Compile<BsonDocument>(pair.SortBy);
                var doc = sort as BsonDocumentSortDefinition<BsonDocument>;

                Assert.NotNull(doc);
                Assert.AreEqual(pair.Mongo, doc.Document.ToString(), String.Format("Iteration {0}", i));
            }
        }

        [Test]
        public void TestBadSortBy()
        {
            var pairs = new[] 
            {
                new { Sort = "name()" },
                new { Sort = "name(2)" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];

                Assert.Throws<SortSpecParserException>(() => new SortSpecToSortDefinition().Compile<BsonDocument>(pair.Sort));
            }
        }
    }
}

