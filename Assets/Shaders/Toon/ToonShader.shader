Shader "Custom/ToonShader"
{
	// Variables
	Properties
	{
		// TEXTURE
		// Allows for a texture property
		[Header(Textures)]
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
		_Color("Main Color", Color) = (.5,.5,.5,1)

		// Colouring		
		[Header(Coloruing Effect Settings)]
		_EffectColor("Effect color", color) = (1, 1, 1, 1)
		[HideInInspector]
		_EffectDistance("Effect distance", float) = 10.0
		_EffectStrokeColor("Effect stroke color", color) = (0, 0, 0, 1)
		_EffectRadius("Effect stroke weight", float) = 0.1
		_EffectScale("Effect scale", float) = 0.1
		_EffectSpeed("Effect speed", float) = 1.0
		_EffectFrequency("Effect frequency", float) = 1.0

		[HideInInspector]
		_ClosestDistance("Closest distance to the fragment", float) = 10000
	}
	SubShader
	{	
		// Coloured and lit pass
		Tags
		{
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}
		Lighting On
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase
			// Enable batching on shadows
			#pragma multi_compile_shadowcaster
			// Instancing
			#pragma multi_compile_instancing
			// Enable higher instancing batch count
			// Disable this if deploying to mobile as it will not work
			#pragma instancing_options force_same_maxcount_for_gl
			// Prepassing
			#pragma multi_compile_prepassfinal
			// shadow helper functions and macros
			#include "AutoLight.cginc"

			struct v2f
			{
				float2 texcoord			: TEXCOORD0;
				float3 normal			: TEXCOORD1;
				float3 lightDir			: TEXCOORD2;
				LIGHTING_COORDS(3,4)
				float3 worldPosOffset	: TEXCOORD5;
				float4 pos				: SV_POSITION;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			uniform fixed _ArraySize;
			uniform fixed4 _Positions[1000];
			uniform fixed _Distances[1000];

			UNITY_INSTANCING_CBUFFER_START(InstanceProperties)
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)

				UNITY_DEFINE_INSTANCED_PROP(fixed4, _EffectColor)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectDistance)
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _EffectStrokeColor)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectRadius)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectScale)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectSpeed)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectFrequency)
			UNITY_INSTANCING_CBUFFER_END

			v2f vert(appdata_full v)
			{
				v2f o;
				
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.pos = UnityObjectToClipPos(v.vertex);
				
				// Tiling
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				// Lighting
				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.normal = v.normal;

				// Moving world pos for colouring circle ripple effect
				o.worldPosOffset = mul(unity_ObjectToWorld, v.vertex);

				float3 tempV = o.worldPosOffset;
				
				o.worldPosOffset.x += UNITY_ACCESS_INSTANCED_PROP(_EffectScale) * sin(_Time.w * UNITY_ACCESS_INSTANCED_PROP(_EffectSpeed) + ((tempV.z + tempV.y) * 0.5) * UNITY_ACCESS_INSTANCED_PROP(_EffectFrequency));
				o.worldPosOffset.z += UNITY_ACCESS_INSTANCED_PROP(_EffectScale) * sin(_Time.w * UNITY_ACCESS_INSTANCED_PROP(_EffectSpeed) + ((tempV.x + tempV.y) * 0.5) * UNITY_ACCESS_INSTANCED_PROP(_EffectFrequency));

				// Compute shadow data
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return o;
			}

			fixed4 frag(v2f a_i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(a_i);

				a_i.lightDir = normalize(a_i.lightDir);

				fixed4 col = tex2D(_MainTex, a_i.texcoord);
				col *= UNITY_ACCESS_INSTANCED_PROP(_Color);
				fixed4 output = col;

				// Compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
				fixed atten = LIGHT_ATTENUATION(a_i);

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

				float distanceTwo = UNITY_ACCESS_INSTANCED_PROP(_EffectDistance) + -UNITY_ACCESS_INSTANCED_PROP(_EffectRadius);

				fixed4 borderColor = output - UNITY_ACCESS_INSTANCED_PROP(_EffectStrokeColor);

				fixed4 outputColour = output + (mul(step(UNITY_ACCESS_INSTANCED_PROP(_EffectDistance), mag), UNITY_ACCESS_INSTANCED_PROP(_EffectColor))) - (mul(step(distanceTwo, mag), borderColor));
				
				
				fixed diff = saturate(dot(a_i.normal, a_i.lightDir));

				fixed4 c;
				c.rgb = (outputColour * _LightColor0.rgb * diff) * (atten * 2);
				c.a = outputColour.a;

				// Return the colour
				return c;
			}
			ENDCG
		}

		// Blending in other light sources such as point lights
		Pass
		{
			Tags{ "LightMode" = "ForwardAdd" }                       // Again, this pass tag is important otherwise Unity may not give the correct light information.
			Blend One One                                         // Additively blend this pass with the previous one(s). This pass gets run once per pixel light.
			
			Cull Back
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// This line tells Unity to compile this pass for forward add, giving attenuation information for the light.
			#pragma multi_compile_fwdadd_fullshadows   
			// Enable batching on shadows
			#pragma multi_compile_shadowcaster
			// Instancing
			#pragma multi_compile_instancing
			// Enable higher instancing batch count
			// Disable this if deploying to mobile as it will not work
			#pragma instancing_options force_same_maxcount_for_gl
			// Prepassing
			#pragma multi_compile_prepassfinal

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct v2f
			{
				float4 pos		: SV_POSITION;
				float2 texcoord	: TEXCOORD0;
				float3 normal	: TEXCOORD1;
				float3 lightDir	: TEXCOORD2;
				LIGHTING_COORDS(3,4)                            // Macro to send shadow & attenuation to the vertex shader.
				float3 worldPosOffset	: TEXCOORD5;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform fixed _ArraySize;
			uniform fixed4 _Positions[1000];
			uniform fixed _Distances[1000];

			UNITY_INSTANCING_CBUFFER_START(InstanceProperties)
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)

				UNITY_DEFINE_INSTANCED_PROP(fixed4, _EffectColor)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectDistance)
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _EffectStrokeColor)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectRadius)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectScale)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectSpeed)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _EffectFrequency)
			UNITY_INSTANCING_CBUFFER_END

			v2f vert(appdata_full v)
			{
				v2f o;

				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.pos = UnityObjectToClipPos(v.vertex);

				// Tiling
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.normal = v.normal;

				// Moving world pos for colouring circle ripple effect
				o.worldPosOffset = mul(unity_ObjectToWorld, v.vertex);

				float3 tempV = o.worldPosOffset;

				o.worldPosOffset.x += UNITY_ACCESS_INSTANCED_PROP(_EffectScale) * sin(_Time.w * UNITY_ACCESS_INSTANCED_PROP(_EffectSpeed) + ((tempV.z + tempV.y) * 0.5) * UNITY_ACCESS_INSTANCED_PROP(_EffectFrequency));
				o.worldPosOffset.z += UNITY_ACCESS_INSTANCED_PROP(_EffectScale) * sin(_Time.w * UNITY_ACCESS_INSTANCED_PROP(_EffectSpeed) + ((tempV.x + tempV.y) * 0.5) * UNITY_ACCESS_INSTANCED_PROP(_EffectFrequency));

				TRANSFER_VERTEX_TO_FRAGMENT(o);                 // Macro to send shadow & attenuation to the fragment shader.

				return o;
			}

			fixed4 _LightColor0; // Colour of the light used in this pass.

			fixed4 frag(v2f a_i) : COLOR
			{
				UNITY_SETUP_INSTANCE_ID(a_i);

				a_i.lightDir = normalize(a_i.lightDir);

				fixed4 col = tex2D(_MainTex, a_i.texcoord);
				col *= UNITY_ACCESS_INSTANCED_PROP(_Color);
				fixed4 output = col;

				// Compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
				fixed atten = LIGHT_ATTENUATION(a_i);

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

				float distanceTwo = UNITY_ACCESS_INSTANCED_PROP(_EffectDistance) + -UNITY_ACCESS_INSTANCED_PROP(_EffectRadius);
				fixed4 borderColor = output - UNITY_ACCESS_INSTANCED_PROP(_EffectStrokeColor);

				fixed4 outputColour = output + (mul(step(UNITY_ACCESS_INSTANCED_PROP(_EffectDistance), mag), UNITY_ACCESS_INSTANCED_PROP(_EffectColor))) - (mul(step(distanceTwo, mag), borderColor));


				fixed diff = saturate(dot(a_i.normal, a_i.lightDir));

				fixed4 c;
				c.rgb = (outputColour * _LightColor0.rgb * diff) * (atten * 2);
				c.a = outputColour.a;

				// Return the colour
				return c;
			}
			ENDCG
		}

		// pull in shadow caster from VertexLit built-in shader
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"		
	}

	FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}