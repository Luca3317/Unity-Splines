using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines.Linear
{
    public class LinearGenerator : ISplineGenerator
    {
        public int SegmentSize => _segmentSize;

        public int SlideSize => _slideSize;

        public string GeneratorType => _generatorType;

        public Vector3 Evaluate(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException();
            return points[0] * (1-t) + points[1] * t;
        }

        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException();
            if (order < 0 || order > 2) throw new System.ArgumentOutOfRangeException();
            else if (order == 0) return Evaluate(t, points);
            else if (order == 1) return -points[0] + points[1];
            return Vector3.zero;
        }

        public IList<float> GetExtremaTs(IList<Vector3> points)
        {
            return new float[] { 0f, 1f };
        }

        public float GetNormalsModifier(Vector3 normal, float t, IList<float> normalAngleOffsets)
        {
            if (normalAngleOffsets.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, normalAngleOffsets.Count, _generatorType, _segmentSize));
            return normalAngleOffsets[0] * (1 - t) + normalAngleOffsets[1] * t;
        }

        public IList<Vector3> SplitSegment(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));

            Vector3 point0 = points[0], point1 = points[1];
            Vector3 splitPoint = Evaluate(t, points);

            List<Vector3> newPoints = new List<Vector3>();
            newPoints.Add(point0);
            newPoints.Add(splitPoint);
            newPoints.Add(point1);
            return newPoints;
        }

        private const int _segmentSize = 2;
        private const int _slideSize = 1;
        private const string _generatorType = "Linear";
    }
}