using UnityEngine;
using System.Collections;

public class PlayMovie : MonoBehaviour
{
#if !UNITY_WEBGL
	public MovieTexture Texture;

    void Start()
    {
        Texture.Play();
    }
#endif
}