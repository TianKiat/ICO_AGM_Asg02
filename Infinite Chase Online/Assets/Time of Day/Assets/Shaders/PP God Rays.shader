Shader "Hidden/Time of Day/God Rays"
{
	Properties
	{
		_MainTex ("Base", 2D) = "" {}
		_ColorBuffer ("Color", 2D) = "" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
		#if UNITY_UV_STARTS_AT_TOP
		float2 uv1 : TEXCOORD1;
		#endif
	};

	struct v2f_radial {
		float4 pos        : SV_POSITION;
		float2 uv         : TEXCOORD0;
		float2 blurVector : TEXCOORD1;
	};

	sampler2D _MainTex;
	sampler2D _ColorBuffer;
	sampler2D_float _CameraDepthTexture;

	uniform half4 _LightColor;
	uniform half4 _BlurRadius4;
	uniform half4 _LightPosition;
	uniform half4 _MainTex_TexelSize;

	#define SAMPLES 6

	v2f vert(appdata_img v) {
		v2f o;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;

		#if UNITY_UV_STARTS_AT_TOP
		o.uv1 = v.texcoord.xy;
		if (_MainTex_TexelSize.y < 0)
			o.uv1.y = 1-o.uv1.y;
		#endif

		return o;
	}

	v2f_radial vert_radial(appdata_img v) {
		v2f_radial o;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  v.texcoord.xy;
		o.blurVector = (_LightPosition.xy - v.texcoord.xy) * _BlurRadius4.xy;

		return o;
	}

	half TransformColor(half4 skyboxValue) {
		return 1.01-skyboxValue.a;
	}

	half4 frag_depth(v2f i) : COLOR {
		half4 sceneColor = tex2D(_MainTex, i.uv);

		// Sample depth
		#if UNITY_UV_STARTS_AT_TOP
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv1);
		#else
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
		#endif
		float depth = Linear01Depth(rawDepth);

		// Consider maximum radius
		#if UNITY_UV_STARTS_AT_TOP
		half2 vec = _LightPosition.xy - i.uv1;
		#else
		half2 vec = _LightPosition.xy - i.uv;
		#endif
		half dist = saturate(_LightPosition.w - length(vec));

		// Consider shafts blockers
		return (depth > 0.99) ? TransformColor(sceneColor) * dist : 0;
	}

	half4 frag_nodepth(v2f i) : COLOR {
		half4 sceneColor = tex2D(_MainTex, i.uv);

		// Consider maximum radius
		#if UNITY_UV_STARTS_AT_TOP
		half2 vec = _LightPosition.xy - i.uv1;
		#else
		half2 vec = _LightPosition.xy - i.uv;
		#endif
		half dist = saturate(_LightPosition.w - length(vec));

		// Consider shafts blockers
		return TransformColor(sceneColor) * dist;
	}

	half4 frag_radial(v2f_radial i) : COLOR {
		half4 color = half4(0,0,0,0);
		for(int j = 0; j < int(SAMPLES); j++)
		{
			half4 tmpColor = tex2D(_MainTex, i.uv);
			color += tmpColor;
			i.uv += i.blurVector;
		}
		return color / float(SAMPLES);
	}

	half4 frag_screen(v2f i) : COLOR {
		half4 colorA = tex2D(_MainTex, i.uv);
		#if UNITY_UV_STARTS_AT_TOP
		half4 colorB = tex2D(_ColorBuffer, i.uv1);
		#else
		half4 colorB = tex2D(_ColorBuffer, i.uv);
		#endif
		half4 depthMask = saturate(colorB * _LightColor);
		return 1.0f - (1.0f-colorA) * (1.0f-depthMask);
	}

	half4 frag_add(v2f i) : COLOR {
		half4 colorA = tex2D(_MainTex, i.uv);
		#if UNITY_UV_STARTS_AT_TOP
		half4 colorB = tex2D(_ColorBuffer, i.uv1);
		#else
		half4 colorB = tex2D(_ColorBuffer, i.uv);
		#endif
		half4 depthMask = saturate(colorB * _LightColor);
		return colorA + depthMask;
	}
	ENDCG

	Subshader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_screen
			ENDCG
		}

		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert_radial
			#pragma fragment frag_radial
			ENDCG
		}

		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_depth
			ENDCG
		}

		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_nodepth
			ENDCG
		}

		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_add
			ENDCG
		}
	}

	Fallback Off
}
