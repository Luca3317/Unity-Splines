using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnitySplines
{
    /*
     * Credits to https://stackoverflow.com/users/419761/l33t
     * 
     * TODO
     * Replace this?
     * - Mutable struct (but; so is ArraySegment)
     * - Items should be private, but ArraySegment exposes the underlying lists 
     * 
     * Alternatively, class CollectionSegment : ReadOnlyCollection
     */
    public readonly struct ListSegment<T> : IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IEnumerable
    {
        private IList<T> Items { get; }
        public int Offset { get; }
        public int Count { get; }

        public T this[int index] => ElementAt(index);

        public ListSegment(IList<T> items, int offset, int count)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Offset = offset;
            Count = count;

            if (items.Count < offset + count)
            {
                throw new ArgumentException("List segment out of range.", nameof(count));
            }
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null) throw new System.ArgumentNullException(nameof(array));
            if (index < 0) throw new System.ArgumentOutOfRangeException(nameof(index));
            if (array.Length - index < Count) throw new System.ArgumentException();
            if (Count > 0)
            {
                for (int i = 0; i < Count; i++)
                    array[index + i] = ElementAt(i);
            }
        }

        public bool Contains(T item) => IndexOf(item) != -1;

        public int IndexOf(T item)
        {
            for (var i = Offset; i < Offset + Count; i++)
            {
                if (Items[i].Equals(item))
                {
                    return i;
                }
            }

            return -1;
        }

        private T ElementAt(int index)
        {
            if (Count > 0)
            {
                return Items[Offset + index];
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public ListSegmentEnumerator GetEnumerator() => new ListSegmentEnumerator(this);

        #region IEnumerable<T> interface
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region ICollection<T> interface
        bool ICollection<T>.IsReadOnly => true;

        void ICollection<T>.Add(T item) => throw new NotImplementedException();
        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
        void ICollection<T>.Clear() => throw new NotImplementedException();
        #endregion

        #region IList<T> interface
        void IList<T>.Insert(int index, T item) => throw new NotImplementedException();
        void IList<T>.RemoveAt(int index) => throw new NotImplementedException();
        T IList<T>.this[int index]
        {
            get => ElementAt(index);
            set => throw new NotImplementedException();
        }
        #endregion

        public struct ListSegmentEnumerator : IEnumerator<T>
        {
            private readonly IList<T> items;
            private readonly int start;
            private readonly int end;
            private int current;

            public ListSegmentEnumerator(ListSegment<T> segment)
            {
                items = segment.Items;
                start = segment.Offset;
                end = start + segment.Count;
                current = start - 1;
            }

            public bool MoveNext()
            {
                if (current < end)
                {
                    current++;

                    return current < end;
                }
                return false;
            }

            public T Current => items[current];
            object IEnumerator.Current => Current;

            void IEnumerator.Reset() => current = start - 1;
            void IDisposable.Dispose() { }
        }
    }
}