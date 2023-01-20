using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    /*
    * TODO:
    * Maybe alternatively make SegmentedCollection that does not enforce any restraints on the amount of points, and just offers helper methods for segmenting a backing list and inserting segments.
    * 
    * 
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
    public interface ISegmentedCollection<T> : IEnumerable<T>, IEnumerable
    {
        public int SegmentSize { get; }
        public int SlideSize { get; }
        public int SegmentCount { get; }
        public int Count { get; }
        public ListSegment<T> Segment(int segmentIndex);

        public void AddSegment(IEnumerable<T> items);
        public void AddSegmentRange(IEnumerable<T> items);
        public void InsertSegment(int segmentIndex, IEnumerable<T> items);
        public void InsertSegmentRange(int segmentIndex, IEnumerable<T> items);
        public void RemoveSegmentAt(int segmentIndex);
        public void RemoveSegmentRange(int segmentIndex, int count);
        public void SetItem(int pointIndex, T item);
        public bool Contains(T item);
        public void Clear();
        public int IndexOf(T item);
        public IEnumerable<int> SegmentIndecesOf(int pointIndex);
        public IEnumerable<int> SegmentIndecesOf(T item);
        public void SetSegmentSizes(int segmentSize, int slideSize);
        public void CopyTo(T[] array, int index);

        protected const string _atLeastOneSegmentErrorMsg = "A spline must always consist of at least one base segment";
        protected const string _segmentSizeAtLeast1ErrorMsg = "The segment size and slide size of a segmented collection must be at least 1";
        protected const string _segmentSizeSmallerThanSlideErrorMsg = "The segmentSize has to be bigger or equal to the slideSize";
        protected const string _tooFewItemsToCreateErrorMsg = "The amount of passed in items ({0}) is not sufficient to create a segmented collection with segmentSize {1}";
        protected const string _tooFewItemsToConvertErrorMsg = "The amount of items ({0}) is not sufficient to set segmentSize to {1}";
        protected const string _incompatibleAmountOfItemsErrorMsg = "The amount of passed in items ({0}) is incompatible with segmentSize {1} and slideSize {2}";
        protected const string _incompatibleAmountOfNewItemsErrorMsg = "Invalid amount of items to form one new segment. Expected amount: {0}";
    }
}