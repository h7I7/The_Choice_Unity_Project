// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/NormalDepthEdgeDetection"
{
	Properties
	{
		// Diffuse texture
		_MainTex("Base (RGB)", 2D) = "white" {}
		_EdgeColor("Edge Colour (RGB)", Color) = (0,0,0,1)
		_Distance("Effect Distance Percent", Range(0.0, 1.0)) = 1.0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}
		//Base Pass
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			sampler2D _MainTex; float4 _MainTex_ST;
			//sampler2D _CameraDepthTexture; float4 _CameraDepthTexture_ST;
			sampler2D _CameraDepthNormalsTexture; float4 _CameraDepthNormalsTexture_ST;
			// TexelSize format = (1.0/width, 1.0/height, width, height)
			float4 _CameraDepthTexture_TexelSize;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			float _Distance;

			half4 frag(v2f i) : COLOR
			{
				half4 texcol;
				float2 invUv = i.uv;

				//invUv.y = 1 - invUv.y;

				float3 normalValues;
				float depthValue;
				DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, invUv), depthValue, normalValues);
				
				float distStep = 1 - step(_Distance, depthValue);
				
				float3 normalColor = float3(normalValues.x, normalValues.y, normalValues.z) * log(LinearEyeDepth(depthValue)) * distStep;
				//our output texture is our normal data as colours and our depth as alpha
				texcol = float4(normalColor, 1.0);
				return texcol;
			}
			ENDCG
		}
		//Get the output
		GrabPass{}

		//Edge Pass
		Pass
		{
			ZTest LEqual Cull Off ZWrite Off
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
	
			sampler2D _GrabTexture : register(s0); float4 _GrabTexture_ST;

			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			sampler2D _MainTex; float4 _MainTex_ST;
			sampler2D _CameraDepthNormalsTexture; float4 _CameraDepthNormalsTexture_ST;

			float4 _MainTex_TexelSize;
			float4 _EdgeColor;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _GrabTexture);
				return o;
			}

			float edgeDetect(sampler2D tex, float2 uv)
			{
				//we need to get the screen parameters and shift over our reads by one pixel
				float2 delta = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);

				float3 hr = float3(0, 0, 0);
				float3 vt = float3(0, 0, 0);
				//Read out our adjacent Pixels
				//sample for depth + Normals
				// ---------------
				// | m |    | m  |
				// | 2 |    | 3  |
				// ---------------
				// |   | m  |    |
				// |   | 1  |    |
				// ---------------
				// | m |    | m  |
				// | 4 |    | 5  |
				// ---------------
				float4 m1 = tex2D(tex, (uv));
				float4 m2 = tex2D(tex, (uv + float2(-1.0,  1.0) * delta));
				float4 m3 = tex2D(tex, (uv + float2(1.0,  1.0) * delta));
				float4 m4 = tex2D(tex, (uv + float2(-1.0, -1.0) * delta));
				float4 m5 = tex2D(tex, (uv + float2(1.0, -1.0) * delta));

				float ld0; float ld1; float ld2; float ld3; float ld4;
				ld0 = Linear01Depth(m1.a);
				ld1 = Linear01Depth(m2.a);
				ld2 = Linear01Depth(m3.a);
				ld3 = Linear01Depth(m4.a);
				ld4 = Linear01Depth(m5.a);

				float3 n0 = m2.rgb;
				float3 n1 = m3.rgb;
				float3 n2 = m4.rgb;
				float3 n3 = m5.rgb;

				float3 ng1 = n3 - n0;
				float3 ng2 = n2 - n1;
				float ng = step(0.5,sqrt(dot(ng1, ng1) + dot(ng2, ng2)));



				float depthValue = step(0.001, ld0 - ((ld1 + ld2 + ld3 + ld4) * 0.25f));


				return  clamp(0, 1, ng + depthValue);
			}

			half4 frag(v2f i) : COLOR
			{
				half4 texcol;
				float sobelValue = edgeDetect(_GrabTexture, i.uv);
				float4 mainTex = tex2D(_MainTex, i.uv);
				float4 interpolatedColor = lerp(mainTex, _EdgeColor, sobelValue);
				texcol = interpolatedColor;

				return texcol;
			}
			ENDCG
		}
	}
}