using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(SplineLoft))]
class LoftEditor : Editor
{
    SplineLoft loft;
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
    SerializedProperty DestroyOnStart;
    SerializedProperty CurveForm;
    SerializedProperty CurvePath;
    SerializedProperty mergeSubForms;
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
        DestroyOnStart = serializedObject.FindProperty("DestroyOnStart");
        CurveForm = serializedObject.FindProperty("CurveForm");
        CurvePath = serializedObject.FindProperty("CurvePath");
        mergeSubForms = serializedObject.FindProperty("mergeSubForms");
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
        EditorGUILayout.PropertyField(CurveForm, new GUIContent("Форма"));
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
        //EditorGUILayout.PropertyField(mergeSubForms, new GUIContent("Merge sub-line Forms"));

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
}

