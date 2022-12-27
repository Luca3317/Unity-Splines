using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines.Bezier
{
    public class BezierGenerator : ISplineGenerator
    {
        public int SegmentSize => _segmentSize;
        public int SlideSize => _slideSize;

        public string GeneratorType => _generatorType;

        public static Vector3 Evaluate(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            float t3 = t * t2;
            float mt = 1 - t;
            float mt2 = mt * mt;
            float mt3 = mt * mt2;

            // Bernstein Polynomial
            // ValueAt_Cubic(t) =
            // startPoint *      ( -t^3 + 3t^2 - 3t + 1 ) +
            // controlPoint[0] * ( t3t^3 - 6t^2 + 3t ) +
            // controlPoint[1] * ( -3t^3 + 3t^2 ) +
            // endPoint *        ( t^3 )

            return
                p0 * mt3 +
                p1 * 3 * mt2 * t +
                p2 * 3 * mt * t2 +
                p3 * t3;
        }

        public Vector3 Evaluate(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return Evaluate(t, points[0], points[1], points[2], points[3]);
        }

        private const int _segmentSize = 4;
        private const int _slideSize = 3;
        private const string _generatorType = "Bezier";
    }
}