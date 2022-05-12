using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroTVFX
{
    public static class FilterTaps
    {
        public static readonly float[] lumaFilter8Tap =
		{
		   -0.0020f, -0.0009f, 0.0038f, 0.0178f, 0.0445f,
			0.0817f, 0.1214f, 0.1519f, 0.1634f
		};

		public static readonly float[] chromaFilter8Tap =
		{
			0.0046f, 0.0082f, 0.0182f, 0.0353f, 0.0501f,
			0.0832f, 0.1062f, 0.1222f, 0.1280f
		};

        public static readonly float[] lumaFilter24Taps = new float[]
		{
			-0.000012020f,
			-0.000022146f,
			-0.000013155f,
			-0.000012020f,
			-0.000049979f,
			-0.000113940f,
			-0.000122150f,
			-0.000005612f,
			0.000170516f,
			0.000237199f,
			0.000169640f,
			0.000285688f,
			0.000984574f,
			0.002018683f,
			0.002002275f,
			-0.000909882f,
			-0.007049081f,
			-0.013222860f,
			-0.012606931f,
			0.002460860f,
			0.035868225f,
			0.084016453f,
			0.135563500f,
			0.175261268f,
			0.190176552f
		};

		public static readonly float[] chromaFilter24Taps = new float[]
		{
			-0.000118847f,
			-0.000271306f,
			-0.000502642f,
			-0.000930833f,
			-0.001451013f,
			-0.002064744f,
			-0.002700432f,
			-0.003241276f,
			-0.003524948f,
			-0.003350284f,
			-0.002491729f,
			-0.000721149f,
			0.002164659f,
			0.006313635f,
			0.011789103f,
			0.018545660f,
			0.026414396f,
			0.035100710f,
			0.044196567f,
			0.053207202f,
			0.061590275f,
			0.068803602f,
			0.074356193f,
			0.077856564f,
			0.079052396f
		};
    }
}