using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public class SegmentedReadOnlyCollection<T> : ReadOnlyCollection<T>, ISegmentedCollection<T>
    {
        public int SegmentSize => _segmentSize;
        public int SlideSize => _slideSize;
        public int SegmentCount => Items.Count >= _segmentSize ? (Items.Count - _segmentSize) / _slideSize + 1 : 0;

        public ListSegment<T> Segment(int segmentIndex) => new ListSegment<T>(this, segmentIndex * _slideSize, _slideSize);

        public SegmentedReadOnlyCollection(int segmentSize, int slideSize, IList<T> list) : base(list) 
        {
            if (segmentSize < 1 || slideSize < 1) throw new System.ArgumentException(_segmentSizeAtLeast1ErrorMsg);
            if (segmentSize < slideSize) throw new System.ArgumentException(string.Format(_segmentSizeSmallerThanSlideErrorMsg));
            if (Items.Count < segmentSize) throw new System.ArgumentException(string.Format(_tooFewItemsToConvertErrorMsg, Items.Count, segmentSize));

            int segmentIndex = Items.Count - (Items.Count - segmentSize) % slideSize;
            int count = (Items.Count - segmentSize) % slideSize;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < _slideSize; j++)
                    Items.RemoveAt(segmentIndex * _slideSize);
            }

            _segmentSize = segmentSize;
            _slideSize = slideSize;
        }

        public void SetItem(int pointIndex, T item) => Items[pointIndex] = item;

        public IEnumerable<int> SegmentIndecesOf(T item) => SegmentIndecesOf(IndexOf(item));
        public IEnumerable<int> SegmentIndecesOf(int pointIndex)
        {
            IList<int> indeces = MathUtility.PointToSegmentIndeces(pointIndex, _segmentSize, _slideSize);
            for (int i = 0; i < indeces.Count; i++)
                if (indeces[i] >= SegmentCount)
                    indeces.RemoveAt(i--);

            return indeces;
        }

        #region Explicit ISegmentedCollection Implementation
        void ISegmentedCollection<T>.AddSegment(IEnumerable<T> items) => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.AddSegmentRange(IEnumerable<T> items) => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.InsertSegment(int segmentIndex, IEnumerable<T> items) => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.InsertSegmentRange(int segmentIndex, IEnumerable<T> items) => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.RemoveSegmentAt(int segmentIndex) => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.RemoveSegmentRange(int segmentIndex, int count) => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.SetItem(int pointIndex, T item) => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.Clear() => throw new System.NotSupportedException();
        void ISegmentedCollection<T>.SetSegmentSizes(int segmentSize, int slideSize) => throw new System.NotSupportedException();
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
    }
}