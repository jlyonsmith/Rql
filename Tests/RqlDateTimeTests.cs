using System;
using NUnit.Framework;
using System.Collections.Generic;
using Rql;
using Rql.MongoDB;
using MongoDB.Bson;

namespace Rql.Tests
{
    [TestFixture]
    public class RqlDateTimeTests
    {
        [Test]
        public void TestUtcNow()
        {
            var utcNow = DateTime.UtcNow;
            var rqlUtcNow = RqlDateTime.UtcNow;

            Assert.AreEqual(utcNow.Date, ((DateTime)rqlUtcNow).Date);
        }

        [Test]
        public void TestRoundTripFromString()
        {
            var dateTimeString = "@2014-09-16T10:46:30Z";
            var rqlDateTime = new RqlDateTime(dateTimeString);
            var dateTime = (DateTime)rqlDateTime;

            Assert.AreEqual(DateTimeKind.Utc, dateTime.Kind);
            Assert.AreEqual(2014, dateTime.Year);
            Assert.AreEqual(9, dateTime.Month);
            Assert.AreEqual(16, dateTime.Day);
            Assert.AreEqual(10, dateTime.Hour);
            Assert.AreEqual(46, dateTime.Minute);
            Assert.AreEqual(30, dateTime.Second);
        }

        [Test]
        public void TestRoundTripFromDateTime()
        {
            var originalDateTime = new DateTime(2014, 9, 16, 10, 51, 26, DateTimeKind.Utc);
            var rqlDateTime = new RqlDateTime(originalDateTime);
            var dateTime = (DateTime)rqlDateTime;

            Assert.AreEqual(DateTimeKind.Utc, dateTime.Kind);
            Assert.AreEqual(2014, dateTime.Year);
            Assert.AreEqual(9, dateTime.Month);
            Assert.AreEqual(16, dateTime.Day);
            Assert.AreEqual(10, dateTime.Hour);
            Assert.AreEqual(51, dateTime.Minute);
            Assert.AreEqual(26, dateTime.Second);
        }

        [Test]
        public void TestMustBeUtcDateTime()
        {
            var dateTime = DateTime.Now;

            Assert.Throws<ArgumentException>(() => new RqlDateTime(dateTime));
        }
    }
}

