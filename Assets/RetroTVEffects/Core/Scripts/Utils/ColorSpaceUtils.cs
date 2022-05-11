using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroTVFX
{
    public static class ColorSpaceUtils
    {
        public static readonly Matrix4x4 RGB2YIQ = new Matrix4x4(
            new Vector4(0.299f, 0.596f, 0.221f, 0f),
            new Vector4(0.587f, -0.275f, -0.523f, 0f),
            new Vector4(0.114f, -0.321f, 0.311f, 0f),
            new Vector4(0f, 0f, 0f, 0f)
        );

        public static readonly Matrix4x4 YIQ2RGB = new Matrix4x4(
            new Vector4(1f, 1f, 1f, 0f),
            new Vector4(0.956f, -0.272f, -1.106f, 0f),
            new Vector4(0.621f, -0.647f, 1.703f, 0f),
            new Vector4(0f, 0f, 0f, 0f)
        );
    }
}