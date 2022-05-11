using UnityEngine;
using System.Collections;

namespace RetroTVFX.Extras
{
    [ExecuteInEditMode]
    public class LoResEffect : MonoBehaviour
    {
        public int ScreenResX = 320;
        public int ScreenResY = 240;

        public bool PointFilter = true;

        public bool OverrideAspect = false;
        public float CamAspect = 1f;

        public Camera MainCam;
        public Camera[] CamArray;

        private RenderTexture _tempTex;

        void OnDestroy()
        {
            cleanupTempTex();
        }

        void cleanupTempTex()
        {
            if (_tempTex != null)
            {
                if (Application.isPlaying) Destroy(_tempTex);
                else DestroyImmediate(_tempTex);
            }
        }

        void createTempTex( int depth, RenderTextureFormat format )
        {
            cleanupTempTex();
            _tempTex = new RenderTexture(ScreenResX, ScreenResY, depth, format);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (MainCam == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            if (_tempTex == null || _tempTex.width != ScreenResX || _tempTex.height != ScreenResY)
            {
                createTempTex(src.depth, src.format);
            }

            float baseAspect = (float)ScreenResX / (float)ScreenResY;
            float aspect = OverrideAspect ? CamAspect : baseAspect;
            
            _tempTex.filterMode = PointFilter ? FilterMode.Point : FilterMode.Bilinear;

            this.MainCam.aspect = aspect;
            this.MainCam.targetTexture = _tempTex;
            this.MainCam.Render();

            if (CamArray != null)
            {
                for (int i = 0; i < CamArray.Length; i++)
                {
                    CamArray[i].aspect = aspect;
                    CamArray[i].targetTexture = _tempTex;
                    CamArray[i].Render();
                }
            }

            Graphics.Blit(_tempTex, dest);
        }
    }
}