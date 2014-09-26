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

        private BigInteger id;

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
            {
                id = BigInteger.Zero;
                return;
            }

            byte[] tmp;

            // We need to keep BigIntegers positive
            if ((data[data.Length - 1] & 0x80) != 0)
            {
                tmp = new byte[data.Length + 1];
                Array.Copy(data, tmp, data.Length);
                tmp[tmp.Length - 1] = 0;
            }
            else
                tmp = data;

            id = new BigInteger(tmp);
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

            var r = id % 62;
            var q = id / 62;
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
            return id.ToByteArray();
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

            return other.id == this.id;
        }

        public bool Equals(RqlId other)
        {
            return other.id == this.id;
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

            BigInteger n = 0;
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

            id = n;
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

