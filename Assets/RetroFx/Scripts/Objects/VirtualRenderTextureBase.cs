using UnityEngine;

namespace RetroFx
{
    public abstract class VirtualRenderTextureBase : IVirtualRenderTexture
    {
        public int Depth => Texture.depth;

        public RenderTextureFormat Format => Texture.format;

        public int Height => Texture.height;

        public int Width => Texture.width;

        public abstract RenderTexture Texture { get; }

        public void BlitInside(RenderTexture source, Material material, int passes = -1)
        {
            if (source == Texture) throw new System.Exception("Unable to blit texture to itself");
            Graphics.Blit(source, Texture, material, passes);
        }

        public void BlitTo(RenderTexture target, Material material, int passes = -1)
        {
            if (target == Texture) throw new System.Exception("Unable to blit texture to itself");
            Graphics.Blit(Texture, target, material, passes);
        }

        public void BlitTo(IVirtualRenderTexture target, Material material, int passes = -1)
        {
            target.BlitInside(Texture, material, passes);
        }

        public bool CheckCompatibility(int width, int height)
        {
            return Texture.width == width && Texture.height == height;
        }

        public void CopyInside(RenderTexture source)
        {
            if (source == Texture) throw new System.Exception("Unable to copy texture to itself");
            Graphics.Blit(source, Texture);
        }

        public void CopyTo(RenderTexture target)
        {
            if (target == Texture) throw new System.Exception("Unable to copy texture to itself");
            Graphics.Blit(Texture, target);
        }

        public void CopyTo(IVirtualRenderTexture target)
        {
            target.CopyInside(Texture);
        }

        public virtual void Release()
        {
            Texture.Release();
        }

        public void SetFilterMode(FilterMode filterMode)
        {
            Texture.filterMode = filterMode;
        }
    }
}