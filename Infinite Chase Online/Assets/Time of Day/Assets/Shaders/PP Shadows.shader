Shader "Hidden/Time of Day/Cloud Shadows"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CloudTex ("Alpha Layers (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "TOD_Base.cginc"
	#include "TOD_Clouds.cginc"

	uniform sampler2D _MainTex;
	uniform sampler2D _CloudTex;
	uniform sampler2D_float _CameraDepthTexture;

	uniform float4x4 _FrustumCornersWS;
	uniform float4 _MainTex_TexelSize;
	uniform float _Cutoff;
	uniform float _Fade;
	uniform float _Intensity;

	struct v2f {
		float4 pos       : SV_POSITION;
		float2 uv        : TEXCOORD0;
		float2 uv_depth  : TEXCOORD1;
		float3 cameraRay : TEXCOORD2;
	};

	v2f vert(appdata_img v) {
		v2f o;

		half index = v.vertex.z;
		v.vertex.z = 0.1;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.uv       = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;

		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1-o.uv.y;
		#endif

		o.cameraRay = _FrustumCornersWS[(int)index];

		return o;
	}

	half4 frag(v2f i) : COLOR {
		half4 sceneColor = tex2D(_MainTex, i.uv);

		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
		float depth = Linear01Depth(rawDepth);
		float3 cameraToWorldPos = depth * i.cameraRay;
		float3 worldPos = _WorldSpaceCameraPos + cameraToWorldPos;
		float3 skyPos = mul((float3x3)TOD_World2Sky, worldPos);

		float3 vertnorm = TOD_LocalLightDirection;
		float3 cloudPos = CloudPosition(vertnorm);

		float4 cloudUV = 0;
		cloudUV.xy = skyPos.xz + TOD_CloudWind.xz;
		cloudUV.zw = mul(TOD_ROTATION_UV(radians(10.0)), skyPos.xz) + TOD_CloudWind.xz;

		half a = TOD_CloudOpacity * CloudShadow(_CloudTex, cloudUV);
		a *= smoothstep(_Cutoff, _Cutoff + _Fade, Luminance(sceneColor.rgb)) * _Intensity;

		if (depth == 1)
		{
			return sceneColor;
		}
		else
		{
			return half4(sceneColor.rgb * (1-a), sceneColor.a);
		}
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
			#pragma multi_compile _ TOD_OUTPUT_LINEAR
			#pragma multi_compile _ TOD_CLOUDS_DENSITY
			#pragma multi_compile _ TOD_CLOUDS_BUMPED
			ENDCG
		}
	}

	Fallback Off
}
