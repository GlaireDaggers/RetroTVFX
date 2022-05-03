using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooPita.RetroTvFx
{
    public interface IInputSignal
    {
        public VirtualRenderTexture Texture { get; }
    }
}