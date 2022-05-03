using UnityEngine;

namespace YooPita.RetroTvFx
{
    public class RetroTvEffect
    {
        public RetroTvEffect(RetroTvEffectPresset retroTvEffectPresset)
        {
            var shader = Shader.Find("Hidden/NTSCEffect");
            _material = new Material(shader);
            _retroTvEffectPresset = retroTvEffectPresset;
            UpdateValues();
        }

        private RetroTvEffectPresset _retroTvEffectPresset;
        private const int _passCompositeEncode = 0;
        private const int _passCompositeDecode = 1;
        private const int _passCompositeFinal = 2;

        private const int _passVga = 4;
        private const int _passComponent = 5;

        private const int _passSvideoEncode = 6;
        private const int _passSvideoDecode = 7;

        private const int _passTvOverlay = 3;

        private Material _material;

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

        public void Blit(IVirtualRenderTexture input, IVirtualRenderTexture output)
        {
            AllocateTemporaryTextureByPresset(ref _compositeTemp);
            BlitByCurrentMode(input, output);
            StretchToDisplay(output);
            LastUpdate();
            if (_retroTvEffectPresset.CheckWasUpdated())
                UpdateValues();
        }

        public void UpdateValues()
        {
            QuantizeRGB();

            SetBoolKeyword("ANTI_FLICKER", _retroTvEffectPresset.AntiFlicker, _antiFlickerEnabled);
            SetBoolKeyword("ROLLING_FLICKER", _retroTvEffectPresset.EnableRollingFlicker, _rollingFlickerEnabled);
            SetBoolKeyword("PIXEL_MASK", _retroTvEffectPresset.EnablePixelMask, _pixelMaskEnabled);
            SetBoolKeyword("USE_TV_CURVATURE", _retroTvEffectPresset.EnableTvCurvature, _tvCurvatureEnabled);
            SetBoolKeyword("QUANTIZE_RGB", _retroTvEffectPresset.QuantizeRGB, _quantizeRGBEnabled);
            SetBoolKeyword("RF_SIGNAL", _retroTvEffectPresset.VideoMode == VideoMode.RF, _rfEnabled);

            _material.SetMatrix("_RGB2YIQ_MAT", _tvMatrices.Rgb2yiqMatrix);
            _material.SetMatrix("_YIQ2RGB_MAT", _tvMatrices.Yiq2rgbMatrix);

            _material.SetTexture("_OverlayImg", _retroTvEffectPresset.TvOverlay);

            _material.SetFloatArray("_LumaFilter", _filterKernelTaps.LumaFilter);
            _material.SetFloatArray("_ChromaFilter", _filterKernelTaps.ChromaFilter);
            _material.SetFloat("_Realtime", Time.realtimeSinceStartup);

            _material.SetVector("_IQOffset", new Vector4(
                _retroTvEffectPresset.IqScale.x,
                _retroTvEffectPresset.IqScale.y,
                _retroTvEffectPresset.IqOffset.x,
                _retroTvEffectPresset.IqOffset.y));

            _material.SetFloat("_RFNoise", _retroTvEffectPresset.RfNoise);
            _material.SetFloat("_LumaSharpen", _retroTvEffectPresset.LumaSharpen);

            _material.SetInt("_Framecount", -_frameCount);
            _material.SetVector("_ScreenSize", new Vector4(
                _retroTvEffectPresset.DisplayWidth,
                _retroTvEffectPresset.DisplayHeight,
                1f / _retroTvEffectPresset.DisplayWidth,
                1f / _retroTvEffectPresset.DisplayHeight));

            _material.SetFloat("_RollingFlickerAmount", _retroTvEffectPresset.RollingFlickerFactor);
            _material.SetVector("_FlickerOffs", new Vector4(
                _flickerOffset,
                _flickerOffset + _retroTvEffectPresset.RollingVSyncTime,
                0f,
                0f));

            _material.SetVector("_PixelMaskScale", new Vector4(
                _retroTvEffectPresset.MaskRepeat.x,
                _retroTvEffectPresset.MaskRepeat.y));
            _material.SetTexture("_PixelMask", _retroTvEffectPresset.PixelMaskTexture);
            _material.SetFloat("_Brightness", _retroTvEffectPresset.PixelMaskBrightness);

            _material.SetFloat("_TVCurvature", _retroTvEffectPresset.Curvature);
        }

        private void LastUpdate()
        {
            if (_retroTvEffectPresset.EnableBurstCountAnimation)
            {
                _frameCount = (_frameCount + 1) % 3;
                _material.SetInt("_Framecount", -_frameCount);
            }

            if (_retroTvEffectPresset.EnableRollingFlicker)
            {
                _flickerOffset += _retroTvEffectPresset.RollingVSyncTime;
                _material.SetVector("_FlickerOffs", new Vector4(
                _flickerOffset,
                _flickerOffset + _retroTvEffectPresset.RollingVSyncTime,
                0f,
                0f));
            }
        }

        private void AllocateTemporaryTextureByPresset(ref IVirtualRenderTexture texture)
        {
            if (texture == null ||
                texture.Width != _retroTvEffectPresset.DisplayWidth ||
                texture.Height != _retroTvEffectPresset.DisplayHeight)
            {
                if (texture != null) texture.Release();
                texture = new VirtualTemporaryRenderTexture(
                    _retroTvEffectPresset.DisplayWidth,
                    _retroTvEffectPresset.DisplayHeight,
                    24,
                    RenderTextureFormat.ARGBHalf);
            }
        }

        private VirtualTemporaryRenderTexture GetTemporaryTextureByPresset()
        {
            return new VirtualTemporaryRenderTexture(
                _retroTvEffectPresset.DisplayWidth,
                _retroTvEffectPresset.DisplayHeight,
                24,
                RenderTextureFormat.ARGBHalf);
        }

        private void SetBoolKeyword(string keyword, bool enabled, bool keywordEnabled)
        {
            if (enabled != keywordEnabled)
            {
                if (enabled)
                    _material.EnableKeyword(keyword);
                else
                    _material.DisableKeyword(keyword);
            }

            keywordEnabled = enabled;
        }

        private void QuantizeRGB()
        {
            if (_retroTvEffectPresset.QuantizeRGB)
            {
                Vector4 quantize = new Vector4(
                    Mathf.Pow(2f, _retroTvEffectPresset.RBits),
                    Mathf.Pow(2f, _retroTvEffectPresset.GBits),
                    Mathf.Pow(2f, _retroTvEffectPresset.BBits),
                    1f);

                Vector4 oneOverQuantize = new Vector4(
                    1f / quantize.x,
                    1f / quantize.y,
                    1f / quantize.z,
                    1f);

                _material.SetVector("_QuantizeRGB", quantize);
                _material.SetVector("_OneOverQuantizeRGB", oneOverQuantize);
            }
        }

        private void BlitByCurrentMode(IVirtualRenderTexture input, IVirtualRenderTexture output)
        {
            if (_retroTvEffectPresset.VideoMode == VideoMode.Composite || _retroTvEffectPresset.VideoMode == VideoMode.RF)
                BlitComposite(input, output);
            else if (_retroTvEffectPresset.VideoMode == VideoMode.SVideo)
                BlitSVideo(input, output);
            else if (_retroTvEffectPresset.VideoMode == VideoMode.VGA)
                BlitVga(input, output);
            else if (_retroTvEffectPresset.VideoMode == VideoMode.VGAFast)
                BlitVgaFast(input, output);
            else if (_retroTvEffectPresset.VideoMode == VideoMode.Component)
                BlitComponent(input, output);
        }

        private void BlitComposite(IVirtualRenderTexture input, IVirtualRenderTexture output)
        {
            IVirtualRenderTexture tempTexture1 = GetTemporaryTextureByPresset();
            IVirtualRenderTexture tempTexture2 = GetTemporaryTextureByPresset();
            IVirtualRenderTexture tempLastComposite = GetTemporaryTextureByPresset();

            input.CopyTo(tempTexture1);
            tempTexture1.BlitTo(tempTexture2, _material, _passCompositeEncode);
            PassLastFrame(tempLastComposite, tempTexture2);
            tempTexture2.BlitTo(tempTexture1, _material, _passCompositeDecode);
            tempTexture1.BlitTo(output, _material, _passCompositeFinal);

            tempTexture1.Release();
            tempTexture2.Release();
            tempLastComposite.Release();
        }

        private void BlitSVideo(IVirtualRenderTexture input, IVirtualRenderTexture output)
        {
            var tempTexture = GetTemporaryTextureByPresset();
            var tempLastComposite = GetTemporaryTextureByPresset();

            input.BlitTo(tempTexture, _material, _passSvideoEncode);
            PassLastFrame(tempLastComposite, tempTexture);
            tempTexture.BlitTo(output, _material, _passSvideoDecode);

            tempTexture.Release();
            tempLastComposite.Release();
        }

        private void BlitVga(IVirtualRenderTexture input, IVirtualRenderTexture output)
        {
            input.BlitTo(output, _material, _passVga);
        }

        private void BlitVgaFast(IVirtualRenderTexture input, IVirtualRenderTexture output)
        {
            input.CopyTo(output);
        }

        private void BlitComponent(IVirtualRenderTexture input, IVirtualRenderTexture output)
        {
            input.BlitTo(output, _material, _passComponent);
        }

        private void PassLastFrame(IVirtualRenderTexture lastComposite, IVirtualRenderTexture currentComposite)
        {
            _compositeTemp.CopyTo(lastComposite);
            currentComposite.CopyTo(_compositeTemp);
            _material.SetTexture("_LastCompositeTex", lastComposite.Texture);
        }

        private void StretchToDisplay(IVirtualRenderTexture output)
        {
            if (_retroTvEffectPresset.StretchToDisplay)
            {
                var temp = GetTemporaryTextureByPresset();
                BlitQuad(output, temp);
                temp.CopyTo(output);
                temp.Release();
            }
            else
            {
                float screenAspectRatio = (float)Screen.width / Screen.height;

                if (screenAspectRatio < _retroTvEffectPresset.AspectRatio)
                    FitToScreenWidth(output, screenAspectRatio);
                else
                    FitToScreenHeight(output, screenAspectRatio);
            }
        }

        private void FitToScreenWidth(IVirtualRenderTexture output, float screenAspectRatio)
        {
            float width = 1f;
            float height = screenAspectRatio / _retroTvEffectPresset.AspectRatio;
            float heightDiff = 1f - height;

            var temp = GetTemporaryTextureByPresset();
            BlitQuadByRectangle(
                new Rect(0f, heightDiff * 0.5f, width, height),
                output,
                temp);
            temp.CopyTo(output);
            temp.Release();
        }

        private void FitToScreenHeight(IVirtualRenderTexture output, float screenAspectRatio)
        {
            float height = 1f;
            float width = (1f / screenAspectRatio) * _retroTvEffectPresset.AspectRatio;
            float widthDiff = 1f - width;

            var temp = GetTemporaryTextureByPresset();
            BlitQuadByRectangle(new Rect(widthDiff * 0.5f, 0f, width, height), output, temp);
            temp.CopyTo(output);
            temp.Release();
        }

        private void BlitQuad(IVirtualRenderTexture sourceTexture, IVirtualRenderTexture destinationTexture)
        {
            BlitQuadByRectangle(new Rect(0f, 0f, 1f, 1f), sourceTexture, destinationTexture);
        }

        private void BlitQuadByRectangle(Rect rectangle, IVirtualRenderTexture sourceTexture, IVirtualRenderTexture destinationTexture)
        {
            GL.PushMatrix();
            GL.LoadOrtho();

            RenderTexture.active = destinationTexture.Texture;
            GL.Clear(true, true, Color.black);

            _material.SetTexture("_MainTex", sourceTexture.Texture);
            _material.SetPass(_passTvOverlay);
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
    }
}