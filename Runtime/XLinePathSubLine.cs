using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("Chuvi/Line/XLinePathSubLine")]
public class XLinePathSubLine : MonoBehaviour
{

    public delegate void XLinePathSubLineChange(XLinePathSubLine subline);
    public event XLinePathSubLineChange OnLineChanged;
#if UNITY_EDITOR
    public virtual bool InEditorShowGizmos
    {
        get
        {
            if (Parent == null) return false;
            return Parent.editor_inEditorShowGizmos;
        }
    }
#endif
    [SerializeField]
    protected bool _isClosed = false;
    public bool IsClosed
    {
        get { return _isClosed; }
        set
        {
            recalcSegments |= _isClosed != value;
            //Debug.Log("set IsClosed: " + value);
            _isClosed = value;
        }
    }
    protected float _length = 1.0f;
    protected int parentPrecision = 50;
    public float Length
    {
        get { return _length; }
    }
    [SerializeField]
    public Color curveColor = Color.white;
    public string PointNamePrefix = "Point_";
    public bool AvtoCorrectPivot = false;
    public bool AvtoCorrectLineOnRoad = false;
    public int AvtoCorrectLineOnRoadSegmentCount = 10;
    public bool Force2D = false;

    public XLinePathSegment[] Segments
    {
        get 
        {
            if (points == null || points.Length == 0) return new XLinePathSegment[0];

            if (recalcSegments ||
#if UNITY_EDITOR
        parentPrecision != parent.editor_Precision || 
#endif
                _segments == null)
            {
                //CorrectPivot();
                if (parent == null) parent = GetComponentInParent<XLinePath>();
#if UNITY_EDITOR 
                parentPrecision = parent.editor_Precision; 
#endif
                _length = 0;
                _segments = new XLinePathSegment[IsClosed ? points.Length : points.Length - 1];
                for (int i = 0; i < _segments.Length; i++)
                {
                    if (!_isClosed)
                    {
                        _segments[i] = new XLinePathSegment(points[i], points[i + 1]);
                    }
                    else
                    {
                        _segments[i] = new XLinePathSegment(points[i], i + 1 < points.Length ? points[i + 1] : points[0]);
                    }
                    _segments[i].SegmentPartsCount = parentPrecision;
                    _segments[i].Recalculate(Force2D);
                    _length += _segments[i].length;
                }
                recalcSegments = false;
            }
            return _segments;
        }
    }
    protected XLinePath parent;
    public XLinePath Parent
    {
        get
        {
            if (parent == null) parent = GetComponentInParent<XLinePath>();
            return parent;
        }
    }
    protected XLinePathSegment[] _segments;
    protected XLinePathPoint[] points;
    protected bool _recalcSegments = true;
    protected bool recalcSegments
    {
        get => _recalcSegments;
        set
        {
            if (_recalcSegments == value) return;
            _recalcSegments = value;
        }
    }

    public bool IsDirty { get { return recalcSegments; } }

    void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        points = GetComponentsInChildren<XLinePathPoint>();
        parent = GetComponentInParent<XLinePath>();
        _segments = new XLinePathSegment[0];
        recalcSegments = true;
        if (points.Length == 0) return;

        if(AvtoCorrectPivot) CorrectPivot();
        Recalculate();
    }

    public static XSegment[] GetSegments(Vector3[] pts, bool IsClosed)
    {
        float _length = 0;
        XSegment[] _segments = new XSegment[IsClosed ? pts.Length : pts.Length - 1];
        for (int i = 0; i < _segments.Length; i++)
        {
            if (!IsClosed)
            {
                _segments[i] = new XSegment(pts[i], pts[i + 1]);
            }
            else
            {
                _segments[i] = new XSegment(pts[i], i + 1 < pts.Length ? pts[i + 1] : pts[0]);
            }
            _segments[i].SegmentPartsCount = 10;
            _segments[i].Recalculate();
            _length += _segments[i].length;
        }

        return _segments;
    }

    public static XSegment[] GetSegments(XLinePathPoint[] pts, bool IsClosed, bool IsSmooth)
    {
        float _length = 0;
        XSegment[] _segments = new XSegment[IsClosed ? pts.Length : pts.Length - 1];
        for (int i = 0; i < _segments.Length; i++)
        {
            if (!IsClosed)
            {
                _segments[i] = new XSegment(pts[i].Pos, pts[i].ForwardPoint, pts[i].BackwardPoint,
                    pts[i + 1].Pos, pts[i + 1].ForwardPoint, pts[i + 1].BackwardPoint, IsSmooth);
            }
            else
            {
                XLinePathPoint p2 = i + 1 < pts.Length ? pts[i + 1] : pts[0];
                _segments[i] = new XSegment(pts[i].Pos, pts[i].ForwardPoint, pts[i].BackwardPoint,
                 p2.Pos, p2.ForwardPoint, p2.BackwardPoint, IsSmooth);
            }
            _segments[i].SegmentPartsCount = 10;
            _segments[i].Recalculate();
            _length += _segments[i].length;
        }

        return _segments;
    }

    public virtual void Recalculate()
    {
        if (parent == null) parent = GetComponentInParent<XLinePath>(true);
        points = GetComponentsInChildren<XLinePathPoint>();
#if UNITY_EDITOR
        parentPrecision = parent.editor_Precision; 
#endif
        _length = 0;
        _segments = new XLinePathSegment[IsClosed ? points.Length : points.Length - 1];
        for (int i = 0; i < _segments.Length; i++)
        {
            if (!_isClosed)
            {
                if (!string.IsNullOrEmpty(PointNamePrefix)) points[i].name = PointNamePrefix + i;
                if (!string.IsNullOrEmpty(PointNamePrefix)) points[i + 1].name = PointNamePrefix + (i + 1);
                _segments[i] = new XLinePathSegment(points[i], points[i + 1]);
            }
            else
            {
                if (!string.IsNullOrEmpty(PointNamePrefix)) points[i].name = PointNamePrefix + i;
                _segments[i] = new XLinePathSegment(points[i], i + 1 < points.Length ? points[i + 1] : points[0]);
            }
            _segments[i].SegmentPartsCount = parentPrecision;
            _segments[i].Recalculate(Force2D);
            _length += _segments[i].length;
        }
        recalcSegments = false;
    }

    public void CorrectPivot()
    {
        Bounds b = GetBounds();
        Vector3 pos = transform.position;
        Vector3 offset = pos - b.center;

        Vector3[] pos_points = new Vector3[points.Length];
        for (int i = 0; i < pos_points.Length; i++)
        {
            pos_points[i] = points[i].transform.position;
        }

        transform.position -= offset;
        for (int i = 0; i < pos_points.Length; i++)
        {
            points[i].transform.position = pos_points[i];
        }
    }

    public void AddPoint(XLinePathPoint point)
    {
        List<XLinePathPoint> _pts = new List<XLinePathPoint>(points);
        _pts.Add(point);
        points = _pts.ToArray();
        recalcSegments = true;
    }
    internal void AddPoints(XLinePathPoint[] points)
    {
        List<XLinePathPoint> _pts = new List<XLinePathPoint>(points);
        _pts.AddRange(points);
        points = _pts.ToArray();
        recalcSegments = true;
    }

    public XLinePathPoint[] GetPoints()
    {
        points = GetComponentsInChildren<XLinePathPoint>();
        return points;
    }

    public Vector3 GetInterpolatedPoint(float dist)
    {
        if (recalcSegments)
        {
            Recalculate();
        }
        float prevlen = 0f;
        float len = 0f;
        while (dist >= Length)
            dist -= Length;

        while (dist < 0)
            dist += Length;


        for (int i = 0; i < Segments.Length; i++)
        {
            prevlen = len;
            len += Segments[i].length;
            if (dist <= len)
            {

                float t = (dist - prevlen) / Segments[i].length;
                return Segments[i].GetInterpolatedPoint(t, Force2D);

            }
        }
        return Segments[Segments.Length - 1].GetInterpolatedPoint(1, Force2D);
    }
    public float GetInterpolatedValues(float dist, out Vector3 pos, out Vector3 vel, out Vector3 acc, out Vector3 up)
    {
        if (recalcSegments)
        {
            Recalculate();
        }
        float prevlen = 0f;
        float len = 0f;
        //while (dist >= Length)
        //    dist -= Length;

        //while (dist < 0)
        //    dist += Length;
        dist = Mathf.Repeat(dist, Length);


        for (int i = 0; i < Segments.Length; i++)
        {
            prevlen = len;
            len += Segments[i].length;
            if (dist <= len)
            {

                float t = (dist - prevlen) / Segments[i].length;

                Segments[i].GetInterpolatedValues(t, Force2D, out pos, out vel, out acc, out up);
                return dist;
            }
        }
        Segments[Segments.Length - 1].GetInterpolatedValues(1, Force2D, out pos, out vel, out acc, out up);
        return dist;
    }

    public float GetInterpolatedValues(float dist, out Vector3 pos, out Vector3 vel, out Vector3 acc)
    {
        if (recalcSegments)
        {
            Recalculate();
        }
        float prevlen = 0f;
        float len = 0f;
        //while (dist >= Length)
        //    dist -= Length;

        //while (dist < 0)
        //    dist += Length;
        dist = Mathf.Repeat(dist, Length);


        for (int i = 0; i < Segments.Length; i++)
        {
            prevlen = len;
            len += Segments[i].length;
            if (dist <= len)
            {

                float t = (dist - prevlen) / Segments[i].length;

                Segments[i].GetInterpolatedValues(t, Force2D, out pos, out vel, out acc);
                return dist;
            }
        }
        Segments[Segments.Length - 1].GetInterpolatedValues(1, Force2D, out pos, out vel, out acc);
        return dist;
    }

    public float GetInterpolatedValues(float dist, out Vector3 pos, out Vector3 vel)
    {
        if (recalcSegments)
        {
            Recalculate();
        }
        float prevlen = 0f;
        float len = 0f;

        //while (dist >= Length)
        //    dist -= Length;

        //while (dist < 0)
        //    dist += Length;
        dist = Mathf.Repeat(dist, Length);


        for (int i = 0; i < Segments.Length; i++)
        {
            prevlen = len;
            len += Segments[i].length;
            if (dist <= len)
            {

                float t = (dist - prevlen) / Segments[i].length;

                Segments[i].GetInterpolatedValues(t, Force2D, out pos, out vel);
                return dist;
            }
        }
        Segments[Segments.Length - 1].GetInterpolatedValues(1, Force2D, out pos, out vel);
        return dist;
    }

    public Vector3[] GetCurvePoints()
    {
        List<Vector3> vectors = new List<Vector3>();
        //Init();

        if (parent == null) parent = GetComponentInParent<XLinePath>();
#if UNITY_EDITOR
        int steps = Segments.Length * parent.editor_Precision;
#else
        int steps = Segments.Length * 50; 
#endif
        float stepLength = Length / steps;

        for (int i = 0; i <= steps; i++)
        {
            float current = i * stepLength;
            vectors.Add(GetInterpolatedPoint(current));
        }

        if (!IsClosed)
            vectors[vectors.Count - 1] = Segments[Segments.Length - 1].GetInterpolatedPoint(1, Force2D);
        return vectors.ToArray();
    }

    public void Smooth()
    {
        XLinePathPoint[] _points = GetComponentsInChildren<XLinePathPoint>();
        for (int i = 0; i < _points.Length; i++)
        {
            XLinePathPoint point = _points[i];

            XLinePathPoint pointBefore;
            XLinePathPoint pointAfter;

            if (i > 0)
            {
                pointBefore = _points[i - 1];
                if (i < (_points.Length - 1))
                {
                    pointAfter = _points[i + 1];
                }
                else
                {
                    if (IsClosed)
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
                    if (IsClosed)
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

            if (point.isSmooth)
            {
                Vector3 v = p2 - p1;
                Vector3 v1 = point.Pos - p1;
                Vector3 v2 = point.Pos - p2;
                point.forwardPoint =
                    point.transform.worldToLocalMatrix.MultiplyPoint(point.Pos + v.normalized * v2.magnitude);
                point.backwardPoint =
                    point.transform.worldToLocalMatrix.MultiplyPoint(point.Pos - v.normalized * v1.magnitude);
            }
            else
            {
                point.BackwardPoint = Vector3.Lerp(point.Pos, pointBefore.Pos, 1f / 3f);
                point.ForwardPoint = Vector3.Lerp(point.Pos, pointAfter.Pos, 1f / 3f);
            }
        }
    }

    public void GetInterpolatedValuesEx(float dist, out Vector3 pos, out Vector3 vel)
    {
        float prevlen = 0f;
        float len = 0f;
        for (int i = 0; i < Segments.Length; i++)
        {
            prevlen = len;
            len += Segments[i].length;
            if (dist <= len)
            {
                float t = (dist - prevlen) / Segments[i].length;
                pos = Segments[i].GetInterpolatedPoint(t, Force2D);
                vel = Segments[i].GetInterpolatedVelocity(t, Force2D);

                return;
            }
        }

        Segments[Segments.Length - 1].GetInterpolatedValues(1, Force2D, out pos, out vel);
        if (vel == Vector3.zero)
            vel = Segments[Segments.Length - 1].GetInterpolatedVelocity(0.99f, Force2D);
    }

    public Vector3 GetInterpolatedPointEx(float dist)
    {
        float prevlen = 0f;
        float len = 0f;

        for (int i = 0; i < Segments.Length; i++)
        {
            prevlen = len;
            len += Segments[i].length;
            if (dist <= len)
            {

                float t = (dist - prevlen) / Segments[i].length;
                return Segments[i].GetInterpolatedPoint(t, Force2D);

            }
        }
        return Segments[Segments.Length - 1].GetInterpolatedPoint(1, Force2D);
    }

    public Vector3 GetInterpolatedPointExLocal(float dist)
    {
        float prevlen = 0f;
        float len = 0f;

        for (int i = 0; i < Segments.Length; i++)
        {
            prevlen = len;
            len += Segments[i].length;
            if (dist <= len)
            {

                float t = (dist - prevlen) / Segments[i].length;
                return Segments[i].GetInterpolatedPointLocal(t);

            }
        }
        return Segments[Segments.Length - 1].GetInterpolatedPointLocal(1);
    }

    public Vector3 GetPosition(bool _2D)
    {
        if (_2D) return GetComponent<RectTransform>().anchoredPosition;
        return transform.position;
    }

#if UNITY_EDITOR
    public void DrawSegmentGizmo(int subdivs, float gizmoPointRadius = 0.2f)
    {
        /*if (points == null || points.Length == 0) */
        points = GetComponentsInChildren<XLinePathPoint>();
        if (parent == null) parent = GetComponentInParent<XLinePath>();
        if (Segments == null || Segments.Length == 0) return;
        Color col = Gizmos.color;
        Gizmos.color = curveColor;
        for (int i = 0; i < Segments.Length; i++)
        {
            Segments[i].DrawSegmentGizmo(subdivs, gizmoPointRadius);
        }
        Gizmos.color = col;
    } 
#endif

    private Bounds GetBounds()
    {
        Bounds b = new Bounds();
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        //for (int i = 0; i < pointsForm.Count; i++)
        foreach (XLinePathPoint p in points)
        {
            min = Vector3.Min(min, p.transform.position);
            max = Vector3.Max(max, p.transform.position);
        }
        b.SetMinMax(min, max);
        //b.center = Vector3.zero;
        return b;
    }

    public void SetSmoothPoints(float div = 3f)
    {
        XLinePathPoint[] _points = GetComponentsInChildren<XLinePathPoint>();
        XLinePathPoint pointBefore;
        XLinePathPoint pointAfter;
        for (int i = 0; i < _points.Length; i++)
        {
            XLinePathPoint point = _points[i];
            if (i > 0)
            {
                pointBefore = _points[i - 1];
                if (i < (_points.Length - 1))
                {
                    pointAfter = _points[i + 1];
                }
                else
                {
                    if (IsClosed)
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
                    if (IsClosed)
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

            Vector3 p1 = Vector3.Lerp(point.Pos, pointBefore.Pos, 1f / div);
            Vector3 p2 = Vector3.Lerp(point.Pos, pointAfter.Pos, 1f / div);

            Vector3 v = p2 - p1;
            Vector3 v1 = point.Pos - p1;
            Vector3 v2 = point.Pos - p2;
            Vector3 fwd = point.Pos + v.normalized * v2.magnitude;//point.transform.worldToLocalMatrix.MultiplyPoint(point.Pos + v.normalized * v2.magnitude);
            Vector3 bwd = point.Pos - v.normalized * v1.magnitude;
            point.transform.forward = (fwd - point.transform.position).normalized;
            point.forwardPoint = point.transform.worldToLocalMatrix.MultiplyPoint(fwd);//point.transform.worldToLocalMatrix.MultiplyPoint(point.Pos + v.normalized * v2.magnitude);
            point.backwardPoint = point.transform.worldToLocalMatrix.MultiplyPoint(bwd);//point.transform.worldToLocalMatrix.MultiplyPoint(point.Pos - v.normalized * v1.magnitude);
            point.isSmooth = true;
        }
    }
    public void SetBreakPoints()
    {
        XLinePathPoint[] _points = GetComponentsInChildren<XLinePathPoint>();
        XLinePathPoint pointBefore;
        XLinePathPoint pointAfter;
        for (int i = 0; i < _points.Length; i++)
        {
            XLinePathPoint point = _points[i];
            if (i > 0)
            {
                pointBefore = _points[i - 1];
                if (i < (_points.Length - 1))
                {
                    pointAfter = _points[i + 1];
                }
                else
                {
                    if (IsClosed)
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
                    if (IsClosed)
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
        }
    }
    public void SetCornerPoints()
    {
        XLinePathPoint[] _points = GetComponentsInChildren<XLinePathPoint>();
        for (int i = 0; i < _points.Length; i++)
        {
            XLinePathPoint point = _points[i];
            point.isSmooth = false;
            point.isSmooth = false;
            point.backwardPoint = point.forwardPoint = Vector3.zero;
        }
    }

    public void SetDirty()
    {
        recalcSegments = true;
        OnLineChanged?.Invoke(this);
        ((IXLinePath)parent).OnSubLineChanged(this);
    }

    private void OnTransformChildrenChanged()
    {
        SetDirty();
    }


    public class XSegment
    {
        public class XPoint
        {
            public bool isSmooth = false;

            public Vector3 forwardPoint;
            public Vector3 backwardPoint;

            public Vector3 ForwardPoint
            {
                get { return this.forwardPoint; }
                set
                {
                    forwardPoint = value;
                    if (isSmooth)
                    {
                        float len = backwardPoint.magnitude;
                        backwardPoint = -forwardPoint.normalized * len;
                    }
                }
            }
            public Vector3 BackwardPoint
            {
                get { return backwardPoint; }
                set
                {
                    backwardPoint = value;
                    if (isSmooth)
                    {
                        float len = forwardPoint.magnitude;
                        forwardPoint = -backwardPoint.normalized * len;
                    }
                }
            }

            public Vector3 ForwardDir
            {
                get { return (ForwardPoint - Pos).normalized; }
            }
            public Vector3 BackwardDir
            {
                get { return (BackwardPoint - Pos).normalized; }
            }
            
            public Vector3 Pos { get; set; }

            public XPoint(Vector3 pos, Vector3 fwd, Vector3 bwd, bool _isSmooth)
            {
                Pos = pos;
                isSmooth = _isSmooth;
                ForwardPoint = fwd;
                BackwardPoint = bwd;
            }
        }

        public float length;
        /// <summary>
        /// Количество частей на которые разбит сегмент
        /// </summary>
        public int SegmentPartsCount = 100;

        private struct LengthSegment
        {
            public float T;
            public float len;
        }

        private LengthSegment[] _segments;

        public XPoint Start { get { return _start; } }
        public XPoint End { get { return _end; } }

        public Vector3 Direction { get { return _dir; } }
        public Vector3 InvDirection { get { return _invdir; } }

        private XPoint _start;
        private XPoint _end;
        private Vector3 _dir, _invdir;

        public XSegment(XPoint start, XPoint end)
        {
            _start = start;
            _end = end;
            _dir = (_end.Pos - _start.Pos).normalized;
            _invdir = (_start.Pos - _end.Pos).normalized;
        }
        public XSegment(Vector3 start_pos, Vector3 start_fwd, Vector3 start_bwd, Vector3 end_pos, Vector3 end_fwd, Vector3 end_bwd, bool _isSmooth)
        {
            _start = new XPoint(start_pos, start_fwd, start_bwd, _isSmooth);
            _end = new XPoint(end_pos, end_fwd, end_bwd, _isSmooth);
            _dir = (_end.Pos - _start.Pos).normalized;
            _invdir = (_start.Pos - _end.Pos).normalized;
        }
        public XSegment(Vector3 start_pos, Vector3 end_pos)
        {
            _start = new XPoint(start_pos, start_pos, start_pos, false);
            _end = new XPoint(end_pos, end_pos, end_pos, false);
            _dir = (_end.Pos - _start.Pos).normalized;
            _invdir = (_start.Pos - _end.Pos).normalized;
        }
        
        public Vector3 GetInterpolatedPoint(float t)
        {
            Vector3 p0 = _start.Pos;
            Vector3 p1 = _start.ForwardPoint;
            Vector3 p2 = _end.BackwardPoint;
            Vector3 p3 = _end.Pos;

            t = ConstantT(t);
            float t1 = 1.0f - t;
            float t12 = t1 * t1;
            float t2 = t * t;
            return t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;
        }
        public Vector3 GetInterpolatedVelocity(float t)
        {
            Vector3 p0 = _start.Pos;
            Vector3 p1 = _start.ForwardPoint;
            Vector3 p2 = _end.BackwardPoint;
            Vector3 p3 = _end.Pos;

            t = ConstantT(t);
            float tm1 = t - 1.0f;
            float tm12 = tm1 * tm1;
            float t2 = t * t;

            return 3 * ((p3 - p2) * t2 + (p1 - p0) * tm12 + (p1 - p2) * 2 * t * tm1);
        }
        public Vector3 GetInterpolatedAcceleration(float t)
        {
            Vector3 p0 = _start.Pos;
            Vector3 p1 = _start.ForwardPoint;
            Vector3 p2 = _end.BackwardPoint;
            Vector3 p3 = _end.Pos;

            t = ConstantT(t);
            float tm1 = t - 1.0f;

            return 6 * ((2 * p1 - p0 - p2) * tm1 + (p1 - 2 * p2 + p3) * t);
        }
        public void GetInterpolatedValues(float t, out Vector3 pos, out Vector3 vel, out Vector3 acc)
        {
            Vector3 p0 = _start.Pos;
            Vector3 p1 = _start.ForwardPoint;
            Vector3 p2 = _end.BackwardPoint;
            Vector3 p3 = _end.Pos;

            t = ConstantT(t);
            float t1 = 1.0f - t;
            float t12 = t1 * t1;
            float t2 = t * t;
            float tm1 = -t1;

            pos = t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;
            vel = 3 * ((p3 - p2) * t2 + (p1 - p0) * t12 + (p1 - p2) * 2 * t * tm1);
            acc = 6 * ((2 * p1 - p0 - p2) * tm1 + (p1 - 2 * p2 + p3) * t);
        }
        public void GetInterpolatedValues(float t, out Vector3 pos, out Vector3 vel)
        {
            Vector3 p0 = _start.Pos;
            Vector3 p1 = _start.ForwardPoint;
            Vector3 p2 = _end.BackwardPoint;
            Vector3 p3 = _end.Pos;

            t = ConstantT(t);
            float t1 = 1.0f - t;
            float t12 = t1 * t1;
            float t2 = t * t;
            float tm1 = -t1;

            pos = t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;

            vel = 3 * ((p3 - p2) * t2 + (p1 - p0) * t12 + (p1 - p2) * 2 * t * tm1);
        }
        public Vector3 GetPoint(float t)
        {
            Vector3 p0 = _start.Pos;
            Vector3 p1 = _start.ForwardPoint;
            Vector3 p2 = _end.BackwardPoint;
            Vector3 p3 = _end.Pos;

            float tinv = 1.0f - t;
            float tinv_pow = tinv * tinv;
            float t_pow = t * t;
            return tinv * tinv_pow * p0 + 3f * (t * tinv_pow * p1 + t_pow * tinv * p2) + t_pow * t * p3;
        }
        public void Recalculate()
        {
            float len = 0;
            _segments = new LengthSegment[SegmentPartsCount];
            int countSegments = (SegmentPartsCount - 1);
            _segments[0].len = 0;
            _segments[0].T = 0;
            for (int i = 1; i < SegmentPartsCount; i++)
            {
                float t1 = (i - 1) / (float)countSegments;
                float t2 = (i) / (float)countSegments;
                Vector3 p1 = GetPoint(t1);
                Vector3 p2 = GetPoint(t2);
                float len1 = (p2 - p1).magnitude;
                len += len1;
                _segments[i].len = len;
                _segments[i].T = t2;
            }

            length = len;
        }

        float ConstantT(float t)
        {
            float len = t * length;
            int a = 0;
            int b = _segments.Length - 1;

            if (len > _segments[b].len)
            {
                return t;
            }

            if (len < 0)
            {
                return 0;
            }

            while (a != b)
            {
                int c = (a + b) / 2;
                if (c == a)
                {
                    break;
                }
                if (c == b)
                {
                    a = b;
                    break;
                }

                if (len > _segments[c].len)
                {
                    a = c;
                }
                else
                {
                    b = c;
                }
            }

            float localLen = len - _segments[a].len;
            float localPartLen = _segments[b].len - _segments[a].len;
            float localT = (localLen / localPartLen);

            return (1 - localT) * _segments[a].T + localT * _segments[b].T;


        }
    }

}

public class XLinePathSubLines : IEnumerable<XLinePathSubLine>
{
    List<XLinePathSubLine> sublines = new List<XLinePathSubLine>();

    public XLinePathSubLine this[int subline]
    {
        get { return sublines[subline]; }
        set { sublines[subline] = value; }
    }
    public void AddPoint(int subline, XLinePathPoint point)
    {
        if (sublines.Count == subline)
            sublines.Add(new XLinePathSubLine());
        XLinePathSubLine sl = sublines[subline];
        sl.AddPoint(point);
    }
    public void AddPoints(int subline, XLinePathPoint[] points)
    {
        if (sublines.Count == subline)
            sublines.Add(new XLinePathSubLine());
        XLinePathSubLine sl = sublines[subline];
        sl.AddPoints(points);
    }

    public IEnumerator<XLinePathSubLine> GetEnumerator()
    {
        return sublines.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return sublines.GetEnumerator();
    }

    public static implicit operator XLinePathSubLines(XLinePathSubLine[] sublines)
    {
        XLinePathSubLines sl = new XLinePathSubLines();
        sl.sublines.AddRange(sublines);
        return sl;
    }

    public int Count { get { return sublines.Count; } }
}