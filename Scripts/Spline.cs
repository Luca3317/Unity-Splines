using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    /*
     * Representation of a full spline. Primarily consists of a segmented point-collection (SplinePoints) and a SplineGenerator.
     * 
     * Maybe inherit from SplinePoints instead of wrapper methods?
     */
    [System.Serializable]
    public class Spline
    {
        #region SplinePoints Property Wrappers
        public int SegmentSize => _generator.SegmentSize;
        public int SlideSize => _generator.SlideSize;
       
        /// <summary>
        /// Returns the amount of points in the collection.
        /// </summary>
        public int PointCount => _points.PointCount;
        /// <summary>
        /// Returns the point at index i of the collection.
        /// </summary>
        /// <param name="i">The index of the point to return.</param>
        /// <returns></returns>
        public Vector3 Point(int i) => _points.Point(i);
        /// <summary>
        /// Returns all points in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<Vector3> Points => _points.Points;

        /// <summary>
        /// Return the amount of segments in the collection.
        /// </summary>
        public int SegmentCount => _points.SegmentCount;
        /// <summary>
        /// Returns the i-th segment in the collection.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IList<Vector3> Segment(int i) => _points.Segment(i); // TODO test this
        /// <summary>
        /// Returns all segments in the collection as an IEnumerable.
        /// </summary>
        public IEnumerable<IList<Vector3>> Segments => _points.Segments;
        #endregion

        public Spline(ISplineGenerator generator, params Vector3[] points) => Init(generator, points);
        public Spline(ISplineGenerator generator, IEnumerable<Vector3> points) => Init(generator, points);
        public Spline(ISplineGenerator generator, SplinePoints points) => Init(generator, points.Points);

        public Vector3 ValueAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);
            return _generator.Evaluate(segmentT, _points.Segment(segmentIndex));
        }

        public Vector3 TangentAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineHelper.PercentageToSegmentPercentage(t);
            return _generator.EvaluateDerivative(segmentT, 1, _points.Segment(segmentIndex));
        }

        public void SetGenerator(ISplineGenerator generator)
        {
            if (generator == _generator) return;
            _generator = generator;
            _points.SetSegmentSizes(_generator.SegmentSize, _generator.SlideSize);
        }

        #region SplinePoints Method Wrappers
        /// <summary>
        /// Appends a new segment to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void AddSegment(params Vector3[] points) => _points.Insert(_points.PointCount, points);
        /// <summary>
        /// Appends a new segment to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void AddSegment(ICollection<Vector3> points) => _points.Insert(_points.PointCount, points);
        /// <summary>
        /// Inserts a new segment into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegment(int segmentIndex, params Vector3[] points) => _points.Insert(SplineHelper.SegmentToPointIndex(segmentIndex, _generator.SegmentSize, _generator.SlideSize), points);
        /// <summary>
        /// Inserts a new segment into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegment(int segmentIndex, ICollection<Vector3> points) => _points.Insert(SplineHelper.SegmentToPointIndex(segmentIndex, _generator.SegmentSize, _generator.SlideSize), points);
        /// <summary>
        /// Inserts a new segment into the collection. Does not necessarily preserve current segments.
        /// </summary>
        /// <param name="pointIndex">The index of the new segment, as point index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegmentAtPoint(int pointIndex, params Vector3[] points) => _points.Insert(pointIndex, points);
        /// <summary>
        /// Inserts a new segment into the collection. Does not necessarily preserve current segments.
        /// </summary>
        /// <param name="pointIndex">The index of the new segment, as point index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegmentAtPoint(int pointIndex, ICollection<Vector3> points) => _points.Insert(pointIndex, points);

        /// <summary>
        /// Appends new segments to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void AddSegments(params Vector3[] points) => _points.InsertRange(_points.PointCount, points);
        /// <summary>
        /// Appends new segments to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void AddSegments(ICollection<Vector3> points) => _points.InsertRange(_points.PointCount, points);
        /// <summary>
        /// Inserts new segments into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the first of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void InsertSegments(int segmentIndex, params Vector3[] points) => _points.InsertRange(SplineHelper.SegmentToPointIndex(segmentIndex, _generator.SegmentSize, _generator.SlideSize), points);
        /// <summary>
        /// Inserts new segments into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the first of the new segments, as segment index.</param>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void InsertSegments(int segmentIndex, ICollection<Vector3> points) => _points.InsertRange(SplineHelper.SegmentToPointIndex(segmentIndex, _generator.SegmentSize, _generator.SlideSize), points);
        /// <summary>
        /// Inserts new segments into the collection. Does not necessarily preserve current segments.
        /// </summary>
        /// <param name="pointIndex">The index of the first of the new segments, as point index.</param>
        /// <param name="points">The points of the new segments. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegmentsAtPoint(int pointIndex, params Vector3[] points) => _points.InsertRange(pointIndex, points);
        /// <summary>
        /// Inserts new segments into the collection. Does not necessarily preserve current segments.
        /// </summary>
        /// <param name="pointIndex">The index of the first of the new segments, as point index.</param>
        /// <param name="points">The points of the new segments. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegmentsAtPoint(int pointIndex, ICollection<Vector3> points) => _points.InsertRange(pointIndex, points);

        /// <summary>
        /// Deletes the last segment of the collection. Preserves all other current segments.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        public void DeleteLastSegment() => _points.Remove(_points.PointCount - _points.SlideSize);
        /// <summary>
        /// Deletes the si-th segment of the collection. Preserves all other current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the segment to remove, as segmentIndex.</param>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        public void DeleteSegment(int si) => _points.Remove(SplineHelper.SegmentToPointIndex(si, _generator.SegmentSize, _generator.SlideSize));
        /// <summary>
        /// Interpretes the points pi to pi + slideSize as segment, and deletes it from the collection. 
        /// </summary>
        /// <param name="pi">The index of the segment to remove, as pointIndex.</param>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        public void DeleteSegmentAtPoint(int pi) => _points.Remove(pi);
        #endregion

        [SerializeField] private SplinePoints _points;
        [SerializeField] private ISplineGenerator _generator; 

        protected void Init(ISplineGenerator generator, IEnumerable<Vector3> points)
        {
            _generator = generator;
            _points = new SplinePoints(_generator.SegmentSize, _generator.SlideSize, points);
        }
    }
}