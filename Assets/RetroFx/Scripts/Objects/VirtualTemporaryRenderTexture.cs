using UnityEngine;

namespace RetroFx
{
    public class VirtualTemporaryRenderTexture : VirtualRenderTextureBase, IVirtualRenderTexture
    {
        public VirtualTemporaryRenderTexture(int width, int height, int depth, RenderTextureFormat format)
        {
            _texture = RenderTexture.GetTemporary(width, height, depth, format);
        }

        public override RenderTexture Texture => _texture;
        private readonly RenderTexture _texture;

        public override void Release()
        {
            RenderTexture.ReleaseTemporary(_texture);
        }
    }
}