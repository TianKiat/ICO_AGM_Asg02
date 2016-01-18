Shader "Time of Day/Cloud Billboard Far"
{
	Properties
	{
		_MainTex ("Density Map (RGBA)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "TOD_Base.cginc"
	#include "TOD_Clouds.cginc"
	#define TOD_SCATTERING_RAYLEIGH 0
	#include "TOD_Scattering.cginc"

	uniform sampler2D _MainTex;
	uniform sampler2D _BumpMap;

	uniform float4 _MainTex_ST;
	uniform float4 _BumpMap_ST;

	struct v2f {
		float4 position : SV_POSITION;
		float4 color    : TEXCOORD0;
		float4 texcoord : TEXCOORD1;
		float3 viewDir  : TEXCOORD2;
		float3 lightDir : TEXCOORD3;
		float3 lightCol : TEXCOORD4;
	};

	v2f vert(appdata_tan v) {
		v2f o;

		o.position = TOD_TRANSFORM_VERT(v.vertex);

		o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.texcoord.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);

		o.viewDir = normalize(mul(TOD_World2Sky, mul(_Object2World, v.vertex)).xyz);
		o.lightDir = TOD_LocalSunDirection;
		o.lightCol = ScatteringColor(o.viewDir, TOD_CloudScattering);

		o.color.rgb = CloudColor(o.viewDir, o.lightDir);
		o.color.a   = TOD_CloudOpacity;

		TANGENT_SPACE_ROTATION;
		o.viewDir = normalize(mul(rotation, o.viewDir));
		o.lightDir = normalize(mul(rotation, o.lightDir));

		return o;
	}

	half4 frag(v2f i) : COLOR {
		return CloudBillboardColor(_MainTex, _BumpMap, i.texcoord, i.color, i.viewDir, i.lightDir, i.lightCol);
	}
	ENDCG

	SubShader
	{
		Tags
		{
			"Queue"="Geometry+520"
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
			#pragma multi_compile _ TOD_CLOUDS_DENSITY
			#pragma multi_compile _ TOD_CLOUDS_BUMPED
			ENDCG
		}
	}

	Fallback Off
}
