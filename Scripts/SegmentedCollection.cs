using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    /*
     * Handles storage and segmentation of a collection.
     * 
     * Invariant: always contains at least one segment / segmentSize-many items (called base segment).
     *      1. Create a list with either:
     *          exactly segmentSize many items (so: just the base segment) or
     *          segmentSize many items + slideSize * x many items (so: base segment + x following segments)
     *      2. After that you may
     *          2.1 Add segments
     *              This cant harm the integrity of the base segment; 
     *              however, if insertion happens at 0, it will be replaced by the new items (rather, the base segment will become the second segment)
     *          2.2 Remove segments
     *              Error if only base segment remains
     *              Otherwise, this cant harm the integrity of the base segment either;
     *          2.3 Change segment / slide sizes
     *              Should only happen if the generator is changed to another generator which requires different segment sizes
     *              
     *      4. You CANNOT add / remove indvidual items; would make it impossible to keep integrity of segment structure
    */
    [System.Serializable]
    public class SegmentedCollection<T>
    {
        /// <summary>
        /// The amount of items that constitutes one full segment in the collection (i.e. the amount of items returned when requesting a segment). 
        /// </summary>
        public int SegmentSize => _segmentSize;
        /// <summary>
        /// The amount of items that will create a new segment in the collection (i.e. the amount of items that must be passed in when adding a segment).
        /// </summary>
        public int SlideSize => _slideSize;

        /// <summary>
        /// Returns the amount of items in the collection.
        /// </summary>
        public int ItemCount => _items.Count;
        /// <summary>
        /// Return the amount of segments in the collection.
        /// </summary>
        public int SegmentCount => _items.Count >= _segmentSize ? (_items.Count - _segmentSize) / _slideSize + 1 : 0;

        /// <summary>
        /// Returns the item at index i of the collection.
        /// </summary>
        /// <param name="i">The index of the item to return.</param>
        /// <returns></returns>
        public T Item(int i) => _items[i];
        /// <summary>
        /// Returns all items in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<T> Items => _items.AsReadOnly();

        /// <summary>
        /// Returns the i-th segment in the collection.
        /// </summary>
        /// <param name="segmentIndex"></param>
        /// <returns></returns>
        public ListSegment<T> Segment(int segmentIndex)
        {
            return new ListSegment<T>(_items, MathUtility.SegmentToPointIndex(segmentIndex, _segmentSize, _slideSize), _segmentSize);
        }
        /// <summary>
        /// Returns all segments in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<ListSegment<T>> Segments { get { for (int i = 0; i < SegmentCount; i++) yield return Segment(i); } }

        [System.Runtime.CompilerServices.IndexerName("MyItem")]
        public ListSegment<T> this[int segmentIndex] => Segment(segmentIndex);
        [System.Runtime.CompilerServices.IndexerName("MyItem")]
        public T this[int segmentIndex, int pointIndex] => _items[MathUtility.SegmentToPointIndex(segmentIndex, _segmentSize, _slideSize) + pointIndex];

        public SegmentedCollection(int segmentSize, int slideSize, IEnumerable<T> items) => Init(segmentSize, slideSize, items);
        public SegmentedCollection(int segmentSize, int slideSize, params T[] items) => Init(segmentSize, slideSize, items);
        public SegmentedCollection(int segmentSize, int slideSize, SegmentedCollection<T> items) => Init(segmentSize, slideSize, items._items.GetRange(0, items._items.Count - (items._items.Count - segmentSize) % slideSize));

        public void SetItem(int itemIndex, T newItem)
        {
            _items[itemIndex] = newItem;
        }

        public void Add(ICollection<T> items)
        {
            if (items.Count != _slideSize) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewItemsErrorMsg, _slideSize));
            _items.AddRange(items);
        }

        public void AddRange(ICollection<T> items)
        {
            if (items.Count % _slideSize != 0) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewItemsErrorMsg, _slideSize));
            _items.AddRange(items);
        }

        /// <summary>
        /// Inserts a new segment into the collection at index.
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <param name="items"></param>
        public void InsertAtPoint(int pointIndex, ICollection<T> items)
        {
            if (items.Count != _slideSize) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewItemsErrorMsg, _slideSize));
            _items.InsertRange(pointIndex, items);
        }

        /// <summary>
        /// Inserts multiple new segments into the collection at index.
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <param name="items"></param>
        public void InsertRangeAtPoint(int pointIndex, ICollection<T> items)
        {
            if (items.Count % _slideSize != 0) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewItemsErrorMsg, _slideSize));
            _items.InsertRange(pointIndex, items);
        }

        /// <summary>
        /// Removes the segment at segmentIndex from the collection.
        /// </summary>
        /// <param name="segementIndex"></param>
        /// <param name="items"></param>
        public void RemoveAtPoint(int pointIndex)
        {
            if (SegmentCount <= 1) throw new System.InvalidOperationException(string.Format(_atLeastOneSegmentErrorMsg));
            _items.RemoveRange(pointIndex, _slideSize);
        }

        public void InsertAtSegment(int segmentIndex, ICollection<T> items)
        {
            if (items.Count != _slideSize) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewItemsErrorMsg, _slideSize));
            _items.InsertRange(segmentIndex * _slideSize, items);
        }

        public void InsertRangeAtSegment(int segmentIndex, ICollection<T> items)
        {
            if (items.Count % _slideSize != 0) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewItemsErrorMsg, _slideSize));
            _items.InsertRange(segmentIndex * _slideSize, items);
        }

        public void RemoveAtSegment(int segmentIndex)
        {
            if (SegmentCount <= 1) throw new System.InvalidOperationException(string.Format(_atLeastOneSegmentErrorMsg));
            _items.RemoveRange(segmentIndex * _slideSize, _slideSize);
        }

        public int IndexOf(T item) => _items.IndexOf(item);
        public IEnumerable<int> SegmentIndecesOf(int itemIndex) => SegmentIndecesOf(_items[itemIndex]);
        public IEnumerable<int> SegmentIndecesOf(T item)
        {
            IList<int> indeces = MathUtility.PointToSegmentIndeces(_items.IndexOf(item), _segmentSize, _slideSize);
            indeces.Remove(SegmentCount);
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
            if (_items.Count < segmentSize) throw new System.ArgumentException(string.Format(_tooFewItemsToConvertErrorMsg, _items.Count, segmentSize));

            _items.RemoveRange(_items.Count - (_items.Count - segmentSize) % slideSize, (_items.Count - segmentSize) % slideSize);
            _segmentSize = segmentSize;
            _slideSize = slideSize;
        }

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

        private void Init(int segmentSize, int slideSize, IEnumerable<T> items)
        {
            _items = new List<T>(items);
            if (segmentSize < 1 || slideSize < 1) throw new System.ArgumentException(string.Format(_segmentSizeAtLeast1ErrorMsg));
            // TODO: This constraint might not be necessary
            if (segmentSize < slideSize) throw new System.ArgumentException(string.Format(_segmentSizeSmallerThanSlideErrorMsg));
            if (_items.Count < segmentSize) throw new System.ArgumentException(string.Format(_tooFewItemsToCreateErrorMsg, _items.Count, segmentSize));
            if ((_items.Count - segmentSize) % slideSize != 0) throw new System.ArgumentException(string.Format(_incompatibleAmountOfItemsErrorMsg, _items.Count, segmentSize, slideSize));

            _segmentSize = segmentSize;
            _slideSize = slideSize;
        }
    }
}
