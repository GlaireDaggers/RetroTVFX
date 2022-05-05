// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Retro/RetroDiffuse"
{
	Properties
	{
		_Color("Color", COLOR) = (1,1,1,1)
		_AmbientColor("Ambient", COLOR) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}

		_ResolutionX("Resolution X", Int) = 320
		_ResolutionY("Resolution Y", Int) = 240
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				half4 col : TEXCOORD1;
				float3 pos : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _Color;
			float4 _AmbientColor;

			int _ResolutionX;
			int _ResolutionY;

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);

				float2 res = float2(_ResolutionX, _ResolutionY) * 0.5;
				res /= o.vertex.w;

				o.vertex.xy *= res;
				o.vertex.xy = floor(o.vertex.xy);
				o.vertex.xy /= res;

				o.pos = o.vertex.xyz;

				o.uv = TRANSFORM_TEX(v.uv, _MainTex) * o.vertex.z;
				UNITY_TRANSFER_FOG(o,o.vertex);

				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.col = ( dot(worldNormal, float3(0, 1, 0)) + _AmbientColor ) * _Color;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.uv / i.pos.z;
				// sample the texture
				fixed4 col = tex2D(_MainTex, uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col * i.col;
		}
		ENDCG
	}
	}
}
