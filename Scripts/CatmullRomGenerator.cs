using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines.CatmullRom
{
    public class CatmullRomGenerator : Singleton<CatmullRomGenerator>, ISplineGenerator
    {
        public int SegmentSize => generator.SegmentSize;
        public int SlideSize => generator.SlideSize;
        public string GeneratorType => _generatorType;

        public static Vector3 Evaluate(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => SplineUtility.Evaluate(t, 0, _characteristicMatrix, p0, p1, p2, p3);
        
        public static Vector3 EvaluateDerivative(float t, int order, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => SplineUtility.Evaluate(t, order, _characteristicMatrix, p0, p1, p2, p3);

      //  public static IList<float> GetExtremaTs(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) => Cardinal.CardinalGenerator.GetExtremaTs(0.5f, p0, p1, p2, p3);

        public Vector3 Evaluate(float t, IList<Vector3> points) => generator.Evaluate(t, points);

        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points) => generator.EvaluateDerivative(t, order, points);

        public IList<float> GetExtremaTs(IList<Vector3> points) => generator.GetExtremaTs(points);

        public (int firstSegmentIndex, IList<Vector3> newSegments) SplitSegment(float t, int segmentIndex, SplineBase spline) => generator.SplitSegment(t, segmentIndex, spline);

        private CatmullRomGenerator() { }

        private static readonly Cardinal.CardinalGenerator generator = new Cardinal.CardinalGenerator(0.5f);
        private const string _generatorType = "CatmullRom";
        private static readonly Matrix4x4 _characteristicMatrix;
    }
}
