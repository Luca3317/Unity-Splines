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
        public string GeneratorType { get; }
        public Vector3 Evaluate(float t, IList<Vector3> points);
    }
}