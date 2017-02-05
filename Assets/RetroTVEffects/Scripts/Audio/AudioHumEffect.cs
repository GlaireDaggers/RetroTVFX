using UnityEngine;
using System.Collections;

public class AudioHumEffect : MonoBehaviour
{
	public int HumCycle = 60;

	[Range(0f, 0.001f)]
	public float HumAmount = 0.0001f;

	[Range(0f, 0.001f)]
	public float NoiseAmount = 0;

	private float sampleRate = 48000f;

	private float phase;

	private System.Random rand = new System.Random();

	// Use this for initialization
	void Start()
	{
		sampleRate = AudioSettings.outputSampleRate;
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		float increment = HumCycle * Mathf.PI / sampleRate;
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;

			float val = Mathf.Sin(phase);
			if (val >= 0f) val = 1f;
			else val = -1f;

			val *= HumAmount;

			float noise = (float)((rand.NextDouble() * 2.0) - 1.0);
			noise *= NoiseAmount;

			for (int c = 0; c < channels; c++)
			{
				data[i + c] += val + noise;
			}

			if (phase > 2 * Mathf.PI) phase = 0f;
		}
	}
}