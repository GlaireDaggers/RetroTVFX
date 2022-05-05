using UnityEngine;

namespace YooPita.RetroTvFx
{
	public class AudioHumEffect : MonoBehaviour
	{
		[SerializeField, Range(0f, 60000f)]	private int _humCycle = 60;
		[SerializeField, Range(0f, 0.001f)] private float _humAmount = 0.0001f;
		[SerializeField, Range(0f, 0.001f)] private float _noiseAmount = 0;
		private float _sampleRate = 48000f;
		private float _phase;
		private System.Random _random = new System.Random();

		void Start()
		{
			_sampleRate = AudioSettings.outputSampleRate;
		}

		void OnAudioFilterRead(float[] data, int channels)
		{
			float increment = _humCycle * Mathf.PI / _sampleRate;
			for (int i = 0; i < data.Length; i += channels)
			{
				_phase += increment;

				float val = Mathf.Sin(_phase);
				if (val >= 0f) val = 1f;
				else val = -1f;

				val *= _humAmount;

				float noise = (float)((_random.NextDouble() * 2.0) - 1.0);
				noise *= _noiseAmount;

				for (int c = 0; c < channels; c++)
				{
					data[i + c] += val + noise;
				}

				if (_phase > 2 * Mathf.PI) _phase = 0f;
			}
		}
	}
}