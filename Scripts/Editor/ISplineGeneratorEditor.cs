using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnitySplines
{
    public interface ISplineGeneratorEditor
    {
        /*
         * TODO
         * I will probably later make splinebase<T> => Spline<T> with T being the generator
         * Then therell be a genericSpline : Spline<ISplineGenerator> with added methods to get / update generator
         * These ISplineGeneratorEditor are technically only needed for the genericsplines; will have to see
         * If I can change Spline to GenericSpline in these method signatures, it would simplify the generator property;
         * Right now, for stateful generators (ie catmull rom) the generatoreditor needs to hold an instance it can update
         * When using genericspline, it could access the splines generator directly
         */
        public ISplineGenerator Generator { get; }

        public void DrawPoints(SplineBase spline)
        {
            foreach (Vector3 pointPos in spline.PointsPositions)
            {
                Handles.color = Color.red;
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

        public void DrawPoint(SplineBase spline, int pointIndex)
        {
            Vector3 position = spline.PointPosition(pointIndex);
            Handles.color = Color.red;
            Handles.SphereHandleCap
            (
                0,
                position,
                Quaternion.identity,
                HandleUtility.GetHandleSize(position) * 0.2f,
                EventType.Repaint
            );
        }

        public void DrawPointsConnectors(SplineBase spline)
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

        public void DrawSplineSpecificInspector(Spline spline)
        {

        }

        public void AddSegment(Spline spline, params SplinePoint[] points)
        {
            spline.AddSegment(points);
        }
    }
}
