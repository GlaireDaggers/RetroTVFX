using UnityEngine;

namespace RetroFx.Presets
{
    [System.Serializable, CreateAssetMenu(fileName = "TvFxPreset", menuName = "ScriptableObjects/TvFxPreset")]
    public class RetroTvEffectPreset : ScriptableObject
    {
        public RetroTvEffect.VideoMode VideoMode => _videoMode;
        public int DisplayWidth => _displayWidth;
        public int DisplayHeight => _displayHeight;
        public bool StretchToDisplay => _stretchToDisplay;
        public float AspectRatio => _aspectRatio;
        public bool EnableTvCurvature => _enableTvCurvature;
        public float Curvature => 8f - _curvature + 2f;
        public Texture2D TvOverlay => _tvOverlay;
        public bool EnablePixelMask => _enablePixelMask;
        public Texture2D PixelMaskTexture => _pixelMaskTexture;
        public Vector2 MaskRepeat => _maskRepeat;
        public float PixelMaskBrightness => _pixelMaskBrightness;
        public Vector2 IqOffset => _iqOffset;
        public Vector2 IqScale => _iqScale;
        public float RfNoise => _rfNoise;
        public float LumaSharpen => _lumaSharpen;
        public bool QuantizeRGB => _quantizeRGB;
        public int RBits => _rBits;
        public int GBits => _gBits;
        public int BBits => _bBits;
        public bool EnableBurstCountAnimation => _enableBurstCountAnimation;
        public bool AntiFlicker => _antiFlicker;
        public bool EnableRollingFlicker => _enableRollingFlicker;
        public float RollingFlickerFactor => _rollingFlickerFactor;
        public float RollingVSyncTime => _rollingVSyncTime;

        [SerializeField] private RetroTvEffect.VideoMode _videoMode = RetroTvEffect.VideoMode.Composite;
        [SerializeField] private int _displayWidth = 320;
        [SerializeField] private int _displayHeight = 240;
        [SerializeField] private bool _stretchToDisplay = true;
        [SerializeField] private float _aspectRatio = 1.33f;
        [Tooltip("Apply curvature to display")]
        [SerializeField] private bool _enableTvCurvature = false;
        [SerializeField, Range(0f, 8f)] private float _curvature = 0f;
        [Tooltip("Overlay image applied (before curvature)")]
        [SerializeField] private Texture2D _tvOverlay;
        [SerializeField] private bool _enablePixelMask = true;
        [SerializeField] private Texture2D _pixelMaskTexture;
        [SerializeField] private Vector2 _maskRepeat = new Vector2(160, 90);
        [SerializeField, Range(1f, 2f)] private float _pixelMaskBrightness = 1f;
        [SerializeField] private Vector2 _iqOffset = Vector2.zero;
        [SerializeField] private Vector2 _iqScale = Vector2.one;
        [SerializeField, Range(0f, 2f)] private float _rfNoise = 0.25f;
        [SerializeField, Range(0f, 4f)] private float _lumaSharpen = 0f;
        [SerializeField] private bool _quantizeRGB = false;
        [SerializeField, Range(2, 8)] private int _rBits = 8;
        [SerializeField, Range(2, 8)] private int _gBits = 8;
        [SerializeField, Range(2, 8)] private int _bBits = 8;
        [SerializeField] private bool _enableBurstCountAnimation = true;
        [SerializeField] private bool _antiFlicker = false;
        [SerializeField] private bool _enableRollingFlicker = false;
        [SerializeField, Range(0f, 1f)] private float _rollingFlickerFactor = 0.25f;
        [SerializeField, Range(0f, 2f)] private float _rollingVSyncTime = 1f;
        private bool _wasUpdated;

        public void OnValidate()
        {
            _wasUpdated = true;
        }

        public bool CheckWasUpdated()
        {
            if (_wasUpdated)
            {
                _wasUpdated = false;
                return true;
            }
            else
                return false;
        }
    }
}