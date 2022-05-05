using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooPita.RetroTvFx
{
    public class Tv : MonoBehaviour
    {
        [SerializeField] private Material _targerMaterial;
        [SerializeField] private int _screenWidth = 1280;
        [SerializeField] private int _screenHeight = 800;
        [SerializeField] private TvVideoPlayer _videoPlayer;
        [SerializeField] private RetroTvEffectPreset _preset;

        private VirtualRenderTexture _outputTexture;
        private RetroTvEffect _effect;

        private void Awake()
        {
            CreateRetroTvEffectFromPreset();
            AllocateOutputTexture();
        }

        private void Update()
        {
            AllocateOutputTexture();
            _effect.Blit(_videoPlayer.Texture, _outputTexture);
            if (_preset.CheckWasUpdated())
                UpdateEffectValuesByPresset();
        }

        private void OnEnable()
        {
            AllocateOutputTexture();
        }

        private void OnDisable()
        {
            if (_outputTexture != null)
            {
                _outputTexture.Release();
                _outputTexture = null;
            }
        }

        private void AllocateOutputTexture()
        {
            if (_outputTexture == null || !_outputTexture.CheckCompatibility(_screenWidth, _screenHeight))
            {
                VirtualRenderTexture.AllocateTexture(ref _outputTexture, _screenWidth, _screenHeight, 24, RenderTextureFormat.ARGBHalf);
                _outputTexture.SetFilterMode(FilterMode.Point);
                _targerMaterial.mainTexture = _outputTexture.Texture;
                _targerMaterial.SetTexture("_EmissionMap", _outputTexture.Texture);
            }
        }

        public void ChangePreset(RetroTvEffectPreset preset)
        {
            if (preset != _preset)
            {
                _preset = preset;
                CreateRetroTvEffectFromPreset();
            }
        }

        private void CreateRetroTvEffectFromPreset()
        {
            _effect = new RetroTvEffect();
            UpdateEffectValuesByPresset();
        }

        private void UpdateEffectValuesByPresset()
        {
            _effect.Mode = _preset.VideoMode;
            _effect.Width = _preset.DisplayWidth;
            _effect.Height = _preset.DisplayHeight;
            _effect.StretchToDisplay = _preset.StretchToDisplay;
            _effect.AspectRatio = _preset.AspectRatio;
            _effect.EnableTvCurvature = _preset.EnableTvCurvature;
            _effect.Curvature = _preset.Curvature;
            _effect.TvOverlay = _preset.TvOverlay;
            _effect.EnablePixelMask = _preset.EnablePixelMask;
            _effect.PixelMaskTexture = _preset.PixelMaskTexture;
            _effect.MaskRepeat = _preset.MaskRepeat;
            _effect.PixelMaskBrightness = _preset.PixelMaskBrightness;
            _effect.IqOffset = _preset.IqOffset;
            _effect.IqScale = _preset.IqScale;
            _effect.RfNoise = _preset.RfNoise;
            _effect.LumaSharpen = _preset.LumaSharpen;
            _effect.QuantizeRGB = _preset.QuantizeRGB;
            _effect.RBits = _preset.RBits;
            _effect.GBits = _preset.GBits;
            _effect.BBits = _preset.BBits;
            _effect.EnableBurstCountAnimation = _preset.EnableBurstCountAnimation;
            _effect.AntiFlicker = _preset.AntiFlicker;
            _effect.EnableRollingFlicker = _preset.EnableRollingFlicker;
            _effect.RollingFlickerFactor = _preset.RollingFlickerFactor;
            _effect.RollingVSyncTime = _preset.RollingVSyncTime;

            _effect.UpdateValues();
        }
    }
}