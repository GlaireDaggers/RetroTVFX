using UnityEngine;

namespace RetroFx
{
	public class AudioHumEffect : MonoBehaviour
	{
		[SerializeField] private int _humCycle = 60;
		[SerializeField, Range(0f, 0.001f)] private float _humAmount = 0.0001f;
		[SerializeField, Range(0f, 0.001f)] private float _noiseAmount = 0;
		private float _sampleRate = 48000f;
		private float _phase;
		private System.Random _random;

		private void Start()
		{
			_sampleRate = AudioSettings.outputSampleRate;
			_random = new System.Random();
		}

		private void OnAudioFilterRead(float[] data, int channels)
		{
			float increment = _humCycle * Mathf.PI / _sampleRate;
			for (int i = 0; i < data.Length; i += channels)
			{
				_phase += increment;

				float humValue = Mathf.Sin(_phase) >= 0 ? 1: -1;
				humValue *= _humAmount;

				float noise = (float)((_random.NextDouble() * 2.0) - 1.0);
				noise *= _noiseAmount;

				for (int c = 0; c < channels; c++)
				{
					data[i + c] += humValue + noise;
				}

				if (_phase > 2 * Mathf.PI) _phase = 0f;
			}
		}
	}
}