using System;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using Rql;
using Rql.MongoDB;
using System.Collections.Generic;

namespace Rql.MongoDB.Tests
{
    [TestFixture()]
    public class RqlMongoNamespaceTests
    {
        class RqlTestNamespace : IRqlNamespace
        {
            private Dictionary<string, RqlCollectionInfo> CollectionInfos { get; set; }
            protected Dictionary<Type, string> CollectionTypes { get; set; }

            public RqlTestNamespace()
            {
                this.CollectionInfos = new Dictionary<string, RqlCollectionInfo>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {
                        "datas",
                        new RqlCollectionInfo(
                            this,
                            "Data",
                            new Dictionary<string, RqlFieldInfo>
                            {
                            { "id", new RqlFieldInfo("_id", RqlDataType.Id) },
                            { "deleted", new RqlFieldInfo("deleted", RqlDataType.Boolean) },
                            { "field", new RqlFieldInfo("field", RqlDataType.String) },
                            { "a", new RqlFieldInfo("a", RqlDataType.Integer) },
                            { "b", new RqlFieldInfo("b", RqlDataType.Integer) },
                            { "c", new RqlFieldInfo("c", RqlDataType.Integer) },
                            { "theValueOfPi", new RqlFieldInfo("theValueOfPi", RqlDataType.Double) },
                            { "when", new RqlFieldInfo("when", RqlDataType.DateTime) },
                            { "logical", new RqlFieldInfo("logical", RqlDataType.Boolean) },
                            { "reference", new RqlFieldInfo("reference", RqlDataType.Id) },
                            { "references", new RqlFieldInfo("references", RqlDataType.Tuple, RqlDataType.Id) },
                            { "references.#", new RqlFieldInfo("references.#", RqlDataType.Id) },
                            { "other", new RqlFieldInfo("other", RqlDataType.Document) },
                            { "other.id", new RqlFieldInfo("other._id", RqlDataType.Id) },
                            { "other.a", new RqlFieldInfo("other.a", RqlDataType.Integer) },
                            { "other.numbers", new RqlFieldInfo("other.numbers", RqlDataType.Tuple, RqlDataType.Double) },
                            { "other.numbers.#", new RqlFieldInfo("other.numbers.#", RqlDataType.Double) },
                            { "others", new RqlFieldInfo("others", RqlDataType.Tuple, RqlDataType.Document) },
                            { "others.id", new RqlFieldInfo("others._id", RqlDataType.Id) },
                            { "others.a", new RqlFieldInfo("others.a", RqlDataType.Integer) },
                            { "others.numbers", new RqlFieldInfo("others.numbers", RqlDataType.Tuple, RqlDataType.Double) },
                            { "others.numbers.#", new RqlFieldInfo("others.numbers.#", RqlDataType.Double) },
                            { "others.#", new RqlFieldInfo("others.#", RqlDataType.Document) },
                            { "matrix", new RqlFieldInfo("matrix", RqlDataType.Tuple, RqlDataType.Tuple) },
                            { "matrix.#", new RqlFieldInfo("matrix.#", RqlDataType.Tuple, RqlDataType.Integer) },
                            { "matrix.#.#", new RqlFieldInfo("matrix.#.#", RqlDataType.Integer) },
                            { "unsettable", new RqlFieldInfo("unsettable", RqlDataType.String) },
                        })
                    }
                    // Only one entry for now...
                };
            }

            public string[] GetCollectionNames()
            {
                return this.CollectionInfos.Keys.ToArray();
            }

            public RqlCollectionInfo GetCollectionInfo(Type type)
            {
                string collectionName;

                if (this.CollectionTypes.TryGetValue(type, out collectionName))
                    return GetCollectionInfo(collectionName);
                else
                    return null;
            }

            public RqlCollectionInfo GetCollectionInfo(string collectionName)
            {
                RqlCollectionInfo collectionInfo;

                if (this.CollectionInfos.TryGetValue(collectionName, out collectionInfo))
                    return collectionInfo;
                else
                    return null;
            }
        }

        [Test()]
        public void TestAllFieldNames()
        {
            var rqlNamespace = new RqlMongoNamespace(Assembly.GetExecutingAssembly());
            var rqlTestNamespace  = new RqlTestNamespace();

            string[] expectedCollectionNames = rqlTestNamespace.GetCollectionNames();
            string[] collectionNames = rqlNamespace.GetCollectionNames();

            CollectionAssert.AreEqual(expectedCollectionNames, collectionNames);

            foreach (var collectionName in expectedCollectionNames)
            {
                RqlCollectionInfo expectedCollectionInfo = rqlTestNamespace.GetCollectionInfo(collectionName);
                RqlCollectionInfo collectionInfo = rqlNamespace.GetCollectionInfo(collectionName);

                Assert.NotNull(expectedCollectionInfo);
                Assert.NotNull(collectionInfo);
                Assert.AreEqual(expectedCollectionInfo.Name, collectionInfo.Name);
                Assert.NotNull(collectionInfo.RqlNamespace);

                string[] expectedFieldNames = expectedCollectionInfo.GetFieldInfoNames();
                string[] fieldNames = collectionInfo.GetFieldInfoNames();

                CollectionAssert.AreEqual(expectedFieldNames, fieldNames);

                foreach (var fieldName in expectedFieldNames)
                {
                    RqlFieldInfo expectedFieldInfo = expectedCollectionInfo.GetFieldInfo(fieldName);
                    RqlFieldInfo fieldInfo = collectionInfo.GetFieldInfo(fieldName);

                    Assert.NotNull(expectedFieldInfo);
                    Assert.NotNull(fieldInfo);

                    string message = String.Format(
                        "Expected RqlFieldInfo: {{ Name: \"{0}\", RqlType: {1}, RqlSubType: {2} }}", 
                        expectedFieldInfo.Name, expectedFieldInfo.RqlType, expectedFieldInfo.SubRqlType);

                    Assert.NotNull(fieldInfo, message);
                    Assert.AreEqual(expectedFieldInfo.Name, fieldInfo.Name, message);
                    Assert.AreEqual(expectedFieldInfo.RqlType, fieldInfo.RqlType, message);
                    Assert.AreEqual(expectedFieldInfo.SubRqlType, fieldInfo.SubRqlType, message);
                }
            }
        }
    }
}

