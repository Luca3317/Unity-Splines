using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;

namespace UnitySplines
{
    [CustomEditor(typeof(EditorSpline))]
    public class EditorSplineEditor : Editor
    {
        EditorSpline editorSpline;

        int selectedGenerator;
        string[] generatorList;

        ISplineGeneratorEditor generatorEditor;

        int selectedPoint = -1;
        int selectedSegment = -1;

        bool alignNormalsToCurveOrientations;

        IReadOnlyList<Vector3> flattened;
        List<Vector3> flattenedNormalsPos = new List<Vector3>(), flattenedNormalsNeg = new List<Vector3>();

        private void OnEnable()
        {
            editorSpline = target as EditorSpline;
             if (editorSpline.Spline == null) Debug.LogWarning("it was null");

            InitGenerators();
        }

        public override void OnInspectorGUI()
        {
            // Generator
            int newSelectedGenerator = EditorGUILayout.Popup(selectedGenerator, generatorList);
            if (selectedGenerator != newSelectedGenerator)
            {
                selectedGenerator = newSelectedGenerator;
                SplineEditorUitlity.SetGenerator(editorSpline.Spline, generatorList[selectedGenerator], ref generatorEditor);
            }

            // Accuracy
            int newAccuracy = EditorGUILayout.IntSlider(editorSpline.Spline.Accuracy, 10, 1000);
            if (editorSpline.Spline.Accuracy != newAccuracy)
            {
                editorSpline.Spline.SetAccuracy(newAccuracy);
            }

            DrawUILine(Color.white);

            // Transform
            SplineEditorUitlity.SplineRotPosScaleInspector(editorSpline.Spline);

            float newNormalOffset = EditorGUILayout.Slider(editorSpline.Spline.NormalAngleOffset, 0f, 360f);
            if (editorSpline.Spline.NormalAngleOffset != newNormalOffset)
            {
                editorSpline.Spline.SetNormalAngleOffset(newNormalOffset);
            }

            DrawUILine(Color.white);

            // Space
            SplineSpace newSpace = (SplineSpace)EditorGUILayout.EnumPopup(editorSpline.Spline.Space);
            if (newSpace != editorSpline.Spline.Space)
            {
                editorSpline.Spline.SetSpace(newSpace);
            }

            DrawUILine(Color.white);

            // Point Transform
            EditorGUILayout.LabelField("Selected Point: " + (selectedPoint > -1 ? selectedPoint : "-"));
            if (selectedPoint >= 0)
            {
                Vector3 newPosition = EditorGUILayout.Vector3Field("Position", editorSpline.Spline.PointPosition(selectedPoint));
                if (newPosition != editorSpline.Spline.PointPosition(selectedPoint))
                {
                    editorSpline.Spline.SetPointPosition(selectedPoint, newPosition);
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Vector3Field("Position", Vector3.zero);
                }
            }

            // Normals 3D
            if (editorSpline.Spline.Space == SplineSpace.XYZ)
            {
                alignNormalsToCurveOrientations = EditorGUILayout.Toggle(alignNormalsToCurveOrientations);
            }

            // Normals 2D
            else
            {

            }


            // Placeholder stuff
            if (GUILayout.Button("Add segment"))
            {
                List<SplinePoint> newPoints = new List<SplinePoint>();
                for (int i = 0; i < editorSpline.Spline.SlideSize; i++)
                    newPoints.Add(new SplinePoint(new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10))));

                editorSpline.Spline.AddSegment(newPoints);
            }


            if (GUI.changed) SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        { 
            // Paint scene
            if (Event.current.type == EventType.Repaint)
            {
                SplineEditorUitlity.DrawSpline(editorSpline.Spline);
                generatorEditor.DrawPointsConnectors(editorSpline.Spline);
                generatorEditor.DrawPoints(editorSpline.Spline);
            }

            // If Control
            else if (Event.current.control)
            {
                // If mouse on point
                // else if mouse on spline
                if (Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Debug.Log("adding");

                    Vector3 pos = SplineEditorUitlity.GetMouseWorldPosition(editorSpline.Spline.Space);

                    List<SplinePoint> points = new List<SplinePoint>();
                    for (int i = 0; i < editorSpline.Spline.SlideSize - 1; i++)
                        points.Add(new SplinePoint(pos - editorSpline.Spline.PointPosition(editorSpline.Spline.PointCount - 1) - Vector3.one * i));
                    points.Add(new SplinePoint(pos));

                    generatorEditor.AddSegment(editorSpline.Spline, points.ToArray());
                }
            }

            // Select point
            // TODO this is directly copied from old editor; clean up
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                for (int i = 0; i < editorSpline.Spline.PointCount; i++)
                {
                    float dst = HandleUtility.DistanceToCircle(editorSpline.Spline.PointPosition(i), (HandleUtility.GetHandleSize(editorSpline.Spline.PointPosition(i) / 2) * (i % 3 == 0 ? 1f : 0.5f)));
                    if (dst == 0)  
                    {
                        selectedPoint = i;
                        GUI.changed = true;
                        break;
                    }
                }

                if (selectedSegment != -1)
                {
                    //Debug.Log(BezierUtility.CurveLineIntersectionTs(path.SegmentAtIndex(selectedSegment), SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(Event.current.mousePosition), BezierCurveEditorUtilities.GetMouseWorldPosition(BezierCurveDimension.XYZ), 100, 0.1f).Count);

                    // var sief = BezierUtility.CurveLineIntersectionTs(path.SegmentAtIndex(selectedSegment), SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(Event.current.mousePosition), BezierCurveEditorUtilities.GetMouseWorldPosition(BezierCurveDimension.XYZ), 1000, 0.01f);
                    (var minDist, var minDistIndex) = SplineEditorUitlity.Get3DIntersect(editorSpline.Spline.Generator, editorSpline.Spline.SegmentPositions(selectedSegment), Event.current.mousePosition, 100);

                    editorSpline.Spline.SplitAt(selectedSegment + minDistIndex);
                    selectedSegment = -1;
                    // Debug.Log("t = " + minDistIndex + " with " + minDist);
                }
            }

            // On KeyDown
            else if (Event.current.type == EventType.KeyDown)
            {
                // Escape => Deselect point
                if (Event.current.keyCode == KeyCode.Escape && selectedPoint >= 0)
                {
                    selectedPoint = -1;
                    GUI.changed = true;
                }
            }

            // Make sure spline is not deselected on click
            else if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(0);

            // Spline Transform Tool
            SplineEditorUitlity.SplineHandle(editorSpline.Spline, Tools.current);

            // Point Transform Tool
            if (selectedPoint >= 0) SplineEditorUitlity.PointHandle(editorSpline.Spline, selectedPoint);

            if (GUI.changed) Repaint();
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        private void InitGenerators()
        {
            SplineEditorUitlity.PopulateGeneratorList(ref generatorList);
            Debug.Log("Editorspline null: " + (editorSpline == null)); 
            Debug.Log("spline null: " + (editorSpline.Spline == null));  
            Debug.Log("generator null: " + (editorSpline.Spline.Generator == null)); 
            selectedGenerator = System.Array.IndexOf(generatorList, editorSpline.Spline.Generator.GeneratorType);
            if (selectedGenerator < 0) Debug.LogError("Spline has generator type that is not supported");
            SplineEditorUitlity.SetGenerator(editorSpline.Spline, generatorList[selectedGenerator], ref generatorEditor);
        }
        /*
        EditorSpline es;
        Spline spline;

        float t = 0f;
        int accuracy = 10;
        SplineSpace space = SplineSpace.XYZ;

        ISplineGeneratorEditor generatorEditor;

        public void OnEnable()
        {
            es = target as EditorSpline;
            if (es == null) Debug.LogError("Editorspline null");

            if (es.Spline == null) Debug.LogError("Spline null");
            else Debug.LogError("we good");

            spline = es.Spline;

            PopulateGeneratorList();
            UpdateSplineGeneratorEditor(spline.Generator.GeneratorType);
        }

        int selectedGenerator;
        static string[] generatorList;

        GeneratorEnum generatorToSetTo;
        public enum GeneratorEnum
        {
            Bezier,
            Hermite,
            Linear,
            Cardinal,
            CatmullRom,
            B
        }

        private void PopulateGeneratorList()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SplineBase));
            var typesWithHelpAttribute =
                   from type in assembly.GetTypes()
                   where type.IsDefined(typeof(SplineGeneratorEditorAttribute), false)
                   select (type.GetCustomAttribute<SplineGeneratorEditorAttribute>()).GeneratorType;

            generatorList = typesWithHelpAttribute.ToArray();
        }

        private void UpdateSplineGeneratorEditor(string generatorType)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SplineBase));
            var typesWithHelpAttribute =
                   from type in assembly.GetTypes()
                   where type.IsDefined(typeof(SplineGeneratorEditorAttribute), false) && (type.GetCustomAttribute<SplineGeneratorEditorAttribute>()).GeneratorType == generatorType
                   select type;

            if (typesWithHelpAttribute.Count() > 1) Debug.LogError("Found multiple types with SplineGenerator(" + generatorType + ") attribute");
            else if (typesWithHelpAttribute.Count() == 0) Debug.LogError("Did not find any types with SplineGenerator(" + generatorType + ") attribute");

            generatorEditor = (ISplineGeneratorEditor)System.Activator.CreateInstance(typesWithHelpAttribute.ElementAt(0));
        }

        private void SetGenerator(int selection)
        {
            selectedGenerator = selection;
            UpdateSplineGeneratorEditor(generatorList[selection]);
            spline.SetGenerator(generatorEditor.Generator);
        }

        bool foldoutPosrotscal = false;
        public override void OnInspectorGUI()
        {
            int newSelectedGenerator = EditorGUILayout.Popup(selectedGenerator, generatorList);
            if (newSelectedGenerator != selectedGenerator) SetGenerator(newSelectedGenerator);

            SplineEditorUitlity.TSliderInspector(ref t, spline);
            SplineEditorUitlity.AccuracySliderInspector(ref accuracy);
            spline.SetAccuracy(accuracy);
            SplineEditorUitlity.SplineSpaceInspector(ref space);
            spline.SetSpace(space);

            foldoutPosrotscal = SplineEditorUitlity.FoldoutWithSubtitle(foldoutPosrotscal, "PosRotScale", "Well what do you think it is", "Oy");
            if (foldoutPosrotscal)
            SplineEditorUitlity.SplineRotPosScaleInspector(spline, true);

            if (GUI.changed)
            {
                Debug.Log("Inspector GUI changed");
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                SplineEditorUitlity.DrawSpline(spline);
                generatorEditor.DrawPointsConnectors(spline);
                generatorEditor.DrawPoints(spline);
                SplineEditorUitlity.Draw3DGizmoAt(spline, t);
                SplineEditorUitlity.DrawBoundingBox(spline);
            }
            else if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(0);

            SplineEditorUitlity.SplineHandle(spline, Tools.current);

            if (GUI.changed)
            {
                Debug.Log("Scene GUI changed");
                SceneView.RepaintAll();
            }
        }
        */
    }
}
