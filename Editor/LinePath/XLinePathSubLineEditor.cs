﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(XLinePathSubLine))]
public class XLinePathSubLineEditor : Editor
{
    private static bool _foldout;
    private static bool _seg_foldout;
    private static bool _show_primitives;
    private static bool _show_info;
    private XLinePathSubLine _target;
    private static float radius = 1;
    private static int circle_segments = 4;
    private static float size = 1;
    private static float tri_radius = 1;
    private List<XLinePathSubLine.XSegment.XPoint> points = new List<XLinePathSubLine.XSegment.XPoint>();

    void OnEnable()
    {
        _target = target as XLinePathSubLine;
    }

    private void OnDisable()
    {
        if (_target.inEditorClickModeOn)
            SetAsSelected();
    }

    [MenuItem(("Chuvi/Line/Create XLine"))]
    public static void CreateCurveObject()
    {
        //Undo.RegisterSceneUndo("Create Curve Improved");
        GameObject go = new GameObject("XLine", typeof(XLinePath));
        GameObject _go = new GameObject("SubLine", typeof(XLinePathSubLine));
        _go.transform.parent = go.transform;
        _go.transform.localPosition = Vector3.zero;
        //go.GetComponent<XLinePathSubLine>().autoSmooth = true;
        Undo.RegisterCreatedObjectUndo(go, "CreateCurveObject");
        //Undo.DestroyObjectImmediate(go);
    }

    void SetAsSelected()
    {
        Selection.activeGameObject = _target.gameObject;
    }
   //float div = 3;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical("box");
        _target.PointNamePrefix = EditorGUILayout.TextField("PointNamePrefix", _target.PointNamePrefix);
        _target.IsClosed = EditorGUILayout.Toggle("Is Closed:", _target.IsClosed);
        _target.Force2D = EditorGUILayout.Toggle("Force 2D:", _target.Force2D);
        EditorGUILayout.BeginHorizontal();
        {
            Undo.RecordObject(_target, "Zero Transform Position");
            EditorGUILayout.PrefixLabel("Color:");
            _target.curveColor = EditorGUILayout.ColorField(_target.curveColor);
        }
        EditorGUILayout.EndHorizontal();
        _target.AvtoCorrectPivot = EditorGUILayout.Toggle("Avto correct pivot", _target.AvtoCorrectPivot);
        _target.AvtoCorrectLineOnRoad = EditorGUILayout.BeginToggleGroup("Smooth on road", _target.AvtoCorrectLineOnRoad);
        
        if (_target.AvtoCorrectLineOnRoad)
        {
            _target.AvtoCorrectLineOnRoadSegmentCount = EditorGUILayout.IntField("SegmentCount", _target.AvtoCorrectLineOnRoadSegmentCount);
            if(GUILayout.Button("Divide"))
                SmoothRoad(_target.AvtoCorrectLineOnRoadSegmentCount);
        }
        EditorGUILayout.EndToggleGroup();
        if (GUILayout.Button("Snap to road"))
        {
            XLinePathPoint[] _points = _target.GetComponentsInChildren<XLinePathPoint>();
            for (int i = 0; i < _points.Length; i++)
                _points[i].SnapToRoad();
        }

        EditorGUILayout.HelpBox("Curve Length: "+_target.Length.ToString("f2") + "\n\rPoint count: " + _target.GetPoints().Length, MessageType.Info);

        if (_target.inEditorClickModeOn)
            GUI.color = Color.yellow;
        if (GUILayout.Button("Create points"))
        {
            _target.inEditorClickModeOn = !_target.inEditorClickModeOn;
        }
        GUI.color = Color.white;
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Correct pivot to center BB"))
        {
            Undo.RecordObject(_target.transform, "Zero Transform Position");
            _target.CorrectPivot();
        }
        //div = EditorGUILayout.FloatField(div);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Smooth"))
        {
            #region smooth all
            _target.SetSmoothPoints();
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll(); 
            #endregion
        }
        if (GUILayout.Button("Break"))
        {
            #region break
            _target.SetBreakPoints();
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll(); 
            #endregion
        }
        if (GUILayout.Button("Corner"))
        {
            #region corner
            _target.SetCornerPoints();
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll(); 
            #endregion
        }
        GUILayout.EndHorizontal();

        if (_show_primitives)
            BeginGrayColor();
        if (GUILayout.Button("Primitives"))
            _show_primitives = !_show_primitives;
        EndColor();
        if (_show_primitives)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            radius = EditorGUILayout.FloatField("Radius", radius);
            circle_segments = EditorGUILayout.IntField("Segm", circle_segments);
            if (circle_segments < 3) circle_segments = 3;
            bool change = EditorGUI.EndChangeCheck();
            EditorGUILayout.EndVertical();
            if (change || GUILayout.Button("Circle", GUILayout.Height(36)))
            {
                #region smooth all
                ClearPoints();

                float step = (Mathf.PI * 2f) / circle_segments;

                for (int i = 0; i < circle_segments; i++)
                {
                    GameObject go = new GameObject("Point_" + i, typeof(XLinePathPoint));
                    go.transform.SetParent(_target.transform);
                    go.transform.localScale = Vector3.one;
                    go.transform.localPosition = new Vector3(Mathf.Sin(step * i) * radius, 0, Mathf.Cos(step * i) * radius);
                }

                _target.SetSmoothPoints(circle_segments <= 4 ? 2.5f : 3f);



                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
                #endregion
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            size = EditorGUILayout.FloatField("Size", size);
            if (GUILayout.Button("Quad"))
            {
                #region break
                ClearPoints();
                float step = Mathf.PI * 0.5f;
                float rad = new Vector2(size, size).magnitude;
                for (int i = 0; i < 4; i++)
                {
                    GameObject go = new GameObject("Point_" + i, typeof(XLinePathPoint));
                    go.transform.SetParent(_target.transform);
                    go.transform.localScale = Vector3.one;
                    go.transform.localPosition = new Vector3(Mathf.Sin(step * i + step * 0.5f) * rad, 0, Mathf.Cos(step * i+ step * 0.5f) * rad);
                }
                _target.SetBreakPoints();
                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
                #endregion
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            tri_radius = EditorGUILayout.FloatField("Triangle rad", tri_radius);
            if (GUILayout.Button("Triangle"))
            {
                #region corner
                ClearPoints();
                float step = (Mathf.PI * 2f) / 3f;
                for (int i = 0; i < 3; i++)
                {
                    GameObject go = new GameObject("Point_" + i, typeof(XLinePathPoint));
                    go.transform.SetParent(_target.transform);
                    go.transform.localScale = Vector3.one;
                    go.transform.localPosition = new Vector3(Mathf.Sin(step * i) * tri_radius, 0, Mathf.Cos(step * i) * tri_radius);
                }
                _target.SetBreakPoints();
                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
                #endregion
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        #region Info
        if (_show_info)
            BeginGrayColor();
        if (GUILayout.Button("Info"))
            _show_info = !_show_info;
        EndColor();
        if (_show_info)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            _foldout = EditorGUILayout.Foldout(_foldout, "Points Actions");
            EditorGUI.indentLevel--;
            if (_foldout)
            {
                XLinePathPoint[] _points = _target.GetComponentsInChildren<XLinePathPoint>();
                #region points
                for (int i = 0; i < _points.Length; i++)
                {
                    XLinePathPoint point = _points[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(25);
                    GUILayout.Label(string.Format("Point {0}  ({1})", i, point.gameObject.name));
                    GUILayout.FlexibleSpace();
                    XLinePathPoint pointBefore;
                    XLinePathPoint pointAfter;
                    if (GUILayout.Button("Smooth"))
                    {
                        if (i > 0)
                        {
                            pointBefore = _points[i - 1];
                            if (i < (_points.Length - 1))
                            {
                                pointAfter = _points[i + 1];
                            }
                            else
                            {
                                if (_target.IsClosed)
                                {
                                    pointAfter = _points[0];
                                }
                                else
                                {
                                    pointAfter = _points[i];
                                }

                            }
                        }
                        else if (i < (_points.Length - 1))
                        {
                            pointAfter = _points[i + 1];
                            if (i > 0)
                            {
                                pointBefore = _points[i - 1];
                            }
                            else
                            {
                                if (_target.IsClosed)
                                {
                                    pointBefore = _points[_points.Length - 1];
                                }
                                else
                                {
                                    pointBefore = _points[i];
                                }

                            }
                        }
                        else
                        {
                            pointBefore = _points[i - 1];
                            pointAfter = _points[i + 1];
                        }

                        Vector3 p1 = Vector3.Lerp(point.Pos, pointBefore.Pos, 1f / 3f);
                        Vector3 p2 = Vector3.Lerp(point.Pos, pointAfter.Pos, 1f / 3f);

                        Vector3 v = p2 - p1;
                        Vector3 v1 = point.Pos - p1;
                        Vector3 v2 = point.Pos - p2;
                        point.forwardPoint = point.transform.worldToLocalMatrix.MultiplyPoint(point.Pos + v.normalized * v2.magnitude);
                        point.backwardPoint = point.transform.worldToLocalMatrix.MultiplyPoint(point.Pos - v.normalized * v1.magnitude);
                        point.isSmooth = true;
                        EditorUtility.SetDirty(target);
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("Break"))
                    {
                        if (i > 0)
                        {
                            pointBefore = _points[i - 1];
                            if (i < (_points.Length - 1))
                            {
                                pointAfter = _points[i + 1];
                            }
                            else
                            {
                                if (_target.IsClosed)
                                {
                                    pointAfter = _points[0];
                                }
                                else
                                {
                                    pointAfter = _points[i];
                                }

                            }
                        }
                        else if (i < (_points.Length - 1))
                        {
                            pointAfter = _points[i + 1];
                            if (i > 0)
                            {
                                pointBefore = _points[i - 1];
                            }
                            else
                            {
                                if (_target.IsClosed)
                                {
                                    pointBefore = _points[_points.Length - 1];
                                }
                                else
                                {
                                    pointBefore = _points[i];
                                }

                            }
                        }
                        else
                        {
                            pointBefore = _points[i - 1];
                            pointAfter = _points[i + 1];
                        }
                        point.isSmooth = false;
                        point.BackwardPoint = Vector3.Lerp(point.Pos, pointBefore.Pos, 1f / 3f);
                        point.ForwardPoint = Vector3.Lerp(point.Pos, pointAfter.Pos, 1f / 3f);
                        EditorUtility.SetDirty(target);

                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("Corner"))
                    {
                        point.isSmooth = false;
                        point.isSmooth = false;
                        point.backwardPoint = point.forwardPoint = Vector3.zero;
                        EditorUtility.SetDirty(target);

                        SceneView.RepaintAll();
                    }
                    GUILayout.EndHorizontal();

                }
                #endregion
            }
            EditorGUI.indentLevel++;
            _seg_foldout = EditorGUILayout.Foldout(_seg_foldout, "Segments");
            if (_seg_foldout)
            {
                for (int i = 0; i < _target.Segments.Length; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.PrefixLabel("Start:");
                    EditorGUILayout.ObjectField(_target.Segments[i].Start, typeof(XLinePathPoint), true);
                    EditorGUILayout.PrefixLabel("End:");
                    EditorGUILayout.ObjectField(_target.Segments[i].End, typeof(XLinePathPoint), true);
                    GUILayout.Label("Length: " + _target.Segments[i].length);
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        #endregion

        if (GUILayout.Button("Reverse line directions"))
        {
            Undo.RegisterFullObjectHierarchyUndo(_target, _target.name);
            XLinePathPoint[] _points = _target.GetComponentsInChildren<XLinePathPoint>();
            Undo.RecordObjects(_points, "Reverse line directions");

            for (int i = 0; i < _points.Length / 2; i++)
            {
                int i2 = _points.Length - i - 1;
                _points[i].transform.SetSiblingIndex(i2);
                _points[i2].transform.SetSiblingIndex(i);
                var fwdpt = _points[i].forwardPoint;
                var bcdpt = _points[i].backwardPoint;
                _points[i].forwardPoint = bcdpt;
                _points[i].backwardPoint = fwdpt;
                fwdpt = _points[i2].forwardPoint;
                bcdpt = _points[i2].backwardPoint;
                _points[i2].forwardPoint = bcdpt;
                _points[i2].backwardPoint = fwdpt;
            }
        }

        if (GUI.changed)
        {
            _target.SetDirty();
            EditorUtility.SetDirty(target);
        }
    }

    void SmoothRoad(int divide)
    {
        Undo.RegisterFullObjectHierarchyUndo(_target, _target.name);
           var segs = _target.Segments;

        if (points.Count == 0)
        {
            XLinePathPoint[] pts = _target.GetComponentsInChildren<XLinePathPoint>(true);
            points.AddRange(System.Array.ConvertAll<XLinePathPoint, XLinePathSubLine.XSegment.XPoint>(pts, (val) => new XLinePathSubLine.XSegment.XPoint(val.Pos, val.ForwardPoint, val.BackwardPoint, val.isSmooth)));
        }

        List<XLinePathPoint> ps = new List<XLinePathPoint>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            float d = Vector3.Distance(points[i].Pos, points[i + 1].Pos);
            if (d > 60)
            {
                int count = (int)(d / 30f);
                float _divide = d / count;
                for (int i1 = 0; i1 < count; i1++)
                {
                    float t = (i1 * _divide) / d;
                    Vector3 p = segs[i].GetPoint(t, false);
                    Ray r = new Ray(p + Vector3.up * 3f, -Vector3.up);
                    RaycastHit hit;
                    if (Physics.Raycast(r, out hit))
                        ps.Add(CreatePoint(hit.point));
                }
            }
            else ps.Add(CreatePoint(points[i].Pos));
        }
        while (_target.transform.childCount > 0)
            DestroyImmediate(_target.transform.GetChild(0).gameObject);

        for (int i = 0; i < ps.Count; i++)
        {
            ps[i].transform.SetParent(_target.transform, true);
            ps[i].transform.SetSiblingIndex(i);
        }
    }

    XLinePathPoint CreatePoint(Vector3 pos)
    {
        GameObject point = new GameObject(string.Format("Point_{0}", _target.gameObject.GetComponentsInChildren<XLinePathPoint>().Length), typeof(XLinePathPoint));
        point.transform.position = pos;
        //point.transform.SetParent(_target.transform, true);
        Undo.RegisterCreatedObjectUndo(point, "Create curve point");

        return point.GetComponent<XLinePathPoint>();
    }

    void ClearPoints()
    {
        XLinePathPoint[] _points = _target.GetComponentsInChildren<XLinePathPoint>();
        for (int i = 0; i < _points.Length; i++)
        {
            DestroyImmediate(_points[i].gameObject);
        }
    }

    Color old_col;
    bool is_begin_color = false;
    void BeginGrayColor()
    {
        BeginColor(Color.gray);
    }

    void BeginColor(Color col)
    {
        is_begin_color = true;
           old_col = GUI.color;
        GUI.color = col;
    }

    void EndColor()
    {
        if(is_begin_color)
        GUI.color = old_col;
    }

    void OnSceneGUI()
    {
        if (GUI.changed)
            EditorUtility.SetDirty(target);
        if (!_target.inEditorClickModeOn)
            return;
        Event e = Event.current;
        Handles.BeginGUI();
        GUI.color = Color.white * 0.8f;
        EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
        EditorGUILayout.LabelField("CREATE POINTS MODE");
        EditorGUILayout.LabelField("RIGHT CLICK TURN OFF MODE");
        EditorGUILayout.EndVertical();
        GUI.color = Color.white;
        Handles.EndGUI();
        if (!Event.current.alt && !Event.current.control && !Event.current.shift && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            RaycastHit hit;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            try
            {
                hit = (RaycastHit)HandleUtility.RaySnap(ray);
                //if (hit.transform != null)
                {
                    //Undo.RegisterSceneUndo("Create curve Pont");
                    GameObject point = new GameObject(string.Format("Point_{0}", _target.gameObject.GetComponentsInChildren<XLinePathPoint>().Length), typeof(XLinePathPoint));
                    point.transform.position = hit.point;
                    point.transform.SetParent(_target.transform, true);
                    Undo.RegisterCreatedObjectUndo(point, "Create curve point");
                    //Undo.DestroyObjectImmediate(point);
                    EditorUtility.SetDirty(point);
                }
                Event.current.Use();
            }
            catch
            {
                Debug.Log("Filed to add Curve point.");
            }
        }
        else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            _target.inEditorClickModeOn = false;
    }
}