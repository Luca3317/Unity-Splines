using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnitySplines
{
    [System.Serializable]
    public class SegmentedCollection<T> : ISegmentedCollection<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
        public int SegmentSize => _segmentSize;
        public int SlideSize => _slideSize;
        public int SegmentCount => _items.Count >= _segmentSize ? (_items.Count - _segmentSize) / _slideSize + 1 : 0;
        public int Count => _items.Count;

        public T this[int i]
        {
            get => _items[i];
            set => _items[i] = value;
        }

        public ListSegment<T> Segment(int segmentIndex) => new ListSegment<T>(_items, MathUtility.SegmentToPointIndex(segmentIndex, _segmentSize, _slideSize), _segmentSize);

        public SegmentedCollection(int segmentSize, int slideSize) 
        {
            _items = new List<T>();
            SetSegmentSizes(segmentSize, slideSize);
        }
        public SegmentedCollection(int segmentSize, int slideSize, IList<T> list)
        {  
            _items =  new List<T>(list);
            SetSegmentSizes(segmentSize, slideSize);
        }

        public void AddSegment(IEnumerable<T> items)
        {
            int count = 0;
            ICollection<T> collection = items as ICollection<T>;
            if (collection != null)
                count = collection.Count;
            else
            {
                using (IEnumerator<T> enumerator = items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        count++;
                } 
            }

            if (count != _slideSize) throw new System.ArgumentException();
            foreach (T item in items) _items.Add(item);
        }

        public void AddSegmentRange(IEnumerable<T> items)
        {
            int count = 0;
            ICollection<T> collection = items as ICollection<T>;
            if (collection != null)
                count = collection.Count;
            else
            {
                using (IEnumerator<T> enumerator = items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        count++;
                }
            }

            if (count % _slideSize != 0) throw new System.ArgumentException();
            foreach (T item in items) _items.Add(item);
        }

        public void InsertSegment(int segmentIndex, IEnumerable<T> items)
        {
            int count = 0;
            ICollection<T> collection = items as ICollection<T>;
            if (collection != null)
                count = collection.Count;
            else
            {
                using (IEnumerator<T> enumerator = items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        count++;
                }
            }

            if (count != _slideSize) throw new System.ArgumentException();
            int i = 0;
            foreach (T item in items) _items.Insert(segmentIndex * _slideSize + i++, item);
        }

        public void InsertSegmentRange(int segmentIndex, IEnumerable<T> items)
        {
            int count = 0;
            ICollection<T> collection = items as ICollection<T>;
            if (collection != null)
                count = collection.Count;
            else
            {
                using (IEnumerator<T> enumerator = items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        count++;
                }
            }

            if (count % _slideSize != 0) throw new System.ArgumentException();
            int i = 0;
            foreach (T item in items) _items.Insert(segmentIndex * _slideSize + i++, item);
        }

        public void RemoveSegmentAt(int segmentIndex)
        {
            for (int i = 0; i < _slideSize; i++)
                _items.RemoveAt(segmentIndex * _slideSize);
        }

        public void RemoveSegmentRange(int segmentIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < _slideSize; j++)
                    _items.RemoveAt(segmentIndex * _slideSize);
            }
        }

        public void SetItem(int pointIndex, T item) => _items[pointIndex] = item;

        public bool Contains(T item) => _items.Contains(item);

        public void Clear() => _items.Clear();

        public int IndexOf(T item) => _items.IndexOf(item);
        public IEnumerable<int> SegmentIndecesOf(T item) => SegmentIndecesOf(_items.IndexOf(item));
        public IEnumerable<int> SegmentIndecesOf(int pointIndex)
        {
            IList<int> indeces = MathUtility.PointToSegmentIndeces(pointIndex, _segmentSize, _slideSize);
            for (int i = 0; i < indeces.Count; i++)
                if (indeces[i] >= SegmentCount)
                    indeces.RemoveAt(i--);

            return indeces;
        }

        /// <summary>
        /// Sets the segment sizes.
        /// </summary>
        /// <param name="segmentSize">The amount of items that constitute a full segment.</param>
        /// <param name="slideSize">The amount of items that constitute a new segment.</param>
        /// <exception cref="ArgumentException">Thrown if either segmentSize or slideSize is lower than 1.</exception>
        public void SetSegmentSizes(int segmentSize, int slideSize)
        {
            if (segmentSize < 1 || slideSize < 1) throw new System.ArgumentException(_segmentSizeAtLeast1ErrorMsg);
            if (segmentSize < slideSize) throw new System.ArgumentException(string.Format(_segmentSizeSmallerThanSlideErrorMsg));
            if (_items.Count < segmentSize) throw new System.ArgumentException(string.Format(_tooFewItemsToConvertErrorMsg, _items.Count, segmentSize));

            int count = (_items.Count - segmentSize) % slideSize;
            for (int i = 0; i < count; i++)
            {
                _items.RemoveAt(_items.Count - 1);
            }

            _segmentSize = segmentSize;
            _slideSize = slideSize;
        }

        public void CopyTo(T[] array, int index)
        {
            _items.CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        #region Explicit Interface implementations

        #region Supported

        #region ICollection
        bool ICollection<T>.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        #endregion

        #region IList
        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
        #endregion

        #endregion

        #region Not Supported

        #region ICollection
        void ICollection<T>.Add(T item)
        {
            throw new System.NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new System.NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        object ICollection.SyncRoot => throw new NotSupportedException();
        #endregion

        #region IList
        object IList.this[int index]
        {
            get => throw new NotSupportedException();
            set => throw new NotImplementedException();
        }
        void IList<T>.Insert(int index, T item)
        {
            throw new System.NotImplementedException();
        }
        void IList<T>.RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }
        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }
        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }
        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }
        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }
        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion

        #endregion

        // Move this logic to spline
        private const string _atLeastOneSegmentErrorMsg = "A spline must always consist of at least one base segment";

        private const string _segmentSizeAtLeast1ErrorMsg = "The segment size and slide size of a segmented collection must be at least 1";
        private const string _segmentSizeSmallerThanSlideErrorMsg = "The segmentSize has to be bigger or equal to the slideSize";
        private const string _tooFewItemsToCreateErrorMsg = "The amount of passed in items ({0}) is not sufficient to create a segmented collection with segmentSize {1}";
        private const string _tooFewItemsToConvertErrorMsg = "The amount of items ({0}) is not sufficient to set segmentSize to {1}";
        private const string _incompatibleAmountOfItemsErrorMsg = "The amount of passed in items ({0}) is incompatible with segmentSize {1} and slideSize {2}";
        private const string _incompatibleAmountOfNewItemsErrorMsg = "Invalid amount of items to form one new segment. Expected amount: {0}";

        [SerializeField] private int _segmentSize;
        [SerializeField] private int _slideSize;
        [SerializeField] private List<T> _items;
    }
}