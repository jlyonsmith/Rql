using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Globalization;

namespace Rql
{
    public enum RqlTokenType
    {
        Error,
        Identifier,
        Constant,
        LeftParen,
        RightParen,
        Comma,
        End
    }

    public enum RqlDataType
    {
        None = 0,
        Boolean,
        Integer,
        Double,
        String,
        Id,
        DateTime,
        Document
    }

    public class RqlToken
    {
        public RqlToken(RqlTokenType tokenType, int offset)
        {
            this.Offset = offset;
            this.TokenType = tokenType;
        }

        public RqlToken(RqlTokenType tokenType, int offset, object data) : this(tokenType, offset)
        {
            this.Data = data;
        }

        public static RqlToken End(int offset) { return new RqlToken(RqlTokenType.End, offset); }
        public static RqlToken Comma(int offset) { return new RqlToken(RqlTokenType.Comma, offset); }
        public static RqlToken LeftParen(int offset) { return new RqlToken(RqlTokenType.LeftParen, offset); }
        public static RqlToken RightParen(int offset) { return new RqlToken(RqlTokenType.RightParen, offset); }
        public static RqlToken Error(int offset) { return new RqlToken(RqlTokenType.Error, offset); }
        public static RqlToken Identifier(int offset, string name) { return new RqlToken(RqlTokenType.Identifier, offset, name); }
        public static RqlToken Constant(int offset, object value) { return new RqlToken(RqlTokenType.Constant, offset, value); }

        public bool IsComma { get { return this.TokenType == RqlTokenType.Comma; } }
        public bool IsLeftParen { get { return this.TokenType == RqlTokenType.LeftParen; } }
        public bool IsRightParen { get { return this.TokenType == RqlTokenType.RightParen; } }
        public bool IsError { get { return this.TokenType == RqlTokenType.Error; } }
        public bool IsIdentifier { get { return this.TokenType == RqlTokenType.Identifier; } }
        public bool IsConstant { get { return this.TokenType == RqlTokenType.Constant; } }
        public bool IsEnd { get { return this.TokenType == RqlTokenType.End; } }

        public RqlTokenType TokenType { get; private set; }
        public object Data { get; private set; }
        public int Offset { get; private set; }

        public override string ToString()
        {
            return string.Format("[RqlToken: TokenType={0}, Offset = {1}, Data={2}]", TokenType, Offset, Data);
        }
    }

    public class RqlTokenizer
    {
        private int offset;
        private string input;
        private StringBuilder sb;
        private RqlToken nextToken;
        private RqlToken stopToken;

        public int Offset { get { return offset; } }
        public string Input { get { return input; } }

        public RqlTokenizer(string input)
        {
            this.input = input;
            this.offset = 0;
            this.sb = new StringBuilder(32);
        }

        private char ReadChar()
        {
            char c;

            if (offset < input.Length)
                c = input[offset];
            else
                c ='\0';

            offset++;
            return c;
        }

        private void UnreadChar()
        {
            if (offset > 0)
                offset--;
        }

        public RqlToken Next()
        {
            if (stopToken != null)
                return stopToken;

            if (nextToken != null)
            {
                RqlToken token = nextToken;

                nextToken = null;
                return token;
            }

            char c = ReadChar();

            while (c != '\0' && Char.IsWhiteSpace(c))
                c = ReadChar();

            int tokenOffset = this.offset - 1;
            
            if (c == '\0')
                return (stopToken = RqlToken.End(offset));

            if (Char.IsLetter(c))
            {
                sb.Clear();
                sb.Append(c);

                c = ReadChar();

                bool lastCharWasDot = false;

                while (c != 0 && (Char.IsLetterOrDigit(c)) || c == '.')
                {
                    sb.Append(c);

                    if (c == '.')
                    {
                        if (lastCharWasDot)
                            goto Error;

                        lastCharWasDot = true;
                    }
                    else
                        lastCharWasDot = false;

                    c = ReadChar();
                }

                UnreadChar();

                string s = sb.ToString();

                if (s == "true")
                    return RqlToken.Constant(tokenOffset, (object)true);
                else if (s == "false")
                    return RqlToken.Constant(tokenOffset, (object)false);
                else if (s == "null")
                    return RqlToken.Constant(tokenOffset, null);
                else
                    return RqlToken.Identifier(tokenOffset, s);
            }
            else if (Char.IsDigit(c) || c == '-')
            {
                sb.Clear();
                sb.Append(c);
                c = ReadChar();

                if (sb[0] == '-' && !Char.IsDigit(c))
                    goto Error;

                while (Char.IsDigit(c))
                {
                    sb.Append(c);
                    c = ReadChar();
                }

                if (c == '.')
                {
                    sb.Append(c);
                    c = ReadChar();

                    while (Char.IsDigit(c) )
                    {
                        sb.Append(c);
                        c = ReadChar();
                    }
                    
                    UnreadChar();

                    Double n;

                    if (Double.TryParse(sb.ToString(), out n))
                        return RqlToken.Constant(tokenOffset, n);
                }
                else
                {
                    UnreadChar();

                    Int32 n;

                    if (Int32.TryParse(sb.ToString(), out n))
                        return RqlToken.Constant(tokenOffset, n);
                }
            }
            else if (c == '$')
            {
                sb.Clear();
                sb.Append(c);
                c = ReadChar();

                while (Char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    c = ReadChar();
                }

                UnreadChar();

                if (sb.Length > 0)
                    return RqlToken.Constant(tokenOffset, new RqlId(sb.ToString()));
            }
            else if (c == '@')
            {
                sb.Clear();
                sb.Append(c);
                c = ReadChar();

                while (c != 'Z')
                {
                    sb.Append(c);
                    c = ReadChar();
                }

                sb.Append(c);

                RqlDateTime dateTime;

                if (RqlDateTime.TryParse(sb.ToString(), out dateTime))
                    return RqlToken.Constant(tokenOffset, dateTime);
            }
            else if (c == '\'')
            {
                sb.Clear();
                c = ReadChar();

                while (c != '\0' && c != '\'')
                {
                    sb.Append(c);
                    c = ReadChar();
                }

                if (c == '\'')
                    return RqlToken.Constant(tokenOffset, sb.ToString());
            }
            else
            {
                switch ((char)c)
                {
                    case ',':
                        return RqlToken.Comma(tokenOffset);
                    case '(':
                        return RqlToken.LeftParen(tokenOffset);
                    case ')':
                        return RqlToken.RightParen(tokenOffset);
                }
            }

        Error:
            return (stopToken = RqlToken.Error(tokenOffset));
        }

        public RqlToken PeekNext()
        {
            if (stopToken != null)
                return stopToken;

            if (nextToken == null)
            {
                nextToken = Next();
            }

            return nextToken;
        }
    }
}

