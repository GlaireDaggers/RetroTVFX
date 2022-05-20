using UnityEngine;
using System.Collections;

namespace RetroTVFX.Extras
{
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
}