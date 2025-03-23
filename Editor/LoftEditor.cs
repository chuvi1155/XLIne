using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(SplineLoft))]
class LoftEditor : Editor
{
    SplineLoft loft;
    int selectedDistort = -1;
    Plane selectedPlane;
    bool isCleared;
    static string message = "Для того чтобы построить Loft(не знаю как переводится), нужно чтобы:\n" +
                            "1: Изначально было выделенно 2 объекта\n" +
                            "2: Оба объекта должны иметь компонент AntaresBezierCurve\n" +
                            "3: В именах одного из них должна присутствовать надпись '_Path', а у другого '_Form'\n" +
                            "4: .......... эээээээээээ и вроде все";
    [MenuItem("Chuvi/Loft/Loft")]
    static void RunCreateLoft()
    {
        if (EditorUtility.DisplayDialog("ВНИМАНИЕ!!!", message, "Ok", "Cancel"))
        {
            if (Selection.gameObjects.Length == 2)
            {
                GameObject[] gos = Selection.gameObjects;
                if (gos[0].GetComponent<XLinePath>() != null && gos[1].GetComponent<XLinePath>() != null)
                {
                    GameObject main = new GameObject("LoftObject");
                    SplineLoft sl = main.AddComponent<SplineLoft>();
                    if (gos[0].name.ToUpper().IndexOf("_PATH") >= 0)
                        sl.CurvePath = gos[0].GetComponent<XLinePath>();
                    else if (gos[0].name.ToUpper().IndexOf("_FORM") >= 0)
                        sl.CurveForm = gos[0].GetComponent<XLinePath>();
                    else
                    {
                        EditorUtility.DisplayDialog("Ошибка!!!", "В имени объекта '" + gos[0].name + "' не найдено слов '_Path' или '_Form'\n<=================================>\nПункт 3 не выполнен.", "Close");
                        GameObject.DestroyImmediate(main);
                        return;
                    }

                    if (gos[1].name.ToUpper().IndexOf("_PATH") >= 0)
                        sl.CurvePath = gos[1].GetComponent<XLinePath>();
                    else if (gos[1].name.ToUpper().IndexOf("_FORM") >= 0)
                        sl.CurveForm = gos[1].GetComponent<XLinePath>();
                    else
                    {
                        EditorUtility.DisplayDialog("Ошибка!!!", "В имени объекта '" + gos[1].name + "' не найдено слов '_Path' или '_Form'\n<=================================>\nПункт 3 не выполнен.", "Close");
                        GameObject.DestroyImmediate(main);
                        return;
                    }
                    main.name = gos[0].name.Substring(0, gos[0].name.IndexOf("_")) + "_LoftObject";
                    sl.Init();
                    Selection.activeGameObject = main;
                }
                else
                {
                    EditorUtility.DisplayDialog("ВНИМАНИЕ!!!", "Бля, я же говорил. ==> " + message + "\n<=================================>\nПункт 2 не выполнен.", "Ok");
                    //GameObject.DestroyImmediate(main);
                    return;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("ВНИМАНИЕ!!!", "Бля, я же говорил. ==> " + message + "\n<=================================>\nПункт 1 не выполнен.", "Ok");
                //GameObject.DestroyImmediate(main);
                return;
            }
        }
    }


    SerializedProperty AddCollider;
    //SerializedProperty DestroyOnStart;
    SerializedProperty CurveForm;
    SerializedProperty CapStart;
    SerializedProperty CapEnd;
    SerializedProperty CurvePath;
    //SerializedProperty mergeSubForms;
    SerializedProperty material;
    /*SerializedProperty Form2D;*/
    SerializedProperty UseCanvasRenderer;
    //SerializedProperty SetPathByPoints;
    SerializedProperty PathQuality;
    SerializedProperty SetFormByPoints;
    SerializedProperty FormQuality;
    SerializedProperty UseCenterForm;
    SerializedProperty MirrorCopyForm;
    SerializedProperty mirrorCopyByX;
    SerializedProperty mirrorCopyByY;
    SerializedProperty mirrorCopyByZ;
    SerializedProperty Scale;
    SerializedProperty Offset;
    SerializedProperty RotateForm;
    /*SerializedProperty FixedX;
    SerializedProperty FixedY;
    SerializedProperty FixedZ;*/
    /*SerializedProperty MirrorFormX;
    SerializedProperty MirrorFormY;
    SerializedProperty MirrorFormZ;*/
    SerializedProperty InvertFace;
    SerializedProperty IsSmooth;
    SerializedProperty Tiling;
    SerializedProperty _calculateLightmapsUVs;
    SerializedProperty sublineIndex;
    SerializedProperty distortPoints;

    void OnEnable()
    {
        AddCollider = serializedObject.FindProperty("AddCollider");
        //DestroyOnStart = serializedObject.FindProperty("DestroyOnStart");
        CurveForm = serializedObject.FindProperty("CurveForm");
        CapStart = serializedObject.FindProperty("CapStart");
        CapEnd = serializedObject.FindProperty("CapEnd");
        CurvePath = serializedObject.FindProperty("CurvePath");
        //mergeSubForms = serializedObject.FindProperty("mergeSubForms");
        material = serializedObject.FindProperty("material");
        /*Form2D = serializedObject.FindProperty("Form2D");*/
        UseCanvasRenderer = serializedObject.FindProperty("UseCanvasRenderer");
        //SetPathByPoints = serializedObject.FindProperty("SetPathByPoints");
        PathQuality = serializedObject.FindProperty("PathQuality");
        SetFormByPoints = serializedObject.FindProperty("SetFormByPoints");
        FormQuality = serializedObject.FindProperty("FormQuality");
        UseCenterForm = serializedObject.FindProperty("UseCenterForm");
        MirrorCopyForm = serializedObject.FindProperty("MirrorCopyForm");
        mirrorCopyByX = serializedObject.FindProperty("mirrorCopyByX");
        mirrorCopyByY = serializedObject.FindProperty("mirrorCopyByY");
        mirrorCopyByZ = serializedObject.FindProperty("mirrorCopyByZ");
        Scale = serializedObject.FindProperty("Scale");
        Offset = serializedObject.FindProperty("Offset");
        RotateForm = serializedObject.FindProperty("RotateForm");
        InvertFace = serializedObject.FindProperty("InvertFace");
        IsSmooth = serializedObject.FindProperty("IsSmooth");
        Tiling = serializedObject.FindProperty("Tiling");
        _calculateLightmapsUVs = serializedObject.FindProperty("_calculateLightmapsUVs");
        sublineIndex = serializedObject.FindProperty("sublineIndex");
        distortPoints = serializedObject.FindProperty("distortPoints");
        if (loft == null)
            loft = target as SplineLoft;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(AddCollider, new GUIContent("Добавить коллайдер"));
        //EditorGUILayout.PropertyField(DestroyOnStart, new GUIContent("DestroyOnStart"));

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(CurvePath, new GUIContent("Путь"));
        var line = CurvePath.objectReferenceValue as XLinePath;
        if (line != null && line.Sublines != null && line.Sublines.Count > 1)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(sublineIndex, new GUIContent("Subline Index"));
            if (sublineIndex.intValue >= line.Sublines.Count)
                sublineIndex.intValue = line.Sublines.Count - 1;
            else if (sublineIndex.intValue < 0)
                sublineIndex.intValue = 0;
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(CurveForm, new GUIContent("Форма"));
        var line_form = CurveForm.objectReferenceValue as XLinePath;
        if (line_form != null && line_form.Sublines != null && line_form.Sublines.Count > 0)
        {
            if (line_form.Sublines.Any(sl => sl.IsClosed))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(CapStart, new GUIContent("Закрыть форму в начале"));
                EditorGUILayout.PropertyField(CapEnd, new GUIContent("Закрыть форму в конце"));
                EditorGUI.indentLevel--;
            }
        }
        //EditorGUILayout.PropertyField(mergeSubForms, new GUIContent("Merge sub-line Forms"));
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(material, new GUIContent("Materials"));
        EditorGUI.indentLevel--;

        /*EditorGUILayout.PropertyField(Form2D, new GUIContent("2D путь"));*/
        /*if(Form2D.boolValue)
            EditorGUILayout.PropertyField(UseCanvasRenderer, new GUIContent("Use CanvasRenderer"));*/

        /*EditorGUILayout.PropertyField(SetPathByPoints, new GUIContent("Путь по точкам"));
        if(!SetPathByPoints.boolValue)*/
            EditorGUILayout.PropertyField(PathQuality, new GUIContent("Детал. пути"));

        EditorGUILayout.PropertyField(SetFormByPoints, new GUIContent("Форма по точкам"));
        if (!SetFormByPoints.boolValue)
            EditorGUILayout.PropertyField(FormQuality, new GUIContent("Детал. формы"));
        EditorGUILayout.PropertyField(UseCenterForm, new GUIContent("Pivot is center"));

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(MirrorCopyForm, new GUIContent("Зеркальная копия формы"));
        if (MirrorCopyForm.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(mirrorCopyByX, new GUIContent("копия формы по X"));
            EditorGUILayout.PropertyField(mirrorCopyByY, new GUIContent("копия формы по Y"));
            EditorGUILayout.PropertyField(mirrorCopyByZ, new GUIContent("копия формы по Z"));
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.PropertyField(Scale, new GUIContent("Масштаб"));
        EditorGUILayout.PropertyField(Offset, new GUIContent("Смещен. формы"));
        EditorGUILayout.PropertyField(RotateForm, new GUIContent("Вращать форму"));

        /*EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(FixedX, new GUIContent("Фиксировать направление по оси X"));
        EditorGUILayout.PropertyField(FixedY, new GUIContent("Фиксировать направление по оси Y"));
        EditorGUILayout.PropertyField(FixedZ, new GUIContent("Фиксировать направление по оси Z"));
        EditorGUILayout.EndVertical();*/

        /*EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Отразить форму:");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(MirrorFormX, new GUIContent(":X"), GUILayout.MaxWidth(40));
        EditorGUILayout.PropertyField(MirrorFormY, new GUIContent(":Y"), GUILayout.MaxWidth(40));
        EditorGUILayout.PropertyField(MirrorFormZ, new GUIContent(":Z"), GUILayout.MaxWidth(40));
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();*/
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(InvertFace, new GUIContent("Инвертирование лицевой стороны"));
        EditorGUILayout.PropertyField(IsSmooth, new GUIContent("Сглаживание"));
        EditorGUILayout.PropertyField(Tiling, new GUIContent("Tiling"));
        EditorGUILayout.PropertyField(_calculateLightmapsUVs, new GUIContent("Calculate Lightmaps UVs"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(distortPoints, new GUIContent("Path Distortions"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        GUI.changed = serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Сброс"))
        {
            loft.isInitOK = false;
            loft.Init();
            EditorUtility.SetDirty(loft);
        }
        else if(GUI.changed)
        {
            //Debug.Log("GUI changed");
            loft.isInitOK = false;
            loft.Init();
            EditorUtility.SetDirty(loft);
        }
    }



    private void OnSceneGUI()
    {
        Color col = Handles.color;
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.control && !Event.current.shift)
        {
            selectedDistort = -1;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            for (int i = 0; i < loft.DistortPoints.Length; i++)
            {
                var dp = loft.DistortPoints[i];
                loft.CurvePath.Sublines[dp.subline].GetInterpolatedValues(dp.distance, out Vector3 pos, out Vector3 vel, out Vector3 acc, out Vector3 up);
                selectedPlane = new Plane(up, pos);
                if(selectedPlane.Raycast(ray, out var enter))
                {
                    var pt = ray.GetPoint(enter);
                    if(Vector3.Distance(pt, pos) <= dp.scale)
                    {
                        selectedDistort = i;
                        break;
                    }
                }
            }
        }

        if (selectedDistort != -1)
        {
            float dist = float.MaxValue;
            Vector3 pos = Vector3.zero;
            var dp = loft.DistortPoints[selectedDistort];
            var sl = loft.CurvePath.Sublines[dp.subline];
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            selectedPlane.Raycast(ray, out var enter);
            pos = ray.GetPoint(enter);
            //foreach (var segm in sl.Segments)
            for (int i1 = 0; i1 < sl.Segments.Length; i1++)
            {
                var segm = sl.Segments[i1];
                var pt1 = segm.Start.Pos;
                var pt2 = segm.End.Pos;
                float _dist1 = Vector3.Distance(pos, pt1);
                if (_dist1 < dist)
                {
                    dist = _dist1;
                    var segm_precission = Mathf.Min(segm.SegmentPartsCount, 12);
                    var step = 1f / segm_precission;
                    for (int i = 0; i < segm_precission; i++)
                    {
                        var pt = segm.GetPoint(step * i, false);
                        _dist1 = Vector3.Distance(pos, pt);
                        if (_dist1 < dist)
                        {
                            dist = _dist1;
                            pos = pt;
                        }
                    }
                }
            }
            Handles.DrawWireDisc(pos, selectedPlane.normal, dp.scale, 4f);
        }

        //HandleUtility.ClosestPointToPolyLine()
        for (int i = 0; i < loft.DistortPoints.Length; i++)
        {
            var dp = loft.DistortPoints[i];
            loft.CurvePath.Sublines[dp.subline].GetInterpolatedValues(dp.distance, out Vector3 pos, out Vector3 vel, out Vector3 acc, out Vector3 up);
            
            if(i == selectedDistort)
                Handles.color = Color.red;
            else
                Handles.color = Color.cyan;
            Handles.DrawWireDisc(pos, up, dp.scale, 4f);

            if (i == selectedDistort)
            {
                EditorGUI.BeginChangeCheck();
                byte updateType = 0; // 1-position, 2-scale
                Vector3 newPos = pos;
                Vector3 scale = Vector3.one * dp.scale;
                if (Event.current.control)
                {
                    newPos = Handles.PositionHandle(pos, Quaternion.LookRotation(vel, up));
                    updateType = 1;
                }
                else if (Event.current.shift)
                {
                    scale = Handles.ScaleHandle(scale, pos, Quaternion.LookRotation(vel, up));
                    updateType = 2;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    switch (updateType)
                    {
                        case 1:
                            Undo.RecordObject(loft, "Change distort pos");
                            var dist = newPos - pos;
                            dp.distance += dist.magnitude * Mathf.Sign(Vector3.Dot(dist, vel));
                            loft.DistortPoints[i] = dp;
                            break;
                        case 2:
                            Undo.RecordObject(loft, "Change distort scale");
                            if (scale.x != dp.scale)
                                dp.scale = scale.x;
                            else if (scale.y != dp.scale)
                                dp.scale = scale.y;
                            else if (scale.z != dp.scale)
                                dp.scale = scale.z;
                            loft.DistortPoints[i] = dp;
                            break;
                        default:
                            break;
                    }
                    loft.isInitOK = false;
                    loft.Init();
                    EditorUtility.SetDirty(loft);
                } 
            }
        }
        Handles.color = col;

        if(selectedDistort != -1 && Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
            Event.current.Use();
    }
}

