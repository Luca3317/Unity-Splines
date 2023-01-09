using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public struct SplineExtrema
    {
        Vector3? maxima;
        Vector3? minima;

        public Vector3 Maxima
        {
            get
            {
                if (maxima == null) Clear();
                return maxima.Value;
            }
        }
        public Vector3 Minima
        {
            get
            {
                if (minima == null) Clear();
                return minima.Value;
            }
        }

        public void InsertValue<T>(float t, ISplineGenerator generator, IList<T> points) where T : SplinePointBase => InsertValue(t, generator, SplineHelper.SplinePointsToVector(points));
        public void InsertValue(float t, ISplineGenerator generator, IList<Vector3> points)
        {
            if (maxima == null) Clear();
            if (t > ((points.Count - generator.SegmentSize) / generator.SlideSize + 1) || t < 0 || float.IsNaN(t)) return;

            InsertValueImpl(generator.Evaluate(t, points));
        }

        public void InsertValueT<T>(float t, Spline<T> spline) where T: SplinePointBase
        {
            if (maxima == null) Clear();
            if (t > spline.SegmentCount || t < 0 || float.IsNaN(t)) return;

            InsertValueImpl(spline.ValueAt(t));
        }

        public void Clear()
        {
            maxima = Vector3.negativeInfinity;
            minima = Vector3.positiveInfinity;
        }

        public void Combine(SplineExtrema extrema)
        {
            if (maxima == null) Clear();

            InsertValueImpl(extrema.Minima);
            InsertValueImpl(extrema.Maxima);
        }

        private void InsertValueImpl(Vector3 newValue)
        {
            maxima = new Vector3(Mathf.Max(newValue.x, maxima.Value.x), Mathf.Max(newValue.y, maxima.Value.y), Mathf.Max(newValue.z, maxima.Value.z));
            minima = new Vector3(Mathf.Min(newValue.x, minima.Value.x), Mathf.Min(newValue.y, minima.Value.y), Mathf.Min(newValue.z, minima.Value.z));
        }
    }
}