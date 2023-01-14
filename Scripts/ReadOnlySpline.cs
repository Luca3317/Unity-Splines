using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    [System.Serializable]
    public class ReadOnlySpline : SplineBase
    {
        public ReadOnlySpline(ISplineGenerator generator, bool cache, params SplinePoint[] points) : base(generator, cache, points)
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) : base(generator, cache, points)
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) : base(generator, cache, points)
        { }
    }

    [System.Serializable]
    public class ReadOnlySpline<T> : ReadOnlySpline where T : new()
    {
        public T PointData(int pointIndex) => _pointData.Item(pointIndex);
        public ListSegment<T> SegmentData(int segmentIndex) => _pointData.Segment(segmentIndex);

        public ReadOnlySpline(ISplineGenerator generator, bool cache, params SplinePoint[] points) : base(generator, cache, points)
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) : base(generator, cache, points)
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) : base(generator, cache, points)
        { }

        protected override void InitSpline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points)
        {
            List<Vector3> pointsPositions = new List<Vector3>();
            List<float> pointsNormalAngles = new List<float>();
            List<T> pointsData = new List<T>();

            foreach (SplinePoint pointStruct in points)
            {
                pointsPositions.Add(pointStruct.Position);
                pointsNormalAngles.Add(pointStruct.NormalAngle);
                pointsData.Add(new T());
            }

            _pointPositions = new SegmentedCollection<Vector3>(generator.SegmentSize, generator.SlideSize, pointsPositions);
            _pointNormals = new SegmentedCollection<float>(generator.SegmentSize, generator.SlideSize, pointsNormalAngles);
            _pointData = new SegmentedCollection<T>(generator.SegmentSize, generator.SlideSize, pointsData);
            _generator = generator;

            if (cache)
            {
                _cacher = new SplineCacher();
                for (int i = 0; i < SegmentCount; i++) _cacher.Add();
            }

            _space = SplineSpace.XYZ;
        }

        [SerializeField] protected SegmentedCollection<T> _pointData;

        protected override void AddRange(ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();
            List<T> data = new List<T>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
                data.Add(new T());
            }

            _pointPositions.AddRange(positions);
            _pointNormals.AddRange(normalAngles);
            _pointData.AddRange(data);

            // TODO this will throw an error if actually adding more than one segment
            if (_cacher != null)
            {
                _cacher.Add(SegmentCount - 1);
            }
        }

        protected override void Add(ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();
            List<T> data = new List<T>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
                data.Add(new T());
            }

            _pointPositions.Add(positions);
            _pointNormals.Add(normalAngles);
            _pointData.Add(data);

            if (_cacher != null)
            {
                _cacher.Add(SegmentCount - 1);
            }
        }

        protected override void Insert(int i, ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();
            List<T> data = new List<T>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
                data.Add(new T());
            }

            _pointPositions.InsertAtSegment(i, positions);
            _pointNormals.InsertAtSegment(i, normalAngles);
            _pointData.InsertAtSegment(i, data);

            if (_cacher != null)
            {
                _cacher.Add(i);
            }
        }

        protected override void InsertRange(int i, ICollection<SplinePoint> points)
        {
            List<Vector3> positions = new List<Vector3>();
            List<float> normalAngles = new List<float>();
            List<T> data = new List<T>();

            foreach (SplinePoint point in points)
            {
                positions.Add(point.Position);
                normalAngles.Add(point.NormalAngle);
                data.Add(new T());
            }

            _pointPositions.InsertRangeAtSegment(i, positions);
            _pointNormals.InsertRangeAtSegment(i, normalAngles);
            _pointData.InsertRangeAtSegment(i, data);

            if (_cacher != null)
            {
                _cacher.Add(i);
            }
        }

        protected override void Remove(int i)
        {
            if (_cacher != null)
            {
                _cacher.Remove(i);
            }
            _pointPositions.RemoveAtSegment(i);
            _pointNormals.RemoveAtSegment(i);
            _pointData.RemoveAtSegment(i);
        }
    }
}