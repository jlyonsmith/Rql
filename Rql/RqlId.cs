using System;

namespace Rql
{
    public struct RqlId : IFormattable
    {
        private string id;

        public readonly static RqlId Zero = new RqlId();

        public RqlId(string s) : this()
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
                    return id;
                case "$":
                case "G":
                default:
                    return '$' + id;
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
                throw new ArgumentException("RQL id must start with '$' symbol");

            if (s.Length < 2)
                throw new ArgumentException("RQL id must be at least two characters long");

            this.id = s.Substring(1);
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
                id = Zero;
                return false;
            }
        }
    }
}

