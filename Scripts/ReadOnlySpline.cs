using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    [System.Serializable]
    public class ReadOnlySpline : SplineBase
    {
        public ReadOnlySpline(ISplineGenerator generator, bool cache, params Vector3[] points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, IEnumerable<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, SegmentedCollection<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points.Items))
        { }
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

        public ReadOnlySpline(ISplineGenerator generator, bool cache, params Vector3[] points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, IEnumerable<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, SegmentedCollection<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points.Items))
        { }
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
    }
}