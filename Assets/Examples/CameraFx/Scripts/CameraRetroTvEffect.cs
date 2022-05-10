using RetroFx.Presets;
using UnityEngine;

namespace RetroFx.CameraFx
{
    [RequireComponent(typeof(Camera))]
    public class CameraRetroTvEffect : MonoBehaviour
    {
        private int Width => _autoWidth ? _calculatedWidth : _width;

        [SerializeField] private bool _enable = true;
        [SerializeField] private bool _smoothOutputTexture = true;
        [SerializeField] private bool _smoothFxRender = true;
        [SerializeField] private RetroTvEffectPreset _preset;
        [SerializeField] private int _width = 256;
        [SerializeField] private int _height = 240;
        [SerializeField, Range(1f, 5f)] float _scaleMultiplayer = 1f;
        [SerializeField] private bool _autoWidth = false;
        [SerializeField] private bool _stretchToDisplay = false;
        [SerializeField] private RetroTvEffect.FilterKernelTaps _filterKernelTaps = RetroTvEffect.FilterKernelTaps.FilterKernelTaps8;

        private RetroTvEffect _effect;
        private Camera _camera;
        private VirtualRenderTexture _outputTexture;
        private VirtualRenderTexture _cameraTargetTexture;
        private int _calculatedWidth = 0;
        private bool _currentSmoothOutputTexture;

        private void Awake()
        {
            _currentSmoothOutputTexture = _smoothOutputTexture;
            CalculateWidth();
            Inicialize();
            UpdateFilterKernelTaps();
            AllocateOutputTexture();
            AllocateCameraTargetTexture();
            UpdateEffectValuesByPresset();
            Blit();
        }

        private void FixedUpdate()
        {
            UpdateFilterKernelTaps();

            CalculateWidth();
            if (_cameraTargetTexture.Width != Width || _cameraTargetTexture.Height != _height || _currentSmoothOutputTexture != _smoothOutputTexture)
                AllocateCameraTargetTexture();
            if (_outputTexture.Width != Screen.width || _outputTexture.Height != Screen.height || _currentSmoothOutputTexture != _smoothOutputTexture)
                AllocateOutputTexture();

            if (_enable)
            {
                UpdateEffectValuesByPresset();
                Blit();
            }
            else
                _cameraTargetTexture.CopyTo(_outputTexture);
        }

        private void OnGUI()
        {
            GUI.DrawTexture(CalculateRect(), _outputTexture.Texture);
        }

        private void Inicialize()
        {
            if (_camera == null) _camera = GetComponent<Camera>();
            if (_effect == null) _effect = new RetroTvEffect();
        }

        private void Blit()
        {
            _effect.Blit(_cameraTargetTexture.Texture, _outputTexture.Texture);
        }

        private void AllocateOutputTexture()
        {
            VirtualRenderTexture.AllocateTexture(
                ref _outputTexture,
                Screen.width,
                Screen.height,
                24,
                RenderTextureFormat.ARGBHalf);

            _currentSmoothOutputTexture = _smoothOutputTexture;
            if (!_smoothOutputTexture) _outputTexture.SetFilterMode(FilterMode.Point);
        }

        private void AllocateCameraTargetTexture()
        {
            VirtualRenderTexture.AllocateTexture(
                ref _cameraTargetTexture,
                Width,
                _height,
                24,
                RenderTextureFormat.ARGBHalf);
            _cameraTargetTexture.SetFilterMode(FilterMode.Point);
            _camera.forceIntoRenderTexture = true;
            _camera.targetTexture = _cameraTargetTexture.Texture;
        }

        private void UpdateEffectValuesByPresset()
        {
            _effect.Mode = _preset.VideoMode;
            _effect.Width = (int)Mathf.Floor(Width * _scaleMultiplayer);
            _effect.Height = (int)Mathf.Floor(_height * _scaleMultiplayer);
            _effect.StretchToDisplay = _preset.StretchToDisplay;
            _effect.AspectRatio = _preset.AspectRatio;
            _effect.EnableTvCurvature = _preset.EnableTvCurvature;
            _effect.Curvature = _preset.Curvature;
            _effect.TvOverlay = _preset.TvOverlay;
            _effect.EnablePixelMask = _preset.EnablePixelMask;
            _effect.PixelMaskTexture = _preset.PixelMaskTexture;
            _effect.PixelPerMask = _preset.PixelPerMask;
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
            _effect.SmoothRender = _smoothFxRender;

            _effect.UpdateValues();
        }

        private void CalculateWidth()
        {
            var aspectRatio = (float)Screen.width / Screen.height;
            _calculatedWidth = (int)Mathf.Floor(aspectRatio * _height);
        }

        private Rect CalculateRect()
        {
            if (_stretchToDisplay)
                return new Rect(0, 0, Screen.width, Screen.height);
            if (Screen.width > Screen.height)
            {
                var factor = (float)Screen.height / _height;
                var width = _width * factor;
                var horizontalOffset = (Screen.width - (width)) / 2f;
                return new Rect(horizontalOffset, 0, width, Screen.height);
            }
            else
            {
                var factor = (float)Screen.width / _width;
                var height = _height * factor;
                var verticalOffset = (Screen.height - (height)) / 2f;
                return new Rect(0, verticalOffset, Screen.width, height);
            }
        }

        private void UpdateFilterKernelTaps()
        {
            if (_effect.FilterKernel != _filterKernelTaps)
            {
                _effect.FilterKernel = _filterKernelTaps;
            }
        }
    }
}