namespace JetFistGames.RetroTVFX
{
    using UnityEngine;
    using System.Collections;

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

        private RenderTexture tempTex;

        void OnDestroy()
        {
            cleanupTempTex();
        }

        void cleanupTempTex()
        {
            if (tempTex != null)
            {
                if (Application.isPlaying) Destroy(tempTex);
                else DestroyImmediate(tempTex);
            }
        }

        void createTempTex( int depth, RenderTextureFormat format )
        {
            cleanupTempTex();
            tempTex = new RenderTexture(ScreenResX, ScreenResY, depth, format);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (MainCam == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            if (tempTex == null || tempTex.width != ScreenResX || tempTex.height != ScreenResY)
            {
                createTempTex(src.depth, src.format);
            }

            float baseAspect = (float)ScreenResX / (float)ScreenResY;
            float aspect = OverrideAspect ? CamAspect : baseAspect;
            
            tempTex.filterMode = PointFilter ? FilterMode.Point : FilterMode.Bilinear;

            this.MainCam.aspect = aspect;
            this.MainCam.targetTexture = tempTex;
            this.MainCam.Render();

            if (CamArray != null)
            {
                for (int i = 0; i < CamArray.Length; i++)
                {
                    CamArray[i].aspect = aspect;
                    CamArray[i].targetTexture = tempTex;
                    CamArray[i].Render();
                }
            }

            Graphics.Blit(tempTex, dest);
        }
    }
}