using System;

namespace Rql
{
    public sealed class RqlId : IFormattable
    {
        private string id;

        private RqlId()
        {
        }

        public RqlId(string s)
        {
            InternalParse(s);
        }

        public RqlId(RqlId other)
        {
            this.id = other.id;
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
                    return id.Substring(1);
                case "$":
                case "G":
                default:
                    return id;
            }
        }

        #endregion

        public override string ToString()
        {
            return ToString("G", null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;

            return id.Equals(((RqlId)obj).id);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        private void InternalParse(string s)
        {
            if (!s.StartsWith("$"))
                this.id = "$" + s;
            else
                this.id = s;
        }
        
        public static RqlId Parse(string s)
        {
            return new RqlId(s);
        }
        
        public static bool TryParse(string s, out RqlId id)
        {
            try
            {
                id = RqlId.Parse(s);
                return true;
            }
            catch (Exception)
            {
                id = null;
                return false;
            }
        }
    }
}

