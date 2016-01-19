#ifndef TOD_CLOUDS_INCLUDED
#define TOD_CLOUDS_INCLUDED

inline float3 CloudPosition(float3 viewDir)
{
	float mult = 1.0 / lerp(0.15, 1.0, viewDir.y);
	return float3(viewDir.x * mult, 1, viewDir.z * mult) / TOD_CloudSize;
}

inline float3 CloudColor(float3 viewDir, float3 lightDir)
{
	float lerpValue = saturate(1 + 4 * lightDir.y) * saturate(dot(viewDir, lightDir) + 1.25);
	float3 cloudColor = lerp(TOD_MoonCloudColor, TOD_CloudBrightness * TOD_SunCloudColor, lerpValue);
	float3 fogColor = TOD_CloudBrightness * TOD_FogColor;
	return lerp(cloudColor, fogColor, TOD_Fogginess);
}

inline half3 CloudLayerDensity(sampler2D densityTex, float4 uv, float3 viewDir)
{
	half3 density = 0;

#if TOD_CLOUDS_DENSITY
	const half4 stepsize = half4(0.0, 1.0, 2.0, 3.0) * 0.1;
	const half4 sumy = half4(0.5000, 0.2500, 0.1250, 0.0625) / half4(1, 2, 3, 4);
	const half4 sumz = half4(0.5000, 0.2500, 0.1250, 0.0625);

	half2 stepdir = viewDir.xz;

	half4 n1 = tex2D(densityTex, uv.xy + stepdir * stepsize.x);
	half4 n2 = tex2D(densityTex, uv.zw + stepdir * stepsize.y);
	half4 n3 = tex2D(densityTex, uv.xy + stepdir * stepsize.z);
	half4 n4 = tex2D(densityTex, uv.zw + stepdir * stepsize.w);

	// Noise when marching in up direction
	half4 ny = half4(n1.r, n1.g + n2.g, n1.b + n2.b + n3.b, n1.a + n2.a + n3.a + n4.a);

	// Noise when marching in view direction
	half4 nz = half4(n1.r, n2.g, n3.b, n4.a);

	// Density when marching in up direction
	density.y += dot(ny, sumy);

	// Density when marching in view direction
	density.z += dot(nz, sumz);
#else
	half4 n = tex2D(densityTex, uv.xy);

	// Density when marching in up direction
	density.y += n.r;

	// Density when marching in view direction
	density.z += n.r;
#endif

#if TOD_CLOUDS_BUMPED
	half2 stepA = TOD_CloudCoverage;
	half2 stepB = TOD_CloudCoverage + TOD_CloudSharpness;
	half2 stepC = TOD_CloudDensity;

	// Coverage
	density.yz = smoothstep(stepA, stepB, density.yz) + saturate(density.yz - stepB) * stepC;

	// Opacity
	density.x = saturate(density.z);

	// Shading
	density.yz *= half2(TOD_CloudAttenuation, TOD_CloudSaturation);

	// Remap
	density.yz = 1.0 - exp2(-density.yz);
#else
	// Coverage
	density.yz = (density.yz - TOD_CloudCoverage) * half2(TOD_CloudAttenuation, TOD_CloudDensity);

	// Opacity
	density.x = saturate(density.z);
#endif

	return density;
}

inline half4 CloudLayerColor(sampler2D densityTex, float4 uv, float4 color, float3 viewDir, float3 lightDir, float3 lightCol)
{
	half3 density = CloudLayerDensity(densityTex, uv, viewDir);

	half4 res = 0;
	res.a = density.x;
	res.rgb = 1.0 - density.y;

	res *= color;

#if TOD_CLOUDS_BUMPED
	res.rgb += saturate((1.0 - density.z) * (1.0 - TOD_Fogginess)) * lightCol;
#endif

	return res;
}

inline half CloudShadow(sampler2D densityTex, float4 uv)
{
	return CloudLayerDensity(densityTex, uv, float3(0, 1, 0)).y;
}

inline half3 CloudBillboardDensity(sampler2D densityTex, float2 uv, float3 viewDir)
{
	half3 density = 0;

	half4 tex = tex2D(densityTex, uv.xy);

	// Density when marching in up direction
	density.y = tex.a;

	// Density when marching in view direction
#if TOD_CLOUDS_DENSITY
	density.z = lerp(lerp(tex.r, tex.g, saturate(viewDir.x)), tex.b, saturate(-viewDir.x));
#else
	density.z = tex.r;
#endif

	// Opacity
	density.x = saturate(density.z);

	// Shading
	density.y *= TOD_CloudAttenuation;

	return density;
}

inline half4 CloudBillboardColor(sampler2D densityTex, sampler2D normalTex, float4 uv, float4 color, float3 viewDir, float3 lightDir, float3 lightCol)
{
	half3 density = CloudBillboardDensity(densityTex, uv.xy, viewDir);

	half4 res = 0;
	res.a = density.x;
	res.rgb = 1.0 - density.y;

#if TOD_CLOUDS_BUMPED
	half3 normal = UnpackNormal(tex2D(normalTex, uv.zw));
	half NdotS = dot(normal, lightDir);
	res.rgb += 0.25 * (1.0 - TOD_Fogginess) * NdotS;
#endif

	res *= color;

#if TOD_CLOUDS_BUMPED
	res.rgb += saturate((1.0 - density.z) * (1.0 - TOD_Fogginess)) * lightCol;
#endif

	return res;
}

#endif
