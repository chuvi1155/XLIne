using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("Chuvi/Line/XLinePath")]
public class XLinePath : MonoBehaviour
{

    XLinePathSubLines sublines;
    public XLinePathSubLines Sublines
    {
        get
        {
            if(sublines == null)
                sublines = GetComponentsInChildren<XLinePathSubLine>();
            return sublines;
        }
    }
    public bool IsInit { get; private set; }

    
    void Start()
    {
        //if (!Application.isPlaying)
        //{
        //    XLinePathSubLine[] sl = GetComponentsInChildren<XLinePathSubLine>();
        //    if (sl == null || sl.Length == 0)
        //    {
        //        GameObject sl_go = new GameObject("Subline (1)", typeof(XLinePathSubLine));
        //        sl_go.transform.SetParent(transform);
        //        sl_go.transform.localScale = Vector3.one;
        //        sl_go.transform.localPosition = Vector3.zero;
        //    }
        //}
        Init();
    }

    public void Init()
    {
        sublines = GetComponentsInChildren<XLinePathSubLine>();
        for (int i = 0; i < Sublines.Count; i++)
        {
            Sublines[i].Init();
        }
        IsInit = true;
    }

    public Vector3 GetInterpolatedPoint(int subline, float dist)
    {
        XLinePathSubLine sl = Sublines[subline];
        return sl.GetInterpolatedPoint(dist);
    }

    public void GetInterpolatedValues(int subline, float dist, out Vector3 pos, out Vector3 vel, out Vector3 acc, out Vector3 up)
    {
        XLinePathSubLine sl = Sublines[subline];
        sl.GetInterpolatedValues(dist, out pos, out vel, out acc, out up);
    }
    public void GetInterpolatedValues(int subline, float dist, out Vector3 pos, out Vector3 vel, out Vector3 acc)
    {
        XLinePathSubLine sl = Sublines[subline];
        sl.GetInterpolatedValues(dist, out pos, out vel, out acc);
    }

    public void GetInterpolatedValues(int subline, float dist, out Vector3 pos, out Vector3 vel)
    {
        XLinePathSubLine sl = Sublines[subline];
        sl.GetInterpolatedValues(dist, out pos, out vel);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Указывает плавность сегментов (Editor only)
    /// </summary>
    public int Precision = 50;
    /// <summary>
    ///  (Editor only)
    /// </summary>
    public bool inEditorShowGizmos = true;
    /// <summary>
    ///  (Editor only)
    /// </summary>
    public float gizmoPointRadius = 0.2f;
    void OnDrawGizmos()
    {
        if (!inEditorShowGizmos)
            return;
        sublines = GetComponentsInChildren<XLinePathSubLine>();
        foreach (var item in Sublines)
        {
            item.DrawSegmentGizmo(Precision, gizmoPointRadius);
        }
        if (sublines.Count == 0)
            sublines = null;
    } 
#endif

    public Vector3[] GetCurvePoints(int subline)
    {
        XLinePathSubLine sl = Sublines[subline];
        return sl.GetCurvePoints();
    }

    public void Smooth(int subline)
    {
        Sublines[subline].Smooth();
    }

    public void GetInterpolatedValuesEx(int subline, float dist, out Vector3 pos, out Vector3 vel)
    {
        XLinePathSubLine sl = Sublines[subline];
        sl.GetInterpolatedValuesEx(dist, out pos, out vel);
    }

    public Vector3 GetInterpolatedPointEx(int subline, float dist)
    {
        XLinePathSubLine sl = Sublines[subline];
        return sl.GetInterpolatedPointEx(dist);
    }
}
