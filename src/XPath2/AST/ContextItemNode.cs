// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class ContextItemNode : AbstractNode
    {
        public ContextItemNode(XPath2Context context)
            : base(context)
        {
        }

        public override bool IsContextSensitive()
        {
            return true;
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            return CoreFuncs.ContextNode(provider);
        }
    }
}