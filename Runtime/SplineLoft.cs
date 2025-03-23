using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; 
#endif

[AddComponentMenu("Chuvi/Line/_Modificators/SplineLoft")]
public class SplineLoft : MonoBehaviour
{
    public XLinePath CurveForm;
    public bool CapStart;
    public bool CapEnd;
    public XLinePath CurvePath;
    public List<Material> material = new List<Material>();
    public int PathQuality = 10;
    public int FormQuality = 10;
    public bool isInitOK = false;
    public bool InvertFace = false;
    protected MeshFilter filter;
    public Vector3 Offset = Vector3.zero;
    public float Scale = 1;
    public bool MirrorFormX, MirrorFormY, MirrorFormZ;
    public bool MirrorCopyForm;
    public bool mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ;
    public bool AddCollider = true;
    public bool IsSmooth = true;
    public Vector3 RotateForm;
    public bool ShowInEditor;
    public bool SetFormByPoints = false;
    public Vector2 Tiling = Vector2.one;
    public bool mergeSubForms = true;
    public bool brakePathSegments = false;
    public float brakePathSegmentOffset = 0;
    public bool DestroyOnStart = false;
    public bool UseCenterForm = false;
    public bool Form2D = false;
    public bool UseCanvasRenderer = false;

    [SerializeField] int sublineIndex;
    [SerializeField] bool _calculateLightmapsUVs = false;
    [SerializeField, HideInInspector] private Vector2[] uv2;
    [SerializeField] Distort[] distortPoints;
    [SerializeField] AnimationCurve distortionGrad = new AnimationCurve();

    public Distort[] DistortPoints => distortPoints;

    public virtual void Start()
    {
        filter = GetComponent<MeshFilter>();
        if (filter == null && !UseCanvasRenderer)
            filter = gameObject.AddComponent<MeshFilter>();

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
        {
            PathQuality = CurvePath.Sublines[0].Precision;
            DrawLoft();
        }
        if (Application.isPlaying && DestroyOnStart)
        {
            Destroy(CurvePath.gameObject);
            Destroy(CurveForm.gameObject);
        }
    }

    public void DrawLoft()
    {
        if (isInitOK) UpdateMesh();
        else Init();
    }
    public virtual void Init(bool force = false)
    {
        if (!isInitOK || force)
        {
            if (CurveForm == null || CurvePath == null)
            {
                Clear();
                return;
            }
            CurveForm.Init();
            CurvePath.Init();
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
        if (filter == null && !UseCanvasRenderer)
        {
            filter = GetComponent<MeshFilter>();
            if (filter == null)
                filter = gameObject.AddComponent<MeshFilter>();
        }
        if (CurveForm == null || CurvePath == null || (Application.isPlaying && gameObject.isStatic))
        {
            if (UseCanvasRenderer)
            {
                CanvasRenderer _cr = GetComponent<CanvasRenderer>();
                if (_cr != null)
                {
                    _cr.Clear();
                }
            }
            //else if(filter != null)
            //{
            //    if (filter.sharedMesh != null)
            //        filter.sharedMesh.Clear();
            //    filter.sharedMesh = null;
            //}
            Debug.Log("UpdateMesh skip");
            sw.Stop();
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
        else 
            mesh.Clear();
        if (UseCanvasRenderer && Form2D)
            mergeSubForms = false;

        GenerateLoft(mesh);
        mesh.UploadMeshData(false);
        if (_calculateLightmapsUVs)
        {
#if UNITY_EDITOR
            Unwrapping.GenerateSecondaryUVSet(mesh);
            uv2 = mesh.uv2;
#else
        if(uv2.Length > 0)
            mesh.uv2 = uv2;
#endif 
        }
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
                        mat.color = CurvePath.Sublines[i].CurveColor;
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
                        mat.color = sl.CurveColor;
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
        }
        if (res == null)
            return;
        if (CurveForm.Sublines == null || CurvePath.Sublines == null) return;
        res.Clear();
        distortionGrad.ClearKeys();
        var sl_path = CurvePath.Sublines[sublineIndex];//[i1];
        if (sl_path.Segments == null || sl_path.Segments.Length == 0)
            return;
        if (distortPoints == null || distortPoints.Length == 0)
        {
            distortionGrad.AddKey(0, 1);
            distortionGrad.AddKey(sl_path.Length, 1);
        }
        else
        {
            bool hasStart = false;
            bool hasEnd = false;
            for (int i = 0; i < distortPoints.Length; i++)
            {
                if (distortPoints[i].subline == sublineIndex)
                {
                    distortionGrad.AddKey(new Keyframe(distortPoints[i].distance, distortPoints[i].scale, -0.01f, 0.01f));
                    if (distortPoints[i].distance == 0)
                        hasStart = true;
                    if (distortPoints[i].distance == sl_path.Length)
                        hasEnd = true;
                }
            }
            if(!hasStart)
                distortionGrad.AddKey(new Keyframe(0, 1, -0.01f, 0.01f));
            if (!hasEnd)
                distortionGrad.AddKey(new Keyframe(sl_path.Length, 1, -0.01f, 0.01f));
        }

        CombineInstance[] combine = new CombineInstance[CurveForm.Sublines.Count * CurvePath.Sublines.Count];
        Mesh[] _mesh = new Mesh[CurveForm.Sublines.Count * CurvePath.Sublines.Count];
        for (int i = 0; i < CurveForm.Sublines.Count; i++)
        {
            var sl_form = CurveForm.Sublines[i];
            if (sl_form.Segments == null || sl_form.Segments.Length == 0)
                return;
            //for (int i1 = 0; i1 < CurvePath.Sublines.Count; i1++)
            //{
            int meshIndx = i * CurvePath.Sublines.Count;// + i1;
            sl_path.Force2D = Form2D;
            _mesh[meshIndx] = new Mesh();
            _mesh[meshIndx].name = "_mesh_combine";

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
            SetMesh2(_mesh[meshIndx], stepPath, pointsForm, uvx, sublineIndex);

            if (MirrorCopyForm)
            {
                Bounds b = GetBounds(pointsForm);
                var copyForm = GetMirrorCopyForm(pointsForm, b.center, mirrorCopyByX, mirrorCopyByY, mirrorCopyByZ);
                if (pointsForm.Length < 2) continue;
                // move by path and create mesh
                var copyMesh = new Mesh();
                var inv = InvertFace;

                InvertFace = (!InvertFace && mirrorCopyByY);
                SetMesh2(copyMesh, stepPath, copyForm, uvx, sublineIndex);
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
        Vector2[][] formFrameUVs_detailed = new Vector2[PathQuality + 1][];
        Vector2[][] formFrameUVs = new Vector2[PathQuality + 1][];
        float[] formFrameLengths = new float[pointsForm.Length];
        var len = sl.Length;
        for (int i = 0; i < PathQuality + 1; i++)
        {
            formFrameVerts[i] = new Vector3[pointsForm.Length];
            formFrameUVs_detailed[i] = new Vector2[pointsForm.Length];
            formFrameUVs[i] = new Vector2[pointsForm.Length];
            var dist = step * i;
            sl.GetInterpolatedValues(dist, out var pos, out var vel, out var acc, out var up, true);
            var mat = Matrix4x4.TRS(pos - pathPos, Quaternion.LookRotation(vel.normalized, up), Vector3.one);
            var uv1_y = dist / len;
            var distort = distortionGrad.Evaluate(dist);
            for (int n = 0; n < pointsForm.Length; n++)
            {
                formFrameVerts[i][n] = mat.MultiplyPoint(pointsForm[n] * distort);

                formFrameUVs[i][n] = new Vector2(uvx[n], uv1_y);
                if (i == 0)
                    formFrameUVs_detailed[i][n] = new Vector2(uvx[n], 0); // temp by Y set distance between previews point
                else
                {
                    var d = Vector3.Distance(formFrameVerts[i][n], formFrameVerts[i - 1][n]); // calculate full length by each form pointline
                    formFrameLengths[n] += d;
                    formFrameUVs_detailed[i][n] = new Vector2(uvx[n], formFrameLengths[n]); // temp by Y set distance between previews point
                }
            }
        }

        for (int i = 0; i < PathQuality + 1; i++)
        {
            for (int n = 0; n < pointsForm.Length; n++)
            {
                var uv = formFrameUVs_detailed[i][n];
                uv.y = uv.y / (formFrameLengths[n] / Tiling.y);
                formFrameUVs_detailed[i][n] = uv;
            }
        }

        int capCount = (CapStart ? pointsForm.Length : 0) + (CapEnd ? pointsForm.Length : 0);
        Vector3[] verts = new Vector3[PathQuality * (pointsForm.Length - 1) * 4 + capCount];
        List<int>[][] normals = new List<int>[PathQuality + 1][];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector2[] uvs1 = new Vector2[verts.Length];
        int[] indx = new int[((verts.Length - capCount) / 2) * 3];
        int v = 0, tri = 0;
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
                verts[v] = formFrameVerts[i][n];//,
                verts[v + 1] = formFrameVerts[i][n + 1];//,
                verts[v + 2] = formFrameVerts[i + 1][n + 1];//,
                verts[v + 3] = formFrameVerts[i + 1][n];

                if (IsSmooth)
                {
                    List<int> normalIndexes_n = normals[i][n] == null ? new List<int>() : normals[i][n];
                    if (normals[i][n] == null) 
                        normals[i][n] = normalIndexes_n;
                    List<int> normalIndexes_n1 = normals[i][n + 1] == null ? new List<int>() : normals[i][n + 1];
                    if (normals[i][n + 1] == null)
                        normals[i][n + 1] = normalIndexes_n1;
                    List<int> next_normalIndexes_n = normals[i + 1][n] == null ? new List<int>() : normals[i + 1][n];
                    if (normals[i + 1][n] == null) 
                        normals[i + 1][n] = next_normalIndexes_n;
                    List<int> next_normalIndexes_n1 = normals[i + 1][n + 1] == null ? new List<int>() : normals[i + 1][n + 1];
                    if (normals[i + 1][n + 1] == null) 
                        normals[i + 1][n + 1] = next_normalIndexes_n1;
                    normalIndexes_n.Add(v);//(verts.Count - 4);
                    normalIndexes_n1.Add(v + 1);//(verts.Count - 3);
                    next_normalIndexes_n1.Add(v + 2);//(verts.Count - 2);
                    next_normalIndexes_n.Add(v + 3);//(verts.Count - 1); 
                }

                //uvs.AddRange(new Vector2[]
                //{
                uvs[v] =     formFrameUVs[i][n];//,
                uvs[v + 1] = formFrameUVs[i][n + 1];//,
                uvs[v + 2] = formFrameUVs[i + 1][n + 1];//,
                uvs[v + 3] = formFrameUVs[i + 1][n];
                //});
                //uvs1.AddRange(new Vector2[]
                //{
                uvs1[v] =     formFrameUVs_detailed[i][n];//,
                uvs1[v + 1] = formFrameUVs_detailed[i][n + 1];//,
                uvs1[v + 2] = formFrameUVs_detailed[i + 1][n + 1];//,
                uvs1[v + 3] = formFrameUVs_detailed[i + 1][n];
                //});

                if (InvertFace)
                {
                    //indx.AddRange(new int[]
                    //{
                    indx[tri++] = v;    //verts.Count - 4, //0
                    indx[tri++] = v + 1;//verts.Count - 3, //1
                    indx[tri++] = v + 2;//verts.Count - 2, //2

                    indx[tri++] = v + 2;//verts.Count - 2, //2
                    indx[tri++] = v + 3;//verts.Count - 1, //3
                    indx[tri++] = v;    //verts.Count - 4  //0
                    //});
                }
                else
                {
                    //indx.AddRange(new int[]
                    //{
                    indx[tri++] = v;    //verts.Count - 4, //0
                    indx[tri++] = v + 3;//verts.Count - 1, //3
                    indx[tri++] = v + 2;//verts.Count - 2, //2

                    indx[tri++] = v + 2;//verts.Count - 2, //2
                    indx[tri++] = v + 1;//verts.Count - 3, //1
                    indx[tri++] = v;    //verts.Count - 4  //0
                    //});
                }
                v += 4;
            }
        }

        Triangulator triang = null;
        int[] indexes = null;
        if (CapStart)
        {
            triang = new Triangulator(pointsForm);
            indexes = triang.Triangulate();
            int indx_len = indx.Length;
            System.Array.Resize(ref indx, indx.Length + indexes.Length);
            int startIndex_vert = CapEnd ? (verts.Length - pointsForm.Length - pointsForm.Length) : (verts.Length - pointsForm.Length);
            int startIndex_tri = indx_len;//CapEnd ? (indx.Length - 6 - 6) : (indx.Length - 6);
            for (int i = 0; i < indexes.Length; i++)
                indx[i + startIndex_tri] = indexes[InvertFace ? indexes.Length - i - 1 : i] + startIndex_vert;
            for (int i = 0; i < pointsForm.Length; i++)
            {
                verts[i + startIndex_vert] = formFrameVerts[0][i];
                // тут не так должно быть, надо делать отдельную развертку для закрываюших поверхностей
                uvs[i + startIndex_vert] = uvs[i];
                uvs1[i + startIndex_vert] = uvs1[i];
            }
        }
        if (CapEnd)
        {
            if (triang == null)
            {
                triang = new Triangulator(pointsForm);
                indexes = triang.Triangulate();
            }
            int indx_len = indx.Length;
            System.Array.Resize(ref indx, indx.Length + indexes.Length);
            int startIndex_vert = verts.Length - pointsForm.Length;
            int startIndex_tri = indx_len;//indx.Length - 6;
            for (int i = 0; i < indexes.Length; i++)
                indx[i + startIndex_tri] = indexes[InvertFace ? i : indexes.Length - i - 1] + startIndex_vert;
            for (int i = 0; i < pointsForm.Length; i++)
            {
                verts[i + startIndex_vert] = formFrameVerts[PathQuality][i];
                // тут не так должно быть, надо делать отдельную развертку для закрываюших поверхностей
                uvs[i + startIndex_vert] = uvs[i];
                uvs1[i + startIndex_vert] = uvs1[i];
            }
        }


        res.SetVertices(verts);
        res.SetUVs(0, uvs);
        res.SetUVs(1, uvs1);
        res.SetIndices(indx, MeshTopology.Triangles, 0);

        if (IsSmooth)
        {
            res.RecalculateNormals();
            var norms = res.normals;

            for (int i = 0; i < normals.Length; i++)
            {
                var line = normals[i];
                for (int i1 = 0; i1 < line.Length; i1++)
                {
                    var points = line[i1];
                    Vector3 norm = Vector3.zero;
                    for (int i2 = 0; i2 < points.Count; i2++)
                    {
                        norm += norms[points[i2]];
                    }
                    norm.Normalize();
                    for (int i2 = 0; i2 < points.Count; i2++)
                    {
                        norms[points[i2]] = norm;
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
    
    /// <summary>
    /// Return form points in local space
    /// </summary>
    /// <param name="sl"></param>
    /// <param name="step"></param>
    /// <param name="uvx"></param>
    /// <param name="MirrorFormX"></param>
    /// <param name="MirrorFormY"></param>
    /// <param name="MirrorFormZ"></param>
    /// <param name="RotateForm"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
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
                sl.GetInterpolatedValues(step * i, out var pos, out var _, out var _, out var _, true);
                pos = sl.transform.InverseTransformPoint(pos + Offset);
                pos.x *= MirrorFormX ? -1 : 1;
                pos.y *= MirrorFormY ? -1 : 1;
                pos.z *= MirrorFormZ ? -1 : 1;
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

    public void Clear()
    {
        Debug.Log("ClearLoft");
#if UNITY_EDITOR
        if (CurvePath != null)
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
            Init(true);
        //Gizmos.color = Color.yellow;
        //for (int i = 0; i < distortionGrad.keys.Length; i++)
        //{
        //    var pos = CurvePath.GetInterpolatedPoint(sublineIndex, distortionGrad.keys[i].time);
        //    Gizmos.DrawSphere(pos, distortionGrad.keys[i].value);
        //}

        /*if(filter.sharedMesh == null) return;

        var verts = filter.sharedMesh.vertices;
        var normals = filter.sharedMesh.normals;
        var tangents = filter.sharedMesh.tangents;
        for (int i = 0; i < normals.Length; i++)
        {
            Gizmos.color = Color.white;
            var pos = transform.TransformPoint(verts[i]);
            var dir = transform.TransformDirection(normals[i]);
            Gizmos.DrawRay(pos, dir);
            Gizmos.color = Color.blue;
            dir = transform.TransformDirection(tangents[i]);
            Gizmos.DrawRay(pos, dir);
        }*/
    }
#endif

    [System.Serializable]
    public struct Distort
    {
        public int subline;
        public float distance;
        public float scale;
    }
}
