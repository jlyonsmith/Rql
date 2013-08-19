using System;
using NUnit.Framework;
using Rql;

namespace Rql.Tests
{
    [TestFixture]
    public class RqlTokenizerTests
    {
        [Test]
        public void TestEmptyString()
        {
            RqlTokenizer tokenizer = new RqlTokenizer("");

            Assert.IsTrue(tokenizer.Next().IsEnd);
        }

        [Test]
        public void TestGoodExpression()
        {
                                                     //0000000000111111111122222222223333333333444444444455555555556666666666777777777788888888889999999999
                                                     //0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            RqlTokenizer tokenizer = new RqlTokenizer("  foo ( a1b2, 123.45, 'xyz',bar(true,false,null),123,-1,$51d1e6,@2013-06-24T15:30:00Z,tit.tat.1) ");

            RqlToken[] expectedTokens = 
            {
                new RqlToken(RqlTokenType.Identifier, 2, "foo"),
                new RqlToken(RqlTokenType.LeftParen, 6),
                new RqlToken(RqlTokenType.Identifier, 8, "a1b2"),
                new RqlToken(RqlTokenType.Comma, 12),
                new RqlToken(RqlTokenType.Constant, 14, 123.45),
                new RqlToken(RqlTokenType.Comma, 20),
                new RqlToken(RqlTokenType.Constant, 22, "xyz"),
                new RqlToken(RqlTokenType.Comma, 27),
                new RqlToken(RqlTokenType.Identifier, 28, "bar"),
                new RqlToken(RqlTokenType.LeftParen, 31),
                new RqlToken(RqlTokenType.Constant, 32, true),
                new RqlToken(RqlTokenType.Comma, 36),
                new RqlToken(RqlTokenType.Constant, 37, false),
                new RqlToken(RqlTokenType.Comma, 42),
                new RqlToken(RqlTokenType.Constant, 43, null),
                new RqlToken(RqlTokenType.RightParen, 47),
                new RqlToken(RqlTokenType.Comma, 48),
                new RqlToken(RqlTokenType.Constant, 49, 123),
                new RqlToken(RqlTokenType.Comma, 52),
                new RqlToken(RqlTokenType.Constant, 53, -1),
                new RqlToken(RqlTokenType.Comma, 55),
                new RqlToken(RqlTokenType.Constant, 56, new RqlId("$51d1e6")),
                new RqlToken(RqlTokenType.Comma, 63),
                new RqlToken(RqlTokenType.Constant, 64, new RqlDateTime("@2013-06-24T15:30:00Z")),
                new RqlToken(RqlTokenType.Comma, 85),
                new RqlToken(RqlTokenType.Identifier, 86, "tit.tat.1"),
                new RqlToken(RqlTokenType.RightParen, 95),
                new RqlToken(RqlTokenType.End, 98),
            };

            RqlToken token = tokenizer.PeekNext();
            RqlToken expectedToken = expectedTokens[0];

            Assert.AreEqual(expectedToken.TokenType, token.TokenType);
            Assert.AreEqual(expectedToken.Offset, token.Offset);
            Assert.AreEqual(expectedToken.Data, token.Data);

            for (int i = 0; i < expectedTokens.Length; i++)
            {
                token = tokenizer.Next();
                expectedToken = expectedTokens[i];

                string s = String.Format(
                    "Token '{0}', Offset {1}, Data {2}", expectedToken.TokenType.ToString(), expectedToken.Offset, 
                    expectedToken.Data == null ? "null" : expectedToken.Data);

                Assert.AreEqual(expectedToken.TokenType, token.TokenType, s);
                Assert.AreEqual(expectedToken.Offset, token.Offset, s);
                Assert.AreEqual(expectedToken.Data, token.Data, s);
            }
            
            token = tokenizer.Next();
            Assert.IsTrue(token.IsEnd);
            token = tokenizer.PeekNext();
            Assert.IsTrue(token.IsEnd);
        }
        
        [Test]
        public void TestBadExpression()
        {
                                                     //0000000000111111111122222222223333333333
                                                     //0123456789012345678901234567890123456789
            RqlTokenizer tokenizer = new RqlTokenizer("  abc ( % ");

            RqlToken[] expectedTokens = 
            {
                new RqlToken(RqlTokenType.Identifier, 2, "abc"),
                new RqlToken(RqlTokenType.LeftParen, 6),
                new RqlToken(RqlTokenType.Error, 8),
            };

            RqlToken token = null;

            for (int i = 0; i < expectedTokens.Length; i++)
            {
                token = tokenizer.Next();

                RqlToken expectedToken = expectedTokens[i];

                Assert.AreEqual(expectedToken.TokenType, token.TokenType);
                Assert.AreEqual(expectedToken.Offset, token.Offset);
                Assert.AreEqual(expectedToken.Data, token.Data);
            }

            token = tokenizer.Next();
            Assert.IsTrue(token.IsError);
            token = tokenizer.PeekNext();
            Assert.IsTrue(token.IsError);
        }

        public void TestBadIdentifier()
        {
                                                     //0000000000111111111122222222223333333333
                                                     //0123456789012345678901234567890123456789
            RqlTokenizer tokenizer = new RqlTokenizer("a..b");
            RqlToken token = null;

            token = tokenizer.Next();
            Assert.IsTrue(token.IsError);
        }
    }
}

