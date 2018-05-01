using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Trains.Library.Spline
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineInspector : Editor
    {
        private BezierSpline spline;
        private Transform handleTransform;
        private Quaternion handleRotation;

        private const int stepsPerCurve = 20;
        private const float directionScale = 0.5f;
        private const float handleSize = 0.04f;
        private const float pickSize = 0.06f;

        private int selectedIndex = -1;

        private void OnSceneGUI()
        {
            spline = target as BezierSpline;
            handleTransform = spline.transform;
            handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? handleTransform.rotation
                : Quaternion.identity;

            var p0 = ShowPoint(0);

            for (int i = 1; i < spline.ControlPointCount; i += 3)
            {
                var p1 = ShowPoint(i);
                var p2 = ShowPoint(i + 1);
                var p3 = ShowPoint(i + 2);

                Handles.color = Color.cyan;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);

                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 4f);

                p0 = p3;
            }
            
            ShowDirections();
            
        }

        public override void OnInspectorGUI()
        {
            spline = target as BezierSpline;

            if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
            {
                DrawSelectedPointInspector();
            }

            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(spline, "Add Curve");
                spline.AddCurve();

                EditorUtility.SetDirty(spline);
            }
        }

        private void DrawSelectedPointInspector()
        {
            GUILayout.Label("Selected Point");
            EditorGUI.BeginChangeCheck();

            var point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move point");
                EditorUtility.SetDirty(spline);

                spline.SetControlPoint(selectedIndex, handleTransform.InverseTransformPoint(point));
            }
        }

        private Vector3 ShowPoint(int index)
        {
            var point = handleTransform.TransformPoint(spline.GetControlPoint(index));

            var size = HandleUtility.GetHandleSize(point);
            Handles.color = Color.white;
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {
                selectedIndex = index;
                Repaint();
            }

            if (selectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move point");
                    EditorUtility.SetDirty(spline);

                    spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
                }
            }

            return point;
        }

        private void ShowDirections()
        {
            Handles.color = Color.green;
            var point = spline.GetPoint(0f);
            Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);

            var steps = stepsPerCurve * spline.CurveCount;


            for (float i = 0; i <= steps; i++)
            {
                point = spline.GetPoint(i / steps);
                Handles.DrawLine(point, point + spline.GetDirection(i / steps) * directionScale);
            }
        }
    }
}