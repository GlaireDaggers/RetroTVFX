using UnityEngine;

namespace RetroFx
{
    public class VirtualRenderTexture : VirtualRenderTextureBase, IVirtualRenderTexture
    {
        public VirtualRenderTexture(int width, int height, int depth, RenderTextureFormat format)
        {
            _texture = new RenderTexture(width, height, depth, format);
        }

        public override RenderTexture Texture => _texture;
        private readonly RenderTexture _texture;

        public override void Release()
        {
            _texture.Release();
        }

        public static void AllocateTexture(ref VirtualRenderTexture texture, int width, int height, int depth, RenderTextureFormat format)
        {
            if (texture != null) texture.Release();
            texture = new VirtualRenderTexture(width, height, depth, format);
        }
    }
}