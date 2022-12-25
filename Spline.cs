using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Representation of a full spline. Primarily consists of a segmented point-collection (SplinePoints) and a SplineGenerator.
 * 
 * Maybe inherit from SplinePoints instead of wrapper methods?
 */
[System.Serializable]
public class Spline
{
    public Vector3 ValueAt(float t) => throw new System.NotImplementedException();

    [SerializeField] protected SplinePoints points;
    [SerializeField] protected ISplineGenerator generator;
}