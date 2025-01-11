using UnityEngine;
using System.Collections;

[AddComponentMenu("Chuvi/Line/XLinePathPoint")]
public class XLinePathPoint : MonoBehaviour
{
    public bool isSmooth = true;

    public Vector3 forwardPoint;
    public Vector3 backwardPoint;

    private Transform _thisTransform;
    private RectTransform _thisRectTransform;

    public void Awake()
    {
        _thisTransform = transform;
    }

    private bool _isDirty = true;

    public bool IsDirty
    {
        get { return _isDirty; }
        set
        {
            if(ParentCurve != null && _isDirty != value) ParentCurve.SetDirty();
            _isDirty = value;
        }
    }

    public Vector3 LocalForwardPoint
    {
        get { return forwardPoint; }
        set
        {
            Vector3 old = forwardPoint;
            forwardPoint = value;

            if (forwardPoint != old)
            {

                IsDirty = true;
            }

            if (isSmooth)
            {
                float len = backwardPoint.magnitude;
                backwardPoint = -forwardPoint.normalized * len;
            }
        }
    }
    public Vector3 LocalBackwardPoint
    {
        get { return backwardPoint; }
        set
        {
            Vector3 old = backwardPoint;
            backwardPoint = value;
            if (backwardPoint != old)
            {
                IsDirty = true;
            }
            if (isSmooth)
            {
                float len = forwardPoint.magnitude;
                forwardPoint = -backwardPoint.normalized * len;
            }
        }
    }
    public Vector3 ForwardPoint
    {
        get { return this.ThisTransform.localToWorldMatrix.MultiplyPoint(forwardPoint); }
        set
        {
            Vector3 old = forwardPoint;
            forwardPoint = ThisTransform.worldToLocalMatrix.MultiplyPoint(value);

            if (forwardPoint != old)
            {

                IsDirty = true;
            }

            if (isSmooth)
            {
                float len = backwardPoint.magnitude;
                backwardPoint = -forwardPoint.normalized * len;
            }
        }
    }
    public Vector3 BackwardPoint
    {
        get { return ThisTransform.localToWorldMatrix.MultiplyPoint(backwardPoint); }
        set
        {
            Vector3 old = backwardPoint;
            backwardPoint = this.ThisTransform.worldToLocalMatrix.MultiplyPoint(value);
            if (backwardPoint != old)
            {
                IsDirty = true;
            }
            if (isSmooth)
            {
                float len = forwardPoint.magnitude;
                forwardPoint = -backwardPoint.normalized * len;
            }
        }
    }

    public Vector3 ForwardDir
    {
        get { return ForwardPoint - Pos; }
    }
    public Vector3 BackwardDir
    {
        get { return BackwardPoint - Pos; }
    }

    public Vector3 ForwardDir2D
    {
        get { return (ForwardPoint2D - Pos2D).normalized; }
    }
    public Vector3 BackwardDir2D
    {
        get { return (BackwardPoint2D - Pos2D).normalized; }
    }

    public Vector3 LocalPos
    {
        get { return ThisTransform.localPosition; }
        set
        {
            if (ThisTransform.localPosition != value)
                ParentCurve.SetDirty();
            ThisTransform.localPosition = value;
        }
    }
    public Vector3 Pos
    {
        get { return ThisTransform.position; }
        set
        {
            if (ThisTransform.position != value)
                ParentCurve.SetDirty();
            ThisTransform.position = value;
        }
    }

    public Transform ThisTransform
    {
        get
        {
            if (_thisTransform == null)
                _thisTransform = transform;
            return _thisTransform;
        }
    }

    public RectTransform ThisRectTransform
    {
        get
        {
            if (_thisRectTransform == null)
                _thisRectTransform = GetComponent<RectTransform>();
            return _thisRectTransform;
        }
    }
    
    public Vector2 Pos2D
    {
        get
        {
            if (ThisRectTransform == null)
                _thisRectTransform = gameObject.AddComponent<RectTransform>();
            return ThisRectTransform.anchoredPosition;
        }
    }
    public Vector2 ForwardPoint2D
    {
        get
        {
            if (ThisRectTransform == null)
                _thisRectTransform = gameObject.AddComponent<RectTransform>();
            Vector2 p = RectTransformUtility.WorldToScreenPoint(null, ForwardPoint);
            Vector2 lp;
            RectTransform rtr = ThisRectTransform.parent.GetComponent<RectTransform>();
            if (rtr == null)
                rtr = ThisRectTransform.parent.gameObject.AddComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtr, p, null, out lp);
            return lp;
        }
    }
    public Vector2 BackwardPoint2D
    {
        get
        {
            if (ThisRectTransform == null)
                _thisRectTransform = gameObject.AddComponent<RectTransform>();
            Vector2 p = RectTransformUtility.WorldToScreenPoint(null, BackwardPoint);
            Vector2 lp;
            RectTransform rtr = ThisRectTransform.parent.GetComponent<RectTransform>();
            if (rtr == null)
                rtr = ThisRectTransform.parent.gameObject.AddComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtr, p, null, out lp);
            return lp;
        }
    }

    public XLinePathSubLine ParentCurve
    {
        get
        {
            if (!_parentCurve)
                _parentCurve = transform.parent.GetComponent<XLinePathSubLine>();
            return _parentCurve;
        }
    }
    private XLinePathSubLine _parentCurve;

    public Vector3 GetPos(bool _2D)
    {
        return _2D ? (Vector3)Pos2D : Pos;
    }
    public Vector3 GetForwardPoint(bool _2D)
    {
        return _2D ? (Vector3)ForwardPoint2D : ForwardPoint;
    }
    public Vector3 GetBackwardPoint(bool _2D)
    {
        return _2D ? (Vector3)BackwardPoint2D : BackwardPoint;
    }

    void OnDisable()
    {
        ParentCurve.SetDirty();
    }
    void OnDestroy()
    {
        ParentCurve.SetDirty();
    }

    public void SnapToRoad()
    {
        Ray r = new Ray(Pos + Vector3.up * 3f, -Vector3.up);
        RaycastHit hit;
        LayerMask roadLayer = LayerMask.NameToLayer("Road");
        if (Physics.Raycast(r, out hit,10000, 1 << roadLayer))
        {
            Debug.Log(hit.transform.gameObject.name, hit.transform);
            Pos = hit.point;
            //transform.up = hit.normal;
            transform.localRotation = Quaternion.LookRotation(transform.forward, hit.normal);
        }
        ParentCurve.SetDirty();
    }

#if UNITY_EDITOR

    Vector3 oldPos;
    Quaternion oldRot;
    void OnDrawGizmos()
    {
        if (transform.parent == null) return;
        if (!_parentCurve)
            _parentCurve = transform.parent.GetComponent<XLinePathSubLine>();
        if (!_parentCurve || !_parentCurve.InEditorShowGizmos)
            return;
        Gizmos.color = Color.yellow * 2f;
        if (!ParentCurve.Force2D) Gizmos.DrawSphere(Pos, ParentCurve.Parent.editor_gizmoPointRadius > 0 ? ParentCurve.Parent.editor_gizmoPointRadius * 4f : 0.4f);// Gizmos.DrawIcon(Pos, "dot", true);
        else Gizmos.DrawSphere(Pos, ParentCurve.Parent.editor_gizmoPointRadius * 2f);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(ForwardPoint, Pos);
        Gizmos.DrawLine(Pos, BackwardPoint);

        if (oldPos != transform.position || oldRot != transform.localRotation || IsDirty)
        {
            ParentCurve.SetDirty();
            IsDirty = false;
        }
        oldPos = transform.position;
        oldRot = transform.localRotation;
    } 
#endif
}
