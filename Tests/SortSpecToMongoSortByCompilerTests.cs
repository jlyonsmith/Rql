using NUnit.Framework;
using System;
using Rql;
using Rql.MongoDB;
using System.Reflection;
using MongoDB.Driver.Builders;

namespace Rql.MongoDB.Tests
{
    [TestFixture]
    public class SortSpecToMongoSortByCompilerTests
    {
        [Test()]
        public void TestSortBy()
        {
            var pairs = new[] 
            {
                new { SortBy = "", Mongo = "" },
                new { SortBy = "name(1),age(-1)", Mongo = "{ \"name\" : 1, \"age\" : -1 }" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];
                var sortBy = new SortSpecToMongoSortByCompiler().Compile(pair.SortBy);
                string mongo = (sortBy == SortBy.Null ? "" : sortBy.ToString());

                Assert.AreEqual(pair.Mongo, mongo, String.Format("Iteration {0}", i));
            }
        }

        [Test]
        public void TestBadSortBy()
        {
            var pairs = new[] 
            {
                new { SortBy = "name()" },
                new { SortBy = "name(2)" },
            };

            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i];

                Assert.Throws<SortSpecParserException>(() => new SortSpecToMongoSortByCompiler().Compile(pair.SortBy));
            }
        }
    }
}

