using UnityEngine;

/// Cloud shadow camera component.

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera Cloud Shadows")]
public class TOD_Shadows : TOD_ImageEffect
{
	public Shader ShadowShader = null;

	public Texture2D CloudTexture = null;

	[Range(0f, 1f)] public float Cutoff    = 0.0f;
	[Range(0f, 1f)] public float Fade      = 0.0f;
	[Range(0f, 1f)] public float Intensity = 0.5f;

	private Material shadowMaterial = null;

	protected void OnEnable()
	{
		if (!ShadowShader) ShadowShader = Shader.Find("Hidden/Time of Day/Cloud Shadows");

		shadowMaterial = CreateMaterial(ShadowShader);
	}

	protected void OnDisable()
	{
		if (shadowMaterial) DestroyImmediate(shadowMaterial);
	}

	[ImageEffectOpaque]
	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckSupport(true))
		{
			Graphics.Blit(source, destination);
			return;
		}

		sky.Components.Shadows = this;

		shadowMaterial.SetMatrix("_FrustumCornersWS", FrustumCorners());
		shadowMaterial.SetTexture("_CloudTex", CloudTexture);
		shadowMaterial.SetFloat("_Cutoff", Cutoff);
		shadowMaterial.SetFloat("_Fade", Fade);
		shadowMaterial.SetFloat("_Intensity", Intensity * Mathf.Clamp01(1 - sky.SunZenith / 90f));

		CustomBlit(source, destination, shadowMaterial);
	}
}
