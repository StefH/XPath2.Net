// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public class ArithmeticUnaryOperatorNode : AtomizedUnaryOperatorNode
    {
        public ArithmeticUnaryOperatorNode(XPath2Context context, UnaryOperator action, object node)
            : base(context, action, node, XPath2ResultType.Number)
        {
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            try
            {
                object value = CoreFuncs.CastToNumber1(Context,
                    CoreFuncs.Atomize(this[0].Execute(provider, dataPool)));
                if (value != Undefined.Value)
                    return _unaryOper(provider, value);
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
    }
}