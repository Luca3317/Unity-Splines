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

        public Matrix4x4 CharacteristicMatrix => _characteristicMatrix;

        public static Vector3 Evaluate(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return SplineHelper.Evaluate(t, 0, _characteristicMatrix, p0, p1, p2, p3);
        }

        public Vector3 Evaluate(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineHelper.Evaluate(t, 0, _characteristicMatrix, points);
        }

        public static Vector3 EvaluateDerivative(float t, int order, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return SplineHelper.Evaluate(t, order, _characteristicMatrix, p0, p1, p2, p3);
        }

        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineHelper.Evaluate(t, order, _characteristicMatrix, points);
        }

        public static IList<float> GetExtremaTs(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 a = 3 * (-p0 + 3 * p1 - 3 * p2 + p3);
            Vector3 b = 6 * (p0 - 2 * p1 + p2);
            Vector3 c = 3 * (p1 - p0);

            float tx1 = a.x != 0 ? (-b.x + Mathf.Sqrt(b.x * b.x - 4 * a.x * c.x)) / (2 * a.x) : float.NaN;
            float tx2 = a.x != 0 ? (-b.x - Mathf.Sqrt(b.x * b.x - 4 * a.x * c.x)) / (2 * a.x) : float.NaN;
            float ty1 = a.y != 0 ? (-b.y + Mathf.Sqrt(b.y * b.y - 4 * a.y * c.y)) / (2 * a.y) : float.NaN;
            float ty2 = a.y != 0 ? (-b.y - Mathf.Sqrt(b.y * b.y - 4 * a.y * c.y)) / (2 * a.y) : float.NaN;
            float tz1 = a.z != 0 ? (-b.z + Mathf.Sqrt(b.z * b.z - 4 * a.z * c.z)) / (2 * a.z) : float.NaN;
            float tz2 = a.z != 0 ? (-b.z - Mathf.Sqrt(b.z * b.z - 4 * a.z * c.z)) / (2 * a.z) : float.NaN;

            return new float[] { tx1, tx2, ty1, ty2, tz1, tz2 };
        }

        public IList<float> GetExtremaTs(IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return GetExtremaTs(points[0], points[1], points[2], points[3]);
        }

        private const int _segmentSize = 4;
        private const int _slideSize = 3;
        private const string _generatorType = "Bezier";
        private static readonly Matrix4x4 _characteristicMatrix = new Matrix4x4
        (
            new Vector4(1, -3, 3, -1),
            new Vector4(0, 3, -6, 3),
            new Vector4(0, 0, 3, -3),
            new Vector4(0, 0, 0, 1)
        );
    }
}