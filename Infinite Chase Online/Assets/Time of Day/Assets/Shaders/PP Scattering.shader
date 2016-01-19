Shader "Hidden/Time of Day/Scattering"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DitheringTexture ("Dithering Lookup Texture (A)", 2D) = "black" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "TOD_Base.cginc"
	#include "TOD_Scattering.cginc"
	#define BAYER_DIM 8.0

	uniform sampler2D _MainTex;
	uniform sampler2D_float _CameraDepthTexture;
	uniform sampler2D _DitheringTexture;

	uniform float4x4 _FrustumCornersWS;
	uniform float4 _MainTex_TexelSize;
	uniform float4 _Density;

	struct v2f {
		float4 pos       : SV_POSITION;
		float2 uv        : TEXCOORD0;
		float2 uv_depth  : TEXCOORD1;
#if TOD_OUTPUT_DITHERING
		float2 uv_dither : TEXCOORD2;
#endif
		float3 cameraRay : TEXCOORD3;
		float3 skyDir    : TEXCOORD4;
	};

	v2f vert(appdata_img v) {
		v2f o;

		half index = v.vertex.z;
		v.vertex.z = 0.1;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.uv        = v.texcoord.xy;
		o.uv_depth  = v.texcoord.xy;

#if TOD_OUTPUT_DITHERING
		o.uv_dither = v.texcoord.xy * _ScreenParams.xy * (1.0 / BAYER_DIM);
#endif

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0) o.uv.y = 1-o.uv.y;
#endif

		o.cameraRay = _FrustumCornersWS[(int)index];
		o.skyDir    = normalize(mul((float3x3)TOD_World2Sky, o.cameraRay));

		return o;
	}

	inline float FogDensity(float3 cameraToWorldPos)
	{
		float heightFalloff = _Density.x;
		float heightDensity = _Density.y;
		float globalDensity = _Density.z;

		// Unpack depth value
		float fogIntensity = length(cameraToWorldPos) * heightDensity;

		// Apply height falloff
		if (heightFalloff > 0 && abs(cameraToWorldPos.y) > 0.01)
		{
			float t = heightFalloff * cameraToWorldPos.y;
			fogIntensity *= (1.0 - exp(-t)) / t;
		}

		// Clamp intensity
		fogIntensity = min(10, globalDensity * fogIntensity);

		return 1.0 - exp(-fogIntensity);
	}

	half4 frag(v2f i) : COLOR {
		half4 color = tex2D(_MainTex, i.uv);

		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
		float depth = Linear01Depth(rawDepth);
		float3 cameraToWorldPos = depth * i.cameraRay;

		if (depth != 1) depth = FogDensity(cameraToWorldPos);

		half4 scattering = ScatteringColor(normalize(i.skyDir), depth);

#if TOD_OUTPUT_DITHERING
		scattering.rgb += tex2D(_DitheringTexture, i.uv_dither).a * (1.0 / (BAYER_DIM * BAYER_DIM + 1.0));
#endif

#if !TOD_OUTPUT_HDR
		scattering = TOD_HDR2LDR(scattering);
#endif

#if !TOD_OUTPUT_LINEAR
		scattering = TOD_LINEAR2GAMMA(scattering);
#endif

		if (depth == 1)
		{
			color.rgb += scattering.rgb;
		}
		else
		{
			color.rgb = lerp(color.rgb, scattering.rgb, depth);
		}

		return color;
	}
	ENDCG

	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ TOD_OUTPUT_HDR
			#pragma multi_compile _ TOD_OUTPUT_LINEAR
			#pragma multi_compile _ TOD_OUTPUT_DITHERING
			ENDCG
		}
	}

	Fallback Off
}
