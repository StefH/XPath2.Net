// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This delegate is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public delegate object BinaryOperator(IContextProvider provider, object arg1, object arg2);

    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public class BinaryOperatorNode : AbstractNode
    {
        protected BinaryOperator _binaryOper;
        private readonly XPath2ResultType _resultType;

        public BinaryOperatorNode(XPath2Context context, BinaryOperator action, object node1, object node2,
            XPath2ResultType resultType)
            : base(context)
        {
            _binaryOper = action;
            _resultType = resultType;
            Add(node1);
            Add(node2);
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            return _binaryOper(provider, this[0].Execute(provider, dataPool),
                this[1].Execute(provider, dataPool));
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            return _resultType;
        }
    }
}