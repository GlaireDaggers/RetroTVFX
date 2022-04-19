// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/DitherShader"
{
	Properties
	{
		_Color("Color", COLOR) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}

		_ResolutionX("Resolution X", Int) = 320
		_ResolutionY("Resolution Y", Int) = 240
	}
		SubShader
	{
		Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
		LOD 100

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		half4 col : COLOR;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	float4 _Color;

	int _ResolutionX;
	int _ResolutionY;

	v2f vert(appdata v, out float4 outpos : SV_POSITION)
	{
		v2f o;

		outpos = UnityObjectToClipPos(v.vertex);

		float2 res = float2(_ResolutionX, _ResolutionY) * 0.5;
		res /= outpos.w;

		outpos.xy *= res;
		outpos.xy = floor(outpos.xy);
		outpos.xy /= res;

		o.uv = TRANSFORM_TEX(v.uv, _MainTex);

		float3 worldNormal = UnityObjectToWorldNormal(v.normal);
		o.col =  _Color;

		return o;
	}

	fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target 
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.uv);

		// checker value will be negative for 4x4 blocks of pixels
		// in a checkerboard pattern
		screenPos.xy = floor(screenPos.xy) * 0.5;
		float checker = -frac(screenPos.r + screenPos.g);

		// clip HLSL instruction stops rendering a pixel if value is negative
		clip(checker);

		return col * i.col;
	}
		ENDCG
	}
	}
}
