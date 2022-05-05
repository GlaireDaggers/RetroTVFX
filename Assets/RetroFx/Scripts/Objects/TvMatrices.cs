using UnityEngine;

namespace YooPita.RetroTvFx
{
    public class TvMatrices : ITvMatrices
    {
        public TvMatrices()
        {
            _rgb2yiqMatrix.SetRow(0, new Vector4(0.299f, 0.587f, 0.114f, 0f));
            _rgb2yiqMatrix.SetRow(1, new Vector4(0.596f, -0.275f, -0.321f, 0f));
            _rgb2yiqMatrix.SetRow(2, new Vector4(0.221f, -0.523f, 0.311f, 0f));

            _yiq2rgbMatrix.SetRow(0, new Vector4(1f, 0.956f, 0.621f, 0f));
            _yiq2rgbMatrix.SetRow(1, new Vector4(1f, -0.272f, -0.647f, 0f));
            _yiq2rgbMatrix.SetRow(2, new Vector4(1f, -1.106f, 1.703f, 0f));
        }

        public Matrix4x4 Rgb2yiqMatrix => _rgb2yiqMatrix;

        public Matrix4x4 Yiq2rgbMatrix => _yiq2rgbMatrix;

        private Matrix4x4 _rgb2yiqMatrix;
        private Matrix4x4 _yiq2rgbMatrix;
    }
}