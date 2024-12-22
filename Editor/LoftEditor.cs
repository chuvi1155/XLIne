using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(SplineLoft))]
class LoftEditor : Editor
{
    SplineLoft loft;
    bool isCleared;
    SerializedProperty material;
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
                    sl.InitLoft();
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

    void OnEnable()
    {
        material = serializedObject.FindProperty("material");
        if (loft == null)
            loft = target as SplineLoft;
        //if (!ClearLoftMeshes.ClearedMeshes.ContainsKey(loft.gameObject))
        //{
        //    ClearLoftMeshes.ClearedMeshes.Add(loft.gameObject, true);
        //    loft.AddCollider = true;
        //}
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //if (Application.isPlaying)
        //{
        //    this.DrawDefaultInspector();
        //    return;
        //}
        //serializedObject.Update();
        //ClearLoftMeshes.ClearedMeshes[loft.gameObject] = GUILayout.Toggle(ClearLoftMeshes.ClearedMeshes[loft.gameObject], "Удаляемый");
        loft.AddCollider = GUILayout.Toggle(loft.AddCollider, "Добавить коллайдер");
        loft.DestroyOnStart = GUILayout.Toggle(loft.DestroyOnStart, "DestroyOnStart");

        EditorGUILayout.BeginVertical("box");
        loft.CurveForm = (XLinePath)EditorGUILayout.ObjectField("Form bezier", loft.CurveForm, typeof(XLinePath), true);
        loft.CurvePath = (XLinePath)EditorGUILayout.ObjectField("Path bezier", loft.CurvePath, typeof(XLinePath), true);
        if (loft.CurvePath != null && loft.CurvePath.Sublines != null && loft.CurvePath.Sublines.Count > 1)
            loft.mergeSubForms = EditorGUILayout.Toggle("Merge sub-line Forms", loft.mergeSubForms);
        //loft.material = (Material)EditorGUILayout.ObjectField("Material", loft.material, typeof(Material), true);
        EditorGUI.indentLevel++;
        EditorGUI.BeginChangeCheck();
        material = serializedObject.FindProperty("material");
        bool showChildren = EditorGUILayout.PropertyField(material, new GUIContent("Materials"));
        if (material.isExpanded)
        {
            material.arraySize = EditorGUILayout.IntField(material.arraySize);
            //while (material.MoveArrayElement(0, material.arraySize))
            //    EditorGUILayout.PropertyField(material, new GUIContent("Materials"));
            for (int i = 0; i < material.arraySize; i++)
            {
                loft.material[i] = (Material)EditorGUILayout.ObjectField("Element_" + i, loft.material[i], typeof(Material), true);
            }
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        EditorGUI.indentLevel--;
        loft.Form2D = GUILayout.Toggle(loft.Form2D, "2D путь");
        if (loft.Form2D)
        {
            loft.UseCanvasRenderer = GUILayout.Toggle(loft.UseCanvasRenderer, "Use CanvasRenderer");
            if (GUI.changed && !Application.isPlaying)
            {
                GUI.changed = false;
            }
        }

        loft.SetPathByPoints = GUILayout.Toggle(loft.SetPathByPoints, "Путь по точкам");
        if (!loft.SetPathByPoints)
            loft.PathQuality = EditorGUILayout.IntField("Детал. пути", loft.PathQuality);
        loft.SetFormByPoints = GUILayout.Toggle(loft.SetFormByPoints, "Форма по точкам");
        if (!loft.SetFormByPoints)
            loft.FormQuality = EditorGUILayout.IntField("Детал. формы", loft.FormQuality);
        loft.UseCenterForm = GUILayout.Toggle(loft.UseCenterForm, "Pivot is center");

        EditorGUILayout.BeginVertical("box");
        loft.MirrorCopyForm = GUILayout.Toggle(loft.MirrorCopyForm, "Зеркальная копия формы");
        loft.mirrorCopyByX = GUILayout.Toggle(loft.mirrorCopyByX, "копия формы по X");
        loft.mirrorCopyByY = GUILayout.Toggle(loft.mirrorCopyByY, "копия формы по Y");
        loft.mirrorCopyByZ = GUILayout.Toggle(loft.mirrorCopyByZ, "копия формы по Z");
        EditorGUILayout.EndVertical();
        loft.Scale = EditorGUILayout.FloatField("Масштаб", loft.Scale);
        loft.Offset = EditorGUILayout.Vector3Field("Смещен. формы", loft.Offset);
        loft.RotateForm = EditorGUILayout.Vector3Field("Вращать форму", loft.RotateForm);

        EditorGUILayout.BeginVertical("box");
        loft.FixedX = GUILayout.Toggle(loft.FixedX, "Фиксировать направление по оси X");
        loft.FixedY = GUILayout.Toggle(loft.FixedY, "Фиксировать направление по оси Y");
        loft.FixedZ = GUILayout.Toggle(loft.FixedZ, "Фиксировать направление по оси Z");
        EditorGUILayout.EndVertical();

        GUILayout.Label("Отразить форму:");
        EditorGUILayout.BeginHorizontal();
        loft.MirrorFormX = GUILayout.Toggle(loft.MirrorFormX, ":X", GUILayout.MaxWidth(40));
        loft.MirrorFormY = GUILayout.Toggle(loft.MirrorFormY, ":Y", GUILayout.MaxWidth(40));
        loft.MirrorFormZ = GUILayout.Toggle(loft.MirrorFormZ, ":Z", GUILayout.MaxWidth(40));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        loft.InvertFace = GUILayout.Toggle(loft.InvertFace, "Инвертирование лицевой стороны");
        loft.IsSmooth = GUILayout.Toggle(loft.IsSmooth, "Сглаживание");
        loft.Tiling = EditorGUILayout.Vector2Field("Tiling", loft.Tiling);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (GUILayout.Button("Сброс"))
        {
            loft.isInitOK = false;
            loft.InitLoft();
            //EditorUtility.SetDirty(loft);
        }
        else if(GUI.changed)
        {
            //if (loft.isInitOK) loft.UpdateMesh();
            //else loft.InitLoft();
            Debug.Log("GUI changed");
            loft.isInitOK = false;
            loft.InitLoft();
            //EditorUtility.SetDirty(loft);
        }
    }
}

