// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class IfNode : AbstractNode
    {
        public IfNode(XPath2Context context, object cond, object thenBranch, object elseBranch)
            : base(context)
        {
            Add(cond);
            Add(thenBranch);
            Add(elseBranch);
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            if (CoreFuncs.BooleanValue(this[0].Execute(provider, dataPool)) == CoreFuncs.True)
                return this[1].Execute(provider, dataPool);
            else
                return this[2].Execute(provider, dataPool);
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            XPath2ResultType res1 = this[1].GetReturnType(dataPool);
            XPath2ResultType res2 = this[2].GetReturnType(dataPool);
            if (res1 == res2)
                return res1;
            return XPath2ResultType.Any;
        }
    }
}