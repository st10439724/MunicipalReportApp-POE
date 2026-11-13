using System;
using System.Collections.Generic;

namespace MunicipalReportingApp.Structures
{
  
    public sealed class TreeNode<T>
    {
        public TreeNode(T value, TreeNode<T>? parent)
        {
            Value = value;
            Parent = parent;
        }

        public T Value { get; set; }
        public TreeNode<T>? Parent { get; }
        public List<TreeNode<T>> Children { get; } = new();

        public bool IsLeaf => Children.Count == 0;

        public TreeNode<T> AddChild(T value)
        {
            var child = new TreeNode<T>(value, this);
            Children.Add(child);
            return child;
        }

       
        public int Depth()
        {
            int depth = 0;
            var current = Parent;
            while (current != null)
            {
                depth++;
                current = current.Parent;
            }
            return depth;
        }
    }

    
    ///Practise this one 
    /// A basic (general) tree. Used for the municipality's organisation chart:
    /// Municipality, then departments, then the request categories each department handles.
   
    public sealed class GeneralTree<T>
    {
        public GeneralTree(T rootValue)
        {
            Root = new TreeNode<T>(rootValue, null);
        }

        public TreeNode<T> Root { get; }

   
        public int Count()
        {
            int count = 0;
            foreach (var _ in PreOrder()) count++;
            return count;
        }

        
        public int Height()
        {
            return HeightOf(Root);
        }

        private static int HeightOf(TreeNode<T> node)
        {
            int tallest = 0;
            foreach (var child in node.Children)
            {
                tallest = Math.Max(tallest, HeightOf(child));
            }
            return 1 + tallest;
        }

       
        public IEnumerable<TreeNode<T>> PreOrder()
        {
            var stack = new Stack<TreeNode<T>>();
            stack.Push(Root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;

                for (int i = node.Children.Count - 1; i >= 0; i--)  
                {
                    stack.Push(node.Children[i]);
                }
            }
        }

       
        public IEnumerable<TreeNode<T>> LevelOrder()
        {
            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(Root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return node;
                foreach (var child in node.Children) queue.Enqueue(child);
            }
        }

        
        public TreeNode<T>? Find(Func<T, bool> match)
        {
            foreach (var node in PreOrder())
            {
                if (match(node.Value)) return node;
            }
            return null;
        }
    }
}