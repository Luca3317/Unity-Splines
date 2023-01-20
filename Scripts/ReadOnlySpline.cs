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
        public ReadOnlySpline(ISplineGenerator generator, bool cache, SegmentedCollection<Vector3> points) : base(generator, cache, SplineUtility.VectorsToSplinePoints(points))
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, params SplinePoint[] points) : base(generator, cache, points)
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, IEnumerable<SplinePoint> points) : base(generator, cache, points)
        { }
        public ReadOnlySpline(ISplineGenerator generator, bool cache, SegmentedCollection<SplinePoint> points) : base(generator, cache, points)
        { }
    }
}