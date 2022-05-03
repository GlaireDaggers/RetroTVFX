using UnityEngine;

namespace YooPita.RetroTvFx
{
    [RequireComponent(typeof(Camera))]
    public class CameraRetroTvEffect : MonoBehaviour
    {
        [SerializeField] private Vector2 _screenResolution = new Vector2(320, 240);
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }
    }
}