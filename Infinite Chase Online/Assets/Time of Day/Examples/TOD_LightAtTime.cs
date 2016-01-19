using UnityEngine;

[RequireComponent(typeof(Light))]
public class TOD_LightAtTime : MonoBehaviour
{
	public AnimationCurve Intensity = new AnimationCurve() {
		keys = new Keyframe[] { new Keyframe(0, 0), new Keyframe(12, 1), new Keyframe(24, 0) }
	};

	private Light lightComponent;

	protected void Start()
	{
		lightComponent = GetComponent<Light>();
	}

	protected void Update()
	{
		lightComponent.intensity = Intensity.Evaluate(TOD_Sky.Instance.Cycle.Hour);
		lightComponent.enabled   = (lightComponent.intensity > 0);
	}
}
