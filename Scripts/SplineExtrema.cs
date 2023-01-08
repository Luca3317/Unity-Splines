using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public class SplineExtrema
    {
        Vector3 maxima;
        Vector3 minima;

        public Vector3 Maxima => maxima;
        public Vector3 Minima => minima;

        public SplineExtrema()
        {
            maxima = Vector3.negativeInfinity;
            minima = Vector3.positiveInfinity;
        }

        public void InsertValue(Vector3 newValue)
        {
            maxima = new Vector3(Mathf.Max(newValue.x, maxima.x), Mathf.Max(newValue.y, maxima.y), Mathf.Max(newValue.z, maxima.z));
            minima = new Vector3(Mathf.Min(newValue.x, minima.x), Mathf.Min(newValue.y, minima.y), Mathf.Min(newValue.z, minima.z));
        }

        public void InsertValueT<T>(float t, Spline<T> spline) where T: SplinePointBase
        {
            if (t > spline.SegmentCount || t < 0 || float.IsNaN(t)) return;
            Vector3 valueAt = spline.ValueAt(t);
            maxima = new Vector3(Mathf.Max(valueAt.x, maxima.x), Mathf.Max(valueAt.y, maxima.y), Mathf.Max(valueAt.z, maxima.z));
            minima = new Vector3(Mathf.Min(valueAt.x, minima.x), Mathf.Min(valueAt.y, minima.y), Mathf.Min(valueAt.z, minima.z));
        }

        public void Clear()
        {
            maxima = Vector3.negativeInfinity;
            minima = Vector3.positiveInfinity;
        }
    }
}