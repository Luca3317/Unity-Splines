using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnitySplines
{

    [SplineGeneratorEditor("Bezier")]
    public class BezierGeneratorEditor : ISplineGeneratorEditor
    {
        public ISplineGenerator Generator => Bezier.BezierGenerator.Instance;

        public void DrawPoint(SplineBase spline, int pointIndex)
        {
            if (pointIndex % 3 == 0) Handles.color = Color.red;
            else Handles.color = Color.white;

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
                if (i++ % 3 == 0) Handles.color = Color.red;
                else Handles.color = Color.white;

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
            Handles.color = Color.white;
            for (int i = 0; i < spline.SegmentCount; i++)
            {
                Handles.DrawLine(spline.PointPosition(i * spline.SlideSize), spline.PointPosition(i * spline.SlideSize + 1));
                Handles.DrawLine(spline.PointPosition(i * spline.SlideSize + 2), spline.PointPosition(i * spline.SlideSize + 3));
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