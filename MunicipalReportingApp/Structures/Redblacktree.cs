using System;
using System.Collections.Generic;

namespace MunicipalReportingApp.Structures
{
    /// i needed assistance with this one help 
    /// A red-black tree (left-leaning variant). Every node is coloured red or black and the
    /// colours obey these rules:
    ///   1. The root is black.
    ///   2. A red node never has a red child (no two reds in a row).
    ///   3. Every path from the root to an empty spot passes the same number of black nodes.
    ///   4. (Left-leaning rule) a red link only ever leans to the left.
    /// Together these keep the longest path at most twice the shortest, so the height stays
    /// O(log n). It is a little less tightly balanced than an AVL tree (so lookups can take a
    /// step or two more) but it only ever needs cheap colour changes and a few rotations per insert,
    /// which makes it a good fit for data that is added all the time.
    /// </summary>
    public sealed class RedBlackTree<TKey, TValue> : BinaryTree<TKey, TValue>
    {
        public RedBlackTree(IComparer<TKey>? comparer = null) : base(comparer)
        {
        }

        public int Rotations { get; private set; }

        public int ColourFlips { get; private set; }

        public override void Insert(TKey key, TValue value)
        {
            Root = Put(Root, key, value);
            Root.IsRed = false;                    
        }

        private static bool IsRed(BinaryNode<TKey, TValue>? node)
        {
            return node != null && node.IsRed;
        }

        private BinaryNode<TKey, TValue> Put(BinaryNode<TKey, TValue>? node, TKey key, TValue value)
        {
            if (node == null)
            {
                Count++;
                return new BinaryNode<TKey, TValue>(key, value) { IsRed = true };  
            }

            int c = Comparer.Compare(key, node.Key);

            if (c < 0) node.Left = Put(node.Left, key, value);
            else if (c > 0) node.Right = Put(node.Right, key, value);
            else node.Value = value;

            if (IsRed(node.Right) && !IsRed(node.Left))
            {
                node = RotateLeft(node);
            }
            if (IsRed(node.Left) && IsRed(node.Left!.Left))
            {
                node = RotateRight(node);
            }
            if (IsRed(node.Left) && IsRed(node.Right))
            {
                FlipColours(node);
            }

            return node;
        }

        private BinaryNode<TKey, TValue> RotateLeft(BinaryNode<TKey, TValue> h)
        {
            var x = h.Right!;
            h.Right = x.Left;
            x.Left = h;
            x.IsRed = h.IsRed;
            h.IsRed = true;
            Rotations++;
            return x;
        }

        private BinaryNode<TKey, TValue> RotateRight(BinaryNode<TKey, TValue> h)
        {
            var x = h.Left!;
            h.Left = x.Right;
            x.Right = h;
            x.IsRed = h.IsRed;
            h.IsRed = true;
            Rotations++;
            return x;
        }

        private void FlipColours(BinaryNode<TKey, TValue> h)
        {
            h.IsRed = !h.IsRed;
            h.Left!.IsRed = !h.Left.IsRed;
            h.Right!.IsRed = !h.Right.IsRed;
            ColourFlips++;
        }

        
        public bool IsValid()
        {
            if (Root == null) return true;
            if (Root.IsRed) return false;
            return BlackHeight(Root) >= 0;
        }

        
        private static int BlackHeight(BinaryNode<TKey, TValue>? node)
        {
            if (node == null) return 1;

            if (IsRed(node.Right)) return -1;                              
            if (node.IsRed && IsRed(node.Left)) return -1;                

            int left = BlackHeight(node.Left);
            int right = BlackHeight(node.Right);

            if (left < 0 || right < 0 || left != right) return -1;         

            return left + (node.IsRed ? 0 : 1);
        }
    }
}