// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Test/Mono"
{
	Properties
	{
		// Texture
		[Header(Textures)]
		_MainTex ("Main Texture", 2D) = "white" {}

		// Colour settings
		[Header(Colour Settings)]
		_Intensity("Saturation", float) = 1.0
		_Brightness("Brightness", float) = 1.0

		// Colour effect
		[Header(Colour Effects)]
		_EffectDistance("Effect distance", float) = 10.0
		_EffectMaxDist("Effect maximum distance", float) = 10.0
		_EffectMinDist("Effect minimum distance", float) = 2.0
		_EffectScale("Effect scale", float) = 1.0
		_EffectSpeed("Effect speed", float) = 1.0
		_EffectFrequency("Effect frequency", float) = 1.0
	}
	SubShader
	{
		Tags {
			"RenderType"="Opaque" 
		}
		LOD 100
		Pass
		{
			Name "COLOURINGEFFECT"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPosOffset : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _Intensity;
			float _Brightness;

			float _ArraySize;
			float3 _Positions[25];

			float _EffectDistance;
			float _EffectMaxDist;
			float _EffectMinDist;
			float _EffectScale;
			float _EffectSpeed;
			float _EffectFrequency;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.worldPosOffset = mul(unity_ObjectToWorld, v.vertex);

				float3 tempV = o.worldPosOffset;

				o.worldPosOffset.x += _EffectScale * sin(_Time.w * _EffectSpeed + ((tempV.z + tempV.y) / 2) * _EffectFrequency);
				o.worldPosOffset.z += _EffectScale * sin(_Time.w * _EffectSpeed + ((tempV.x + tempV.y) / 2) * _EffectFrequency);

				return o;
			}
			
			fixed4 frag (v2f a_i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, a_i.uv);

				// Calculating saturation and brightness
				fixed lum = saturate(Luminance(col.rgb) * -_Brightness);
				fixed4 output = col;
				output.rgb = lerp(col.rgb, fixed3(lum, lum, lum), -_Intensity);
				output.a = col.a;

				// Finding the lowest distance to the nearest position
				float mag = _EffectDistance;
				for (int i = 0; i < _ArraySize; ++i)
				{
					mag = clamp(distance(_Positions[i], a_i.worldPosOffset), 0, mag);
				}

				// The 'averages' of all colour channels
				float avg = output.r * 0.3 + output.g * 0.59 + output.b * 0.11;

				fixed4 diff = output;

				output.x = 1;
				output.y = 1;
				output.z = 1;

				// Return the colour
				return output - (mul(smoothstep(_EffectMaxDist, _EffectMinDist, mag), 1 - diff));
			}
			ENDCG
		}
	}
}
