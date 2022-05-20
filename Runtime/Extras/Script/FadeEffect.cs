using UnityEngine;
using System.Collections;

namespace RetroTVFX.Extras
{
    [ExecuteInEditMode]
    public class FadeEffect : MonoBehaviour
    {
        [HideInInspector]
        public Shader FadeShader;

        public Color FadeColor = Color.black;

        [Range(0f, 1f)]
        public float FadeSeparation = 0.5f;

        [Range(0f, 1f)]
        public float FadeFactor = 0f;
        
        private Material _mat;

        void OnDisable()
        {
            if (Application.isPlaying)
                Destroy(_mat);
            else
                DestroyImmediate(_mat);
        }

        float eval(float input, float start, float end)
        {
            return Mathf.Clamp01((input - start) / (end - start));
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_mat == null)
                _mat = new Material(FadeShader);

            float sep = FadeSeparation * 0.66f;
            float r = eval(FadeFactor, 0f, 1f - sep);
            float g = eval(FadeFactor, sep * 0.5f, 1f - (sep * 0.5f));
            float b = eval(FadeFactor, sep, 1f);

            _mat.SetColor("_FadeColor", FadeColor);
            _mat.SetVector("_FadeFactor", new Vector4(r, g, b, 0.0f));

            Graphics.Blit(src, dest, _mat, 0);
        }
    }
}