using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    /*
     * Handles storage and structure of the point collection, primarily segment logic.
     * 
     * Invariant: always contains at least one segment / segmentSize-many points (called base segment).
     *      1. Create a list with either:
     *          exactly segmentSize many points (so: just the base segment) or
     *          segmentSize many points + slideSize * x many points (so: base segment + x following segments)
     *      2. After that you may
     *          2.1 Add segments
     *              This cant harm the integrity of the base segment; 
     *              however, if insertion happens at 0, it will be replaced by the new points (rather, the base segment will become the second segment)
     *          2.2 Remove segments
     *              Error if only base segment remains
     *              Otherwise, this cant harm the integrity of the base segment either;
     *          2.3 Change segment / slide sizes
     *              Should only happen if the generator is changes to another generator which requires different segment sizes
     *              TODO implement case for when new sizes incompatible with current points          
     *              
     *      4. You CANNOT add / remove indvidual points; would make it impossible to keep integrity of segment structure
    */
    [System.Serializable]
    public class SplinePoints
    {
        /// <summary>
        /// The amount of points that constitutes one full segment in the collection (i.e. the amount of points returned when requesting a segment). 
        /// </summary>
        public int SegmentSize => _segmentSize;
        /// <summary>
        /// The amount of points that will create a new segment in the collection (i.e. the amount of points that must be passed in when adding a segment).
        /// </summary>
        public int SlideSize => _slideSize;
        
        /// <summary>
        /// Returns the amount of points in the collection.
        /// </summary>
        public int PointCount => _points.Count;
        /// <summary>
        /// Returns the point at index i of the collection.
        /// </summary>
        /// <param name="i">The index of the point to return.</param>
        /// <returns></returns>
        public Vector3 Point(int i) => _points[i];
        /// <summary>
        /// Returns all points in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<Vector3> Points => _points.AsReadOnly();

        /// <summary>
        /// Return the amount of segments in the collection.
        /// </summary>
        public int SegmentCount => _points.Count >= _segmentSize ? (_points.Count - _segmentSize) / _slideSize + 1 : 0;
        /// <summary>
        /// Returns the i-th segment in the collection.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IList<Vector3> Segment(int i) => i == 0 ? _points.GetRange(0, _segmentSize) : _points.GetRange(_segmentSize - 1 + (i - 1) * _slideSize, _segmentSize); // TODO test this
        /// <summary>
        /// Returns all segments in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<IList<Vector3>> Segments { get { for (int i = 0; i < SegmentCount; i++) yield return Segment(i); } }

        public SplinePoints(int segmentSize, int slideSize, IEnumerable<Vector3> points) => Init(segmentSize, slideSize, points);
        public SplinePoints(int segmentSize, int slideSize, params Vector3[] points) => Init(segmentSize, slideSize, points);
        public SplinePoints(int segmentSize, int slideSize, SplinePoints points) => Init(segmentSize, slideSize, points._points.GetRange(0, points._points.Count - (points._points.Count - segmentSize) % slideSize));

        public void Insert(int index, ICollection<Vector3> points)
        {
            if (points.Count != _slideSize) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewPointsErrorMsg, _slideSize));
            _points.InsertRange(index, points);
        }

        public void InsertRange(int index, ICollection<Vector3> points)
        {
            if (points.Count % _slideSize != 0) throw new System.ArgumentException(string.Format(_incompatibleAmountOfNewPointsErrorMsg, _slideSize));
            _points.InsertRange(index, points);
        }

        public void Remove(int index)
        {
            if (SegmentCount <= 1) throw new System.InvalidOperationException(string.Format(_atLeastOneSegmentErrorMsg));
            _points.RemoveRange(index, _slideSize);
        }

        /// <summary>
        /// Sets the segment sizes.
        /// </summary>
        /// <param name="segmentSize">The amount of points that constitue a full segment.</param>
        /// <param name="slideSize">The amount of points that constitute a new segment.</param>
        /// <exception cref="ArgumentException">Thrown if either segmentSize or slideSize is lower than 1.</exception>
        public void SetSegmentSizes(int segmentSize, int slideSize)
        {
            if (segmentSize < 1 || slideSize < 1) throw new System.ArgumentException(_segmentSizeAtLeast1ErrorMsg);
            // TODO: Placeholder code; if the new segmentsize is larger than the amount of points contained in the collection, new points must be added
            for (int i = _points.Count; i < segmentSize; i++) _points.Add(_points[i - 1] + Vector3.one);
            _points.RemoveRange(_points.Count - (_points.Count - segmentSize) % slideSize, (_points.Count - segmentSize) % slideSize);
            _segmentSize = segmentSize;
            _slideSize = slideSize;
        }

        private const string _atLeastOneSegmentErrorMsg = "A spline must always consist of at least one base segment";
        private const string _segmentSizeAtLeast1ErrorMsg = "The segment size and slide size of a spline-point collection must be at least 1";
        private const string _segmentSizeSmallerThanSlideErrorMsg = "The segmentSize has to be bigger or equal to the slideSize";
        private const string _tooFewPointsErrorMsg = "The amount of passed in points ({0}) is not sufficient to create a spline with segmentSize {1}";
        private const string _incompatibleAmountOfPointsErrorMsg = "The amount of passed in points ({0}) is incompatible with segmentSize {1} and slideSize {2}";
        private const string _incompatibleAmountOfNewPointsErrorMsg = "Invalid amount of points to form one new curve segment. Expected amount: {0}";

        [SerializeField] private int _segmentSize;
        [SerializeField] private int _slideSize;
        [SerializeField] private List<Vector3> _points;

        private void Init(int segmentSize, int slideSize, IEnumerable<Vector3> points)
        {
            _points = new List<Vector3>(points);
            if (segmentSize < 1 || slideSize < 1) throw new System.ArgumentException(string.Format(_segmentSizeAtLeast1ErrorMsg));
            // TODO: This constraint might not be necessary
            if (segmentSize < slideSize) throw new System.ArgumentException(string.Format(_segmentSizeSmallerThanSlideErrorMsg));
            if (_points.Count < segmentSize) throw new System.ArgumentException(string.Format(_tooFewPointsErrorMsg, _points.Count, segmentSize));
            if ((_points.Count - segmentSize) % slideSize != 0) throw new System.ArgumentException(string.Format(_incompatibleAmountOfPointsErrorMsg, _points.Count, segmentSize, slideSize));
            
            _segmentSize = segmentSize;
            _slideSize = slideSize;
        }


    }
}
