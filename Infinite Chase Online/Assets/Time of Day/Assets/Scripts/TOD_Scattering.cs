using UnityEngine;

/// Atmospheric scattering and aerial perspective camera component.

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera Atmospheric Scattering")]
public class TOD_Scattering : TOD_ImageEffect
{
	public Shader ScatteringShader = null;

	public Texture2D DitheringTexture = null;

	[Range(0f, 1f)] public float GlobalDensity = 0.001f;
	[Range(0f, 1f)] public float HeightFalloff = 0.001f;
	public float ZeroLevel = 0.0f;

	private Material scatteringMaterial = null;

	protected void OnEnable()
	{
		if (!ScatteringShader) ScatteringShader = Shader.Find("Hidden/Time of Day/Scattering");

		scatteringMaterial = CreateMaterial(ScatteringShader);
	}

	protected void OnDisable()
	{
		if (scatteringMaterial) DestroyImmediate(scatteringMaterial);
	}

	protected void OnPreCull()
	{
		if (sky && sky.Initialized) sky.Components.AtmosphereRenderer.enabled = false;
	}

	protected void OnPostRender()
	{
		if (sky && sky.Initialized) sky.Components.AtmosphereRenderer.enabled = true;
	}

	[ImageEffectOpaque]
	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckSupport(true))
		{
			Graphics.Blit(source, destination);
			return;
		}

		sky.Components.Scattering = this;

		float heightFalloff = HeightFalloff;
		float heightDensity = Mathf.Exp(-heightFalloff * ( cam.transform.position.y - ZeroLevel));
		float globalDensity = GlobalDensity;

		scatteringMaterial.SetMatrix("_FrustumCornersWS", FrustumCorners());
		scatteringMaterial.SetTexture("_DitheringTexture", DitheringTexture);
		scatteringMaterial.SetVector("_Density", new Vector4(heightFalloff, heightDensity, globalDensity, 0));

		CustomBlit(source, destination, scatteringMaterial);
	}
}
