using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

class AutomaticCreater : EditorWindow
{
    XLinePath path, form;
    Material fenceMat, stolbMat;

    [MenuItem("Chuvi/Loft/Fence automatic")]
    public static void RunCreateFenceAutomatic()
    {
        EditorWindow.GetWindow(typeof(AutomaticCreater), false, "Automatic create", true);
    }

    void OnInspectorUpdate()
    {
        this.Repaint();
        if (Selection.activeGameObject != null)
            path = Selection.activeGameObject.GetComponent<XLinePath>();
        else path = null;
    }

    void OnGUI()
    {
        form = (XLinePath)EditorGUILayout.ObjectField("Form", form, typeof(XLinePath), true);
        EditorGUILayout.ObjectField("Path", path, typeof(XLinePath), true);
        fenceMat = (Material)EditorGUILayout.ObjectField("Материал забора", fenceMat, typeof(Material), true);
        stolbMat = (Material)EditorGUILayout.ObjectField("Материал столба", stolbMat, typeof(Material), true);
        if (path != null)
        {
            if (GUILayout.Button("Create"))
            {
                //AntaresBezierCurve[] objs = new AntaresBezierCurve[2];
                //objs[0] = path;
                //objs[1] = form;
                //Selection.objects = objs;
                FenceLoft fence = FenceEditor.RunCreateFence(path, form);
                if (fence != null)
                {
                    Undo.RegisterCreatedObjectUndo(fence.gameObject, "Create fence");
                    fence.GetComponent<Renderer>().sharedMaterial = fenceMat;
                    fence.ColumnMat = stolbMat;
                }
            }
        }
    }
}

