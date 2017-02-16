#define DECODE_FILTER_TAPS_8
// #define DECODE_FILTER_TAPS_24

namespace JetFistGames.RetroTVFX
{
	
	using UnityEngine;
    using System.Collections;

    public enum VideoType
    {
        /// <summary>
        /// RF takes YIQ and muxes luma and chroma into a single signal (actually, real RF also includes audio)
        /// Real RF then modulates it with a radio wave carrier. It's subject to extra blurring and noise
        /// </summary>
        RF,

        /// <summary>
        /// Composite takes YIQ and muxes luma and chroma into a single signal
        /// Slightly less color blurring than RF and no noise, but still fairly blurry
        /// </summary>
        Composite,

        /// <summary>
        /// S-Video takes YIQ and separates it into two signals - a luma signal, and a muxed chroma signal
        /// Closer to Component, but with some color bleeding still present
        /// </summary>
        SVideo,

        /// <summary>
        /// Component takes YIQ information and sends it over three different cables.
        /// Since there's no signal multiplexing, the output is very clean.
        /// </summary>
        Component,

        /// <summary>
        /// VGA (and SCART) can transmit pure un-muxed RGB color.
        /// This will result in image quality nearly identical to the input. Used in arcade games.
        /// </summary>
        VGA,

        /// <summary>
        /// A more efficient version of VGA/SCART. Just blits directly to the screen.
        /// </summary>
        VGAFast,
    }

    [ExecuteInEditMode]
    public class CRTEffect : MonoBehaviour
    {
        private const int PASS_COMPOSITE_ENCODE = 0;
        private const int PASS_COMPOSITE_DECODE = 1;
		private const int PASS_COMPOSITE_FINAL = 2;

		private const int PASS_VGA = 4;
        private const int PASS_COMPONENT = 5;

        private const int PASS_SVIDEO_ENCODE = 6;
        private const int PASS_SVIDEO_DECODE = 7;

        private const int PASS_TV_OVERLAY = 3;

        [Header("Shader Properties")]

        [HideInInspector]
        public Shader shader;

        [Tooltip("Which style of video to use")]
        public VideoType VideoMode = VideoType.Composite;

        public int DisplaySizeX = 960;
        public int DisplaySizeY = 480;

        public bool StretchToDisplay = true;

        public float AspectRatio = 1.33f;

        [Tooltip("Apply curvature to display")]
        public bool EnableTVCurvature = false;

        [Range(0f, 1f)]
        public float Curvature = 0f;

        [Tooltip("Overlay image applied (before curvature)")]
        public Texture2D TVOverlay;

        public bool EnablePixelMask = true;
        public Texture2D PixelMaskTexture;
        public int MaskRepeatX = 160;
        public int MaskRepeatY = 90;

        [Range(1f, 2f)]
        public float PixelMaskBrightness = 1f;

        public Vector2 IQOffset = Vector2.zero;
        public Vector2 IQScale = Vector2.one;

        [Range(0f, 2f)]
        public float RFNoise = 0.25f;

		[Range(0f, 4f)]
		public float LumaSharpen = 0f;

        public bool QuantizeRGB = false;

        [Range(2, 8)]
        public int RBits = 8;

        [Range(2, 8)]
        public int GBits = 8;

        [Range(2, 8)]
        public int BBits = 8;

        public bool EnableBurstCountAnimation = true;
        public bool AntiFlicker = false;

        public bool EnableRollingFlicker = false;

        [Range(0f, 1f)]
        public float RollingFlickerFactor = 0.25f;

        [Range(0f, 2f)]
        public float RollingVSyncTime = 1f;

        private Material material;

        private RenderTexture compositeTemp;

        private int frameCount = 0;

        private float flickerOffset = 0f;

        private bool antiFlickerEnabled = false;
        private bool rollingFlickerEnabled = false;
        private bool pixelMaskEnabled = false;
        private bool tvCurvatureEnabled = false;
        private bool quantizeRGBEnabled = false;
        private bool rfEnabled = false;

		#region Decode filter kernel
#if DECODE_FILTER_TAPS_8
		private float[] lumaFilter =
		{
		   -0.0020f, -0.0009f, 0.0038f, 0.0178f, 0.0445f,
			0.0817f, 0.1214f, 0.1519f, 0.1634f
		};

		private float[] chromaFilter =
		{
			0.0046f, 0.0082f, 0.0182f, 0.0353f, 0.0501f,
			0.0832f, 0.1062f, 0.1222f, 0.1280f
		};
#else
		private float[] lumaFilter = new float[]
		{
			-0.000012020f,
			-0.000022146f,
			-0.000013155f,
			-0.000012020f,
			-0.000049979f,
			-0.000113940f,
			-0.000122150f,
			-0.000005612f,
			0.000170516f,
			0.000237199f,
			0.000169640f,
			0.000285688f,
			0.000984574f,
			0.002018683f,
			0.002002275f,
			-0.000909882f,
			-0.007049081f,
			-0.013222860f,
			-0.012606931f,
			0.002460860f,
			0.035868225f,
			0.084016453f,
			0.135563500f,
			0.175261268f,
			0.190176552f
		};

		private float[] chromaFilter = new float[]
		{
			-0.000118847f,
			-0.000271306f,
			-0.000502642f,
			-0.000930833f,
			-0.001451013f,
			-0.002064744f,
			-0.002700432f,
			-0.003241276f,
			-0.003524948f,
			-0.003350284f,
			-0.002491729f,
			-0.000721149f,
			0.002164659f,
			0.006313635f,
			0.011789103f,
			0.018545660f,
			0.026414396f,
			0.035100710f,
			0.044196567f,
			0.053207202f,
			0.061590275f,
			0.068803602f,
			0.074356193f,
			0.077856564f,
			0.079052396f
		};
#endif
		#endregion

		private Matrix4x4 rgb2yiq_mat = Matrix4x4.identity;
        private Matrix4x4 yiq2rgb_mat = Matrix4x4.identity;

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                Destroy(this.material);
            }
            else
            {
                DestroyImmediate(this.material);
            }

            antiFlickerEnabled = false;
            rollingFlickerEnabled = false;
            pixelMaskEnabled = false;
            tvCurvatureEnabled = false;
            quantizeRGBEnabled = false;
            rfEnabled = false;

            if (compositeTemp != null)
            {
                if (Application.isPlaying)
                    Destroy(compositeTemp);
                else
                    DestroyImmediate(compositeTemp);
            }
        }

        void Update()
        {
            ensureResources();
        }

        void LateUpdate()
        {
            if (EnableBurstCountAnimation)
            {
                this.frameCount++;
                this.frameCount %= 3;
            }

            if (EnableRollingFlicker)
            {
                this.flickerOffset += this.RollingVSyncTime;
            }
        }

        void ensureResources()
        {
            if (this.rgb2yiq_mat == Matrix4x4.identity)
            {
                this.rgb2yiq_mat.SetRow(0, new Vector4(0.299f, 0.587f, 0.114f, 0f));
                this.rgb2yiq_mat.SetRow(1, new Vector4(0.596f, -0.275f, -0.321f, 0f));
                this.rgb2yiq_mat.SetRow(2, new Vector4(0.221f, -0.523f, 0.311f, 0f));
            }

            if (this.yiq2rgb_mat == Matrix4x4.identity)
            {
                this.yiq2rgb_mat.SetRow(0, new Vector4(1f, 0.956f, 0.621f, 0f));
                this.yiq2rgb_mat.SetRow(1, new Vector4(1f, -0.272f, -0.647f, 0f));
                this.yiq2rgb_mat.SetRow(2, new Vector4(1f, -1.106f, 1.703f, 0f));
            }

            if (this.material == null)
            {
                this.material = new Material(shader);
            }

            material.SetMatrix("_RGB2YIQ_MAT", this.rgb2yiq_mat);
            material.SetMatrix("_YIQ2RGB_MAT", this.yiq2rgb_mat);

            material.SetTexture("_OverlayImg", this.TVOverlay);
			
            material.SetFloatArray("_LumaFilter", this.lumaFilter);
            material.SetFloatArray("_ChromaFilter", this.chromaFilter);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            ensureResources();

            setKeyword("ANTI_FLICKER", this.AntiFlicker, ref this.antiFlickerEnabled);
            setKeyword("ROLLING_FLICKER", this.EnableRollingFlicker, ref this.rollingFlickerEnabled);
            setKeyword("PIXEL_MASK", this.EnablePixelMask, ref this.pixelMaskEnabled);
            setKeyword("USE_TV_CURVATURE", this.EnableTVCurvature, ref this.tvCurvatureEnabled);
            setKeyword("QUANTIZE_RGB", this.QuantizeRGB, ref this.quantizeRGBEnabled);
            setKeyword("RF_SIGNAL", this.VideoMode == VideoType.RF, ref this.rfEnabled);

            if (compositeTemp == null || compositeTemp.width != DisplaySizeX || compositeTemp.height != DisplaySizeY)
            {
                if (compositeTemp != null)
                    RenderTexture.ReleaseTemporary(compositeTemp);

                compositeTemp = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
            }

            if (QuantizeRGB)
            {
                Vector4 quantize = new Vector4(Mathf.Pow(2f, RBits), Mathf.Pow(2f, GBits), Mathf.Pow(2f, BBits), 1f);
                Vector4 oneOverQuantize = new Vector4(1f / quantize.x, 1f / quantize.y, 1f / quantize.z, 1f);

                material.SetVector("_QuantizeRGB", quantize);
                material.SetVector("_OneOverQuantizeRGB", oneOverQuantize);
            }

			material.SetFloat("_Realtime", Time.realtimeSinceStartup);

            material.SetVector("_IQOffset", new Vector4(IQScale.x, IQScale.y, IQOffset.x, IQOffset.y));
            material.SetMatrix("_RGB2YIQ_MAT", this.rgb2yiq_mat);
            material.SetMatrix("_YIQ2RGB_MAT", this.yiq2rgb_mat);

            material.SetFloat("_RFNoise", this.RFNoise);
			material.SetFloat("_LumaSharpen", this.LumaSharpen);

            material.SetInt("_Framecount", -this.frameCount);
            material.SetVector("_ScreenSize", new Vector4(DisplaySizeX, DisplaySizeY, 1f / DisplaySizeX, 1f / DisplaySizeY));

            material.SetFloat("_RollingFlickerAmount", this.RollingFlickerFactor);
            material.SetVector("_FlickerOffs", new Vector4(this.flickerOffset, this.flickerOffset + RollingVSyncTime, 0f, 0f));

            material.SetVector("_PixelMaskScale", new Vector4(MaskRepeatX, MaskRepeatY));
            material.SetTexture("_PixelMask", PixelMaskTexture);
            material.SetFloat("_Brightness", PixelMaskBrightness);

            material.SetFloat("_TVCurvature", Curvature);
            

            RenderTexture pass1 = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
			//pass1.filterMode = FilterMode.Point;
			RenderTexture pass2 = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
			//pass2.filterMode = FilterMode.Point;

            RenderTexture lastComposite = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);

            RenderTexture final = pass1;

            if (this.VideoMode == VideoType.Composite || this.VideoMode == VideoType.RF)
            {
                Graphics.Blit(src, pass1);

				Graphics.Blit(pass1, pass2, material, PASS_COMPOSITE_ENCODE);

				// pass last frame's signal and save current frame's signal
				Graphics.Blit(compositeTemp, lastComposite);
				Graphics.Blit(pass2, compositeTemp);
				material.SetTexture("_LastCompositeTex", lastComposite);
				
				Graphics.Blit(pass2, pass1, material, PASS_COMPOSITE_DECODE);
				Graphics.Blit(pass1, pass2, material, PASS_COMPOSITE_FINAL);

				final = pass2;
			}
            else if (this.VideoMode == VideoType.SVideo)
            {
                Graphics.Blit(src, pass1);

                Graphics.Blit(pass1, pass2, material, PASS_SVIDEO_ENCODE);

                // pass last frame's signal and save current frame's signal
                Graphics.Blit(compositeTemp, lastComposite);
                Graphics.Blit(pass2, compositeTemp);
                material.SetTexture("_LastCompositeTex", lastComposite);

                Graphics.Blit(pass2, pass1, material, PASS_SVIDEO_DECODE);
            }
            else if (this.VideoMode == VideoType.VGA)
            {
                Graphics.Blit(src, pass1, material, PASS_VGA);
            }
            else if (this.VideoMode == VideoType.VGAFast)
            {
                final = src;
            }
            else if (this.VideoMode == VideoType.Component)
            {
                Graphics.Blit(src, pass1, material, PASS_COMPONENT);
            }

            //Graphics.Blit(final, dest, material, PASS_TV_OVERLAY);
            if (StretchToDisplay)
                blitQuad(final, dest, material, PASS_TV_OVERLAY);
            else
            {
                float screenAspect = (float)Screen.width / (float)Screen.height;

                if (screenAspect < AspectRatio)
                {
                    // fit to screen width
                    float width = 1f;
                    float height = screenAspect / AspectRatio;
                    float heightDiff = 1f - height;

                    blitQuad(new Rect(0f, heightDiff * 0.5f, width, height), final, dest, material, PASS_TV_OVERLAY);
                }
                else
                {
                    // fit to screen height
                    float height = 1f;
                    float width = (1f / screenAspect) * AspectRatio;
                    float widthDiff = 1f - width;

                    blitQuad(new Rect(widthDiff * 0.5f, 0f, width, height), final, dest, material, PASS_TV_OVERLAY);
                }
            }

            RenderTexture.ReleaseTemporary(pass1);
            RenderTexture.ReleaseTemporary(pass2);
            RenderTexture.ReleaseTemporary(lastComposite);
        }

        private void setKeyword(string keyword, bool enabled, ref bool keywordEnabled)
        {
            if (enabled != keywordEnabled)
            {
                if (enabled)
                    material.EnableKeyword(keyword);
                else
                    material.DisableKeyword(keyword);
            }

            keywordEnabled = enabled;
        }

        private void blitQuad(RenderTexture src, RenderTexture dest, Material material, int pass)
        {
            blitQuad(new Rect(0f, 0f, 1f, 1f), src, dest, material, pass);
        }

        private void blitQuad(Rect rect, RenderTexture src, RenderTexture dest, Material material, int pass)
        {
            GL.PushMatrix();
            GL.LoadOrtho();

            RenderTexture.active = dest;
            GL.Clear(true, true, Color.black);

            material.SetTexture("_MainTex", src);
            material.SetPass(pass);
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);
            GL.TexCoord2(0, 0);
            GL.Vertex3(rect.x, rect.y, 0.1f);

            GL.TexCoord2(1, 0);
            GL.Vertex3(rect.xMax, rect.y, 0.1f);

            GL.TexCoord2(1, 1);
            GL.Vertex3(rect.xMax, rect.yMax, 0.1f);

            GL.TexCoord2(0, 1);
            GL.Vertex3(rect.x, rect.yMax, 0.1f);
            GL.End();

            GL.PopMatrix();
        }
    }

}