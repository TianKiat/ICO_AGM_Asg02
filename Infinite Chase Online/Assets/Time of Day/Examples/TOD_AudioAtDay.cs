using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TOD_AudioAtDay : MonoBehaviour
{
	public  float fadeTime = 1;
	private float lerpTime = 0;

	private AudioSource audioComponent;
	private float audioVolume;

	protected void Start()
	{
		audioComponent = GetComponent<AudioSource>();
		audioVolume    = audioComponent.volume;

		audioComponent.enabled = TOD_Sky.Instance.IsDay;
	}

	protected void Update()
	{
		int sign = (TOD_Sky.Instance.IsDay) ? +1 : -1;
		lerpTime = Mathf.Clamp01(lerpTime + sign * Time.deltaTime / fadeTime);

		audioComponent.volume  = Mathf.Lerp(0, audioVolume, lerpTime);
		audioComponent.enabled = (audioComponent.volume > 0);
	}
}
