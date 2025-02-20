using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.UI;

[CustomEditor(typeof(XLinePathPoint))]
public class XLinePathPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        XLinePathPoint point = (target as XLinePathPoint);
        bool is2d = point.ParentCurve.Force2D;
        if (!is2d && point.GetComponent<RectTransform>() != null)
            point.gameObject.AddComponent<Transform>();
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.PrefixLabel("Is Smooth");

            bool lastSmooth = point.isSmooth;
            point.isSmooth = EditorGUILayout.Toggle(point.isSmooth);
            if (!lastSmooth && point.isSmooth)
            {
                point.WorldForwardPoint = point.WorldForwardPoint;
            }
        }
        GUILayout.EndHorizontal();
        Color col = GUI.color;
        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.blue;
        GUILayout.Box(new GUIContent(EditorGUIUtility.whiteTexture));
        GUI.color = col;
        //Vector3 val = EditorGUILayout.Vector3Field("Point 1", point.ForwardPoint);
        float len = EditorGUILayout.FloatField("Point 1 length", point.WorldForwardDir.magnitude);
        EditorGUILayout.EndHorizontal();
        if (is2d) EditorGUILayout.LabelField("Local:", point.WorldForwardPoint2D.ToString());
        if (GUI.changed)
        {
            //point.ForwardPoint = val;
            point.WorldForwardPoint = point.Pos + point.ThisTransform.forward.normalized * len;
            GUI.changed = false;
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.red;
        GUILayout.Box(new GUIContent(EditorGUIUtility.whiteTexture));
        GUI.color = col;
        len = EditorGUILayout.FloatField("Point 2 length", point.WorldBackwardDir.magnitude);
        EditorGUILayout.EndHorizontal();
        if (is2d) EditorGUILayout.LabelField("Local:", point.WorldBackwardPoint2D.ToString());
        if (GUI.changed)
        {
            // correct direction along transform forward axis
            point.WorldBackwardPoint = point.Pos - point.ThisTransform.forward.normalized * len;
            GUI.changed = false;
            EditorUtility.SetDirty(target);
        }

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Set corner"))
            {
                point.SetCorner();
                EditorUtility.SetDirty(target);
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Snap to road"))
        {
            point.SnapToRoad();
        }
        if (GUILayout.Button("Add Point"))
        {
            var pt = Instantiate(point);
            pt.ThisTransform.SetParent(point.ThisTransform.parent);
            pt.ThisTransform.SetSiblingIndex(point.ThisTransform.GetSiblingIndex() + 1);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    Vector3 oldPos = Vector3.zero;
    Quaternion oldRot = Quaternion.identity;
    public void OnSceneGUI()
    {
        XLinePathPoint point = (target as XLinePathPoint);
        if (point.ParentCurve == null) return;
        Color col = Handles.color;
        //if (Event.current.type == EventType.MouseDown)
        {
            if (point.WorldForwardPoint != point.Pos)
            {
                Handles.color = Color.blue;
                EditorGUI.BeginChangeCheck();
                var newPos = Handles.FreeMoveHandle(point.WorldForwardPoint, HandleUtility.GetHandleSize(point.WorldForwardPoint) * .1f, Vector3.one, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(point, "Change WorldForwardPoint");
                    var dir = newPos - point.Pos;
                    if(point.isSmooth)
                        point.ThisTransform.forward = dir.normalized;
                    point.WorldForwardPoint = point.Pos + point.ThisTransform.forward * dir.magnitude;
                }
                Handles.color = col;
            }
            if (point.WorldBackwardPoint != point.Pos)
            {
                Handles.color = Color.red;
                EditorGUI.BeginChangeCheck();
                var newPos = Handles.FreeMoveHandle(point.WorldBackwardPoint, HandleUtility.GetHandleSize(point.WorldBackwardPoint) * .1f, Vector3.one, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(point, "Change WorldBackwardPoint");
                    var dir = newPos - point.Pos;
                    if (point.isSmooth)
                        point.ThisTransform.forward = -dir.normalized;
                    point.WorldBackwardPoint = point.Pos + point.ThisTransform.forward * -dir.magnitude;
                }
                Handles.color = col;
            } 
        }


        Handles.color = Color.green;
        Handles.DrawLine(point.WorldForwardPoint, point.Pos);
        Handles.DrawLine(point.Pos, point.WorldBackwardPoint);
        if (Event.current.type == EventType.MouseUp)
        {
            if (oldPos != point.ThisTransform.position || oldRot != point.ThisTransform.rotation)
            {
                point.IsDirty = true;
                oldPos = point.ThisTransform.position;
                oldRot = point.ThisTransform.rotation;
            } 
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
        //Repaint();
    }
}
