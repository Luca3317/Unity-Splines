using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines.Cardinal
{
    public class CardinalGenerator : ISplineGenerator
    {
        public int SegmentSize => _segmentSize;
        public int SlideSize => _slideSize;
        public string GeneratorType => _generatorType;
        public float Scale => _scale;

        public CardinalGenerator(float scale)
        {
            SetScale(scale);
        }

        public void SetScale(float scale)
        {
            _scale = scale;
            _characteristicMatrix = CreateMatrix(_scale);
        }

        public static Vector3 Evaluate(float scale, float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => SplineUtility.Evaluate(t, 0, CreateMatrix(scale), p0, p1, p2, p3);

        public static Vector3 EvaluateDerivative(float scale, float t, int order, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => SplineUtility.Evaluate(t, order, CreateMatrix(scale), p0, p1, p2, p3);

        public static IList<float> GetExtremaTs(float scale, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            IList<Vector3> bezier = ToBezier(scale, p0, p1, p2, p3);
            return Bezier.BezierGenerator.GetExtremaTs(bezier[0], bezier[1], bezier[2], bezier[3]);
        }

        public Vector3 Evaluate(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineUtility.Evaluate(t, 0, _characteristicMatrix, points[0], points[1], points[2], points[3]);
        }

        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineUtility.Evaluate(t, order, _characteristicMatrix, points[0], points[1], points[2], points[3]);
        }

        public IList<float> GetExtremaTs(IList<Vector3> points) => GetExtremaTs(_scale, points[0], points[1], points[2], points[3]);

        public (int firstSegmentIndex, IList<Vector3> newSegments) SplitSegment(float t, int segmentIndex, SplineBase spline)
        {
            IList<Vector3> list = new List<Vector3>();

            Vector3 splitPoint = Evaluate(t, spline.SegmentPositions(segmentIndex));
            IList<Vector3> segment = spline.SegmentPositions(segmentIndex);

            list.Add(segment[0]);
            list.Add(segment[1]);
            list.Add(splitPoint);

            return (segmentIndex, list);
        }

        private float _scale;
        private Matrix4x4 _characteristicMatrix;

        private const int _segmentSize = 4;
        private const int _slideSize = 1;
        private const string _generatorType = "Cardinal";

        private static Matrix4x4 CreateMatrix(float scale)
        {
            return new Matrix4x4
            (
                  new Vector4(0, -scale, 2 * scale, -scale),
                  new Vector4(1, 0, scale - 3, 2 - scale),
                  new Vector4(0, scale, 3 - 2 * scale, scale - 2),
                  new Vector4(0, 0, -scale, scale)
            );
        }

        private static List<Vector3> ToBezier(float scale, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            List<Vector3> bezierpoints = new List<Vector3>();
            bezierpoints.Add(p1);
            bezierpoints.Add(p1 + scale * (p2 - p0) / 3);
            bezierpoints.Add(p2 - scale * (p3 - p1) / 3);
            bezierpoints.Add(p2);
            return bezierpoints;
        }
    }
}
