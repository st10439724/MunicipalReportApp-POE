using System;
using System.Collections.Generic;

namespace MunicipalReportingApp.Structures
{
  
    /// Promise i still dont get htis 
    public sealed class AvlTree<TKey, TValue> : BinaryTree<TKey, TValue>
    {
        public AvlTree(IComparer<TKey>? comparer = null) : base(comparer)
        {
        }

        
        public int Rotations { get; private set; }

        public override void Insert(TKey key, TValue value)
        {
            Root = InsertNode(Root, key, value);
        }

        private BinaryNode<TKey, TValue> InsertNode(BinaryNode<TKey, TValue>? node, TKey key, TValue value)
        {
            if (node == null)
            {
                Count++;
                return new BinaryNode<TKey, TValue>(key, value);
            }

            int c = Comparer.Compare(key, node.Key);

            if (c < 0)
            {
                node.Left = InsertNode(node.Left, key, value);
            }
            else if (c > 0)
            {
                node.Right = InsertNode(node.Right, key, value);
            }
            else
            {
                node.Value = value;      
                return node;
            }

            return Rebalance(node);
        }

        private static int HeightOf(BinaryNode<TKey, TValue>? node)
        {
            return node == null ? 0 : node.Height;
        }

        private static void UpdateHeight(BinaryNode<TKey, TValue> node)
        {
            node.Height = 1 + Math.Max(HeightOf(node.Left), HeightOf(node.Right));
        }

    
        private static int BalanceOf(BinaryNode<TKey, TValue> node)
        {
            return HeightOf(node.Left) - HeightOf(node.Right);
        }

        private BinaryNode<TKey, TValue> Rebalance(BinaryNode<TKey, TValue> node)
        {
            UpdateHeight(node);
            int balance = BalanceOf(node);

            if (balance > 1)
            {
                
                if (BalanceOf(node.Left!) < 0)
                {
                    node.Left = RotateLeft(node.Left!);
                }
                return RotateRight(node);
            }

            if (balance < -1)
            {
                
                if (BalanceOf(node.Right!) > 0)
                {
                    node.Right = RotateRight(node.Right!);
                }
                return RotateLeft(node);
            }

            return node;
        }

        private BinaryNode<TKey, TValue> RotateRight(BinaryNode<TKey, TValue> y)
        {
            var x = y.Left!;
            y.Left = x.Right;
            x.Right = y;

            UpdateHeight(y);     
            UpdateHeight(x);
            Rotations++;
            return x;
        }

        private BinaryNode<TKey, TValue> RotateLeft(BinaryNode<TKey, TValue> x)
        {
            var y = x.Right!;
            x.Right = y.Left;
            y.Left = x;

            UpdateHeight(x);
            UpdateHeight(y);
            Rotations++;
            return y;
        }

        
        public bool IsBalanced()
        {
            return CheckBalanced(Root);
        }

        private static bool CheckBalanced(BinaryNode<TKey, TValue>? node)
        {
            if (node == null) return true;
            return Math.Abs(BalanceOf(node)) <= 1 && CheckBalanced(node.Left) && CheckBalanced(node.Right);
        }
    }
}