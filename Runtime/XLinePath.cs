using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("Chuvi/Line/XLinePath")]
public class XLinePath : MonoBehaviour, IXLinePath
{
    public delegate void XLinePathChange(int sublineIndex, XLinePathSubLine subline);
    public event XLinePathChange OnLineChanged;

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

    public float GetInterpolatedValues(int subline, float dist, out Vector3 pos, out Vector3 vel, out Vector3 acc, out Vector3 up)
    {
        XLinePathSubLine sl = Sublines[subline];
        return sl.GetInterpolatedValues(dist, out pos, out vel, out acc, out up);
    }
    public float GetInterpolatedValues(int subline, float dist, out Vector3 pos, out Vector3 vel, out Vector3 acc)
    {
        XLinePathSubLine sl = Sublines[subline];
        return sl.GetInterpolatedValues(dist, out pos, out vel, out acc);
    }

    public float GetInterpolatedValues(int subline, float dist, out Vector3 pos, out Vector3 vel)
    {
        XLinePathSubLine sl = Sublines[subline];
        return sl.GetInterpolatedValues(dist, out pos, out vel);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Указывает плавность сегментов (Editor only)
    /// </summary>
    public int editor_Precision = 50;
    /// <summary>
    ///  (Editor only)
    /// </summary>
    public bool editor_inEditorShowGizmos = true;
    /// <summary>
    ///  (Editor only)
    /// </summary>
    public float editor_gizmoPointRadius = 0.2f;

    public XLinePathSubLine editor_changedSubLine;

    void OnDrawGizmos()
    {
        if (!editor_inEditorShowGizmos)
            return;
        sublines = GetComponentsInChildren<XLinePathSubLine>();
        foreach (var item in Sublines)
        {
            item.DrawSegmentGizmo(editor_Precision, editor_gizmoPointRadius);
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

    void IXLinePath.OnSubLineChanged(XLinePathSubLine subLine)
    {
        for (int i = 0; i < Sublines.Count; i++)
        {
            if(subLine == Sublines[i])
            {
                OnLineChanged?.Invoke(i, subLine);
#if UNITY_EDITOR
                editor_changedSubLine = subLine;
#endif
                return;
            }
        }
#if UNITY_EDITOR
        editor_changedSubLine = subLine;
#endif
    }
}
