// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ArcReactor/Additive_core_higlight" {
Properties {
	_StartColor ("Start Color", Color) = (0.5,0.5,0.5,0.5)	
	_EndColor ("End Color", Color) = (0.5,0.5,0.5,0.5)	
	_CoreColor("Core Color",Color) = (1,1,1,1)
	_FadeLevel("Fade level", Range(0.001,1.0)) = 0
	_MainTex ("Texture", 2D) = "white" {}
	_NoiseMask ("Noise mask", 2D) = "black" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_CoreCoef ("Core highlight", Range(0.0,1.0)) = 0.5	
	_NoiseCoef ("Noise power", Range (-1.0,1.0)) = 1
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _NoiseMask;
			fixed4 _StartColor;
			fixed4 _EndColor;
			fixed4 _CoreColor;
			half _CoreCoef;
			half _NoiseCoef;
			half _FadeLevel;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;												
			};
			
			float4 _MainTex_ST;
			float4 _NoiseMask_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);				
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);				
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : COLOR
			{				
			
				float4 c = tex2D(_MainTex, i.texcoord);
				half fadeCoef = saturate((1-i.texcoord.x)/_FadeLevel);
				float4 col = i.color * lerp(lerp(_StartColor,_EndColor,i.texcoord.x),_CoreColor,_CoreCoef * c.r) * c * 2.0f;
				float4 noise = tex2D(_NoiseMask,TRANSFORM_TEX(i.texcoord,_NoiseMask))*_NoiseCoef;
				c = fadeCoef * (col + noise);
				return c;
			}
			ENDCG 
		}
	}	
}
}
