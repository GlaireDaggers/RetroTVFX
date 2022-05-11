#include "UnityCG.cginc"

// CONSTANTS
// ====================

#define PI 3.14159265
#define SCANLINE_PHASE_OFFSET (PI * 0.66667)
#define CHROMA_MOD_FREQ (PI / 3.0)
#define CHROMA_AMP 1.5
#define ENCODE_GAMMA (1.0 / 2.2)

#define SATURATION 1.0
#define BRIGHTNESS 1.0
#define chroma_mod (2.0 * SATURATION / CHROMA_AMP)

#define NTSC_GAMMA 1.0

#define YIQOFFSET float3( 0.0, 0.5, 0.5 )
#define YIQEXPAND float3( 1.0, 2.0, 2.0 )
#define YIQSCALE float3( 1.0, 0.5, 0.5 )

#define RGB_SHIFT float4( 0.02, 0.02, 0.02, 0.0 )
#define RGB_SCALE ( 1.0 - RGB_SHIFT )

#if defined(RF_SIGNAL)
#define COMPOSITE_LOWPASS 1.25
#else
#define COMPOSITE_LOWPASS 1.0
#endif

#define SVIDEO_LOWPASS 1.0
// ====================

// PARAMETERS
// ====================
sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _PixelMask;
float4 _PixelMaskScale;
float _Brightness;

sampler2D _OverlayImg;
float _TVCurvature;

sampler2D _LastCompositeTex;

float4x4 _RGB2YIQ_MAT;
float4x4 _YIQ2RGB_MAT;

float _Framecount;

float _RollingFlickerAmount;
float4 _FlickerOffs;

float _SignalFilter[9];

float _LumaFilter[32];
float _ChromaFilter[32];
int _FilterSize;

float4 _IQOffset;

float _RFNoise;
float _LumaSharpen;

float _Realtime;

float4 _ScreenSize;

float4 _QuantizeRGB;
float4 _OneOverQuantizeRGB;
// ====================

// VERTEX SHADER
// ====================
struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
	float2 pix_no : TEXCOORD1;
	float2 mask_uv : TEXCOORD2;
};

v2f vert_tv(appdata v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;

	float2 invUV = float2(o.uv.x, 1 - o.uv.y);
	o.pix_no = invUV * _ScreenSize.xy;
	o.mask_uv = o.uv * _PixelMaskScale.xy;

	return o;
}
// ====================

// HELPER FUNCTIONS
// ====================
inline float3 rgb2yiq(float3 col)
{
	return mul(_RGB2YIQ_MAT, float4(col, 0.0)).xyz * float3(1.0, _IQOffset.xy) + float3(0.0, _IQOffset.zw);
}

inline float3 fetch_yiq_offset(float2 uv, float offset, float one_x)
{
	float3 yiq = tex2D(_MainTex, uv + float2(offset * one_x, 0.0)).xyz;
	yiq -= YIQOFFSET;
	return yiq;
}

inline float3 yiq2rgb(float3 yiq)
{
	return mul(_YIQ2RGB_MAT, float4(yiq - YIQOFFSET, 0.0)).rgb;
}

inline float3 fetch_signal(float2 uv)
{
#if defined(ANTI_FLICKER)
	float3 a = tex2D(_MainTex, uv).xyz;
	float3 b = tex2D(_LastCompositeTex, uv).xyz;
	return (a + b) * 0.5;
#else
	return tex2D(_MainTex, uv).xyz;
#endif
}

inline float3 fetch_offset(float2 uv, float offset, float one_x)
{
	return fetch_signal(uv + float2(offset * one_x, 0.0));
}

inline void frag_rolling_flicker(v2f i, inout float3 rgb)
{
	// apply the rolling flicker that results from refresh rate mismatch between camera and TV
#if defined(ROLLING_FLICKER)
	float3 c1 = rgb * ((1 - fmod(i.uv.y + _FlickerOffs.x, 1.0)) * _RollingFlickerAmount + (1 - _RollingFlickerAmount));
	float3 c2 = rgb * ((1 - fmod(i.uv.y + _FlickerOffs.y, 1.0)) * _RollingFlickerAmount + (1 - _RollingFlickerAmount));
	rgb = (c1 + c2) * 0.5;
#endif
}

inline void frag_pixel_mask(float2 uv, inout float3 rgb)
{
	// apply the pixel mask effect
#if defined(PIXEL_MASK)
	rgb *= tex2D(_PixelMask, uv).rgb * _Brightness;
#endif
}

inline void frag_quantize(inout float3 rgb)
{
#if defined(QUANTIZE_RGB)
	rgb = floor(rgb * _QuantizeRGB.xyz) * _OneOverQuantizeRGB.xyz;
#endif
}

float rand(float3 myVector)
{
	return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
}
// ====================

// FRAGMENT SHADER
// ====================
fixed4 frag_composite_encode(v2f i) : SV_Target
{
	float3 rgb = tex2D(_MainTex, i.uv);
	frag_quantize(rgb);
	float3 yiq = rgb2yiq(rgb);

	float chroma_phase = SCANLINE_PHASE_OFFSET * ((i.pix_no.y) + _Framecount);
	float mod_phase = chroma_phase + i.pix_no.x * CHROMA_MOD_FREQ;

	float i_mod = cos(mod_phase);
	float q_mod = sin(mod_phase);

	yiq.y *= i_mod * CHROMA_AMP;
	yiq.z *= q_mod * CHROMA_AMP;

	// encode as signal
#if defined(RF_SIGNAL)
	float rmod = 1.0 - (sin(i.uv.x * 320) * 0.05);
	float noise = (rand(float3(i.uv, _Realtime)) * rmod * 2 - 1) * _RFNoise;

	float signal = (dot(yiq, float3(1.0, 1.0, 1.0)) + noise);
#else
	float signal = (dot(yiq, float3(1.0, 1.0, 1.0)));
#endif

	float3 out_color = float3(signal, signal, signal) * float3(BRIGHTNESS, i_mod * chroma_mod, q_mod * chroma_mod);

	return float4(out_color, 1.0);
}

fixed4 frag_composite_decode(v2f i) : SV_Target
{
	float one_x = _ScreenSize.z * COMPOSITE_LOWPASS;
	float3 signal = float3(0.0, 0.0, 0.0);

	for (int idx = 0; idx < _FilterSize; idx++)
	{
		float offset = float(idx);

		float3 sums = fetch_offset(i.uv, offset - float(_FilterSize), one_x) +
			fetch_offset(i.uv, (float)_FilterSize - offset, one_x);

		signal += sums * float3(_LumaFilter[idx], _ChromaFilter[idx], _ChromaFilter[idx]);
	}
	signal += fetch_signal(i.uv) *
		float3(_LumaFilter[_FilterSize], _ChromaFilter[_FilterSize], _ChromaFilter[_FilterSize]);

	signal += YIQOFFSET;

	return float4(signal, 1);
}

fixed4 frag_composite_final(v2f i) : SV_Target
{
	float one_y = _ScreenSize.w;
	float3 yiq = tex2D(_MainTex, i.uv);

	float3 yiq2 = tex2D(_MainTex, i.uv + float2(_ScreenSize.z * 2, 0.0));
	float3 yiq3 = tex2D(_MainTex, i.uv - float2(_ScreenSize.z * 2, 0.0));
	
	// for realism this should be a scanline-based comb filter, but that doesn't seem to look quite right
	// so for now it's a naive horizontal convolution instead
	yiq.x += (yiq.x * _LumaSharpen * 2) + (yiq2.x * -1 * _LumaSharpen) + (yiq3.x * -1 * _LumaSharpen);

	float3 rgb = yiq2rgb(yiq);

	frag_rolling_flicker(i, rgb);

	return float4(rgb.r, rgb.g, rgb.b, 1.0) * RGB_SCALE + RGB_SHIFT;
}

fixed4 frag_vga(v2f i) : SV_Target
{
	float3 rgb = tex2D(_MainTex, i.uv).rgb;
	frag_quantize(rgb);
	frag_rolling_flicker(i, rgb);

	return float4(rgb.x, rgb.y, rgb.z, 1.0) * RGB_SCALE + RGB_SHIFT;
}

fixed4 frag_component(v2f i) : SV_Target
{
	float3 col = tex2D(_MainTex, i.uv).rgb;
	frag_quantize(col);
	float3 yiq = rgb2yiq(col);
	float3 rgb = yiq2rgb(yiq + YIQOFFSET);

	frag_rolling_flicker(i, rgb);

	return float4(rgb.x, rgb.y, rgb.z, 1.0) * RGB_SCALE + RGB_SHIFT;
}

fixed4 frag_tv_overlay(v2f i) : SV_Target
{
#if defined(USE_TV_CURVATURE)
	half2 coords = i.uv;
	coords = (coords - 0.5) * 2.0;

	float2 intensity = float2(_TVCurvature, _TVCurvature) * 0.1;

	half2 realCoordOffs;
	realCoordOffs.x = (coords.y * coords.y) * intensity.y * (coords.x);
	realCoordOffs.y = (coords.x * coords.x) * intensity.x * (coords.y);

	float2 uv = UnityStereoScreenSpaceUVAdjust(i.uv + realCoordOffs, _MainTex_ST);

	half3 color = tex2D(_MainTex, uv).rgb
		* tex2D(_OverlayImg, uv).rgb;

	frag_pixel_mask(uv * _PixelMaskScale.xy, color);

	return float4(color, 1.0);
#else
	half3 color = tex2D(_MainTex, i.uv).rgb * tex2D(_OverlayImg, i.uv).rgb;
	frag_pixel_mask(i.mask_uv, color);
	return float4(color, 1.0);
#endif
}

fixed4 frag_svideo_encode(v2f i) : SV_Target
{
	// svideo signal encode is nearly identical to component signal encode, except that we pass through the input luma
	// instead of the computed 'signal' value

	float3 rgb = tex2D(_MainTex, i.uv);
	frag_quantize(rgb);
	float3 yiq = rgb2yiq(rgb);

	float chroma_phase = SCANLINE_PHASE_OFFSET * ((i.pix_no.y) + _Framecount);
	float mod_phase = chroma_phase + i.pix_no.x * CHROMA_MOD_FREQ;

	float i_mod = cos(mod_phase);
	float q_mod = sin(mod_phase);

	yiq.y *= i_mod * CHROMA_AMP;
	yiq.z *= q_mod * CHROMA_AMP;

	// encode as signal
	float signal = (dot(yiq, float3(1.0, 1.0, 1.0)));

	float3 out_color = float3(yiq.x, signal, signal) * float3(BRIGHTNESS, i_mod * chroma_mod, q_mod * chroma_mod);

	return float4(out_color, 1.0);
}

fixed4 frag_svideo_decode(v2f i) : SV_Target
{
	float one_x = _ScreenSize.z * COMPOSITE_LOWPASS;
	float3 signal = float3(0.0, 0.0, 0.0);

	for (int idx = 0; idx < _FilterSize; idx++)
	{
		float offset = float(idx);

		float3 sums = fetch_offset(i.uv, offset - float(_FilterSize), one_x) +
			fetch_offset(i.uv, (float)_FilterSize - offset, one_x);

		signal += sums * float3(0.0, _ChromaFilter[idx], _ChromaFilter[idx]);
	}
	signal += fetch_signal(i.uv) *
		float3(1.0, _ChromaFilter[_FilterSize], _ChromaFilter[_FilterSize]);

	signal += YIQOFFSET;

	float3 rgb = yiq2rgb(signal);

	frag_rolling_flicker(i, rgb);

	return float4(rgb.r, rgb.g, rgb.b, 1.0) * RGB_SCALE + RGB_SHIFT;
}
// ====================
