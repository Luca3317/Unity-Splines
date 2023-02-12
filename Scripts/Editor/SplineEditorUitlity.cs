using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace UnitySplines
{
    public static class SplineEditorUitlity
    {
        public static float mousedist = 100f;
        public static Vector3 GetMouseWorldPosition(SplineSpace space, float depthFor3DSpace = 10)
        {
            var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var worldMouse = Physics.Raycast(mouseRay, out var hitInfo, depthFor3DSpace * 2f) ?
                hitInfo.point : mouseRay.GetPoint(depthFor3DSpace);

            // Mouse can only move on XY plane
            if (space == SplineSpace.XY)
            {
                var zDir = mouseRay.direction.z;
                if (zDir != 0)
                {
                    var dstToXYPlane = Mathf.Abs(mouseRay.origin.z / zDir);
                    worldMouse = mouseRay.GetPoint(dstToXYPlane);
                }
            }
            // Mouse can only move on XZ plane 
            else if (space == SplineSpace.XZ)
            {
                var yDir = mouseRay.direction.y;
                if (yDir != 0)
                {
                    var dstToXZPlane = Mathf.Abs(mouseRay.origin.y / yDir);
                    worldMouse = mouseRay.GetPoint(dstToXZPlane);
                }
            }

            return worldMouse;
        }

        // Normal linesintersect3d problem:
        // When selecting segment => wide margin of error necessary
        // When actually splitting => small margin of error necessary
        // Therefore editor only solution
        // TODO: lerp for mindistindex
        public static (float, float) Get3DIntersect(ISplineGenerator generator, IList<Vector3> points, Vector3 point, int accuracy)
        {
            float step = 1f / accuracy;
            float mindist = float.MaxValue;
            float mindistindex = 100f;

            for (int j = 0; j < accuracy - 1; j++)
            {
                float t = j * step;

                Vector3 p1 = HandleUtility.WorldToGUIPoint(generator.Evaluate(t, points)), p2 = HandleUtility.WorldToGUIPoint(generator.Evaluate(t + step, points));
                Vector3 closestPoint = MathUtility.ClosestPointOnLineSegment(point, p1, p2);

                if ((closestPoint - point).magnitude < mindist)
                {
                    mindist = (closestPoint - point).magnitude;
                    mindistindex = (t * 2f + step) / 2f;
                }
            }
            return (mindist, mindistindex);
        }

        #region Generator Editors
        public static void PopulateGeneratorList(ref string[] generatorList)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SplineBase));
            var typesWithHelpAttribute =
                   from type in assembly.GetTypes()
                   where type.IsDefined(typeof(SplineGeneratorEditorAttribute), false)
                   select (type.GetCustomAttribute<SplineGeneratorEditorAttribute>()).GeneratorType;

            generatorList = typesWithHelpAttribute.ToArray();
        }

        public static bool UpdateSplineGeneratorEditor(string generatorType, ref ISplineGeneratorEditor generatorEditor)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SplineBase));
            var typesWithHelpAttribute =
                   from type in assembly.GetTypes()
                   where type.IsDefined(typeof(SplineGeneratorEditorAttribute), false) && (type.GetCustomAttribute<SplineGeneratorEditorAttribute>()).GeneratorType == generatorType
                   select type;

            if (typesWithHelpAttribute.Count() > 1) Debug.LogError("Found multiple types with SplineGenerator(" + generatorType + ") attribute");
            else if (typesWithHelpAttribute.Count() == 0) Debug.LogError("Did not find any types with SplineGenerator(" + generatorType + ") attribute");

            ISplineGeneratorEditor newGeneratorEditor = System.Activator.CreateInstance(typesWithHelpAttribute.ElementAt(0)) as ISplineGeneratorEditor;
            if (newGeneratorEditor == null) return false;

            generatorEditor = newGeneratorEditor;
            return true;
        }

        public static bool SetGenerator(Spline spline, string selection, ref ISplineGeneratorEditor generatorEditor)
        {
            if (!UpdateSplineGeneratorEditor(selection, ref generatorEditor)) return false; 
            spline.SetGenerator(generatorEditor.Generator);
            return true;
        }
        #endregion



        public static void SplineRotPosScaleInspector(Spline spline, bool foldout = true)
        {
            Vector3 newPosition = EditorGUILayout.Vector3Field("Position", spline.position);
            if (newPosition != spline.position) spline.SetPosition(newPosition);

            Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", spline.rotation.eulerAngles);
            if (newRotation != spline.rotation.eulerAngles) spline.SetRotation(new Quaternion(newRotation.x, newRotation.y, newRotation.z, 1));

            Vector3 newScale = EditorGUILayout.Vector3Field("Scale", spline.scale);
            if (newScale != spline.scale) spline.SetScale(newScale);
        }

        public static bool FoldoutWithSubtitle(bool foldout, string header, string subtitle, string tooltip = "")
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 15;
            headerStyle.fontStyle = FontStyle.Bold;

            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 10;

            Rect rect = EditorGUILayout.GetControlRect(false, 50f);

            EditorGUI.LabelField(rect, header, headerStyle);
            foldout = EditorGUI.Foldout(rect, foldout, "", true);
            rect.y += 20.0f;
            EditorGUI.LabelField(rect, subtitle, subtitleStyle);
            rect.y += 10.5f;
            EditorGUI.LabelField(rect, "", GUI.skin.horizontalSlider);
            return foldout;
        }

        public static bool FoldoutWithSubtitle2(bool foldout, string header, string subtitle, string tooltip = "")
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 15;
            headerStyle.fontStyle = FontStyle.Bold;

            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 10;

            foldout = EditorGUILayout.Foldout(foldout, new GUIContent(header, "d"), true, headerStyle);
            EditorGUILayout.LabelField(new GUIContent(subtitle, "b"), subtitleStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            return foldout;
        }

        // Credits to https://gist.github.com/MattRix/5972828
        private static void HeaderWithSubtitle(string header, string subtitle)
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 15;
            headerStyle.fontStyle = FontStyle.Bold;

            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 10;

            Rect rect = EditorGUILayout.GetControlRect(false, 50f);

            EditorGUI.LabelField(rect, header, headerStyle);
            rect.y += 18.0f;
            EditorGUI.LabelField(rect, subtitle, subtitleStyle);
            rect.y += 10.5f;
            EditorGUI.LabelField(rect, "", GUI.skin.horizontalSlider);
        }


        #region InspectorGUI
        public static void TSliderInspector(ref float t, SplineBase spline)
        {
            t = EditorGUILayout.Slider(t, 0f, spline.SegmentCount);
        }

        public static void AccuracySliderInspector(ref int accuracy)
        {
            accuracy = EditorGUILayout.IntSlider(accuracy, 10, 1000);
        }

        public static void SplineSpaceInspector(ref SplineSpace space)
        {
            space = (SplineSpace)EditorGUILayout.EnumPopup(space);
        }
        #endregion

        #region SceneGUI
        public static void DrawSpline(SplineBase spline)
        {
            IReadOnlyList<Vector3> flattened = spline.GetFlattened();

            Handles.color = Color.white;
            for (int i = 0; i < flattened.Count - 1; i++)
            {
                Handles.DrawAAPolyLine(flattened[i], flattened[i + 1]);
            }
        }

        public static void DrawSegment(SplineBase spline, int segmentIndex)
        {
            IReadOnlyList<Vector3> flattened = spline.GetFlattenedSegment(segmentIndex);

            Handles.color = Color.red;
            for (int i = 0; i < flattened.Count - 1; i++)
            {
                Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, flattened[i], flattened[i + 1]);
            }
        }

        public static void DrawPoints(SplineBase spline)
        {
            Handles.color = Color.red;
            for (int i = 0; i < spline.PointCount; i++)
                Handles.SphereHandleCap(
                    0,
                    spline.PointPosition(i),
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(spline.PointPosition(i)) * 0.2f,
                    EventType.Repaint
                );
        }

        public static void DrawPoint(SplineBase spline, int pointIndex)
        {
            Handles.color = Color.red;
            Handles.SphereHandleCap(
                0,
                spline.PointPosition(0),
                Quaternion.identity,
                HandleUtility.GetHandleSize(spline.PointPosition(pointIndex)) * 0.2f,
                EventType.Repaint
            );
        }

        public static void DrawSegmentPoints(SplineBase spline, int segmentIndex)
        {
            Handles.color = Color.red;
            for (int i = 0; i < spline.SegmentSize; i++)
                Handles.SphereHandleCap(
                    0,
                    spline.PointPosition(segmentIndex * spline.SlideSize + i),
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(spline.PointPosition(i)) * 0.2f,
                    EventType.Repaint
                );
        }

        public static void Draw3DGizmoAt(SplineBase spline, float t)
        {
            Vector3 valueAt = spline.ValueAt(t);

            Handles.color = Color.green;
            Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, valueAt, valueAt + spline.TangentAt(t).normalized);

            Handles.color = Color.red;
            Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, valueAt, valueAt + spline.NormalAt(t).normalized);

            /*
             * Rotational axis / up
            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, valueAt, valueAt + spline.TangentAt(t).normalized);
            */
        }

        public static void DrawBoundingBox(SplineBase spline)
        {
            Handles.color = Color.white;
            Bounds b = spline.GetBounds();
            Handles.DrawWireCube(b.center, b.size);
        }

        public static void PointHandle(Spline spline, int pointIndex, Object target = null)
        {
            Vector3 newPos = Handles.PositionHandle(spline.PointPosition(pointIndex), Quaternion.identity);
            if (newPos != spline.PointPosition(pointIndex))
                spline.SetPointPosition(pointIndex, newPos);
        }

        public static void SplineHandle(Spline spline, Tool tool, Object target = null)
        {
            switch (tool)
            {
                case Tool.Move:
                    Vector3 newPosition = Handles.PositionHandle(spline.position, Quaternion.identity);
                    if (newPosition != spline.position)
                    {
                        if (target != null) Undo.RecordObject(target, "Moved spline");
                        spline.SetPosition(newPosition);
                    }
                    break;

                case Tool.Scale:
                    Vector3 newScale = Handles.ScaleHandle(spline.scale, spline.position, Quaternion.identity);
                    if (newScale != spline.scale)
                    {
                        if (target != null) Undo.RecordObject(target, "Scaled spline");
                        spline.SetScale(newScale);
                    }
                    break;

                case Tool.Rotate:
                    Quaternion rot = Handles.RotationHandle(spline.rotation, spline.position);
                    if (new Vector4(rot.x, rot.y, rot.z, rot.w) != new Vector4(spline.rotation.x, spline.rotation.y, spline.rotation.z, spline.rotation.w))
                    {
                        if (target != null) Undo.RecordObject(target, "Rotated spline");
                        spline.SetRotation(rot);
                    }
                    break;
            }
        }
        #endregion

        public static void AlignCurveHandle(Spline spline, AlignTransformPosition align)
        {
            Vector3 alignPos = Vector3.zero;
            Bounds bounds = spline.GetBounds();
            switch (align)
            {
                case AlignTransformPosition.Center: alignPos = bounds.center; break;
                case AlignTransformPosition.BottomRight: alignPos = new Vector3(bounds.center.x + bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z - bounds.size.z / 2); break;
                case AlignTransformPosition.TopRight: alignPos = new Vector3(bounds.center.x + bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z - bounds.size.z / 2); break;
                case AlignTransformPosition.BottomLeft: alignPos = new Vector3(bounds.center.x - bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z - bounds.size.z / 2); break;
                case AlignTransformPosition.TopLeft: alignPos = new Vector3(bounds.center.x - bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z - bounds.size.z / 2); break;
            }
            spline.AlignPosition(alignPos);
            GUI.changed = true;
        }

        public enum AlignTransformPosition
        {
            Center,
            BottomRight,
            TopRight,
            BottomLeft,
            TopLeft
        }

        public enum PositionValues
        {
            Absolute,
            Relative
        }
    }
}