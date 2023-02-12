using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySplines;

public class SplineObject : MonoBehaviour
{
    public Spline spline = new Spline(UnitySplines.Linear.LinearGenerator.Instance, true, new SplinePoint(Vector3.zero), new SplinePoint(Vector3.one));
}
