using System;
using System.Collections.Generic;

namespace MunicipalReportingApp.Structures
{
    /// Descrption
    /// A binary tree whose nodes are ordered by key (left smaller, right larger).
    ///
    /// This base class holds everything the three search trees share:
    ///  - the four classic traversals (in-order, pre-order, post-order, level-order),
    ///  - searching, minimum, maximum and range queries,
    ///  - height and node count.
    ///
    /// The sub-classes only differ in HOW they insert a new node:
    ///  - <see cref="BinarySearchTree{TKey,TValue}"/>  just attaches it (can become lopsided),
    ///  - <see cref="AvlTree{TKey,TValue}"/>           rotates to stay balanced,
    ///  - <see cref="RedBlackTree{TKey,TValue}"/>      uses node colours to stay balanced.
    //
    public abstract class BinaryTree<TKey, TValue>
    {
        protected BinaryTree(IComparer<TKey>? comparer)
        {
            Comparer = comparer ?? System.Collections.Generic.Comparer<TKey>.Default;
        }

        protected IComparer<TKey> Comparer { get; }

        public BinaryNode<TKey, TValue>? Root { get; protected set; }

        
        public int Count { get; protected set; }

        
        public int LastSearchComparisons { get; private set; }

        
        public abstract void Insert(TKey key, TValue value);

        // ------------------------------------------------------------------
        //  Searching
        // ------------------------------------------------------------------

        
        public bool TryGetValue(TKey key, out TValue value)
        {
            int comparisons = 0;
            var current = Root;

            while (current != null)
            {
                comparisons++;
                int c = Comparer.Compare(key, current.Key);

                if (c == 0)
                {
                    LastSearchComparisons = comparisons;
                    value = current.Value;
                    return true;
                }

                current = c < 0 ? current.Left : current.Right;
            }

            LastSearchComparisons = comparisons;
            value = default!;
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return TryGetValue(key, out _);
        }

        public KeyValuePair<TKey, TValue>? Min()
        {
            var current = Root;
            if (current == null) return null;
            while (current.Left != null) current = current.Left;
            return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
        }

        public KeyValuePair<TKey, TValue>? Max()
        {
            var current = Root;
            if (current == null) return null;
            while (current.Right != null) current = current.Right;
            return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
        }

      
        public List<KeyValuePair<TKey, TValue>> Range(TKey low, TKey high)
        {
            var result = new List<KeyValuePair<TKey, TValue>>();
            CollectRange(Root, low, high, result);
            return result;
        }

        private void CollectRange(BinaryNode<TKey, TValue>? node, TKey low, TKey high,
                                  List<KeyValuePair<TKey, TValue>> result)
        {
            if (node == null) return;

            int cLow = Comparer.Compare(low, node.Key);
            int cHigh = Comparer.Compare(high, node.Key);

            if (cLow < 0) CollectRange(node.Left, low, high, result);      

            if (cLow <= 0 && cHigh >= 0)
            {
                result.Add(new KeyValuePair<TKey, TValue>(node.Key, node.Value));
            }

            if (cHigh > 0) CollectRange(node.Right, low, high, result);    
        }

        // ------------------------------------------------------------------
        //  Shape
        // ------------------------------------------------------------------

        public int Height()
        {
            return HeightOf(Root);
        }

        private static int HeightOf(BinaryNode<TKey, TValue>? node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(HeightOf(node.Left), HeightOf(node.Right));
        }

        // ------------------------------------------------------------------
        //  Traversals (all iterative, so even a lop-sided tree cannot overflow the stack)
        // ------------------------------------------------------------------

 
        public IEnumerable<KeyValuePair<TKey, TValue>> InOrder()
        {
            var stack = new Stack<BinaryNode<TKey, TValue>>();
            var current = Root;

            while (current != null || stack.Count > 0)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();
                yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Right;
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> ReverseInOrder()
        {
            var stack = new Stack<BinaryNode<TKey, TValue>>();
            var current = Root;

            while (current != null || stack.Count > 0)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Right;
                }

                current = stack.Pop();
                yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Left;
            }
        }

      
        public IEnumerable<KeyValuePair<TKey, TValue>> PreOrder()
        {
            if (Root == null) yield break;

            var stack = new Stack<BinaryNode<TKey, TValue>>();
            stack.Push(Root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);

                if (node.Right != null) stack.Push(node.Right); 
                if (node.Left != null) stack.Push(node.Left);
            }
        }

       
        public IEnumerable<KeyValuePair<TKey, TValue>> PostOrder()
        {
            if (Root == null) yield break;

            var first = new Stack<BinaryNode<TKey, TValue>>();
            var second = new Stack<BinaryNode<TKey, TValue>>();
            first.Push(Root);

            while (first.Count > 0)
            {
                var node = first.Pop();
                second.Push(node);
                if (node.Left != null) first.Push(node.Left);
                if (node.Right != null) first.Push(node.Right);
            }

            while (second.Count > 0)
            {
                var node = second.Pop();
                yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);
            }
        }

   
        public IEnumerable<KeyValuePair<TKey, TValue>> LevelOrder()
        {
            if (Root == null) yield break;

            var queue = new Queue<BinaryNode<TKey, TValue>>();
            queue.Enqueue(Root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);

                if (node.Left != null) queue.Enqueue(node.Left);
                if (node.Right != null) queue.Enqueue(node.Right);
            }
        }
    }
}