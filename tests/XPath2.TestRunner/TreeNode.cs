using System;
using System.Collections.Generic;
using System.Text;

namespace XPath2.TestRunner
{
    internal class TreeNode : TreeNode<string>
    {
        public TreeNode(string value, object tag = null) : base(value, tag)
        {
        }
    }

    internal class TreeNode<T>
    {
        public T Value { get; set; }

        public object Tag { get; set; }

        public IList<TreeNode<T>> ChildNodes { get; set; }

        public TreeNode(T value, object tag = null)
        {
            Value = value;
            Tag = tag;

            ChildNodes = new List<TreeNode<T>>();
        }

        public TreeNode(T value, params TreeNode<T>[] childNodes)
        {
            Value = value;
            ChildNodes = new List<TreeNode<T>>();
            foreach (var child in childNodes)
            {
                ChildNodes.Add(child);
            }
        }

        private string Traverse(int level = 0)
        {
            var result = new StringBuilder();
            result.Append(new string(' ', level * 2));
            result.Append(Value);
            result.Append(Environment.NewLine);

            foreach (var child in ChildNodes)
            {
                result.Append(child.Traverse(level + 1));
            }

            return result.ToString();
        }

        public override string ToString()
        {
            return Traverse();
        }
    }
}