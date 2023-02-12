using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines.B
{
    public class BGenerator : Singleton<BGenerator>, ISplineGenerator
    {
        public int SegmentSize => _segmentSize;
        public int SlideSize => _slideSize;
        public string GeneratorType => _generatorType;

        public static Vector3 Evaluate(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {

            return SplineUtility.Evaluate(t, 0, _characteristicMatrix, p0, p1, p2, p3);
        }

        public Vector3 Evaluate(float t, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineUtility.Evaluate(t, 0, _characteristicMatrix, points);
        }

        public static Vector3 EvaluateDerivative(float t, int order, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return SplineUtility.Evaluate(t, order, _characteristicMatrix, p0, p1, p2, p3);
        }

        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points)
        {
            if (points.Count != _segmentSize) throw new System.ArgumentException(string.Format(ISplineGenerator._pointAmountErrorMessage, points.Count, _generatorType, _segmentSize));
            return SplineUtility.Evaluate(t, order, _characteristicMatrix, points);
        }

        public IList<float> GetExtremaTs(IList<Vector3> points)
        {
            return new float[] { 0 };
        }

        public (int firstSegmentIndex, IList<Vector3> newSegments) SplitSegment(float t, int segmentIndex, SplineBase spline)
        {
            IList<Vector3> segment = spline.SegmentPositions(segmentIndex);
            IList<Vector3> newSegments = new List<Vector3>();

            Vector3 p0L = (4 * segment[0] + 4 * segment[1]) / 8;
            Vector3 p1L = (segment[0] + 6 * segment[1] + segment[2]) / 8;
            Vector3 p2L = (4 * segment[1] + 4 * segment[2]) / 8;
            Vector3 p3L = (segment[1] + 6 * segment[2] + segment[3]) / 8;

            Vector3 p0R = (segment[0] + 6 * segment[1] + segment[2]) / 4;
            Vector3 p1R = (4 * segment[1] + 4 * segment[2]) / 4;
            Vector3 p2R = (segment[1] + 6 * segment[2] + segment[3]) / 4;
            Vector3 p3R = (4 * segment[2] + 4 * segment[3]) / 4;

            Debug.Log("adding the following points: " + p0L + "; " + p1L + "; " + p2L + "; " + p2R + "; " + p3R);

            newSegments.Add(p0L);
            newSegments.Add(p1L);
            newSegments.Add(p2L);
            newSegments.Add(p2R);
            newSegments.Add(p3R);

            return (segmentIndex, newSegments);
        }

        public IList<Vector3> SplitSegment(float t, IList<Vector3> points)
        {
            throw new System.NotImplementedException();
        }

        private BGenerator() { }

        private const int _segmentSize = 4;
        private const int _slideSize = 1;
        private const string _generatorType = "B";
        /*
        private static readonly Matrix4x4 _characteristicMatrix = new Matrix4x4(
            new Vector4(1, -3, 3, -1),
            new Vector4(4, 0, -6, 3),
            new Vector4(1, 3, 3, -3),
            new Vector4(0, 0, 0, 1)
        );
        */
        private static readonly Matrix4x4 _characteristicMatrix = new Matrix4x4(
            new Vector4(1f / 6f, -0.5f, 0.5f, -1f / 6f),
            new Vector4(4f / 6f, 0, -1, 0.5f),
            new Vector4(1f / 6f, 0.5f, 0.5f, -0.5f),
            new Vector4(0, 0, 0, 1f / 6f)
        );
    }
}
