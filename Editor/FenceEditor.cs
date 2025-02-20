using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(FenceLoft))]
class FenceEditor : Editor
{
    public FenceLoft fence;
    bool isCleared;
    static string message = "Для того чтобы построить забор, нужно чтобы:\n" +
                            "1: Изначально было выделенно 2 объекта\n" +
                            "2: Оба объекта должны иметь компонент AntaresBezierCurve\n" +
                            "3: В именах одного из них должна присутствовать надпись '_Path', а у другого '_Form'\n" +
                            "4: .......... эээээээээээ и вроде все";
    private static bool pathFoldout;
    private static bool formFoldout;
    private static bool columnFoldout;
    private static bool spanFoldout;
    //private bool Fix;

    [MenuItem("Chuvi/Loft/Fence")]
    public static FenceLoft RunCreateFence()
    {
        if (EditorUtility.DisplayDialog("ВНИМАНИЕ!!!", message, "Ok", "Cancel"))
        {
            if (Selection.gameObjects.Length == 2)
            {
                GameObject[] gos = Selection.gameObjects;
                if (gos[0].GetComponent<XLinePath>() != null && gos[1].GetComponent<XLinePath>() != null)
                {
                    string namePath = "";
                    GameObject main = new GameObject("FenceObject");
                    FenceLoft sl = main.AddComponent<FenceLoft>();
                    if (gos[0].name.ToUpper().IndexOf("_PATH") >= 0)
                    {
                        sl.CurvePath = gos[0].GetComponent<XLinePath>();
                        namePath = gos[0].name.Substring(0, gos[0].name.IndexOf("_"));
                    }
                    else if (gos[0].name.ToUpper().IndexOf("_FORM") >= 0)
                        sl.CurveForm = gos[0].GetComponent<XLinePath>();
                    else
                    {
                        EditorUtility.DisplayDialog("Ошибка!!!", "В имени объекта '" + gos[0].name + "' не найдено слов '_Path' или '_Form'\n<=================================>\nПункт 3 не выполнен.", "Close");
                        GameObject.DestroyImmediate(main);
                        return null;
                    }

                    if (gos[1].name.ToUpper().IndexOf("_PATH") >= 0)
                    {
                        sl.CurvePath = gos[1].GetComponent<XLinePath>();
                        namePath = gos[1].name.Substring(0, gos[1].name.IndexOf("_"));
                    }
                    else if (gos[1].name.ToUpper().IndexOf("_FORM") >= 0)
                        sl.CurveForm = gos[1].GetComponent<XLinePath>();
                    else
                    {
                        EditorUtility.DisplayDialog("Ошибка!!!", "В имени объекта '" + gos[1].name + "' не найдено слов '_Path' или '_Form'\n<=================================>\nПункт 3 не выполнен.", "Close");
                        GameObject.DestroyImmediate(main);
                        return null;
                    }
                    main.name = namePath + "_FenceObject";
                    sl.Init();
                    Selection.activeGameObject = main;
                    return sl;
                }
                else
                {
                    EditorUtility.DisplayDialog("ВНИМАНИЕ!!!", "Бля, я же говорил. ==> " + message + "\n<=================================>\nПункт 2 не выполнен.", "Ok");
                    //GameObject.DestroyImmediate(main);
                    return null;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("ВНИМАНИЕ!!!", "Бля, я же говорил. ==> " + message + "\n<=================================>\nПункт 1 не выполнен.", "Ok");
                //GameObject.DestroyImmediate(main);
                return null;
            }
        }
        return null;
    }

    public static FenceLoft RunCreateFence(XLinePath path, XLinePath form)
    {
        string namePath = "";
        GameObject main = new GameObject("FenceObject");
        FenceLoft sl = main.AddComponent<FenceLoft>();
        if (path.name.ToUpper().IndexOf("_PATH") >= 0)
        {
            sl.CurvePath = path.GetComponent<XLinePath>();
            namePath = path.name.Substring(0, path.name.IndexOf("_"));
        }
        else if (path.name.ToUpper().IndexOf("_FORM") >= 0)
            sl.CurveForm = path.GetComponent<XLinePath>();
        else
        {
            EditorUtility.DisplayDialog("Ошибка!!!", "В имени объекта '" + path.name + "' не найдено слов '_Path' или '_Form'\n<=================================>\nПункт 3 не выполнен.", "Close");
            GameObject.DestroyImmediate(main);
            return null;
        }

        if (form.name.ToUpper().IndexOf("_PATH") >= 0)
        {
            sl.CurvePath = form.GetComponent<XLinePath>();
            namePath = form.name.Substring(0, form.name.IndexOf("_"));
        }
        else if (form.name.ToUpper().IndexOf("_FORM") >= 0)
            sl.CurveForm = form.GetComponent<XLinePath>();
        else
        {
            EditorUtility.DisplayDialog("Ошибка!!!", "В имени объекта '" + form.name + "' не найдено слов '_Path' или '_Form'\n<=================================>\nПункт 3 не выполнен.", "Close");
            GameObject.DestroyImmediate(main);
            return null;
        }
        main.name = namePath + "_FenceObject";
        sl.Init();
        Selection.activeGameObject = main;
        return sl;
    }

    void OnEnable()
    {
        if (fence == null)
            fence = target as FenceLoft;
        if (!ClearLoftMeshes.ClearedMeshes.ContainsKey(fence.gameObject))
        {
            ClearLoftMeshes.ClearedMeshes.Add(fence.gameObject, true);
            fence.AddCollider = true;
        }
    }

    public override void OnInspectorGUI()
    {
        if (!ClearLoftMeshes.ClearedMeshes.ContainsKey(fence.gameObject))
        {
            ClearLoftMeshes.ClearedMeshes.Add(fence.gameObject, true);
            fence.AddCollider = true;
        }
        serializedObject.Update();
        ClearLoftMeshes.ClearedMeshes[fence.gameObject] = GUILayout.Toggle(ClearLoftMeshes.ClearedMeshes[fence.gameObject], "Удаляемый");
        fence.AddCollider = GUILayout.Toggle(fence.AddCollider, "Добавить коллайдер");
		fence.DestroyOnStart = GUILayout.Toggle(fence.DestroyOnStart, "DestroyOnStart");
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        #region ObjectFields
        if (!fence.IsTemplate)
            fence.CurveForm = (XLinePath)EditorGUILayout.ObjectField("Form bezier", fence.CurveForm, typeof(XLinePath), true);
        fence.CurvePath = (XLinePath)EditorGUILayout.ObjectField("Path bezier", fence.CurvePath, typeof(XLinePath), true);
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("box");
        fence.Column = (GameObject)EditorGUILayout.ObjectField("Столб", fence.Column, typeof(GameObject), true);
        if (fence.ScaleColumn == Vector3.one)
            if (fence.Column != null)
                fence.ScaleColumn = fence.Column.transform.localScale;
        fence.typeColumn = (FenceLoft.TypeColumn)EditorGUILayout.EnumPopup("Тип столба", fence.typeColumn);
        //if (fence.typeColumn == FenceLoft.TypeColumn.Mesh)
            fence.ColumnMat = (Material)EditorGUILayout.ObjectField("Материал столба", fence.ColumnMat, typeof(Material), true);

        fence.Span = (GameObject)EditorGUILayout.ObjectField("Пролет меж.столбов", fence.Span, typeof(GameObject), true);
        if (fence.WithSpan)
        {
            fence.SpanMat = (Material)EditorGUILayout.ObjectField("Материал пролетов", fence.SpanMat, typeof(Material), true);
            fence.SpanType = (FenceLoft.SpanTypes)EditorGUILayout.EnumPopup("Тип пролетов", fence.SpanType);
            if (fence.SpanType == FenceLoft.SpanTypes.Clone)
                fence.CloneSpanCount = EditorGUILayout.IntField("Кол-во клонов в пролете", fence.CloneSpanCount);
        }
        #endregion
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        fence.IsSmooth = GUILayout.Toggle(fence.IsSmooth, "Сглаживание");
        fence.Tiling = EditorGUILayout.Vector2Field("Tiling", fence.Tiling);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        #region Path
        pathFoldout = EditorGUILayout.Foldout(pathFoldout, "Настройка пути");
        if (pathFoldout)
        {
           /* fence.SetPathByPoints = GUILayout.Toggle(fence.SetPathByPoints, "По точкам");
            if (!fence.SetPathByPoints)*/
            {
                fence.PathQuality = EditorGUILayout.IntField("Детал. пути", fence.PathQuality);
            }
            fence.brakePathSegments = GUILayout.Toggle(fence.brakePathSegments, "Разрыв пролетов");
            fence.brakePathSegmentOffset = EditorGUILayout.FloatField("Усечение пролетов", fence.brakePathSegmentOffset);
        } 
        #endregion
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        #region Form
        formFoldout = EditorGUILayout.Foldout(formFoldout, "Настройка формы");
        if (formFoldout)
        {
            fence.IsTemplate = EditorGUILayout.BeginToggleGroup("Форма-шаблон", fence.IsTemplate);
            fence.template_preset = (FenceLoft.FencePreset)EditorGUILayout.EnumPopup("Preset", fence.template_preset);
            EditorGUILayout.EndToggleGroup();

            fence.SetFormByPoints = GUILayout.Toggle(fence.SetFormByPoints, "По точкам");
            if (!fence.SetFormByPoints)
                fence.FormQuality = EditorGUILayout.IntField("Детал. формы", fence.FormQuality);
            EditorGUILayout.BeginVertical("box");
            fence.MirrorCopyForm = GUILayout.Toggle(fence.MirrorCopyForm, "Зеркальная копия формы");
            if (fence.MirrorCopyForm)
            {
                EditorGUILayout.BeginVertical("box");
                fence.mirrorCopyByX = GUILayout.Toggle(fence.mirrorCopyByX, "копия формы по X");
                fence.mirrorCopyByY = GUILayout.Toggle(fence.mirrorCopyByY, "копия формы по Y");
                fence.mirrorCopyByZ = GUILayout.Toggle(fence.mirrorCopyByZ, "копия формы по Z");
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            fence.Scale = EditorGUILayout.FloatField("Масштаб", fence.Scale);
            fence.Offset = EditorGUILayout.Vector3Field("Смещен. формы", fence.Offset);
            fence.RotateForm = EditorGUILayout.Vector3Field("Вращать форму", fence.RotateForm);

            /*EditorGUILayout.BeginVertical("box");
            fence.FixedX = GUILayout.Toggle(fence.FixedX, "Фиксировать направление по оси X");
            fence.FixedY = GUILayout.Toggle(fence.FixedY, "Фиксировать направление по оси Y");
            fence.FixedZ = GUILayout.Toggle(fence.FixedZ, "Фиксировать направление по оси Z");
            EditorGUILayout.EndVertical();*/

            GUILayout.Label("Отразить форму:");
            EditorGUILayout.BeginHorizontal();
            fence.MirrorFormX = GUILayout.Toggle(fence.MirrorFormX, ":X", GUILayout.MaxWidth(40));
            fence.MirrorFormY = GUILayout.Toggle(fence.MirrorFormY, ":Y", GUILayout.MaxWidth(40));
            fence.MirrorFormZ = GUILayout.Toggle(fence.MirrorFormZ, ":Z", GUILayout.MaxWidth(40));
            EditorGUILayout.EndHorizontal();
            fence.InvertFace = GUILayout.Toggle(fence.InvertFace, "Инвертирование лицевой стороны");
        } 
        #endregion
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        #region Columns
        columnFoldout = EditorGUILayout.Foldout(columnFoldout, "Настройка столбов");
        if (columnFoldout)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            //fence.ByDistance = !EditorGUILayout.Toggle("По количеству", !fence.ByDistance);
            //fence.ByDistance = EditorGUILayout.Toggle("По расстоянию", fence.ByDistance);
            fence.SetupColumn = (FenceLoft.TypeSetupColumn)EditorGUILayout.EnumPopup("Распределение столбов", fence.SetupColumn);
            EditorGUILayout.EndHorizontal();
            if (fence.SetupColumn == FenceLoft.TypeSetupColumn.ByCount )
                fence.CountColumn = EditorGUILayout.IntField("Количество", fence.CountColumn);
            else if (fence.SetupColumn == FenceLoft.TypeSetupColumn.ByDistance)
            {

                for (int i = 0; i < fence.CurvePath.Sublines.Count; i++)
                {
                    EditorGUILayout.LabelField("Длина линии: " + i, fence.CurvePath.Sublines[i].Length.ToString()); 
                }
                fence.Distance = EditorGUILayout.FloatField("Расстояние", fence.Distance);
            }
            else if (fence.SetupColumn == FenceLoft.TypeSetupColumn.Automatic)
            {
                fence.DistanceBetwen = EditorGUILayout.FloatField("Разбивка", fence.DistanceBetwen);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            fence.OffsetColumn = EditorGUILayout.Vector3Field("Смещение столба", fence.OffsetColumn);
            fence._Rotation = EditorGUILayout.Vector3Field("Вращение столба", fence._Rotation);
            fence.ScaleColumn = EditorGUILayout.Vector3Field("Масштаб", fence.ScaleColumn);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            fence.FixedColumnX = GUILayout.Toggle(fence.FixedColumnX, "Фиксировать направление по оси X");
            fence.FixedColumnY = GUILayout.Toggle(fence.FixedColumnY, "Фиксировать направление по оси Y");
            fence.FixedColumnZ = GUILayout.Toggle(fence.FixedColumnZ, "Фиксировать направление по оси Z");
            EditorGUILayout.EndVertical();
        } 
        #endregion
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        #region Span
        spanFoldout = EditorGUILayout.Foldout(spanFoldout, "Настройка пролетов");
        if (spanFoldout)
        {
            EditorGUILayout.BeginVertical("box");
            fence.OffsetSpan = EditorGUILayout.Vector3Field("Смещение пролета", fence.OffsetSpan);
            fence._RotationSpan = EditorGUILayout.Vector3Field("Вращение пролета", fence._RotationSpan);
            fence.ScaleSpan = EditorGUILayout.Vector3Field("Масштаб", fence.ScaleSpan);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            fence.FixedSpanX = GUILayout.Toggle(fence.FixedSpanX, "Фиксировать направление по оси X");
            fence.FixedSpanY = GUILayout.Toggle(fence.FixedSpanY, "Фиксировать направление по оси Y");
            fence.FixedSpanZ = GUILayout.Toggle(fence.FixedSpanZ, "Фиксировать направление по оси Z");
            EditorGUILayout.EndVertical();
        }
        #endregion
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        //Fix = EditorGUILayout.Toggle("Фиксировать", Fix);

        if (GUI.changed /*|| Fix*/)
        {
            if (fence.isInitOK) fence.UpdateMesh();
            else fence.Init();
            EditorUtility.SetDirty(fence);
        }
        if (GUILayout.Button("Обновить"))
        {
            fence.isInitOK = false;
            fence.Init();
            EditorUtility.SetDirty(fence);
        }
        if (GUILayout.Button("Сгенерировать столбы"))
        {
            while (fence.transform.childCount > 0)
                DestroyImmediate(fence.transform.GetChild(0).gameObject);
            fence.GenerateColumn();
            EditorUtility.SetDirty(fence);
        }
    }
}

