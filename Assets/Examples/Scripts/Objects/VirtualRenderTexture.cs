using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooPita.RetroTvFx
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
    }
}