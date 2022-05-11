Shader "Hidden/NTSCEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OverlayImg("Overlay Image", 2D) = "white" {}
		_PixelMask("Pixel Mask", 2D) = "white" {}
		_RGB2YIQ("RGB to YIQ LUT", 3D) = "" {}
		_YIQ2RGB("YIQ to RGB LUT", 3D) = "" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		// PASS_COMPOSITE_ENCODE		= 0
		Pass
		{
			CGPROGRAM
			#pragma multi_compile __ RF_SIGNAL
			#pragma multi_compile __ USE_YIQ_MATRIX
			#pragma multi_compile __ QUANTIZE_RGB
			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_composite_encode
			ENDCG
		}

		// PASS_COMPOSITE_DECODE		= 1
		Pass
		{
			CGPROGRAM
			#pragma multi_compile __ RF_SIGNAL
			#pragma multi_compile __ USE_YIQ_MATRIX
			#pragma multi_compile __ ANTI_FLICKER
			#pragma multi_compile __ ROLLING_FLICKER
			#pragma multi_compile __ PIXEL_MASK

			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_composite_decode
			ENDCG
		}

		// PASS_COMPOSITE_FINAL			= 2
		Pass
		{
			CGPROGRAM
			#pragma multi_compile __ RF_SIGNAL
			#pragma multi_compile __ USE_YIQ_MATRIX
			#pragma multi_compile __ ANTI_FLICKER
			#pragma multi_compile __ ROLLING_FLICKER
			#pragma multi_compile __ PIXEL_MASK

			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_composite_final
			ENDCG
		}

		// PASS_TV_OVERLAY				= 3
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma multi_compile __ PIXEL_MASK
			#pragma multi_compile __ USE_TV_CURVATURE
			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_tv_overlay
			ENDCG
		}

		// PASS_VGA						= 4
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma multi_compile __ ANTI_FLICKER
			#pragma multi_compile __ ROLLING_FLICKER
			#pragma multi_compile __ PIXEL_MASK
			#pragma multi_compile __ QUANTIZE_RGB

			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_vga
			ENDCG
		}

		// PASS_COMPONENT				= 5
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma multi_compile __ USE_YIQ_MATRIX
			#pragma multi_compile __ ANTI_FLICKER
			#pragma multi_compile __ ROLLING_FLICKER
			#pragma multi_compile __ PIXEL_MASK
			#pragma multi_compile __ QUANTIZE_RGB

			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_component
			ENDCG
		}

		// PASS_SVIDEO_ENCODE			= 6
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma multi_compile __ USE_YIQ_MATRIX
			#pragma multi_compile __ QUANTIZE_RGB
			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_svideo_encode
			ENDCG
		}

		// PASS_SVIDEO_DECODE			= 7
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma multi_compile __ USE_YIQ_MATRIX
			#pragma multi_compile __ ANTI_FLICKER
			#pragma multi_compile __ ROLLING_FLICKER
			#pragma multi_compile __ PIXEL_MASK

			#include "RetroTV.cginc"

			#pragma vertex vert_tv
			#pragma fragment frag_svideo_decode
			ENDCG
		}
	}
}
