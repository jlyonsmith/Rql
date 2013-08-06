using System;

namespace Rql
{
    public sealed class RqlId
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

        public override string ToString()
        {
            return "$" + id;
        }

        public static explicit operator String(RqlId other)
        {
            return other.id;
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
            if (s.StartsWith("$"))
                s = s.Substring(1);

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

