using UnityEngine;

public class XLinePathSegment
{
    public float length;
    /// <summary>
    /// Количество частей на которые разбит сегмент
    /// </summary>
    public int SegmentPartsCount = 100;

    private struct LengthSegment
    {
        public float T;
        /// <summary>
        /// current length on current time-point
        /// </summary>
        public float len;
    }

    private LengthSegment[] _segments;

    public XLinePathPoint Start { get { return _start; } }
    public XLinePathPoint End { get { return _end; } }

    public Vector3 Direction { get { return _dir; } }
    public Vector3 InvDirection { get { return _invdir; } }

    private XLinePathPoint _start;
    private XLinePathPoint _end;
    private Vector3 _dir, _invdir;

    public XLinePathSegment(XLinePathPoint start, XLinePathPoint end)
    {
        _start = start;
        _end = end;
        _dir = (_end.Pos - _start.Pos).normalized;
        _invdir = (_start.Pos - _end.Pos).normalized;
    }

    public Vector3 GetInterpolatedPointLocal(float t)
    {
        bool is2D = _start.ThisRectTransform != null;
        Vector3 p0 = is2D ? (Vector3)_start.Pos2D : _start.LocalPos;
        Vector3 p1 = is2D ? (Vector3)_start.WorldForwardPoint2D : _start.LocalForwardPoint;
        Vector3 p2 = is2D ? (Vector3)_end.WorldBackwardPoint2D : _end.LocalBackwardPoint;
        Vector3 p3 = is2D ? (Vector3)_end.Pos2D : _end.LocalPos;

        t = ConstantT(t);
        float t1 = 1.0f - t;
        float t12 = t1 * t1;
        float t2 = t * t;
        return t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;
    }

    public Vector3 GetInterpolatedPoint(float t, bool force2D = false)
    {
        Vector3 p0 = force2D ? (Vector3)_start.Pos2D : _start.Pos;
        Vector3 p1 = force2D ? (Vector3)_start.WorldForwardPoint2D : _start.WorldForwardPoint;
        Vector3 p2 = force2D ? (Vector3)_end.WorldBackwardPoint2D : _end.WorldBackwardPoint;
        Vector3 p3 = force2D ? (Vector3)_end.Pos2D : _end.Pos;

        t = ConstantT(t);
        float t1 = 1.0f - t;
        float t12 = t1 * t1;
        float t2 = t * t;
        return t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;
    }

    public Vector3 GetInterpolatedVelocity(float t, bool force2D = false)
    {
        Vector3 p0 = force2D ? (Vector3)_start.Pos2D : _start.Pos;
        Vector3 p1 = force2D ? (Vector3)_start.WorldForwardPoint2D : _start.WorldForwardPoint;
        Vector3 p2 = force2D ? (Vector3)_end.WorldBackwardPoint2D : _end.WorldBackwardPoint;
        Vector3 p3 = force2D ? (Vector3)_end.Pos2D : _end.Pos;

        t = ConstantT(t);
        float tm1 = t - 1.0f;
        float tm12 = tm1 * tm1;
        float t2 = t * t;

        return 3 * ((p3 - p2) * t2 + (p1 - p0) * tm12 + (p1 - p2) * 2 * t * tm1);
    }

    public Vector3 GetInterpolatedAcceleration(float t, bool force2D = false)
    {
        Vector3 p0 = force2D ? (Vector3)_start.Pos2D : _start.Pos;
        Vector3 p1 = force2D ? (Vector3)_start.WorldForwardPoint2D : _start.WorldForwardPoint;
        Vector3 p2 = force2D ? (Vector3)_end.WorldBackwardPoint2D : _end.WorldBackwardPoint;
        Vector3 p3 = force2D ? (Vector3)_end.Pos2D : _end.Pos;

        t = ConstantT(t);
        float tm1 = t - 1.0f;

        return 6 * ((2 * p1 - p0 - p2) * tm1 + (p1 - 2 * p2 + p3) * t);
    }

    public void GetInterpolatedValues(float t, bool force2D, out Vector3 pos, out Vector3 vel, out Vector3 right, out Vector3 up)
    {
        GetInterpolatedValues(t, force2D, false, out pos, out vel, out right, out up);
    }
    public void GetInterpolatedValues(float t, bool force2D, bool localPoints, out Vector3 pos, out Vector3 vel, out Vector3 right, out Vector3 up)
    {
        Vector3 p0 = force2D ? (Vector3)_start.Pos2D : localPoints ? _start.LocalPos : _start.Pos;
        Vector3 p1 = force2D ? (Vector3)_start.WorldForwardPoint2D : localPoints ? _start.LocalForwardPoint : _start.WorldForwardPoint;
        Vector3 p2 = force2D ? (Vector3)_end.WorldBackwardPoint2D : localPoints ? _end.LocalBackwardPoint : _end.WorldBackwardPoint;
        Vector3 p3 = force2D ? (Vector3)_end.Pos2D : localPoints ? _end.LocalPos : _end.Pos;


        if (p1 == p0 && p2 == p3) 
        {
            pos = Vector3.Lerp(p0, p3, t);
            vel = (p3 - p0).normalized;
            right = Vector3.Lerp(_start.ThisTransform.right, _end.ThisTransform.right, t);
            up = Vector3.Lerp(_start.ThisTransform.up, _end.ThisTransform.up, t);
            return;
        }
        t = ConstantT(t);

        float t1 = 1.0f - t;
        float t12 = t1 * t1;
        float t2 = t * t;
        float tm1 = -t1;

        pos = t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;

        vel = 3 * ((p3 - p2) * t2 + (p1 - p0) * t12 + (p1 - p2) * 2 * t * tm1);
        //vel = Vector3.Lerp(_start.transform.forward, _end.transform.forward, t);
        //acc = 6 * ((2 * p1 - p0 - p2) * tm1 + (p1 - 2 * p2 + p3) * t);
        right = Vector3.Lerp(_start.ThisTransform.right, _end.ThisTransform.right, t);
        up = Vector3.Lerp(_start.ThisTransform.up, _end.ThisTransform.up, t);
    }
    public void GetInterpolatedValues(float t, bool force2D, out Vector3 pos, out Vector3 vel, out Vector3 right)
    {
        Vector3 p0 = force2D ? (Vector3)_start.Pos2D : _start.Pos;
        Vector3 p1 = force2D ? (Vector3)_start.WorldForwardPoint2D : _start.WorldForwardPoint;
        Vector3 p2 = force2D ? (Vector3)_end.WorldBackwardPoint2D : _end.WorldBackwardPoint;
        Vector3 p3 = force2D ? (Vector3)_end.Pos2D : _end.Pos;

        t = ConstantT(t);
        float t1 = 1.0f - t;
        float t12 = t1 * t1;
        float t2 = t * t;
        float tm1 = -t1;

        pos = t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;

        vel = 3 * ((p3 - p2) * t2 + (p1 - p0) * t12 + (p1 - p2) * 2 * t * tm1);
        //vel = Vector3.Lerp(_start.transform.forward, _end.transform.forward, t);
        //acc = 6 * ((2 * p1 - p0 - p2) * tm1 + (p1 - 2 * p2 + p3) * t);
        right = Vector3.Lerp(_start.transform.right, _end.transform.right, t);
    }

    public void GetInterpolatedValues(float t, bool force2D, out Vector3 pos, out Vector3 vel)
    { 
        Vector3 p0 = force2D ? (Vector3)_start.Pos2D : _start.Pos;
        Vector3 p1 = force2D ? (Vector3)_start.WorldForwardPoint2D : _start.WorldForwardPoint;
        Vector3 p2 = force2D ? (Vector3)_end.WorldBackwardPoint2D : _end.WorldBackwardPoint;
        Vector3 p3 = force2D ? (Vector3)_end.Pos2D : _end.Pos;

        t = ConstantT(t);
        float t1 = 1.0f - t;
        float t12 = t1 * t1;
        float t2 = t * t;
        float tm1 = -t1;

        pos = t1 * t12 * p0 + 3f * (t * t12 * p1 + t2 * t1 * p2) + t2 * t * p3;

        vel = 3 * ((p3 - p2) * t2 + (p1 - p0) * t12 + (p1 - p2) * 2 * t * tm1);
    }

    public Vector3 GetPoint(float t, bool force2D)
    {
        Vector3 p0 = force2D ? (Vector3)_start.Pos2D : _start.Pos;
        Vector3 p1 = force2D ? (Vector3)_start.WorldForwardPoint2D : _start.WorldForwardPoint;
        Vector3 p2 = force2D ? (Vector3)_end.WorldBackwardPoint2D : _end.WorldBackwardPoint;
        Vector3 p3 = force2D ? (Vector3)_end.Pos2D : _end.Pos;

        float tinv = 1.0f - t;
        float tinv_pow = tinv * tinv;
        float t_pow = t * t;
        return tinv * tinv_pow * p0 + 3f * (t * tinv_pow * p1 + t_pow * tinv * p2) + t_pow * t * p3;
    }
    public Vector3 GetPoint2D(float t)
    {
        float t1 = 1.0f - t;
        float t12 = t1 * t1;
        float t2 = t * t;
        return t1 * t12 * _start.Pos2D + 3f * (t * t12 * _start.WorldForwardPoint2D + t2 * t1 * _end.WorldBackwardPoint2D) + t2 * t * _end.Pos2D;
    }
    public void Recalculate(bool force2D)
    {
        float len = 0;
        if (SegmentPartsCount == 1)
        {
            SegmentPartsCount = 2;
        }
        _segments = new LengthSegment[SegmentPartsCount];
        int countSegments = (SegmentPartsCount - 1);
        _segments[0].len = 0;
        _segments[0].T = 0;
        for (int i = 1; i < SegmentPartsCount; i++)
        {
            float t1 = (i - 1) / (float)countSegments;
            float t2 = (i) / (float)countSegments;
            Vector3 p1 = GetPoint(t1, force2D);
            Vector3 p2 = GetPoint(t2, force2D);
            float len1 = (p2 - p1).magnitude;
            len += len1;
            _segments[i].len = len;
            _segments[i].T = t2;
        }
        _start.IsDirty = false;
        _end.IsDirty = false;
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

#if UNITY_EDITOR
    public void DrawSegmentGizmo(int subdivs/*, float gizmoPointRadius = 0.2f*/)
    {
        if (_start == null || _end == null) return;

        for (int i = 0; i < subdivs; i++)
        {
            Vector3 p1 = GetPoint((float)i / subdivs, false);
            Vector3 p2 = GetPoint((i + 1f) / subdivs, false);
            /*if (gizmoPointRadius > 0)
            {
                Color col = Gizmos.color;
                Gizmos.color = Color.gray;
                Gizmos.DrawSphere(p1, gizmoPointRadius);
                Gizmos.DrawSphere(p2, gizmoPointRadius);
                Gizmos.color = col; 
            }*/
            Gizmos.DrawLine(p1, p2);
        }
    } 
#endif
}
