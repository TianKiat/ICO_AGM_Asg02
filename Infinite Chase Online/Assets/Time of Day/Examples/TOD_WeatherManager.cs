using UnityEngine;

public class TOD_WeatherManager : MonoBehaviour
{
	public enum RainType
	{
		None,
		Light,
		Heavy
	}

	public enum CloudType
	{
		None,
		Few,
		Scattered,
		Broken,
		Overcast
	}

	public enum AtmosphereType
	{
		Clear,
		Storm,
		Dust,
		Fog
	}

	public ParticleSystem RainParticleSystem = null;

	public float FadeTime = 10f;

	public RainType       Rain       = default(RainType);
	public CloudType      Clouds     = default(CloudType);
	public AtmosphereType Atmosphere = default(AtmosphereType);

	private float cloudOpacityMax;
	private float cloudBrightnessMax;
	private float atmosphereBrightnessMax;
	private float rainEmissionMax;

	private float cloudOpacity;
	private float cloudCoverage;
	private float cloudBrightness;
	private float atmosphereFog;
	private float atmosphereBrightness;
	private float rainEmission;

	protected void Start()
	{
		var sky = TOD_Sky.Instance;

		// Get current values
		cloudOpacity         = sky.Clouds.Opacity;
		cloudCoverage        = sky.Clouds.Coverage;
		cloudBrightness      = sky.Clouds.Brightness;
		atmosphereFog        = sky.Atmosphere.Fogginess;
		atmosphereBrightness = sky.Atmosphere.Brightness;
		rainEmission         = RainParticleSystem ? RainParticleSystem.emissionRate : 0;

		// Get maximum values
		cloudOpacityMax         = cloudOpacity;
		cloudBrightnessMax      = cloudBrightness;
		atmosphereBrightnessMax = atmosphereBrightness;
		rainEmissionMax         = rainEmission;
	}

	protected void Update()
	{
		var sky = TOD_Sky.Instance;

		// Update rain state
		switch (Rain)
		{
			case RainType.None:
				rainEmission = 0.0f;
				break;

			case RainType.Light:
				rainEmission = rainEmissionMax * 0.5f;
				break;

			case RainType.Heavy:
				rainEmission = rainEmissionMax;
				break;
		}

		// Update cloud state
		switch (Clouds)
		{
			case CloudType.None:
				cloudOpacity  = 0.0f;
				cloudCoverage = 0.0f;
				break;

			case CloudType.Few:
				cloudOpacity  = cloudOpacityMax;
				cloudCoverage = 0.1f;
				break;

			case CloudType.Scattered:
				cloudOpacity  = cloudOpacityMax;
				cloudCoverage = 0.3f;
				break;

			case CloudType.Broken:
				cloudOpacity  = cloudOpacityMax;
				cloudCoverage = 0.6f;
				break;

			case CloudType.Overcast:
				cloudOpacity  = cloudOpacityMax;
				cloudCoverage = 1.0f;
				break;
		}

		// Update atmosphere state
		switch (Atmosphere)
		{
			case AtmosphereType.Clear:
				cloudBrightness      = cloudBrightnessMax;
				atmosphereBrightness = atmosphereBrightnessMax;
				atmosphereFog        = 0.0f;
				break;

			case AtmosphereType.Storm:
				cloudBrightness      = cloudBrightnessMax * 0.3f;
				atmosphereBrightness = atmosphereBrightnessMax * 0.5f;
				atmosphereFog        = 1.0f;
				break;

			case AtmosphereType.Dust:
				cloudBrightness      = cloudBrightnessMax;
				atmosphereBrightness = atmosphereBrightnessMax;
				atmosphereFog        = 0.5f;
				break;

			case AtmosphereType.Fog:
				cloudBrightness      = cloudBrightnessMax;
				atmosphereBrightness = atmosphereBrightnessMax;
				atmosphereFog        = 1.0f;
				break;
		}

		// FadeTime is not exact as the fade smoothens a little towards the end
		float t = Time.deltaTime / FadeTime;

		// Update visuals
		sky.Clouds.Opacity        = Mathf.Lerp(sky.Clouds.Opacity,        cloudOpacity,         t);
		sky.Clouds.Coverage       = Mathf.Lerp(sky.Clouds.Coverage,       cloudCoverage,        t);
		sky.Clouds.Brightness     = Mathf.Lerp(sky.Clouds.Brightness,     cloudBrightness,      t);
		sky.Atmosphere.Fogginess  = Mathf.Lerp(sky.Atmosphere.Fogginess,  atmosphereFog,        t);
		sky.Atmosphere.Brightness = Mathf.Lerp(sky.Atmosphere.Brightness, atmosphereBrightness, t);

		if (RainParticleSystem)
		{
			RainParticleSystem.emissionRate = Mathf.Lerp(RainParticleSystem.emissionRate, rainEmission, t);
		}
	}
}
