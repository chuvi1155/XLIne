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
    bool isDirty = false;
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

    bool IXLinePath.IsDirty 
    {
        get
        {
            var d = isDirty;
            isDirty = false;
            return d;
        }
        set => isDirty = value;
    }

    int IXLinePath.Precision =>
#if UNITY_EDITOR
        editor_Precision;
#else
        50;
#endif
    bool IXLinePath.InEditorShowGizmos =>
#if UNITY_EDITOR
        editor_inEditorShowGizmos;
#else 
        false;
#endif

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

    /// <summary>
    /// Get interpolated point from curve bezier 4 points
    /// </summary>
    /// <param name="t">Range 0-1</param>
    /// <param name="_start">Start point segment</param>
    /// <param name="_startFwd">Start point direction</param>
    /// <param name="_end">End point of segment</param>
    /// <param name="_endBcwd">End point inverted direction</param>
    /// <returns></returns>
    public static Vector3 Interpolate(float t, Vector3 _start, Vector3 _startFwdPt, Vector3 _endBcwdPt, Vector3 _end)
    {
        Vector3 p0 = _start;
        Vector3 p1 = _startFwdPt;
        Vector3 p2 = _endBcwdPt;
        Vector3 p3 = _end;

        float tinv = 1.0f - t;
        float tinv_pow = tinv * tinv;
        float t_pow = t * t;
        return tinv * tinv_pow * p0 + 3f * (t * tinv_pow * p1 + t_pow * tinv * p2) + t_pow * t * p3;
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
    [SerializeField] int editor_Precision = 50;
    /// <summary>
    ///  (Editor only)
    /// </summary>
    [SerializeField] bool editor_inEditorShowGizmos = true;
    ///// <summary>
    /////  (Editor only)
    ///// </summary>
    //[SerializeField] float editor_gizmoPointRadius = 0f;

    void OnDrawGizmos()
    {
        if (!editor_inEditorShowGizmos)
            return;
        sublines = GetComponentsInChildren<XLinePathSubLine>();
        foreach (var item in Sublines)
        {
            item.DrawSegmentGizmo(editor_Precision/*, editor_gizmoPointRadius*/);
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
                isDirty = true;
                return;
            }
        }
        isDirty = true;
    }
    public float GetLength(int sublineIndex)
    {
        return Sublines[sublineIndex].Length;
    }
}

