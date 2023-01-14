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

        public void InsertValueT(float segmentT, int segmentIndex, SplineBase spline) => InsertValueT(segmentT, spline.Generator, spline.SegmentPositions(segmentIndex));
        public void InsertValueT(float t, ISplineGenerator generator, IList<Vector3> points)
        {
            if (maxima == null) Clear();
            if (t > 1 || t < 0 || float.IsNaN(t)) return;

            InsertValueImpl(generator.Evaluate(t, points));
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