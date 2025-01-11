using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(XLinePathPoint))]
public class XLinePathPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        XLinePathPoint point = (target as XLinePathPoint);
        bool is2d = point.GetComponent<RectTransform>() != null;
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.PrefixLabel("Is Smooth");

            bool lastSmooth = point.isSmooth;
            point.isSmooth = EditorGUILayout.Toggle(point.isSmooth);
            if (!lastSmooth && point.isSmooth)
            {
                point.ForwardPoint = point.ForwardPoint;
            }
        }
        GUILayout.EndHorizontal();
        Color col = GUI.color;
        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.blue;
        GUILayout.Box(new GUIContent(EditorGUIUtility.whiteTexture));
        GUI.color = col;
        //Vector3 val = EditorGUILayout.Vector3Field("Point 1", point.ForwardPoint);
        float len = EditorGUILayout.FloatField("Point 1 length", point.ForwardDir.magnitude);
        EditorGUILayout.EndHorizontal();
        if (is2d) EditorGUILayout.LabelField("Local:", point.ForwardPoint2D.ToString());
        if (GUI.changed)
        {
            //point.ForwardPoint = val;
            point.ForwardPoint = point.Pos + point.transform.forward.normalized * len;
            GUI.changed = false;
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.red;
        GUILayout.Box(new GUIContent(EditorGUIUtility.whiteTexture));
        GUI.color = col;
        //val = EditorGUILayout.Vector3Field("Point 2", point.BackwardPoint);
        len = EditorGUILayout.FloatField("Point 2 length", point.BackwardDir.magnitude);
        EditorGUILayout.EndHorizontal();
        if (is2d) EditorGUILayout.LabelField("Local:", point.BackwardPoint2D.ToString());
        if (GUI.changed)
        {
            //point.BackwardPoint = val;
            point.BackwardPoint = point.Pos - point.transform.forward.normalized * len;
            GUI.changed = false;
            EditorUtility.SetDirty(target);
        }

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Set corner"))
            {
                point.isSmooth = false;
                point.backwardPoint = point.forwardPoint = Vector3.zero;
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
            pt.transform.SetParent(point.transform.parent);
            pt.transform.SetSiblingIndex(point.transform.GetSiblingIndex() + 1);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(target);

    }
    Vector3 oldPos = Vector3.zero;
    public void OnSceneGUI()
    {
        XLinePathPoint point = (target as XLinePathPoint);
        if (point.ParentCurve == null) return;
        Color col = Handles.color;
        //if (Event.current.type == EventType.MouseDown)
        {
            if (point.ForwardPoint != point.Pos)
            {
                Handles.color = Color.blue;
                var fmh_99_81_638701359867601746 = Quaternion.identity; point.ForwardPoint = Handles.FreeMoveHandle(point.ForwardPoint, HandleUtility.GetHandleSize(point.ForwardPoint) * .1f, Vector3.one, Handles.SphereHandleCap);
                Handles.color = col;
            }
            if (point.BackwardPoint != point.Pos)
            {
                Handles.color = Color.red;
                var fmh_105_83_638701359867617414 = Quaternion.identity; point.BackwardPoint = Handles.FreeMoveHandle(point.BackwardPoint, HandleUtility.GetHandleSize(point.BackwardPoint) * .1f, Vector3.one, Handles.SphereHandleCap);
                Handles.color = col;
            } 
        }


        Handles.color = Color.green;
        Handles.DrawLine(point.ForwardPoint, point.Pos);
        Handles.DrawLine(point.Pos, point.BackwardPoint);
        if (oldPos != point.ThisTransform.position)
        {
            //point.ParentCurve.CorrectPivot();
            point.ParentCurve.Recalculate();
            oldPos = point.ThisTransform.position;
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
        //Repaint();
    }
}
