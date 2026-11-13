using System.Collections.Generic;

namespace MunicipalReportingApp.Structures
{

    public sealed class BinarySearchTree<TKey, TValue> : BinaryTree<TKey, TValue>
    {
        public BinarySearchTree(IComparer<TKey>? comparer = null) : base(comparer)
        {
        }

        public override void Insert(TKey key, TValue value)
        {
            var newNode = new BinaryNode<TKey, TValue>(key, value);

            if (Root == null)
            {
                Root = newNode;
                Count++;
                return;
            }

            var current = Root;

            while (true)
            {
                int c = Comparer.Compare(key, current.Key);

                if (c == 0)
                {
                    current.Value = value;      
                    return;
                }

                if (c < 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = newNode;
                        Count++;
                        return;
                    }
                    current = current.Left;
                }
                else
                {
                    if (current.Right == null)
                    {
                        current.Right = newNode;
                        Count++;
                        return;
                    }
                    current = current.Right;
                }
            }
        }
    }
}