using UnityEngine;
using System.Collections;

[AddComponentMenu("Chuvi/Line/XLinePathPoint")]
public class XLinePathPoint : MonoBehaviour
{
    public bool isSmooth = true;

    [SerializeField] Vector3 localForwardPoint;
    [SerializeField] Vector3 localBackwardPoint;
    [SerializeField] Vector3 worldForwardPoint;
    [SerializeField] Vector3 worldBackwardPoint;

    private Transform _thisTransform;
    private RectTransform _thisRectTransform;
    private XLinePathSubLine _parentCurve;

    public void Awake()
    {
        _thisTransform = transform;
//#if UNITY_EDITOR
//        oldPos = _thisTransform.position;
//        oldRot = _thisTransform.localRotation; 
//#endif
    }

    private bool _isDirty = true;

    public bool IsDirty
    {
        get { return _isDirty; }
        set
        {
            _isDirty = value;
            if (ParentCurve != null && _isDirty)
            {
                worldForwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localForwardPoint);
                worldBackwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localBackwardPoint);
                ParentCurve.SetDirty();
            }
        }
    }

    public Vector3 LocalForwardPoint
    {
        get { return localForwardPoint; }
        //set
        //{
        //    if (localForwardPoint != value)
        //    {
        //        IsDirty = true;
        //        localForwardPoint = value;
        //        //worldForwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localForwardPoint);

        //        if (isSmooth)
        //        {
        //            float len = localBackwardPoint.magnitude;
        //            localBackwardPoint = -localForwardPoint.normalized * len;
        //           // worldBackwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localBackwardPoint);
        //        }
        //    }
        //}
    }
    public Vector3 LocalBackwardPoint
    {
        get { return localBackwardPoint; }
        //set
        //{
        //    if (localBackwardPoint != value)
        //    {
        //        IsDirty = true;
        //        localBackwardPoint = value;
        //        //worldBackwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localBackwardPoint);

        //        if (isSmooth)
        //        {
        //            float len = localForwardPoint.magnitude;
        //            localForwardPoint = -localBackwardPoint.normalized * len;
        //            //worldForwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localForwardPoint);
        //        }
        //    }
        //}
    }
    public Vector3 WorldForwardPoint
    {
        get { return worldForwardPoint; }
        set
        {
            if (worldForwardPoint != value)
            {
                IsDirty = true;
                worldForwardPoint = value;
                localForwardPoint = ThisTransform.worldToLocalMatrix.MultiplyPoint(value);

                if (isSmooth)
                {
                    float len = localBackwardPoint.magnitude;
                    localBackwardPoint = -localForwardPoint.normalized * len;
                    worldBackwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localBackwardPoint);
                }
            }
        }
    }
    public Vector3 WorldBackwardPoint
    {
        get { return worldBackwardPoint; }
        set
        {
            if (worldBackwardPoint != value)
            {
                IsDirty = true;
                worldBackwardPoint = value;
                localBackwardPoint = this.ThisTransform.worldToLocalMatrix.MultiplyPoint(value);

                if (isSmooth)
                {
                    float len = localForwardPoint.magnitude;
                    localForwardPoint = -localBackwardPoint.normalized * len;
                    worldForwardPoint = ThisTransform.localToWorldMatrix.MultiplyPoint(localForwardPoint);
                }
            }
        }
    }

    public Vector3 WorldForwardDir
    {
        get { return WorldForwardPoint - Pos; }
    }
    public Vector3 WorldBackwardDir
    {
        get { return WorldBackwardPoint - Pos; }
    }

    public Vector3 ForwardDir2D
    {
        get { return (WorldForwardPoint2D - Pos2D).normalized; }
    }
    public Vector3 BackwardDir2D
    {
        get { return (WorldBackwardPoint2D - Pos2D).normalized; }
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
    public Vector2 WorldForwardPoint2D
    {
        get
        {
            if (ThisRectTransform == null)
                _thisRectTransform = gameObject.AddComponent<RectTransform>();
            Vector2 p = RectTransformUtility.WorldToScreenPoint(null, WorldForwardPoint);
            Vector2 lp;
            RectTransform rtr = ThisRectTransform.parent.GetComponent<RectTransform>();
            if (rtr == null)
                rtr = ThisRectTransform.parent.gameObject.AddComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtr, p, null, out lp);
            return lp;
        }
    }
    public Vector2 WorldBackwardPoint2D
    {
        get
        {
            if (ThisRectTransform == null)
                _thisRectTransform = gameObject.AddComponent<RectTransform>();
            Vector2 p = RectTransformUtility.WorldToScreenPoint(null, WorldBackwardPoint);
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

    public Vector3 GetPos(bool _2D)
    {
        return _2D ? (Vector3)Pos2D : Pos;
    }
    public Vector3 GetForwardPoint(bool _2D)
    {
        return _2D ? (Vector3)WorldForwardPoint2D : WorldForwardPoint;
    }
    public Vector3 GetBackwardPoint(bool _2D)
    {
        return _2D ? (Vector3)WorldBackwardPoint2D : WorldBackwardPoint;
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

    public void SetBreak()
    {
        isSmooth = false;
    }

    public void SetCorner()
    {
        isSmooth = false;
        worldForwardPoint = Pos;
        worldBackwardPoint = Pos;
        localForwardPoint = Vector3.zero;
        localBackwardPoint = Vector3.zero;
    }

    public void ReverseDirections()
    {
        var fwdpt = worldForwardPoint;
        var bcdpt = worldBackwardPoint;
        worldForwardPoint = bcdpt;
        worldBackwardPoint = fwdpt;

        localForwardPoint = ThisTransform.worldToLocalMatrix.MultiplyPoint(worldForwardPoint);
        localBackwardPoint = ThisTransform.worldToLocalMatrix.MultiplyPoint(worldBackwardPoint);

        IsDirty = true;
    }

#if UNITY_EDITOR

    //Vector3 oldPos;
    //Quaternion oldRot;
    void OnDrawGizmos()
    {
        if (transform.parent == null) return;
        if (!_parentCurve)
            _parentCurve = transform.parent.GetComponent<XLinePathSubLine>();
        if (!_parentCurve || !((IXLinePath)_parentCurve.Parent).InEditorShowGizmos)
            return;
        Gizmos.color = Color.yellow * 2f;
        if (!ParentCurve.Force2D) Gizmos.DrawSphere(Pos, 0.4f);
        else Gizmos.DrawSphere(Pos, 2f);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(WorldForwardPoint, Pos);
        Gizmos.DrawLine(Pos, WorldBackwardPoint);

        //if (oldPos != transform.position || oldRot != transform.localRotation || IsDirty)
        //{
            //ParentCurve.SetDirty();
            //IsDirty = false;
        //}
        //oldPos = transform.position;
        //oldRot = transform.localRotation;
    } 
#endif
}
