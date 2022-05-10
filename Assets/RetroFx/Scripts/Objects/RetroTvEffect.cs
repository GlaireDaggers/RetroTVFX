using UnityEngine;

namespace RetroFx
{
    public class RetroTvEffect
    {
        public RetroTvEffect()
        {
            var ntscEffectShader = Shader.Find("Hidden/NtscEffect");
            _ntscEffectMaterial = new Material(ntscEffectShader);
        }

        public VideoMode Mode = VideoMode.Composite;
        public int Width = 320;
        public int Height = 240;
        public bool StretchToDisplay = true;
        public float AspectRatio = 1.33f;

        [Tooltip("Apply curvature to display")]
        public bool EnableTvCurvature = false;
        [Range(0f, 1f)] public float Curvature = 0f;

        [Tooltip("Overlay image applied (before curvature)")]
        public Texture2D TvOverlay;
        public bool EnablePixelMask = true;
        public Texture2D PixelMaskTexture;
        public Vector2 PixelPerMask = new Vector2(1, 1);
        [Range(1f, 2f)] public float PixelMaskBrightness = 1f;
        public Vector2 IqOffset = Vector2.zero;
        public Vector2 IqScale = Vector2.one;
        [Range(0f, 2f)] public float RfNoise = 0.25f;
        [Range(0f, 4f)] public float LumaSharpen = 0f;
        public bool QuantizeRGB = false;
        [Range(2, 8)] public int RBits = 8;
        [Range(2, 8)] public int GBits = 8;
        [Range(2, 8)] public int BBits = 8;
        public bool EnableBurstCountAnimation = true;
        public bool AntiFlicker = false;
        public bool EnableRollingFlicker = false;
        [Range(0f, 1f)] public float RollingFlickerFactor = 0.25f;
        [Range(0f, 2f)] public float RollingVSyncTime = 1f;
        public FilterKernelTaps FilterKernel = FilterKernelTaps.FilterKernelTaps8;
        public bool SmoothRender = true;

        private float CalculatedCurvature => Curvature;

        private const int _passCompositeEncode = 0;
        private const int _passCompositeDecode = 1;
        private const int _passCompositeFinal = 2;

        private const int _passVga = 4;
        private const int _passComponent = 5;

        private const int _passSvideoEncode = 6;
        private const int _passSvideoDecode = 7;

        private const int _passTvOverlay = 3;

        private Material _ntscEffectMaterial;

        private int _frameCount = 0;

        private float _flickerOffset = 0f;

		private bool _antiFlickerEnabled;
		private bool _rollingFlickerEnabled;
		private bool _pixelMaskEnabled;
		private bool _tvCurvatureEnabled;
		private bool _quantizeRGBEnabled;
		private bool _rfEnabled;

        private IFilterKernelTaps _filterKernelTaps = new FilterKernelTaps8();

        private ITvMatrices _tvMatrices = new TvMatrices();

        private IVirtualRenderTexture _compositeTemp;

        public void Blit(RenderTexture input, RenderTexture output)
        {
            AllocateTemporaryTextureByPreset(ref _compositeTemp);
            BlitByCurrentMode(input, output);
            DoStretchToDisplay(output);
            LastUpdate();
        }

        public void UpdateValues()
        {
            DoQuantizeRGB();

            UpdateFilterKernelTaps();

            SetBoolKeyword("ANTI_FLICKER", AntiFlicker, ref _antiFlickerEnabled);
            SetBoolKeyword("ROLLING_FLICKER", EnableRollingFlicker, ref _rollingFlickerEnabled);
            SetBoolKeyword("PIXEL_MASK", EnablePixelMask, ref _pixelMaskEnabled);
            SetBoolKeyword("USE_TV_CURVATURE", EnableTvCurvature, ref _tvCurvatureEnabled);
            SetBoolKeyword("QUANTIZE_RGB", QuantizeRGB, ref _quantizeRGBEnabled);
            SetBoolKeyword("RF_SIGNAL", Mode == VideoMode.RF, ref _rfEnabled);

            _ntscEffectMaterial.SetMatrix("_RGB2YIQ_MAT", _tvMatrices.Rgb2yiqMatrix);
            _ntscEffectMaterial.SetMatrix("_YIQ2RGB_MAT", _tvMatrices.Yiq2rgbMatrix);

            _ntscEffectMaterial.SetTexture("_OverlayImg", TvOverlay);

            _ntscEffectMaterial.SetFloatArray("_LumaFilter", _filterKernelTaps.LumaFilter);
            _ntscEffectMaterial.SetFloatArray("_ChromaFilter", _filterKernelTaps.ChromaFilter);
            _ntscEffectMaterial.SetFloat("_Realtime", Time.realtimeSinceStartup);

            _ntscEffectMaterial.SetVector("_IQOffset", new Vector4(IqScale.x, IqScale.y, IqOffset.x, IqOffset.y));

            _ntscEffectMaterial.SetFloat("_RFNoise", RfNoise);
            _ntscEffectMaterial.SetFloat("_LumaSharpen", LumaSharpen);

            _ntscEffectMaterial.SetInt("_Framecount", -_frameCount);
            _ntscEffectMaterial.SetVector("_ScreenSize", new Vector4(Width, Height, 1f / Width, 1f / Height));

            _ntscEffectMaterial.SetFloat("_RollingFlickerAmount", RollingFlickerFactor);
            _ntscEffectMaterial.SetVector("_FlickerOffs", new Vector4(_flickerOffset, _flickerOffset + RollingVSyncTime, 0f, 0f));

            _ntscEffectMaterial.SetVector("_PixelMaskScale", new Vector4(Width / PixelPerMask.x, Height / PixelPerMask.y));
            _ntscEffectMaterial.SetTexture("_PixelMask", PixelMaskTexture);
            _ntscEffectMaterial.SetFloat("_Brightness", PixelMaskBrightness);

            _ntscEffectMaterial.SetFloat("_TVCurvature", CalculatedCurvature);
        }

        private void LastUpdate()
        {
            _ntscEffectMaterial.SetFloat("_Realtime", Time.realtimeSinceStartup);

            if (EnableBurstCountAnimation)
            {
                _frameCount = (_frameCount + 1) % 3;
                _ntscEffectMaterial.SetInt("_Framecount", -_frameCount);
            }

            if (EnableRollingFlicker)
            {
                _flickerOffset += RollingVSyncTime;
                _ntscEffectMaterial.SetVector("_FlickerOffs", new Vector4(
                _flickerOffset,
                _flickerOffset + RollingVSyncTime,
                0f,
                0f));
            }
        }

        private void AllocateTemporaryTextureByPreset(ref IVirtualRenderTexture texture)
        {
            if (texture == null || !texture.CheckCompatibility(Width, Height))
            {
                if (texture != null) texture.Release();
                texture = new VirtualTemporaryRenderTexture(Width, Height, 24, RenderTextureFormat.ARGBHalf);
                texture.SetFilterMode(CalculateFilterMode());
            }
        }

        private VirtualTemporaryRenderTexture GetTemporaryTextureByTexture(RenderTexture renderTexture)
        {
            var tempTexture = new VirtualTemporaryRenderTexture(renderTexture.width, renderTexture.height, 24, RenderTextureFormat.ARGBHalf);
            tempTexture.SetFilterMode(CalculateFilterMode());
            return tempTexture;
        }

        private void SetBoolKeyword(string keyword, bool enabled, ref bool keywordEnabled)
        {
            if (enabled != keywordEnabled)
            {
                if (enabled)
                    _ntscEffectMaterial.EnableKeyword(keyword);
                else
                    _ntscEffectMaterial.DisableKeyword(keyword);
            }

            keywordEnabled = enabled;
        }

        private void DoQuantizeRGB()
        {
            if (QuantizeRGB)
            {
                Vector4 quantize = new Vector4(Mathf.Pow(2f, RBits), Mathf.Pow(2f, GBits), Mathf.Pow(2f, BBits), 1f);

                Vector4 oneOverQuantize = new Vector4(1f / quantize.x, 1f / quantize.y, 1f / quantize.z, 1f);

                _ntscEffectMaterial.SetVector("_QuantizeRGB", quantize);
                _ntscEffectMaterial.SetVector("_OneOverQuantizeRGB", oneOverQuantize);
            }
        }

        private void BlitByCurrentMode(RenderTexture input, RenderTexture output)
        {
            if (Mode == VideoMode.Composite || Mode == VideoMode.RF)
                BlitComposite(input, output);
            else if (Mode == VideoMode.SVideo)
                BlitSVideo(input, output);
            else if (Mode == VideoMode.VGA)
                BlitVga(input, output);
            else if (Mode == VideoMode.VGAFast)
                BlitVgaFast(input, output);
            else if (Mode == VideoMode.Component)
                BlitComponent(input, output);
        }

        private void BlitComposite(RenderTexture input, RenderTexture output)
        {
            IVirtualRenderTexture tempTexture1 = GetTemporaryTextureByTexture(output);
            IVirtualRenderTexture tempTexture2 = GetTemporaryTextureByTexture(output);
            IVirtualRenderTexture tempLastComposite = GetTemporaryTextureByTexture(output);

            tempTexture1.CopyInside(input);
            tempTexture1.BlitTo(tempTexture2, _ntscEffectMaterial, _passCompositeEncode);
            PassLastFrame(tempLastComposite, tempTexture2);
            tempTexture2.BlitTo(tempTexture1, _ntscEffectMaterial, _passCompositeDecode);
            tempTexture1.BlitTo(output, _ntscEffectMaterial, _passCompositeFinal);

            tempTexture1.Release();
            tempTexture2.Release();
            tempLastComposite.Release();
        }

        private void BlitSVideo(RenderTexture input, RenderTexture output)
        {
            var tempTexture = GetTemporaryTextureByTexture(output);
            var tempLastComposite = GetTemporaryTextureByTexture(output);

            tempTexture.BlitInside(input, _ntscEffectMaterial, _passSvideoEncode);
            PassLastFrame(tempLastComposite, tempTexture);
            tempTexture.BlitTo(output, _ntscEffectMaterial, _passSvideoDecode);

            tempTexture.Release();
            tempLastComposite.Release();
        }

        private void BlitVga(RenderTexture input, RenderTexture output)
        {
            Graphics.Blit(input, output, _ntscEffectMaterial, _passVga);
        }

        private void BlitVgaFast(RenderTexture input, RenderTexture output)
        {
            Graphics.Blit(input, output);
        }

        private void BlitComponent(RenderTexture input, RenderTexture output)
        {
            Graphics.Blit(input, output, _ntscEffectMaterial, _passComponent);
        }

        private void PassLastFrame(IVirtualRenderTexture lastComposite, IVirtualRenderTexture currentComposite)
        {
            _compositeTemp.CopyTo(lastComposite);
            currentComposite.CopyTo(_compositeTemp);
            _ntscEffectMaterial.SetTexture("_LastCompositeTex", lastComposite.Texture);
        }

        private void DoStretchToDisplay(RenderTexture output)
        {
            if (StretchToDisplay)
            {
                var temp = GetTemporaryTextureByTexture(output);
                BlitQuad(output, temp.Texture);
                temp.CopyTo(output);
                temp.Release();
            }
            else
            {
                float screenAspectRatio = (float)Screen.width / Screen.height;

                if (screenAspectRatio < AspectRatio)
                    FitToScreenWidth(output, screenAspectRatio);
                else
                    FitToScreenHeight(output, screenAspectRatio);
            }
        }

        private void FitToScreenWidth(RenderTexture output, float screenAspectRatio)
        {
            float width = 1f;
            float height = screenAspectRatio / AspectRatio;
            float heightDiff = 1f - height;

            var temp = GetTemporaryTextureByTexture(output);
            BlitQuadByRectangle(
                new Rect(0f, heightDiff * 0.5f, width, height),
                output,
                temp.Texture);
            temp.CopyTo(output);
            temp.Release();
        }

        private void FitToScreenHeight(RenderTexture output, float screenAspectRatio)
        {
            float height = 1f;
            float width = (1f / screenAspectRatio) * AspectRatio;
            float widthDiff = 1f - width;

            var temp = GetTemporaryTextureByTexture(output);
            BlitQuadByRectangle(new Rect(widthDiff * 0.5f, 0f, width, height), output, temp.Texture);
            temp.CopyTo(output);
            temp.Release();
        }

        private void UpdateFilterKernelTaps()
        {
            if (_filterKernelTaps is FilterKernelTaps8 && FilterKernel == FilterKernelTaps.FilterKernelTaps24)
                _filterKernelTaps = new FilterKernelTaps24();
            else if (_filterKernelTaps is FilterKernelTaps24 && FilterKernel == FilterKernelTaps.FilterKernelTaps8)
                _filterKernelTaps = new FilterKernelTaps8();
        }

        private void BlitQuad(RenderTexture sourceTexture, RenderTexture destinationTexture)
        {
            BlitQuadByRectangle(new Rect(0f, 0f, 1f, 1f), sourceTexture, destinationTexture);
        }

        private void BlitQuadByRectangle(Rect rectangle, RenderTexture sourceTexture, RenderTexture destinationTexture)
        {
            GL.PushMatrix();
            GL.LoadOrtho();

            RenderTexture.active = destinationTexture;
            GL.Clear(true, true, Color.black);

            _ntscEffectMaterial.SetTexture("_MainTex", sourceTexture);
            _ntscEffectMaterial.SetPass(_passTvOverlay);
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);
            GL.TexCoord2(0, 0);
            GL.Vertex3(rectangle.x, rectangle.y, 0.1f);

            GL.TexCoord2(1, 0);
            GL.Vertex3(rectangle.xMax, rectangle.y, 0.1f);

            GL.TexCoord2(1, 1);
            GL.Vertex3(rectangle.xMax, rectangle.yMax, 0.1f);

            GL.TexCoord2(0, 1);
            GL.Vertex3(rectangle.x, rectangle.yMax, 0.1f);
            GL.End();

            GL.PopMatrix();
        }

        private FilterMode CalculateFilterMode()
        {
            return SmoothRender ? FilterMode.Trilinear : FilterMode.Point;
        }

        public enum VideoMode
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

        public enum FilterKernelTaps
        {
            FilterKernelTaps8,
            FilterKernelTaps24
        }
    }
}