namespace JetFistGames.RetroTVFX
{

    using UnityEngine;
    using System.Collections;

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
        
        private Material mat;

        void OnDisable()
        {
            if (Application.isPlaying)
                Destroy(mat);
            else
                DestroyImmediate(mat);
        }

        float eval(float input, float start, float end)
        {
            return Mathf.Clamp01((input - start) / (end - start));
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (mat == null)
                mat = new Material(FadeShader);

            float sep = FadeSeparation * 0.66f;
            float r = eval(FadeFactor, 0f, 1f - sep);
            float g = eval(FadeFactor, sep * 0.5f, 1f - (sep * 0.5f));
            float b = eval(FadeFactor, sep, 1f);

            mat.SetColor("_FadeColor", FadeColor);
            mat.SetVector("_FadeFactor", new Vector4(r, g, b, 0.0f));

            Graphics.Blit(src, dest, mat, 0);
        }
    }

}