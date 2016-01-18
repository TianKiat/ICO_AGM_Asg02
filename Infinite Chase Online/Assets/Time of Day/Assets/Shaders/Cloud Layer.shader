Shader "Time of Day/Cloud Layer"
{
	Properties
	{
		_MainTex ("Density Map (RGBA)", 2D) = "white" {}
		_DitheringTexture ("Dithering Lookup Texture (A)", 2D) = "black" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "TOD_Base.cginc"
	#include "TOD_Clouds.cginc"
	#define TOD_SCATTERING_RAYLEIGH 0
	#include "TOD_Scattering.cginc"
	#define BAYER_DIM 8.0

	uniform sampler2D _MainTex;
	uniform sampler2D _DitheringTexture;

	struct v2f {
		float4 position : SV_POSITION;
		float4 color    : TEXCOORD0;
		float4 texcoord : TEXCOORD1;
		float3 viewDir  : TEXCOORD2;
		float3 lightDir : TEXCOORD3;
		float3 lightCol : TEXCOORD4;
#if TOD_OUTPUT_DITHERING
		float2 dither   : TEXCOORD5;
#endif
	};

	v2f vert(appdata_tan v) {
		v2f o;

		o.position = TOD_TRANSFORM_VERT(v.vertex);

		float3 vertnorm = normalize(v.vertex.xyz);
		float3 cloudPos = CloudPosition(vertnorm);
		o.texcoord.xy = cloudPos.xz + TOD_CloudOffset.xz + TOD_CloudWind.xz;
		o.texcoord.zw = mul(TOD_ROTATION_UV(radians(10.0)), cloudPos.xz) + TOD_CloudOffset.xz + TOD_CloudWind.xz;

#if TOD_OUTPUT_DITHERING
		float4 projPos = ComputeScreenPos(o.position);
		o.dither = projPos.xy / projPos.w * _ScreenParams.xy * (1.0 / BAYER_DIM);
#endif

		o.viewDir = normalize(cloudPos);
		o.lightDir = TOD_LocalSunDirection;
		o.lightCol = ScatteringColor(vertnorm, TOD_CloudScattering);

		o.color.rgb = CloudColor(o.viewDir, o.lightDir);
		o.color.a   = TOD_CloudOpacity * saturate(500 * vertnorm.y * vertnorm.y);

		return o;
	}

	half4 frag(v2f i) : COLOR {
		half4 color = CloudLayerColor(_MainTex, i.texcoord, i.color, i.viewDir, i.lightDir, i.lightCol);

#if TOD_OUTPUT_DITHERING
		color.rgb += tex2D(_DitheringTexture, i.dither).a * (1.0 / (BAYER_DIM * BAYER_DIM + 1.0));
#endif

		return color;
	}
	ENDCG

	SubShader
	{
		Tags
		{
			"Queue"="Geometry+530"
			"RenderType"="Background"
			"IgnoreProjector"="True"
		}

		Pass
		{
			ZWrite Off
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ TOD_OUTPUT_DITHERING
			#pragma multi_compile _ TOD_CLOUDS_DENSITY
			#pragma multi_compile _ TOD_CLOUDS_BUMPED
			ENDCG
		}
	}

	Fallback Off
}
