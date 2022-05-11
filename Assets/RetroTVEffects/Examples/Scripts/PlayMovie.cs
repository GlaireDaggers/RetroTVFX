using UnityEngine;
using UnityEngine.Video;
using System.Collections;

[RequireComponent(typeof(VideoPlayer))]
public class PlayMovie : MonoBehaviour
{
#if !UNITY_WEBGL
    public VideoClip clip;

    private VideoPlayer _playerComponent;

    void Start()
    {
        _playerComponent = GetComponent<VideoPlayer>();
        _playerComponent.clip = clip;
    }
#endif
}