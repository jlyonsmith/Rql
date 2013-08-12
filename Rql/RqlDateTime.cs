using System;
using System.Globalization;

namespace Rql
{
    public struct RqlDateTime
    {
        private DateTime dateTime;

        public static readonly string FormatPattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
        public static readonly RqlDateTime Zero = new RqlDateTime(DateTime.MinValue);

        public RqlDateTime(DateTime dateTime) : this()
        {
            this.dateTime = dateTime;
        }
        
        public RqlDateTime(string s) : this()
        {
            InternalParse(s);
        }

        public override string ToString()
        {
            return ToString("@");
        }

        public string ToString(string format)
        {
            switch (format)
            {
                case "@":
                    return "@" + this.dateTime.ToString(FormatPattern);
                case "n":
                    return this.dateTime.ToString(FormatPattern);
                default:
                    return this.dateTime.ToString(format);
            }
        }

        public static explicit operator DateTime(RqlDateTime other)
        {
            return other.dateTime;
        }

        public void InternalParse(string s)
        {
            if (s.StartsWith("@"))
                s = s.Substring(1);

            this.dateTime = DateTime.SpecifyKind(
                DateTime.ParseExact(s, RqlDateTime.FormatPattern, null, DateTimeStyles.None), DateTimeKind.Utc);
        }

        public static RqlDateTime Parse(string s)
        {
            return new RqlDateTime(s);
        }

        public static bool TryParse(string s, out RqlDateTime dateTime)
        {
            try
            {
                dateTime = RqlDateTime.Parse(s);
                return true;
            }
            catch (Exception)
            {
                dateTime = Zero;
                return false;
            }
        }

        public static string GetUtcOffsetForTimeZone(string timeZoneId)
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            TimeSpan utcOffset = tzi.GetUtcOffset(DateTime.Now);
            int hours = utcOffset.Hours;
            return Math.Sign(hours) >= 0 ? "+" : "-" + Math.Abs(hours).ToString("00") + utcOffset.Minutes.ToString("00");
        }
    }
}

