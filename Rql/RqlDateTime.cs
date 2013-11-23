using System;
using System.Globalization;

namespace Rql
{
    public struct RqlDateTime : IFormattable
    {
        private DateTime dateTime;

        public static readonly string FormatPattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        public RqlDateTime(DateTime dateTime) : this()
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Argument must be a UTC time");

            this.dateTime = dateTime;
        }

        public RqlDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            this.dateTime = DateTime.SpecifyKind(new DateTime(year, month, day, hour, minute, second), DateTimeKind.Utc);
        }
        
        public RqlDateTime(string s) : this()
        {
            InternalParse(s);
        }

        #region IFormattable implementation

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider != null)
            {
                ICustomFormatter formatter = formatProvider.GetFormat(this.GetType()) as ICustomFormatter;

                if (formatter != null)
                    return formatter.Format(format, this, formatProvider);
            }

            if (format == null) 
                format = "G";

            switch (format)
            {
                case "n":
                    return this.dateTime.ToString(FormatPattern);
                case "@":
                case "G":
                default:
                    return "@" + this.dateTime.ToString(FormatPattern);
            }
        }

        #endregion

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        public static explicit operator DateTime(RqlDateTime other)
        {
            return other.dateTime;
        }

        public void InternalParse(string s)
        {
            if (!s.StartsWith("@"))
                throw new ArgumentException("RQL date/time must start with '@' symbol");

            this.dateTime = DateTime.SpecifyKind(
                DateTime.ParseExact(s.Substring(1), RqlDateTime.FormatPattern, null, DateTimeStyles.None), DateTimeKind.Utc);
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
                dateTime = new RqlDateTime(new DateTime(0, DateTimeKind.Utc));
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

        public static RqlDateTime UtcNow
        {
            get 
            {
                return new RqlDateTime(DateTime.UtcNow);
            }
        }
    }
}

