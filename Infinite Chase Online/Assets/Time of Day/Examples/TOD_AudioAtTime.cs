using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TOD_AudioAtTime : MonoBehaviour
{
	public AnimationCurve Volume = new AnimationCurve() {
		keys = new Keyframe[] { new Keyframe(0, 0), new Keyframe(12, 1), new Keyframe(24, 0) }
	};

	private AudioSource audioComponent;

	protected void Start()
	{
		audioComponent = GetComponent<AudioSource>();
	}

	protected void Update()
	{
		audioComponent.volume  = Volume.Evaluate(TOD_Sky.Instance.Cycle.Hour);
		audioComponent.enabled = (audioComponent.volume > 0);
	}
}
