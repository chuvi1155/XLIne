using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XLinePath))]
public class XLinePathEditor : Editor
{
    SerializedProperty editor_Precision;
    SerializedProperty editor_inEditorShowGizmos;
    //SerializedProperty editor_gizmoPointRadius;

    private void OnEnable()
    {
        editor_Precision = serializedObject.FindProperty("editor_Precision");
        editor_inEditorShowGizmos = serializedObject.FindProperty("editor_inEditorShowGizmos");
        //editor_gizmoPointRadius = serializedObject.FindProperty("editor_gizmoPointRadius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(editor_Precision, new GUIContent("Line smoothes"));
        EditorGUILayout.PropertyField(editor_inEditorShowGizmos, new GUIContent("Show Gizmos"));
        //EditorGUILayout.PropertyField(editor_gizmoPointRadius, new GUIContent("Gizmo size"));
        //editor_gizmoPointRadius.floatValue = EditorGUILayout.Slider("Gizmo size", editor_gizmoPointRadius.floatValue, 0f, 1f);
        //_target.editor_Precision = EditorGUILayout.IntField("Line smoothes", _target.editor_Precision);
        //_target.editor_inEditorShowGizmos = EditorGUILayout.Toggle("Show Gizmos", _target.editor_inEditorShowGizmos);
        //_target.editor_gizmoPointRadius = EditorGUILayout.Slider("Gizmo size", _target.editor_gizmoPointRadius, 0f, 1f);

        if (GUILayout.Button("Create Subline"))
        {
            XLinePath _target = target as XLinePath;
            GameObject point = new GameObject(string.Format("Subline_{0}", _target.gameObject.GetComponentsInChildren<XLinePathSubLine>().Length), typeof(XLinePathSubLine));
            point.transform.position = Vector3.zero;
            point.transform.SetParent(_target.transform, true);
            Undo.RegisterCreatedObjectUndo(point, "Create curve subline");
            EditorUtility.SetDirty(point);
        }
        GUI.changed = serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
