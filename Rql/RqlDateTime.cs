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
            return "@" + this.dateTime.ToString(FormatPattern);
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
    }
}

