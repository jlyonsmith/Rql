using System;
using System.Globalization;

namespace Rql
{
    public struct RqlTimeSpan : IFormattable
    {
        private TimeSpan timeSpan;

        // See http://en.wikipedia.org/wiki/ISO_8601#Durations
        public static readonly string FormatPattern = @"'P'd'D''T'hh'H'mm'M'ss'.'fff'S'";

        public RqlTimeSpan(TimeSpan timeSpan) : this()
        {
            this.timeSpan = timeSpan;
        }

        public RqlTimeSpan(int days, int hours, int minutes, int seconds)
        {
            this.timeSpan = new TimeSpan(days, hours, minutes, seconds);
        }

        public RqlTimeSpan(string s) : this()
        {
            InternalParse(s);
        }

        #region IFormattable

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
                return this.timeSpan.ToString(FormatPattern);
            case "~":
            case "G":
            default:
                return "~" + this.timeSpan.ToString(FormatPattern);
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

        public static explicit operator TimeSpan(RqlTimeSpan other)
        {
            return other.timeSpan;
        }

        public void InternalParse(string s)
        {
            if (!s.StartsWith("~"))
                throw new ArgumentException("RQL date/time must start with '~'");

            if (s.Length > 1)
                this.timeSpan = TimeSpan.ParseExact(s.Substring(1), RqlTimeSpan.FormatPattern, CultureInfo.InvariantCulture);
            else
                this.timeSpan = TimeSpan.MinValue;
        }

        public static RqlTimeSpan Parse(string s)
        {
            return new RqlTimeSpan(s);
        }

        public static bool TryParse(string s, out RqlTimeSpan timeSpan)
        {
            try
            {
                timeSpan = RqlTimeSpan.Parse(s);
                return true;
            }
            catch (Exception)
            {
                timeSpan = new RqlTimeSpan(new TimeSpan(0));
                return false;
            }
        }

        public static RqlTimeSpan Zero
        {
            get 
            {
                return new RqlTimeSpan(TimeSpan.Zero);
            }
        }
    }
}

