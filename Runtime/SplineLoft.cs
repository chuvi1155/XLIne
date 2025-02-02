using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

#if UNITY_EDITOR
using UnityEditor; 
#endif

[AddComponentMenu("Chuvi/Line/_Modificators/SplineLoft")]
public class SplineLoft : MonoBehaviour
{
    public XLinePath CurveForm;
    public XLinePath CurvePath;
    public List<Material> material = new List<Material>();
    public int PathQuality = 10;
    public int FormQuality = 10;
    public bool isInitOK = false;
    public bool InvertFace = false;
    public MeshFilter filter;
    public Vector3 Offset = Vector3.zero;
    public float Scale = 1;
    public bool MirrorFormX, MirrorFormY, MirrorFormZ;
    public bool MirrorCopyForm;
    public bool mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ;
    public bool AddCollider = true;
    public bool IsSmooth = true;
    public Vector3 RotateForm;
    public bool ShowInEditor;
    public bool SetPathByPoints = false;
    public bool SetFormByPoints = false;
    public Vector2 Tiling = Vector2.one;
    /*[SerializeField] bool FixedX = false;
    [SerializeField] bool FixedY = false;
    [SerializeField] bool FixedZ = false;*/
    public bool mergeSubForms = true;
    //public bool brakeFormSegments = false;
    public bool brakePathSegments = false;
    public float brakePathSegmentOffset = 0;
    public bool DestroyOnStart = false;
    public bool UseCenterForm = false;
    public bool Form2D = false;
    public bool UseCanvasRenderer = false;

    private Vector3 multiFormOffset;
    [SerializeField, HideInInspector] private Vector2[] uv2;

    public virtual void Start()
    {
        if (CurveForm == null || CurvePath == null)
        {
            if (filter != null && filter.sharedMesh != null)
            {
                filter.sharedMesh.Clear();
                filter.sharedMesh = null;
            }
            if (UseCanvasRenderer)
            {
                CanvasRenderer _cr = GetComponent<CanvasRenderer>();
                if (_cr != null) _cr.Clear(); 
            }
            return;
        }
        isInitOK = filter.sharedMesh != null && filter.sharedMesh.vertexCount > 0;
        if (!isInitOK)
            DrawLoft();
        if (Application.isPlaying && DestroyOnStart)
        {
            Destroy(CurvePath.gameObject);
            Destroy(CurveForm.gameObject);
        }
    }

    public void DrawLoft()
    {
        if (isInitOK) UpdateMesh();
        else InitLoft();
    }
    public void ClearLoft()
    {
        Debug.Log("ClearLoft");
#if UNITY_EDITOR
        if(CurvePath != null)
            ((IXLinePath)CurvePath).IsDirty = false;
        if (CurveForm != null)
            ((IXLinePath)CurveForm).IsDirty = false;
#endif
        if (filter != null && filter.sharedMesh != null)
        {
            filter.sharedMesh.Clear();
            DestroyImmediate(filter.sharedMesh);
            filter.sharedMesh = null;
        }
        if (UseCanvasRenderer)
        {
            CanvasRenderer _cr = GetComponent<CanvasRenderer>();
            if (_cr != null) _cr.Clear(); 
        }
    }
    public virtual void InitLoft(bool force = false)
    {
        //Debug.Log("InitLoft");
        if (!isInitOK || force)
        {
            if (CurveForm == null || CurvePath == null)
            {
                ClearLoft();
                return;
            }
            CurveForm.Init();
            CurvePath.Init();

            filter = GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
                filter.sharedMesh = new Mesh();
                filter.sharedMesh.name = "filter.sharedMesh";
            }
            if (!UseCanvasRenderer)
            {
                if (GetComponent<MeshRenderer>() == null)
                    gameObject.AddComponent<MeshRenderer>();
            }
            else
            {
                var mr = GetComponent<MeshRenderer>();
                if (mr != null)
                    DestroyImmediate(mr);
            }
            UpdateMesh();
            if (AddCollider)
            {
                var mc = gameObject.GetComponent<MeshCollider>();
                if (mc == null)
                    mc = gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = filter.sharedMesh;
            }
            else
            {
                var col = GetComponent<MeshCollider>();
                if (col != null)
                    DestroyImmediate(col);
            }
            isInitOK = true;
        }
    }

    public virtual void UpdateMesh()
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        if (CurveForm == null || CurvePath == null)
        {
            if(filter.sharedMesh != null)
                filter.sharedMesh.Clear();
            filter.sharedMesh = null;
            if (UseCanvasRenderer)
            {
                CanvasRenderer _cr = GetComponent<CanvasRenderer>();
                if (_cr != null)
                {
                    _cr.Clear();
                    //DestroyImmediate(_cr);
                }
            }
            Debug.Log("UpdateMesh skip");
            return;
        }


        if (!CurveForm.IsInit) CurveForm.Init();
        if (!CurvePath.IsInit) CurvePath.Init();

        var mesh = filter.sharedMesh;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.MarkDynamic();
            mesh.name = "filter.sharedMesh";
        }
        if (UseCanvasRenderer && Form2D)
            mergeSubForms = false;

        GenerateLoft(mesh);
        mesh.UploadMeshData(false);
#if UNITY_EDITOR
        Unwrapping.GenerateSecondaryUVSet(mesh);
        uv2 = mesh.uv2;
#else
        if(uv2.Length > 0)
            mesh.uv2 = uv2;
#endif
        //mesh.RecalculateNormals();
        //mesh.RecalculateTangents();
        mesh.MarkModified();
        filter.sharedMesh = mesh;
        var mc = GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.convex = true;
            mc.sharedMesh = mesh;
            mc.convex = false;
        }
        
        if (Form2D && UseCanvasRenderer)
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                DestroyImmediate(mr);
            }
            CanvasRenderer cr = GetComponent<CanvasRenderer>();
            if (cr != null)
            {
                cr.Clear();
                //DestroyImmediate(cr);
            }
            if (cr == null)
                cr = gameObject.AddComponent<CanvasRenderer>();
            if (cr != null)
            {
                cr.materialCount = 0;
                cr.SetMesh(mesh);
                int n = 1;
                for (int i = 0; i < filter.sharedMesh.subMeshCount; i++)
                {
                    if (CurvePath.Sublines != null && i < CurvePath.Sublines.Count)
                    {
                        Material mat = material != null && material.Count > 0 && material[i % material.Count] != null ?
                            new Material(material[i % material.Count]) :
                            new Material(Shader.Find("Diffuse"));
                        mat.color = CurvePath.Sublines[i].curveColor;
                        cr.materialCount = n++;
                        cr.SetMaterial(mat, i);
                    }
                    else
                    {
                        Debug.LogWarning("Set material escape, CurvePath.Sublines is null or CurvePath.Sublines.Count < (subMeshCount)" + filter.sharedMesh.subMeshCount);
                    } 
                }
            }
        }
        else
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
            //mr.enabled = true;
            mr.sharedMaterials = material.ToArray();

            if (Form2D)
            {
                //mr.useLightProbes = false;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }

            int n = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (n < CurvePath.Sublines.Count)
                {
                    XLinePathSubLine sl = CurvePath.Sublines[n++];
                    while (sl.GetPoints().Length < 2 && n < CurvePath.Sublines.Count)
                    {
                        sl = CurvePath.Sublines[n++];
                    }
                    if (CurvePath.Sublines != null && sl.GetPoints().Length > 1)
                    {
                        Material mat = material != null && material.Count > 0 && material[i % material.Count] != null ?
                            new Material(material[i % material.Count]) :
                            new Material(Shader.Find("Diffuse"));
                        mat.color = sl.curveColor;
                        if(mr.sharedMaterial == null && (mr.sharedMaterials == null || mr.sharedMaterials.Length == 0))
                            mr.sharedMaterial = mat;
                    }
                    else
                    {
                        Debug.LogWarning("Set material escape, CurvePath.Sublines is null or CurvePath.Sublines.Count < (subMeshCount)" + mesh.subMeshCount);
                    }  
                }
            }
        }

        sw.Stop();
        Debug.Log("UpdateMesh " + sw.Elapsed);
    }
    
    private void GenerateLoft(Mesh res)
    {
        if (PathQuality < 1 || FormQuality < 1)
        {
            PathQuality = Mathf.Max(1, PathQuality);
            FormQuality = Mathf.Max(1, FormQuality);
            //return; 
        }
        if (res == null)
            return;
        if (CurveForm.Sublines == null || CurvePath.Sublines == null) return;
        res.Clear();
        CombineInstance[] combine = new CombineInstance[CurveForm.Sublines.Count * CurvePath.Sublines.Count];
        //multiFormOffset = Vector3.zero;
        Mesh[] _mesh = new Mesh[CurveForm.Sublines.Count * CurvePath.Sublines.Count];
        for (int i = 0; i < CurveForm.Sublines.Count; i++)
        {
            var sl_form = CurveForm.Sublines[i];
            for (int i1 = 0; i1 < CurvePath.Sublines.Count; i1++)
            {
                int meshIndx = i * CurvePath.Sublines.Count + i1;
                var sl_path = CurvePath.Sublines[i1];
                if (sl_form.Segments == null || sl_form.Segments.Length == 0 ||
                    sl_path.Segments == null || sl_path.Segments.Length == 0)
                    continue;
                sl_path.Force2D = Form2D;
                _mesh[meshIndx] = new Mesh();
                _mesh[meshIndx].name = "_mesh_combine";
                //if (i > 0)
                    //multiFormOffset += sl_form.GetPosition(Form2D) - CurveForm.Sublines[i - 1].GetPosition(Form2D);

                //if (!brakeFormSegments)
                //{
                if (sl_form.Length == 0)
                {
                    Debug.LogErrorFormat(sl_form, "Subline: {0}, have zero Length", sl_form.name);
                    continue;
                }
                float stepForm = sl_form.Length / FormQuality;
                if (stepForm == 0) continue;
                float[] uvx;
                // get form points
                var pointsForm = GetFormPoints(sl_form, stepForm, out uvx, MirrorFormX, MirrorFormY, MirrorFormZ, RotateForm, Scale);
                if (pointsForm.Length < 2) continue;
                // get step-segment size
                float stepPath = sl_path.Length / PathQuality;
                // move by path and create mesh
                SetMesh2(_mesh[meshIndx], stepPath, pointsForm, uvx, i1);

                if (MirrorCopyForm)
                {
                    Bounds b = GetBounds(pointsForm);
                    var copyForm = GetMirrorCopyForm(pointsForm, b.center, mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ);
                    if (pointsForm.Length < 2) continue;
                    // move by path and create mesh
                    var copyMesh = new Mesh();
                    var inv = InvertFace;

                    InvertFace = (!InvertFace && mirrorCopyByY);
                    SetMesh2(copyMesh, stepPath, copyForm, uvx, i1);
                    InvertFace = inv;

                    CombineInstance[] c_copy = new CombineInstance[2];
                    c_copy[0] = new CombineInstance();
                    c_copy[0].mesh = _mesh[meshIndx];
                    c_copy[1] = new CombineInstance();
                    c_copy[1].mesh = copyMesh;

                    var m = new Mesh();
                    m.CombineMeshes(c_copy, true, false);
                    
                    DestroyImmediate(_mesh[meshIndx]);
                    _mesh[meshIndx] = m;
                }

                // mesh for one subline path
                combine[meshIndx] = new CombineInstance();
                combine[meshIndx].mesh = _mesh[meshIndx];
                //}
                /*else
                {
                    CombineInstance[] _combine = new CombineInstance[sl_form.Segments.Length];
                    Mesh[] _m_temp = new Mesh[sl_form.Segments.Length];
                    for (int n = 0; n < sl_form.Segments.Length; n++)
                    {
                        _m_temp[n] = new Mesh();
                        XLinePathSegment seg = sl_form.Segments[n];
                        float stepForm = seg.length / FormQuality;
                        if (stepForm == 0) continue;
                        float[] uvx;
                        Vector3 pos = sl_form.GetPosition(Form2D);
                        List<Vector3> pointsForm = GetFormOptimizedPoints(seg, pos, stepForm, out uvx, MirrorFormX, MirrorFormY, MirrorFormZ, RotateForm, Scale);
                        if (pointsForm.Count < 2) continue;
                        float stepPath = sl_path.Length / PathQuality;
                        _m_temp[n].name = "_m_combine_" + n;
                        SetMesh(_m_temp[n], stepPath, pointsForm, uvx, i1);
                        _combine[n] = new CombineInstance();
                        _combine[n].mesh = _m_temp[n];
                    }
                    _mesh[meshIndx].CombineMeshes(_combine, true, false);
                    foreach (var __m in _m_temp)
                        DestroyImmediate(__m);
                    combine[meshIndx] = new CombineInstance();
                    //combine[meshIndx].subMeshIndex = meshIndx;
                    combine[meshIndx].mesh = _mesh[meshIndx];

                } */
            }
        }
        List<CombineInstance> list = new List<CombineInstance>(combine);
        list.RemoveAll(val => val.mesh == null);
        combine = list.ToArray();
        if (combine.Length > 0)
        {
            if (!mergeSubForms)
                res.subMeshCount = combine.Length;
            res.CombineMeshes(combine, mergeSubForms, false);
            if (!Form2D) res.tangents = CalculateTangents(res.triangles, res.vertices, res.uv, res.normals);
            else
            {
                Color32[] cols = new Color32[res.vertexCount];
                Color32 white = Color.white;
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i] = white;
                }
                res.colors32 = cols;
            }
            foreach (var __m in _mesh)
                DestroyImmediate(__m);
        }
        else
        {
            res.Clear();
            foreach (var __m in _mesh)
                DestroyImmediate(__m);
        }
    }

    private void SetMesh2(Mesh res, float step, Vector3[] pointsForm, float[] uvx, int path_sublineIndx)
    {
        var sl = CurvePath.Sublines[path_sublineIndx];

        Vector3 pathPos = Form2D ? (Vector3)GetComponent<RectTransform>().anchoredPosition : transform.position;
        Vector3[][] formFrameVerts = new Vector3[PathQuality + 1][];
        Vector2[][] formFrameUVs = new Vector2[PathQuality + 1][];
        float[] formFrameLengths = new float[pointsForm.Length];
        for (int i = 0; i < PathQuality + 1; i++)
        {
            formFrameVerts[i] = new Vector3[pointsForm.Length];
            formFrameUVs[i] = new Vector2[pointsForm.Length];

            sl.GetInterpolatedValues(step * i, out var pos, out var vel, out var acc, out var up);
            var mat = Matrix4x4.TRS(pos - pathPos, Quaternion.LookRotation(vel.normalized, up), Vector3.one);

            for (int n = 0; n < pointsForm.Length; n++)
            {
                formFrameVerts[i][n] = mat.MultiplyPoint(pointsForm[n]);
                if (i == 0)
                    formFrameUVs[i][n] = new Vector2(uvx[n], 0); // temp by Y set distance between previews point
                else
                {
                    var d = Vector3.Distance(formFrameVerts[i][n], formFrameVerts[i - 1][n]); // calculate full length by each form pointline
                    formFrameLengths[n] += d;
                    formFrameUVs[i][n] = new Vector2(uvx[n], formFrameLengths[n]); // temp by Y set distance between previews point
                }
            }
        }

        for (int i = 0; i < PathQuality + 1; i++)
        {
            for (int n = 0; n < pointsForm.Length; n++)
            {
                var uv = formFrameUVs[i][n];
                uv.y = uv.y / (formFrameLengths[n] / Tiling.y);
                formFrameUVs[i][n] = uv;
            }
        }

        List<Vector3> verts = new List<Vector3>();
        List<int>[][] normals = new List<int>[PathQuality + 1][];
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> uvs1 = new List<Vector2>();
        List<int> indx = new List<int>();
        for (int i = 0; i < PathQuality; i++)
        {
            if (IsSmooth)
            {
                if (normals[i] == null)
                    normals[i] = new List<int>[pointsForm.Length];
                if (normals[i + 1] == null)
                    normals[i + 1] = new List<int>[pointsForm.Length]; 
            }

            for (int n = 0; n < pointsForm.Length - 1; n++)
            {

                verts.AddRange(new Vector3[]
                {
                    formFrameVerts[i][n],
                    formFrameVerts[i][n + 1],
                    formFrameVerts[i + 1][n + 1],
                    formFrameVerts[i + 1][n]
                });

                if (IsSmooth)
                {
                    List<int> normalIndexes_n = normals[i][n] == null ? new List<int>() : normals[i][n];
                    if (normals[i][n] == null) normals[i][n] = normalIndexes_n;
                    List<int> normalIndexes_n1 = normals[i][n + 1] == null ? new List<int>() : normals[i][n + 1];
                    if (normals[i][n + 1] == null) normals[i][n + 1] = normalIndexes_n1;
                    List<int> next_normalIndexes_n = normals[i + 1][n] == null ? new List<int>() : normals[i + 1][n];
                    if (normals[i + 1][n] == null) normals[i + 1][n] = next_normalIndexes_n;
                    List<int> next_normalIndexes_n1 = normals[i + 1][n + 1] == null ? new List<int>() : normals[i + 1][n + 1];
                    if (normals[i + 1][n + 1] == null) normals[i + 1][n + 1] = next_normalIndexes_n1;
                    normalIndexes_n.Add(verts.Count - 4);
                    normalIndexes_n1.Add(verts.Count - 3);
                    next_normalIndexes_n1.Add(verts.Count - 2);
                    next_normalIndexes_n.Add(verts.Count - 1); 
                }

                uvs.AddRange(new Vector2[]
                {
                    formFrameUVs[i][n],
                    formFrameUVs[i][n + 1],
                    formFrameUVs[i + 1][n + 1],
                    formFrameUVs[i + 1][n]
                });
                uvs1.AddRange(new Vector2[]
                {
                    formFrameUVs[i][n],
                    formFrameUVs[i][n + 1],
                    formFrameUVs[i + 1][n + 1],
                    formFrameUVs[i + 1][n]
                });

                /*Vector3 norm;
                 norm = GetPolygonNormal(verts[verts.Count - 4], verts[verts.Count - 3], verts[verts.Count - 2], verts[verts.Count - 1]);

                 normals.Add(norm);
                 normals.Add(norm);
                 normals.Add(norm);
                 normals.Add(norm);*/

                if (InvertFace)
                {
                    indx.AddRange(new int[]
                    {
                        verts.Count - 4, //0
                        verts.Count - 3, //1
                        verts.Count - 2, //2

                        verts.Count - 2, //2
                        verts.Count - 1, //3
                        verts.Count - 4  //0
                    });
                }
                else
                {
                    indx.AddRange(new int[]
                    {
                        verts.Count - 4, //0
                        verts.Count - 1, //3
                        verts.Count - 2, //2

                        verts.Count - 2, //2
                        verts.Count - 3, //1
                        verts.Count - 4  //0
                    });
                }
            }
        }

        res.SetVertices(verts);
        res.SetUVs(0, uvs);
        //Debug.Log(string.Join(" ", uvs));
        //res.SetUVs(1, uvs1);
        res.SetIndices(indx.ToArray(), MeshTopology.Triangles, 0);

        if (IsSmooth)
        {
            res.RecalculateNormals();
            var norms = res.normals;

            foreach (var line in normals)
            {
                foreach (var points in line)
                {
                    Vector3 norm = Vector3.zero;
                    foreach (var pt in points)
                    {
                        norm += norms[pt];
                    }
                    norm.Normalize();
                    foreach (var pt in points)
                    {
                        norms[pt] = norm;
                    }
                }
            }

            res.normals = norms;
        }
        else
            res.RecalculateNormals();
        res.RecalculateTangents();
        res.RecalculateBounds();
    }
    
    private Vector3[] GetFormPoints(XLinePathSubLine sl, float step, out float[] uvx, bool MirrorFormX, bool MirrorFormY, bool MirrorFormZ, Vector3 RotateForm, float scale)
    {
        List<Vector3> verts = new List<Vector3>(FormQuality);
        Quaternion quat = Quaternion.Euler(RotateForm);
        //int count = SetFormByPoints ? sl.GetPoints().Length : FormQuality + 1;
        //TODO доделать возвращение точек если детализация формы отключена
        if (!SetFormByPoints)
        {
            uvx = new float[FormQuality + 1];
            for (int i = 0; i < FormQuality + 1; i++)
            {
                uvx[i] = (step * i) / (sl.Length / Tiling.x);
                sl.GetInterpolatedValues(step * i, out var pos, out var _, out var _, out var _);
                pos = sl.transform.InverseTransformPoint(pos + Offset);
                pos.x *= MirrorFormX ? -1 : 1;
                pos.y *= MirrorFormY ? -1 : 1;
                pos.z *= MirrorFormZ ? -1 : 1;
                //var mat = Matrix4x4.TRS(pos, Quaternion.LookRotation(vel.normalized, up), Vector3.one);
                verts.Add((quat * pos) * scale);
            }
        }
        else
        {
            float len = 0;
            var _points = sl.GetPoints();
            int count = sl.IsClosed ? _points.Length + 1 : _points.Length;
            uvx = new float[count];
            for (int i = 0; i < count; i++)
            {
                var p = (i < _points.Length ? _points[i].LocalPos : _points[0].LocalPos) + Offset;

                p.x *= MirrorFormX ? -1 : 1;
                p.y *= MirrorFormY ? -1 : 1;
                p.z *= MirrorFormZ ? -1 : 1;

                p = quat * p;
                p *= scale;
                verts.Add(p);

                uvx[i] = (len / (sl.Length / Tiling.x));
                len += i < _points.Length - 1 ? sl.Segments[i].length : 0;
            } 
        }

        return verts.ToArray();
    }

//    private void SetMesh(Mesh res, float step, List<Vector3> pointsForm, float[] uvx, int path_sublineIndx)
//    {
//#if UNITY_EDITOR
//        g_list.Clear();
//#endif
//        List<Vector3> verts = new List<Vector3>();
//        List<Vector2> uvs = new List<Vector2>();
//        List<Vector2> uvs1 = new List<Vector2>();
//        List<Vector3> normals = new List<Vector3>();
//        List<int> indx = new List<int>();
//        // двигаемся по пути
//        if (brakePathSegments)
//        {
//            if (!SetPathByPoints) SetInterplatedSeg(step, pointsForm, uvx, verts, uvs, uvs1, normals, indx, path_sublineIndx);
//            else SetNotInterplatedSeg(pointsForm, uvx, verts, uvs, uvs1, normals, indx, path_sublineIndx); 
//        }
//        else
//        {
//            if (!SetPathByPoints) SetInterplated(step, pointsForm, uvx, verts, uvs, uvs1, normals, indx, path_sublineIndx);
//            else SetNotInterplated(pointsForm, uvx, verts, uvs, uvs1, normals, indx, path_sublineIndx); 
//        }

//        res.SetVertices(verts);
//        res.SetUVs(0, uvs);
//        res.SetUVs(1, uvs1);
//        res.SetIndices(indx.ToArray(), MeshTopology.Triangles, 0);

//        if (IsSmooth)
//            res.normals = normals.ToArray();
//        else
//            res.RecalculateNormals();
//        res.RecalculateBounds();
//    }

//    private void SetInterplated(float step, List<Vector3> pointsForm, float[] uvx, List<Vector3> verts, List<Vector2> uvs, List<Vector2> uvs1, List<Vector3> normals, List<int> indx, int path_sublineIndex)
//    {
//        #region Data
//        Bounds b = GetBounds(pointsForm);
//        Vector3 p0, p1;
//        float uvy1, uvy2;
//        Vector2 _uv1 = new Vector2(0, 0);
//        Vector2 _uv2 = new Vector2(1, 0);
//        Vector2 _uv3 = new Vector2(1, 1);
//        Vector2 _uv4 = new Vector2(0, 1);
//        Vector2 _uv1_ = new Vector2(1, 0);
//        Vector2 _uv2_ = new Vector2(0, 0);
//        Vector2 _uv3_ = new Vector2(0, 1);
//        Vector2 _uv4_ = new Vector2(1, 1);
//        Quaternion look1 = Quaternion.identity;
//        Quaternion look2 = Quaternion.identity;
//        Vector3[] mirrorCopyForm = MirrorCopyForm ? GetMirrorCopyForm(pointsForm.ToArray(), b.center, mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ) : null;
//        bool isclosed = CurvePath.Sublines[path_sublineIndex].IsClosed;
//        #endregion
//        Vector3 pathPos = Form2D ? (Vector3)GetComponent<RectTransform>().anchoredPosition : transform.position;
//        p0 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, 0) - pathPos;
//        if (isclosed)
//        {
//            Vector3 temp1 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, (PathQuality) * step) - pathPos;
//            Vector3 temp2 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, step) - pathPos;
//            Vector3 dir1 = ((p0 - temp1) + (temp2 - p0));
//            dir1 = SetFixedDir(dir1);
//            look1 = Quaternion.LookRotation(dir1.normalized, Form2D ? Vector3.forward : Vector3.up);
//        }
//        else
//        {
//            Vector3 temp2 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, step) - pathPos;
//            Vector3 dir1 = (temp2 - p0);
//            dir1 = SetFixedDir(dir1);
//            look1 = Quaternion.LookRotation(dir1.normalized, Form2D ? Vector3.forward : Vector3.up);
//        }
//        Quaternion firstLook = look1;
//        int PQ = PathQuality + 1;
//        for (int i = 1; i < PQ; i++)
//        {
//            uvy1 = (i * step) / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);
//            uvy2 = ((i + 1) * step) / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);

//            #region Prepare look1, look2
//            p1 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, i * step) - pathPos;
//            if (PQ > 2)
//            {
//                if (i + 1 < PQ)
//                {
//                    Vector3 temp2 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, (i + 1) * step) - pathPos;
//                    Vector3 dir1 = (p1 - p0);
//                    Vector3 dir2 = ((temp2 - p1) + dir1);
//                    SetFixedDir(ref dir1, ref dir2);
//                    look2 = Quaternion.LookRotation(dir2.normalized, Form2D ? Vector3.forward : Vector3.up);
//                }
//                else
//                {
//                    if (isclosed)
//                        look2 = firstLook;
//                    else
//                    {
//                        Vector3 dir2 = (p1 - p0);
//                        dir2= SetFixedDir(dir2);
//                        look2 = Quaternion.LookRotation(dir2.normalized, Form2D ? Vector3.forward : Vector3.up);
//                    }
//                }
//            }
//            else
//            {
//                Vector3 dir2 = (p1 - p0);

//                dir2 = SetFixedDir(dir2);
//                look2 = Quaternion.LookRotation(dir2.normalized, Form2D ? Vector3.forward : Vector3.up);
//            }
//            #endregion
//#if UNITY_EDITOR
//            g_list.Add(new g_Data(p0, look1));
//            g_list.Add(new g_Data(p1, look2)); 
//#endif
//            int count = pointsForm.Count - 1;
//            Vector3 center = UseCenterForm ? b.center : Vector3.zero;
//            for (int i1 = 0; i1 < count; i1++)
//            {
//                FillMeshData(pointsForm, 
//                             uvx,
//                             verts, 
//                             uvs, 
//                             uvs1, 
//                             normals, 
//                             indx,
//                             center, 
//                             p0, p1, 
//                             uvy1, uvy2, 
//                             _uv1, _uv2, _uv3, _uv4, 
//                             _uv1_, _uv2_, _uv3_, _uv4_, 
//                             look1, look2, 
//                             mirrorCopyForm, i1);
//            }

//            p0 = p1;
//            look1 = look2;
//        }
//    }

    //private void SetFixedDir(ref Vector3 dir1, ref Vector3 dir2)
    //{
    //    if (FixedX)
    //    {
    //        dir1.x = 0;
    //        dir2.x = 0;
    //    }
    //    if (FixedY)
    //    {
    //        dir1.y = 0;
    //        dir2.y = 0;
    //    }
    //    if (FixedZ)
    //    {
    //        dir1.z = 0;
    //        dir2.z = 0;
    //    }
    //    if (dir1 == Vector3.zero)
    //        dir1 = Vector3.forward;
    //    if (dir2 == Vector3.zero)
    //        dir2 = Vector3.forward;
    //}

    //private Vector3 SetFixedDir(Vector3 dir1)
    //{
    //    if (FixedX) dir1.x = 0;
    //    if (FixedY) dir1.y = 0;
    //    if (FixedZ) dir1.z = 0;
    //    if (dir1 == Vector3.zero)
    //        dir1 = Vector3.forward;
    //    return dir1;
    //}

//    private void SetInterplatedSeg(float step, List<Vector3> pointsForm, float[] uvx, List<Vector3> verts, List<Vector2> uvs, List<Vector2> uvs1, List<Vector3> normals, List<int> indx, int path_sublineIndex)
//    {
//        #region Data
//        Bounds b = GetBounds(pointsForm);
//        Vector3 p0, p1;
//        Vector3 dir1, dir2, firstDir;
//        float uvy1, uvy2;
//        Vector2 _uv1 = new Vector2(0, 0);
//        Vector2 _uv2 = new Vector2(1, 0);
//        Vector2 _uv3 = new Vector2(1, 1);
//        Vector2 _uv4 = new Vector2(0, 1);
//        Vector2 _uv1_ = new Vector2(1, 0);
//        Vector2 _uv2_ = new Vector2(0, 0);
//        Vector2 _uv3_ = new Vector2(0, 1);
//        Vector2 _uv4_ = new Vector2(1, 1);
//        Quaternion look1 = Quaternion.identity;
//        Quaternion look2 = Quaternion.identity;
//        Vector3[] mirrorCopyForm = MirrorCopyForm ? GetMirrorCopyForm(pointsForm.ToArray(), b.center, mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ) : null;
//        bool isclosed = CurvePath.Sublines[0].IsClosed;
//        #endregion
//        Vector3 pathPos = Form2D ? (Vector3)GetComponent<RectTransform>().anchoredPosition : transform.position;
//        p0 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, 0) - pathPos;
//        if (isclosed)
//        {
//            Vector3 temp1 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, (PathQuality) * step) - pathPos;
//            Vector3 temp2 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, step) - pathPos;
//            dir1 = ((p0 - temp1) + (temp2 - p0));
//            dir1 = SetFixedDir(dir1);
//            look1 = Quaternion.LookRotation(dir1.normalized, Form2D ? Vector3.forward : Vector3.up);
//        }
//        else
//        {
//            Vector3 temp2 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, step) - pathPos;
//            dir1 = (temp2 - p0);
//            dir1 = SetFixedDir(dir1);
//            look1 = Quaternion.LookRotation(dir1.normalized, Form2D ? Vector3.forward : Vector3.up);
//        }
//        Quaternion firstLook = look1;
//        firstDir = dir1.normalized;
//        int PQ = PathQuality + 1;
//        for (int i = 1; i < PQ + 1; i++)
//        {
//            uvy1 = (i * step) / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);
//            uvy2 = ((i + 1) * step) / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);

//            #region Prepare look1, look2
//            p1 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, i * step) - pathPos;
//            if (PQ > 2)
//            {
//                if (i + 1 < PQ)
//                {
//                    Vector3 temp2 = CurvePath.GetInterpolatedPointEx(path_sublineIndex, (i + 1) * step) - pathPos;
//                    dir1 = (p1 - p0);
//                    dir2 = ((temp2 - p1) + dir1);

//                    SetFixedDir(ref dir1, ref dir2);

//                    look2 = Quaternion.LookRotation(dir2.normalized, Form2D ? Vector3.forward : Vector3.up);
//                }
//                else
//                {
//                    if (isclosed)
//                    {
//                        look2 = firstLook;
//                        dir2 = firstDir;
//                    }
//                    else
//                    {
//                        dir2 = (p1 - p0);
//                        dir2 = SetFixedDir(dir2);
//                        look2 = Quaternion.LookRotation(dir2.normalized, Form2D ? Vector3.forward : Vector3.up);
//                    }
//                }
//            }
//            else
//            {
//                dir2 = (p1 - p0);
//                dir2 = SetFixedDir(dir2);
//                look2 = Quaternion.LookRotation(dir2.normalized, Form2D ? Vector3.forward : Vector3.up);
//            }
//            #endregion
//#if UNITY_EDITOR
//            g_list.Add(new g_Data(p0, look1));
//            g_list.Add(new g_Data(p1, look2));
//#endif
//            int count = pointsForm.Count - 1;

//            Vector3 pos1 = i == 0 ? p0 - dir1 * brakePathSegmentOffset : p0;
//            Vector3 pos2 = i == PQ - 1 ? p1 - dir2 * brakePathSegmentOffset : p1;

//            Vector3 center = UseCenterForm ? b.center : Vector3.zero;
//            for (int i1 = 0; i1 < count; i1++)
//            {
//                FillMeshData(pointsForm,
//                             uvx,
//                             verts,
//                             uvs,
//                             uvs1,
//                             normals,
//                             indx,
//                             center,
//                             pos1, pos2,
//                             uvy1, uvy2,
//                             _uv1, _uv2, _uv3, _uv4,
//                             _uv1_, _uv2_, _uv3_, _uv4_,
//                             look1, look2,
//                             mirrorCopyForm, i1);
//            }

//            p0 = p1;
//            look1 = look2;
//        }
//    }
//    private void SetNotInterplated(List<Vector3> pointsForm, float[] uvx, List<Vector3> verts, List<Vector2> uvs, List<Vector2> uvs1, List<Vector3> normals, List<int> indx, int path_sublineIndex)
//    {
//        #region fields
//        Bounds b = GetBounds(pointsForm);
//        Vector3 p0, p1;
//        float uvy1, uvy2;
//        Vector2 _uv1 = new Vector2(0, 0);
//        Vector2 _uv2 = new Vector2(1, 0);
//        Vector2 _uv3 = new Vector2(1, 1);
//        Vector2 _uv4 = new Vector2(0, 1);
//        Vector2 _uv1_ = new Vector2(1, 0);
//        Vector2 _uv2_ = new Vector2(0, 0);
//        Vector2 _uv3_ = new Vector2(0, 1);
//        Vector2 _uv4_ = new Vector2(1, 1);
//        float l = 0;
//        Quaternion look1 = Quaternion.identity;
//        Quaternion look2 = Quaternion.identity;
//        Quaternion firstlook = Quaternion.identity;
//        Vector3[] mirrorCopyForm = MirrorCopyForm ? GetMirrorCopyForm(pointsForm.ToArray(), b.center, mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ) : null;
//        bool isclosed = CurvePath.Sublines[path_sublineIndex].IsClosed;
//        #endregion
//        Vector3 pathPos = Form2D ? (Vector3)GetComponent<RectTransform>().anchoredPosition : transform.position;
//        XLinePathSegment[] segments = CurvePath.Sublines[path_sublineIndex].Segments;
//        if (isclosed)
//            look1 = Quaternion.LookRotation((segments[segments.Length - 1].Direction + segments[0].Direction).normalized, Form2D ? Vector3.forward : Vector3.up);
//        else look1 = Quaternion.LookRotation(segments[0].Direction, Form2D ? Vector3.forward : Vector3.up);
//        firstlook = look1;
//        for (int i = 0; i < segments.Length; i++)
//        {
//            #region Preparing direction
//            uvy1 = l / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);
//            l += i < segments.Length - 1 ? segments[i].length : 0;
//            uvy2 = l / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);

//            p0 = segments[i].Start.GetPos(Form2D) - pathPos;
//            p1 = segments[i].End.GetPos(Form2D) - pathPos;

//            if (isclosed && i == segments.Length - 1)
//            {
//                look2 = firstlook;
//            }
//            else
//            {
//                if (i + 1 < segments.Length)
//                    look2 = Quaternion.LookRotation((segments[i].Direction + segments[i + 1].Direction).normalized, Form2D ? Vector3.forward : Vector3.up);
//                else
//                    look2 = Quaternion.LookRotation(segments[i].Direction, Form2D ? Vector3.forward : Vector3.up);
//            }

//            #endregion
//#if UNITY_EDITOR
//            g_list.Add(new g_Data(p0, look1));
//            g_list.Add(new g_Data(p1, look2));
//#endif

//            int count = pointsForm.Count - 1;
//            Vector3 center = UseCenterForm ? b.center : Vector3.zero;
//            for (int i1 = 0; i1 < count; i1++)
//            {
//                FillMeshData(pointsForm,
//                             uvx,
//                             verts,
//                             uvs,
//                             uvs1,
//                             normals,
//                             indx,
//                             center,
//                             p0, p1,
//                             uvy1, uvy2,
//                             _uv1, _uv2, _uv3, _uv4,
//                             _uv1_, _uv2_, _uv3_, _uv4_,
//                             look1, look2,
//                             mirrorCopyForm, i1);
//            }
//            look1 = look2;
//        }
//    }
//    private void SetNotInterplatedSeg(List<Vector3> pointsForm, float[] uvx, List<Vector3> verts, List<Vector2> uvs, List<Vector2> uvs1, List<Vector3> normals, List<int> indx, int path_sublineIndex)
//    {
//        #region fields
//        Bounds b = GetBounds(pointsForm);
//        Vector3 p0, p1;
//        float uvy1, uvy2;
//        Vector2 _uv1 = new Vector2(0, 0);
//        Vector2 _uv2 = new Vector2(1, 0);
//        Vector2 _uv3 = new Vector2(1, 1);
//        Vector2 _uv4 = new Vector2(0, 1);
//        Vector2 _uv1_ = new Vector2(1, 0);
//        Vector2 _uv2_ = new Vector2(0, 0);
//        Vector2 _uv3_ = new Vector2(0, 1);
//        Vector2 _uv4_ = new Vector2(1, 1);
//        float l = 0;
//        Quaternion look1 = Quaternion.identity;
//        Quaternion look2 = Quaternion.identity;
//        Vector3[] mirrorCopyForm = MirrorCopyForm ? GetMirrorCopyForm(pointsForm.ToArray(), b.center, mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ) : null;
//        #endregion
//        Vector3 pathPos = Form2D ? (Vector3)GetComponent<RectTransform>().anchoredPosition : transform.position;
//        XLinePathSegment[] segments = CurvePath.Sublines[path_sublineIndex].Segments;
//        for (int i = 0; i < segments.Length; i++)
//        {
//            #region Preparing direction
//            uvy1 = l / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);
//            l += i < segments.Length - 1 ? segments[i].length : 0;
//            uvy2 = l / (CurvePath.Sublines[path_sublineIndex].Length / Tiling.y);

//            p0 = segments[i].Start.GetPos(Form2D) - pathPos;
//            p1 = segments[i].End.GetPos(Form2D) - pathPos;

//            look1 = look2 = Quaternion.LookRotation(segments[i].Direction, Form2D ? Vector3.forward : Vector3.up);

//            #endregion
//#if UNITY_EDITOR
//            g_list.Add(new g_Data(p0, look1));
//            g_list.Add(new g_Data(p1, look2));
//#endif

//            int count = pointsForm.Count - 1;
//            Vector3 pos1 = p0 - segments[i].Direction * brakePathSegmentOffset;
//            Vector3 pos2 = p1 - segments[i].InvDirection * brakePathSegmentOffset;
//            Vector3 center = UseCenterForm ? b.center : Vector3.zero;
//            for (int i1 = 0; i1 < count; i1++)
//            {
//                FillMeshData(pointsForm,
//                             uvx,
//                             verts,
//                             uvs,
//                             uvs1,
//                             normals,
//                             indx,
//                             center,
//                             pos1, pos2,
//                             uvy1, uvy2,
//                             _uv1, _uv2, _uv3, _uv4,
//                             _uv1_, _uv2_, _uv3_, _uv4_,
//                             look1, look2,
//                             mirrorCopyForm, i1);
//            }
//        }
//    }

    //private void FillMeshData(
    //    List<Vector3> pointsForm, 
    //    float[] uvx, 
    //    List<Vector3> verts, List<Vector2> uvs, List<Vector2> uvs1, List<Vector3> normals, List<int> indx, 
    //    Vector3 centerBound, Vector3 pstart, Vector3 pend, 
    //    float uvy1, float uvy2, Vector2 _uv1, Vector2 _uv2, Vector2 _uv3, Vector2 _uv4, 
    //    Vector2 _uv1_, Vector2 _uv2_, Vector2 _uv3_, Vector2 _uv4_, 
    //    Quaternion look1, Quaternion look2, Vector3[] mirrorCopyForm, int i1)
    //{
    //    Vector3 pos1, pos2, pos3, pos4;
    //    float uvx1, uvx2;
    //    uvx1 = uvx[i1];
    //    uvx2 = uvx[i1 + 1];

    //    pos1 = (look1 * (centerBound - pointsForm[i1])    );// - (UseCenterForm ? Vector3.zero : centerBound);
    //    pos2 = (look1 * (centerBound - pointsForm[i1 + 1]));// - (UseCenterForm ? Vector3.zero : centerBound);
    //    pos3 = (look2 * (centerBound - pointsForm[i1 + 1]));// - (UseCenterForm ? Vector3.zero : centerBound);
    //    pos4 = (look2 * (centerBound - pointsForm[i1]));// - (UseCenterForm ? Vector3.zero : centerBound);

    //    Vector3 off1 = look1 * new Vector3(Offset.x + multiFormOffset.x, 0, Offset.z + multiFormOffset.z);
    //    Vector3 off2 = look2 * new Vector3(Offset.x + multiFormOffset.x, 0, Offset.z + multiFormOffset.z);
    //    if (Offset.y != 0)
    //    {
    //        off1.y = Offset.y + multiFormOffset.y;
    //        off2.y = Offset.y + multiFormOffset.y;
    //    }

    //    #region Vertices
    //    verts.Add(pstart + off1 - pos1);
    //    verts.Add(pstart + off1 - pos2);
    //    verts.Add(pend + off2 - pos3);
    //    verts.Add(pend + off2 - pos4);

    //    if (MirrorCopyForm)
    //    {
    //        pos1 = look1 * (centerBound - mirrorCopyForm[i1]);
    //        pos2 = look1 * (centerBound - mirrorCopyForm[i1 + 1]);
    //        pos3 = look2 * (centerBound - mirrorCopyForm[i1 + 1]);
    //        pos4 = look2 * (centerBound - mirrorCopyForm[i1]);
    //        off1.x *= -1;
    //        off1.z *= -1;
    //        off2.x *= -1;
    //        off2.z *= -1;
    //        verts.Add(pend + off2 - pos4);
    //        verts.Add(pend + off2 - pos3);
    //        verts.Add(pstart + off1 - pos2);
    //        verts.Add(pstart + off1 - pos1);
    //    }
    //    #endregion

    //    if (IsSmooth)
    //    {
    //        #region Normals
    //        Vector3 n1 = look1 * Vector3.right;
    //        Vector3 n2 = look2 * Vector3.right;
    //        Vector3 norm;
    //        if (MirrorCopyForm) norm = GetPolygonNormal(verts[verts.Count - 8], verts[verts.Count - 7], verts[verts.Count - 6], verts[verts.Count - 5]);
    //        else norm = GetPolygonNormal(verts[verts.Count - 4], verts[verts.Count - 3], verts[verts.Count - 2], verts[verts.Count - 1]);

    //        if (Vector3.Dot(norm, n1) > 0 && InvertFace)
    //        {
    //            n1 *= -1;
    //            n2 *= -1;
    //        }

    //        normals.Add(n1);
    //        normals.Add(n1);
    //        normals.Add(n2);
    //        normals.Add(n2);

    //        if (MirrorCopyForm)
    //        {
    //            norm = GetPolygonNormal(verts[verts.Count - 4], verts[verts.Count - 3], verts[verts.Count - 2], verts[verts.Count - 1]);

    //            n1 = look1 * Vector3.left;
    //            n2 = look2 * Vector3.left;
    //            if (Vector3.Dot(norm, n1) > 0 && InvertFace)
    //            {
    //                n1 *= -1;
    //                n2 *= -1;
    //            }

    //            normals.Add(n2);
    //            normals.Add(n2);
    //            normals.Add(n1);
    //            normals.Add(n1);
    //        }
    //        #endregion
    //    }

    //    #region UVs
    //    uvs.Add(new Vector2(uvx1, uvy1));
    //    uvs.Add(new Vector2(uvx2, uvy1));
    //    uvs.Add(new Vector2(uvx2, uvy2));
    //    uvs.Add(new Vector2(uvx1, uvy2));

    //    uvs1.Add(_uv1);
    //    uvs1.Add(_uv2);
    //    uvs1.Add(_uv3);
    //    uvs1.Add(_uv4);

    //    if (MirrorCopyForm)
    //    {
    //        uvs.Add(new Vector2(1 - uvx1, uvy1));
    //        uvs.Add(new Vector2(1 - uvx2, uvy1));
    //        uvs.Add(new Vector2(1 - uvx2, uvy2));
    //        uvs.Add(new Vector2(1 - uvx1, uvy2));

    //        uvs1.Add(_uv1_);
    //        uvs1.Add(_uv2_);
    //        uvs1.Add(_uv3_);
    //        uvs1.Add(_uv4_);
    //    }
    //    #endregion

    //    #region Indexes
    //    if (InvertFace)
    //    {
    //        indx.Add(verts.Count - 4); //0
    //        indx.Add(verts.Count - 3); //1
    //        indx.Add(verts.Count - 2); //2

    //        indx.Add(verts.Count - 2); //2 
    //        indx.Add(verts.Count - 1); //3
    //        indx.Add(verts.Count - 4); //0
    //    }
    //    else
    //    {
    //        indx.Add(verts.Count - 4); //0
    //        indx.Add(verts.Count - 1); //3
    //        indx.Add(verts.Count - 2); //2

    //        indx.Add(verts.Count - 2); //2 
    //        indx.Add(verts.Count - 3); //1
    //        indx.Add(verts.Count - 4); //0
    //    }
    //    if (MirrorCopyForm)
    //    {
    //        if (InvertFace)
    //        {
    //            indx.Add(verts.Count - 8); //0
    //            indx.Add(verts.Count - 7); //1
    //            indx.Add(verts.Count - 6); //2

    //            indx.Add(verts.Count - 6); //2 
    //            indx.Add(verts.Count - 5); //3
    //            indx.Add(verts.Count - 8); //0
    //        }
    //        else
    //        {
    //            indx.Add(verts.Count - 8); //0
    //            indx.Add(verts.Count - 5); //3
    //            indx.Add(verts.Count - 6); //2

    //            indx.Add(verts.Count - 6); //2 
    //            indx.Add(verts.Count - 7); //1
    //            indx.Add(verts.Count - 8); //0
    //        }
    //    }
    //    #endregion
    //}

    //private List<Vector3> GetFormOptimizedPoints(XLinePathSubLine curve, float step, out float[] uvx, bool MirrorFormX, bool MirrorFormY, bool MirrorFormZ, Vector3 RotateForm, float scale)
    //{
    //    Vector3 p = Vector3.zero, old_p = Vector3.zero, dir = Vector3.zero, old_dir = Vector3.zero;
    //    List<Vector3> pointsForm = new List<Vector3>();
    //    List<float> uv = new List<float>();
    //    Vector3 zero = Vector3.zero;
    //    Quaternion rot = Quaternion.Euler(RotateForm);
    //    if (!SetFormByPoints)
    //    {
    //        #region SetFormInterpolate
    //        for (int i = 0; i < FormQuality + 1; i++)
    //        {
    //            p = curve.GetInterpolatedPointExLocal(i * step);
    //            //p -= curve.transform.position;
    //            dir = p - old_p;
    //            if (dir.normalized != old_dir.normalized || dir == zero)
    //            {
    //                p.x *= MirrorFormX ? -1 : 1;
    //                p.y *= MirrorFormY ? -1 : 1;
    //                p.z *= MirrorFormZ ? -1 : 1;

    //                p = rot * p;
    //                p *= scale;
    //                pointsForm.Add(p);
    //                uv.Add((i * step) / (curve.Length / Tiling.x));
    //            }

    //            old_dir = dir.normalized;
    //            old_p = p;
    //        } 
    //        #endregion
    //    }
    //    else
    //    {
    //        XLinePathPoint[] _points = curve.GetComponentsInChildren<XLinePathPoint>();
    //        #region SetFormByPoints
    //        float len = 0;
    //        int count = curve.IsClosed ? _points.Length + 1 : _points.Length;
    //        for (int i = 0; i < count; i++)
    //        {
    //            if (i < _points.Length) p = _points[i].LocalPos;
    //            else p = _points[0].LocalPos;

    //            //p -= curve.transform.position;

    //            p.x *= MirrorFormX ? -1 : 1;
    //            p.y *= MirrorFormY ? -1 : 1;
    //            p.z *= MirrorFormZ ? -1 : 1;

    //            p = rot * p;
    //            p *= scale;
    //            pointsForm.Add(p);

    //            uv.Add(len / (curve.Length / Tiling.x));
    //            len += i < _points.Length - 1 ? curve.Segments[i].length : 0;
    //        } 
    //        #endregion
    //    }
    //    uvx = uv.ToArray();
    //    return pointsForm;
    //}
    //private List<Vector3> GetFormOptimizedPoints(XLinePathSegment segment, Vector3 curvePos, float step, out float[] uvx, bool MirrorFormX, bool MirrorFormY, bool MirrorFormZ, Vector3 RotateForm, float scale)
    //{
    //    Vector3 p = Vector3.zero, old_p = Vector3.zero, dir = Vector3.zero, old_dir = Vector3.zero;
    //    List<Vector3> pointsForm = new List<Vector3>();
        
    //    Quaternion rot = Quaternion.Euler(RotateForm);
    //    if (!SetFormByPoints)
    //    {
    //        #region SetFormInterpolate
    //        List<float> uv = new List<float>();
    //        for (int i = 0; i < FormQuality + 1; i++)
    //        {
    //            p = segment.GetInterpolatedPointLocal(i * step);
    //            //p -= curvePos;
    //            dir = p - old_p;
    //            if (dir.normalized != old_dir.normalized || dir == Vector3.zero)
    //            {
    //                p.x *= MirrorFormX ? -1 : 1;
    //                p.y *= MirrorFormY ? -1 : 1;
    //                p.z *= MirrorFormZ ? -1 : 1;

    //                p = rot * p;
    //                p *= scale;
    //                pointsForm.Add(p);
    //                uv.Add((i * step) / (segment.length / Tiling.x));
    //            }

    //            old_dir = dir.normalized;
    //            old_p = p;
    //        }
    //        uvx = uv.ToArray();
    //        #endregion
    //    }
    //    else
    //    {
    //        #region SetFormByPoints
    //        uvx = new float[2];
    //        p = segment.Start.LocalPos;
    //        //p -= curvePos;
    //        p.x *= MirrorFormX ? -1 : 1;
    //        p.y *= MirrorFormY ? -1 : 1;
    //        p.z *= MirrorFormZ ? -1 : 1;
    //        p = rot * p;
    //        p *= scale;
    //        pointsForm.Add(p);
    //        uvx[0] = 0;

    //        p = segment.End.LocalPos;
    //        p -= curvePos;
    //        p.x *= MirrorFormX ? -1 : 1;
    //        p.y *= MirrorFormY ? -1 : 1;
    //        p.z *= MirrorFormZ ? -1 : 1;
    //        p = rot * p;
    //        p *= scale;
    //        pointsForm.Add(p);
    //        uvx[1] = 1f / (segment.length / Tiling.x); 
    //        #endregion
    //    }
    //    return pointsForm;
    //}
    
    public static Bounds GetBounds(IEnumerable<Vector3> points)
    {
        Bounds b = new Bounds();
        float xMax = float.MinValue, yMax = float.MinValue, zMax = float.MinValue;
        float xMin = float.MaxValue, yMin = float.MaxValue, zMin = float.MaxValue;
        //for (int i = 0; i < pointsForm.Count; i++)
        foreach(Vector3 vec in points)
        {
            xMax = Mathf.Max(xMax, vec.x); yMax = Mathf.Max(yMax, vec.y); zMax = Mathf.Max(zMax, vec.z);
            xMin = Mathf.Min(xMin, vec.x); yMin = Mathf.Min(yMin, vec.y); zMin = Mathf.Min(zMin, vec.z);
        }
        Vector3 max = new Vector3(xMax, yMax, zMax);
        Vector3 min = new Vector3(xMin, yMin, zMin);
        b.SetMinMax(min, max);
        //b.center = Vector3.zero;
        return b;
    }

    public Vector3 GetPolygonNormal(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        Vector3 vu;
        Vector3 vt;
        vu = v3 - v1;
        vt = v2 - v4;
        Vector3 normal = Vector3.Cross(vu, vt);
        normal.Normalize();
        return normal;
    }

    Vector3[] GetMirrorCopyForm(Vector3[] pathForm, Vector3 center, bool mirrorByX, bool mirrorByY, bool mirrorByZ)
    {
        center = Vector3.zero;
        Vector3[] res = new Vector3[pathForm.Length];
        for (int i = 0; i < pathForm.Length; i++)
        {
            res[i] = pathForm[i];
            if (mirrorByX)
            {
                res[i].x = (center.x - res[i].x) + center.x;
            }
            if (mirrorByY)
            {
                res[i].y = (center.y - res[i].y) + center.y;
            }
            if (mirrorByZ)
            {
                res[i].z = (center.z - res[i].z) + center.z;
            }
        }
        return res;
    }

    public static Vector4[] CalculateTangents(int[] tris, Vector3[] vertices, Vector2[] uv, Vector3[] normals)
    {
        int triangleCount = tris.Length;
        int vertexCount = vertices.Length;

        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];

        List<Vector4> tangents = new List<Vector4>();

        for (int a = 0; a < triangleCount; a += 3)
        {
            int i1 = tris[a + 0];
            int i2 = tris[a + 1];
            int i3 = tris[a + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);

            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }

        for (int a = 0; a < vertexCount; ++a)
        {
            Vector3 n = normals[a];
            Vector3 t = tan1[a];

            Vector3.OrthoNormalize(ref n, ref t);
            tangents.Add(new Vector4(t.x, t.y, t.z, (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f));
        }

        return tangents.ToArray();
    }


    public virtual void Clear()
    {
        if (DestroyOnStart || Application.isEditor)
        {
            filter.sharedMesh.Clear();
            DestroyImmediate(filter.sharedMesh);
            filter.sharedMesh = null;
        }
    }

#if UNITY_EDITOR
    struct g_Data
    {
        public Vector3 pos;
        public Quaternion look;
        public Vector3 dir;

        public g_Data(Vector3 _pos, Quaternion _look)
        {
            pos = _pos;
            look = _look;
            dir = look * Vector3.forward;
            dir.Normalize();
        }
    }
    //List<g_Data> g_list = new List<g_Data>();

    void OnDrawGizmos()
    {
        if (CurvePath == null || CurveForm == null) return;
        //for (int i = 0; i < g_list.Count; i+=2)
        //{
        //    Gizmos.color = Color.blue;
        //    Vector3 pos = Form2D ? (Vector3)(transform as RectTransform).anchoredPosition : transform.position;
        //    Gizmos.DrawRay(pos + g_list[i].pos, g_list[i].dir);
        //    Gizmos.DrawSphere(pos + g_list[i].pos, 0.05f);
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawRay(pos + g_list[i + 1].pos, g_list[i].dir);
        //    Gizmos.DrawSphere(pos + g_list[i + 1].pos, 0.05f);
        //}
        if (((IXLinePath)CurvePath).IsDirty || ((IXLinePath)CurveForm).IsDirty)
            InitLoft(true);

        /*if(filter.sharedMesh == null) return;

        var verts = filter.sharedMesh.vertices;
        var normals = filter.sharedMesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            var pos = transform.TransformPoint(verts[i]);
            var dir = transform.TransformDirection(normals[i]);
            Gizmos.DrawRay(pos, dir);
        }*/
    }
#endif

}
