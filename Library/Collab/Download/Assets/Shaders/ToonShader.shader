// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ToonShader"
{
	// Variables
	Properties
	{
		// TEXTURE
		// Allows for a texture property
		[Header(Textures)]
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
		// Color property
		_Color("Color", Color) = (1,1,1,1)

		// OUTLINE
		[Header(Outline Settings)]
		_OutlineTex("Outline Texture (RGB)", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
		_OutlineWidth("Outline Width", Range(1.0, 5.0)) = 1.1

		// LIGHTING
		[Header(Lighting)]
		_Threshold("Cel Threshold", Range(0.0, 20.0)) = 0.1
		_Ambient("Ambient Intesity", Range(0.0, 1.0)) = 0.1

		// Colour settings
		[Header(Colour Settings)]
		_Intensity("Saturation", float) = 1.0
		_Brightness("Brightness", float) = 1.0

		// COLOURING		
		[Header(Coloruing Effect Settings)]
		_EffectDistance("Effect distance", float) = 10.0
		_EffectMaxDist("Effect maximum distance", float) = 10.0
		_EffectMinDist("Effect minimum distance", float) = 2.0
		_EffectScale("Effect scale", float) = 1.0
		_EffectSpeed("Effect speed", float) = 1.0
		_EffectFrequency("Effect frequency", float) = 1.0
	}
	Subshader
	{
		Tags
		{
			"Queue" = "Transparent"
		}

		// Rendering the outline
		Pass
		{
			Name "OUTLINE"

			ZWrite Off

			// Lets ShaderLab(SL) communicate with CG (C for Graphics)
			CGPROGRAM

			//\==================================================
			//\	Function defines
			//\==================================================

			// Define for the building function
			#pragma vertex vert
			// Define for coloring function
			#pragma fragment frag


			//\==================================================
			//\	Includes
			//\==================================================

			// Built in shader functions
			#include "UnityCG.cginc"


			//\==================================================
			//\	Structures - Can recieve data like vertices, normals, colors, uv
			//\==================================================

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};


			//\==================================================
			//\	Imports - Re-import properties from SL to CG
			//\==================================================

			float4 _OutlineColor;
			sampler2D _OutlineTex;
			float _OutlineWidth;


			//\==================================================
			//\	Vertex function - Builds the object
			//\==================================================

			v2f vert(appdata IN)
			{
				IN.vertex.xyz *= _OutlineWidth;

				v2f OUT;

				OUT.pos = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;

				return OUT;
			}


			//\==================================================
			//\	Fragment function - Colors it in
			//\==================================================

			fixed4 frag(v2f IN) : SV_Target
			{
				float4 texColor = tex2D(_OutlineTex, IN.uv);

				return texColor * _OutlineColor;
			}

			ENDCG
		}


		UsePass "Test/Mono/COLOURINGEFFECT"


		//Tags
		//{
		//	"RenderType" = "Opaque"
		//}
		//
		////Rendering the texture and lighting if any
		//Pass
		//{
		//	Blend SrcColor DstColor

		//	// Lets ShaderLab(SL) communicate with CG (C for Graphics)
		//	CGPROGRAM
		//
		//
		//	//\==================================================
		//	//\	Function defines
		//	//\==================================================
		//
		//	// Define for the building function
		//	#pragma vertex vert
		//	// Define for coloring function
		//	#pragma fragment frag
		//
		//
		//	//\==================================================
		//	//\	Includes
		//	//\==================================================
		//
		//	// Built in shader functions
		//	#include "UnityCG.cginc"
		//
		//
		//	//\==================================================
		//	//\	Structures - Can recieve data like vertices, normals, colors, uv
		//	//\==================================================
		//
		//	struct appdata
		//	{
		//		float4 vertex : POSITION;
		//		float2 uv : TEXCOORD0;
		//	};
		//
		//	struct v2f
		//	{
		//		float4 pos : SV_POSITION;
		//		float2 uv : TEXCOORD0;
		//		float3 worldNormal : NORMAL;
		//	};
		//
		//
		//	//\==================================================
		//	//\	Imports - Re-import properties from SL to CG
		//	//\==================================================
		//
		//	float4 _Color;
		//	sampler2D _MainTex;
		//	float4 _MainTex_ST;
		//
		//	fixed4 _LightColor0;
		//	half _Ambient;
		//
		//	float _Threshold;
		//
		//
		//	//\==================================================
		//	//\	Toon shading function - Calculates the angle between a texel (I think) and the lighting
		//	//\==================================================
		//
		//	float LightToonShading(float3 normal, float3 lightDir)
		//	{
		//		float NdotL = max(0.0, dot(normalize(normal), normalize(lightDir)));
		//		return floor(NdotL * _Threshold) / (_Threshold - 0.5);
		//	}

		//
		//	//\==================================================
		//	//\	Vertex function - Builds the object
		//	//\==================================================
		//
		//	v2f vert(appdata_full IN)
		//	{
		//		v2f OUT;
		//
		//		OUT.pos = UnityObjectToClipPos(IN.vertex);
		//		OUT.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
		//		OUT.worldNormal = mul(IN.normal.xyz, (float3x3)unity_WorldToObject);
		//
		//		return OUT;
		//	}
		//
		//
		//	//\==================================================
		//	//\	Fragment function - Colors it in
		//	//\==================================================
		//
		//	fixed4 frag(v2f IN) : SV_Target
		//	{
		//		fixed4 col = _Color;
		//		col.rgb *= saturate(LightToonShading(IN.worldNormal, _WorldSpaceLightPos0.xyz) + _Ambient) * _LightColor0.rgb;
		//		return col;
		//	}
		//
		//	ENDCG
		//}
	}
}
