using UnityEngine;
using System.Collections;

namespace RetroTVFX.Extras
{
	public class AudioHumEffect : MonoBehaviour
	{
		public int HumCycle = 60;

		[Range(0f, 0.001f)]
		public float HumAmount = 0.0001f;

		[Range(0f, 0.001f)]
		public float NoiseAmount = 0;

		private float _sampleRate = 48000f;

		private float _phase;

		private System.Random _rand = new System.Random();

		// Use this for initialization
		void Start()
		{
			_sampleRate = AudioSettings.outputSampleRate;
		}

		void OnAudioFilterRead(float[] data, int channels)
		{
			float increment = HumCycle * Mathf.PI / _sampleRate;
			for (int i = 0; i < data.Length; i += channels)
			{
				_phase += increment;

				float val = Mathf.Sin(_phase);
				if (val >= 0f) val = 1f;
				else val = -1f;

				val *= HumAmount;

				float noise = (float)((_rand.NextDouble() * 2.0) - 1.0);
				noise *= NoiseAmount;

				for (int c = 0; c < channels; c++)
				{
					data[i + c] += val + noise;
				}

				if (_phase > 2 * Mathf.PI) _phase = 0f;
			}
		}
	}
}