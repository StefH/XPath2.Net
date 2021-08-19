/*
XML Path Language (XPath) 2.0 (Second Edition)
W3C Recommendation 14 December 2010 (Link errors corrected 3 January 2011)

http://www.w3.org/TR/xpath20/
http://www.w3.org/TR/2010/REC-xpath20-20101214/

Microsoft Public License (Ms-PL)
See the file License.rtf or License.txt for the license details.

Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
All rights reserved.
*/


%{  
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
    %}

%token StringLiteral
%token IntegerLiteral
%token DecimalLiteral
%token DoubleLiteral

%token NCName
%token QName
%token VarName

%token FOR
%token IN
%token IF
%token THEN
%token ELSE
%token SOME
%token EVERY
%token SATISFIES
%token RETURN
%token AND
%token OR
%token TO
%token DOCUMENT
%token ELEMENT
%token ATTRIBUTE
%token TEXT
%token COMMENT
%token PROCESSING_INSTRUCTION

%token ML
%token DIV
%token IDIV
%token MOD
%token UNION
%token EXCEPT
%token INTERSECT
%token INSTANCE_OF
%token TREAT_AS
%token CASTABLE_AS
%token CAST_AS
%token EQ NE LT GT GE LE
%token IS

%token NODE
%token DOUBLE_PERIOD
%token DOUBLE_SLASH /* // */
%token EMPTY_SEQUENCE /* empty-sequence() */
%token ITEM /* item() */

%token AXIS_CHILD AXIS_DESCENDANT AXIS_ATTRIBUTE AXIS_SELF AXIS_DESCENDANT_OR_SELF
   AXIS_FOLLOWING_SIBLING AXIS_FOLLOWING AXIS_PARENT AXIS_ANCESTOR AXIS_PRECEDING_SIBLING
   AXIS_PRECEDING AXIS_ANCESTOR_OR_SELF AXIS_NAMESPACE

%token Indicator1 /* ? */
%token Indicator2 /* + */
%token Indicator3 /* * */

%token DOCUMENT_NODE
%token SCHEMA_ELEMENT
%token SCHEMA_ATTRIBUTE

%start Expr
%%

Expr
  : ExprSingle
  {
     $$ = AbstractNode.Create(context, $1);
  }
  | Expr ',' ExprSingle
  {
     ExprNode expr = $1 as ExprNode;
     if (expr == null)
         expr = new ExprNode(context, $1);
     expr.Add($3);
     $$ = expr;
  }
  ;

ExprSingle
  : FORExpr
  | QuantifiedExpr
  | IfExpr
  | OrExpr
  ;


FORExpr
  : SimpleForClause RETURN ExprSingle
  {
     ForNode node = (ForNode)$1;
     node.AddTail($3);
     $$ = node;
  }
  ;

SimpleForClause
  : FOR ForClauseBody
  {
     $$ = $2;
  }
  ;

ForClauseBody
  : ForClauseOperator
  | ForClauseBody ',' ForClauseOperator
  {
     ((ForNode)$1).Add($3);
     $$ = $1;
  }
  ;

ForClauseOperator
  : '$' VarName IN ExprSingle
  {
      $$ = new ForNode(context, (Tokenizer.VarName)$2, $4);
  }
  ;

QuantifiedExpr
  : SOME QuantifiedExprBody SATISFIES ExprSingle
  {
     ForNode node = (ForNode)$2;
     node.AddTail($4);
     $$ = new UnaryOperatorNode(context, (provider, arg) => CoreFuncs.Some(arg), node, XPath2ResultType.Boolean);
  }
  | EVERY QuantifiedExprBody SATISFIES ExprSingle
  {
     ForNode node = (ForNode)$2;
     node.AddTail($4);
     $$ = new UnaryOperatorNode(context, (provider, arg) => CoreFuncs.Every(arg), node, XPath2ResultType.Boolean);
  }
  ;

QuantifiedExprBody
  : QuantifiedExprOper
  | QuantifiedExprBody ',' QuantifiedExprOper
  {
     ((ForNode)$1).Add($3);
     $$ = $1;
  }
  ;
  
QuantifiedExprOper
  : '$' VarName IN ExprSingle
  {
     $$ = new ForNode(context, (Tokenizer.VarName)$2, $4);
  }
  ;

IfExpr
  : IF '(' Expr ')' THEN ExprSingle ELSE ExprSingle
  {
     $$ = new IfNode(context, $3, $6, $8);
  }
  ;

OrExpr
  : AndExpr
  | OrExpr OR AndExpr
  {
     $$ = new OrExprNode(context, $1, $3);
  }
  ;

AndExpr 
  : ComparisonExpr
  | AndExpr AND ComparisonExpr
  {
     $$ = new AndExprNode(context, $1, $3);
  }
  ;

ComparisonExpr
  : RangeExpr
  | ValueComp
  | GeneralComp
  | NodeComp
  ;

GeneralComp
  : RangeExpr '=' RangeExpr
  {
     $$ = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralEQ(context, arg1, arg2), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr '!' '='  RangeExpr
  {
     $$ = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralNE(context, arg1, arg2), $1, $4, XPath2ResultType.Boolean);
  }
  | RangeExpr '<' RangeExpr
  {
     $$ = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralLT(context, arg1, arg2), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr '<' '=' RangeExpr
  {
     $$ = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralLE(context, arg1, arg2), $1, $4, XPath2ResultType.Boolean);
  }
  | RangeExpr '>' RangeExpr
  {
     $$ = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralGT(context, arg1, arg2), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr '>' '=' RangeExpr
  {
     $$ = new BinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.GeneralGE(context, arg1, arg2), $1, $4, XPath2ResultType.Boolean);
  }
  ;

ValueComp
  : RangeExpr EQ RangeExpr
  {
     $$ = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorEq(arg1, arg2), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr NE RangeExpr
  {
     $$ = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.Not(CoreFuncs.OperatorEq(arg1, arg2)), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr LT RangeExpr
  {
     $$ = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg2, arg1), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr LE RangeExpr
  {
     $$ = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg2, arg1) == CoreFuncs.True ||
          CoreFuncs.OperatorEq(arg1, arg2) == CoreFuncs.True ? CoreFuncs.True : CoreFuncs.False, $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr GT RangeExpr
  {
     $$ = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg1, arg2), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr GE RangeExpr
  {
     $$ = new AtomizedBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.OperatorGt(arg1, arg2) == CoreFuncs.True ||
          CoreFuncs.OperatorEq(arg1, arg2) == CoreFuncs.True ? CoreFuncs.True : CoreFuncs.False, $1, $3, XPath2ResultType.Boolean);
  }
  ;

NodeComp
  : RangeExpr IS RangeExpr
  {
     $$ = new SingletonBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.SameNode(arg1, arg2), $1, $3, XPath2ResultType.Boolean);
  }
  | RangeExpr '<' '<' RangeExpr
  {
     $$ = new SingletonBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.PrecedingNode(arg1, arg2), $1, $4, XPath2ResultType.Boolean);
  }
  | RangeExpr '>' '>' RangeExpr
  {
     $$ = new SingletonBinaryOperatorNode(context, 
       (provider, arg1, arg2) => CoreFuncs.FollowingNode(arg1, arg2), $1, $4, XPath2ResultType.Boolean);
  }
  ;

RangeExpr
  : AdditiveExpr
  | AdditiveExpr TO AdditiveExpr
  {
      $$ = new RangeNode(context, $1, $3);
  }
  ;

AdditiveExpr
  : MultiplicativeExpr
  | AdditiveExpr '+' MultiplicativeExpr
  {
     $$ = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) + ValueProxy.New(arg2), $1, $3, 
            ArithmeticBinaryOperatorNode.AdditionResult);
  }
  | AdditiveExpr '-' MultiplicativeExpr 
  {
     $$ = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) - ValueProxy.New(arg2), $1, $3, 
            ArithmeticBinaryOperatorNode.SubstractionResult);
  }
  ;

MultiplicativeExpr
  : UnionExpr 
  | MultiplicativeExpr ML UnionExpr 
  {
     $$ = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) * ValueProxy.New(arg2), $1, $3, 
            ArithmeticBinaryOperatorNode.MultiplyResult);
  }
  | MultiplicativeExpr DIV UnionExpr  
  {
     $$ = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) / ValueProxy.New(arg2), $1, $3, 
            ArithmeticBinaryOperatorNode.DivisionResult);
  }
  | MultiplicativeExpr IDIV UnionExpr  
  {
     $$ = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.op_IntegerDivide(ValueProxy.New(arg1), ValueProxy.New(arg2)), $1, $3, null);
  }
  | MultiplicativeExpr MOD UnionExpr  
  {
     $$ = new ArithmeticBinaryOperatorNode(context,
        (provider, arg1, arg2) => ValueProxy.New(arg1) % ValueProxy.New(arg2), $1, $3, null);
  }
  ;

UnionExpr
  : IntersectExceptExpr
  | UnionExpr UNION IntersectExceptExpr
  {
     $$ = new OrderedBinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Union(context, arg1, arg2), $1, $3, XPath2ResultType.NodeSet);
  }
  | UnionExpr '|' IntersectExceptExpr 
  {
     $$ = new OrderedBinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Union(context, arg1, arg2), $1, $3, XPath2ResultType.NodeSet);
  }
  ;

IntersectExceptExpr
  : InstanceofExpr
  | IntersectExceptExpr INTERSECT InstanceofExpr
  {
     $$ = new OrderedBinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Intersect(context, arg1, arg2), $1, $3, XPath2ResultType.NodeSet);
  }
  | IntersectExceptExpr EXCEPT InstanceofExpr
  {
     $$ = new BinaryOperatorNode(context, 
        (provider, arg1, arg2) => CoreFuncs.Except(context, arg1, arg2), $1, $3, XPath2ResultType.NodeSet);
  }
  ;

InstanceofExpr
  : TreatExpr
  | TreatExpr INSTANCE_OF SequenceType
  {
     SequenceType destType = (SequenceType)$3;
     $$ = new UnaryOperatorNode(context, 
        (provider, arg) => CoreFuncs.InstanceOf(context, arg, destType), $1, XPath2ResultType.Boolean);
  }
  ;

TreatExpr
  : CastableExpr
  | CastableExpr TREAT_AS SequenceType      
  {
     SequenceType destType = (SequenceType)$3;
     $$ = new UnaryOperatorNode(context, 
        (provider, arg) => CoreFuncs.TreatAs(context, arg, destType), $1, CoreFuncs.GetXPath2ResultType(destType));
  }
  ;

CastableExpr
  : CastExpr  
  | CastExpr CASTABLE_AS SingleType
  {     
     SequenceType destType = (SequenceType)$3;
     ValueNode value = $1 as ValueNode;
     bool isString = $1 is String || (value != null && value.Content is String);
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
     $$ = new UnaryOperatorNode(context, (provider, arg) => CoreFuncs.Castable(context, arg, destType, isString), $1, XPath2ResultType.Boolean);
  }
  ;

CastExpr
  : UnaryExpr
  | UnaryExpr CAST_AS SingleType
  {
     SequenceType destType = (SequenceType)$3;
     ValueNode value = $1 as ValueNode;
     bool isString = $1 is String || (value != null && value.Content is String);
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
     $$ = new UnaryOperatorNode(context, (provider, arg) => 
        CoreFuncs.CastTo(context, arg, destType, isString), $1, CoreFuncs.GetXPath2ResultType(destType));
  }
  ;
  
UnaryExpr
  : UnaryOperator ValueExpr
  {
     if ($1 != null)
     {
       if ($1 == CoreFuncs.True)
          $$ = new AtomizedUnaryOperatorNode(context, (provider, arg) => -ValueProxy.New(arg), $2, XPath2ResultType.Number);
        else
          $$ = new AtomizedUnaryOperatorNode(context, (provider, arg) => 0 + ValueProxy.New(arg), $2, XPath2ResultType.Number);
     }
     else
        $$ = $2;
  }
  ;

UnaryOperator
  : /* Empty */
  {
     $$ = null;
  }
  | '+' UnaryOperator
  {
     if ($2 == null)
       $$ = CoreFuncs.False;
     else
        $$ = $2;
  } 
  | '-' UnaryOperator
  {
     if ($2 == null || $2 == CoreFuncs.False)
         $$ = CoreFuncs.True;
     else
         $$ = CoreFuncs.False;
  }
  ; 

ValueExpr
  : PathExpr 
  ;

PathExpr
  : '/' 
  {
     $$ = new UnaryOperatorNode(context, (provider, arg) => 
        XPath2NodeIterator.Create(CoreFuncs.GetRoot(arg)), new ContextItemNode(context), XPath2ResultType.NodeSet);
  }
  | '/' RelativePathExpr
  { 
     $$ = $2 is PathStep 
       ? new PathExprNode(context, (PathStep)$2) : $2;
  }
  | DOUBLE_SLASH RelativePathExpr
  {
     PathStep descendantOrSelf = new PathStep(SequenceType.Node, 
        XPath2ExprType.DescendantOrSelf);
     descendantOrSelf.AddLast(PathStep.Create(context, $2));
     $$ = new PathExprNode(context, descendantOrSelf);
  }
  | RelativePathExpr
  {
     $$ = $1 is PathStep 
       ? new PathExprNode(context, (PathStep)$1) : $1;
  }
  ;

RelativePathExpr
  : StepExpr
  | RelativePathExpr '/' StepExpr
  {
     PathStep relativePathExpr = PathStep.Create(context, $1);
     relativePathExpr.AddLast(PathStep.Create(context, $3));
     $$ = relativePathExpr;
  }
  | RelativePathExpr DOUBLE_SLASH StepExpr
  {
     PathStep relativePathExpr = PathStep.Create(context, $1);
     PathStep descendantOrSelf = new PathStep(SequenceType.Node, 
        XPath2ExprType.DescendantOrSelf);
     relativePathExpr.AddLast(descendantOrSelf);
     relativePathExpr.AddLast(PathStep.Create(context, $3));
     $$ = relativePathExpr;
  }
  ;

StepExpr
  : AxisStep 
  | FilterExpr
  ;

AxisStep
  : ForwardStep
  | ForwardStep PredicateList
  {
     $$ = PathStep.CreateFilter(context, $1, (List<Object>)$2);
  }
  | ReverseStep
  | ReverseStep PredicateList
  {
     $$ = PathStep.CreateFilter(context, $1, (List<Object>)$2);
  }
  ;

ForwardStep
   : AXIS_CHILD NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Child);
   }
   | AXIS_DESCENDANT NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Descendant);
   }
   | AXIS_ATTRIBUTE NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Attribute);
   }
   | AXIS_SELF NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Self);
   }
   | AXIS_DESCENDANT_OR_SELF NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.DescendantOrSelf);
   }
   | AXIS_FOLLOWING_SIBLING NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.FollowingSibling);
   }
   | AXIS_FOLLOWING NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Following);
   }
   | AXIS_NAMESPACE NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Namespace);
   }
   | AbbrevForwardStep
   ;

AbbrevForwardStep
   : '@' NodeTest
   {
       $$ = new PathStep($2, XPath2ExprType.Attribute);
   }
   | NodeTest
   {
       $$ = new PathStep($1, XPath2ExprType.Child);
   }
   ;

ReverseStep
   : AXIS_PARENT NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Parent);
   }
   | AXIS_ANCESTOR NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Ancestor);
   }
   | AXIS_PRECEDING_SIBLING NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.PrecedingSibling);
   }
   | AXIS_PRECEDING NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.Preceding);
   }
   | AXIS_ANCESTOR_OR_SELF NodeTest 
   {
      $$ = new PathStep($2, XPath2ExprType.AncestorOrSelf);
   }
   | AbbrevReverseStep
   ;

AbbrevReverseStep
   : DOUBLE_PERIOD
   {
      $$ = new PathStep(XPath2ExprType.Parent);
   }
   ;

NodeTest
   : KindTest
   | NameTest
   ;

NameTest
   : QName
   {
      XmlQualifiedName qualifiedName = QNameParser.Parse((String)$1, 
        context.NamespaceManager, context.NamespaceManager.DefaultNamespace, context.NameTable);
      $$ = XmlQualifiedNameTest.New(qualifiedName.Name, qualifiedName.Namespace);
   }
   | Wildcard
   ;

Wildcard
   : '*'
   {
      $$ = XmlQualifiedNameTest.New(null, null);
   }
   | NCName ':' '*'
   {
      string ncname = (String)$1;
      string ns = context.NamespaceManager.LookupNamespace(ncname);
      if (ns == null)
        throw new XPath2Exception("XPST0081", Properties.Resources.XPST0081, ncname);
      $$ = XmlQualifiedNameTest.New(null, ns);
   }
   | '*' ':' NCName 
   {
      $$ = XmlQualifiedNameTest.New(context.NameTable.Add((String)$3), null);
   }
   ;

FilterExpr
   : PrimaryExpr
   | PrimaryExpr PredicateList
   {
      $$ = new FilterExprNode(context, $1, (List<Object>)$2);
   }
   ;

PredicateList
   : Predicate
   {
      List<Object> nodes = new List<Object>();
      nodes.Add($1);
      $$ = nodes;
   }
   | PredicateList Predicate
   {
      List<Object> nodes = (List<Object>)$1;
      nodes.Add($2);
      $$ = nodes;
   }
   ;

Predicate
   : '[' Expr ']'
   {
      $$ = $2;
   }
   ;

PrimaryExpr
   : Literal
   | VarRef
   {
      $$ = new VarRefNode(context, (Tokenizer.VarName)$1);
   }
   | ParenthesizedExpr
   | ContextItemExpr
   {
      $$ = new ContextItemNode(context);
   }
   | FunctionCall
   ;

Literal
   : NumericLiteral
   | StringLiteral
   ;

NumericLiteral
   : IntegerLiteral
   | DecimalLiteral
   | DoubleLiteral
   ;
   
VarRef
   : '$' VarName
   {
      $$ = $2;
   }
   ;


ParenthesizedExpr
   : '(' ')'
   {
      $$ = new ValueNode(context, Undefined.Value);
   }
   | '(' Expr ')'
   {
      $$ = $2;
   }
   ;

ContextItemExpr
   : '.'
   ; 

FunctionCall
   : QName '(' ')'
   {
      XmlQualifiedName identity = QNameParser.ParseFunction((string)$1, context.NamespaceManager, 
         context.DefaultFunctionNamespace, context.NameTable);
      string ns = identity.Namespace;
      if (identity.Namespace == String.Empty)            
          ns = XmlReservedNs.NsXQueryFunc;
      $$ = new FuncNode(context, identity.Name, ns);
   }
   | QName '(' Args ')'
   {
      XmlQualifiedName identity = QNameParser.ParseFunction((string)$1, context.NamespaceManager, 
         context.DefaultFunctionNamespace, context.NameTable);
      string ns = identity.Namespace;
      if (identity.Namespace == String.Empty)            
          ns = XmlReservedNs.NsXQueryFunc;
      List<Object> args = (List<Object>)$3;
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
            $$ = new UnaryOperatorNode(context, (provider, arg) => 
               CoreFuncs.CastToItem(context, arg, seqtype), args[0], CoreFuncs.GetXPath2ResultType(seqtype)); 
          }
      else
         $$ = new FuncNode(context, identity.Name, ns, (List<Object>)$3);
   }
   ;

Args
   : ExprSingle
   {
      List<Object> list = new List<Object>();
      list.Add($1);
      $$ = list;
   }
   | Args ',' ExprSingle
   {
      List<Object> list = (List<Object>)$1;
      list.Add($3);
      $$ = list;
   }
   ;

SingleType
   : AtomicType
   | AtomicType '?'
   {
      SequenceType type = (SequenceType)$1;
      type.Cardinality = XmlTypeCardinality.ZeroOrOne;
      $$ = type;
   }
   ;

SequenceType
   : ItemType  
   | ItemType Indicator1
   {
      SequenceType type = (SequenceType)$1;
      type.Cardinality = XmlTypeCardinality.ZeroOrMore; 
      $$ = type;
   }
   | ItemType Indicator2
   {
      SequenceType type = (SequenceType)$1;
      type.Cardinality = XmlTypeCardinality.OneOrMore;
      $$ = type;
   }
   | ItemType Indicator3
   {
      SequenceType type = (SequenceType)$1;
      type.Cardinality = XmlTypeCardinality.ZeroOrOne;
      $$ = type;
   }
   | EMPTY_SEQUENCE
   {
      $$ = SequenceType.Void;
   }
   ;

ItemType
   : AtomicType 
   | KindTest 
   | ITEM
   {
      $$ = new SequenceType(XmlTypeCode.Item);
   }
   ;

AtomicType 
   : QName
   {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)$1, true, out xmlType);
      $$ = new SequenceType((XmlSchemaType)xmlType, XmlTypeCardinality.One, null);
   }
   ;

KindTest
   : DocumentTest
   | ElementTest
   | AttributeTest
   | SchemaElementTest
   | SchemaAttributeTest
   | PITest
   | CommentTest
   | TextTest
   | AnyKindTest
   ;

AnyKindTest
   : NODE '(' ')'
   {
      $$ = SequenceType.Node;
   }
   ;

DocumentTest
   : DOCUMENT_NODE '(' ')'
   {
      $$ = SequenceType.Document;
   }
   | DOCUMENT_NODE '(' ElementTest ')'
   {
      SequenceType type = (SequenceType)$3;
      type.TypeCode = XmlTypeCode.Document;
      $$ = type;
   }
   | DOCUMENT_NODE '(' SchemaElementTest ')'
   {
      SequenceType type = (SequenceType)$3;
      type.TypeCode = XmlTypeCode.Document;
      $$ = type;
   }
   ;

TextTest
   : TEXT '(' ')'
   {
      $$ = SequenceType.Text;
   }
   ;

CommentTest
   : COMMENT '(' ')'
   {
      $$ = SequenceType.Comment;
   }
   ;

PITest
   : PROCESSING_INSTRUCTION '(' ')'
   {
      $$ = SequenceType.ProcessingInstruction;
   }
   | PROCESSING_INSTRUCTION '(' NCName ')'
   {
      XmlQualifiedNameTest nameTest = XmlQualifiedNameTest.New((String)$3, null);
      $$ = new SequenceType(XmlTypeCode.ProcessingInstruction, nameTest);
   }
   | PROCESSING_INSTRUCTION '(' StringLiteral ')'
   {
      XmlQualifiedNameTest nameTest = XmlQualifiedNameTest.New((String)$3, null);
      $$ = new SequenceType(XmlTypeCode.ProcessingInstruction, nameTest);
   }
   ;

ElementTest
   : ELEMENT '(' ')'
   {
      $$ = SequenceType.Element;
   }
   | ELEMENT '(' ElementNameOrWildcard ')'   
   {
      $$ = new SequenceType(XmlTypeCode.Element, (XmlQualifiedNameTest)$3);
   }
   | ELEMENT '(' ElementNameOrWildcard ',' TypeName ')'   
   {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)$5, true, out xmlType);
      $$ = new SequenceType(XmlTypeCode.Element, (XmlQualifiedNameTest)$3, (XmlSchemaType)xmlType, false);      
   }
   | ELEMENT '(' ElementNameOrWildcard ',' TypeName '?' ')'   
   {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)$5, true, out xmlType);
      $$ = new SequenceType(XmlTypeCode.Element, (XmlQualifiedNameTest)$3, (XmlSchemaType)xmlType, true);      
   }
   ;

ElementNameOrWildcard
   : ElementName
   {
      $$ = XmlQualifiedNameTest.New((XmlQualifiedName)QNameParser.Parse((string)$1, 
         context.NamespaceManager, context.NamespaceManager.DefaultNamespace, context.NameTable));
   }
   | '*'
   {
      $$ = XmlQualifiedNameTest.New(null, null);
   }
   ;

AttributeTest
   : ATTRIBUTE '(' ')'
   {
      $$ = SequenceType.Attribute;
   }
   | ATTRIBUTE '(' AttributeOrWildcard ')'
   {
      $$ = new SequenceType(XmlTypeCode.Attribute, (XmlQualifiedNameTest)$3);
   }
   | ATTRIBUTE '(' AttributeOrWildcard ',' TypeName ')'
   {
      XmlSchemaObject xmlType;
      CoreFuncs.TryProcessTypeName(context, (string)$5, true, out xmlType);
      $$ = new SequenceType(XmlTypeCode.Attribute, (XmlQualifiedNameTest)$3, (XmlSchemaType)xmlType);      
   }
   ;

AttributeOrWildcard
   : AttributeName
   {
      $$ = XmlQualifiedNameTest.New((XmlQualifiedName)QNameParser.Parse((string)$1, 
         context.NamespaceManager, context.NamespaceManager.DefaultNamespace, context.NameTable));
   }
   | '*'
   {
      $$ = XmlQualifiedNameTest.New(null, null);
   }
   ;
   
SchemaElementTest
   : SCHEMA_ELEMENT '(' ElementName ')'   
   {
      XmlQualifiedName qname = QNameParser.Parse((string)$3, context.NamespaceManager, 
         context.NamespaceManager.DefaultNamespace, context.NameTable);
      XmlSchemaElement schemaElement = (XmlSchemaElement)context.SchemaSet.GlobalElements[qname];
      if (schemaElement == null)
          throw new XPath2Exception("XPST0008", Properties.Resources.XPST0008, qname);
      $$ = new SequenceType(schemaElement);
   } 
   ;
    
SchemaAttributeTest
   : SCHEMA_ATTRIBUTE '(' AttributeName ')'
   {
      XmlQualifiedName qname = QNameParser.Parse((string)$3, context.NamespaceManager, 
         context.NamespaceManager.DefaultNamespace, context.NameTable);
      XmlSchemaAttribute schemaAttribute = (XmlSchemaAttribute)context.SchemaSet.GlobalAttributes[qname];
      if (schemaAttribute == null)
          throw new XPath2Exception("XPST0008", Properties.Resources.XPST0008, qname);
      $$ = new SequenceType(schemaAttribute);
   }
   ;
    
AttributeName
   : QName
   ;

ElementName
   : QName
   ;
    
TypeName
   : QName
   ;

%%
}