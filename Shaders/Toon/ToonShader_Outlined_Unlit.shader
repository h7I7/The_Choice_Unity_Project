// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ToonShader_Outlined_Unlit"
{
	// Variables
	Properties
	{
		// TEXTURE
		// Allows for a texture property
		[Header(Textures)]
		[NoScaleOffset] _MainTex("Main Texture (RGB)", 2D) = "white" {}
		_Color("Main Color", Color) = (.5,.5,.5,1)

		// OUTLINE
		[Header(Outline Settings)]
		_OutlineTex("Outline texture", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth("Outline Width", Range(0.0, 5.0)) = 0.2
		_OutlineWidthFill("Outline Width Fill Amount", Range(0.0, 1.0)) = 1.0

		// Colour settings
		[Header(Colour Settings)]
		_Intensity("Saturation", float) = 1.0
		_Brightness("Brightness", float) = 1.0

		// COLOURING		
		[Header(Coloruing Effect Settings)]
		_EffectColor("Effect color", color) = (1, 1, 1, 1)
		[HideInInspector]
		_EffectDistance("Effect distance", float) = 10.0
		_EffectStrokeColor("Effect stroke color", color) = (0, 0, 0, 1)
		_EffectRadius("Effect stroke weight", float) = 0.1
		_EffectScale("Effect scale", float) = 0.1
		_EffectSpeed("Effect speed", float) = 1.0
		_EffectFrequency("Effect frequency", float) = 1.0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent+10"
		}

		// Outline
		Pass
		{
			Name "OUTLINE"

			ZWrite Off
			Cull Front
			//ZTest LEqual

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _OutlineTex;
			float4 _OutlineColor;
			float _OutlineWidth;
			float _OutlineWidthFill;

			v2f vert(appdata IN)
			{
				IN.vertex.xyz += IN.color * _OutlineWidth * _OutlineWidthFill;
				
				v2f OUT;

				OUT.pos = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_TARGET
			{
				float4 texColor = tex2D(_OutlineTex, IN.uv);
				return texColor * _OutlineColor;
			}

			ENDCG
		}
	
		// Coloured and lit pass
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}
		Lighting On
		LOD 100
		Pass
		{
			Name "COLOUREFFECTLIT"

			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float3 worldPosOffset : TEXCOORD1;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _Color;

			float _Intensity;
			float _Brightness;


			float _ArraySize;
			float4 _Positions[1000];
			float _Distances[1000];


			float4 _EffectColor;
			float _EffectDistance;
			float4 _EffectStrokeColor;
			float _EffectRadius;
			float _EffectScale;
			float _EffectSpeed;
			float _EffectFrequency;

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex); // Tiling

				// Moving world pos for colouring circle ripple effect
				o.worldPosOffset = mul(unity_ObjectToWorld, v.vertex);

				float3 tempV = o.worldPosOffset;
				
				o.worldPosOffset.x += _EffectScale * sin(_Time.w * _EffectSpeed + ((tempV.z + tempV.y) * 0.5) * _EffectFrequency);
				o.worldPosOffset.z += _EffectScale * sin(_Time.w * _EffectSpeed + ((tempV.x + tempV.y) * 0.5) * _EffectFrequency);

				return o;
			}

			fixed4 frag(v2f a_i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, a_i.texcoord);
				col *= _Color;

				// Calculating saturation and brightness
				fixed lum = saturate(Luminance(col.rgb) * -_Brightness);
				fixed4 output = col;
				output.rgb = lerp(col.rgb, fixed3(lum, lum, lum), -_Intensity);
				output.a = col.a;

				// The following code is really janky but it works and
				// its decently efficient considering what I'd have to do
				// to make it not janky so I'm leaving it like this

				// Finding the lowest distance to the nearest position
				// This starts as a really high numbers so that it is guaranteed there
				// is a position in range of this number
				float mag = 10000;
				for (int i = 0; i < _ArraySize; ++i)
				{
					float dist = distance(_Positions[i].xyz, a_i.worldPosOffset);
					float offsetDist = dist - _Distances[i] + 10;

					mag = clamp(offsetDist, 0, mag);
				}
			
				float distanceTwo = _EffectDistance + -_EffectRadius;

				fixed4 borderColor = output - _EffectStrokeColor;

				fixed4 outputColour = output + (mul(step(_EffectDistance, mag), _EffectColor)) - (mul(step(distanceTwo, mag), borderColor));

				// Return the colour
				return outputColour;
			}
			ENDCG
		}

		// pull in shadow caster from VertexLit built-in shader
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}

	Fallback "VertexLit"
}


