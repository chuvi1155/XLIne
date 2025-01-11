using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XLinePath))]
public class XLinePathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        XLinePath _target = target as XLinePath;
        _target.editor_Precision = EditorGUILayout.IntField("Line smoothes", _target.editor_Precision);
        _target.editor_inEditorShowGizmos = EditorGUILayout.Toggle("Show Gizmos", _target.editor_inEditorShowGizmos);
        _target.editor_gizmoPointRadius = EditorGUILayout.Slider("Gizmo size", _target.editor_gizmoPointRadius, 0f, 1f);

        if (GUILayout.Button("Create Subline"))
        {
            GameObject point = new GameObject(string.Format("Subline_{0}", _target.gameObject.GetComponentsInChildren<XLinePathSubLine>().Length), typeof(XLinePathSubLine));
            point.transform.position = Vector3.zero;
            point.transform.SetParent(_target.transform, true);
            Undo.RegisterCreatedObjectUndo(point, "Create curve subline");
            EditorUtility.SetDirty(point);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
