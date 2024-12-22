using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LineSpace))]
class LineSpaceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Color gui_col = GUI.color;
        GUI.color = (target as LineSpace).IsEmulateInEditor ? Color.yellow : gui_col;
        if (GUILayout.Button("Emulate"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                (targets[i] as LineSpace).IsEmulateInEditor = !(targets[i] as LineSpace).IsEmulateInEditor;
            }
        }
        GUI.color = gui_col;
    }
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Chuvi/Line/_Modificators/LineSpace")]
public class LineSpace : MonoBehaviour 
{
	public XLinePath curve;
    public bool AvtoMove = true;
    public float Distantion;
    public int subline = 0;
    public float Speed = 5;
    public bool IsFollowed = false;
    Vector3 pos, vel;

    public float wheelRadius = 1;
    public float wheelDirections = -1;
    public Transform[] frontWheels;
    public Transform[] rearWheels;
    float oldDist = 0;
    float ang = 0;

#if UNITY_EDITOR
    bool isEmulateInEditor = false;
    double dt;
    public bool IsEmulateInEditor
    {
        get { return isEmulateInEditor; }
        set
        {
            isEmulateInEditor = value;
            if (isEmulateInEditor)
            {
                dt = EditorApplication.timeSinceStartup;
                EditorApplication.update += Emulator;
            }
            else
            {
                EditorApplication.update -= Emulator;
            }
        }
    }

    void Emulator()
    {
        if (EditorApplication.isPlaying) return;
        if (transform == null)
        {
            IsEmulateInEditor = false;
            return;
        }
        float _dt = (float)(EditorApplication.timeSinceStartup - dt);
        Distantion += _dt * Speed;
        dt = EditorApplication.timeSinceStartup;
    }
#endif

    void Update () 
	{
        if (curve == null || !curve.enabled || !curve.gameObject.activeSelf) return;
        if (Application.isPlaying && AvtoMove)
            Distantion += Time.deltaTime * Speed;
        curve.GetInterpolatedValues(subline, Distantion, out pos, out vel, out Vector3 acc, out Vector3 up);
        transform.position = pos;
        if (IsFollowed)
        {
            //transform.forward = vel;
            //transform.right = acc;
            //transform.LookAt(transform.position + vel, Vector3.Cross(vel.normalized, acc.normalized));
            transform.LookAt(transform.position + vel, up);
        }

        Debug.DrawRay(pos, vel * 10f, Color.blue);
        Debug.DrawRay(pos, acc * 10f, Color.red);

        if (frontWheels != null && frontWheels.Length > 0)
        {
            if(wheelRadius != 0)
            ang = (((Distantion - oldDist) / wheelRadius) * Mathf.Rad2Deg) * wheelDirections;
            for (int i = 0; i < frontWheels.Length; i++)
            {
                frontWheels[i].Rotate(ang, 0, 0, Space.Self);
                //frontWheels[i].localEulerAngles += new Vector3(ang, 0, 0);
            }
            for (int i = 0; i < rearWheels.Length; i++)
            {
                rearWheels[i].Rotate(ang, 0, 0, Space.Self);
                //rearWheels[i].localEulerAngles += new Vector3(ang, 0, 0);
            }
        }
        oldDist = Distantion;
    }

    public Vector3 GetPosition()
    {
        return pos;
    }
    public Vector3 GetDirection()
    {
        return vel;
    }
}
