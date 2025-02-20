using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CircleLine))]
public class CircleLineEditor : Editor
{
    SerializedProperty PointNamePrefix;
    SerializedProperty Force2D;
    SerializedProperty curveColor;
    SerializedProperty _radius;

    private void OnEnable()
    {
        PointNamePrefix = serializedObject.FindProperty("PointNamePrefix");
        Force2D = serializedObject.FindProperty("_force2D");
        curveColor = serializedObject.FindProperty("_curveColor");
        _radius = serializedObject.FindProperty("_radius");
    }

    public override void OnInspectorGUI()
    {
        CircleLine _target = target as CircleLine;
        serializedObject.Update();

        //_target.PointNamePrefix = EditorGUILayout.TextField("PointNamePrefix", _target.PointNamePrefix);
        //_target.Force2D = EditorGUILayout.Toggle("Force 2D:", _target.Force2D);
        EditorGUILayout.PropertyField(PointNamePrefix);
        EditorGUILayout.PropertyField(Force2D, new GUIContent("Force 2D:"));

        EditorGUILayout.BeginHorizontal();
        {
            Undo.RecordObject(_target, "Zero Transform Position");
            EditorGUILayout.PrefixLabel("Color:");
            //_target.curveColor = EditorGUILayout.ColorField(_target.curveColor);
            EditorGUILayout.PropertyField(curveColor);
        }
        EditorGUILayout.EndHorizontal();

        //_target.Radius = EditorGUILayout.FloatField("Radius", _target.Radius);
        EditorGUILayout.PropertyField(_radius);

        serializedObject.ApplyModifiedProperties();

        if (_target.IsDirty)
            _target.Recalculate();
        EditorGUILayout.LabelField("  Curve Length: ", _target.Length.ToString("f2"));
    }
}
#endif

[AddComponentMenu("Chuvi/Line/Circle")]
public class CircleLine : XLinePathSubLine
{
    [SerializeField] float _radius = 1;
    public float Radius
    {
        get { return _radius; }
        set
        {
            if (_radius != value)
            {
                _radius = value;
                SetDirty();
            }
            else _radius = value;
        }
    }

    public override void Init()
    {
        IsClosed = true;
        _segments = new XLinePathSegment[0];
        recalcSegments = true;

        Recalculate();
        if (AvtoCorrectPivot) CorrectPivot();
    }

    public override void Recalculate()
    {
        IsClosed = true;
        if (parent == null) parent = GetComponentInParent<XLinePath>();
        points = GetComponentsInChildren<XLinePathPoint>();
        if (points.Length > 4)
        {
            for (int i = 4; i < points.Length; i++)
            {
                DestroyImmediate(points[i].gameObject);
            }
        }

        float step = Mathf.PI * 0.5f;
        if (points.Length < 4)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject go = new GameObject("Point_" + i, typeof(XLinePathPoint));
                go.transform.SetParent(transform);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3(Mathf.Sin(step * i) * _radius, 0, Mathf.Cos(step * i) * _radius);
                go.GetComponent<XLinePathPoint>().isSmooth = true;
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                points[i].transform.localPosition = new Vector3(Mathf.Sin(step * i) * _radius, 0, Mathf.Cos(step * i) * _radius);
                points[i].isSmooth = true;
            }
        }

        _length = 0;
        _segments = new XLinePathSegment[4];
        for (int i = 0; i < _segments.Length; i++)
        {
                if (!string.IsNullOrEmpty(PointNamePrefix)) points[i].name = PointNamePrefix + i;
                _segments[i] = new XLinePathSegment(points[i], i + 1 < points.Length ? points[i + 1] : points[0]);

            _segments[i].SegmentPartsCount = Precision;
            _segments[i].Recalculate(Force2D);
            _length += _segments[i].length;
        }
        SetSmoothPoints(2.5f);
       recalcSegments = false;
    }
}
