using System;
using NUnit.Framework;
using System.Collections.Generic;
using Rql;
using Rql.MongoDB;
using MongoDB.Bson;

namespace Rql.Tests
{
    [TestFixture]
    public class RqlTimeSpanTests
    {
        [Test]
        public void TestTimeRoundTripFromString()
        {
            var timeSpanString = "~P0DT01H30M20.123S";
            var rqlTimeSpan = new RqlTimeSpan(timeSpanString);
            var timeSpan = (TimeSpan)rqlTimeSpan;

            Assert.AreEqual(0, timeSpan.Days);
            Assert.AreEqual(1, timeSpan.Hours);
            Assert.AreEqual(30, timeSpan.Minutes);
            Assert.AreEqual(20, timeSpan.Seconds);
            Assert.AreEqual(123, timeSpan.Milliseconds);
        }

        [Test]
        public void TestRoundTripFromTimeSpan()
        {
            var originalTimeSpan = new TimeSpan(12, 15, 6, 45, 27);
            var rqlTimeSpan = new RqlTimeSpan(originalTimeSpan);
            var timeSpan = (TimeSpan)rqlTimeSpan;

            Assert.AreEqual(12, timeSpan.Days);
            Assert.AreEqual(15, timeSpan.Hours);
            Assert.AreEqual(6, timeSpan.Minutes);
            Assert.AreEqual(45, timeSpan.Seconds);
            Assert.AreEqual(27, timeSpan.Milliseconds);
        }

        [Test]
        public void TestRoundTripFromTimeSpanTimeOnly()
        {
            var originalTimeSpan = new TimeSpan(0, 1, 45, 27, 222);
            var rqlTimeSpan = new RqlTimeSpan(originalTimeSpan);

            Assert.AreEqual("~P0DT01H45M27.222S", rqlTimeSpan.ToString());

            var timeSpan = (TimeSpan)rqlTimeSpan;

            Assert.AreEqual(0, timeSpan.Days);
            Assert.AreEqual(1, timeSpan.Hours);
            Assert.AreEqual(45, timeSpan.Minutes);
            Assert.AreEqual(27, timeSpan.Seconds);
            Assert.AreEqual(222, timeSpan.Milliseconds);
        }
    }
}

