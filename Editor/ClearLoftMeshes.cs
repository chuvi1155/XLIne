using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

class ClearLoftMeshes : MonoBehaviour
{
    public static Dictionary<GameObject, bool> ClearedMeshes = new Dictionary<GameObject,bool>();
    [MenuItem("Chuvi/Loft/Clear for start")]
    static void ClearForStart()
    {
        foreach (KeyValuePair<GameObject, bool> p in ClearedMeshes)
        {
            if (p.Value)
            {
                if (p.Key != null)
                {
                    if (p.Key.GetComponent<MeshFilter>() != null)
                    {
                        DestroyImmediate(p.Key.GetComponent<MeshFilter>().sharedMesh, true);
                    }
                    if (p.Key.GetComponent<FenceLoft>() != null)
                    {
                        p.Key.GetComponent<FenceLoft>().Clear();
                    }
                }
            }
        }
        ClearedMeshes.Clear();
        Selection.objects = new UnityEngine.Object[0];
    }
	
	[MenuItem("Chuvi/Loft/Clear for start (by search)")]
	static void ClearForStart2()
	{
		SplineLoft[] objs = Object.FindObjectsByType<SplineLoft>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (SplineLoft obj in objs)
		{
			if (obj.GetComponent<MeshFilter>() != null)
            {
                DestroyImmediate(obj.GetComponent<MeshFilter>().sharedMesh, true);
            }
            if (obj.GetComponent<FenceLoft>() != null)
            {
                obj.GetComponent<FenceLoft>().Clear();
            }
		}
	}
}

