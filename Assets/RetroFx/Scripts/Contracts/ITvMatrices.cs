using UnityEngine;

namespace YooPita.RetroTvFx
{
    public interface ITvMatrices
    {
        public Matrix4x4 Rgb2yiqMatrix { get; }
        public Matrix4x4 Yiq2rgbMatrix { get; }
    }
}