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
        [SerializeField] private RetroTvEffectPresset _presset;

        private VirtualRenderTexture _outputTexture;
        private RetroTvEffect _effect;

        private void Awake()
        {
            _effect = new RetroTvEffect(_presset);
            AllocateOutputTexture();
        }

        private void Update()
        {
            AllocateOutputTexture();
            _effect.Blit(_videoPlayer.Texture, _outputTexture);
        }

        private void AllocateOutputTexture()
        {
            if (_outputTexture == null || !_outputTexture.CheckCompatibility(_screenWidth, _screenHeight))
            {
                if (_outputTexture != null) _outputTexture.Release();
                _outputTexture = new VirtualRenderTexture(_screenWidth, _screenHeight, 24, RenderTextureFormat.ARGBHalf);
                _outputTexture.SetFilterMode(FilterMode.Point);
                _targerMaterial.mainTexture = _outputTexture.Texture;
            }
        }

        public void ChangePresset(RetroTvEffectPresset presset)
        {
            if (presset != _presset)
            {
                _presset = presset;
                _effect = new RetroTvEffect(_presset);
            }
        }
    }
}