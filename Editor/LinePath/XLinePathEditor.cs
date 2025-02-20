using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XLinePath))]
public class XLinePathEditor : Editor
{
    SerializedProperty editor_Precision;
    SerializedProperty editor_inEditorShowGizmos;

    private void OnEnable()
    {
        editor_Precision = serializedObject.FindProperty("editor_Precision");
        editor_inEditorShowGizmos = serializedObject.FindProperty("editor_inEditorShowGizmos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(editor_Precision, new GUIContent("Line smoothes"));
        EditorGUILayout.PropertyField(editor_inEditorShowGizmos, new GUIContent("Show Gizmos"));

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
