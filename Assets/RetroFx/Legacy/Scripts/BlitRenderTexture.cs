using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BlitRenderTexture : MonoBehaviour
{
    public RenderTexture BlitTex;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (BlitTex != null)
            Graphics.Blit(BlitTex, dest);
    }
}