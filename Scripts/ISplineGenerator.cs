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

        public Vector3 Evaluate(float t, IList<Vector3> points);

        protected const string _pointAmountErrorMessage = "The passed in amount of points ({0}) does not constitute exactly one {1} segment (required amount: {2})";
    }
}