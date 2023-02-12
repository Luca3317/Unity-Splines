using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnitySplines
{
    [SplineGeneratorEditor("Linear")]
    public class LinearGeneratorEditor : ISplineGeneratorEditor
    {
        public ISplineGenerator Generator => Linear.LinearGenerator.Instance;

        public void DrawPoints(SplineBase spline)
        {
            Handles.color = Color.white;
            foreach (Vector3 position in spline.PointsPositions)
            {
                Handles.SphereHandleCap
                (
                    0,
                    position,
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(position) * 0.2f,
                    EventType.Repaint
                );
            }
        }
    }
}