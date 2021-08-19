// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 16 "Xpath.y"
  
// ReSharper Disable All
#pragma warning disable 162

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using System.Xml;
using System.Xml.Schema;

using Wmhelp.XPath2.AST;
using Wmhelp.XPath2.Proxy;
using Wmhelp.XPath2.MS;

namespace Wmhelp.XPath2
{
    internal class YYParser
    {         
        private XPath2Context context;

        public YYParser(XPath2Context context)
        {
            errorText = new StringWriter();
            this.context = context;
        }

        public object yyparseSafe(Tokenizer tok)
        {
            return yyparseSafe(tok, null);
        }

        public object yyparseSafe(Tokenizer tok, object yyDebug)
        { 
            try
            {
                return yyparse(tok, yyDebug);
            }
            catch (XPath2Exception)
            {
                throw;
            }
            catch (Exception)
            {
                throw new XPath2Exception("XPST0003", "{2} at line {1} pos {0}", tok.ColNo, tok.LineNo, errorText.ToString());
            }
        }

        public object yyparseDebug (Tokenizer tok)
        {
            return yyparseSafe(tok, new yydebug.yyDebugSimple());
        }
    
#line default
  /** error text **/
  public readonly TextWriter errorText = null;

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }

  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected)
  {
    if (errorText == null)
    {
      return;
    }
    
    if (expected?.Length > 0)
    {
      errorText.Write(message + ", expecting");
      for (int n = 0; n < expected.Length; ++n)
      {
        errorText.WriteLine(" " + expected[n]);
      }
    }
    else
    {
      errorText.WriteLine(message);
    }
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
//t  protected yydebug.yyDebug debug;

  protected static  int yyFinal = 7;
//t // Put this array into a separate class so it is only initialized if debugging is actually used
//t // Use MarshalByRefObject to disable inlining
//t class YYRules : MarshalByRefObject {
//t  public static  string [] yyRule = {
//t    "$accept : Expr",
//t    "Expr : ExprSingle",
//t    "Expr : Expr ',' ExprSingle",
//t    "ExprSingle : FORExpr",
//t    "ExprSingle : QuantifiedExpr",
//t    "ExprSingle : IfExpr",
//t    "ExprSingle : OrExpr",
//t    "FORExpr : SimpleForClause RETURN ExprSingle",
//t    "SimpleForClause : FOR ForClauseBody",
//t    "ForClauseBody : ForClauseOperator",
//t    "ForClauseBody : ForClauseBody ',' ForClauseOperator",
//t    "ForClauseOperator : '$' VarName IN ExprSingle",
//t    "QuantifiedExpr : SOME QuantifiedExprBody SATISFIES ExprSingle",
//t    "QuantifiedExpr : EVERY QuantifiedExprBody SATISFIES ExprSingle",
//t    "QuantifiedExprBody : QuantifiedExprOper",
//t    "QuantifiedExprBody : QuantifiedExprBody ',' QuantifiedExprOper",
//t    "QuantifiedExprOper : '$' VarName IN ExprSingle",
//t    "IfExpr : IF '(' Expr ')' THEN ExprSingle ELSE ExprSingle",
//t    "OrExpr : AndExpr",
//t    "OrExpr : OrExpr OR AndExpr",
//t    "AndExpr : ComparisonExpr",
//t    "AndExpr : AndExpr AND ComparisonExpr",
//t    "ComparisonExpr : RangeExpr",
//t    "ComparisonExpr : ValueComp",
//t    "ComparisonExpr : GeneralComp",
//t    "ComparisonExpr : NodeComp",
//t    "GeneralComp : RangeExpr '=' RangeExpr",
//t    "GeneralComp : RangeExpr '!' '=' RangeExpr",
//t    "GeneralComp : RangeExpr '<' RangeExpr",
//t    "GeneralComp : RangeExpr '<' '=' RangeExpr",
//t    "GeneralComp : RangeExpr '>' RangeExpr",
//t    "GeneralComp : RangeExpr '>' '=' RangeExpr",
//t    "ValueComp : RangeExpr EQ RangeExpr",
//t    "ValueComp : RangeExpr NE RangeExpr",
//t    "ValueComp : RangeExpr LT RangeExpr",
//t    "ValueComp : RangeExpr LE RangeExpr",
//t    "ValueComp : RangeExpr GT RangeExpr",
//t    "ValueComp : RangeExpr GE RangeExpr",
//t    "NodeComp : RangeExpr IS RangeExpr",
//t    "NodeComp : RangeExpr '<' '<' RangeExpr",
//t    "NodeComp : RangeExpr '>' '>' RangeExpr",
//t    "RangeExpr : AdditiveExpr",
//t    "RangeExpr : AdditiveExpr TO AdditiveExpr",
//t    "AdditiveExpr : MultiplicativeExpr",
//t    "AdditiveExpr : AdditiveExpr '+' MultiplicativeExpr",
//t    "AdditiveExpr : AdditiveExpr '-' MultiplicativeExpr",
//t    "MultiplicativeExpr : UnionExpr",
//t    "MultiplicativeExpr : MultiplicativeExpr ML UnionExpr",
//t    "MultiplicativeExpr : MultiplicativeExpr DIV UnionExpr",
//t    "MultiplicativeExpr : MultiplicativeExpr IDIV UnionExpr",
//t    "MultiplicativeExpr : MultiplicativeExpr MOD UnionExpr",
//t    "UnionExpr : IntersectExceptExpr",
//t    "UnionExpr : UnionExpr UNION IntersectExceptExpr",
//t    "UnionExpr : UnionExpr '|' IntersectExceptExpr",
//t    "IntersectExceptExpr : InstanceofExpr",
//t    "IntersectExceptExpr : IntersectExceptExpr INTERSECT InstanceofExpr",
//t    "IntersectExceptExpr : IntersectExceptExpr EXCEPT InstanceofExpr",
//t    "InstanceofExpr : TreatExpr",
//t    "InstanceofExpr : TreatExpr INSTANCE_OF SequenceType",
//t    "TreatExpr : CastableExpr",
//t    "TreatExpr : CastableExpr TREAT_AS SequenceType",
//t    "CastableExpr : CastExpr",
//t    "CastableExpr : CastExpr CASTABLE_AS SingleType",
//t    "CastExpr : UnaryExpr",
//t    "CastExpr : UnaryExpr CAST_AS SingleType",
//t    "UnaryExpr : UnaryOperator ValueExpr",
//t    "UnaryOperator :",
//t    "UnaryOperator : '+' UnaryOperator",
//t    "UnaryOperator : '-' UnaryOperator",
//t    "ValueExpr : PathExpr",
//t    "PathExpr : '/'",
//t    "PathExpr : '/' RelativePathExpr",
//t    "PathExpr : DOUBLE_SLASH RelativePathExpr",
//t    "PathExpr : RelativePathExpr",
//t    "RelativePathExpr : StepExpr",
//t    "RelativePathExpr : RelativePathExpr '/' StepExpr",
//t    "RelativePathExpr : RelativePathExpr DOUBLE_SLASH StepExpr",
//t    "StepExpr : AxisStep",
//t    "StepExpr : FilterExpr",
//t    "AxisStep : ForwardStep",
//t    "AxisStep : ForwardStep PredicateList",
//t    "AxisStep : ReverseStep",
//t    "AxisStep : ReverseStep PredicateList",
//t    "ForwardStep : AXIS_CHILD NodeTest",
//t    "ForwardStep : AXIS_DESCENDANT NodeTest",
//t    "ForwardStep : AXIS_ATTRIBUTE NodeTest",
//t    "ForwardStep : AXIS_SELF NodeTest",
//t    "ForwardStep : AXIS_DESCENDANT_OR_SELF NodeTest",
//t    "ForwardStep : AXIS_FOLLOWING_SIBLING NodeTest",
//t    "ForwardStep : AXIS_FOLLOWING NodeTest",
//t    "ForwardStep : AXIS_NAMESPACE NodeTest",
//t    "ForwardStep : AbbrevForwardStep",
//t    "AbbrevForwardStep : '@' NodeTest",
//t    "AbbrevForwardStep : NodeTest",
//t    "ReverseStep : AXIS_PARENT NodeTest",
//t    "ReverseStep : AXIS_ANCESTOR NodeTest",
//t    "ReverseStep : AXIS_PRECEDING_SIBLING NodeTest",
//t    "ReverseStep : AXIS_PRECEDING NodeTest",
//t    "ReverseStep : AXIS_ANCESTOR_OR_SELF NodeTest",
//t    "ReverseStep : AbbrevReverseStep",
//t    "AbbrevReverseStep : DOUBLE_PERIOD",
//t    "NodeTest : KindTest",
//t    "NodeTest : NameTest",
//t    "NameTest : QName",
//t    "NameTest : Wildcard",
//t    "Wildcard : '*'",
//t    "Wildcard : NCName ':' '*'",
//t    "Wildcard : '*' ':' NCName",
//t    "FilterExpr : PrimaryExpr",
//t    "FilterExpr : PrimaryExpr PredicateList",
//t    "PredicateList : Predicate",
//t    "PredicateList : PredicateList Predicate",
//t    "Predicate : '[' Expr ']'",
//t    "PrimaryExpr : Literal",
//t    "PrimaryExpr : VarRef",
//t    "PrimaryExpr : ParenthesizedExpr",
//t    "PrimaryExpr : ContextItemExpr",
//t    "PrimaryExpr : FunctionCall",
//t    "Literal : NumericLiteral",
//t    "Literal : StringLiteral",
//t    "NumericLiteral : IntegerLiteral",
//t    "NumericLiteral : DecimalLiteral",
//t    "NumericLiteral : DoubleLiteral",
//t    "VarRef : '$' VarName",
//t    "ParenthesizedExpr : '(' ')'",
//t    "ParenthesizedExpr : '(' Expr ')'",
//t    "ContextItemExpr : '.'",
//t    "FunctionCall : QName '(' ')'",
//t    "FunctionCall : QName '(' Args ')'",
//t    "Args : ExprSingle",
//t    "Args : Args ',' ExprSingle",
//t    "SingleType : AtomicType",
//t    "SingleType : AtomicType '?'",
//t    "SequenceType : ItemType",
//t    "SequenceType : ItemType Indicator1",
//t    "SequenceType : ItemType Indicator2",
//t    "SequenceType : ItemType Indicator3",
//t    "SequenceType : EMPTY_SEQUENCE",
//t    "ItemType : AtomicType",
//t    "ItemType : KindTest",
//t    "ItemType : ITEM",
//t    "AtomicType : QName",
//t    "KindTest : DocumentTest",
//t    "KindTest : ElementTest",
//t    "KindTest : AttributeTest",
//t    "KindTest : SchemaElementTest",
//t    "KindTest : SchemaAttributeTest",
//t    "KindTest : PITest",
//t    "KindTest : CommentTest",
//t    "KindTest : TextTest",
//t    "KindTest : AnyKindTest",
//t    "AnyKindTest : NODE '(' ')'",
//t    "DocumentTest : DOCUMENT_NODE '(' ')'",
//t    "DocumentTest : DOCUMENT_NODE '(' ElementTest ')'",
//t    "DocumentTest : DOCUMENT_NODE '(' SchemaElementTest ')'",
//t    "TextTest : TEXT '(' ')'",
//t    "CommentTest : COMMENT '(' ')'",
//t    "PITest : PROCESSING_INSTRUCTION '(' ')'",
//t    "PITest : PROCESSING_INSTRUCTION '(' NCName ')'",
//t    "PITest : PROCESSING_INSTRUCTION '(' StringLiteral ')'",
//t    "ElementTest : ELEMENT '(' ')'",
//t    "ElementTest : ELEMENT '(' ElementNameOrWildcard ')'",
//t    "ElementTest : ELEMENT '(' ElementNameOrWildcard ',' TypeName ')'",
//t    "ElementTest : ELEMENT '(' ElementNameOrWildcard ',' TypeName '?' ')'",
//t    "ElementNameOrWildcard : ElementName",
//t    "ElementNameOrWildcard : '*'",
//t    "AttributeTest : ATTRIBUTE '(' ')'",
//t    "AttributeTest : ATTRIBUTE '(' AttributeOrWildcard ')'",
//t    "AttributeTest : ATTRIBUTE '(' AttributeOrWildcard ',' TypeName ')'",
//t    "AttributeOrWildcard : AttributeName",
//t    "AttributeOrWildcard : '*'",
//t    "SchemaElementTest : SCHEMA_ELEMENT '(' ElementName ')'",
//t    "SchemaAttributeTest : SCHEMA_ATTRIBUTE '(' AttributeName ')'",
//t    "AttributeName : QName",
//t    "ElementName : QName",
//t    "TypeName : QName",
//t  };
//t public static string getRule (int index) {
//t    return yyRule [index];
//t }
//t}
  protected static  string [] yyNames = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"'!'",null,null,"'$'",null,null,
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,"':'",null,"'<'","'='","'>'","'?'",
    "'@'",null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    "'['",null,"']'",null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,"'|'",null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    "StringLiteral","IntegerLiteral","DecimalLiteral","DoubleLiteral",
    "NCName","QName","VarName","FOR","IN","IF","THEN","ELSE","SOME",
    "EVERY","SATISFIES","RETURN","AND","OR","TO","DOCUMENT","ELEMENT",
    "ATTRIBUTE","TEXT","COMMENT","PROCESSING_INSTRUCTION","ML","DIV",
    "IDIV","MOD","UNION","EXCEPT","INTERSECT","INSTANCE_OF","TREAT_AS",
    "CASTABLE_AS","CAST_AS","EQ","NE","LT","GT","GE","LE","IS","NODE",
    "DOUBLE_PERIOD","DOUBLE_SLASH","EMPTY_SEQUENCE","ITEM","AXIS_CHILD",
    "AXIS_DESCENDANT","AXIS_ATTRIBUTE","AXIS_SELF",
    "AXIS_DESCENDANT_OR_SELF","AXIS_FOLLOWING_SIBLING","AXIS_FOLLOWING",
    "AXIS_PARENT","AXIS_ANCESTOR","AXIS_PRECEDING_SIBLING",
    "AXIS_PRECEDING","AXIS_ANCESTOR_OR_SELF","AXIS_NAMESPACE",
    "Indicator1","Indicator2","Indicator3","DOCUMENT_NODE",
    "SCHEMA_ELEMENT","SCHEMA_ATTRIBUTE",
  };

  /** index-checked interface to yyNames[].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
  public static string yyname (int token) {
    if ((token < 0) || (token > yyNames.Length)) return "[illegal]";
    string name;
    if ((name = yyNames[token]) != null) return name;
    return "[unknown]";
  }

  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected string[] yyExpecting (int state) {
    int token, n, len = 0;
    bool[] ok = new bool[yyNames.Length];

    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }

    string [] result = new string[len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = yyNames[token];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex, Object yyd)
				 {
//t    this.debug = (yydebug.yyDebug)yyd;
    return yyparse(yyLex);
  }

  /** initial size and increment of the state/value stack [default 256].
      This is not final so that it can be overwritten outside of invocations
      of yyparse().
    */
  protected int yyMax;

  /** executed at the beginning of a reduce action.
      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
      Can be overwritten to provide deep copy, etc.
      @param first value for $1, or null.
      @return first.
    */
  protected Object yyDefault (Object first) {
    return first;
  }

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex)
				{
    if (yyMax <= 0) yyMax = 256;			// initial size
    int yyState = 0;                                   // state stack ptr
    int [] yyStates = new int[yyMax];	                // state stack 
    Object yyVal = null;                               // value stack ptr
    Object [] yyVals = new Object[yyMax];	        // value stack
    int yyToken = -1;					// current input
    int yyErrorFlag = 0;				// #tks to shift

    int yyTop = 0;
    goto skip;
    yyLoop:
    yyTop++;
    skip:
    for (;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        int[] i = new int[yyStates.Length+yyMax];
        yyStates.CopyTo (i, 0);
        yyStates = i;
        Object[] o = new Object[yyVals.Length+yyMax];
        yyVals.CopyTo (o, 0);
        yyVals = o;
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
//t      if (debug != null) debug.push(yyState, yyVal);

      yyDiscarded: for (;;) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
//t            if (debug != null)
//t              debug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
//t            if (debug != null)
//t              debug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyerror("syntax error", yyExpecting(yyState));
//t              if (debug != null) debug.error("syntax error");
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
//t                  if (debug != null)
//t                    debug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto yyLoop;
                }
//t                if (debug != null) debug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
//t              if (debug != null) debug.reject();
              throw new yyParser.yyException("irrecoverable syntax error");
  
            case 3:
              if (yyToken == 0) {
//t                if (debug != null) debug.reject();
                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
              }
//t              if (debug != null)
//t                debug.discard(yyState, yyToken, yyname(yyToken),
//t  							yyLex.value());
              yyToken = -1;
              goto yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
//t        if (debug != null)
//t          debug.reduce(yyState, yyStates[yyV-1], yyN, YYRules.yyRule[yyN], yyLen[yyN]);
        yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 1:
#line 136 "Xpath.y"
  {
     yyVal = AbstractNode.Create(context, yyVals[0+yyTop]);
  }
  break;
case 2:
#line 140 "Xpath.y"
  {
     ExprNode expr = yyVals[-2+yyTop] as ExprNode;
     if (expr == null)
         expr = new ExprNode(context, yyVals[-2+yyTop]);
     expr.Add(yyVals[0+yyTop]);
     yyVal = expr;
  }
  break;
case 7:
#line 159 "Xpath.y"
  {
     ForNode node = (ForNode)yyVals[-2+yyTop];
     node.AddTail(yyVals[0+yyTop]);
     yyVal = node;
  }
  break;
case 8:
#line 168 "Xpath.y"
  {
     yyVal = yyVals[0+yyTop];
  }
  break;
case 10:
#line 176 "Xpath.y"
  {
     ((ForNode)yyVals[-2+yyTop]).Add(yyVals[0+yyTop]);
     yyVal = yyVals[-2+yyTop];
  }
  break;
case 11:
#line 184 "Xpath.y"
  {
      yyVal = new ForNode(context, (Tokenizer.VarName)yyVals[-2+yyTop], yyVals[0+yyTop]);
  }
  break;
case 12:
#line 191 "Xpath.y"
  {
     ForNode node = (ForNode)yyVals[-2+yyTop];
     node.AddTail(yyVals[0+yyTop]);
     yyVal = new UnaryOperatorNode(context, (provider, arg) => CoreFuncs.Some(arg), node, XPath2ResultType.Boolean);
  }
  break;
case 13:
#line 197 "Xpath.y"
  {
     ForNode node = (ForNode)yyVals[-2+yyTop];
     node.AddTail(yyVals[0+yyTop]);
     yyVal = new UnaryOperatorNode(context, (provider, arg) => CoreFuncs.Every(arg), node, XPath2ResultType.Boolean);
  }
  break;
case 15:
#line 207 "Xpath.y"
  {
     ((ForNode)yyVals[-2+yyTop]).Add(yyVals[0+yyTop]);
     yyVal = yyVals[-2+yyTop];
  }
  break;
case 16:
#line 215 "Xpath.y"
  {
     yyVal = new ForNode(context, (Tokenizer.VarName)yyVals[-2+yyTop], yyVals[0+yyTop]);
  }
  break;
case 17:
#line 222 "Xpath.y"
  {
     yyVal = new IfNode(context, yyVals[-5+yyTop], yyVals[-2+yyTop], yyVals[0+yyTop]);
  }
  break;
case 19:
#line 230 "Xpath.y"
  {
     yyVal = new OrExprNode(context, yyVals[-2+yyTop], yyVals[0+yyTop]);
  }
  break;
case 21:
#line 238 "Xpath.y"
  {
     yyVal = new AndExprNode(context, yyVals[-2+yyTop], yyVals[0+yyTop]);
  }
  break;
case 26:
#line 252 "Xpath.y"
  {
     yyVal = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralEQ(context, arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 27:
#line 257 "Xpath.y"
  {
     yyVal = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralNE(context, arg1, arg2), yyVals[-3+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 28:
#line 262 "Xpath.y"
  {
     yyVal = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralLT(context, arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 29:
#line 267 "Xpath.y"
  {
     yyVal = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralLE(context, arg1, arg2), yyVals[-3+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 30:
#line 272 "Xpath.y"
  {
     yyVal = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralGT(context, arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 31:
#line 277 "Xpath.y"
  {
     yyVal = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralGE(context, arg1, arg2), yyVals[-3+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 32:
#line 285 "Xpath.y"
  {
     yyVal = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorEq(arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 33:
#line 290 "Xpath.y"
  {
     yyVal = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.Not(CoreFuncs.OperatorEq(arg1, arg2)), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 34:
#line 295 "Xpath.y"
  {
     yyVal = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg2, arg1), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 35:
#line 300 "Xpath.y"
  {
     yyVal = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg2, arg1) == CoreFuncs.True ||
          CoreFuncs.OperatorEq(arg1, arg2) == CoreFuncs.True ? CoreFuncs.True : CoreFuncs.False, yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 36:
#line 306 "Xpath.y"
  {
     yyVal = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 37:
#line 311 "Xpath.y"
  {
     yyVal = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg1, arg2) == CoreFuncs.True ||
          CoreFuncs.OperatorEq(arg1, arg2) == CoreFuncs.True ? CoreFuncs.True : CoreFuncs.False, yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 38:
#line 320 "Xpath.y"
  {
     yyVal = new SingletonBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.SameNode(arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 39:
#line 325 "Xpath.y"
  {
     yyVal = new SingletonBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.PrecedingNode(arg1, arg2), yyVals[-3+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 40:
#line 330 "Xpath.y"
  {
     yyVal = new SingletonBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.FollowingNode(arg1, arg2), yyVals[-3+yyTop], yyVals[0+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 42:
#line 339 "Xpath.y"
  {
      yyVal = new RangeNode(context, yyVals[-2+yyTop], yyVals[0+yyTop]);
  }
  break;
case 44:
#line 347 "Xpath.y"
  {
     yyVal = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) + ValueProxy.New(arg2), yyVals[-2+yyTop], yyVals[0+yyTop], 
            ArithmeticBinaryOperatorNode.AdditionResult);
  }
  break;
case 45:
#line 353 "Xpath.y"
  {
     yyVal = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) - ValueProxy.New(arg2), yyVals[-2+yyTop], yyVals[0+yyTop], 
            ArithmeticBinaryOperatorNode.SubstractionResult);
  }
  break;
case 47:
#line 363 "Xpath.y"
  {
     yyVal = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) * ValueProxy.New(arg2), yyVals[-2+yyTop], yyVals[0+yyTop], 
            ArithmeticBinaryOperatorNode.MultiplyResult);
  }
  break;
case 48:
#line 369 "Xpath.y"
  {
     yyVal = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) / ValueProxy.New(arg2), yyVals[-2+yyTop], yyVals[0+yyTop], 
            ArithmeticBinaryOperatorNode.DivisionResult);
  }
  break;
case 49:
#line 375 "Xpath.y"
  {
     yyVal = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.op_IntegerDivide(ValueProxy.New(arg1), ValueProxy.New(arg2)), yyVals[-2+yyTop], yyVals[0+yyTop], null);
  }
  break;
case 50:
#line 380 "Xpath.y"
  {
     yyVal = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) % ValueProxy.New(arg2), yyVals[-2+yyTop], yyVals[0+yyTop], null);
  }
  break;
case 52:
#line 389 "Xpath.y"
  {
     yyVal = new OrderedBinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Union(context, arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.NodeSet);
  }
  break;
case 53:
#line 394 "Xpath.y"
  {
     yyVal = new OrderedBinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Union(context, arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.NodeSet);
  }
  break;
case 55:
#line 403 "Xpath.y"
  {
     yyVal = new OrderedBinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Intersect(context, arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.NodeSet);
  }
  break;
case 56:
#line 408 "Xpath.y"
  {
     yyVal = new BinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Except(context, arg1, arg2), yyVals[-2+yyTop], yyVals[0+yyTop], XPath2ResultType.NodeSet);
  }
  break;
case 58:
#line 417 "Xpath.y"
  {
     SequenceType destType = (SequenceType)yyVals[0+yyTop];
     yyVal = new UnaryOperatorNode(context, 
        (provider, arg) => CoreFuncs.InstanceOf(context, arg, destType), yyVals[-2+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 60:
#line 427 "Xpath.y"
  {
     SequenceType destType = (SequenceType)yyVals[0+yyTop];
     yyVal = new UnaryOperatorNode(context, 
        (provider, arg) => CoreFuncs.TreatAs(context, arg, destType), yyVals[-2+yyTop], CoreFuncs.GetXPath2ResultType(destType));
  }
  break;
case 62:
#line 437 "Xpath.y"
  {     
     SequenceType destType = (SequenceType)yyVals[0+yyTop];
     ValueNode value = yyVals[-2+yyTop] as ValueNode;
     bool isString = yyVals[-2+yyTop] is String || (value != null && value.Content is String);
     if (destType == null)
         throw new XPath2Exception("XPST0051",Properties.Resources.XPST0051, "xs:untyped");
     if (destType.SchemaType == SequenceType.XmlSchema.AnyType)
         throw new XPath2Exception("XPST0051",Properties.Resources.XPST0051, "xs:anyType");
     if (destType.SchemaType == SequenceType.XmlSchema.AnySimpleType)
         throw new XPath2Exception("XPST0051",Properties.Resources.XPST0051, "xs:anySimpleType");
     if (destType.TypeCode == XmlTypeCode.AnyAtomicType)
         throw new XPath2Exception("XPST0051", Properties.Resources.XPST0051, "xs:anyAtomicType");
     if (destType.TypeCode == XmlTypeCode.Notation)
         throw new XPath2Exception("XPST0080", Properties.Resources.XPST0080, destType);
     if (destType.Cardinality == XmlTypeCardinality.ZeroOrMore || destType.Cardinality == XmlTypeCardinality.OneOrMore)
         throw new XPath2Exception("XPST0080",Properties.Resources.XPST0080, destType);
     yyVal = new UnaryOperatorNode(context, (provider, arg) => CoreFuncs.Castable(context, arg, destType, isString), yyVals[-2+yyTop], XPath2ResultType.Boolean);
  }
  break;
case 64:
#line 460 "Xpath.y"
  {
     SequenceType destType = (SequenceType)yyVals[0+yyTop];
     ValueNode value = yyVals[-2+yyTop] as ValueNode;
     bool isString = yyVals[-2+yyTop] is String || (value != null && value.Content is String);
     if (destType == null)
         throw new XPath2Exception("XPST0051", Properties.Resources.XPST0051, "xs:untyped");
     if (destType.SchemaType == SequenceType.XmlSchema.AnyType)
         throw new XPath2Exception("XPST0051", Properties.Resources.XPST0051, "xs:anyType");
     if (destType.SchemaType == SequenceType.XmlSchema.AnySimpleType)
         throw new XPath2Exception("XPST0051", Properties.Resources.XPST0051, "xs:anySimpleType");
     if (destType.TypeCode == XmlTypeCode.AnyAtomicType)
         throw new XPath2Exception("XPST0051", Properties.Resources.XPST0051, "xs:anyAtomicType");
     if (destType.TypeCode == XmlTypeCode.Notation)
         throw new XPath2Exception("XPST0080", Properties.Resources.XPST0080, destType);
     if (destType.Cardinality == XmlTypeCardinality.ZeroOrMore || destType.Cardinality == XmlTypeCardinality.OneOrMore)
         throw new XPath2Exception("XPST0080", Properties.Resources.XPST0080, destType);
     yyVal = new UnaryOperatorNode(context, (provider, arg) => 
        CoreFuncs.CastTo(context, arg, destType, isString), yyVals[-2+yyTop], CoreFuncs.GetXPath2ResultType(destType));
  }
  break;
case 65:
#line 483 "Xpath.y"
  {
     if (yyVals[-1+yyTop] != null)
     {
       if (yyVals[-1+yyTop] == CoreFuncs.True)
          yyVal = new AtomizedUnaryOperatorNode(context, (provider, arg) => -ValueProxy.New(arg), yyVals[0+yyTop], XPath2ResultType.Number);
        else
          yyVal = new AtomizedUnaryOperatorNode(context, (provider, arg) => 0 + ValueProxy.New(arg), yyVals[0+yyTop], XPath2ResultType.Number);
     }
     else
        yyVal = yyVals[0+yyTop];
  }
  break;
case 66:
#line 498 "Xpath.y"
  {
     yyVal = null;
  }
  break;
case 67:
#line 502 "Xpath.y"
  {
     if (yyVals[0+yyTop] == null)
       yyVal = CoreFuncs.False;
     else
        yyVal = yyVals[0+yyTop];
  }
  break;
case 68:
#line 509 "Xpath.y"
  {
     if (yyVals[0+yyTop] == null || yyVals[0+yyTop] == CoreFuncs.False)
         yyVal = CoreFuncs.True;
     else
         yyVal = CoreFuncs.False;
  }
  break;
case 70:
#line 523 "Xpath.y"
  {
     yyVal = new UnaryOperatorNode(context, (provider, arg) => 
        XPath2NodeIterator.Create(CoreFuncs.GetRoot(arg)), new ContextItemNode(context), XPath2ResultType.NodeSet);
  }
  break;
case 71:
#line 528 "Xpath.y"
  { 
     yyVal = yyVals[0+yyTop] is PathStep 
       ? new PathExprNode(context, (PathStep)yyVals[0+yyTop]) : yyVals[0+yyTop];
  }
  break;
case 72:
#line 533 "Xpath.y"
  {
     PathStep descendantOrSelf = new PathStep(SequenceType.Node, 
        XPath2ExprType.DescendantOrSelf);
     descendantOrSelf.AddLast(PathStep.Create(context, yyVals[0+yyTop]));
     yyVal = new PathExprNode(context, descendantOrSelf);
  }
  break;
case 73:
#line 540 "Xpath.y"
  {
     yyVal = yyVals[0+yyTop] is PathStep 
       ? new PathExprNode(context, (PathStep)yyVals[0+yyTop]) : yyVals[0+yyTop];
  }
  break;
case 75:
#line 549 "Xpath.y"
  {
     PathStep relativePathExpr = PathStep.Create(context, yyVals[-2+yyTop]);
     relativePathExpr.AddLast(PathStep.Create(context, yyVals[0+yyTop]));
     yyVal = relativePathExpr;
  }
  break;
case 76:
#line 555 "Xpath.y"
  {
     PathStep relativePathExpr = PathStep.Create(context, yyVals[-2+yyTop]);
     PathStep descendantOrSelf = new PathStep(SequenceType.Node, 
        XPath2ExprType.DescendantOrSelf);
     relativePathExpr.AddLast(descendantOrSelf);
     relativePathExpr.AddLast(PathStep.Create(context, yyVals[0+yyTop]));
     yyVal = relativePathExpr;
  }
  break;
case 80:
#line 573 "Xpath.y"
  {
     yyVal = PathStep.CreateFilter(context, yyVals[-1+yyTop], (List<Object>)yyVals[0+yyTop]);
  }
  break;
case 82:
#line 578 "Xpath.y"
  {
     yyVal = PathStep.CreateFilter(context, yyVals[-1+yyTop], (List<Object>)yyVals[0+yyTop]);
  }
  break;
case 83:
#line 585 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Child);
   }
  break;
case 84:
#line 589 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Descendant);
   }
  break;
case 85:
#line 593 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Attribute);
   }
  break;
case 86:
#line 597 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Self);
   }
  break;
case 87:
#line 601 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.DescendantOrSelf);
   }
  break;
case 88:
#line 605 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.FollowingSibling);
   }
  break;
case 89:
#line 609 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Following);
   }
  break;
case 90:
#line 613 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Namespace);
   }
  break;
case 92:
#line 621 "Xpath.y"
  {
       yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Attribute);
   }
  break;
case 93:
#line 625 "Xpath.y"
  {
       yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Child);
   }
  break;
case 94:
#line 632 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Parent);
   }
  break;
case 95:
#line 636 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Ancestor);
   }
  break;
case 96:
#line 640 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.PrecedingSibling);
   }
  break;
case 97:
#line 644 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.Preceding);
   }
  break;
case 98:
#line 648 "Xpath.y"
  {
      yyVal = new PathStep(yyVals[0+yyTop], XPath2ExprType.AncestorOrSelf);
   }
  break;
case 100:
#line 656 "Xpath.y"
  {
      yyVal = new PathStep(XPath2ExprType.Parent);
   }
  break;
case 103:
#line 668 "Xpath.y"
  {
      XmlQualifiedName qualifiedName = QNameParser.Parse((String)yyVals[0+yyTop], 
        context.NamespaceManager, context.NamespaceManager.DefaultNamespace, context.NameTable);
      yyVal = XmlQualifiedNameTest.New(qualifiedName.Name, qualifiedName.Namespace);
   }
  break;
case 105:
#line 678 "Xpath.y"
  {
      yyVal = XmlQualifiedNameTest.New(null, null);
   }
  break;
case 106:
#line 682 "Xpath.y"
  {
      string ncname = (String)yyVals[-2+yyTop];
      string ns = context.NamespaceManager.LookupNamespace(ncname);
      if (ns == null)
        throw new XPath2Exception("XPST0081", Properties.Resources.XPST0081, ncname);
      yyVal = XmlQualifiedNameTest.New(null, ns);
   }
  break;
case 107:
#line 690 "Xpath.y"
  {
      yyVal = XmlQualifiedNameTest.New(context.NameTable.Add((String)yyVals[0+yyTop]), null);
   }
  break;
case 109:
#line 698 "Xpath.y"
  {
      yyVal = new FilterExprNode(context, yyVals[-1+yyTop], (List<Object>)yyVals[0+yyTop]);
   }
  break;
case 110:
#line 705 "Xpath.y"
  {
      List<Object> nodes = new List<Object>();
      nodes.Add(yyVals[0+yyTop]);
      yyVal = nodes;
   }
  break;
case 111:
#line 711 "Xpath.y"
  {
      List<Object> nodes = (List<Object>)yyVals[-1+yyTop];
      nodes.Add(yyVals[0+yyTop]);
      yyVal = nodes;
   }
  break;
case 112:
#line 720 "Xpath.y"
  {
      yyVal = yyVals[-1+yyTop];
   }
  break;
case 114:
#line 728 "Xpath.y"
  {
      yyVal = new VarRefNode(context, (Tokenizer.VarName)yyVals[0+yyTop]);
   }
  break;
case 116:
#line 733 "Xpath.y"
  {
      yyVal = new ContextItemNode(context);
   }
  break;
case 123:
#line 752 "Xpath.y"
  {
      yyVal = yyVals[0+yyTop];
   }
  break;
case 124:
#line 760 "Xpath.y"
  {
      yyVal = new ValueNode(context, Undefined.Value);
   }
  break;
case 125:
#line 764 "Xpath.y"
  {
      yyVal = yyVals[-1+yyTop];
   }
  break;
case 127:
#line 775 "Xpath.y"
  {
      XmlQualifiedName identity = QNameParser.ParseFunction((string)yyVals[-2+yyTop], context.NamespaceManager, 
         context.DefaultFunctionNamespace, context.NameTable);
      string ns = identity.Namespace;
      if (identity.Namespace == String.Empty)            
          ns = XmlReservedNs.NsXQueryFunc;
      yyVal = new FuncNode(context, identity.Name, ns);
   }
  break;
case 128:
#line 784 "Xpath.y"
  {
      XmlQualifiedName identity = QNameParser.ParseFunction((string)yyVals[-3+yyTop], context.NamespaceManager, 
         context.DefaultFunctionNamespace, context.NameTable);
      string ns = identity.Namespace;
      if (identity.Namespace == String.Empty)            
          ns = XmlReservedNs.NsXQueryFunc;
      List<Object> args = (List<Object>)yyVals[-1+yyTop];
      XmlSchemaObject schemaType;
      if (args.Count == 1 && CoreFuncs.TryProcessTypeName(context, 
           new XmlQualifiedName(identity.Name, ns), false, out schemaType))
         {
            SequenceType seqtype =
               new SequenceType((XmlSchemaSimpleType)schemaType, XmlTypeCardinality.One, null);
            if (seqtype == null)
               throw new XPath2Exception("XPST0051", Properties.Resources.XPST0051, "untyped");
            if (seqtype.TypeCode == XmlTypeCode.Notation)
               throw new XPath2Exception("XPST0051", Properties.Resources.XPST0051, "NOTATION");
            yyVal = new UnaryOperatorNode(context, (provider, arg) => 
               CoreFuncs.CastToItem(context, arg, seqtype), args[0], CoreFuncs.GetXPath2ResultType(seqtype)); 
          }
      else
         yyVal = new FuncNode(context, identity.Name, ns, (List<Object>)yyVals[-1+yyTop]);
   }
  break;
case 129:
#line 811 "Xpath.y"
  {
      List<Object> list = new List<Object>();
      list.Add(yyVals[0+yyTop]);
      yyVal = list;
   }
  break;
case 130:
#line 817 "Xpath.y"
  {
      List<Object> list = (List<Object>)yyVals[-2+yyTop];
      list.Add(yyVals[0+yyTop]);
      yyVal = list;
   }
  break;
case 132:
#line 827 "Xpath.y"
  {
      SequenceType type = (SequenceType)yyVals[-1+yyTop];
      type.Cardinality = XmlTypeCardinality.ZeroOrOne;
      yyVal = type;
   }
  break;
case 134:
#line 837 "Xpath.y"
  {
      SequenceType type = (SequenceType)yyVals[-1+yyTop];
      type.Cardinality = XmlTypeCardinality.ZeroOrMore; 
      yyVal = type;
   }
  break;
case 135:
#line 843 "Xpath.y"
  {
      SequenceType type = (SequenceType)yyVals[-1+yyTop];
      type.Cardinality = XmlTypeCardinality.OneOrMore;
      yyVal = type;
   }
  break;
case 136:
#line 849 "Xpath.y"
  {
      SequenceType type = (SequenceType)yyVals[-1+yyTop];
      type.Cardinality = XmlTypeCardinality.ZeroOrOne;
      yyVal = type;
   }
  break;
case 137:
#line 855 "Xpath.y"
  {
      yyVal = SequenceType.Void;
   }
  break;
case 140:
#line 864 "Xpath.y"
  {
      yyVal = new SequenceType(XmlTypeCode.Item);
   }
  break;
case 141:
#line 871 "Xpath.y"
  {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)yyVals[0+yyTop], true, out xmlType);
      yyVal = new SequenceType((XmlSchemaType)xmlType, XmlTypeCardinality.One, null);
   }
  break;
case 151:
#line 892 "Xpath.y"
  {
      yyVal = SequenceType.Node;
   }
  break;
case 152:
#line 899 "Xpath.y"
  {
      yyVal = SequenceType.Document;
   }
  break;
case 153:
#line 903 "Xpath.y"
  {
      SequenceType type = (SequenceType)yyVals[-1+yyTop];
      type.TypeCode = XmlTypeCode.Document;
      yyVal = type;
   }
  break;
case 154:
#line 909 "Xpath.y"
  {
      SequenceType type = (SequenceType)yyVals[-1+yyTop];
      type.TypeCode = XmlTypeCode.Document;
      yyVal = type;
   }
  break;
case 155:
#line 918 "Xpath.y"
  {
      yyVal = SequenceType.Text;
   }
  break;
case 156:
#line 925 "Xpath.y"
  {
      yyVal = SequenceType.Comment;
   }
  break;
case 157:
#line 932 "Xpath.y"
  {
      yyVal = SequenceType.ProcessingInstruction;
   }
  break;
case 158:
#line 936 "Xpath.y"
  {
      XmlQualifiedNameTest nameTest = XmlQualifiedNameTest.New((String)yyVals[-1+yyTop], null);
      yyVal = new SequenceType(XmlTypeCode.ProcessingInstruction, nameTest);
   }
  break;
case 159:
#line 941 "Xpath.y"
  {
      XmlQualifiedNameTest nameTest = XmlQualifiedNameTest.New((String)yyVals[-1+yyTop], null);
      yyVal = new SequenceType(XmlTypeCode.ProcessingInstruction, nameTest);
   }
  break;
case 160:
#line 949 "Xpath.y"
  {
      yyVal = SequenceType.Element;
   }
  break;
case 161:
#line 953 "Xpath.y"
  {
      yyVal = new SequenceType(XmlTypeCode.Element, (XmlQualifiedNameTest)yyVals[-1+yyTop]);
   }
  break;
case 162:
#line 957 "Xpath.y"
  {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)yyVals[-1+yyTop], true, out xmlType);
      yyVal = new SequenceType(XmlTypeCode.Element, (XmlQualifiedNameTest)yyVals[-3+yyTop], (XmlSchemaType)xmlType, false);      
   }
  break;
case 163:
#line 963 "Xpath.y"
  {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)yyVals[-2+yyTop], true, out xmlType);
      yyVal = new SequenceType(XmlTypeCode.Element, (XmlQualifiedNameTest)yyVals[-4+yyTop], (XmlSchemaType)xmlType, true);      
   }
  break;
case 164:
#line 972 "Xpath.y"
  {
      yyVal = XmlQualifiedNameTest.New((XmlQualifiedName)QNameParser.Parse((string)yyVals[0+yyTop], 
         context.NamespaceManager, context.NamespaceManager.DefaultNamespace, context.NameTable));
   }
  break;
case 165:
#line 977 "Xpath.y"
  {
      yyVal = XmlQualifiedNameTest.New(null, null);
   }
  break;
case 166:
#line 984 "Xpath.y"
  {
      yyVal = SequenceType.Attribute;
   }
  break;
case 167:
#line 988 "Xpath.y"
  {
      yyVal = new SequenceType(XmlTypeCode.Attribute, (XmlQualifiedNameTest)yyVals[-1+yyTop]);
   }
  break;
case 168:
#line 992 "Xpath.y"
  {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)yyVals[-1+yyTop], true, out xmlType);
      yyVal = new SequenceType(XmlTypeCode.Attribute, (XmlQualifiedNameTest)yyVals[-3+yyTop], (XmlSchemaType)xmlType);      
   }
  break;
case 169:
#line 1001 "Xpath.y"
  {
      yyVal = XmlQualifiedNameTest.New((XmlQualifiedName)QNameParser.Parse((string)yyVals[0+yyTop], 
         context.NamespaceManager, context.NamespaceManager.DefaultNamespace, context.NameTable));
   }
  break;
case 170:
#line 1006 "Xpath.y"
  {
      yyVal = XmlQualifiedNameTest.New(null, null);
   }
  break;
case 171:
#line 1013 "Xpath.y"
  {
      XmlQualifiedName qname = QNameParser.Parse((string)yyVals[-1+yyTop], context.NamespaceManager, 
         context.NamespaceManager.DefaultNamespace, context.NameTable);
      XmlSchemaElement schemaElement = (XmlSchemaElement)context.SchemaSet.GlobalElements[qname];
      if (schemaElement == null)
          throw new XPath2Exception("XPST0008", Properties.Resources.XPST0008, qname);
      yyVal = new SequenceType(schemaElement);
   }
  break;
case 172:
#line 1025 "Xpath.y"
  {
      XmlQualifiedName qname = QNameParser.Parse((string)yyVals[-1+yyTop], context.NamespaceManager, 
         context.NamespaceManager.DefaultNamespace, context.NameTable);
      XmlSchemaAttribute schemaAttribute = (XmlSchemaAttribute)context.SchemaSet.GlobalAttributes[qname];
      if (schemaAttribute == null)
          throw new XPath2Exception("XPST0008", Properties.Resources.XPST0008, qname);
      yyVal = new SequenceType(schemaAttribute);
   }
  break;
#line default
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
//t          if (debug != null) debug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
//t            if (debug != null)
//t               debug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
          }
          if (yyToken == 0) {
//t            if (debug != null) debug.accept(yyVal);
            return yyVal;
          }
          goto yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
//t        if (debug != null) debug.shift(yyStates[yyTop], yyState);
	 goto yyLoop;
      }
    }
  }

   static  short [] yyLhs  = {              -1,
    0,    0,    1,    1,    1,    1,    2,    6,    7,    7,
    8,    3,    3,    9,    9,   10,    4,    5,    5,   11,
   11,   12,   12,   12,   12,   15,   15,   15,   15,   15,
   15,   14,   14,   14,   14,   14,   14,   16,   16,   16,
   13,   13,   17,   17,   17,   18,   18,   18,   18,   18,
   19,   19,   19,   20,   20,   20,   21,   21,   22,   22,
   24,   24,   25,   25,   27,   28,   28,   28,   29,   30,
   30,   30,   30,   31,   31,   31,   32,   32,   33,   33,
   33,   33,   35,   35,   35,   35,   35,   35,   35,   35,
   35,   39,   39,   37,   37,   37,   37,   37,   37,   40,
   38,   38,   42,   42,   43,   43,   43,   34,   34,   36,
   36,   45,   44,   44,   44,   44,   44,   46,   46,   51,
   51,   51,   47,   48,   48,   49,   50,   50,   52,   52,
   26,   26,   23,   23,   23,   23,   23,   54,   54,   54,
   53,   41,   41,   41,   41,   41,   41,   41,   41,   41,
   63,   55,   55,   55,   62,   61,   60,   60,   60,   56,
   56,   56,   56,   64,   64,   57,   57,   57,   67,   67,
   58,   59,   68,   66,   65,
  };
   static  short [] yyLen = {           2,
    1,    3,    1,    1,    1,    1,    3,    2,    1,    3,
    4,    4,    4,    1,    3,    4,    8,    1,    3,    1,
    3,    1,    1,    1,    1,    3,    4,    3,    4,    3,
    4,    3,    3,    3,    3,    3,    3,    3,    4,    4,
    1,    3,    1,    3,    3,    1,    3,    3,    3,    3,
    1,    3,    3,    1,    3,    3,    1,    3,    1,    3,
    1,    3,    1,    3,    2,    0,    2,    2,    1,    1,
    2,    2,    1,    1,    3,    3,    1,    1,    1,    2,
    1,    2,    2,    2,    2,    2,    2,    2,    2,    2,
    1,    2,    1,    2,    2,    2,    2,    2,    1,    1,
    1,    1,    1,    1,    1,    3,    3,    1,    2,    1,
    2,    3,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    2,    2,    3,    1,    3,    4,    1,    3,
    1,    2,    1,    2,    2,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    3,    3,    4,    4,    3,    3,    3,    4,    4,    3,
    4,    6,    7,    1,    1,    3,    4,    6,    1,    1,
    4,    4,    1,    1,    1,
  };
   static  short [] yyDefRed = {            0,
    0,    0,    0,    0,    0,    0,    0,    1,    3,    4,
    5,    0,    0,    0,   20,    0,   23,   24,   25,    0,
    0,    0,    0,   54,    0,    0,    0,    0,    0,    0,
    0,    9,    0,    0,    0,   14,    0,   67,   68,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  119,
  120,  121,  122,    0,    0,    0,    0,    0,    0,    0,
    0,  100,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  126,   65,   69,    0,   74,   77,
   78,    0,    0,   93,   91,   99,  101,  102,  104,    0,
  113,  114,  115,  116,  117,  118,  142,  143,  144,  145,
  146,  147,  148,  149,  150,    0,    0,    0,    0,    0,
    0,    0,    2,    0,    7,   21,   32,   33,   34,   36,
   37,   35,   38,   26,    0,    0,    0,   28,    0,    0,
   30,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   56,   55,  141,  137,  140,   58,  139,  138,    0,   60,
   62,    0,   64,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  103,   83,   84,   85,   86,   87,   88,   89,
   94,   95,   96,   97,   98,   90,    0,    0,    0,  123,
  124,    0,    0,   92,    0,    0,    0,    0,    0,  110,
    0,    0,    0,   10,    0,    0,   12,   15,   13,   27,
   29,   39,   31,   40,  134,  135,  136,  132,  106,  127,
  129,    0,  174,  160,  165,    0,  164,  173,  166,  170,
    0,  169,  155,  156,    0,    0,  157,  151,  152,    0,
    0,    0,    0,  125,  107,   76,   75,    0,  111,   11,
    0,   16,    0,  128,    0,  161,    0,  167,  159,  158,
  153,  154,  171,  172,  112,    0,  130,  175,    0,    0,
    0,  162,    0,  168,   17,  163,
  };
  protected static  short [] yyDgoto  = {             7,
    8,    9,   10,   11,   12,   13,   31,   32,   35,   36,
   14,   15,   16,   17,   18,   19,   20,   21,   22,   23,
   24,   25,  176,   26,   27,  181,   28,   29,  106,  107,
  108,  109,  110,  111,  112,  219,  113,  114,  115,  116,
  117,  118,  119,  120,  220,  121,  122,  123,  124,  125,
  126,  242,  178,  179,  127,  128,  129,  130,  131,  132,
  133,  134,  135,  246,  289,  247,  251,  252,
  };
  protected static  short [] yySindex = {          -16,
   11,   14,   24,   24,   31,   31,   34,    0,    0,    0,
    0, -190, -184, -172,    0, 1280,    0,    0,    0,  -40,
 -232, -115, -165,    0, -182, -185, -174, -168, 1934, -137,
   84,    0,  -16, -133,  -20,    0,  -19,    0,    0,  -16,
   31,  -16,   31,   31,   31,   31,   31,   31,   31,   31,
   31,   77,  -28,  -31,   31,   31,   31,   31,   31,   31,
   31,   31,   31,   31,   31, -213, -213, -122, -122,    0,
    0,    0,    0,   90,  102,  112,  119,  123,  125,  129,
  130,    0, 2112,  222,  222,  222,  222,  222,  222,  222,
  222,  222,  222,  222,  222,  222,  132,  133,  135,  -87,
  -37, 2112,  222,  120,    0,    0,    0,  -45,    0,    0,
    0,   88,   88,    0,    0,    0,    0,    0,    0,   88,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  -85,   11,  -18,  -84,  -16,
   24,  -16,    0, -172,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   31,   31,   31,    0,   31,   31,
    0,   50, -232, -232, -115, -115, -115, -115, -165, -165,
    0,    0,    0,    0,    0,    0,    0,    0, -199,    0,
    0,  124,    0,  147,  -23,   -4,   -2,  142,  149,    2,
  150,  -45,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  -41,  -69,  -66,    0,
    0,   41,  -45,    0,  -63, 2112, 2112,  -16,   88,    0,
   88,   88,  -16,    0,  -61,  -16,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   45,    0,    0,    0,   58,    0,    0,    0,    0,
   59,    0,    0,    0,  159,  163,    0,    0,    0,  164,
  166,  167,  168,    0,    0,    0,    0,  -34,    0,    0,
  -16,    0,  -16,    0,  -51,    0,  -51,    0,    0,    0,
    0,    0,    0,    0,    0,  -56,    0,    0,  -22,  172,
  -16,    0,  173,    0,    0,    0,
  };
  protected static  short [] yyRindex = {         2009,
    0,    0,    0,    0, 2009, 2009,    0,    0,    0,    0,
    0,  453,    0, 1098,    0,  667,    0,    0,    0, 1873,
 1792, 1432, 1330,    0, 1063,  989,  955,  887,    0,    0,
  -57,    0, 2009,    0,    0,    0,    0,    0,    0, 2009,
 2009, 2009, 2009, 2009, 2009, 2009, 2009, 2009, 2009, 2009,
 2009,    0, 2009, 2009, 2009, 2009, 2009, 2009, 2009, 2009,
 2009, 2009, 2009, 2009, 2009,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    1,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 2009,  515,    0,   36,    0,    0,    0,  549,    0,    0,
    0,   71,  106,    0,    0,    0,    0,    0,    0,  141,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 2009,
    0, 2009,    0, 1109,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 2009, 2009, 2009,    0, 2009, 2009,
    0, 1907, 1826, 1839, 1466, 1500, 1547, 1745, 1364, 1398,
    0,    0,    0,    0,    0,    0,    0,    0, 1023,    0,
    0,  921,    0,    0, 2009,    0,    0,    0,    0,    0,
    0,  583,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  620,    0,    0,    0,    0, 2009,  177,    0,
  444,  480, 2009,    0,    0, 2009,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 2009,    0, 2009,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 2009,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyGindex = {          -26,
  -29,    0,    0,    0,    0,    0,    0,   80,  212,   82,
  178,  182, 1876,    0,    0,    0,  171,   78,   -3,   74,
   79,    0,  161,    0,    0,  162,    0,  140,    0,    0,
  -67,  -62,    0,    0,    0,  -92,    0,  599,    0,    0,
   91,    0,    0,    0, -149,    0,    0,    0,    0,    0,
    0,    0,   93,    0,    0,   33,    0,   38,    0,    0,
    0,    0,    0,    0,  -21,   47,    0,   40,
  };
  protected static  short [] yyTable = {           259,
  103,  217,   56,  211,   57,    5,  138,    6,   63,   40,
  143,    5,  145,    6,    5,  192,    6,  240,  292,    5,
  221,    6,  225,  141,  141,   40,    5,  222,    6,  159,
  160,  157,  156,  103,  213,  105,  244,  245,  249,  250,
  293,  103,  257,  103,  103,  103,   30,  103,  173,   58,
   59,   60,   61,   33,  165,  166,  167,  168,  285,   34,
  103,  103,  103,   76,   77,   78,   79,   80,  105,  269,
   79,  269,  269,    5,  212,    6,  105,   40,  105,  105,
  105,  264,  105,   41,   40,  274,   81,   42,  273,  174,
  175,  103,   56,  103,   57,  105,  105,  105,  276,  278,
   43,  275,  277,   79,   67,   81,   66,   97,   98,   99,
  227,   79,  229,   79,   79,   79,   68,   79,  235,  236,
  237,   64,   65,   69,  103,  136,  105,  137,  105,  139,
   79,   79,   79,  163,  164,  169,  170,  155,   81,  173,
  108,  185,  171,  172,   38,   39,   81,  184,   81,   81,
   81,  186,   81,  266,  267,  241,  177,  177,  187,  105,
  182,  182,  188,   79,  189,   81,   81,   81,  190,  191,
   62,  207,  208,  108,  209,  210,   80,  215,  218,  223,
  226,  108,  253,  108,  108,  108,  238,  108,  239,  254,
  258,  268,  243,  270,   79,  248,  272,  265,   81,  279,
  108,  108,  108,  280,  281,  271,  282,  283,  284,   80,
  288,  291,  294,  296,    8,   37,  224,   80,  144,   80,
   80,   80,  228,   80,  146,  162,    1,  180,    2,   81,
  183,    3,    4,  108,   55,   76,   80,   80,   80,  260,
    1,  286,    2,  287,  261,    3,    4,    1,  263,    2,
  140,  142,    3,    4,  262,  290,  216,  243,  255,  248,
    0,  295,  256,  104,  108,    0,    0,    0,  103,   80,
    0,  103,  103,  103,  103,  103,    0,    0,    0,    0,
   98,    0,  103,  103,  103,  103,  103,  103,  103,  103,
  103,  103,  103,  103,  103,  103,  103,  103,  103,  103,
   80,    0,  103,  105,    0,    0,  105,  105,  105,  105,
  105,    0,    0,    0,    0,    0,    0,  105,  105,  105,
  105,  105,  105,  105,  105,  105,  105,  105,  105,  105,
  105,  105,  105,  105,  105,    0,    0,  105,   79,    0,
    0,   79,   79,   79,   79,   79,    0,    0,    0,    0,
    0,    0,   79,   79,   79,   79,   79,   79,   79,   79,
   79,   79,   79,   79,   79,   79,   79,   79,   79,   79,
    0,    0,   79,   81,    0,    0,   81,   81,   81,   81,
   81,    0,    0,    0,    0,    0,    0,   81,   81,   81,
   81,   81,   81,   81,   81,   81,   81,   81,   81,   81,
   81,   81,   81,   81,   81,    0,    0,   81,  108,    0,
    0,  108,  108,  108,  108,  108,    0,    0,    0,    0,
    0,    0,  108,  108,  108,  108,  108,  108,  108,  108,
  108,  108,  108,  108,  108,  108,  108,  108,  108,  108,
    0,    0,  108,   82,   80,    0,    0,   80,   80,   80,
   80,   80,    6,    0,    0,    0,    0,    0,   80,   80,
   80,   80,   80,   80,   80,   80,   80,   80,   80,   80,
   80,   80,   80,   80,   80,   80,   82,    0,   80,  109,
    0,    0,   74,  193,   82,    0,   82,   82,   82,    0,
   82,    0,    0,    6,    0,    0,    6,    0,   76,   77,
   78,   79,   80,   82,   82,   82,    0,    0,    0,    0,
    0,    0,  109,    0,   70,    0,    0,    0,    0,    0,
  109,   81,  109,  109,  109,    0,  109,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   82,    0,    0,  109,
  109,  109,   97,   98,   99,    6,    0,   70,   73,    0,
    0,    0,    0,    0,    0,   70,    0,   70,   70,   70,
    0,    0,    0,    0,    0,    0,    0,   82,    0,    0,
    0,    0,  109,    0,   70,   70,   70,    0,    0,    0,
    0,   73,   72,    0,    0,    0,    0,    0,    0,   73,
    0,   73,   73,   73,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  109,    0,    0,    0,   70,   73,   73,
   73,    0,    0,    0,    0,   72,    0,    0,    0,   71,
    0,    0,    0,   72,    0,   72,   72,   72,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   70,    0,
    0,   73,   72,   72,   72,    0,    0,    0,    0,    0,
    0,    0,   71,    0,    0,    0,    0,    0,    0,    0,
   71,    0,   71,   71,   71,    0,   22,    0,    0,    0,
    0,    0,   73,    0,    0,   72,    0,    0,    0,   71,
   71,   71,  194,  195,  196,  197,  198,  199,  200,  201,
  202,  203,  204,  205,  206,    0,    0,    0,    0,    0,
    0,  214,    0,    0,    0,    0,   72,   22,    0,    0,
   22,   82,   71,    0,   82,   82,   82,   82,   82,    0,
    6,    0,    0,    6,    6,   82,   82,   82,   82,   82,
   82,   82,   82,   82,   82,   82,   82,   82,   82,   82,
   82,   82,   82,   71,    0,   82,    0,  109,    0,    0,
  109,  109,  109,  109,  109,    0,    0,    0,    0,   22,
    0,  109,  109,  109,  109,  109,  109,  109,  109,  109,
  109,  109,  109,  109,  109,  109,  109,  109,  109,    0,
    0,  109,   70,    0,    0,   70,   70,   70,   70,   70,
    0,    0,    0,    0,    0,    0,   70,   70,   70,   70,
   70,   70,   70,   70,   70,   70,   70,   70,   70,   70,
   70,   70,   70,   70,    0,    0,   73,    0,    0,   73,
   73,   73,   73,   73,    0,    0,    0,    0,    0,    0,
   73,   73,   73,   73,   73,   73,   73,   73,   73,   73,
   73,   73,   73,   73,   73,   73,   73,   73,    0,    0,
   72,    0,    0,   72,   72,   72,   72,   72,    0,    0,
    0,    0,    0,    0,   72,   72,   72,   72,   72,   72,
   72,   72,   72,   72,   72,   72,   72,   72,   72,   72,
   72,   72,    0,    0,    0,    0,   63,   71,    0,    0,
   71,   71,   71,   71,   71,    0,    0,    0,    0,    0,
    0,   71,   71,   71,   71,   71,   71,   71,   71,   71,
   71,   71,   71,   71,   71,   71,   71,   71,   71,   63,
  131,    0,    0,    0,    0,    0,    0,   63,    0,   63,
   63,   63,    0,    0,   22,    0,    0,   22,   22,   22,
   22,    0,    0,    0,    0,    0,   63,   63,   63,    0,
    0,    0,    0,  131,   61,    0,    0,    0,    0,    0,
    0,  131,    0,  131,  131,  131,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   63,
  131,  131,  131,    0,    0,    0,    0,   61,   59,    0,
    0,    0,    0,    0,    0,   61,    0,   61,   61,   61,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   63,    0,    0,  131,   61,   61,   61,    0,    0,    0,
    0,   59,  133,    0,    0,    0,    0,    0,    0,   59,
    0,   59,   59,   59,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  131,    0,    0,   61,   59,   59,
   59,    0,    0,    0,    0,  133,    0,    0,    0,    0,
    0,    0,   57,  133,    0,  133,  133,  133,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   61,    0,
    0,   59,  133,  133,  133,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   57,    0,   18,    0,    0,
    0,    0,    0,   57,    0,   57,   57,   57,   19,    0,
    0,    0,   59,    0,    0,  133,    0,    0,    0,    0,
    0,    0,   57,   57,   57,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   18,    0,
    0,   18,    0,    0,    0,    0,  133,    0,    0,   19,
    0,    0,   19,    0,   63,   57,    0,   63,   63,   63,
   63,   63,    0,    0,    0,    0,    0,    0,   63,   63,
   63,   63,   63,   63,   63,   63,   63,   63,    0,   63,
   63,   63,   63,   63,   63,   63,   57,    0,  131,    0,
   18,  131,  131,  131,  131,  131,    0,    0,    0,    0,
    0,   19,  131,  131,  131,  131,  131,  131,  131,  131,
  131,  131,    0,  131,  131,  131,  131,  131,  131,  131,
    0,    0,   61,    0,    0,   61,   61,   61,   61,   61,
    0,    0,    0,    0,    0,    0,   61,   61,   61,   61,
   61,   61,   61,   61,   61,    0,    0,   61,   61,   61,
   61,   61,   61,   61,    0,    0,   59,    0,    0,   59,
   59,   59,   59,   59,    0,    0,    0,    0,    0,    0,
   59,   59,   59,   59,   59,   59,   59,   59,    0,    0,
    0,   59,   59,   59,   59,   59,   59,   59,    0,    0,
  133,    0,    0,  133,  133,  133,  133,  133,    0,    0,
    0,    0,    0,    0,  133,  133,  133,  133,  133,  133,
  133,  133,   52,    0,    0,  133,  133,  133,  133,  133,
  133,  133,    0,    0,    0,    0,    0,    0,    0,   51,
   57,    0,    0,   57,   57,   57,   57,   57,    0,   53,
   51,   54,    0,    0,   57,   57,   57,   57,   57,   57,
   57,    0,    0,    0,    0,   57,   57,   57,   57,   57,
   57,   57,   51,   52,    0,   18,    0,    0,   18,   18,
   51,   18,   51,   51,   51,    0,   19,    0,    0,   19,
   19,    0,   19,    0,    0,    0,    0,    0,    0,   51,
   51,   51,    0,    0,    0,    0,   52,   53,    0,    0,
    0,    0,    0,    0,   52,    0,   52,   52,   52,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   51,   52,   52,   52,    0,    0,    0,    0,
   53,   46,    0,    0,    0,    0,    0,    0,   53,    0,
   53,   53,   53,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   51,    0,    0,   52,   53,   53,   53,
    0,    0,    0,    0,   46,   47,    0,    0,    0,    0,
    0,    0,   46,    0,   46,   46,   46,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   52,    0,    0,
   53,   46,   46,   46,    0,    0,    0,    0,   47,   48,
    0,    0,    0,    0,    0,    0,   47,    0,   47,   47,
   47,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   53,    0,    0,   46,   47,   47,   47,    0,    0,
    0,    0,   48,    0,    0,    0,    0,    0,    0,    0,
   48,    0,   48,   48,   48,    0,   49,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   47,   48,
   48,   48,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   44,   45,   46,   47,   48,   49,   50,   49,
    0,    0,    0,    0,    0,    0,    0,   49,    0,   49,
   49,   49,   48,    0,    0,    0,    0,   51,    0,    0,
   51,   51,   51,   51,   51,    0,   49,   49,   49,    0,
    0,   51,   51,   51,   51,   51,    0,    0,    0,    0,
    0,    0,   51,   51,   51,   51,   51,   51,   51,    0,
    0,   52,    0,    0,   52,   52,   52,   52,   52,   49,
    0,    0,    0,    0,    0,   52,   52,   52,   52,   52,
    0,    0,    0,    0,    0,    0,   52,   52,   52,   52,
   52,   52,   52,    0,    0,   53,    0,    0,   53,   53,
   53,   53,   53,    0,    0,    0,    0,    0,    0,   53,
   53,   53,   53,   53,    0,    0,    0,    0,    0,    0,
   53,   53,   53,   53,   53,   53,   53,    0,    0,   46,
    0,    0,   46,   46,   46,   46,   46,    0,    0,    0,
    0,    0,    0,   46,   46,   46,   46,    0,    0,    0,
    0,    0,    0,    0,   46,   46,   46,   46,   46,   46,
   46,    0,    0,   47,    0,    0,   47,   47,   47,   47,
   47,    0,    0,    0,   50,    0,    0,   47,   47,   47,
   47,    0,    0,    0,    0,    0,    0,    0,   47,   47,
   47,   47,   47,   47,   47,    0,    0,   48,    0,    0,
   48,   48,   48,   48,   48,    0,    0,   50,    0,    0,
    0,   48,   48,   48,   48,   50,    0,   50,   50,   50,
    0,   43,   48,   48,   48,   48,   48,   48,   48,    0,
    0,    0,    0,    0,   50,   50,   50,    0,    0,    0,
    0,    0,    0,    0,   49,    0,    0,   49,   49,   49,
   49,   49,    0,    0,   43,   44,    0,    0,   49,   49,
   49,   49,   43,    0,   43,   43,   43,   50,   45,   49,
   49,   49,   49,   49,   49,   49,    0,    0,    0,    0,
    0,   43,   43,   43,    0,    0,    0,    0,   44,    0,
    0,    0,    0,    0,    0,    0,   44,    0,   44,   44,
   44,   45,   41,    0,    0,    0,    0,    0,    0,   45,
    0,   45,   45,   45,   43,   44,   44,   44,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   45,   45,
   45,    0,    0,    0,    0,   41,   42,    0,    0,    0,
    0,    0,    0,   41,    0,    0,   41,    0,   44,  147,
  148,  149,  150,  151,  152,  153,  154,    0,  158,  161,
    0,   45,   41,   41,   41,    0,    0,    0,    0,   42,
    0,    0,    0,    0,    0,    0,    0,   42,    0,    0,
   42,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   41,   42,   42,   42,  100,
    0,    0,    0,  101,    0,  104,    0,    0,    0,  105,
  102,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  103,    0,   42,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   50,    0,    0,   50,   50,   50,   50,   50,
    0,    0,    0,    0,    0,    0,   50,   50,   50,   50,
  230,  231,  232,    0,  233,  234,    0,   50,   50,   50,
   50,   50,   50,   50,   66,    0,    0,    0,   66,    0,
   66,    0,    0,    0,   66,   66,    0,    0,    0,   43,
    0,    0,   43,   43,   43,   43,   43,    0,    0,    0,
    0,    0,   66,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   43,   43,   43,   43,   43,   43,
   43,    0,    0,   44,    0,    0,   44,   44,   44,   44,
   44,    0,    0,    0,    0,    0,   45,    0,    0,   45,
   45,   45,   45,   45,    0,    0,    0,    0,   44,   44,
   44,   44,   44,   44,   44,    0,    0,    0,    0,    0,
    0,   45,   45,   45,   45,   45,   45,   45,    0,    0,
   41,    0,    0,   41,   41,   41,   41,  100,    0,    0,
    0,  101,    0,  104,    0,    0,    0,  105,    0,    0,
    0,    0,    0,    0,    0,   41,   41,   41,   41,   41,
   41,   41,    0,    0,   42,  103,    0,   42,   42,   42,
   42,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   70,   71,   72,   73,   74,   75,    0,    0,    0,   42,
   42,   42,   42,   42,   42,   42,    0,    0,    0,    0,
   76,   77,   78,   79,   80,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   81,   82,   83,    0,    0,   84,   85,
   86,   87,   88,   89,   90,   91,   92,   93,   94,   95,
   96,    0,    0,    0,   97,   98,   99,    0,    0,    0,
    0,    0,    0,    0,    0,   66,   66,   66,   66,   66,
   66,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   66,   66,   66,   66,   66,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   66,   66,
   66,    0,    0,   66,   66,   66,   66,   66,   66,   66,
   66,   66,   66,   66,   66,   66,    0,    0,    0,   66,
   66,   66,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   70,   71,
   72,   73,   74,   75,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   76,   77,
   78,   79,   80,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   81,   82,    0,    0,    0,   84,   85,   86,   87,
   88,   89,   90,   91,   92,   93,   94,   95,   96,    0,
    0,    0,   97,   98,   99,
  };
  protected static  short [] yyCheck = {            41,
    0,   47,   43,   41,   45,   43,   33,   45,  124,   44,
   40,   43,   42,   45,   43,   83,   45,   41,   41,   43,
  113,   45,   41,   44,   44,   44,   43,  120,   45,   61,
   62,   60,   61,   33,  102,    0,   41,   42,   41,   42,
   63,   41,   41,   43,   44,   45,   36,   47,  262,  282,
  283,  284,  285,   40,   58,   59,   60,   61,   93,   36,
   60,   61,   62,  277,  278,  279,  280,  281,   33,  219,
    0,  221,  222,   43,  101,   45,   41,   44,   43,   44,
   45,   41,   47,  274,   44,   41,  300,  272,   44,  303,
  304,   91,   43,   93,   45,   60,   61,   62,   41,   41,
  273,   44,   44,   33,  290,    0,  289,  321,  322,  323,
  140,   41,  142,   43,   44,   45,  291,   47,  318,  319,
  320,  287,  288,  292,  124,  263,   91,   44,   93,  263,
   60,   61,   62,   56,   57,   62,   63,   61,   33,  262,
    0,   40,   64,   65,    5,    6,   41,   58,   43,   44,
   45,   40,   47,  216,  217,  185,   66,   67,   40,  124,
   68,   69,   40,   93,   40,   60,   61,   62,   40,   40,
  286,   40,   40,   33,   40,  263,    0,   58,   91,  265,
  265,   41,   41,   43,   44,   45,   63,   47,   42,   41,
   41,  218,  262,  223,  124,  262,  226,  261,   93,   41,
   60,   61,   62,   41,   41,  267,   41,   41,   41,   33,
  262,  268,   41,   41,  272,    4,  137,   41,   41,   43,
   44,   45,  141,   47,   43,   55,  264,   67,  266,  124,
   69,  269,  270,   93,  275,  277,   60,   61,   62,  207,
  264,  271,  266,  273,  207,  269,  270,  264,  209,  266,
  271,  271,  269,  270,  208,  277,  302,  262,  257,  262,
   -1,  291,  261,   42,  124,   -1,   -1,   -1,  268,   93,
   -1,  271,  272,  273,  274,  275,   -1,   -1,   -1,   -1,
  322,   -1,  282,  283,  284,  285,  286,  287,  288,  289,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
  124,   -1,  302,  268,   -1,   -1,  271,  272,  273,  274,
  275,   -1,   -1,   -1,   -1,   -1,   -1,  282,  283,  284,
  285,  286,  287,  288,  289,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  299,   -1,   -1,  302,  268,   -1,
   -1,  271,  272,  273,  274,  275,   -1,   -1,   -1,   -1,
   -1,   -1,  282,  283,  284,  285,  286,  287,  288,  289,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
   -1,   -1,  302,  268,   -1,   -1,  271,  272,  273,  274,
  275,   -1,   -1,   -1,   -1,   -1,   -1,  282,  283,  284,
  285,  286,  287,  288,  289,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  299,   -1,   -1,  302,  268,   -1,
   -1,  271,  272,  273,  274,  275,   -1,   -1,   -1,   -1,
   -1,   -1,  282,  283,  284,  285,  286,  287,  288,  289,
  290,  291,  292,  293,  294,  295,  296,  297,  298,  299,
   -1,   -1,  302,    0,  268,   -1,   -1,  271,  272,  273,
  274,  275,    0,   -1,   -1,   -1,   -1,   -1,  282,  283,
  284,  285,  286,  287,  288,  289,  290,  291,  292,  293,
  294,  295,  296,  297,  298,  299,   33,   -1,  302,    0,
   -1,   -1,  261,  262,   41,   -1,   43,   44,   45,   -1,
   47,   -1,   -1,   41,   -1,   -1,   44,   -1,  277,  278,
  279,  280,  281,   60,   61,   62,   -1,   -1,   -1,   -1,
   -1,   -1,   33,   -1,    0,   -1,   -1,   -1,   -1,   -1,
   41,  300,   43,   44,   45,   -1,   47,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   93,   -1,   -1,   60,
   61,   62,  321,  322,  323,   93,   -1,   33,    0,   -1,
   -1,   -1,   -1,   -1,   -1,   41,   -1,   43,   44,   45,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,   -1,
   -1,   -1,   93,   -1,   60,   61,   62,   -1,   -1,   -1,
   -1,   33,    0,   -1,   -1,   -1,   -1,   -1,   -1,   41,
   -1,   43,   44,   45,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  124,   -1,   -1,   -1,   93,   60,   61,
   62,   -1,   -1,   -1,   -1,   33,   -1,   -1,   -1,    0,
   -1,   -1,   -1,   41,   -1,   43,   44,   45,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,
   -1,   93,   60,   61,   62,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   33,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   41,   -1,   43,   44,   45,   -1,    0,   -1,   -1,   -1,
   -1,   -1,  124,   -1,   -1,   93,   -1,   -1,   -1,   60,
   61,   62,   84,   85,   86,   87,   88,   89,   90,   91,
   92,   93,   94,   95,   96,   -1,   -1,   -1,   -1,   -1,
   -1,  103,   -1,   -1,   -1,   -1,  124,   41,   -1,   -1,
   44,  268,   93,   -1,  271,  272,  273,  274,  275,   -1,
  268,   -1,   -1,  271,  272,  282,  283,  284,  285,  286,
  287,  288,  289,  290,  291,  292,  293,  294,  295,  296,
  297,  298,  299,  124,   -1,  302,   -1,  268,   -1,   -1,
  271,  272,  273,  274,  275,   -1,   -1,   -1,   -1,   93,
   -1,  282,  283,  284,  285,  286,  287,  288,  289,  290,
  291,  292,  293,  294,  295,  296,  297,  298,  299,   -1,
   -1,  302,  268,   -1,   -1,  271,  272,  273,  274,  275,
   -1,   -1,   -1,   -1,   -1,   -1,  282,  283,  284,  285,
  286,  287,  288,  289,  290,  291,  292,  293,  294,  295,
  296,  297,  298,  299,   -1,   -1,  268,   -1,   -1,  271,
  272,  273,  274,  275,   -1,   -1,   -1,   -1,   -1,   -1,
  282,  283,  284,  285,  286,  287,  288,  289,  290,  291,
  292,  293,  294,  295,  296,  297,  298,  299,   -1,   -1,
  268,   -1,   -1,  271,  272,  273,  274,  275,   -1,   -1,
   -1,   -1,   -1,   -1,  282,  283,  284,  285,  286,  287,
  288,  289,  290,  291,  292,  293,  294,  295,  296,  297,
  298,  299,   -1,   -1,   -1,   -1,    0,  268,   -1,   -1,
  271,  272,  273,  274,  275,   -1,   -1,   -1,   -1,   -1,
   -1,  282,  283,  284,  285,  286,  287,  288,  289,  290,
  291,  292,  293,  294,  295,  296,  297,  298,  299,   33,
    0,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   43,
   44,   45,   -1,   -1,  268,   -1,   -1,  271,  272,  273,
  274,   -1,   -1,   -1,   -1,   -1,   60,   61,   62,   -1,
   -1,   -1,   -1,   33,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   41,   -1,   43,   44,   45,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,
   60,   61,   62,   -1,   -1,   -1,   -1,   33,    0,   -1,
   -1,   -1,   -1,   -1,   -1,   41,   -1,   43,   44,   45,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  124,   -1,   -1,   93,   60,   61,   62,   -1,   -1,   -1,
   -1,   33,    0,   -1,   -1,   -1,   -1,   -1,   -1,   41,
   -1,   43,   44,   45,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  124,   -1,   -1,   93,   60,   61,
   62,   -1,   -1,   -1,   -1,   33,   -1,   -1,   -1,   -1,
   -1,   -1,    0,   41,   -1,   43,   44,   45,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,
   -1,   93,   60,   61,   62,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   33,   -1,    0,   -1,   -1,
   -1,   -1,   -1,   41,   -1,   43,   44,   45,    0,   -1,
   -1,   -1,  124,   -1,   -1,   93,   -1,   -1,   -1,   -1,
   -1,   -1,   60,   61,   62,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   -1,  124,   -1,   -1,   41,
   -1,   -1,   44,   -1,  268,   93,   -1,  271,  272,  273,
  274,  275,   -1,   -1,   -1,   -1,   -1,   -1,  282,  283,
  284,  285,  286,  287,  288,  289,  290,  291,   -1,  293,
  294,  295,  296,  297,  298,  299,  124,   -1,  268,   -1,
   93,  271,  272,  273,  274,  275,   -1,   -1,   -1,   -1,
   -1,   93,  282,  283,  284,  285,  286,  287,  288,  289,
  290,  291,   -1,  293,  294,  295,  296,  297,  298,  299,
   -1,   -1,  268,   -1,   -1,  271,  272,  273,  274,  275,
   -1,   -1,   -1,   -1,   -1,   -1,  282,  283,  284,  285,
  286,  287,  288,  289,  290,   -1,   -1,  293,  294,  295,
  296,  297,  298,  299,   -1,   -1,  268,   -1,   -1,  271,
  272,  273,  274,  275,   -1,   -1,   -1,   -1,   -1,   -1,
  282,  283,  284,  285,  286,  287,  288,  289,   -1,   -1,
   -1,  293,  294,  295,  296,  297,  298,  299,   -1,   -1,
  268,   -1,   -1,  271,  272,  273,  274,  275,   -1,   -1,
   -1,   -1,   -1,   -1,  282,  283,  284,  285,  286,  287,
  288,  289,   33,   -1,   -1,  293,  294,  295,  296,  297,
  298,  299,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,
  268,   -1,   -1,  271,  272,  273,  274,  275,   -1,   60,
   61,   62,   -1,   -1,  282,  283,  284,  285,  286,  287,
  288,   -1,   -1,   -1,   -1,  293,  294,  295,  296,  297,
  298,  299,   33,    0,   -1,  268,   -1,   -1,  271,  272,
   41,  274,   43,   44,   45,   -1,  268,   -1,   -1,  271,
  272,   -1,  274,   -1,   -1,   -1,   -1,   -1,   -1,   60,
   61,   62,   -1,   -1,   -1,   -1,   33,    0,   -1,   -1,
   -1,   -1,   -1,   -1,   41,   -1,   43,   44,   45,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   93,   60,   61,   62,   -1,   -1,   -1,   -1,
   33,    0,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,
   43,   44,   45,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  124,   -1,   -1,   93,   60,   61,   62,
   -1,   -1,   -1,   -1,   33,    0,   -1,   -1,   -1,   -1,
   -1,   -1,   41,   -1,   43,   44,   45,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  124,   -1,   -1,
   93,   60,   61,   62,   -1,   -1,   -1,   -1,   33,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   43,   44,
   45,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  124,   -1,   -1,   93,   60,   61,   62,   -1,   -1,
   -1,   -1,   33,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   41,   -1,   43,   44,   45,   -1,    0,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   93,   60,
   61,   62,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  293,  294,  295,  296,  297,  298,  299,   33,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   43,
   44,   45,   93,   -1,   -1,   -1,   -1,  268,   -1,   -1,
  271,  272,  273,  274,  275,   -1,   60,   61,   62,   -1,
   -1,  282,  283,  284,  285,  286,   -1,   -1,   -1,   -1,
   -1,   -1,  293,  294,  295,  296,  297,  298,  299,   -1,
   -1,  268,   -1,   -1,  271,  272,  273,  274,  275,   93,
   -1,   -1,   -1,   -1,   -1,  282,  283,  284,  285,  286,
   -1,   -1,   -1,   -1,   -1,   -1,  293,  294,  295,  296,
  297,  298,  299,   -1,   -1,  268,   -1,   -1,  271,  272,
  273,  274,  275,   -1,   -1,   -1,   -1,   -1,   -1,  282,
  283,  284,  285,  286,   -1,   -1,   -1,   -1,   -1,   -1,
  293,  294,  295,  296,  297,  298,  299,   -1,   -1,  268,
   -1,   -1,  271,  272,  273,  274,  275,   -1,   -1,   -1,
   -1,   -1,   -1,  282,  283,  284,  285,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  293,  294,  295,  296,  297,  298,
  299,   -1,   -1,  268,   -1,   -1,  271,  272,  273,  274,
  275,   -1,   -1,   -1,    0,   -1,   -1,  282,  283,  284,
  285,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  293,  294,
  295,  296,  297,  298,  299,   -1,   -1,  268,   -1,   -1,
  271,  272,  273,  274,  275,   -1,   -1,   33,   -1,   -1,
   -1,  282,  283,  284,  285,   41,   -1,   43,   44,   45,
   -1,    0,  293,  294,  295,  296,  297,  298,  299,   -1,
   -1,   -1,   -1,   -1,   60,   61,   62,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  268,   -1,   -1,  271,  272,  273,
  274,  275,   -1,   -1,   33,    0,   -1,   -1,  282,  283,
  284,  285,   41,   -1,   43,   44,   45,   93,    0,  293,
  294,  295,  296,  297,  298,  299,   -1,   -1,   -1,   -1,
   -1,   60,   61,   62,   -1,   -1,   -1,   -1,   33,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   43,   44,
   45,   33,    0,   -1,   -1,   -1,   -1,   -1,   -1,   41,
   -1,   43,   44,   45,   93,   60,   61,   62,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   60,   61,
   62,   -1,   -1,   -1,   -1,   33,    0,   -1,   -1,   -1,
   -1,   -1,   -1,   41,   -1,   -1,   44,   -1,   93,   44,
   45,   46,   47,   48,   49,   50,   51,   -1,   53,   54,
   -1,   93,   60,   61,   62,   -1,   -1,   -1,   -1,   33,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,   -1,   -1,
   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   93,   60,   61,   62,   36,
   -1,   -1,   -1,   40,   -1,   42,   -1,   -1,   -1,   46,
   47,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   64,   -1,   93,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  268,   -1,   -1,  271,  272,  273,  274,  275,
   -1,   -1,   -1,   -1,   -1,   -1,  282,  283,  284,  285,
  155,  156,  157,   -1,  159,  160,   -1,  293,  294,  295,
  296,  297,  298,  299,   36,   -1,   -1,   -1,   40,   -1,
   42,   -1,   -1,   -1,   46,   47,   -1,   -1,   -1,  268,
   -1,   -1,  271,  272,  273,  274,  275,   -1,   -1,   -1,
   -1,   -1,   64,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  293,  294,  295,  296,  297,  298,
  299,   -1,   -1,  268,   -1,   -1,  271,  272,  273,  274,
  275,   -1,   -1,   -1,   -1,   -1,  268,   -1,   -1,  271,
  272,  273,  274,  275,   -1,   -1,   -1,   -1,  293,  294,
  295,  296,  297,  298,  299,   -1,   -1,   -1,   -1,   -1,
   -1,  293,  294,  295,  296,  297,  298,  299,   -1,   -1,
  268,   -1,   -1,  271,  272,  273,  274,   36,   -1,   -1,
   -1,   40,   -1,   42,   -1,   -1,   -1,   46,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  293,  294,  295,  296,  297,
  298,  299,   -1,   -1,  268,   64,   -1,  271,  272,  273,
  274,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  257,  258,  259,  260,  261,  262,   -1,   -1,   -1,  293,
  294,  295,  296,  297,  298,  299,   -1,   -1,   -1,   -1,
  277,  278,  279,  280,  281,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  300,  301,  302,   -1,   -1,  305,  306,
  307,  308,  309,  310,  311,  312,  313,  314,  315,  316,
  317,   -1,   -1,   -1,  321,  322,  323,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  257,  258,  259,  260,  261,
  262,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  277,  278,  279,  280,  281,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  300,  301,
  302,   -1,   -1,  305,  306,  307,  308,  309,  310,  311,
  312,  313,  314,  315,  316,  317,   -1,   -1,   -1,  321,
  322,  323,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,  260,  261,  262,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  277,  278,
  279,  280,  281,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  300,  301,   -1,   -1,   -1,  305,  306,  307,  308,
  309,  310,  311,  312,  313,  314,  315,  316,  317,   -1,
   -1,   -1,  321,  322,  323,
  };

#line 1048 "Xpath.y"
}
#line default
namespace yydebug {
        using System;
	 public interface yyDebug {
		 void push (int state, Object value);
		 void lex (int state, int token, string name, Object value);
		 void shift (int from, int to, int errorFlag);
		 void pop (int state);
		 void discard (int state, int token, string name, Object value);
		 void reduce (int from, int to, int rule, string text, int len);
		 void shift (int from, int to);
		 void accept (Object value);
		 void error (string message);
		 void reject ();
	 }
	 
	 class yyDebugSimple : yyDebug {
		 void println (string s){
			 Console.WriteLine (s);
		 }
		 
		 public void push (int state, Object value) {
			 println ("push\tstate "+state+"\tvalue "+value);
		 }
		 
		 public void lex (int state, int token, string name, Object value) {
			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
		 }
		 
		 public void shift (int from, int to, int errorFlag) {
			 switch (errorFlag) {
			 default:				// normally
				 println("shift\tfrom state "+from+" to "+to);
				 break;
			 case 0: case 1: case 2:		// in error recovery
				 println("shift\tfrom state "+from+" to "+to
					     +"\t"+errorFlag+" left to recover");
				 break;
			 case 3:				// normally
				 println("shift\tfrom state "+from+" to "+to+"\ton error");
				 break;
			 }
		 }
		 
		 public void pop (int state) {
			 println("pop\tstate "+state+"\ton error");
		 }
		 
		 public void discard (int state, int token, string name, Object value) {
			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
		 }
		 
		 public void reduce (int from, int to, int rule, string text, int len) {
			 println("reduce\tstate "+from+"\tuncover "+to
				     +"\trule ("+rule+") "+text);
		 }
		 
		 public void shift (int from, int to) {
			 println("goto\tfrom state "+from+" to "+to);
		 }
		 
		 public void accept (Object value) {
			 println("accept\tvalue "+value);
		 }
		 
		 public void error (string message) {
			 println("error\t"+message);
		 }
		 
		 public void reject () {
			 println("reject");
		 }
		 
	 }
}
// %token constants
 public class Token {
  public const int StringLiteral = 257;
  public const int IntegerLiteral = 258;
  public const int DecimalLiteral = 259;
  public const int DoubleLiteral = 260;
  public const int NCName = 261;
  public const int QName = 262;
  public const int VarName = 263;
  public const int FOR = 264;
  public const int IN = 265;
  public const int IF = 266;
  public const int THEN = 267;
  public const int ELSE = 268;
  public const int SOME = 269;
  public const int EVERY = 270;
  public const int SATISFIES = 271;
  public const int RETURN = 272;
  public const int AND = 273;
  public const int OR = 274;
  public const int TO = 275;
  public const int DOCUMENT = 276;
  public const int ELEMENT = 277;
  public const int ATTRIBUTE = 278;
  public const int TEXT = 279;
  public const int COMMENT = 280;
  public const int PROCESSING_INSTRUCTION = 281;
  public const int ML = 282;
  public const int DIV = 283;
  public const int IDIV = 284;
  public const int MOD = 285;
  public const int UNION = 286;
  public const int EXCEPT = 287;
  public const int INTERSECT = 288;
  public const int INSTANCE_OF = 289;
  public const int TREAT_AS = 290;
  public const int CASTABLE_AS = 291;
  public const int CAST_AS = 292;
  public const int EQ = 293;
  public const int NE = 294;
  public const int LT = 295;
  public const int GT = 296;
  public const int GE = 297;
  public const int LE = 298;
  public const int IS = 299;
  public const int NODE = 300;
  public const int DOUBLE_PERIOD = 301;
  public const int DOUBLE_SLASH = 302;
  public const int EMPTY_SEQUENCE = 303;
  public const int ITEM = 304;
  public const int AXIS_CHILD = 305;
  public const int AXIS_DESCENDANT = 306;
  public const int AXIS_ATTRIBUTE = 307;
  public const int AXIS_SELF = 308;
  public const int AXIS_DESCENDANT_OR_SELF = 309;
  public const int AXIS_FOLLOWING_SIBLING = 310;
  public const int AXIS_FOLLOWING = 311;
  public const int AXIS_PARENT = 312;
  public const int AXIS_ANCESTOR = 313;
  public const int AXIS_PRECEDING_SIBLING = 314;
  public const int AXIS_PRECEDING = 315;
  public const int AXIS_ANCESTOR_OR_SELF = 316;
  public const int AXIS_NAMESPACE = 317;
  public const int Indicator1 = 318;
  public const int Indicator2 = 319;
  public const int Indicator3 = 320;
  public const int DOCUMENT_NODE = 321;
  public const int SCHEMA_ELEMENT = 322;
  public const int SCHEMA_ATTRIBUTE = 323;
  public const int yyErrorCode = 256;
 }
 namespace yyParser {
  using System;
  /** thrown for irrecoverable syntax errors and stack overflow.
    */
  public class yyException : System.Exception {
    public yyException (string message) : base (message) {
    }
  }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  public interface yyInput {
    /** move on to next token.
        @return false if positioned beyond tokens.
        @throws IOException on input error.
      */
    bool advance (); // throws java.io.IOException;
    /** classifies current token.
        Should not be called if advance() returned false.
        @return current %token or single character.
      */
    int token ();
    /** associated with current token.
        Should not be called if advance() returned false.
        @return value for token().
      */
    Object value ();
  }
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
