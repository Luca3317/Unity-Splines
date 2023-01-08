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

        public Vector3 Evaluate<T>(float t, params T[] points) where T : SplinePointBase => Evaluate(t, SplineHelper.SplinePointsToVector(points));
        public Vector3 Evaluate<T>(float t, IList<T> points) where T : SplinePointBase => Evaluate(t, SplineHelper.SplinePointsToVector(points));
        public Vector3 Evaluate(float t, params Vector3[] points) => Evaluate(t, (IList<Vector3>)points);
        public Vector3 Evaluate(float t, IList<Vector3> points);

        public Vector3 EvaluateDerivative<T>(float t, int order, params T[] points) where T : SplinePointBase => EvaluateDerivative(t, order, SplineHelper.SplinePointsToVector(points));
        public Vector3 EvaluateDerivative<T>(float t, int order, IList<T> points) where T : SplinePointBase => EvaluateDerivative(t, order, SplineHelper.SplinePointsToVector(points));
        public Vector3 EvaluateDerivative(float t, int order, params Vector3[] points) => EvaluateDerivative(t, order, (IList<Vector3>)points);
        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points);

        public IList<float> GetExtrema<T>(params T[] points) where T : SplinePointBase => GetExtrema(SplineHelper.SplinePointsToVector(points));
        public IList<float> GetExtrema<T>(IList<T> points) where T : SplinePointBase => GetExtrema(SplineHelper.SplinePointsToVector(points));
        public IList<float> GetExtrema(params Vector3[] points) => GetExtrema((IList<Vector3>)points);
        public IList<float> GetExtrema(IList<Vector3> points);

        protected const string _pointAmountErrorMessage = "The passed in amount of points ({0}) does not constitute exactly one {1} segment (required amount: {2})";
    }
}