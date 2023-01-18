using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines.Hermite
{
    public class HermiteGenerator : Singleton<HermiteGenerator>, ISplineGenerator
    {
        public int SegmentSize => _segmentSize;
        public int SlideSize => _slideSize;
        public string GeneratorType => _generatorType;

        public static Vector3 Evaluate(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => SplineUtility.Evaluate(t, 0, _characteristicMatrix, p0, p1 - p0, p2, p3 - p2);

        public static Vector3 EvaluateDerivative(float t, int order, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => SplineUtility.Evaluate(t, order, _characteristicMatrix, p0, p1 - p0, p2, p3 - p2);

        public static IList<float> GetExtremaTs(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            IList<Vector3> bezier = ToBezier(p0, p1 - p0, p2, p3 - p2);
            return Bezier.BezierGenerator.GetExtremaTs(bezier[0], bezier[1], bezier[2], bezier[3]);
        }

        public Vector3 Evaluate(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineUtility.Evaluate(t, 0, _characteristicMatrix, points[0], points[1] - points[0], points[2], points[3] - points[2]);
        }

        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineUtility.Evaluate(t, order, _characteristicMatrix, points[0], points[1] - points[0], points[2], points[3] - points[2]);
        }

        public IList<float> GetExtremaTs(IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return GetExtremaTs(points[0], points[1], points[2], points[3]);
        }

        public (int firstSegmentIndex, IList<Vector3> newSegments) SplitSegment(float t, int segmentIndex, SplineBase spline)
        {
            IList<Vector3> newSegments = new List<Vector3>();
            IList<Vector3> segment = spline.SegmentPositions(segmentIndex);
            IList<Vector3> tobezier = ToBezier(segment[0], segment[1] - segment[0], segment[2], segment[3] - segment[2]);
            IList<Vector3> bezier = Bezier.BezierGenerator.SplitSegment(t, tobezier[0], tobezier[1], tobezier[2], tobezier[3]);

            newSegments.Add(segment[0]);
            newSegments.Add(segment[0] + (bezier[1] - segment[0]) * 3);
            newSegments.Add(bezier[3]);
            newSegments.Add(bezier[3] - (bezier[2] - bezier[3]) * 3);
            newSegments.Add(segment[2]);
            newSegments.Add(segment[2] - (bezier[5] - segment[2]) * 3);

            return (segmentIndex, newSegments);
        }

        private HermiteGenerator() { }

        private const int _segmentSize = 4;
        private const int _slideSize = 2;
        private const string _generatorType = "Hermite";
        private static readonly Matrix4x4 _characteristicMatrix = new Matrix4x4
        (
            new Vector4(1, 0, -3, 2),
            new Vector4(0, 1, -2, 1),
            new Vector4(0, 0, 3, -2),
            new Vector4(0, 0, -1, 1)
        );

        private static List<Vector3> ToBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            List<Vector3> bezierpoints = new List<Vector3>
            {
                p0,
                p0 + p1 / 3,
                p2 - p3 / 3,
                p2
            };
            return bezierpoints;
        }
    }
}