using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class OverrideAspectRatio : MonoBehaviour
{
    public bool OverrideAspect = true;
    public float AspectRatio = 1f;

    private Camera myCam;

    void LateUpdate()
    {
       
    }

    void OnPreCull()
    {
        if (myCam == null)
        {
            myCam = GetComponent<Camera>();
        }

        if (OverrideAspect)
            myCam.aspect = AspectRatio;
        else
            myCam.ResetAspect();
    }
}