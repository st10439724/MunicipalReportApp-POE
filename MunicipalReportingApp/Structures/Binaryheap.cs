using System;
using System.Collections.Generic;

namespace MunicipalReportingApp.Structures
{
    ///Descrption
    /// A binary heap stored in an array. The element that sorts FIRST according to the
    /// comparer is always at index 0, so Peek and Pop give the "best" item immediately.
    ///
    /// For the node at index i:  parent = (i - 1) / 2,  children = 2i + 1 and 2i + 2.
    ///  - Push adds at the end and "sifts up" while it beats its parent.   O(log n)
    ///  - Pop takes the root, moves the last item there and "sifts down".  O(log n)
    
    public sealed class BinaryHeap<T>
    {
        private readonly List<T> _items = new();
        private readonly IComparer<T> _comparer;

        public BinaryHeap(IComparer<T>? comparer = null)
        {
            _comparer = comparer ?? Comparer<T>.Default;
        }

        public BinaryHeap(IEnumerable<T> items, IComparer<T>? comparer = null) : this(comparer)
        {
            // Build-heap: put everything in, then fix from the last parent upwards. O(n).
            _items.AddRange(items);
            for (int i = _items.Count / 2 - 1; i >= 0; i--)
            {
                SiftDown(i);
            }
        }

        public int Count => _items.Count;

        public bool IsEmpty => _items.Count == 0;

        public void Push(T item)
        {
            _items.Add(item);
            SiftUp(_items.Count - 1);
        }

        public T Peek()
        {
            if (_items.Count == 0) throw new InvalidOperationException("The heap is empty.");
            return _items[0];
        }

        public T Pop()
        {
            if (_items.Count == 0) throw new InvalidOperationException("The heap is empty.");

            T top = _items[0];
            int last = _items.Count - 1;

            _items[0] = _items[last];
            _items.RemoveAt(last);

            if (_items.Count > 0) SiftDown(0);
            return top;
        }

        public bool TryPop(out T item)
        {
            if (_items.Count == 0)
            {
                item = default!;
                return false;
            }
            item = Pop();
            return true;
        }

        
        public T[] ToArray()
        {
            return _items.ToArray();
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_comparer.Compare(_items[index], _items[parent]) >= 0) break;

                Swap(index, parent);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            int count = _items.Count;

            while (true)
            {
                int left = 2 * index + 1;
                int right = left + 1;
                int smallest = index;

                if (left < count && _comparer.Compare(_items[left], _items[smallest]) < 0) smallest = left;
                if (right < count && _comparer.Compare(_items[right], _items[smallest]) < 0) smallest = right;

                if (smallest == index) break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            (_items[a], _items[b]) = (_items[b], _items[a]);
        }
    }
}