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
        class RqlTestCollectionInfo : IRqlCollectionInfo
        {
            public RqlTestCollectionInfo(IRqlNamespace rqlNamespace, string name, string rqlName)
            {
                this.RqlNamespace = rqlNamespace;
                this.RqlName = rqlName;
                this.Name = name;
            }

            public string[] GetRqlNames()
            {
                throw new NotImplementedException();
            }

            public IRqlFieldInfo GetFieldInfoByName(string name)
            {
                throw new NotImplementedException();
            }

            public IRqlFieldInfo GetFieldInfoByRqlName(string rqlName)
            {
                throw new NotImplementedException();
            }

            public string Name { get; private set; }
            public string RqlName { get; private set; }
            public IRqlNamespace RqlNamespace { get; private set; }
        }

        class RqlTestNamespace : IRqlNamespace
        {
            private List<RqlTestCollectionInfo> CollectionInfos { get; set; }

            public RqlTestNamespace()
            {
                this.CollectionInfos = new List<RqlTestCollectionInfo>()
                {
                    {
                        new RqlTestCollectionInfo(this, "Data", "datas")
                    }
                };
            }

            public string[] GetRqlNames()
            {
                return this.CollectionInfos.Select(c => c.RqlName).ToArray();
            }

            public IRqlCollectionInfo GetCollectionInfoByName(string name)
            {
                return this.CollectionInfos.Find(c => c.Name == name);
            }

            public IRqlCollectionInfo GetCollectionInfoByRqlName(string rqlName)
            {
                return this.CollectionInfos.Find(c => c.RqlName == rqlName);
            }
        }

        [Test()]
        public void TestNamespace()
        {
            var rqlNamespace = new RqlMongoNamespace(Assembly.GetExecutingAssembly());
            var rqlTestNamespace  = new RqlTestNamespace();

            string[] expectedRqlCollectionNames = rqlTestNamespace.GetRqlNames();
            string[] rqlCollectionNames = rqlNamespace.GetRqlNames();

            CollectionAssert.AreEqual(expectedRqlCollectionNames, rqlCollectionNames);

            foreach (var rqlCollectionName in expectedRqlCollectionNames)
            {
                IRqlCollectionInfo expectedCollectionInfo = rqlTestNamespace.GetCollectionInfoByRqlName(rqlCollectionName);
                IRqlCollectionInfo collectionInfo = rqlNamespace.GetCollectionInfoByRqlName(rqlCollectionName);

                Assert.NotNull(expectedCollectionInfo);
                Assert.NotNull(collectionInfo);
                Assert.AreEqual(expectedCollectionInfo.RqlName, collectionInfo.RqlName);
                Assert.NotNull(collectionInfo.RqlNamespace);
            }
        }
    }
}

