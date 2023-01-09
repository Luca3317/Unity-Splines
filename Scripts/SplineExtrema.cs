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

        public void InsertValue(Vector3 newValue)
        {
            if (maxima == null) Clear();

            maxima = new Vector3(Mathf.Max(newValue.x, maxima.Value.x), Mathf.Max(newValue.y, maxima.Value.y), Mathf.Max(newValue.z, maxima.Value.z));
            minima = new Vector3(Mathf.Min(newValue.x, minima.Value.x), Mathf.Min(newValue.y, minima.Value.y), Mathf.Min(newValue.z, minima.Value.z));
        }

        public void InsertValueT<T>(float t, Spline<T> spline) where T: SplinePointBase
        {
            if (maxima == null) Clear();

            if (t > spline.SegmentCount || t < 0 || float.IsNaN(t)) return;
            Vector3 valueAt = spline.ValueAt(t);
            maxima = new Vector3(Mathf.Max(valueAt.x, maxima.Value.x), Mathf.Max(valueAt.y, maxima.Value.y), Mathf.Max(valueAt.z, maxima.Value.z));
            minima = new Vector3(Mathf.Min(valueAt.x, minima.Value.x), Mathf.Min(valueAt.y, minima.Value.y), Mathf.Min(valueAt.z, minima.Value.z));
        }

        public void Clear()
        {
            maxima = Vector3.negativeInfinity;
            minima = Vector3.positiveInfinity;
        }

        public void Combine(SplineExtrema extrema)
        {
            if (maxima == null) Clear();

            InsertValue(extrema.Minima);
            InsertValue(extrema.Maxima);
        }
    }
}