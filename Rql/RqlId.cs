using System;
using System.Numerics;
using System.Text;
using System.Collections;

namespace Rql
{
    public struct RqlId : IFormattable
    {
        // Base 62 digits and reverse lookup values
        static readonly char[] Digits = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        static readonly byte[] DigitValues =
        {
            // 0
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            // 16
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            // 32
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            // 48
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 255, 255, 255, 255, 255, 255,
            // 64
            255, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
            // 80
            51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 255, 255, 255, 255, 255,
            // 96
            255, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            // 112
            25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 255, 255, 255, 255, 255,
        };

        private byte[] id;

        public readonly static RqlId Zero = new RqlId();

        public RqlId(string s) : this()
        {
            InternalParse(s);
        }

        public RqlId(RqlId other)
        {
            this.id = other.id;
        }

        public RqlId(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data must be non-null and at least one byte long");

            id = data;
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

            // Need to keep BigIntegers positive
            byte[] data;

            if (id == null)
            {
                data = new byte[] { 0 };
            }
            else if ((id[id.Length - 1] & 0x80) != 0)
            {
                data = new byte[id.Length + 1];

                Array.Copy(id, data, id.Length);
                data[data.Length - 1] = 0;
            }
            else
            {
                data = id;
            }

            var n = new BigInteger(data);
            var r = n % 62;
            var q = n / 62;
            var sb = new StringBuilder();

            sb.Append(Digits[(int)r]);

            while (q != 0)
            {
                r = q % 62;
                q /= 62;
                sb.Insert(0, Digits[(int)r]);
            }

            switch (format)
            {
            case "n":
                return sb.ToString();
            case "$":
            case "G":
            default:
                return '$' + sb.ToString();
            }
        }

        #endregion

        public byte[] ToByteArray()
        {
            return id;
        }

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
            RqlId other = (RqlId)obj;

            if (Object.ReferenceEquals(other.id, id))
                return true;

            return StructuralComparisons.StructuralComparer.Compare(other.id, id) == 0;
        }

        public bool Equals(RqlId other)
        {
            return StructuralComparisons.StructuralComparer.Compare(other.id, id) == 0;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator==(RqlId a, RqlId b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(RqlId a, RqlId b)
        {
            return !(a == b);
        }

        private void InternalParse(string s)
        {
            if (!s.StartsWith("$"))
                throw new ArgumentException("RQL id must start with '$' symbol");

            BigInteger n;
            BigInteger m = 1;
            int len = s.Length;

            for (int i = len - 1; i >= 1; i--)
            {
                char c = s[i];
                byte d;

                if (c >= DigitValues.Length || (d = DigitValues[c]) == 255)
                    throw new ArgumentException(String.Format("Character '{0}' is not valid in RQL id", c));

                n += m * d;
                m *= 62;
            }

            id = n.ToByteArray();
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

