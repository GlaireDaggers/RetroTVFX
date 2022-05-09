using UnityEngine;

namespace RetroFx
{
    public interface IVirtualRenderTexture
    {
        int Depth { get; }
        RenderTextureFormat Format { get; }
        int Height { get; }
        RenderTexture Texture { get; }
        int Width { get; }

        void BlitInside(RenderTexture source, Material material, int passes = -1);
        void BlitTo(RenderTexture target, Material material, int passes = -1);
        void BlitTo(IVirtualRenderTexture target, Material material, int passes = -1);
        bool CheckCompatibility(int width, int height);
        void CopyInside(RenderTexture source);
        void CopyTo(RenderTexture target);
        void CopyTo(IVirtualRenderTexture target);
        void Release();
        void SetFilterMode(FilterMode filterMode);
    }
}