// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Wmhelp.XPath2.MS;
using Wmhelp.XPath2.Properties;
using Wmhelp.XPath2.yyParser;

namespace Wmhelp.XPath2
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public class Tokenizer : yyInput
    {
        private readonly TextReader m_reader;
        private int m_position;

        private LexerState m_state = LexerState.Default;
        private readonly StringBuilder m_buffer = new StringBuilder();
        private readonly Stack<LexerState> m_states = new Stack<LexerState>();
        private readonly Queue<CurrentToken> m_token = new Queue<CurrentToken>();

        internal Tokenizer()
        {
            LineNo = ColNo = 1;
            m_bookmark = new int[5];
        }

        internal Tokenizer(string strInput)
            : this()
        {
            m_reader = new StringReader(strInput);
        }

        internal Tokenizer(TextReader reader)
            : this()
        {
            m_reader = reader;
        }

        private int m_anchor;
        private readonly int[] m_bookmark;
        private int m_length;

        private object m_value;

        public int LineNo { get; protected set; }
        public int ColNo { get; protected set; }

        public int Position => m_position;

        private struct CurrentToken
        {
            public int token;
            public object value;
            public int anchor;
            public int length;
        }

        public enum LexerState
        {
            Default,
            Operator,
            SingleType,
            ItemType,
            KindTest,
            KindTestForPi,
            CloseKindTest,
            OccurrenceIndicator,
            VarName
        };

        protected char Peek(int lookahead)
        {
            while (lookahead >= m_buffer.Length && m_reader.Peek() != -1)
                m_buffer.Append((char) m_reader.Read());
            if (lookahead < m_buffer.Length)
                return m_buffer[lookahead];
            else
                return '\0';
        }

        protected char Read()
        {
            char ch;
            if (m_buffer.Length > 0)
            {
                ch = m_buffer[0];
                m_buffer.Remove(0, 1);
            }
            else
            {
                int c = m_reader.Read();
                if (c == -1)
                    return '\0';
                else
                    ch = (char) c;
            }
            if (ch != '\r')
            {
                if (ch == '\n')
                {
                    LineNo++;
                    ColNo = 1;
                }
                else
                    ++ColNo;
            }
            m_position++;
            return ch;
        }

        private void BeginToken()
        {
            m_anchor = Position;
            m_length = 0;
        }

        private void EndToken()
        {
            m_length = Position - m_anchor;
        }

        private void BeginToken(int anchor)
        {
            m_anchor = anchor;
            m_length = 0;
        }

        private void EndToken(string s)
        {
            m_length = s.Length;
        }

        private void ConsumeNumber()
        {
            int tok = Token.IntegerLiteral;
            StringBuilder sb = new StringBuilder();
            BeginToken();
            while (XmlCharType.Instance.IsDigit(Peek(0)))
                sb.Append(Read());
            if (Peek(0) == '.')
            {
                tok = Token.DecimalLiteral;
                sb.Append(Read());
                while (XmlCharType.Instance.IsDigit(Peek(0)))
                    sb.Append(Read());
            }
            char c = Peek(0);
            if (c == 'E' || c == 'e')
            {
                tok = Token.DoubleLiteral;
                sb.Append(Read());
                c = Peek(0);
                if (c == '+' || c == '-')
                    sb.Append(Read());
                while (XmlCharType.Instance.IsDigit(Peek(0)))
                    sb.Append(Read());
            }
            EndToken();
            string s = sb.ToString();
            switch (tok)
            {
                case Token.IntegerLiteral:
                    ConsumeToken(tok, (Integer) Decimal.Parse(s, NumberFormatInfo.InvariantInfo));
                    break;

                case Token.DecimalLiteral:
                    ConsumeToken(tok, Decimal.Parse(s, NumberFormatInfo.InvariantInfo));
                    break;

                case Token.DoubleLiteral:
                    ConsumeToken(tok, Double.Parse(s, NumberFormatInfo.InvariantInfo));
                    break;
            }
        }

        private void ConsumeLiteral()
        {
            BeginToken();
            char qoute = Read();
            StringBuilder sb = new StringBuilder();
            char c;
            while ((c = Peek(0)) != qoute || Peek(1) == qoute)
            {
                if (Peek(0) == 0)
                    return;
                if (c == qoute && Peek(1) == qoute)
                    Read();
                sb.Append(Read());
            }
            Read();
            EndToken();
            ConsumeToken(Token.StringLiteral,
                CoreFuncs.NormalizeStringValue(sb.ToString(), false, true));
        }

        private void ConsumeNCName()
        {
            char c;
            StringBuilder sb = new StringBuilder();
            BeginToken();
            while ((c = Peek(0)) != 0 && XmlCharType.Instance.IsNCNameChar(c))
                sb.Append(Read());
            EndToken();
            ConsumeToken(Token.NCName, sb.ToString());
        }

        private void ConsumeQName()
        {
            StringBuilder sb = new StringBuilder();
            char c;
            BeginToken();
            while ((c = Peek(0)) != 0 && XmlCharType.Instance.IsNameChar(c))
                sb.Append(Read());
            EndToken();
            ConsumeToken(Token.QName, sb.ToString());
        }


        private void ConsumeChar(char token)
        {
            CurrentToken curr;
            curr.token = token;
            curr.value = null;
            curr.anchor = m_anchor;
            curr.length = 1;
            m_token.Enqueue(curr);
        }

        private void ConsumeToken(int token)
        {
            ConsumeToken(token, null);
        }

        private void ConsumeToken(int token, int anchor, int length)
        {
            ConsumeToken(token, null, anchor, length);
        }

        private void ConsumeToken(int token, object value, int anchor, int length)
        {
            CurrentToken curr;
            curr.token = token;
            curr.value = value;
            curr.anchor = anchor;
            curr.length = length;
            m_token.Enqueue(curr);
        }

        private void ConsumeToken(int token, object value)
        {
            CurrentToken curr;
            curr.token = token;
            curr.value = value;
            curr.anchor = m_anchor;
            curr.length = m_length;
            m_token.Enqueue(curr);
        }

        private bool MatchText(string text)
        {
            for (int k = 0; k < text.Length; k++)
            {
                char ch = Peek(k);
                if (ch == 0 || ch != text[k])
                    return false;
            }
            for (int k = 0; k < text.Length; k++)
                Read();
            return true;
        }

        private bool MatchIdentifer(params string[] identifer)
        {
            int i = 0;
            for (int sp = 0; sp < identifer.Length; sp++)
            {
                char c;
                while (true)
                {
                    if ((c = Peek(i)) != 0 && XmlCharType.Instance.IsWhiteSpace(c))
                    {
                        while ((c = Peek(i)) != 0 && XmlCharType.Instance.IsWhiteSpace(c))
                            i++;
                        continue;
                    }
                    if (Peek(i) == '(' && Peek(i + 1) == ':')
                    {
                        int n = 1;
                        i += 2;
                        while (true)
                        {
                            c = Peek(i);
                            if (c == 0)
                                break;
                            else if (c == '(' && Peek(i + 1) == ':')
                            {
                                i += 2;
                                n++;
                            }
                            else if (c == ':' && Peek(i + 1) == ')')
                            {
                                i += 2;
                                if (--n == 0)
                                    break;
                            }
                            else
                                i++;
                        }
                        continue;
                    }
                    break;
                }
                string s = identifer[sp];
                m_bookmark[sp] = Position + i;
                if (s.Length > 0)
                {
                    for (int k = 0; k < s.Length; k++, i++)
                        if ((c = Peek(i)) == 0 || c != s[k])
                            return false;
                    if (XmlCharType.Instance.IsStartNCNameChar(s[0]) &&
                        XmlCharType.Instance.IsNCNameChar(Peek(i)))
                        return false;
                }
            }
            while (i-- > 0)
                Read();
            return true;
        }

        private void SkipWhitespace()
        {
            do
            {
                if (XmlCharType.Instance.IsWhiteSpace(Peek(0)))
                {
                    char c;
                    while ((c = Peek(0)) != 0 && XmlCharType.Instance.IsWhiteSpace(c))
                        Read();
                    continue;
                }
                if (Peek(0) == '(' && Peek(1) == ':')
                {
                    Read();
                    Read();
                    int n = 1;
                    while (true)
                    {
                        char c = Peek(0);
                        if (c == 0)
                            break;
                        else if (c == '(' && Peek(1) == ':')
                        {
                            Read();
                            Read();
                            n++;
                        }
                        else if (c == ':' && Peek(1) == ')')
                        {
                            Read();
                            Read();
                            if (--n == 0)
                                break;
                        }
                        else
                            Read();
                    }
                    continue;
                }
                break;
            } while (true);
        }

        private void DefaultState()
        {
            SkipWhitespace();
            BeginToken();
            char c = Peek(0);
            if (c == '\0')
                ConsumeToken(0); // EOF
            else if (c == '.')
            {
                if (Peek(1) == '.')
                {
                    Read();
                    Read();
                    EndToken();
                    ConsumeToken(Token.DOUBLE_PERIOD);
                }
                else if (XmlCharType.Instance.IsDigit(Peek(1)))
                    ConsumeNumber();
                else
                    ConsumeChar(Read());
                m_state = LexerState.Operator;
            }
            else if (c == ')')
            {
                ConsumeChar(Read());
                SkipWhitespace();
                BeginToken();
                m_state = LexerState.Operator;
            }
            else if (c == '*')
            {
                ConsumeChar(Read());
                if (Peek(0) == ':')
                {
                    BeginToken();
                    ConsumeChar(Read());
                    c = Peek(0);
                    if (c != 0 && XmlCharType.Instance.IsStartNCNameChar(c))
                        ConsumeNCName();
                    else
                        throw new XPath2Exception("XPST0003", Resources.ExpectedNCName);
                }
                m_state = LexerState.Operator;
            }
            else if (c == ';' || c == ',' || c == '(' || c == '-' || c == '+' || c == '@' || c == '~')
                ConsumeChar(Read());
            else if (c == '/')
            {
                if (Peek(1) == '/')
                {
                    Read();
                    Read();
                    EndToken();
                    ConsumeToken(Token.DOUBLE_SLASH);
                }
                else
                    ConsumeChar(Read());
            }
            else if (MatchIdentifer("if", "("))
            {
                EndToken("if");
                ConsumeToken(Token.IF);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
            }
            else if (MatchIdentifer("for"))
            {
                EndToken();
                ConsumeToken(Token.FOR);
                SkipWhitespace();
                BeginToken();
                if (Peek(0) == '$')
                    ConsumeChar(Read());
                else
                    throw new XPath2Exception("XPST0003", Resources.ExpectedVariablePrefix, "for");
                m_state = LexerState.VarName;
            }
            else if (MatchIdentifer("some"))
            {
                EndToken();
                ConsumeToken(Token.SOME);
                SkipWhitespace();
                BeginToken();
                if (Peek(0) == '$')
                    ConsumeChar(Read());
                else
                    throw new XPath2Exception("XPST0003", Resources.ExpectedVariablePrefix, "some");
                m_state = LexerState.VarName;
            }
            else if (MatchIdentifer("every"))
            {
                EndToken();
                ConsumeToken(Token.EVERY);
                SkipWhitespace();
                BeginToken();
                if (Peek(0) == '$')
                    ConsumeChar(Read());
                else
                    throw new XPath2Exception("XPST0003", Resources.ExpectedVariablePrefix, "every");
                m_state = LexerState.VarName;
            }
            else if (c == '$')
            {
                ConsumeChar(Read());
                m_state = LexerState.VarName;
            }
            else if (MatchIdentifer("element", "("))
            {
                EndToken("element");
                ConsumeToken(Token.ELEMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("attribute", "("))
            {
                EndToken("attribute");
                ConsumeToken(Token.ATTRIBUTE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("schema-element", "("))
            {
                EndToken("schema-element");
                ConsumeToken(Token.SCHEMA_ELEMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("schema-attribute", "("))
            {
                EndToken("schema-attribute");
                ConsumeToken(Token.SCHEMA_ATTRIBUTE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("comment", "("))
            {
                EndToken("comment");
                ConsumeToken(Token.COMMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("text", "("))
            {
                EndToken("text");
                ConsumeToken(Token.TEXT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("node", "("))
            {
                EndToken("node");
                ConsumeToken(Token.NODE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("document-node", "("))
            {
                EndToken("document-node");
                ConsumeToken(Token.DOCUMENT_NODE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("processing-instruction", "("))
            {
                EndToken("processing-instruction");
                ConsumeToken(Token.PROCESSING_INSTRUCTION);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.Operator);
                m_state = LexerState.KindTestForPi;
            }
            else if (MatchIdentifer("ancestor-or-self", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_ANCESTOR_OR_SELF);
            }
            else if (MatchIdentifer("ancestor", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_ANCESTOR);
            }
            else if (MatchIdentifer("attribute", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_ATTRIBUTE);
            }
            else if (MatchIdentifer("child", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_CHILD);
            }
            else if (MatchIdentifer("descendant-or-self", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_DESCENDANT_OR_SELF);
            }
            else if (MatchIdentifer("descendant", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_DESCENDANT);
            }
            else if (MatchIdentifer("following-sibling", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_FOLLOWING_SIBLING);
            }
            else if (MatchIdentifer("following", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_FOLLOWING);
            }
            else if (MatchIdentifer("parent", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_PARENT);
            }
            else if (MatchIdentifer("preceding-sibling", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_PRECEDING_SIBLING);
            }
            else if (MatchIdentifer("preceding", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_PRECEDING);
            }
            else if (MatchIdentifer("self", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_SELF);
            }
            else if (MatchIdentifer("namespace", "::"))
            {
                EndToken();
                ConsumeToken(Token.AXIS_NAMESPACE);
            }
            else if (c == '"' || c == '\'')
            {
                ConsumeLiteral();
                m_state = LexerState.Operator;
            }
            else if (XmlCharType.Instance.IsDigit(c))
            {
                ConsumeNumber();
                m_state = LexerState.Operator;
            }
            else if (XmlCharType.Instance.IsStartNameChar(c))
            {
                StringBuilder sb = new StringBuilder();
                while ((c = Peek(0)) != 0 && XmlCharType.Instance.IsNCNameChar(c))
                    sb.Append(Read());
                if (Peek(0) == ':')
                {
                    if (Peek(1) == '*')
                    {
                        EndToken();
                        ConsumeToken(Token.NCName, sb.ToString());
                        BeginToken();
                        ConsumeChar(Read());
                        BeginToken();
                        ConsumeChar(Read());
                        m_state = LexerState.Operator;
                    }
                    else
                    {
                        while ((c = Peek(0)) != 0 && XmlCharType.Instance.IsNameChar(c))
                            sb.Append(Read());
                        EndToken();
                        ConsumeToken(Token.QName, sb.ToString());
                        SkipWhitespace();
                        if (Peek(0) != '(')
                            m_state = LexerState.Operator;
                    }
                }
                else
                {
                    EndToken();
                    int anchor = m_anchor;
                    int length = m_length;
                    string ncname = sb.ToString();
                    ConsumeToken(Token.QName, ncname);
                    SkipWhitespace();
                    if (Peek(0) != '(')
                        m_state = LexerState.Operator;
                }
            }
        }

        private void VarNameState()
        {
            SkipWhitespace();
            if (Peek(0) == 0)
                return;
            BeginToken();
            char c = Peek(0);
            if (XmlCharType.Instance.IsNCNameChar(c))
            {
                string prefix = String.Empty;
                StringBuilder sb = new StringBuilder();
                while ((c = Peek(0)) != 0 && XmlCharType.Instance.IsNCNameChar(c))
                    sb.Append(Read());
                if (Peek(0) == ':' && XmlCharType.Instance.IsNCNameChar(Peek(1)))
                {
                    prefix = sb.ToString();
                    Read();
                    sb = new StringBuilder();
                    while ((c = Peek(0)) != 0 && XmlCharType.Instance.IsNCNameChar(c))
                        sb.Append(Read());
                }
                EndToken();
                ConsumeToken(Token.VarName, new VarName(prefix, sb.ToString()));
                m_state = LexerState.Operator;
            }
        }

        private void OperatorState()
        {
            SkipWhitespace();
            BeginToken();
            char c = Peek(0);
            if (c == 0)
                ConsumeToken(0);
            else if (c == ';' || c == ',' || c == '=' || c == '+' || c == '-' || c == '[' || c == '|')
            {
                ConsumeChar(Read());
                if (c == '[')
                    m_states.Push(m_state);
                m_state = LexerState.Default;
            }
            else if (c == '*')
            {
                Read();
                EndToken();
                ConsumeToken(Token.ML);
                m_state = LexerState.Default;
            }
            else if (c == ':' && Peek(1) == '=')
            {
                ConsumeChar(Read());
                BeginToken();
                ConsumeChar(Read());
                m_state = LexerState.Default;
            }
            else if (c == '!' && Peek(1) == '=')
            {
                ConsumeChar(Read());
                BeginToken();
                ConsumeChar(Read());
                m_state = LexerState.Default;
            }
            else if (c == '>')
            {
                ConsumeChar(Read());
                if (Peek(0) == '=' || Peek(0) == '>')
                {
                    BeginToken();
                    ConsumeChar(Read());
                }
                m_state = LexerState.Default;
            }
            else if (c == '<')
            {
                ConsumeChar(Read());
                if (Peek(0) == '=' || Peek(0) == '<')
                {
                    BeginToken();
                    ConsumeChar(Read());
                }
                m_state = LexerState.Default;
            }
            else if (c == '/')
            {
                if (Peek(1) == '/')
                {
                    Read();
                    Read();
                    EndToken();
                    ConsumeToken(Token.DOUBLE_SLASH);
                }
                else
                    ConsumeChar(Read());
                m_state = LexerState.Default;
            }
            else if (c == ')')
            {
                ConsumeChar(Read());
                SkipWhitespace();
                BeginToken();
            }
            else if (c == '?')
                ConsumeChar(Read());
            else if (c == ']')
            {
                ConsumeChar(Read());
                m_state = m_states.Pop();
            }
            else if (MatchIdentifer("then"))
            {
                EndToken();
                ConsumeToken(Token.THEN);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("else"))
            {
                EndToken();
                ConsumeToken(Token.ELSE);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("and"))
            {
                EndToken();
                ConsumeToken(Token.AND);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("div"))
            {
                EndToken();
                ConsumeToken(Token.DIV);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("except"))
            {
                EndToken();
                ConsumeToken(Token.EXCEPT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("idiv"))
            {
                EndToken();
                ConsumeToken(Token.IDIV);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("intersect"))
            {
                EndToken();
                ConsumeToken(Token.INTERSECT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("mod"))
            {
                EndToken();
                ConsumeToken(Token.MOD);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("or"))
            {
                EndToken();
                ConsumeToken(Token.OR);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("return"))
            {
                EndToken();
                ConsumeToken(Token.RETURN);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("satisfies"))
            {
                EndToken();
                ConsumeToken(Token.SATISFIES);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("to"))
            {
                EndToken();
                ConsumeToken(Token.TO);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("union"))
            {
                EndToken();
                ConsumeToken(Token.UNION);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("castable", "as"))
            {
                EndToken();
                ConsumeToken(Token.CASTABLE_AS);
                m_state = LexerState.SingleType;
            }
            else if (MatchIdentifer("cast", "as"))
            {
                EndToken();
                ConsumeToken(Token.CAST_AS);
                m_state = LexerState.SingleType;
            }
            else if (MatchIdentifer("instance", "of"))
            {
                EndToken();
                ConsumeToken(Token.INSTANCE_OF);
                m_state = LexerState.ItemType;
            }
            else if (MatchIdentifer("treat", "as"))
            {
                EndToken();
                ConsumeToken(Token.TREAT_AS);
                m_state = LexerState.ItemType;
            }
            else if (MatchIdentifer("in"))
            {
                EndToken();
                ConsumeToken(Token.IN);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("is"))
            {
                EndToken();
                ConsumeToken(Token.IS);
                m_state = LexerState.Default;
            }
            else if (c == '$')
            {
                ConsumeChar(Read());
                m_state = LexerState.VarName;
            }
            else if (MatchIdentifer("for"))
            {
                EndToken();
                ConsumeToken(Token.FOR);
                SkipWhitespace();
                BeginToken();
                if (Peek(0) == '$')
                    ConsumeChar(Read());
                else
                    throw new XPath2Exception("", Resources.ExpectedVariablePrefix, "for");
                m_state = LexerState.VarName;
            }
            else if (MatchIdentifer("eq"))
            {
                EndToken();
                ConsumeToken(Token.EQ);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("ge"))
            {
                EndToken();
                ConsumeToken(Token.GE);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("gt"))
            {
                EndToken();
                ConsumeToken(Token.GT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("le"))
            {
                EndToken();
                ConsumeToken(Token.LE);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("lt"))
            {
                EndToken();
                ConsumeToken(Token.LT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("ne"))
            {
                EndToken();
                ConsumeToken(Token.NE);
                m_state = LexerState.Default;
            }
            else if (c == '"' || c == '\'')
                ConsumeLiteral();
        }

        #region yyInput Members

        public bool advance()
        {
            return m_token.Count > 0 || Peek(0) != 0;
        }

        private void SingleTypeState()
        {
            SkipWhitespace();
            if (Peek(0) == 0)
                return;
            if (XmlCharType.Instance.IsNameChar(Peek(0)))
            {
                ConsumeQName();
                m_state = LexerState.Operator;
            }
        }

        private void ItemTypeState()
        {
            SkipWhitespace();
            if (Peek(0) == 0)
                return;
            BeginToken();
            char c = Peek(0);
            if (c == '$')
            {
                ConsumeChar(Read());
                m_state = LexerState.VarName;
            }
            else if (MatchIdentifer("empty-sequence", "(", ")"))
            {
                EndToken();
                ConsumeToken(Token.EMPTY_SEQUENCE);
                m_state = LexerState.Operator;
            }
            else if (MatchIdentifer("element", "("))
            {
                EndToken("element");
                ConsumeToken(Token.ELEMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("attribute", "("))
            {
                EndToken("attribute");
                ConsumeToken(Token.ATTRIBUTE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("schema-element", "("))
            {
                EndToken("schema-element");
                ConsumeToken(Token.SCHEMA_ELEMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("schema-attribute", "("))
            {
                EndToken("schema-attribute");
                ConsumeToken(Token.SCHEMA_ATTRIBUTE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("comment", "("))
            {
                EndToken("comment");
                ConsumeToken(Token.COMMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("text", "("))
            {
                EndToken("text");
                ConsumeToken(Token.TEXT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("node", "("))
            {
                EndToken("node");
                ConsumeToken(Token.NODE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("document-node", "("))
            {
                EndToken("document-node");
                ConsumeToken(Token.DOCUMENT_NODE);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTest;
            }
            else if (MatchIdentifer("processing-instruction", "("))
            {
                EndToken("processing-instruction");
                ConsumeToken(Token.PROCESSING_INSTRUCTION);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.OccurrenceIndicator);
                m_state = LexerState.KindTestForPi;
            }
            else if (MatchIdentifer("item", "(", ")"))
            {
                EndToken();
                ConsumeToken(Token.ITEM);
                m_state = LexerState.OccurrenceIndicator;
            }
            else if (c == ';')
            {
                ConsumeChar(Read());
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("then"))
            {
                EndToken();
                ConsumeToken(Token.THEN);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("else"))
            {
                EndToken();
                ConsumeToken(Token.ELSE);
                m_state = LexerState.Default;
            }
            else if (c == '=' || c == '(' || c == '[' || c == '|')
            {
                ConsumeChar(Read());
                if (c == '[')
                    m_states.Push(m_state);
                m_state = LexerState.Default;
            }
            else if (c == ':' && Peek(1) == '=')
            {
                ConsumeChar(Read());
                BeginToken();
                ConsumeChar(Read());
                m_state = LexerState.Default;
            }
            else if (c == '!' && Peek(1) == '=')
            {
                ConsumeChar(Read());
                BeginToken();
                ConsumeChar(Read());
                m_state = LexerState.Default;
            }
            else if (c == '>')
            {
                ConsumeChar(Read());
                if (Peek(0) == '=' || Peek(0) == '>')
                {
                    BeginToken();
                    ConsumeChar(Read());
                }
                m_state = LexerState.Default;
            }
            else if (c == '<')
            {
                ConsumeChar(Read());
                if (Peek(0) == '=' || Peek(0) == '<')
                {
                    BeginToken();
                    ConsumeChar(Read());
                }
                m_state = LexerState.Default;
            }
            else if (c == ')')
            {
                ConsumeChar(Read());
                SkipWhitespace();
                BeginToken();
            }
            else if (MatchIdentifer("external"))
            {
                EndToken();
                ConsumeToken(Token.EXCEPT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("and"))
            {
                EndToken();
                ConsumeToken(Token.AND);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("div"))
            {
                EndToken();
                ConsumeToken(Token.DIV);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("except"))
            {
                EndToken();
                ConsumeToken(Token.EXCEPT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("eq"))
            {
                EndToken();
                ConsumeToken(Token.EQ);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("ge"))
            {
                EndToken();
                ConsumeToken(Token.GE);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("gt"))
            {
                EndToken();
                ConsumeToken(Token.GT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("le"))
            {
                EndToken();
                ConsumeToken(Token.LE);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("lt"))
            {
                EndToken();
                ConsumeToken(Token.LT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("ne"))
            {
                EndToken();
                ConsumeToken(Token.NE);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("idiv"))
            {
                EndToken();
                ConsumeToken(Token.IDIV);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("intersect"))
            {
                EndToken();
                ConsumeToken(Token.INTERSECT);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("mod"))
            {
                EndToken();
                ConsumeToken(Token.MOD);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("or"))
            {
                EndToken();
                ConsumeToken(Token.OR);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("return"))
            {
                EndToken();
                ConsumeToken(Token.RETURN);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("satisfies"))
            {
                EndToken();
                ConsumeToken(Token.SATISFIES);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("to"))
            {
                EndToken();
                ConsumeToken(Token.TO);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("union"))
            {
                EndToken();
                ConsumeToken(Token.UNION);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("castable", "as"))
            {
                EndToken();
                ConsumeToken(Token.CASTABLE_AS);
                m_state = LexerState.SingleType;
            }
            else if (MatchIdentifer("cast", "as"))
            {
                EndToken();
                ConsumeToken(Token.CAST_AS);
                m_state = LexerState.SingleType;
            }
            else if (MatchIdentifer("instance", "of"))
            {
                EndToken();
                ConsumeToken(Token.INSTANCE_OF);
            }
            else if (MatchIdentifer("treat", "as"))
            {
                EndToken();
                ConsumeToken(Token.TREAT_AS);
            }
            else if (MatchIdentifer("in"))
            {
                EndToken();
                ConsumeToken(Token.IN);
                m_state = LexerState.Default;
            }
            else if (MatchIdentifer("is"))
            {
                EndToken();
                ConsumeToken(Token.IS);
                m_state = LexerState.Default;
            }
            else if (XmlCharType.Instance.IsNameChar(c))
            {
                ConsumeQName();
                m_state = LexerState.OccurrenceIndicator;
            }
        }

        private void KindTestState()
        {
            SkipWhitespace();
            if (Peek(0) == 0)
                return;
            BeginToken();
            char c = Peek(0);
            if (c == '{')
            {
                ConsumeChar(Read());
                m_states.Push(LexerState.Operator);
                m_state = LexerState.Default;
            }
            else if (c == ')')
            {
                ConsumeChar(Read());
                m_state = m_states.Pop();
            }
            else if (c == '*')
            {
                ConsumeChar(Read());
                m_state = LexerState.CloseKindTest;
            }
            else if (MatchIdentifer("element", "("))
            {
                EndToken("element");
                ConsumeToken(Token.ELEMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.KindTest);
            }
            else if (MatchIdentifer("schema-element", "("))
            {
                EndToken("schema-element");
                ConsumeToken(Token.SCHEMA_ELEMENT);
                BeginToken(m_bookmark[1]);
                ConsumeChar('(');
                m_states.Push(LexerState.KindTest);
            }
            else if (XmlCharType.Instance.IsNameChar(c))
            {
                ConsumeQName();
                m_state = LexerState.CloseKindTest;
            }
        }

        private void KindTestForPiState()
        {
            SkipWhitespace();
            if (Peek(0) == 0)
                return;
            char c = Peek(0);
            BeginToken();
            if (c == ')')
            {
                ConsumeChar(Read());
                m_state = m_states.Pop();
            }
            else if (XmlCharType.Instance.IsNCNameChar(c))
                ConsumeNCName();
            else if (c == '\'' || c == '"')
                ConsumeLiteral();
        }

        private void CloseKindTestState()
        {
            SkipWhitespace();
            if (Peek(0) == 0)
                return;
            char c = Peek(0);
            BeginToken();
            if (c == ')')
            {
                ConsumeChar(Read());
                m_state = m_states.Pop();
            }
            else if (c == ',')
            {
                ConsumeChar(Read());
                m_state = LexerState.KindTest;
            }
            else if (c == '{')
            {
                ConsumeChar(Read());
                m_states.Push(LexerState.Operator);
                m_state = LexerState.Default;
            }
            else if (c == '?')
                ConsumeChar(Read());
        }

        private void OccurrenceIndicatorState()
        {
            SkipWhitespace();
            BeginToken();
            char c = Peek(0);
            if (c == '*')
            {
                Read();
                EndToken();
                ConsumeToken(Token.Indicator1);
            }
            else if (c == '+')
            {
                Read();
                EndToken();
                ConsumeToken(Token.Indicator2);
            }
            else if (c == '?')
            {
                Read();
                EndToken();
                ConsumeToken(Token.Indicator3);
            }
            m_state = LexerState.Operator;
            OperatorState();
        }

        private void EnterState()
        {
            switch (m_state)
            {
                case LexerState.Default:
                    DefaultState();
                    break;

                case LexerState.Operator:
                    OperatorState();
                    break;

                case LexerState.VarName:
                    VarNameState();
                    break;


                case LexerState.SingleType:
                    SingleTypeState();
                    break;

                case LexerState.ItemType:
                    ItemTypeState();
                    break;

                case LexerState.KindTest:
                    KindTestState();
                    break;

                case LexerState.KindTestForPi:
                    KindTestForPiState();
                    break;

                case LexerState.CloseKindTest:
                    CloseKindTestState();
                    break;

                case LexerState.OccurrenceIndicator:
                    OccurrenceIndicatorState();
                    break;
            }
        }

        public int token()
        {
            if (m_token.Count == 0)
            {
                EnterState();
                if (m_token.Count == 0)
                {
                    m_value = null;
                    return Token.yyErrorCode;
                }
            }
            CurrentToken curr = m_token.Dequeue();
            m_value = curr.value;
            CurrentPos = curr.anchor;
            CurrentLength = curr.length;
            return curr.token;
        }

        public object value()
        {
            return m_value;
        }

        #endregion

        public int CurrentPos { get; private set; }

        public int CurrentLength { get; private set; }

        public class VarName
        {
            public VarName(string prefix, string localName)
            {
                Prefix = prefix;
                LocalName = localName;
            }

            public String Prefix { get; }

            public String LocalName { get; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (Prefix != "")
                {
                    sb.Append(Prefix);
                    sb.Append(':');
                }
                sb.Append(LocalName);
                return sb.ToString();
            }
        }
    }
}