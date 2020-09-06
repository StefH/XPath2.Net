// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This delegate is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public delegate XPath2ResultType GetReturnTypeDelegate(XPath2ResultType resType1, XPath2ResultType resType2);

    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public class ArithmeticBinaryOperatorNode : AtomizedBinaryOperatorNode
    {
        private readonly GetReturnTypeDelegate _returnTypeDelegate;

        public ArithmeticBinaryOperatorNode(XPath2Context context, BinaryOperator action,
            object node1, object node2, GetReturnTypeDelegate returnTypeDelegate)
            : base(context, action, node1, node2, XPath2ResultType.Number)
        {
            _returnTypeDelegate = returnTypeDelegate;
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            try
            {
                object value1 = CoreFuncs.CastToNumber1(Context,
                    CoreFuncs.Atomize(this[0].Execute(provider, dataPool)));
                if (value1 != Undefined.Value)
                {
                    object value2 = CoreFuncs.CastToNumber1(Context,
                        CoreFuncs.Atomize(this[1].Execute(provider, dataPool)));
                    if (value2 != Undefined.Value)
                        return _binaryOper(provider, value1, value2);
                }
                return Undefined.Value;
            }
            catch (DivideByZeroException ex)
            {
                throw new XPath2Exception("", ex.Message, ex);
            }
            catch (OverflowException ex)
            {
                throw new XPath2Exception("", ex.Message, ex);
            }
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            if (_returnTypeDelegate != null)
            {
                XPath2ResultType resType1 = this[0].GetReturnType(dataPool);
                XPath2ResultType resType2 = this[1].GetReturnType(dataPool);
                return _returnTypeDelegate(resType1, resType2);
            }
            return base.GetReturnType(dataPool);
        }

        public static XPath2ResultType AdditionResult(XPath2ResultType resType1, XPath2ResultType resType2)
        {
            if (resType1 == XPath2ResultType.Number ||
                resType2 == XPath2ResultType.Number)
                return XPath2ResultType.Number;
            if (resType1 == XPath2ResultType.DateTime && resType2 == XPath2ResultType.Duration)
                return XPath2ResultType.DateTime;
            if (resType1 == XPath2ResultType.Duration)
            {
                if (resType2 == XPath2ResultType.Duration)
                    return XPath2ResultType.Duration;
                if (resType2 == XPath2ResultType.DateTime)
                    return XPath2ResultType.DateTime;
            }
            return XPath2ResultType.Any;
        }

        public static XPath2ResultType SubstractionResult(XPath2ResultType resType1, XPath2ResultType resType2)
        {
            if (resType1 == XPath2ResultType.Number ||
                resType2 == XPath2ResultType.Number)
                return XPath2ResultType.Number;
            if (resType1 == XPath2ResultType.DateTime)
            {
                if (resType2 == XPath2ResultType.DateTime)
                    return XPath2ResultType.Duration;
                if (resType2 == XPath2ResultType.Duration)
                    return XPath2ResultType.DateTime;
            }
            if (resType1 == XPath2ResultType.Duration && resType2 == XPath2ResultType.Duration)
                return XPath2ResultType.Duration;
            return XPath2ResultType.Any;
        }

        public static XPath2ResultType MultiplyResult(XPath2ResultType resType1, XPath2ResultType resType2)
        {
            if ((resType1 == XPath2ResultType.Duration && resType2 == XPath2ResultType.Number) ||
                (resType1 == XPath2ResultType.Number && resType2 == XPath2ResultType.Duration))
                return XPath2ResultType.Duration;
            if (resType1 == XPath2ResultType.Number && resType2 == XPath2ResultType.Number)
                return XPath2ResultType.Number;
            return XPath2ResultType.Any;
        }

        public static XPath2ResultType DivisionResult(XPath2ResultType resType1, XPath2ResultType resType2)
        {
            if (resType1 == XPath2ResultType.Duration && resType2 == XPath2ResultType.Number)
                return XPath2ResultType.Duration;
            if (resType1 == XPath2ResultType.Duration && resType2 == XPath2ResultType.Duration)
                return XPath2ResultType.Number;
            if (resType1 == XPath2ResultType.Number && resType2 == XPath2ResultType.Number)
                return XPath2ResultType.Number;
            return XPath2ResultType.Any;
        }
    }
}