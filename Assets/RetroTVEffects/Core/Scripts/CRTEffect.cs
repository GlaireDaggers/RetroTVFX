using UnityEngine;
using System.Collections;

namespace RetroTVFX
{
    public enum FilterQuality
    {
        Filter_8Taps,
        Filter_24Taps,
    }

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

        [Tooltip("Quality of the luma & chroma filters")]
        public FilterQuality FilterQuality = FilterQuality.Filter_8Taps;

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

        private Material _material;

        private RenderTexture _compositeTemp;

        private int _frameCount = 0;

        private float _flickerOffset = 0f;

        private bool _antiFlickerEnabled = false;
        private bool _rollingFlickerEnabled = false;
        private bool _pixelMaskEnabled = false;
        private bool _tvCurvatureEnabled = false;
        private bool _quantizeRGBEnabled = false;
        private bool _rfEnabled = false;

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                Destroy(this._material);
            }
            else
            {
                DestroyImmediate(this._material);
            }

            _material = null;

            _antiFlickerEnabled = false;
            _rollingFlickerEnabled = false;
            _pixelMaskEnabled = false;
            _tvCurvatureEnabled = false;
            _quantizeRGBEnabled = false;
            _rfEnabled = false;

            if (_compositeTemp != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_compositeTemp);
                }
                else
                {
                    DestroyImmediate(_compositeTemp);
                }
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
                this._frameCount++;
                this._frameCount %= 3;
            }

            if (EnableRollingFlicker)
            {
                this._flickerOffset += this.RollingVSyncTime;
            }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            ensureResources();

            setKeyword("ANTI_FLICKER", this.AntiFlicker, ref this._antiFlickerEnabled);
            setKeyword("ROLLING_FLICKER", this.EnableRollingFlicker, ref this._rollingFlickerEnabled);
            setKeyword("PIXEL_MASK", this.EnablePixelMask, ref this._pixelMaskEnabled);
            setKeyword("USE_TV_CURVATURE", this.EnableTVCurvature, ref this._tvCurvatureEnabled);
            setKeyword("QUANTIZE_RGB", this.QuantizeRGB, ref this._quantizeRGBEnabled);
            setKeyword("RF_SIGNAL", this.VideoMode == VideoType.RF, ref this._rfEnabled);

            if (_compositeTemp == null || _compositeTemp.width != DisplaySizeX || _compositeTemp.height != DisplaySizeY)
            {
                if (_compositeTemp != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(_compositeTemp);
                    }
                    else
                    {
                        DestroyImmediate(_compositeTemp);
                    }
                }

                _compositeTemp = new RenderTexture(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
            }

            if (QuantizeRGB)
            {
                Vector4 quantize = new Vector4(Mathf.Pow(2f, RBits), Mathf.Pow(2f, GBits), Mathf.Pow(2f, BBits), 1f);
                Vector4 oneOverQuantize = new Vector4(1f / quantize.x, 1f / quantize.y, 1f / quantize.z, 1f);

                _material.SetVector("_QuantizeRGB", quantize);
                _material.SetVector("_OneOverQuantizeRGB", oneOverQuantize);
            }

			_material.SetFloat("_Realtime", Time.realtimeSinceStartup);

            _material.SetVector("_IQOffset", new Vector4(IQScale.x, IQScale.y, IQOffset.x, IQOffset.y));
            _material.SetMatrix("_RGB2YIQ_MAT", ColorSpaceUtils.RGB2YIQ);
            _material.SetMatrix("_YIQ2RGB_MAT", ColorSpaceUtils.YIQ2RGB);

            _material.SetFloat("_RFNoise", this.RFNoise);
			_material.SetFloat("_LumaSharpen", this.LumaSharpen);

            _material.SetInt("_Framecount", -this._frameCount);
            _material.SetVector("_ScreenSize", new Vector4(DisplaySizeX, DisplaySizeY, 1f / DisplaySizeX, 1f / DisplaySizeY));

            _material.SetFloat("_RollingFlickerAmount", this.RollingFlickerFactor);
            _material.SetVector("_FlickerOffs", new Vector4(this._flickerOffset, this._flickerOffset + RollingVSyncTime, 0f, 0f));

            _material.SetVector("_PixelMaskScale", new Vector4(MaskRepeatX, MaskRepeatY));
            _material.SetTexture("_PixelMask", PixelMaskTexture);
            _material.SetFloat("_Brightness", PixelMaskBrightness);

            _material.SetFloat("_TVCurvature", Curvature);
            

            RenderTexture pass1 = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);
			RenderTexture pass2 = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);

            RenderTexture lastComposite = RenderTexture.GetTemporary(DisplaySizeX, DisplaySizeY, src.depth, RenderTextureFormat.ARGBHalf);

            RenderTexture final = pass1;

            if (this.VideoMode == VideoType.Composite || this.VideoMode == VideoType.RF)
            {
                Graphics.Blit(src, pass1);

				Graphics.Blit(pass1, pass2, _material, PASS_COMPOSITE_ENCODE);

				// pass last frame's signal and save current frame's signal
				Graphics.Blit(_compositeTemp, lastComposite);
				Graphics.Blit(pass2, _compositeTemp);
				_material.SetTexture("_LastCompositeTex", lastComposite);
				
				Graphics.Blit(pass2, pass1, _material, PASS_COMPOSITE_DECODE);
				Graphics.Blit(pass1, pass2, _material, PASS_COMPOSITE_FINAL);

				final = pass2;
			}
            else if (this.VideoMode == VideoType.SVideo)
            {
                Graphics.Blit(src, pass1);

                Graphics.Blit(pass1, pass2, _material, PASS_SVIDEO_ENCODE);

                // pass last frame's signal and save current frame's signal
                Graphics.Blit(_compositeTemp, lastComposite);
                Graphics.Blit(pass2, _compositeTemp);
                _material.SetTexture("_LastCompositeTex", lastComposite);

                Graphics.Blit(pass2, pass1, _material, PASS_SVIDEO_DECODE);
            }
            else if (this.VideoMode == VideoType.VGA)
            {
                Graphics.Blit(src, pass1, _material, PASS_VGA);
            }
            else if (this.VideoMode == VideoType.VGAFast)
            {
                final = src;
            }
            else if (this.VideoMode == VideoType.Component)
            {
                Graphics.Blit(src, pass1, _material, PASS_COMPONENT);
            }

            if (StretchToDisplay)
            {
                GraphicsUtils.Blit(final, dest, _material, PASS_TV_OVERLAY);
            }
            else
            {
                float screenAspect = (float)Screen.width / (float)Screen.height;

                if (screenAspect < AspectRatio)
                {
                    // fit to screen width
                    float width = 1f;
                    float height = screenAspect / AspectRatio;
                    float heightDiff = 1f - height;

                    GraphicsUtils.Blit(new Rect(0f, heightDiff * 0.5f, width, height), final, dest, _material, PASS_TV_OVERLAY);
                }
                else
                {
                    // fit to screen height
                    float height = 1f;
                    float width = (1f / screenAspect) * AspectRatio;
                    float widthDiff = 1f - width;

                    GraphicsUtils.Blit(new Rect(widthDiff * 0.5f, 0f, width, height), final, dest, _material, PASS_TV_OVERLAY);
                }
            }

            RenderTexture.ReleaseTemporary(pass1);
            RenderTexture.ReleaseTemporary(pass2);
            RenderTexture.ReleaseTemporary(lastComposite);
        }

        private void ensureResources()
        {
            if (this._material == null)
            {
                this._material = new Material(shader);

                // this is a little silly but when the material is first created, we set dummy arrays of 32 values to _LumaFilter and _ChromaFilter
                // this is larger than either our 8 or 24 tap filters, so we avoid Unity complaining about not being able to resize arrays this way

                float[] dummy = new float[32];
                _material.SetFloatArray("_LumaFilter", dummy);
                _material.SetFloatArray("_ChromaFilter", dummy);
            }

            _material.SetMatrix("_RGB2YIQ_MAT", ColorSpaceUtils.RGB2YIQ);
            _material.SetMatrix("_YIQ2RGB_MAT", ColorSpaceUtils.YIQ2RGB);

            _material.SetTexture("_OverlayImg", this.TVOverlay);
			
            switch (FilterQuality)
            {
                case FilterQuality.Filter_8Taps:
                    _material.SetFloatArray("_LumaFilter", FilterTaps.lumaFilter8Tap);
                    _material.SetFloatArray("_ChromaFilter", FilterTaps.chromaFilter8Tap);
                    _material.SetInt("_FilterSize", 8);
                    break;
                case FilterQuality.Filter_24Taps:
                    _material.SetFloatArray("_LumaFilter", FilterTaps.lumaFilter24Taps);
                    _material.SetFloatArray("_ChromaFilter", FilterTaps.chromaFilter24Taps);
                    _material.SetInt("_FilterSize", 24);
                    break;
            }
        }

        private void setKeyword(string keyword, bool enabled, ref bool keywordEnabled)
        {
            if (enabled != keywordEnabled)
            {
                if (enabled)
                {
                    _material.EnableKeyword(keyword);
                }
                else
                {
                    _material.DisableKeyword(keyword);
                }
            }

            keywordEnabled = enabled;
        }
    }
}