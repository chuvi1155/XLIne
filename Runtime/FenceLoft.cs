using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("Chuvi/Line/_Modificators/FenceLoft")]
public class FenceLoft : SplineLoft
{
    public enum TypeColumn
    { 
        GameObject,
        Mesh
    }
    public enum TypeSetupColumn
    { 
        None,
        ByCount,
        ByDistance,
        Automatic
    }

    public enum SpanTypes
    {
        Scaled,
        Clone
    }

    public TypeSetupColumn SetupColumn = TypeSetupColumn.ByCount;
    public float DistanceBetwen = 0;
    public GameObject Column;
    public GameObject Span;
    public int CountColumn;
    public float Distance;
    public Vector3 OffsetColumn = Vector3.zero;
    public Vector3 ScaleColumn = Vector3.one;
    public Vector3 _Rotation;
    public TypeColumn typeColumn = TypeColumn.Mesh;
    public Material ColumnMat;
    public bool FixedColumnX = false;
    public bool FixedColumnY = false;
    public bool FixedColumnZ = false;

    public SpanTypes SpanType = SpanTypes.Scaled;
    /// <summary>
    /// если выбрана настройка SpanTypes.Clone, то устанавливает количество промежуточных, не масштабируемых пролетов
    /// </summary>
    public int CloneSpanCount = 1;
    public bool FixedSpanX = false;
    public bool FixedSpanY = false;
    public bool FixedSpanZ = false;
    public Vector3 OffsetSpan = Vector3.zero;
    public Vector3 _RotationSpan;
    public Vector3 ScaleSpan = Vector3.one;
    public Material SpanMat;

    public bool IsTemplate
    {
        get { return isTemplate; }
        set
        {
            if (isTemplate != value && !value)
            {
                GameObject formGO1 = GameObject.Find("Temp_Form_Present1");
                if (formGO1 != null)
                    DestroyImmediate(formGO1);
                GameObject formGO2 = GameObject.Find("Temp_Form_Present2");
                if (formGO2 != null)
                    DestroyImmediate(formGO2);
                CurveForm = temp_CurveForm;
            }
            isTemplate = value;
        }
    }
    public FencePreset template_preset = FencePreset.Preset1;

    public bool WithSpan
    {
        get { return Span != null; }
    }
    private Vector3 spanSize;
    //private GameObject span;
    private MeshFilter spanFilter;
    private bool isTemplate = false;
    private XLinePath temp_CurveForm;
    private bool hasCreateTemp = false;
    public enum FencePreset
    {
        Preset1,
        Preset2
    }

    private Mesh m_column, span_mesh, MainMesh;

    public override void Start()
    {
        base.Start();
    }

    public override void InitLoft(bool force = false)
    {
        if (Span != null)
        {
            spanFilter = Span.GetComponent<MeshFilter>();
            Matrix4x4 mat = spanFilter.transform.localToWorldMatrix;
            span_mesh = spanFilter.sharedMesh;
            Vector3[] verts = span_mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = mat * verts[i];
            }
            Bounds b = GetBounds(verts);
            spanSize = b.size; 
        }

        if (Column != null)
        {
            if (Column.GetComponent<MeshFilter>() != null)
                m_column = Column.GetComponent<MeshFilter>().sharedMesh;
        }

        if (IsTemplate)
        {
            switch (template_preset)
            {
                case FencePreset.Preset1:
                    CreateTempFormPresent1();
                    break;
                case FencePreset.Preset2:
                    CreateTempFormPresent2();
                    break;
            }
            hasCreateTemp = true;
        }

        base.InitLoft();

        //if (IsTemplate)
        //{
        //    GameObject formGO = GameObject.Find("Temp_Form");
        //    DestroyImmediate(formGO);
        //    CurveForm = temp_CurveForm;
        //}
    }

    private void CreateTempFormPresent2()
    {
        GameObject formGO1 = GameObject.Find("Temp_Form_Present1");
        if (formGO1 != null)
            DestroyImmediate(formGO1);
        if (temp_CurveForm != null)
        CurveForm = temp_CurveForm; 
        GameObject formGO = GameObject.Find("Temp_Form_Present2");
        if (formGO == null)
        {
            formGO = new GameObject("Temp_Form_Present2", typeof(XLinePath));
            Vector3 sz = Column.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            sz.x *= Column.transform.localScale.x;
            sz.y *= Column.transform.localScale.y;
            sz.z *= Column.transform.localScale.z;

            Vector3 h_sz = sz / 2f;

            float y = sz.y;
            for (int i = 0; i < 2; i++)
            {
                GameObject sublineformGO = new GameObject("line" + i, typeof(XLinePathSubLine));
                sublineformGO.transform.parent = formGO.transform;
                sublineformGO.transform.localPosition = new Vector3(0, y, 0);

                GameObject p1 = new GameObject("p1", typeof(XLinePathPoint));
                GameObject p2 = new GameObject("p2", typeof(XLinePathPoint));
                GameObject p3 = new GameObject("p3", typeof(XLinePathPoint));
                GameObject p4 = new GameObject("p4", typeof(XLinePathPoint));

                p1.transform.parent = sublineformGO.transform;
                p2.transform.parent = sublineformGO.transform;
                p3.transform.parent = sublineformGO.transform;
                p4.transform.parent = sublineformGO.transform;

                p1.GetComponent<XLinePathPoint>().isSmooth = false;
                p2.GetComponent<XLinePathPoint>().isSmooth = false;
                p3.GetComponent<XLinePathPoint>().isSmooth = false;
                p4.GetComponent<XLinePathPoint>().isSmooth = false;

                p1.transform.localPosition = new Vector3(-h_sz.x, h_sz.x, 0);
                p2.transform.localPosition = new Vector3(-h_sz.x, -h_sz.x, 0);
                p3.transform.localPosition = new Vector3(h_sz.x, -h_sz.x, 0);
                p4.transform.localPosition = new Vector3(h_sz.x, h_sz.x, 0);

                XLinePathSubLine sline = sublineformGO.GetComponent<XLinePathSubLine>();
                sline.IsClosed = true;
                y -= (sz.x + sz.x * 0.1f) * 2f;
            }

        }
        XLinePath line = formGO.GetComponent<XLinePath>();
        line.Init();

        temp_CurveForm = CurveForm;
        CurveForm = line;
    }

    private void CreateTempFormPresent1()
    {
        GameObject formGO2 = GameObject.Find("Temp_Form_Present2");
        if (formGO2 != null)
            DestroyImmediate(formGO2);
        if (temp_CurveForm != null)
            CurveForm = temp_CurveForm; 

        GameObject formGO = GameObject.Find("Temp_Form_Present1");
        if (formGO == null)
        {
            formGO = new GameObject("Temp_Form_Present1", typeof(XLinePath));
            Vector3 sz = Column.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            sz.x *= Column.transform.localScale.x;
            sz.y *= Column.transform.localScale.y;
            sz.z *= Column.transform.localScale.z;

            Vector3 h_sz = sz / 2f;

            float y = sz.y;
            for (int i = 0; i < 3; i++)
            {
                GameObject sublineformGO = new GameObject("line" + i, typeof(XLinePathSubLine));
                sublineformGO.transform.parent = formGO.transform;
                sublineformGO.transform.localPosition = new Vector3(0, y, 0);

                GameObject p1 = new GameObject("p1", typeof(XLinePathPoint));
                GameObject p2 = new GameObject("p2", typeof(XLinePathPoint));
                GameObject p3 = new GameObject("p3", typeof(XLinePathPoint));
                GameObject p4 = new GameObject("p4", typeof(XLinePathPoint));

                p1.transform.parent = sublineformGO.transform;
                p2.transform.parent = sublineformGO.transform;
                p3.transform.parent = sublineformGO.transform;
                p4.transform.parent = sublineformGO.transform;

                p1.GetComponent<XLinePathPoint>().isSmooth = false;
                p2.GetComponent<XLinePathPoint>().isSmooth = false;
                p3.GetComponent<XLinePathPoint>().isSmooth = false;
                p4.GetComponent<XLinePathPoint>().isSmooth = false;

                p1.transform.localPosition = new Vector3(-h_sz.x, h_sz.x, 0);
                p2.transform.localPosition = new Vector3(-h_sz.x, -h_sz.x, 0);
                p3.transform.localPosition = new Vector3(h_sz.x, -h_sz.x, 0);
                p4.transform.localPosition = new Vector3(h_sz.x, h_sz.x, 0);

                XLinePathSubLine sline = sublineformGO.GetComponent<XLinePathSubLine>();
                sline.IsClosed = true;
                y -= sz.x + sz.x * 0.1f;
            }

        }
        XLinePath line = formGO.GetComponent<XLinePath>();
        line.Init();

        temp_CurveForm = CurveForm;
        CurveForm = line;
    }

    public override void UpdateMesh()
    {
        if (IsTemplate)
        {
            switch (template_preset)
            {
                case FencePreset.Preset1:
                    CreateTempFormPresent1();
                    break;
                case FencePreset.Preset2:
                    CreateTempFormPresent2();
                    break;
            }
            hasCreateTemp = true;
        }
        if (CurveForm == null)
            return;
        base.UpdateMesh();
        Clear();
        if (filter.sharedMesh != null)
            GenerateColumn();

        hasCreateTemp = false;
        //if (IsTemplate)
        //{
        //    GameObject formGO = GameObject.Find("Temp_Form");
        //    DestroyImmediate(formGO);
        //    CurveForm = temp_CurveForm;
        //}
    }

    public void GenerateColumn()
    {
        if (Column == null)
            return;
        Vector3 position, velocity;
        int n = 0;
        if (typeColumn == TypeColumn.Mesh)
        {
            #region mesh column setting
            GameObject go = new GameObject("MeshColumn");
            go.transform.parent = gameObject.transform;
            MeshFilter filter = go.AddComponent<MeshFilter>();
            if (filter.sharedMesh == null)
                filter.sharedMesh = new Mesh();
            else filter.sharedMesh.Clear();
            filter.sharedMesh.name = "MainMesh";
            MainMesh = filter.sharedMesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            if (WithSpan)
                mr.sharedMaterials = new Material[] { ColumnMat, SpanMat };
            else
                mr.sharedMaterial = ColumnMat; 
            #endregion
        }

        for (int i3 = 0; i3 < CurvePath.Sublines.Count; i3++)
        {
            switch (SetupColumn)
            {
                case TypeSetupColumn.None:
                    break;
                case TypeSetupColumn.ByCount:
                    {
                        #region Methods
                        if (CountColumn <= 0)
                            return;
                        CombineInstance[] combines = new CombineInstance[CountColumn/** CurvePath.Sublines.Count*/];
                        CombineInstance[] span_combines = new CombineInstance[WithSpan ? CountColumn - 1 : 0];

                        float step = CurvePath.Sublines[i3].Length / (float)(CountColumn - 1);
                        Distance = step;
                        float pos = 0;
                        int nc = (CurvePath.Sublines[i3].IsClosed ? CountColumn - 1 : CountColumn);
                        Vector3 oldPos = Vector3.zero;
                        for (int i = 0; i < nc; i++)
                        {
                            pos = i * step;
                            CurvePath.GetInterpolatedValuesEx(i3, pos, out position, out velocity);
                            if (velocity == Vector3.zero)
                            {
                                //пробуем искать направление с начала
                                if (i == 0)
                                {
                                    Vector3 ppp;
                                    CurvePath.GetInterpolatedValuesEx(i3, (i + 1) * step, out ppp, out velocity);
                                }
                            }
                            if (typeColumn == TypeColumn.GameObject) CreateObject(i, position, velocity);
                            else combines[i] = CreateMeshColumns(i, position, velocity);

                            CreateSpanByCount(position, span_combines, oldPos, i, pos);
                            oldPos = position;
                        }

                        if (typeColumn == TypeColumn.Mesh)
                        {
                            #region combine mesh
                            if (WithSpan)
                            {
                                Mesh _temp1 = new Mesh();
                                _temp1.name = "_temp1_ByCount";
                                Mesh _temp2 = new Mesh();
                                _temp2.name = "_temp2_ByCount";
                                _temp1.CombineMeshes(combines, true, true);
                                _temp2.CombineMeshes(span_combines, true, true);
                                MainMesh.CombineMeshes(new CombineInstance[]
                                {
                                new CombineInstance
                                {
                                    mesh = _temp1,
                                },
                                new CombineInstance
                                {
                                    mesh = _temp2,
                                },
                                }, false, false);
                                DestroyImmediate(_temp1);
                                DestroyImmediate(_temp2);
                            }
                            else
                                MainMesh.CombineMeshes(combines, false, true);
                            #endregion
                        }
                        #endregion
                    }
                    break;
                case TypeSetupColumn.ByDistance:
                    {
                        #region Methods
                        if (Distance <= 0)
                            return;
                        float pos = 0;
                        List<CombineInstance> combines = new List<CombineInstance>();
                        List<CombineInstance> span_combines = new List<CombineInstance>();
                        Vector3 oldPos = Vector3.zero;
                        while (pos < CurvePath.Sublines[i3].Length)
                        {
                            CurvePath.GetInterpolatedValuesEx(i3, pos, out position, out velocity);
                            if (typeColumn == TypeColumn.GameObject) CreateObject(n, position, velocity);
                            else combines.Add(CreateMeshColumns(n, position, velocity));
                            pos += Distance;

                            CreateSpan(position, n, span_combines, oldPos);
                            oldPos = position;

                            n++;
                        }
                        if (typeColumn == TypeColumn.Mesh)
                        {
                            #region combine mesh
                            if (WithSpan)
                            {
                                Mesh _temp1 = new Mesh();
                                _temp1.name = "_temp1_ByDistance";
                                Mesh _temp2 = new Mesh();
                                _temp2.name = "_temp2_ByDistance";
                                _temp1.CombineMeshes(combines.ToArray(), true, true);
                                _temp2.CombineMeshes(span_combines.ToArray(), true, true);
                                MainMesh.CombineMeshes(new CombineInstance[]
                                {
                                new CombineInstance
                                {
                                    mesh = _temp1,
                                },
                                new CombineInstance
                                {
                                    mesh = _temp2,
                                },
                                }, false, false);
                                DestroyImmediate(_temp1);
                                DestroyImmediate(_temp2);
                            }
                            else
                                MainMesh.CombineMeshes(combines.ToArray(), true, true);
                            #endregion
                        }
                        CountColumn = n;
                        #endregion
                    }
                    break;
                case TypeSetupColumn.Automatic:
                    {
                        #region Methods
                        XLinePathSubLine sl = CurvePath.Sublines[i3];
                        if (sl.Segments.Length == 0) break;
                        List<CombineInstance> combines = new List<CombineInstance>();
                        List<CombineInstance> span_combines = new List<CombineInstance>();
                        Vector3 oldPos = Vector3.zero;
                        int c = 0;
                        for (int i = 0; i < sl.Segments.Length; i++)
                        {
                            position = sl.Segments[i].GetInterpolatedPoint(0);
                            velocity = sl.Segments[i].GetInterpolatedVelocity(0);
                            if (velocity == Vector3.zero)
                            {
                                //Debug.Log(i);
                                velocity = sl.Segments[i].GetInterpolatedVelocity(0.01f);
                            }
                            if (typeColumn == TypeColumn.GameObject) CreateObject(i, position, velocity);
                            else combines.Add(CreateMeshColumns(i, position, velocity));

                            if (WithSpan && i > 0)
                            {
                                Vector3 dir = position - oldPos;
                                Vector3 scSz = Span.transform.localScale;

                                if (spanSize.x * 0.9f < dir.magnitude)
                                    scSz.x = dir.magnitude / spanSize.x;
                                else if (spanSize.x * 1.1f > dir.magnitude)
                                    scSz.x = dir.magnitude / spanSize.x;

                                if (typeColumn == TypeColumn.GameObject) CreateSpanObject(i, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz);
                                else span_combines.Add(CreateMeshSpan(i, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz));
                            }
                            c++;
                            oldPos = position;
                            if (DistanceBetwen > 0)
                            {
                                n = (int)Math.Ceiling(sl.Segments[i].length / DistanceBetwen);
                                float step = sl.Segments[i].length / n;
                                for (int i1 = 1; i1 < n; i1++)
                                {
                                    float t = (i1 * step) / sl.Segments[i].length;
                                    sl.Segments[i].GetInterpolatedValues(t, false, out position, out velocity);
                                    if (velocity == Vector3.zero)
                                    {
                                        velocity = sl.Segments[i].GetInterpolatedVelocity((i1 * step) / sl.Segments[i].length - 0.001f);
                                    }
                                    if (typeColumn == TypeColumn.GameObject) CreateObject(i1, position, velocity);
                                    else combines.Add(CreateMeshColumns(i1, position, velocity));
                                    if (WithSpan && i1 > 0)
                                    {
                                        Vector3 dir = position - oldPos;
                                        Vector3 scSz = Span.transform.localScale;

                                        if (spanSize.x * 0.9f < dir.magnitude)
                                            scSz.x = dir.magnitude / spanSize.x;
                                        else if (spanSize.x * 1.1f > dir.magnitude)
                                            scSz.x = dir.magnitude / spanSize.x;

                                        if (typeColumn == TypeColumn.GameObject) CreateSpanObject(i1, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz);
                                        else span_combines.Add(CreateMeshSpan(i1, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz));
                                    }
                                    c++;
                                    oldPos = position;
                                }
                            }
                        }
                        if (!sl.IsClosed)
                        {
                            position = sl.Segments[sl.Segments.Length - 1].GetInterpolatedPoint(0.9999f);
                            velocity = sl.Segments[sl.Segments.Length - 1].GetInterpolatedVelocity(0.9999f);
                            if (typeColumn == TypeColumn.GameObject) CreateObject(c, position, velocity);
                            else combines.Add(CreateMeshColumns(c, position, velocity));

                            if (WithSpan && c > 0)
                            {
                                Vector3 dir = position - oldPos;
                                Vector3 scSz = Span.transform.localScale;

                                if (spanSize.x * 0.9f < dir.magnitude)
                                    scSz.x = dir.magnitude / spanSize.x;
                                else if (spanSize.x * 1.1f > dir.magnitude)
                                    scSz.x = dir.magnitude / spanSize.x;

                                if (typeColumn == TypeColumn.GameObject) CreateSpanObject(c, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz);
                                else span_combines.Add(CreateMeshSpan(c, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz));
                            }
                        }
                        else
                        {
                            if (WithSpan && c > 0)
                            {
                                Vector3 dir = sl.Segments[0].Start.Pos - oldPos;
                                Vector3 scSz = Span.transform.localScale;

                                if (spanSize.x * 0.9f < dir.magnitude)
                                    scSz.x = dir.magnitude / spanSize.x;
                                else if (spanSize.x * 1.1f > dir.magnitude)
                                    scSz.x = dir.magnitude / spanSize.x;

                                if (typeColumn == TypeColumn.GameObject) CreateSpanObject(c, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz);
                                else span_combines.Add(CreateMeshSpan(c, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz));
                            }
                        }

                        if (typeColumn == TypeColumn.Mesh)
                        {
                            #region combine mesh
                            if (WithSpan)
                            {
                                Mesh _temp1 = new Mesh();
                                _temp1.name = "_temp1_Automatic";
                                Mesh _temp2 = new Mesh();
                                _temp2.name = "_temp2_Automatic";
                                _temp1.CombineMeshes(combines.ToArray(), true, true);
                                _temp2.CombineMeshes(span_combines.ToArray(), true, true);
                                MainMesh.CombineMeshes(new CombineInstance[]
                                {
                                new CombineInstance
                                {
                                    mesh = _temp1,
                                },
                                new CombineInstance
                                {
                                    mesh = _temp2,
                                },
                                }, false, false);
                                DestroyImmediate(_temp1);
                                DestroyImmediate(_temp2);
                            }
                            else
                                MainMesh.CombineMeshes(combines.ToArray(), true, true);
                            #endregion
                        }
                        #endregion
                    }
                    break;
            } 
        }



        if (typeColumn == TypeColumn.Mesh)
            MainMesh.tangents = CalculateTangents(MainMesh.triangles, MainMesh.vertices, MainMesh.uv, MainMesh.normals);
    }

    private void CreateSpan(Vector3 position, int n, List<CombineInstance> span_combines, Vector3 oldPos)
    {
        if (WithSpan && n > 0)
        {
            Vector3 dir = position - oldPos;
            Vector3 scSz = Span.transform.localScale;

            if (spanSize.x * 0.9f < dir.magnitude)
                scSz.x = dir.magnitude / spanSize.x;
            else if (spanSize.x * 1.1f > dir.magnitude)
                scSz.x = dir.magnitude / spanSize.x;

            if (typeColumn == TypeColumn.GameObject) CreateSpanObject(n, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz);
            else span_combines.Add(CreateMeshSpan(n, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz));
        }
    }

    private void CreateSpanByCount(Vector3 position, CombineInstance[] span_combines, Vector3 oldPos, int i, float dist)
    {
        if (WithSpan && i > 0)
        {
            Vector3 dir = position - oldPos;
            Vector3 scSz = Span.transform.localScale;

            if (spanSize.x * 0.9f < dir.magnitude)
                scSz.x = dir.magnitude / spanSize.x;
            else if (spanSize.x * 1.1f > dir.magnitude)
                scSz.x = dir.magnitude / spanSize.x;

            if (SpanType == SpanTypes.Scaled)
            {
                if (typeColumn == TypeColumn.GameObject) CreateSpanObject(i, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz);
                else span_combines[i - 1] = CreateMeshSpan(i, oldPos + dir.normalized * (dir.magnitude / 2f), dir.normalized, scSz);
            }
            else if (SpanType == SpanTypes.Clone)
            {
                float step = scSz.x / (float)(CloneSpanCount + 1f);
                Vector3 _position, _velocity;
                List<CombineInstance> comb = new List<CombineInstance>();
                if (i == 1)
                {
                    for (int n = 0; n < CloneSpanCount; n++)
                    {
                        CurvePath.GetInterpolatedValuesEx(0, step * (n + 1), out _position, out _velocity);
                        if (typeColumn == TypeColumn.GameObject) CreateSpanObject(i + n + 1, _position, _velocity, scSz);
                        else comb.Add(CreateMeshSpan(i, _position, _velocity, scSz));
                    }
                }
                for (int n = 0; n < CloneSpanCount; n++)
                {
                    CurvePath.GetInterpolatedValuesEx(0, dist + step * (n + 1), out _position, out _velocity);
                    if (typeColumn == TypeColumn.GameObject) CreateSpanObject(i + n + 1, _position, _velocity, scSz);
                    else comb.Add(CreateMeshSpan(i, _position, _velocity, scSz));
                }
                span_combines[i - 1] = new CombineInstance();
                Mesh m = new Mesh();
                m.CombineMeshes(comb.ToArray(), true, true);
                span_combines[i - 1].mesh = m;
                span_combines[i - 1].transform = Matrix4x4.identity;
            }
        }
    }

    private void CreateObject(int n, Vector3 position, Vector3 velocity)
    {
        GameObject go = (GameObject)GameObject.Instantiate(Column);
        Vector3 fwd = go.transform.forward;
        if (!FixedColumnX) fwd.x = velocity.x;
        if (!FixedColumnY) fwd.y = velocity.y;
        if (!FixedColumnZ) fwd.z = velocity.z;
        fwd.Normalize();
        Quaternion look1 = Quaternion.LookRotation(fwd);

        go.name = Column.name + n.ToString();
        go.transform.parent = gameObject.transform;
        go.transform.position = position + look1 * OffsetColumn;
        // go.transform.forward = fwd;
        go.transform.rotation = look1;

        go.transform.Rotate(_Rotation);
        go.transform.localScale = ScaleColumn;
        if(ColumnMat != null)
            go.GetComponent<Renderer>().sharedMaterial = ColumnMat;
    }
    private void CreateSpanObject(int n, Vector3 position, Vector3 velocity, Vector3 sizeScale)
    {
        GameObject go = (GameObject)GameObject.Instantiate(Span);
        Vector3 fwd = go.transform.right;
        if (!FixedSpanX) fwd.x = velocity.x;
        if (!FixedSpanY) fwd.y = velocity.y;
        if (!FixedSpanZ) fwd.z = velocity.z;
        fwd.Normalize();
        Quaternion look1 = Quaternion.LookRotation(fwd) * Quaternion.Euler(new Vector3(0, 90, 0));

        go.name = Span.name + n.ToString();
        go.transform.parent = gameObject.transform;
        go.transform.position = position + look1 * OffsetSpan;
        go.transform.rotation = look1;

        go.transform.Rotate(_RotationSpan);
        go.transform.localScale = Mul(sizeScale, ScaleSpan);
        go.GetComponent<Renderer>().sharedMaterial = SpanMat;
    }

    private CombineInstance CreateMeshColumns(int subMesh, Vector3 position, Vector3 velocity)
    {
        CombineInstance inst = new CombineInstance();
        if (m_column == null) return inst;
        Vector3 fwd = Vector3.forward;
        if (!FixedColumnX) fwd.x = velocity.x;
        if (!FixedColumnY) fwd.y = velocity.y;
        if (!FixedColumnZ) fwd.z = velocity.z;
        fwd.Normalize();
        Quaternion look1 = Quaternion.LookRotation(fwd);
        inst.transform = Matrix4x4.TRS
            (
                //(position - gameObject.transform.position) + look1 * OffsetColumn, 
                position + look1 * OffsetColumn,
                look1 * Quaternion.Euler(_Rotation),
                ScaleColumn
             );
        inst.mesh = m_column;
        inst.subMeshIndex = 0;
        return inst;
    }
    private CombineInstance CreateMeshSpan(int subMesh, Vector3 position, Vector3 velocity, Vector3 sizeScale)
    {
        CombineInstance inst = new CombineInstance();
        if (span_mesh == null) return inst;
        Vector3 fwd = Span.transform.right;
        if (!FixedSpanX) fwd.x = velocity.x;
        if (!FixedSpanY) fwd.y = velocity.y;
        if (!FixedSpanZ) fwd.z = velocity.z;
        fwd.Normalize();
        Quaternion look1 = Quaternion.LookRotation(fwd) * Quaternion.Euler(new Vector3(0, 90, 0));
        //Vector3 sc = Mul(sizeScale, ScaleSpan);
        //Debug.Log(sc);
        inst.transform = Matrix4x4.TRS
            (
                //(position - gameObject.transform.position) + look1 * OffsetSpan,
                position + look1 * OffsetSpan,
                look1 * Quaternion.Euler(_RotationSpan),
                Mul(sizeScale, ScaleSpan)
            );
        inst.mesh = span_mesh;
        return inst;
    }

    Vector3 Mul(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public void Clear()
    {
		if(DestroyOnStart || Application.isEditor)
		{
	        int n = transform.childCount;
	
	        while (transform.childCount > 0)
	        {
	            DestroyImmediate(transform.GetChild(0).gameObject, false);
	            if (n == transform.childCount)
	                break;
	        }
	        DestroyImmediate(MainMesh, false);
		}
    }

    void OnDestroy()
    {
        if (CurvePath) Destroy(CurvePath.gameObject);
        if (CurveForm) Destroy(CurveForm.gameObject);
    }
}