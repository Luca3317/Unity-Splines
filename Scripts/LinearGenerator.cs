using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines.Linear
{
    public class LinearGenerator : Singleton<LinearGenerator>, ISplineGenerator
    {
        public int SegmentSize => _segmentSize;
        public int SlideSize => _slideSize;
        public string GeneratorType => _generatorType;

        public static Vector3 Evaluate(float t, Vector3 start, Vector3 end) => start * (1 - t) + end * t;

        public static Vector3 EvaluateDerivative(float t, int order, Vector3 start, Vector3 end)
        {
            if (order < 0 || order > 2) throw new System.ArgumentOutOfRangeException();
            else if (order == 0) return Evaluate(t, start, end);
            else if (order == 1) return -start + end;
            return Vector3.zero;
        }

        public static IList<float> GetExtremaTs(Vector3 start, Vector3 end) => new float[] { 0f, 1f };

        public static float GetNormalsModifier(Vector3 normal, float t, float startNormalAngleOffset, float endNormalAngleOffset) => startNormalAngleOffset * (1 - t) + startNormalAngleOffset * t;

        public Vector3 Evaluate(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return points[0] * (1 - t) + points[1] * t;
        }

        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return EvaluateDerivative(t, order, points[0], points[1]);
        }
  
        public IList<float> GetExtremaTs(IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return new float[] { 0f, 1f };
        }

        public float GetNormalsModifier(Vector3 normal, float t, IList<float> normalAngleOffsets)
        {
            if (normalAngleOffsets.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, normalAngleOffsets.Count, _generatorType, _segmentSize));
            return normalAngleOffsets[0] * (1 - t) + normalAngleOffsets[1] * t;
        }

        public (int firstSegmentIndex, IList<Vector3> newSegments) SplitSegment(float t, int segmentIndex, SplineBase spline)
        {
            IList<Vector3> segment = new List<Vector3>(spline.SegmentPositions(segmentIndex));
            segment.Insert(1, Evaluate(t, segment));
            return (segmentIndex, segment);
        }

        private LinearGenerator() { }

        private const int _segmentSize = 2;
        private const int _slideSize = 1;
        private const string _generatorType = "Linear";
    }
}