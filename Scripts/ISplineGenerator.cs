using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    /*
    * Generates curves / splines from segment points.
    */
    public interface ISplineGenerator
    {
        public int SegmentSize { get; }
        public int SlideSize { get; }
        public string GeneratorType { get; }

        public Vector3 Evaluate<T>(float t, params T[] points) where T : SplinePoint => Evaluate(t, SplineHelper.SplinePointsToVector(points));
        public Vector3 Evaluate<T>(float t, IList<T> points) where T : SplinePoint => Evaluate(t, SplineHelper.SplinePointsToVector(points));
        public Vector3 Evaluate(float t, params Vector3[] points) => Evaluate(t, (IList<Vector3>)points);
        public Vector3 Evaluate(float t, IList<Vector3> points);

        public Vector3 EvaluateDerivative<T>(float t, int order, params T[] points) where T : SplinePoint => EvaluateDerivative(t, order, SplineHelper.SplinePointsToVector(points));
        public Vector3 EvaluateDerivative<T>(float t, int order, IList<T> points) where T : SplinePoint => EvaluateDerivative(t, order, SplineHelper.SplinePointsToVector(points));
        public Vector3 EvaluateDerivative(float t, int order, params Vector3[] points) => EvaluateDerivative(t, order, (IList<Vector3>)points);
        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points);

        public IList<float> GetExtremaTs<T>(params T[] points) where T : SplinePoint => GetExtremaTs(SplineHelper.SplinePointsToVector(points));
        public IList<float> GetExtremaTs<T>(IList<T> points) where T : SplinePoint => GetExtremaTs(SplineHelper.SplinePointsToVector(points));
        public IList<float> GetExtremaTs(params Vector3[] points) => GetExtremaTs((IList<Vector3>)points);
        public IList<float> GetExtremaTs(IList<Vector3> points);

        protected const string _pointAmountErrorMessage = "The passed in amount of points ({0}) does not constitute exactly one {1} segment (required amount: {2})";
    }
}