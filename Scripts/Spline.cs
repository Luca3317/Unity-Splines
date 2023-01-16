using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnitySplines
{
    [System.Serializable]
    public class Spline : SplineBase
    {
        public Spline(ISplineGenerator generator, bool cache, params Vector3[] points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public Spline(ISplineGenerator generator, bool cache, IEnumerable<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public Spline(ISplineGenerator generator, bool cache, SegmentedCollection<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points.Items))
        { }
        public Spline(ISplineGenerator generator, bool cache, params SplinePoint[] points) : base(generator, cache, points)
        { }
        public Spline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) : base(generator, cache, points)
        { }
        public Spline(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) : base(generator, cache, points)
        { }

        public virtual void SetGenerator(ISplineGenerator generator)
        {
            if (generator == _generator) return;
            if (PointCount < generator.SegmentSize) return;

            _generator = generator;
            _pointPositions.SetSegmentSizes(_generator.SegmentSize, _generator.SlideSize);
            _pointNormals.SetSegmentSizes(_generator.SegmentSize, _generator.SlideSize);
            if (_cacher != null)
            {
                _cacher.SetSize(SegmentCount);
                ClearCache();
            }
        }

        public void SetSpace(SplineSpace newSpace)
        {
            if (_space == newSpace) return;

            for (int i = 0; i < PointCount; i++)
                _pointPositions.SetItem(i, SplineUtility.ConvertToSpace(_pointPositions.Item(i), _space, newSpace));

            ClearCache();
            _space = newSpace;
        }

        public void SetNormalAngleOffset(float newNormalAngleOffset)
        {
            _normalAngleOffset = newNormalAngleOffset % 360;
        }

        public void SetPoint(int pointIndex, SplinePoint newPoint)
        {
            SetPointNormalAngle(pointIndex, newPoint.NormalAngle);
            SetPointPosition(pointIndex, newPoint.Position);
        }

        public void SetPointPosition(int pointIndex, Vector3 newPosition)
        {
            if (_space == SplineSpace.XZ) newPosition.y = _posRotScale.position.y;
            else if (_space == SplineSpace.XY) newPosition.z = _posRotScale.position.z;

            _pointPositions.SetItem(pointIndex, newPosition);

            if (_cacher != null)
            {
                IEnumerable<int> segmentIndeces = _pointPositions.SegmentIndecesOf(pointIndex);
                foreach (int segmentIndex in segmentIndeces) _cacher[segmentIndex].Clear();
                _cacher.Clear();
            }
        }

        public void SetPointNormalAngle(int pointIndex, float newNormalAngle)
        {
            _pointNormals.SetItem(pointIndex, newNormalAngle);
        }

        public void SetPosition(Vector3 newPosition)
        {
            if (_posRotScale.position == newPosition) return;

            PosRotScale newPosRotScale = _posRotScale;
            newPosRotScale.position = newPosition;

            for (int i = 0; i < _pointPositions.ItemCount; i++)
            {

                Vector3 pos = _pointPositions.Item(i);
                pos = SplineUtility.ApplyPosition(pos, -_posRotScale.position);
                _pointPositions.SetItem(i, SplineUtility.ApplyPosition(pos, newPosition));

            }

            _posRotScale = newPosRotScale;
            ClearCache();
        }

        public void SetRotation(Quaternion newRotation)
        {
            if (_posRotScale.rotation == newRotation) return;
            PosRotScale newPosRotScale = _posRotScale;
            newPosRotScale.rotation = newRotation;

            for (int i = 0; i < _pointPositions.ItemCount; i++)
            {

                // get current pos
                Vector3 pos = _pointPositions.Item(i);
                // Unapply old rotation
                pos = SplineUtility.ApplyRotation(pos, _posRotScale.position, Quaternion.Inverse(_posRotScale.rotation));
                // Apply new rotation
                _pointPositions.SetItem(i, SplineUtility.ApplyRotation(pos, _posRotScale.position, newRotation));

                // Todo combine those two last steps, if sensible
            }

            _posRotScale = newPosRotScale;
            ClearCache();
        }

        public void SetScale(Vector3 newScale)
        {
            if (_posRotScale.scale == newScale) return;

            PosRotScale newPosRotScale = _posRotScale;
            newPosRotScale.scale = newScale;

            Vector3 pivot = GetBounds().center;


            for (int i = 0; i < _pointPositions.ItemCount; i++)
            {
                // get current pos
                Vector3 pos = _pointPositions.Item(i);
                // Unapply old scale
                // TODO prevent division by 0
                pos = SplineUtility.ApplyScale(pos, pivot, new Vector3(1f / _posRotScale.scale.x, 1f / _posRotScale.scale.y, 1f / _posRotScale.scale.z));
                // Apply new rotation
                _pointPositions.SetItem(i, SplineUtility.ApplyScale(pos, pivot, newScale));
            }

            _posRotScale = newPosRotScale;
            ClearCache();
        }

        public void AlignPosition(Vector3 alignTo)
        {
            _posRotScale.position = alignTo;
        }

        public void AlignRotation(Quaternion alignTo)
        {
            _posRotScale.rotation = alignTo;
        }

        public void AlignScale(Vector3 alignTo)
        {
            _posRotScale.scale = alignTo;
        }

        public void SplitAt(float t)
        {
            (int segmentIndex, float segmentT) = SplineUtility.PercentageToSegmentPercentage(t);

            IList<Vector3> newSegments = _generator.SplitSegment(segmentT, _pointPositions.Segment(segmentIndex));
            newSegments.RemoveAt(newSegments.Count - 1);
            InsertRange(segmentIndex, newSegments);
            Remove(segmentIndex + 2);
        }

        #region SplinePoints Method Wrappers
        /// <summary>
        /// Appends a new segment to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void AddSegment(params SplinePoint[] points) => Add(points);
        /// <summary>
        /// Appends a new segment to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void AddSegment(ICollection<SplinePoint> points) => Add(points);
        /// <summary>
        /// Inserts a new segment into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegment(int segmentIndex, params SplinePoint[] points) => Insert(segmentIndex, points);
        /// <summary>
        /// Inserts a new segment into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segment. Count must be equal to slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is unequal to slideSize.</exception>
        public void InsertSegment(int segmentIndex, ICollection<SplinePoint> points) => Insert(segmentIndex, points);

        /// <summary>
        /// Appends new segments to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void AddSegments(params SplinePoint[] points) => AddRange(points);
        /// Appends new segments to the end of the collection.
        /// </summary>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void AddSegments(ICollection<SplinePoint> points) => AddRange(points);
        /// <summary>
        /// Inserts new segments into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the first of the new segment, as segment index.</param>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void InsertSegments(int segmentIndex, params SplinePoint[] points) => InsertRange(segmentIndex, points);
        /// <summary>
        /// Inserts new segments into the collection. Preserves current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the first of the new segments, as segment index.</param>
        /// <param name="points">The points of the new segments. Count must be a multiple of slideSize.</param>
        /// <exception cref="ArgumentException">Thrown if count of points is not a multiple of slideSize.</exception>
        public void InsertSegments(int segmentIndex, ICollection<SplinePoint> points) => InsertRange(segmentIndex, points);

        /// <summary>
        /// Deletes the last segment of the collection. Preserves all other current segments.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        public void DeleteLastSegment() => Remove(SegmentCount - 1);
        /// <summary>
        /// Deletes the si-th segment of the collection. Preserves all other current segments.
        /// </summary>
        /// <param name="segmentIndex">The index of the segment to remove, as segmentIndex.</param>
        /// <exception cref="InvalidOperationException">Thrown if the collection does not contain at least two segments. It must always contain at least one full segment.</exception>
        public void DeleteSegment(int segmentIndex) => Remove(segmentIndex);
        #endregion

        protected void AddRange(ICollection<Vector3> points) => AddRange(SplineUtility.VectorsToSplinePoints(points));
        protected virtual void AddRange(ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.AddRange(positions);
            _pointNormals.AddRange(normalAngles);

            // TODO this will throw an error if actually adding more than one segment
            if (_cacher != null)
            {
                for (int i = 0; i < points.Count / SlideSize; i++)
                    _cacher.Insert(SegmentCount - 1);
            }
        }

        protected void Add(ICollection<Vector3> points) => Add(SplineUtility.VectorsToSplinePoints(points));
        protected virtual void Add(ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.Add(positions);
            _pointNormals.Add(normalAngles);

            if (_cacher != null)
            {
                _cacher.Insert(SegmentCount - 1);
            }
        }

        protected void Insert(int i, ICollection<Vector3> points) => Insert(i, SplineUtility.VectorsToSplinePoints(points));
        protected virtual void Insert(int i, ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.InsertAtSegment(i, positions);
            _pointNormals.InsertAtSegment(i, normalAngles);

            if (_cacher != null)
            {
                _cacher.Insert(i);
            }
        }

        protected void InsertRange(int i, ICollection<Vector3> points) => InsertRange(i, SplineUtility.VectorsToSplinePoints(points));
        protected virtual void InsertRange(int i, ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
            }

            _pointPositions.InsertRangeAtSegment(i, positions);
            _pointNormals.InsertRangeAtSegment(i, normalAngles);

            if (_cacher != null)
            {
                for (int j = 0; j < points.Count / SlideSize; j++)
                    _cacher.Insert(i);
            }
        }

        protected virtual void Remove(int i)
        {
            if (_cacher != null)
            {
                _cacher.RemoveAt(i);
            }
            _pointPositions.RemoveAtSegment(i);
            _pointNormals.RemoveAtSegment(i);
        }

    }

    [System.Serializable]
    public class Spline<T> : Spline where T : new()
    {
        public T PointData(int pointIndex) => _pointData.Item(pointIndex);
        public ListSegment<T> SegmentData(int segmentIndex) => _pointData.Segment(segmentIndex);

        public Spline(ISplineGenerator generator, bool cache, params Vector3[] points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public Spline(ISplineGenerator generator, bool cache, IEnumerable<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public Spline(ISplineGenerator generator, bool cache, SegmentedCollection<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points.Items))
        { }
        public Spline(ISplineGenerator generator, bool cache, params SplinePoint[] points) : base(generator, cache, points)
        { }
        public Spline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) : base(generator, cache, points)
        { }
        public Spline(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) : base(generator, cache, points)
        { }

        public override void SetGenerator(ISplineGenerator generator)
        {
            if (generator == _generator) return;
            if (PointCount < generator.SegmentSize) return;

            _generator = generator;
            _pointPositions.SetSegmentSizes(generator.SegmentSize, generator.SlideSize);
            _pointNormals.SetSegmentSizes(generator.SegmentSize, generator.SlideSize);
            _pointData.SetSegmentSizes(generator.SegmentSize, generator.SlideSize);
            if (_cacher != null)
            {
                _cacher.SetSize(SegmentCount);
                ClearCache();
            }
        }

        [SerializeField] protected SegmentedCollection<T> _pointData;

        protected override void InitSpline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points)
        {
            base.InitSpline(generator, cache, points);

            List<T> pointsData = new List<T>();
            for (int i = 0; i < _pointPositions.ItemCount; i++)
                pointsData.Add(new T());

            _pointData = new SegmentedCollection<T>(generator.SegmentSize, generator.SlideSize, pointsData);
        }

        protected override void AddRange(ICollection<SplinePoint> points)
        {
            base.AddRange(points);
            MatchData();
        }

        protected override void Add(ICollection<SplinePoint> points)
        {
            base.Add(points);
            MatchData();
        }

        protected override void Insert(int i, ICollection<SplinePoint> points)
        {
            base.Insert(i, points);
            MatchData();
        }

        protected override void InsertRange(int i, ICollection<SplinePoint> points)
        {
            base.InsertRange(i, points);
            MatchData();
        }

        protected override void Remove(int i)
        {
            base.Remove(i);
            _pointData.RemoveAtSegment(i);
        }

        private void MatchData()
        {
            if (_pointData.ItemCount == _pointPositions.ItemCount) return;

            List<T> data = new List<T>();
            for (int i = _pointData.ItemCount; i < _pointPositions.ItemCount; i++)
                data.Add(new T());

            _pointData.AddRange(data);
        }
    }
}