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
        /// <summary>
        /// The amount of points that constitute a full segment to this Generator.
        /// </summary>
        public int SegmentSize { get; }
        /// <summary>
        /// The amount of points that constitute a new segment to this Generator.
        /// </summary>
        public int SlideSize { get; }
        /// <summary>
        /// The identifier of this Generator.
        /// </summary>
        public string GeneratorType { get; }

        /// <summary>
        /// Evaluates the spline segment at the given t.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="points">Points defining this segment. Count has to be equal to SegmentSize.</param>
        /// <returns></returns>
        public Vector3 Evaluate(float t, params Vector3[] points) => Evaluate(t, (IList<Vector3>)points);
        public Vector3 Evaluate(float t, IList<Vector3> points);

        /// <summary>
        /// Evaluates the spline's derivative at given t and order.
        /// Order = 1 => first derivative, order = 2 => second derivative, ...
        /// </summary>
        /// <param name="t"></param>
        /// <param name="order"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public Vector3 EvaluateDerivative(float t, int order, params Vector3[] points) => EvaluateDerivative(t, order, (IList<Vector3>)points);
        public Vector3 EvaluateDerivative(float t, int order, IList<Vector3> points);

        /// <summary>
        /// Returns the t's at which the segments extrema lie.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public IList<float> GetExtremaTs(params Vector3[] points) => GetExtremaTs((IList<Vector3>)points);
        public IList<float> GetExtremaTs(IList<Vector3> points);

        /// <summary>
        /// Apply optional normal's modifiers.
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="t"></param>
        /// <param name="normalAngleOffsets"></param>
        /// <returns></returns>
        public float GetNormalsModifier(Vector3 normal, float t, IList<float> normalAngleOffsets) => 0f;

        /// <summary>
        /// Split the given segment into new, equal segments.
        /// 
        /// TODO:
        /// For now take spline as argument; maybe think about some universal rule that would allow passing a list like the other ones.
        /// Or, change other non-static functions to accept a spline as well.
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="spline"></param>
        /// <returns></returns>
        public (int firstSegmentIndex, IList<Vector3> newSegments) SplitSegment(float t, int segmentIndex, SplineBase spline);

        /// <summary>
        /// Get points necessary to loop the spline.
        /// </summary>
        /// <param name="spline"></param>
        /// <returns></returns>
        public IList<Vector3> GetLoopConnectionPoints(SplineBase spline);

        protected const string _pointAmountErrorMessage = "The passed in amount of points ({0}) does not constitute exactly one {1} segment (required amount: {2})";
    }
}