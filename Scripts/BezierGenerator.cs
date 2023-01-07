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