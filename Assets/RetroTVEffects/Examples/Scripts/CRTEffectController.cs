using UnityEngine;
using System.Collections;
using RetroTVFX;

namespace RetroTVFX.Examples
{
	public class CRTEffectController : MonoBehaviour
	{
		public CRTEffect Effects;

		public Animator Hallway;
		public Animator Player;

		void Start()
		{
			OnVideoModeChanged(1);
			OnColorDepthChanged(0);
			OnResChanged(2);
		}

		public void OnVideoModeChanged(int mode)
		{
			switch (mode)
			{
				case 0:
					Effects.VideoMode = VideoType.RF;
					break;
				case 1:
					Effects.VideoMode = VideoType.Composite;
					break;
				case 2:
					Effects.VideoMode = VideoType.SVideo;
					break;
				case 3:
					Effects.VideoMode = VideoType.VGA;
					break;
			}
		}

		public void OnColorDepthChanged(int mode)
		{
			switch (mode)
			{
				case 0:
					Effects.QuantizeRGB = false;
					break;
				case 1:
					Effects.QuantizeRGB = true;
					Effects.RBits = 5;
					Effects.GBits = 6;
					Effects.BBits = 5;
					break;
				case 2:
					Effects.QuantizeRGB = true;
					Effects.RBits = 5;
					Effects.GBits = 5;
					Effects.BBits = 5;
					break;
				case 3:
					Effects.QuantizeRGB = true;
					Effects.RBits = 3;
					Effects.GBits = 3;
					Effects.BBits = 3;
					break;
			}
		}

		public void OnResChanged(int mode)
		{
			switch (mode)
			{
				case 0:
					Effects.DisplaySizeX = 640;
					break;
				case 1:
					Effects.DisplaySizeX = 960;
					break;
				case 2:
					Effects.DisplaySizeX = 1280;
					break;
			}
		}

		public void OnFlickerChanged(int mode)
		{
			Effects.EnableRollingFlicker = (mode == 1);
		}

		public void OnCurveChanged(int mode)
		{
			Effects.EnableTVCurvature = (mode == 1);
		}

		public void OnPixelMaskChanged(int mode)
		{
			Effects.EnablePixelMask = (mode == 1);
		}

		public void OnEnableAnimChanged(int mode)
		{
			Hallway.enabled = Player.enabled = (mode == 1);
		}
	}
}