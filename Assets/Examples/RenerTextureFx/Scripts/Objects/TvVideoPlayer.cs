using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace RetroFx.RenerTextureFx
{
    [RequireComponent(typeof(VideoPlayer))]
    public class TvVideoPlayer : MonoBehaviour, IInputSignal
    {
        public VirtualRenderTexture VirtualTexture => _texture;

        private VideoClip CurrentVideo => _videos[_selected];

        [SerializeField] private List<VideoClip> _videos;

        private VideoPlayer _videoPlayer;
        private VirtualRenderTexture _texture;
        private int _selected = 0;

        public void Play()
        {
            ChangeCurrentVideo();
            _videoPlayer.Play();
        }

        public void PlayNext()
        {
            _selected++;
            if (_selected >= _videos.Count) _selected = 0;
            Play();
        }

        private void ChangeCurrentVideo()
        {
            if (_videoPlayer.clip != CurrentVideo)
                _videoPlayer.clip = CurrentVideo;
        }

        private void Awake()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            AllocateVideoTexture();
            Play();
        }

        private void FixedUpdate()
        {
            AllocateVideoTexture();
        }

        private void OnEnable()
        {
            AllocateVideoTexture();
        }

        private void OnDisable()
        {
            if (_texture != null)
            {
                _texture.Release();
                _texture = null;
            }
        }

        private void AllocateVideoTexture()
        {
            int width = (int)CurrentVideo.width;
            int height = (int)CurrentVideo.height;
            if (_texture == null || !_texture.CheckCompatibility(width, height))
            {
                VirtualRenderTexture.AllocateTexture(ref _texture, width, height, 24, RenderTextureFormat.ARGBHalf);
                _texture.SetFilterMode(FilterMode.Point);
                _videoPlayer.targetTexture = _texture.Texture;
            }
        }
    }
}