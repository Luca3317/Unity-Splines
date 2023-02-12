using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnitySplines
{
    [SplineGeneratorEditor("Hermite")]
    public class HermiteGeneratorEditor : ISplineGeneratorEditor
    {
        public ISplineGenerator Generator => Hermite.HermiteGenerator.Instance;

        public void DrawPoint(SplineBase spline, int pointIndex)
        {
            if (pointIndex % 2 == 0) Handles.color = Color.red;
            else Handles.color = Color.blue;

            Handles.SphereHandleCap
            (
                0,
                spline.PointPosition(pointIndex),
                Quaternion.identity,
                HandleUtility.GetHandleSize(spline.PointPosition(pointIndex)) * 0.2f,
                EventType.Repaint
            );
        }

        public void DrawPoints(SplineBase spline)
        {
            int i = 0;
            foreach (Vector3 pointPos in spline.PointsPositions)
            {
                if (i++ % 2 == 0) Handles.color = Color.red;
                else Handles.color = Color.blue;

                Handles.SphereHandleCap
                (
                    0,
                    pointPos,
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(pointPos) * 0.2f,
                    EventType.Repaint
                );
            }
        }

        public void DrawPointsConnectors(SplineBase spline)
        {
            Handles.color = Color.blue;
            for (int i = 0; i < spline.PointCount; i += 2)
            {
                Handles.DrawLine(spline.PointPosition(i), spline.PointPosition(i + 1));
            }
        }

        public void DrawSplineSpecificInspector(Spline spline)
        {

        }

        public void PointHandle(Spline spline, int pointIndex, Tool tool)
        {
            if (tool == Tool.Move)
            {
                Vector3 newPos = Handles.PositionHandle(spline.PointPosition(pointIndex), Quaternion.identity);
                if (newPos != spline.PointPosition(pointIndex))
                    spline.SetPointPosition(pointIndex, newPos);
            }
        }
    }
}
